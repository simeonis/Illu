using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRigController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] Transform _playerCamera;
    [SerializeField] Transform _orientation;

    [Header("Grappling Hook")]
    [SerializeField] GrapplingHookStateMachine _grapplingHook;

    [Header("Head")]
    [SerializeField] Rig _headRig;
    [SerializeField] Transform _head;
    [SerializeField] Transform _headTarget;

    [Header("Right Arm")]
    [SerializeField] Rig _rightArmRig;
    [SerializeField] Transform _rightArmTarget;
    [SerializeField] Transform _rightShoulder;
    [SerializeField] Transform _rightShoulderLimiter;

    // Getters & Setters - Grappling Hook
    bool IsFired { get => _grapplingHook.IsFired; }
    bool IsGrappled { get => _grapplingHook.IsGrappled; }
    Vector3 GrapplePoint { get => _grapplingHook.GrapplePoint; }
    Vector3 ExitPoint { get => _grapplingHook.ExitPoint; }

    void Start()
    {
        _headRig.weight = 1f;
        _rightArmRig.weight = 0f;
    }

    void Update()
    {
        HeadRig();
        RightArmRig();
    }

    void HeadRig()
    {
        float _dot = Vector3.Dot(_playerCamera.forward, _orientation.forward);
        Vector3 direction = (_head.position - _playerCamera.position).normalized;
        
        // Position target 5m infront of head
        _headTarget.position = _head.position + direction * 5f;

        // Adjust head rig weight based on dot product
        _headRig.weight = Mathf.Min(_dot + 1f, 1f);
    }

    void RightArmRig()
    {
        if (IsFired)
            RightArmFired();
        else if (IsGrappled)
            RightArmGrappled();
        else
            _rightArmRig.weight = 0f;
    }

    void RightArmFired()
    {
        _rightArmRig.weight = 1f;

        float dot = Vector3.Dot(_playerCamera.forward, _rightShoulderLimiter.forward);
        Debug.Log(dot);
        
        if (dot >= 0.65f)
        {
            // Calculate position from shoulder
            Vector3 aimTarget = _playerCamera.position + _playerCamera.forward * 1000f;
            Vector3 shoulderToAim = (aimTarget - _rightShoulder.position).normalized;

            // Position
            _rightArmTarget.position = _rightShoulder.position + shoulderToAim;
            
            // Rotation
            _rightArmTarget.right = aimTarget;
            _rightArmTarget.RotateAround(_rightArmTarget.position, _rightArmTarget.right, 90f);
        }
        else
        {
            float angle = Vector3.Angle(_playerCamera.forward, _rightShoulderLimiter.forward);
            Debug.Log("Angle: " + angle);
            Vector3 direction = Quaternion.AngleAxis(angle, _rightShoulderLimiter.up) * _rightShoulderLimiter.forward; 

            // Position
            _rightArmTarget.position = _rightShoulder.position + direction;
        }
    }

    void RightArmGrappled()
    {
        _rightArmRig.weight = 1f;

        // Position
        _rightArmTarget.position = GrapplePoint;

        // Rotation
        _rightArmTarget.right = (GrapplePoint - ExitPoint).normalized;
        _rightArmTarget.RotateAround(_rightArmTarget.position, _rightArmTarget.right, 90f);
    }
}