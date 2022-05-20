using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    // Animation variables
    [Header("Animation Modifiers")]
    [SerializeField] private Animator animator;
    private int isGroundedHash;
    private int isMovingHash;
    private int isJumpingHash; 
    private int isFallingHash;
    private int isDownHash;
    private int standUpHash;
    private int standUpSpeedHash;
    private int verticalVelocityHash;

    // Movement variables
    [Header("Movement Modifiers")]
    [SerializeField, Tooltip("Max speed when walking")] 
    private float walkSpeed = 6f;
    [SerializeField, Tooltip("Max speed when sprinting")]
    private float sprintSpeed = 12f;
    [SerializeField, Tooltip("Time in seconds it takes the player to transition from walking to sprinting")] 
    private float sprintAcceleration = 0.25f;
    [SerializeField, Range(0f, 1f), Tooltip("Air resistance")] 
    private float airResistance = 0.4f;
    private bool isMoving = false;
    private float moveSpeed = 6f;
    private Vector3 moveDirection = Vector3.zero;
    private float verticalVelocity = 0.0f;
    private RaycastHit wallHit;

    // Drag variables
    [Header("Drag Modifiers")]
    [SerializeField, Tooltip("Drag force applied while grounded")] 
    private float groundDrag = 6f;
    [SerializeField, Tooltip("Drag force applied while airborne")] 
    private float airDrag = 2f;

    // Jump variables
    [Header("Jump Modifiers")]
    [SerializeField] private float jumpForce = 8f;
    private bool isJumping = false;

    // Fall variables
    [Header("Fall Modifiers")]
    [SerializeField, Tooltip("Threshold velocity in which the player will count as falling")] 
    private float fallThresholdVelocity = 1f;
    [SerializeField, Tooltip("Highest velocity that ensures a safe landing")] 
    private float safeVelocity = 6f;
    [SerializeField, Tooltip("Lowest velocity that causes a harmful landing")] 
    private float harmfulVelocity = 20f;
    [SerializeField, Tooltip("Time in seconds it takes the player to stand up")]
    private float standUpTime = 1f;
    private bool isFalling = false;
    private bool willLandSafe = true;

    // Gravity variables
    [Header("Gravity Modifiers")]
    [SerializeField, Range(0f, 25f), Tooltip("Force of gravity being applied to the player")] 
    private float gravityScalar = 9.8f;
    private float gravityAcceleration = 0f;

    // Ground variables
    [Header("Ground Modifiers")]
    [SerializeField, Tooltip("Layers that the player can collide with")] 
    private LayerMask groundMask;
    [SerializeField, Range(0f, 1f), Tooltip("Range from feet that checks if the player is grounded")]
    private float groundDetection = 0.25f;
    private bool isGrounded = false;

    // Collider variables
    [Header("Colliders")]
    [SerializeField] private CapsuleCollider wallCollider;
    [SerializeField] private CapsuleCollider groundCollider;
    private Rigidbody playerBody;

    void Start()
    {
        playerBody = GetComponent<Rigidbody>();

        // Movement
        moveSpeed = walkSpeed;

        // Animation Hash (Increases performance)
        isGroundedHash = Animator.StringToHash("isGrounded");
        isMovingHash = Animator.StringToHash("isMoving");
        isJumpingHash = Animator.StringToHash("isJumping");
        isFallingHash = Animator.StringToHash("isFalling");
        isDownHash = Animator.StringToHash("isDown");
        standUpHash = Animator.StringToHash("standUp");
        standUpSpeedHash = Animator.StringToHash("standUpSpeed");
        verticalVelocityHash = Animator.StringToHash("verticalVelocity");
    }

    IEnumerator StandUp()
    {
        float standUpAnimationlength = 4.8f;
        animator.SetFloat(standUpSpeedHash, standUpAnimationlength / standUpTime);
        animator.SetTrigger(standUpHash);

        yield return new WaitForSeconds(standUpTime);

        willLandSafe = true;
    }

    void Update()
    {
        // Grounded
        isGrounded = Physics.CheckSphere(transform.position, groundDetection, groundMask);
        animator.SetBool(isGroundedHash, isGrounded);
        
        // Moving
        isMoving = playerBody.velocity.magnitude > 0.1f;
        animator.SetBool(isMovingHash, isMoving);

        // Jumping
        if (isJumping && isFalling)
        {
            isJumping = false;
            animator.SetBool(isJumpingHash, false);
        }

        // Falling end
        if (isFalling && isGrounded)
        {
            isFalling = false;
            animator.SetBool(isFallingHash, false);
            animator.SetBool(isDownHash, !willLandSafe);
            if (!willLandSafe) StartCoroutine(StandUp());
        }
        // Falling begin
        else if (verticalVelocity < -fallThresholdVelocity)
        {
            isFalling = true;
            animator.SetBool(isFallingHash, true);
        }

        // Landing
        if (isFalling)
        {
            animator.SetFloat(verticalVelocityHash, verticalVelocity);

            // Safe-land
            if (verticalVelocity >= -safeVelocity)
            {
                willLandSafe = true;
            }
            // Flattened
            else if (verticalVelocity <= -harmfulVelocity)
            {
                willLandSafe = false;
            }
            // Stumble
            else
            {
                // Do something
            }
        } 
        
        // Drag
        playerBody.drag = isGrounded ? groundDrag : airDrag;
    }

    void FixedUpdate()
    {
        Gravity();
        Move();
        verticalVelocity = Vector3.Dot(playerBody.velocity, transform.up);
    }

    void Gravity()
    {
        // Reset gravity acceleration
        if (isGrounded && gravityAcceleration != 0)
        {
            gravityAcceleration = 0f;
        }

        // Falling, relative to player's orientation
        if (isFalling)
        {
            gravityAcceleration += 25f * Time.deltaTime; // Increase gravity acceleration
            playerBody.AddForce(transform.up * -(gravityScalar + gravityAcceleration) * 2f, ForceMode.Acceleration);
        }
        // Ascending, relative to player's orientation
        else
        {
            playerBody.AddForce(transform.up * -gravityScalar, ForceMode.Acceleration);
        }
    }

    void Move()
    {
        if (!willLandSafe) return;

        if (isGrounded)
        {
            playerBody.AddForce(moveDirection, ForceMode.Acceleration);
        }
        else
        {
            playerBody.AddForce(moveDirection * airResistance, ForceMode.Acceleration);
        }
    }

    public void Jump()
    {
        if (isGrounded && willLandSafe)
        {
            // Cancel upwards velocity, relative to player's orientation
            playerBody.velocity = playerBody.velocity - Vector3.Project(playerBody.velocity, transform.up);
            
            // Add upwards force, relative to player's orientation
            playerBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            isJumping = true;
            animator.SetBool(isJumpingHash, true);
        }
    }

    public void UpdateMoveDirection(Vector3 direction)
    {
        moveDirection = direction.normalized * moveSpeed * 10f;
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
            Gizmos.DrawRay(transform.position, moveDirection.normalized);
        }
    }
    #endif
}
