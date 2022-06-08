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
    int _isGroundedHash;
    int _isMovingHash;
    int _isJumpingHash; 
    int _isFallingHash;
    int _isSafeLandingHash;
    int _hasLandedHash;
    int _moveSpeedHash;
    int _fallSpeedHash;

    [Header("Rotation Modifiers")]
    [SerializeField] Transform _orientation;
    [SerializeField] Transform _playerCamera;
    float _turnSmoothVelocity;

    [Header("Locomotion Modifiers")]
    [SerializeField, Tooltip("Max walk speed")] 
    float _walkSpeed = 4f;
    [SerializeField, Tooltip("Max sprint speed")] 
    float _sprintSpeed = 10f;
    [SerializeField, Tooltip("Rate of change to reach max speed")]
    float _acceleration = 50;
    [SerializeField, Tooltip("Max acceleration")]
    float _maxAcceleration = 150f;
    [SerializeField, Tooltip("Acceleration factor based on direction change")]
    AnimationCurve _accelerationDotFactor;
    [SerializeField, Range(0f, 10f), Tooltip("Amount of drag being applied")] 
    float _drag = 3f;
    float _moveSpeed;
    Vector3 _moveDir;
    

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
    float _verticalVelocity = 0.0f;

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
    Rigidbody _playerBody;
    float _gravity;

    // Getters & Setters - Grappling Hook
    public bool IsGrappled { get { return _grapplingHook.IsGrappled; } }

    // Getters & Setters - Animation
    public Animator Animator { get { return _animator; } }
    public int IsGroundedHash { get { return _isGroundedHash; } }
    public int IsMovingHash { get { return _isMovingHash; } }
    public int IsJumpingHash { get { return _isJumpingHash; } }
    public int IsFallingHash { get { return _fallSpeedHash; } }
    public int IsSafeLandingHash { get { return _isSafeLandingHash; } }
    public int HasLandedHash { get { return _hasLandedHash; } }
    public int MoveSpeedHash { get { return _moveSpeedHash; } }
    public int FallSpeedHash { get { return _fallSpeedHash; } }

    // Getters & Setters - Locomotion
    public float MoveSpeed { get; set; }
    public float WalkSpeed { get { return _walkSpeed; } }
    public float SprintSpeed { get { return _sprintSpeed; } }
    public float SwingSpeed { get { return _walkSpeed * 0.05f; } }
    public Vector3 MoveDirection { get { return _moveDir; } }
    public float Acceleration { get { return _acceleration; } }
    public float MaxAcceleration { get { return _maxAcceleration; } }
    public AnimationCurve AccelerationDotFactor { get { return _accelerationDotFactor; } }

    // Getters & Setters - Drag
    public float DefaultDrag { get { return _drag; } set { _drag = value; } }
    public float Drag { get; set; }

    // Getters & Setters - Jump
    public float InitialJumpVelocity { get { return _initialJumpVelocity; } }
    public float CoyoteTime { get { return _coyoteTime; } }
    public float CoyoteTimeCounter { get { return _coyoteTimeCounter; } set { _coyoteTimeCounter = value; } }

    // Getters & Setters - Fall
    public float FallThresholdVelocity { get { return _fallThresholdVelocity; } }
    public float SafeVelocity { get { return _safeVelocity; } }
    public float HarmfulVelocity { get { return _harmfulVelocity; } }
    public float VerticalVelocity { get { return _verticalVelocity; } }

    // Getters & Setters - Ground
    public bool IsGrounded { get { return _isGrounded; } }

    // Getters & Setters - Player Input
    public bool IsMovementPressed { get { return _isMovementPressed; } }
    public bool IsJumpPressed { get { return _isJumpPressed; } set { _isJumpPressed = false; } }
    public bool IsSprintPressed { get { return _isSprintPressed; } }

    // Getters & Setters - Other
    public Rigidbody PlayerBody { get { return _playerBody; } }

    // Getters & Setters - State
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }

    void Start()
    {
        // Rigidbody
        _playerBody = GetComponent<Rigidbody>();

        Drag = DefaultDrag;

        // Calculate Gravity + Intial Jump Velocity
        float timeToApex = _maxJumpTime * 0.5f;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;

        // Animation Hash (Increases Performance)
        _isGroundedHash = Animator.StringToHash("isGrounded");
        _isMovingHash = Animator.StringToHash("isMoving");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _isFallingHash = Animator.StringToHash("isFalling");
        _isSafeLandingHash = Animator.StringToHash("isSafeLanding");
        _hasLandedHash = Animator.StringToHash("hasLanded");
        _moveSpeedHash = Animator.StringToHash("moveSpeed");
        _fallSpeedHash = Animator.StringToHash("fallSpeed");

        // State
        _states = new PlayerStateFactory(this);
        _currentState = _states.GetState<PlayerGroundState>();
        _currentState.EnterStates();
    }

    void Update()
    {
        _isGrounded = Physics.CheckSphere(transform.position, _groundDetection, _groundMask);
        if (!_isGrounded) _coyoteTimeCounter -= Time.deltaTime;
        _currentState.UpdateStates();
        _currentState.CheckSwitchStates();
    }

    void FixedUpdate()
    {
        Gravity();
        Locomotion();
        _verticalVelocity = Vector3.Dot(_playerBody.velocity, transform.up);
        _currentState.FixedUpdateStates();
    }

    void Gravity()
    {
        _playerBody.AddForce(transform.up * _gravity, ForceMode.Acceleration);
    }

    void Locomotion()
    {
        _playerBody.AddForce(MoveDirection * MoveSpeed, ForceMode.Acceleration);
        
        // Player's velocity (excluding vertical)
        Vector3 currVel = _playerBody.velocity - Vector3.Project(_playerBody.velocity, transform.up);
        
        _playerBody.AddForce(currVel * -Drag, ForceMode.Acceleration);
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

    // PLAYER INPUT
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