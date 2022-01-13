using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class NetworkPlayerController : NetworkBehaviour
{
    // User Input variables
    private Vector2 movementInput;
    private Vector2 lookInput;

    [Header("Transforms")]
    [SerializeField] private Transform orientation;

    private SyncPlayer syncPlayer;
    private PlayerMotor playerMotor;

    // Mouse variables
    [Header("Mouse Sensitivity")]
    [SerializeField, Range(0f, 1000f)] private float horizontalSensitivity = 10f;
    [SerializeField, Range(0f, 1000f)] private float verticalSensitivity = 10f;

    void Start()
    {
        if (hasAuthority)
            GameManager.TriggerEvent("PlayerSpawned");

        playerMotor = GetComponent<PlayerMotor>();
        syncPlayer = GetComponent<SyncPlayer>();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        enabled = true;

        // Jump
        InputManager.playerControls.Land.Jump.performed += Jump;

        // Sprint
        InputManager.playerControls.Land.Sprint.performed += Sprint;
        InputManager.playerControls.Land.Sprint.canceled += Walk;

        // Crouch
        InputManager.playerControls.Land.Crouch.performed += Crouch;
        InputManager.playerControls.Land.Crouch.canceled += UnCrouch;
    }

    void OnDisable()
    {
        // Jump
        InputManager.playerControls.Land.Jump.performed -= Jump;

        // Sprint
        InputManager.playerControls.Land.Sprint.performed -= Sprint;
        InputManager.playerControls.Land.Sprint.canceled -= Walk;

        // Crouch
        InputManager.playerControls.Land.Crouch.performed -= Crouch;
        InputManager.playerControls.Land.Crouch.canceled -= UnCrouch;
    }

    void Update()
    {
        if (hasAuthority)
        {
            UserInput();
            playerMotor.LookDirection(lookInput);
        }

        // animator.SetFloat("Horizontal", xRotation);
    }

    private void UserInput()
    {
        lookInput = InputManager.playerControls.Land.Look.ReadValue<Vector2>();
        lookInput.x *= horizontalSensitivity * 0.01f;
        lookInput.y *= verticalSensitivity * 0.01f;

        movementInput = InputManager.playerControls.Land.Movement.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        UserMovement();
    }

    private void UserMovement()
    {
        // Calculate movement direction
        if (hasAuthority)
        {
            var moveDirection = orientation.forward * movementInput.y + orientation.right * movementInput.x;
            playerMotor.UserMovement(moveDirection);
        }
    }

    private void Sprint(InputAction.CallbackContext _)
    {
        playerMotor.isSprinting = true;
        if (!playerMotor.isCrouching)
        {
            if (hasAuthority)
                syncPlayer.CmdHandleSprint(true);

            playerMotor.Sprint();
        }
    }

    private void Walk(InputAction.CallbackContext _)
    {
        playerMotor.isSprinting = false;
        if (!playerMotor.isCrouching)
        {
            if (hasAuthority)
                syncPlayer.CmdHandleSprint(false);

            playerMotor.Walk();
        }
    }

    private void Jump(InputAction.CallbackContext _)
    {
        if (playerMotor.canJump && playerMotor.isGrounded)
        {
            if (hasAuthority)
                syncPlayer.CmdSendJump();
            // Disable jump capability
            playerMotor.canJump = false;

            playerMotor.Jump();
        }
    }

    private void Crouch(InputAction.CallbackContext _)
    {
        playerMotor.isCrouching = true;

        if (hasAuthority)
            syncPlayer.CmdHandleCrouch(true);

        playerMotor.Crouch();
    }

    private void UnCrouch(InputAction.CallbackContext _)
    {
        // No obstruction
        if (!playerMotor.underCeiling)
        {
            playerMotor.isCrouching = false;

            if (hasAuthority)
                syncPlayer.CmdHandleCrouch(false);

            playerMotor.UnCrouch();
        }
        // Obstruction
        else if (playerMotor.isCrouching && playerMotor.underCeiling)
        {
            playerMotor.stuckCrouching = true;
        }
    }
}