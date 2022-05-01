using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(PlayerMotor2))]
public class PlayerController2 : MonoBehaviour
{
    // Mouse variables
    [Header("Mouse Sensitivity")]
    [SerializeField, Range(0f, 100f)] private float sensitivity = 50f;
    private float xRotation, yRotation;

    [SerializeField] Transform orientation;
    [SerializeField] Transform playerCamera;
    [SerializeField] CinemachineVirtualCamera cinemachineCamera;
    private CinemachinePOV cinemachinePOV;
    private PlayerControls playerControls;
    private PlayerMotor2 playerMotor;

    void Awake()
    {
        playerControls = new PlayerControls();
    }

    void Start()
    {
        // Motor
        playerMotor = GetComponent<PlayerMotor2>();

        // Camera
        cinemachinePOV = cinemachineCamera.GetCinemachineComponent<CinemachinePOV>();
        cinemachinePOV.m_HorizontalAxis.m_MaxSpeed = sensitivity / 100f * 3.2f;
        cinemachinePOV.m_VerticalAxis.m_MaxSpeed = sensitivity / 100f * 1.2f;

        // Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() 
    {
        playerControls.Land.Enable();

        // Jump
        playerControls.Land.Jump.performed += context => Jump();
    }

    void OnDisable() 
    {
        playerControls.Land.Disable();

        // Jump
        playerControls.Land.Jump.performed -= context => Jump();
    }

    void Update()
    {
        SetLookDirection();
        playerMotor.UpdateMoveDirection(moveDirection);
    }

    private Vector3 moveDirection = Vector3.zero;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    float VectorAngle360(Vector3 from, Vector3 to, Vector3 right)
    {
        float angle = Vector3.Angle(from, to);
        return (Vector3.Angle(right, to) > 90f) ? 360f - angle : angle;            
    }

    void SetLookDirection()
    {
        Vector2 movementInput = playerControls.Land.Movement.ReadValue<Vector2>();
        Vector3 direction = (transform.forward * movementInput.y + transform.right * movementInput.x).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Angle 
            float targetAngle = VectorAngle360(transform.forward, direction, transform.right) + playerCamera.localEulerAngles.y;
            
            float smoothAngle = Mathf.SmoothDampAngle(orientation.localEulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            orientation.rotation = transform.rotation * Quaternion.Euler(0f, smoothAngle, 0f);

            moveDirection = (Quaternion.Euler(transform.up * targetAngle) * transform.forward).normalized;
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }

    void Jump() 
    {
        playerMotor.Jump();
    }

}
