using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkPlayerController : NetworkBehaviour
{
    // UserInput variables
    [Header("User Input")]
    private PlayerControls playerControls;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isSprinting, isCrouching;

    public PlayerControls LocalPlayerControls => playerControls;
    public Quaternion GetRotation() => transform.rotation; // here 
    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    // Transform variables
    private Transform head;
    public Transform orientation;

    // Mouse variables
    [Header("Mouse Sensitivity")]
    [Range(0f, 1000f)]
    public float horizontalSensitivity = 10f;
    [Range(0f, 1000f)]
    public float verticalSensitivity = 10f;
    public float xRotation, yRotation;

    // Movement variables
    [Header("Movement Modifiers")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float sprintTime = 0.25f;
    [Range(0f, 1f)]
    public float airResistance = 0.4f;
    public float moveSpeed = 6f;
    private bool isMoving;
    public Vector3 moveDirection;
    private Vector3 moveDirectionNormalized;
    private IEnumerator sprintCoroutine;

    // Jump variables
    [Header("Jump Modifiers")]
    public float jumpForce = 3f;
    public float jumpCooldown = 0.5f;
    private bool canJump = true;

    // Crouch variables
    [Header("Crouch Modifiers")]
    [Range(0f, 2f)]
    public float crouchHeight = 0.5f;
    public float crouchTime = 0.25f;
    public float ceilingDetection = 0.4f;
    private float defaultHeight;
    private bool underCeiling = false;
    private bool stuckCrouching = false;
    private IEnumerator crouchCoroutine;

    // Stair and Slope variables
    [Header("Stair/Slope Modifiers")]
    [Range(0f, 5f)]
    public float stepUpHeight = 0.6f;
    [Range(0f, 2f)]
    public float detectionRadius = 0.75f;
    public bool realisticSlopes = false;
    private bool onSlope = false;
    private bool onStair = false;
    private RaycastHit slopeHit;
    private float stepHeight;

    // Drag variables
    [Header("Drag Modifiers")]
    public float groundDrag = 6f;
    public float airDrag = 2f;

    // Gravity variables
    [Header("Gravity Modifiers")]
    [Range(0f, 10f)]
    public float gravityScalar = 6.5f;

    // Ground variables
    [Header("Ground Modifiers")]
    public LayerMask groundMask;
    public Transform playerFeet;
    public float groundDetection = 0.4f;
    private bool isGrounded = false;

    // Camera variables
    [Header("Camera Modifiers")]
    public Transform playerCamera;
    private Vector3 defaultLocalPosition;

    [Header("Debug Options")]
    public Text debugText;
    public bool debugMode = false;

    // Miscellaneous
    private Rigidbody playerBody;
    private CapsuleCollider playerCollider;

    // private Animator animator;


    public void PerformJump() => Jump();
    public void PerformCrouch() => Crouch();
    public void PerformUnCrouch() => UnCrouch();
    public void PerformWalk() => Walk();
    public void PerformSprint() => Sprint();

    public void Awake()
    {
        playerControls = new PlayerControls();
        // animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        playerControls.Enable();
    }

    void OnDisable()
    {
        playerControls.Disable();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        enabled = true;

        // Jump
        playerControls.Land.Jump.performed += context => Jump();

        // Sprint
        playerControls.Land.Sprint.performed += context => Sprint();
        playerControls.Land.Sprint.canceled += context => Walk();

        // Crouch
        playerControls.Land.Crouch.performed += context => Crouch();
        playerControls.Land.Crouch.canceled += context => UnCrouch();
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
        if (hasAuthority)
            UserInput();

        UserInfo();
        LookDirection();
        MovementModifiers();

        // animator.SetFloat("Horizontal", xRotation);
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

    public void LookDirection()
    {
        //If the local player sending data 
        if (hasAuthority)
        {
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

    private void Jump()
    {
        if (debugMode)
            Debug.Log("Jump called -> jumpForce = " + jumpForce + " " + "canJump: " + canJump + " " + "isGrounded: " + isGrounded);

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

    private void Crouch()
    {
        Debug.Log("Crouch Called for: " + netId);

        isCrouching = true;

        // Put crouch animation code here
        // --------------------------------



        // --------------------------------

        // Run coroutine to adjust camera and collider's position/height
        if (crouchCoroutine != null)
            StopCoroutine(crouchCoroutine);

        crouchCoroutine = AdjustHeightOverTime(defaultHeight * crouchHeight, crouchTime);
        StartCoroutine(crouchCoroutine);

        if (sprintCoroutine != null)
            StopCoroutine(sprintCoroutine);

        sprintCoroutine = AdjustSpeedOverTime(walkSpeed / 2.0f, sprintTime);
        StartCoroutine(sprintCoroutine);
    }

    private void UnCrouch()
    {
        if (!underCeiling)
        {
            isCrouching = false;

            // Put uncrouch animation code here
            // --------------------------------



            // --------------------------------

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
        Vector3 targetCenter = new Vector3(0f, -offset, 0f);
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
                    if (Vector3.Dot(feetHit.normal, slopeHit.normal) > 0)
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
}
