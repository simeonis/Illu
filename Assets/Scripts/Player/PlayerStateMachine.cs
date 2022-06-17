using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    [Header("Grappling Hook")]
    [SerializeField] GrapplingHookStateMachine _grapplingHook;

    [Header("Animation Modifiers")]
    [SerializeField] Animator _animator;
    [SerializeField, Tooltip("Player \"Landing Flat\" animation clip")]
    AnimationClip _landingFlatAnimationClip;
    [SerializeField, Tooltip("Player \"Stand Up\" animation clip")]
    AnimationClip _standUpAnimationClip;

    [Header("Rotation Modifiers")]
    [SerializeField] Transform _orientation;
    [SerializeField] Transform _playerCamera;
    float _turnSmoothVelocity;

    [Header("Locomotion Modifiers")]
    [SerializeField, Tooltip("Max walk speed")]
    float _walkSpeed = 4f;
    [SerializeField, Tooltip("Max sprint speed")]
    float _sprintSpeed = 10f;
    [SerializeField, Range(0f, 1f)]
    float _friction = 0.5f;
    [SerializeField, Tooltip("Velocity factor based on direction change")]
    AnimationCurve _dotCurveFactor;
    Vector3 _moveDir;
    Vector3 _verticalVel;
    Vector3 _horizontalVel;
    float _moveSpeed;
    float _turnFactor;

    [Header("Jump Modifiers")]
    [SerializeField, Tooltip("Apex of jump")]
    float _maxJumpHeight = 1.5f;
    [SerializeField, Tooltip("Time to complete a full jump in seconds")]
    float _maxJumpTime = 0.75f;
    [SerializeField, Tooltip("Amount of time a player can still jump, even after walking off a platform")]
    float _coyoteTime = 0.15f;
    float _coyoteTimeCounter;
    float _initialJumpVelocity;

    [Header("Fall Modifiers")]
    [SerializeField, Tooltip("Threshold velocity in which the player will count as falling")]
    float _fallThresholdVelocity = 1f;
    [SerializeField, Tooltip("Velocity or lower that ensures a safe landing")]
    float _safeVelocity = 20f;
    [SerializeField, Tooltip("Velocity or higher that causes a harmful landing")]
    float _harmfulVelocity = 24f;

    [Header("Ground Modifiers")]
    [SerializeField, Tooltip("Layers that the player can collide with")]
    LayerMask _groundMask;
    [SerializeField, Range(0f, 1f), Tooltip("Range from feet that checks if the player is grounded")]
    float _groundDetection = 0.18f;
    bool _isGrounded = false;

    // Player Input
    bool _isMovementPressed = false;
    bool _isJumpPressed = false;
    bool _isSprintPressed = false;

    // Other
    Rigidbody _rigidbody;
    float _gravity;

    // Getters & Setters - Grappling Hook
    public bool IsGrappled { get => _grapplingHook.IsGrappled; }
    public bool HasLandedFromSwinging { get; set; } = true;

    // Getters & Setters - Animation
    public Animator Animator { get => _animator; }
    public int IsWalkingHash { get => Animator.StringToHash("isWalking"); }
    public int IsSprintingHash { get => Animator.StringToHash("isSprinting"); }
    public int IsJumpingHash { get => Animator.StringToHash("isJumping"); }

    // Getters & Setters - Locomotion
    public float MoveSpeed { get; set; }
    public float WalkSpeed { get => _walkSpeed; }
    public float SprintSpeed { get => _sprintSpeed; }
    public Vector3 MoveDirection { get => _moveDir; }

    // Getters & Setters - Jump
    public float InitialJumpVelocity { get => _initialJumpVelocity; }
    public float CoyoteTime { get => _coyoteTime; }
    public float CoyoteTimeCounter { get => _coyoteTimeCounter; set => _coyoteTimeCounter = value; }

    // Getters & Setters - Fall
    public float FallThresholdVelocity { get => _fallThresholdVelocity; }
    public float SafeVelocity { get => _safeVelocity; }
    public float HarmfulVelocity { get => _harmfulVelocity; }

    // Getters & Setters - Ground
    public bool IsGrounded { get => _isGrounded; }

    // Getters & Setters - Player Input
    public bool IsMovementPressed { get => _isMovementPressed; }
    public bool IsJumpPressed { get => _isJumpPressed; set => _isJumpPressed = false; }
    public bool IsSprintPressed { get => _isSprintPressed; }

    // Getters & Setters - Other
    public Rigidbody PlayerRigidbody { get => _rigidbody; }

    // Getters & Setters - State
    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }

    void Start()
    {
        // Find rigidbody
        _rigidbody = GetComponent<Rigidbody>();

        // Calculate Gravity + Intial Jump Velocity
        float timeToApex = _maxJumpTime * 0.5f;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;

        // State
        _states = new PlayerStateFactory(this);
        _currentState = _states.GetState<PlayerGroundState>();
        _currentState.EnterStates();
    }

    void Update()
    {
        _isGrounded = Physics.CheckSphere(transform.position, _groundDetection, _groundMask);
        if (!_isGrounded && _coyoteTimeCounter >= 0) _coyoteTimeCounter -= Time.deltaTime;
        _currentState.UpdateStates();
        _currentState.CheckSwitchStates();
    }

    void FixedUpdate()
    {
        Gravity();
        Locomotion();
        _currentState.FixedUpdateStates();
    }

    void Gravity()
    {
        _rigidbody.AddForce(transform.up * _gravity, ForceMode.Acceleration);
    }

    void Locomotion()
    {
        // Calculate the dot product between the current and target velocity,
        // then evaluate it against the turning animation curve, "dotCurveFactor".
        // This helps turning feel snappier as the player no longer has to slow down, then speed up
        _turnFactor = _dotCurveFactor.Evaluate(Vector3.Dot(MoveDirection, _rigidbody.velocity));

        // Apply movement force
        _rigidbody.AddForce(MoveDirection * MoveSpeed * _turnFactor, ForceMode.Acceleration);

        // Split vertical & horizontal velocity
        _verticalVel = Vector3.Scale(_rigidbody.velocity, transform.up);
        _horizontalVel = (_rigidbody.velocity - _verticalVel);

        // Apply friction ONLY to horizontal velocity (i.e., ignore jump + gravity)
        // Note: While swinging, friction is non-existent (up until the player lands)
        _horizontalVel *= Mathf.Pow(HasLandedFromSwinging ? _friction : 1f, Time.deltaTime);

        // Re-combine vertical & horizontal velocity
        _rigidbody.velocity = _verticalVel + _horizontalVel;
    }

    #if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] bool enable = false;
    void OnDrawGizmos()
    {
        if (Application.isPlaying && enable)
            _currentState.GizmosState();
    }
    #endif

    // SECTION: PLAYER INPUT
    public void SetMovement(Vector2 input)
    {
        _isMovementPressed = input.x != 0 || input.y != 0;
        _moveDir = (transform.forward * input.y) + (transform.right * input.x);

        // Rotates player in direction of movement, relative to camera's direction
        if (_moveDir.magnitude >= 0.1f)
        {
            // Visual explanation: https://youtu.be/4HpC--2iowE?t=762
            float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + _playerCamera.localEulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(_orientation.localEulerAngles.y, targetAngle, ref _turnSmoothVelocity, 0.1f);
            _orientation.rotation = transform.rotation * Quaternion.Euler(0f, smoothAngle, 0f);
            _moveDir = Quaternion.Euler(transform.up * targetAngle) * transform.forward;
        }
    }

    public void SetJump(bool isPressed) => _isJumpPressed = isPressed;
    public void SetSprint(bool isPressed) => _isSprintPressed = isPressed;
}