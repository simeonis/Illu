using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // UserInput variables
    [HideInInspector]
    public PlayerControls playerControls;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isSprinting, isCrouching;

    // Transform variables
    private Transform head;
    private Transform orientation;

    // Mouse variables
    [Header("Mouse Sensitivity")]
    [SerializeField, Range(0f, 1000f)] private float horizontalSensitivity = 10f;
    [SerializeField, Range(0f, 1000f)] private float verticalSensitivity = 10f;
    private float xRotation, yRotation;

    // Movement variables
    [Header("Movement Modifiers")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float sprintTime = 0.25f;
    [SerializeField, Range(0f, 1f)] private float airResistance = 0.4f;
    private float moveSpeed = 6f;
    private bool isMoving;
    private Vector3 moveDirection;
    private Vector3 moveDirectionNormalized;
    private IEnumerator sprintCoroutine;

    // Jump variables
    [Header("Jump Modifiers")]
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float jumpCooldown = 0.5f;
    public bool canJump = true;

    // Crouch variables
    [Header("Crouch Modifiers")]
    [SerializeField, Range(0f, 2f)] private float crouchHeight = 0.5f;
    [SerializeField] private float crouchTime = 0.25f;
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
    private Vector3 defaultLocalPosition;

    // Animation variables
    [Header("Animation Modifiers")]
    [SerializeField] private AnimationHumanoid animator;

    [Header("Debug Options")]
    [SerializeField] private Text debugText;
    [SerializeField] private bool debugMode = false;

    // Miscellaneous
    private Rigidbody playerBody;
    private CapsuleCollider playerCollider;

    void Awake() {
        playerControls = new PlayerControls();

        // Jump
        playerControls.Land.Jump.performed += context => Jump();

        // Sprint
        playerControls.Land.Sprint.performed += context => Sprint();
        playerControls.Land.Sprint.canceled += context => Walk();

        // Crouch
        playerControls.Land.Crouch.performed += context => Crouch();
        playerControls.Land.Crouch.canceled += context => UnCrouch();
    }

    void OnEnable() {
        playerControls.Land.Enable();
    }

    void OnDisable() {
        playerControls.Land.Disable();
    }

    void Start()
    {
        // Transform
        orientation = transform.Find("Orientation");
        head = transform.Find("Head");

        // Rigidbody
        playerBody = GetComponent<Rigidbody>();
        playerBody.freezeRotation = true;
        playerBody.useGravity = false;

        // Capsule collider
        playerCollider = GetComponent<CapsuleCollider>();
        defaultHeight = playerCollider.height;

        // Camera
        defaultLocalPosition = playerCamera.localPosition;

        // Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UserInfo();
        UserInput();
        LookDirection();
        MovementModifiers();
    }

    void FixedUpdate()
    {
        UserMovement();
    }

    private void UserInfo()
    {
        isMoving = moveDirection != Vector3.zero;
        isGrounded = Physics.CheckSphere(playerFeet.position, groundDetection, groundMask);
        underCeiling = Physics.CheckSphere(playerCamera.position, ceilingDetection, groundMask);

        int surfaceType = SurfaceDetection();
        onSlope = surfaceType == 1;
        onStair = surfaceType == 2;

        // Animation
        if (isGrounded && !animator.IsGrounded()) animator.Land();
        animator.SetGrounded(isGrounded);

        // Print user information (debug mode)
        if (debugMode && debugText)
        {
            debugText.text = string.Format(
                "Moving: {0}\n" +
                "Grounded: {1}\n" +
                "Ceiling: {2}\n" +
                "Sprint: {3}\n" +
                "Crouch: {4}\n" +
                "Slope: {5}\n" +
                "Stair: {6}", 
                isMoving, isGrounded, underCeiling, isSprinting, isCrouching, onSlope, onStair);
        } 
        else if (!debugMode && debugText)
        {
            debugText.text = "";
        }
    }

    private void UserInput()
    {
        lookInput = playerControls.Land.Look.ReadValue<Vector2>();
        lookInput.x *= horizontalSensitivity * 0.01f;
        lookInput.y *= verticalSensitivity * 0.01f;
        
        movementInput = playerControls.Land.Movement.ReadValue<Vector2>();
    }

    private void LookDirection()
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

        // Rotate orientation so that movement matches the look direction
        orientation.RotateAround(orientation.position, transform.up, lookInput.x);
    }

    public void ResetLookDirection()
    {
        yRotation = xRotation = 0;
        orientation.rotation = new Quaternion();
    }

    private void MovementModifiers() {
        // Drag
        playerBody.drag = isGrounded ? groundDrag : airDrag;

        // Crouch
        if (stuckCrouching && !underCeiling)
        {
            UnCrouch();
            stuckCrouching = false;
        }
    }

    private void Jump()
    {
        if (canJump && isGrounded)
        {
            // Disable jump capability
            canJump = false;

            // Crouch animation
            animator.Jump();

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

    private void Crouch()
    {
        isCrouching = true;

        // Crouch animation
        animator.Crouch();

        // Run coroutine to adjust camera and collider's position/height
        if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
        crouchCoroutine = AdjustHeightOverTime(defaultHeight * crouchHeight, crouchTime);
        StartCoroutine(crouchCoroutine);

        if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
        sprintCoroutine = AdjustSpeedOverTime(walkSpeed / 2.0f, sprintTime);
        StartCoroutine(sprintCoroutine);
    }

    private void UnCrouch()
    {
        if (!underCeiling)
        {
            isCrouching = false;

            // Crouch animation
            animator.UnCrouch();

            // Run coroutine to reset camera and collider's position/height
            if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
            crouchCoroutine = AdjustHeightOverTime(defaultHeight, crouchTime);
            StartCoroutine(crouchCoroutine);

            if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
            sprintCoroutine = AdjustSpeedOverTime(isSprinting ? sprintSpeed : walkSpeed, sprintTime);
            StartCoroutine(sprintCoroutine);
        }
        else if (isCrouching && underCeiling)
        {
            stuckCrouching = true;
        }
    }

    private void Sprint()
    {
        isSprinting = true;
        if (!isCrouching)
        {
            // Put sprint animation code here
            // --------------------------------



            // --------------------------------

            if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
            sprintCoroutine = AdjustSpeedOverTime(sprintSpeed, sprintTime);
            StartCoroutine(sprintCoroutine);
        }
    }

    private void Walk()
    {
        isSprinting = false;
        if (!isCrouching)
        {
            // Put walk animation code here
            // --------------------------------



            // --------------------------------

            if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
            sprintCoroutine = AdjustSpeedOverTime(walkSpeed, sprintTime);
            StartCoroutine(sprintCoroutine);
        }
    }

    private IEnumerator AdjustHeightOverTime(float targetHeight, float duration) 
    {
        // Current values
        Vector3 currentPos = playerCamera.localPosition;
        Vector3 currentCenter = playerCollider.center;
        float currentHeight = playerCollider.height;

        // Target values
        // Target height must be bigger or equal to 2x the capsule collider's radius
        targetHeight = Mathf.Clamp(targetHeight, playerCollider.radius * 2.0f, Mathf.Infinity);
        // Bring "feet" to "head" if crouching in the air, otherwise bring "head" to "feet" if grounded
        float offset = isGrounded ? 1.0f - (targetHeight / defaultHeight) : 0.0f;
        // Capsule collider's target local position
        Vector3 targetCenter = new Vector3(0f, -offset + 1.2f, 0.25f);
        // Player camera's target local position 
        Vector3 targetPos = new Vector3(defaultLocalPosition.x, defaultLocalPosition.y - (offset * 2f), defaultLocalPosition.z);

        // Percentage of time passed in relation to the total duration
        float percent = 0.0f;
        // While percentage is not 100%
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / duration;

            // Gradually adjust the capsule collider and player camera over time
            playerCamera.localPosition = Vector3.Lerp(currentPos, targetPos, percent);
            playerCollider.center = Vector3.Lerp(currentCenter, targetCenter, percent);
            playerCollider.height = Mathf.Lerp(currentHeight, targetHeight, percent);
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

    private int SurfaceDetection()
    {
        if (!isGrounded) return 0;

        Vector3 playerWaistPosition = playerFeet.position + transform.up * stepUpHeight;
        Vector3 waistToFeet = playerFeet.position - playerWaistPosition;

        // Visualize raycasts (debug mode)
        if (debugMode && isMoving)
        {
            Debug.DrawRay(playerFeet.position, moveDirectionNormalized * detectionRadius, Color.black, 0f);
            Debug.DrawRay(playerWaistPosition, moveDirectionNormalized * detectionRadius, Color.black, 0f);
            Debug.DrawRay(playerWaistPosition + moveDirectionNormalized * detectionRadius, waistToFeet, Color.black, 0f);
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

    private void UserMovement()
    {
        // Calculate movement direction
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
}