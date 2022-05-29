using UnityEngine;

public class MovingPlatform : InertialPlatform
{
    [SerializeField] Vector3 _offset = Vector3.up;
    [SerializeField] float _speed = 1.0f;
    [SerializeField] bool _loop = false;
    bool _isTarget = false;
    Vector3 _initialPos;
    Vector3 _targetPos;
    Vector3 _loopTarget;

    [Header("Projection Settings")]
    [SerializeField] Color _color = Color.red;
    [HideInInspector] public bool Fill = false;
    [HideInInspector] public bool Visualize = true;

    void Start()
    {
        _initialPos = transform.position;
        _targetPos = transform.position + _offset;
        _loopTarget = _targetPos;
    }

    protected override void PlayerEnter() => _isTarget = true;
    protected override void PlayerExit() => _isTarget = false;

    void FixedUpdate()
    {
        if (_loop)
        {
            MoveTowards(_loopTarget);
            if (transform.position == _initialPos)
                _loopTarget = _targetPos;
            else if (transform.position == _targetPos)
                _loopTarget = _initialPos;
        }
        else
        {
            MoveTowards(_isTarget ? _targetPos : _initialPos);
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
            if (Fill) Gizmos.DrawCube(transform.position + _offset, transform.localScale);
            else Gizmos.DrawWireCube(transform.position + _offset, transform.localScale);
        }
    }
    #endif
}