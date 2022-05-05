using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    // Movement variables
    [Header("Movement Modifiers")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float sprintAcceleration = 0.25f;
    [SerializeField, Range(0f, 1f)] private float airResistance = 0.4f;
    private float moveSpeed = 6f;
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.zero;
    private RaycastHit wallHit;

    // Drag variables
    [Header("Drag Modifiers")]
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 2f;

    // Jump variables
    [Header("Jump Modifiers")]
    [SerializeField] private float jumpForce = 8f;

    // Gravity variables
    [Header("Gravity Modifiers")]
    [SerializeField, Range(0f, 25f)] private float gravityScalar = 9.8f;
    private float gravityAcceleration = 0f;

    // Ground variables
    [Header("Ground Modifiers")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField, Range(0f, 1f)] private float groundDetection = 0.25f;
    private bool isGrounded = false;

    // Collisions variables
    [Header("Collisions")]
    [SerializeField] private CapsuleCollider wallCollider;
    [SerializeField] private CapsuleCollider groundCollider;

    private Rigidbody playerBody;

    void Start()
    {
        playerBody = GetComponent<Rigidbody>();

        // Movement
        moveSpeed = walkSpeed;
    }

    void Update()
    {
        // Checks
        isGrounded = Physics.CheckSphere(transform.position, groundDetection, groundMask);
        isMoving = playerBody.velocity.magnitude > 0.1f;
        
        // Drag
        playerBody.drag = isGrounded ? groundDrag : airDrag;
    }

    void FixedUpdate()
    {
        Gravity();

        // Apply movement
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
        if (isGrounded)
        {
            // Cancel upwards velocity, relative to player's orientation
            playerBody.velocity = playerBody.velocity - Vector3.Project(playerBody.velocity, transform.up);
            
            // Add upwards force, relative to player's orientation
            playerBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Gravity()
    {
        // Reset gravity acceleration
        if (isGrounded && gravityAcceleration != 0)
        {
            gravityAcceleration = 0f;
        }

        // Falling, relative to player's orientation
        if (Vector3.Dot(playerBody.velocity, transform.up) < 0f)
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

    public void UpdateMoveDirection(Vector3 direction)
    {
        moveDirection = direction.normalized * moveSpeed * 10f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, groundDetection);
    }
}
