using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerInputController : MonoBehaviour
{
    // Mouse variables
    [Header("Mouse Sensitivity")]
    [SerializeField, Range(0f, 100f)] float _sensitivity = 50f;
    [SerializeField] CinemachineVirtualCamera _cinemachineCamera;
    
    CinemachinePOV _cinemachinePOV;
    IPlayerMotor _playerMotor;
    InputAction _inputMovement;

    void Awake()
    {
        _playerMotor = GetComponent<IPlayerMotor>();
    }

    void Start()
    {
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

        InputManager.Instance.playerControls.Player.AlternateFire.started += OnAlternateFire;

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

        InputManager.Instance.playerControls.Player.AlternateFire.started -= OnAlternateFire;

        // Jump
        InputManager.Instance.playerControls.Player.Jump.started -= onJump;
        InputManager.Instance.playerControls.Player.Jump.canceled -= onJump;

        // Sprint
        InputManager.Instance.playerControls.Player.Sprint.started -= onSprint;
        InputManager.Instance.playerControls.Player.Sprint.canceled -= onSprint;
    }

    bool ragdoll = false;
    void OnAlternateFire(InputAction.CallbackContext context) 
    {
        if (!ragdoll)
        {
            _playerMotor.EnableRagdoll();
            ragdoll = true;
        }
        else
        {
            _playerMotor.DisableRagdoll();
            ragdoll = false;
        }
    }

    void Update() => _playerMotor.SetMovement(_inputMovement.ReadValue<Vector2>());
    void onJump(InputAction.CallbackContext context) => _playerMotor.SetJump(context.ReadValueAsButton());
    void onSprint(InputAction.CallbackContext context) => _playerMotor.SetSprint(context.ReadValueAsButton());
}
