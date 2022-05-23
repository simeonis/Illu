using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    // Mouse variables
    [Header("Mouse Sensitivity")]
    [SerializeField, Range(0f, 100f)] private float sensitivity = 50f;
    [SerializeField] private CinemachineVirtualCamera cinemachineCamera;
    
    private CinemachinePOV cinemachinePOV;
    private PlayerMotor playerMotor;
    private InputAction inputMovement;

    void Start()
    {
        // Motor
        playerMotor = GetComponent<PlayerMotor>();

        // Camera
        cinemachinePOV = cinemachineCamera.GetCinemachineComponent<CinemachinePOV>();
        cinemachinePOV.m_HorizontalAxis.m_MaxSpeed = sensitivity / 100f * 3.2f;
        cinemachinePOV.m_VerticalAxis.m_MaxSpeed = sensitivity / 100f * 1.2f;

        // Movement
        inputMovement = InputManager.playerControls.Land.Movement;
    }

    void OnEnable() 
    {
        InputManager.playerControls.Land.Enable();

        // Jump
        InputManager.playerControls.Land.Jump.started += onJump;
        InputManager.playerControls.Land.Jump.canceled += onJump;

        // Sprint
        InputManager.playerControls.Land.Sprint.started += onSprint;
        InputManager.playerControls.Land.Sprint.canceled += onSprint;
    }

    void OnDisable() 
    {
        InputManager.playerControls.Land.Disable();

        // Jump
        InputManager.playerControls.Land.Jump.started -= onJump;
        InputManager.playerControls.Land.Jump.canceled -= onJump;

        // Sprint
        InputManager.playerControls.Land.Sprint.started -= onSprint;
        InputManager.playerControls.Land.Sprint.canceled -= onSprint;
    }

    void Update() => playerMotor.UpdateMovement(inputMovement.ReadValue<Vector2>());
    void onJump(InputAction.CallbackContext context) => playerMotor.SetJump(context.ReadValueAsButton());
    void onSprint(InputAction.CallbackContext context) => playerMotor.SetSprint(context.ReadValueAsButton());
}
