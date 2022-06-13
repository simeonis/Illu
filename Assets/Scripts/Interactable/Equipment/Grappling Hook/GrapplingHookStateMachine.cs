using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHookStateMachine : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] Transform _hook;
    [SerializeField] Transform _exitPoint;
    [SerializeField] float _projectileSpeed = 100f;
    [SerializeField] LayerMask _hookableLayers;
    Transform _defaultHookParent;

    [Header("Rope")]
    [SerializeField] float _maxRopeLength = 200f;
    [SerializeField, Range(1, 500)] int _resolution = 100; 
    [SerializeField, Range(1, 10)] int _wobbleCount = 3;
    [SerializeField, Range(1, 10f)] float _waveCount = 2;
    [SerializeField, Range(0, 5f)] float _waveHeight = 0.5f;
    [SerializeField] LineRenderer _ropeRenderer;

    [Header("Player")]
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] Transform _viewpoint;
    bool _isPrimaryPressed = false;

    [Header("Scriptable Objects")]
    [SerializeField] FloatVariable _ropeRemaining;
    [SerializeField] FloatVariable _grappleDistance;

    GrapplingHookBaseState _currentState;
    GrapplingHookStateFactory _states;

    // Getters & Setters - Projectile
    public Transform Hook { get { return _hook; } }
    public Transform HookDefaultParent { get { return _defaultHookParent; } }
    public Vector3 HookPosition { get { return _hook.position; } set { _hook.position = value; } }
    public Vector3 ExitPoint { get { return _exitPoint.position; } }
    public float ProjectileSpeed { get { return _projectileSpeed; } }
    public LayerMask HookableLayers { get { return _hookableLayers; } }
    public Vector3 GrapplePoint { get; set; }

    // Getters & Setters - Rope
    public float MaxRopeLength { get { return _maxRopeLength; } }
    public float RopeRemaining { get { return _ropeRemaining.Value; } set { _ropeRemaining.Value = value; } }
    public float GrappleDistance { get { return _grappleDistance.Value; } set { _grappleDistance.Value = value; } }
    public int Resolution { get { return _resolution; } }
    public int WobbleCount { get { return _wobbleCount; } }
    public float WaveCount { get { return _waveCount; } }
    public float WaveHeight { get { return _waveHeight; } }
    public LineRenderer RopeRenderer { get { return _ropeRenderer; } }
    
    // Getters & Setters - Player
    public Rigidbody PlayerRigidbody { get { return _rigidbody; } }
    public Transform PlayerViewpoint { get { return _viewpoint; } }
    public bool IsPrimaryPressed { get { return _isPrimaryPressed; } set { _isPrimaryPressed = false; } }
    
    // Getters & Setters - State
    public GrapplingHookBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public bool IsGrappled { get { return _currentState is GrapplingHookGrappledState; } }

    void Start()
    {
        _defaultHookParent = _hook.parent;
        _ropeRemaining.Value = _maxRopeLength;

        _states = new GrapplingHookStateFactory(this);
        _currentState = _states.GetState<GrapplingHookIdleState>();
        _currentState.EnterState();
    }

    void OnEnable()
    {
        InputManager.Instance.playerControls.Player.Fire.started += FirePressed;
        InputManager.Instance.playerControls.Player.Fire.canceled += FirePressed;
    }

    void OnDisable()
    {
        InputManager.Instance.playerControls.Player.Fire.started -= FirePressed;
        InputManager.Instance.playerControls.Player.Fire.canceled -= FirePressed;
    }

    void Update()
    {
        _currentState.UpdateState();
        _currentState.CheckSwitchState();
    }

    void FixedUpdate()
    {
        _currentState.FixedUpdateState();
    }

    void FirePressed(InputAction.CallbackContext context)
    {
        _isPrimaryPressed = context.ReadValueAsButton();
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
}