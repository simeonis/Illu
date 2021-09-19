using System.Collections;
using UnityEngine;
using Mirror;

public class NetworkPlayerController : NetworkBehaviour
{
    // User Input variables
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isSprinting, isCrouching;

    // Transform variables
    [Header("Transforms")]
    [SerializeField] private Transform head; // TEMPORARY
    [SerializeField] private Transform orientation;

    // Mouse variables
    [Header("Mouse Sensitivity")]
    [SerializeField, Range(0f, 1000f)] private float horizontalSensitivity = 10f;
    [SerializeField, Range(0f, 1000f)] private float verticalSensitivity = 10f;
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
    private bool canJump = true;

    // Crouch variables
    [Header("Crouch Modifiers")]
    [SerializeField, Range(0f, 1f)] private float crouchFactor = 0.5f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private float ceilingDetection = 0.4f;
    private float defaultHeight;
    private bool underCeiling = false;
    private bool stuckCrouching = false;
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
    private bool isGrounded = false;

    // Camera variables
    [Header("Camera Modifiers")]
    [SerializeField] private Transform playerCamera;
    [HideInInspector] public bool visionLocked = false;

    // Debug variables
    [Header("Debug Options")]
    [SerializeField] private bool debugMode = false;

    // Miscellaneous
    private Rigidbody playerBody;
    private CapsuleCollider playerCollider;

    // Needed to smooth movement over a network
    public void NetworkJump() => Jump();
    public void NetworkCrouch() => Crouch();
    public void NetworkUnCrouch() => UnCrouch();
    public void NetworkWalk() => Walk();
    public void NetworkSprint() => Sprint();

    void Start()
    {
        // Transform
        orientation = transform.Find("Orientation");

        // ---------------TEMPORARY-------------------
        head = orientation.Find("Model").Find("Head");
        // -------------------------------------------

        // Rigidbody
        playerBody = GetComponent<Rigidbody>();
        playerBody.freezeRotation = true;
        playerBody.useGravity = false;

        // Capsule collider
        playerCollider = GetComponent<CapsuleCollider>();
        defaultHeight = playerCollider.height;

        // Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        enabled = true;

        // Jump
        InputManager.playerControls.Land.Jump.performed += context => Jump();

        // Sprint
        InputManager.playerControls.Land.Sprint.performed += context => Sprint();
        InputManager.playerControls.Land.Sprint.canceled += context => Walk();

        // Crouch
        InputManager.playerControls.Land.Crouch.performed += context => Crouch();
        InputManager.playerControls.Land.Crouch.canceled += context => UnCrouch();
    }

    void OnDisable()
    {
        // Jump
        InputManager.playerControls.Land.Jump.performed -= context => Jump();

        // Sprint
        InputManager.playerControls.Land.Sprint.performed -= context => Sprint();
        InputManager.playerControls.Land.Sprint.canceled -= context => Walk();

        // Crouch
        InputManager.playerControls.Land.Crouch.performed -= context => Crouch();
        InputManager.playerControls.Land.Crouch.canceled -= context => UnCrouch();
    }

    void Update()
    {
        if (hasAuthority)
        {
            UserInput();
            LookDirection();
        }

        UserInfo();
        MovementModifiers();
    }

    void FixedUpdate()
    {
        UserMovement();
    }

    /*  --------------------------
    *        Update functions
    *   -------------------------- */

    private void UserInput()
    {
        lookInput = InputManager.playerControls.Land.Look.ReadValue<Vector2>();
        lookInput.x *= horizontalSensitivity * 0.01f;
        lookInput.y *= verticalSensitivity * 0.01f;

        movementInput = InputManager.playerControls.Land.Movement.ReadValue<Vector2>();
    }

    public void LookDirection()
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

    private void UserMovement()
    {
        // Calculate movement direction
        if (hasAuthority)
            moveDirection = orientation.forward * movementInput.y + orientation.right * movementInput.x;

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

    private void Jump()
    {
        if (canJump && isGrounded)
        {
            // Disable jump capability
            canJump = false;

            // Jump force
            playerBody.velocity = new Vector3(playerBody.velocity.x, 0f, playerBody.velocity.z);
            playerBody.AddForce(transform.up * jumpForce * 10f, ForceMode.Impulse);

            // Enable jump capability after cooldown expires
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        canJump = true;
    }

    private void Walk()
    {
        isSprinting = false;
        if (!isCrouching)
        {
            // Run coroutine to adjust movespeed → walk speed
            if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
            StartCoroutine(sprintCoroutine = AdjustSpeedOverTime(walkSpeed, timeToSprint));
        }
    }

    private void Sprint()
    {
        isSprinting = true;
        if (!isCrouching)
        {
            // Run coroutine to adjust movespeed → sprint speed
            if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
            StartCoroutine(sprintCoroutine = AdjustSpeedOverTime(sprintSpeed, timeToSprint));
        }
    }

    private void Crouch()
    {
        isCrouching = true;

        // Run coroutine to adjust camera and collider's position/height
        if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
        StartCoroutine(crouchCoroutine = AdjustHeightOverTime(defaultHeight * crouchFactor, timeToCrouch));

        // Run coroutine to adjust movespeed → crouch speed
        if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
        StartCoroutine(sprintCoroutine = AdjustSpeedOverTime(walkSpeed / 2.0f, timeToSprint));
    }

    private void UnCrouch()
    {
        // No obstruction
        if (!underCeiling)
        {
            isCrouching = false;

            // Run coroutine to reset camera and collider's position/height
            if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
            StartCoroutine(crouchCoroutine = AdjustHeightOverTime(defaultHeight, timeToCrouch));

            // Run coroutine to adjust movespeed → sprint speed or walk speed
            if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
            StartCoroutine(sprintCoroutine = AdjustSpeedOverTime(isSprinting ? sprintSpeed : walkSpeed, timeToSprint));
        }
        // Obstruction
        else if (isCrouching && underCeiling)
        {
            stuckCrouching = true;
        }
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