using UnityEngine;

public class MovingPlatform : InertialPlatform
{
    public Vector3 TargetPosition;
    [SerializeField] float _speed = 1.0f;
    [SerializeField] bool _loop = false;
    bool _isTarget = false;
    Vector3 _initialPos;
    Vector3 _loopTarget;

    [Header("Projection Settings")]
    [SerializeField] Color _color = Color.red;
    [SerializeField] float _radius = 1f;
    [HideInInspector] public bool Fill = false;
    [HideInInspector] public bool Visualize = true;

    void Start()
    {
        _initialPos = transform.position;
        _loopTarget = TargetPosition;
    }

    protected override void PlayerEnter() => _isTarget = true;
    protected override void PlayerExit() => _isTarget = false;

    void FixedUpdate()
    {
        if (_loop)
        {
            MoveTowards(_loopTarget);
            if (transform.position == _initialPos)
                _loopTarget = TargetPosition;
            else if (transform.position == TargetPosition)
                _loopTarget = _initialPos;
        }
        else
        {
            MoveTowards(_isTarget ? TargetPosition : _initialPos);
        }
    }

    void MoveTowards(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * _speed);
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Visualize)
        {
            Gizmos.color = _color;
            if (Fill) Gizmos.DrawSphere(TargetPosition, _radius);
            else Gizmos.DrawWireSphere(TargetPosition, _radius);
        }
    }
    #endif
}