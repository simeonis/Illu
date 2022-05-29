using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using System;

public delegate void PlayerJumpedEventHandler(System.Object sender, BoolEventArgs args);
public delegate void PlayerSprintEventHandler(System.Object sender, BoolEventArgs args);

public class BoolEventArgs : EventArgs { public bool state { get; set; } }

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

    public event PlayerJumpedEventHandler playerJumped;
    public event PlayerSprintEventHandler playerSprint;

    public Vector2 dir;


    void Start()
    {
        // Motor
        playerMotor = GetComponent<PlayerMotor>();

        // Camera
        cinemachinePOV = cinemachineCamera.GetCinemachineComponent<CinemachinePOV>();
        cinemachinePOV.m_HorizontalAxis.m_MaxSpeed = sensitivity / 100f * 3.2f;
        cinemachinePOV.m_VerticalAxis.m_MaxSpeed = sensitivity / 100f * 1.2f;

        // Movement
        inputMovement = InputManager.Instance.playerControls.Player.Movement;
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

    void Update()
    {
        dir = inputMovement.ReadValue<Vector2>();
        playerMotor.UpdateMovement(
            dir
        );

    }
    void onJump(InputAction.CallbackContext context)
    {
        var state = context.ReadValueAsButton();
        var args = new BoolEventArgs();
        args.state = state;
        playerJumped?.Invoke(this, args);
        playerMotor.SetJump(state);
    }
    void onSprint(InputAction.CallbackContext context)
    {
        var state = context.ReadValueAsButton();
        var args = new BoolEventArgs();
        args.state = state;
        playerSprint?.Invoke(this, args);
        playerMotor.SetSprint(context.ReadValueAsButton());
    }
}
