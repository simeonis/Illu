using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Animation Modifiers")]
    [SerializeField] private Animator animator;
    private int isGroundedHash;
    private int isMovingHash;
    private int isJumpingHash; 
    private int isFallingHash;
    private int isSafeLandingHash;
    private int hasLandedHash;
    private int moveSpeedHash;
    private int fallSpeedHash;

    [Header("Rotation Modifiers")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerCamera;
    private float turnSmoothVelocity;

    [Header("Locomotion Modifiers")]
    [SerializeField, Tooltip("Max walk speed")] 
    private float walkSpeed = 4f;
    [SerializeField, Tooltip("Max sprint speed")] 
    private float sprintSpeed = 10f;
    [SerializeField, Tooltip("Rate of change to reach max speed")]
    private float acceleration = 50;
    [SerializeField, Tooltip("Max acceleration")]
    private float maxAcceleration = 150f;
    [SerializeField, Tooltip("Acceleration factor based on direction change")]
    private AnimationCurve accelerationDotFactor;
    private Vector3 targetVel = Vector3.zero;
    private Vector3 unitMoveDir = Vector3.zero; // User input
    private float verticalVelocity = 0.0f;
    private float moveSpeed = 0.0f;
    private bool isSprintPressed = false;
    private bool isMoving = false;
    private RaycastHit wallHit;

    [Header("Jump Modifiers")]
    [SerializeField, Tooltip("Apex of jump")] 
    private float maxJumpHeight = 1.5f;
    [SerializeField, Tooltip("Time to complete a full jump in seconds")]
    private float maxJumpTime = 0.75f;
    [SerializeField, Tooltip("Amount of time a player can still jump, even after walking off a platform")] 
    private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    private float initialJumpVelocity;
    private bool isJumpPressed = false;
    private bool isJumping = false;

    [Header("Fall Modifiers")]
    [SerializeField, Tooltip("Threshold velocity in which the player will count as falling")] 
    private float fallThresholdVelocity = 1f;
    [SerializeField, Tooltip("Velocity or lower that ensures a safe landing")] 
    private float safeVelocity = 20f;
    [SerializeField, Tooltip("Velocity or higher that causes a harmful landing")] 
    private float harmfulVelocity = 24f;
    [SerializeField, Tooltip("Player \"Landing Flat Impact\" animation clip")]
    private AnimationClip landingFlatImpactAnimationClip;
    [SerializeField, Tooltip("Player \"Stand Up\" animation clip")]
    private AnimationClip standUpAnimationClip;
    private bool isFalling = false;
    private bool isSafeLanding = true;

    [Header("Ground Modifiers")]
    [SerializeField, Tooltip("Layers that the player can collide with")] 
    private LayerMask groundMask;
    [SerializeField, Range(0f, 1f), Tooltip("Range from feet that checks if the player is grounded")]
    private float groundDetection = 0.18f;
    private bool isGrounded = false;

    private Rigidbody playerBody;
    private float gravity;
    private bool isFrozen = false;

    void Start()
    {
        playerBody = GetComponent<Rigidbody>();

        moveSpeed = walkSpeed;

        // Gravity + Jump
        float timeToApex = maxJumpTime * 0.5f;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;

        // Animation Hash (Increases Performance)
        isGroundedHash = Animator.StringToHash("isGrounded");
        isMovingHash = Animator.StringToHash("isMoving");
        isJumpingHash = Animator.StringToHash("isJumping");
        isFallingHash = Animator.StringToHash("isFalling");
        isSafeLandingHash = Animator.StringToHash("isSafeLanding");
        hasLandedHash = Animator.StringToHash("hasLanded");
        moveSpeedHash = Animator.StringToHash("moveSpeed");
        fallSpeedHash = Animator.StringToHash("fallSpeed");
    }

    void Update()
    {
        // Grounded (transform.position = feet)
        isGrounded = Physics.CheckSphere(transform.position, groundDetection, groundMask);
        animator.SetBool(isGroundedHash, isGrounded);
        
        // Locomotion
        isMoving = playerBody.velocity.magnitude > 0.1f;
        animator.SetBool(isMovingHash, isMoving);

        // Falling - Started
        if (!isFalling && !isGrounded && verticalVelocity < -fallThresholdVelocity)
        {
            isFalling = true;
            animator.SetBool(isFallingHash, true);
        }
        // Falling - Finished
        else if (isFalling && isGrounded)
        {
            isFalling = false;
            animator.SetBool(isFallingHash, false);
            
            // Harmful Landing
            if (!isSafeLanding)
            {
                isSafeLanding = true;
                StartCoroutine(StandUp());
            }
        }
        // Falling
        if (isFalling)
        {
            animator.SetFloat(fallSpeedHash, verticalVelocity);

            // Safe Landing
            if (verticalVelocity >= -safeVelocity)
            {
                isSafeLanding = true;
            }
            // Harmful Landing
            else if (verticalVelocity <= -harmfulVelocity)
            {
                isSafeLanding = false;
            }
        } 
    }

    void FixedUpdate()
    {
        Gravity();
        Locomotion();
        Jump();
        
        verticalVelocity = Vector3.Dot(playerBody.velocity, transform.up);
    }

    void Gravity()
    {
        playerBody.AddForce(transform.up * gravity, ForceMode.Acceleration);
    }

    void Locomotion()
    {
        float velDot = Vector3.Dot(unitMoveDir, targetVel);

        // Increases acceleration based on target velocity angle difference to the current velocity
        float accel = acceleration * accelerationDotFactor.Evaluate(velDot);

        // Set move speed to sprint speed if "sprint_key" is pressed
        moveSpeed = isSprintPressed ? sprintSpeed : walkSpeed;

        // Move previous target velocity to current target velocity
        targetVel = Vector3.MoveTowards(targetVel, unitMoveDir * moveSpeed, accel * Time.fixedDeltaTime);
        
        // Adjust the player's animation from walking to sprinting gradually (and vice versa)
        animator.SetFloat(moveSpeedHash, (targetVel.magnitude - walkSpeed) / (sprintSpeed - walkSpeed));
        
        // Rigidbody's current velocity (excluding upwards axis)
        Vector3 currVel = playerBody.velocity - Vector3.Project(playerBody.velocity, transform.up);
        
        // Acceleration needed to achieve target velocity from current velocity
        Vector3 targetAcceleration = (targetVel - currVel) / Time.fixedDeltaTime;
        targetAcceleration = Vector3.ClampMagnitude(targetAcceleration, maxAcceleration);
        
        playerBody.AddForce(targetAcceleration, ForceMode.Acceleration);
    }

    void Jump()
    {
        // Coyote Time
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (!isJumping && (coyoteTimeCounter > 0f) && isJumpPressed)
        {
            isJumping = true;
            isJumpPressed = false; // Remove this to hold jump button
            animator.SetBool(isJumpingHash, true);
            
            // Cancel upwards velocity, relative to player's orientation
            playerBody.velocity -= Vector3.Project(playerBody.velocity, transform.up);
            
            // Add upwards force, relative to player's orientation
            playerBody.AddForce(transform.up * initialJumpVelocity, ForceMode.VelocityChange);
        } 
        // No longer considered jumping when beginning to fall
        else if (isJumping && isFalling)
        {
            isJumping = false;
            animator.SetBool(isJumpingHash, false);
        }
    }

    IEnumerator StandUp()
    {
        Freeze();
        animator.SetTrigger(hasLandedHash);

        yield return new WaitForSeconds(standUpAnimationClip.length + landingFlatImpactAnimationClip.length);

        Unfreeze();
    }

    /// <summary>
    /// Notifies the intended direction.
    /// </summary>
    public void UpdateMovement(Vector2 direction)
    {
        if (isFrozen) return;
        unitMoveDir = (transform.forward * direction.y) + (transform.right * direction.x);

        // Rotates player in direction of movement, relative to camera's direction
        if (unitMoveDir.magnitude >= 0.1f)
        {
            // Visual explanation: https://youtu.be/4HpC--2iowE?t=762
            float targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + playerCamera.localEulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(orientation.localEulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
            orientation.rotation = transform.rotation * Quaternion.Euler(0f, smoothAngle, 0f);
            unitMoveDir = Quaternion.Euler(transform.up * targetAngle) * transform.forward;
        }
    }

    /// <summary>
    /// Notifies that the "jump_key" has been pressed/unpressed.
    /// </summary>
    public void SetJump(bool isPressed)
    {
        if (isFrozen) return;
        isJumpPressed = isPressed;
        if (!isJumpPressed) coyoteTimeCounter = 0f;
    }

    /// <summary>
    /// Notifies that the "sprint_key" has been pressed/unpressed.
    /// </summary>
    public void SetSprint(bool isPressed) 
    {
        if (isFrozen) return;
        isSprintPressed = isPressed;
    }

    /// <summary>
    /// Disables all movement, expect for gravity.
    /// </summary>
    public void Freeze() 
    {
        isFrozen = true;

        // Cancel user input
        isJumpPressed = false;
        isSprintPressed = false;
        unitMoveDir = Vector3.zero;

        // Cancel horizontal movement
        playerBody.velocity -= Vector3.Project(playerBody.velocity, transform.forward + transform.right);
    }

    /// <summary>
    /// Re-enables all movement.
    /// </summary>
    public void Unfreeze()
    {
        isFrozen = false;
    }

    #if UNITY_EDITOR
    [Header("Debug"), InspectorName("enable")] 
    public bool enable = false;
    void OnDrawGizmos()
    {
        if (enable)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, groundDetection);
            Gizmos.color = Color.blue;
            Vector3 direction = unitMoveDir.magnitude > 0f ? unitMoveDir : orientation.forward;
            Gizmos.DrawRay(transform.position, direction);
        }
    }
    #endif
}
