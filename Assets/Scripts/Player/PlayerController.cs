using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerController : MonoBehaviour
{
    // Mouse variables
    [Header("Mouse Sensitivity")]
    [SerializeField, Range(0f, 100f)] float _sensitivity = 50f;
    [SerializeField] CinemachineVirtualCamera _cinemachineCamera;
    
    CinemachinePOV _cinemachinePOV;
    PlayerStateMachine _playerStateMachine;
    InputAction _inputMovement;

    void Start()
    {
        // Motor
        _playerStateMachine = GetComponent<PlayerStateMachine>();

        // Camera
        _cinemachinePOV = _cinemachineCamera.GetCinemachineComponent<CinemachinePOV>();
        _cinemachinePOV.m_HorizontalAxis.m_MaxSpeed = _sensitivity / 100f * 3.2f;
        _cinemachinePOV.m_VerticalAxis.m_MaxSpeed = _sensitivity / 100f * 1.2f;

        // Movement
        _inputMovement = InputManager.Instance.playerControls.Player.Movement;
    }

    void OnEnable() 
    {
        InputManager.Instance.playerControls.Player.Enable();

        // Jump
        InputManager.Instance.playerControls.Player.Jump.started += onJump;
        InputManager.Instance.playerControls.Player.Jump.canceled += onJump;

        // Sprint
        InputManager.Instance.playerControls.Player.Sprint.started += onSprint;
        InputManager.Instance.playerControls.Player.Sprint.canceled += onSprint;
    }

    void OnDisable() 
    {
        InputManager.Instance.playerControls.Player.Disable();

        // Jump
        InputManager.Instance.playerControls.Player.Jump.started -= onJump;
        InputManager.Instance.playerControls.Player.Jump.canceled -= onJump;

        // Sprint
        InputManager.Instance.playerControls.Player.Sprint.started -= onSprint;
        InputManager.Instance.playerControls.Player.Sprint.canceled -= onSprint;
    }

    void Update() => _playerStateMachine.SetMovement(_inputMovement.ReadValue<Vector2>());
    void onJump(InputAction.CallbackContext context) => _playerStateMachine.SetJump(context.ReadValueAsButton());
    void onSprint(InputAction.CallbackContext context) => _playerStateMachine.SetSprint(context.ReadValueAsButton());
}
