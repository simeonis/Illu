using UnityEngine;
using UnityEngine.Events;

public class Hook : MonoBehaviour
{
    Rigidbody _rigidbody;
    Collider _collider;
    UnityEvent<Collision> _event;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _event = new UnityEvent<Collision>();
        Disable();
    }

    void OnCollisionEnter(Collision collision) => _event.Invoke(collision);

    public void Enable()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _collider.enabled = true;
    }

    public void Disable()
    {
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _rigidbody.isKinematic = true;
        _collider.enabled = false;
    }

    public void AddListener(UnityAction<Collision> action) => _event.AddListener(action);
    public void RemoveListener(UnityAction<Collision> action) => _event.RemoveListener(action);
    public void RemoveAllListener() => _event.RemoveAllListeners();
    ~Hook() => _event.RemoveAllListeners();
}