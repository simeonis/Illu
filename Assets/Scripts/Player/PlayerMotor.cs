using System.Collections;
using UnityEngine;

class PlayerMotor : MonoBehaviour
{
    [HideInInspector] public bool isSprinting, isCrouching;
    // Transform variables
    [Header("Transforms")]
    [SerializeField] private Transform head; // TEMPORARY
    [SerializeField] private Transform orientation;

    [Header("Rigid Body")]
    private Rigidbody playerBody;
    private float xRotation, yRotation;

    // Movement variables
    [Header("Movement Modifiers")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float timeToSprint = 0.25f;
    [SerializeField, Range(0f, 1f)] private float airResistance = 0.4f;
    [HideInInspector] public float moveSpeed = 6f;
    [HideInInspector] public Vector3 moveDirection;
    private Vector3 moveDirectionNormalized;
    private IEnumerator sprintCoroutine;
    private bool isMoving;

    // Jump variables
    [Header("Jump Modifiers")]
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [HideInInspector] public bool canJump = true;

    // Crouch variables
    [Header("Crouch Modifiers")]
    [SerializeField, Range(0f, 1f)] private float crouchFactor = 0.5f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private float ceilingDetection = 0.4f;
    private float defaultHeight;
    [HideInInspector] public bool underCeiling = false;
    [HideInInspector] public bool stuckCrouching = false;
    private IEnumerator crouchCoroutine;

    // Stair and Slope variables
    [Header("Stair/Slope Modifiers")]
    [SerializeField, Range(0f, 5f)] private float stepUpHeight = 0.6f;
    [SerializeField, Range(0f, 2f)] private float detectionRadius = 0.75f;
    [SerializeField] private bool realisticSlopes = false;
    private bool onSlope = false;
    private bool onStair = false;
    private RaycastHit slopeHit;
    private float stepHeight;

    // Drag variables
    [Header("Drag Modifiers")]
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 2f;

    // Gravity variables
    [Header("Gravity Modifiers")]
    [SerializeField, Range(0f, 10f)] private float gravityScalar = 6.5f;

    // Ground variables
    [Header("Ground Modifiers")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform playerFeet;
    [SerializeField] private float groundDetection = 0.4f;
    public bool isGrounded = false;
    // Camera variables
    [Header("Camera Modifiers")]
    [SerializeField] private Transform playerCamera;
    [HideInInspector] public bool visionLocked = false;

    // Debug variables
    [Header("Debug Options")]
    [SerializeField] private bool debugMode = false;

    // Miscellaneous
    private CapsuleCollider playerCollider;

    void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Transform
        orientation = transform.Find("Orientation");

        // ---------------TEMPORARY-------------------
        head = orientation.Find("Model").Find("Head");
        // -------------------------------------------

        // Rigidbody
        // playerBody = GetComponent<Rigidbody>();
        playerBody.freezeRotation = true;
        playerBody.useGravity = false;

        // Capsule collider
        playerCollider = GetComponent<CapsuleCollider>();
        defaultHeight = playerCollider.height;

        // Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UserInfo();
        MovementModifiers();
    }

    /*  --------------------------
    *        Update functions
    *   -------------------------- */
    public void LookDirection(Vector2 lookInput)
    {
        if (visionLocked) return;

        // Rotation along the y-axis (left-right)
        yRotation += lookInput.x;
        // Rotation along the x-axis (up-down)
        xRotation -= lookInput.y;

        // Limit vertical rotation to 180 degrees
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotates camera on both axis'
        // World Rotation = Target Rotation * Rotation
        playerCamera.rotation = transform.rotation * Quaternion.Euler(xRotation, yRotation, 0f);

        // Rotate "head" or "eyes" with camera (Aesthetic only)
        head.rotation = playerCamera.rotation;

        // Rotate orientation so that movement matches the look direction
        orientation.RotateAround(orientation.position, transform.up, lookInput.x);
    }

    private void UserInfo()
    {
        isMoving = moveDirection != Vector3.zero;
        isGrounded = Physics.CheckSphere(playerFeet.position, groundDetection, groundMask);
        underCeiling = Physics.CheckSphere(playerCamera.position, ceilingDetection, groundMask);

        int surfaceType = SurfaceDetection();
        onSlope = surfaceType == 1;
        onStair = surfaceType == 2;
    }

    private void MovementModifiers()
    {
        // Drag
        playerBody.drag = isGrounded ? groundDrag : airDrag;

        // Crouch
        if (stuckCrouching && !underCeiling)
        {
            UnCrouch();
            stuckCrouching = false;
        }
    }

    /*  --------------------------
    *       Physics functions
    *   -------------------------- */
    public void UserMovement(Vector3 moveDirection)
    {
        moveDirectionNormalized = moveDirection.normalized; // Used for Slope/Stair detection
        moveDirection = onSlope ? Vector3.ProjectOnPlane(moveDirectionNormalized, slopeHit.normal) : moveDirectionNormalized;

        // Move player up stairs
        if (onStair && isMoving)
        {
            playerBody.AddForce(transform.up * stepHeight * moveSpeed * 0.5f, ForceMode.Impulse);
        }
        // Apply gravity unless ... 
        // the player is on a non-realistic slope and moving
        else if (realisticSlopes || !onSlope || !isMoving)
        {
            Debug.Log("playerBody " + playerBody + " transform " + transform + " gravityScalar " + gravityScalar);
            playerBody.AddForce(-transform.up * gravityScalar * 10f);
        }

        // Apply movement
        if (isGrounded)
        {
            playerBody.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Acceleration);
        }
        else
        {
            playerBody.AddForce(moveDirection * walkSpeed * airResistance * 10f, ForceMode.Acceleration);
        }
    }

    /*  --------------------------
  *       Movement functions
  *   -------------------------- */

    public void Jump()
    {
        // Jump force
        playerBody.velocity = new Vector3(playerBody.velocity.x, 0f, playerBody.velocity.z);

        playerBody.AddForce(transform.up * jumpForce * 10f, ForceMode.Impulse);

        // Enable jump capability after cooldown expires
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ResetJump()
    {
        canJump = true;
    }

    public void Walk()
    {
        // Run coroutine to adjust movespeed → walk speed
        if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
        StartCoroutine(sprintCoroutine = AdjustSpeedOverTime(walkSpeed, timeToSprint));
    }

    public void Sprint()
    {
        // Run coroutine to adjust movespeed → sprint speed
        if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
        StartCoroutine(sprintCoroutine = AdjustSpeedOverTime(sprintSpeed, timeToSprint));
    }

    public void Crouch()
    {
        // Run coroutine to adjust camera and collider's position/height
        if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
        StartCoroutine(crouchCoroutine = AdjustHeightOverTime(defaultHeight * crouchFactor, timeToCrouch));

        // Run coroutine to adjust movespeed → crouch speed
        if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
        StartCoroutine(sprintCoroutine = AdjustSpeedOverTime(walkSpeed / 2.0f, timeToSprint));
    }

    public void UnCrouch()
    {

        // Run coroutine to reset camera and collider's position/height
        if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
        StartCoroutine(crouchCoroutine = AdjustHeightOverTime(defaultHeight, timeToCrouch));

        // Run coroutine to adjust movespeed → sprint speed or walk speed
        if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
        StartCoroutine(sprintCoroutine = AdjustSpeedOverTime(isSprinting ? sprintSpeed : walkSpeed, timeToSprint));

    }

    /*  --------------------------
    *        Helper functions
    *   -------------------------- */

    private int SurfaceDetection()
    {
        if (!isGrounded) return 0;

        Vector3 playerWaistPosition = playerFeet.position + transform.up * stepUpHeight;
        Vector3 waistToFeet = playerFeet.position - playerWaistPosition;

        // Visualize raycasts (debug mode)
        if (debugMode && isMoving)
        {
            Debug.DrawRay(playerFeet.position, moveDirectionNormalized * detectionRadius, Color.yellow, 0f);
            Debug.DrawRay(playerWaistPosition, moveDirectionNormalized * detectionRadius, Color.yellow, 0f);
            Debug.DrawRay(playerWaistPosition + moveDirectionNormalized * detectionRadius, waistToFeet, Color.yellow, 0f);
        }

        if (Physics.Raycast(playerFeet.position, moveDirectionNormalized, out RaycastHit feetHit, detectionRadius, groundMask))
        {
            if (!Physics.Raycast(playerWaistPosition, moveDirectionNormalized, out _, detectionRadius, groundMask))
            {
                if (Physics.Raycast(playerWaistPosition + moveDirectionNormalized * detectionRadius, waistToFeet, out slopeHit, groundMask))
                {
                    if (Vector3.Dot(Vector3Int.RoundToInt(feetHit.normal), Vector3Int.RoundToInt(slopeHit.normal)) > 0)
                    {
                        return 1; // Slope
                    }
                    else
                    {
                        // Calculate the height of the step
                        // End of the feet raycast to the waistToFeet collision point
                        Vector3 feetRayEnd = playerFeet.position + moveDirectionNormalized * detectionRadius;
                        stepHeight = (slopeHit.point - feetRayEnd).magnitude;
                        return 2; // Stair
                    }
                }
            }
        }

        return 0; // Ground
    }

    /*  --------------------------
    *       Coroutine functions
    *   -------------------------- */

    private IEnumerator AdjustHeightOverTime(float targetHeight, float duration)
    {
        bool crouching = targetHeight != defaultHeight;

        /* Current values */
        Vector3 currentCamPos = playerCamera.localPosition;
        Vector3 currentFeetPos = playerFeet.localPosition;
        Vector3 currentColliderCenter = playerCollider.center;
        float currentColliderHeight = playerCollider.height;

        /* Target values */
        // Target height must be bigger or equal to the capsule collider's diameter
        float colliderDiameter = playerCollider.radius * 2.0f;
        float targetColliderHeight = Mathf.Max(targetHeight, colliderDiameter);
        crouchFactor = Mathf.Max(crouchFactor, colliderDiameter / defaultHeight);

        // Capsule collider's target local position
        // Bring "feet" to "head" if crouching in the air, otherwise bring "head" to "feet" if grounded
        // Uses collider's radius as the center offset
        float colliderOffset = crouching ? targetColliderHeight / 2.0f : defaultHeight / 2.0f;
        Vector3 targetColliderCenter = new Vector3(0f, isGrounded ? colliderOffset : defaultHeight - colliderOffset, 0f);

        // Player camera's target local position
        float cameraOffset = isGrounded ? defaultHeight - targetColliderHeight : 0f;
        Vector3 targetCamPos = new Vector3(0f, -cameraOffset, 0f);

        // Player's feet target local position
        Vector3 targetFeetPos = currentFeetPos;
        targetFeetPos.y = (crouching && !isGrounded) ? defaultHeight - targetColliderHeight : 0f;

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / duration;
            playerCamera.localPosition = Vector3.Lerp(currentCamPos, targetCamPos, percent);
            playerFeet.localPosition = Vector3.Lerp(currentFeetPos, targetFeetPos, percent);
            playerCollider.center = Vector3.Lerp(currentColliderCenter, targetColliderCenter, percent);
            playerCollider.height = Mathf.Lerp(currentColliderHeight, targetColliderHeight, percent);
            yield return null;
        }
    }

    private IEnumerator AdjustSpeedOverTime(float targetSpeed, float duration)
    {
        float currentSpeed = moveSpeed;

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / duration;
            moveSpeed = Mathf.Lerp(currentSpeed, targetSpeed, percent);
            yield return null;
        }
    }
}