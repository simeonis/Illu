using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHookStateMachine : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] Hook _hook;
    [SerializeField] Transform exitPoint;
    [SerializeField] float projectileSpeed = 100f;
    [SerializeField] LayerMask hookableLayers;
    Transform _defaultHookParent;
    Vector3 _grappleTarget;

    [Header("Rope")]
    [SerializeField] float maxRopeLength = 200f;
    [SerializeField] FloatVariable _grappleDistance;
    [SerializeField, Range(1, 500)] int resolution = 100; 
    [SerializeField, Range(1, 10)] int wobbleCount = 3;
    [SerializeField, Range(1, 10f)] float waveCount = 2;
    [SerializeField, Range(0, 5f)] float waveHeight = 0.5f;
    [SerializeField] LineRenderer ropeRenderer;
    float _ropeRemaining = 200f;

    [Header("Player")]
    [SerializeField] Transform viewpoint;
    bool _isPrimaryPressed = false;

    GrapplingHookBaseState _currentState;
    GrapplingHookStateFactory _states;

    // Getters & Setters - Projectile
    public Hook Hook { get { return Hook; } }
    public Transform HookTransform { get { return _hook.transform; } }
    public Transform DefaultHookParent { get { return _defaultHookParent; } }
    public Transform ExitPoint { get { return exitPoint; } }
    public float ProjectileSpeed { get { return projectileSpeed; } }
    public LayerMask HookableLayers { get { return hookableLayers; } }
    public Vector3 GrappleTarget { get { return _grappleTarget; } set { _grappleTarget = value; } }

    // Getters & Setters - Rope
    public float MaxRopeLength { get { return maxRopeLength; } }
    public float RopeRemaining { get { return _ropeRemaining; } set { _ropeRemaining = value; } }
    public float GrappleDistance { get { return _grappleDistance.Value; } set { _grappleDistance.Value = value; } }
    public int Resolution { get { return resolution; } }
    public int WobbleCount { get { return wobbleCount; } }
    public float WaveCount { get { return waveCount; } }
    public float WaveHeight { get { return waveHeight; } }
    public LineRenderer RopeRenderer { get { return ropeRenderer; } }
    
    // Getters & Setters - Player
    public Transform Viewpoint { get { return viewpoint; } }
    public bool IsPrimaryPressed { get { return _isPrimaryPressed; } set { _isPrimaryPressed = false; } }
    
    // Getters & Setters - State
    public GrapplingHookBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }

    void Start()
    {
        _states = new GrapplingHookStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();

        _defaultHookParent = _hook.transform.parent;
        _ropeRemaining = maxRopeLength;
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
    public bool IsDebugging { get { return enable; } }
    void OnDrawGizmos()
    {
        if (Application.isPlaying && IsDebugging) 
            _currentState.GizmosState();
    }
    #endif
}