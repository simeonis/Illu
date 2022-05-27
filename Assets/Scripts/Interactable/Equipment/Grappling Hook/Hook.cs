using UnityEngine;
using UnityEngine.Events;

public class Hook : MonoBehaviour
{
    [HideInInspector] public UnityEvent<Collision> OnCollision;
    public Rigidbody Rigidbody { get { return _rigidbody; } }
    
    [SerializeField] CollisionDetectionMode collisionMode = CollisionDetectionMode.Discrete;
    Rigidbody _rigidbody;
    Collider _collider;

    void Start()
    {
        OnCollision = new UnityEvent<Collision>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        
        _rigidbody.interpolation = RigidbodyInterpolation.None;
        Disable();
    }

    void OnCollisionEnter(Collision collision) => OnCollision.Invoke(collision);

    public void Enable()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.collisionDetectionMode = collisionMode;
        _collider.enabled = true;
    }

    public void Disable()
    {
        _rigidbody.isKinematic = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _rigidbody.velocity = Vector3.zero;
        _collider.enabled = false;
    }

    ~Hook() => OnCollision.RemoveAllListeners();
}