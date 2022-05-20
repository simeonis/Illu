using UnityEngine;

[RequireComponent(typeof(SpringJoint))]
public class GrapplingHook : Equipment
{
    [Header("Grappling Hook")]
    [SerializeField] private Transform hook;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private LayerMask hookableLayers;
    private Transform source;
    private Transform defaultHookParent;
    private GrapplingState state = GrapplingState.Idle;
    private enum GrapplingState
    {
        Idle,
        Fired,
        Hit,
    }

    [Header("Grappling Modifiers")]
    public float ropeLength = 200;

    protected override void Start()
    {
        base.Start();
        defaultHookParent = hook.parent;
    }

    private Vector3 target;
    private RaycastHit grappleHit;
    private float distanceFromPlayerToCam;
    public override void EquipmentPrimaryPressed() 
    { 
        source = player.playerCamera;
        if (state == GrapplingState.Idle)
        {
            distanceFromPlayerToCam = Vector3.Distance(source.position, FindNearestPointOnLine(source.position, source.forward, exitPoint.position));
            if (Physics.Raycast(exitPoint.position, source.forward, out grappleHit, distanceFromPlayerToCam + ropeLength, hookableLayers))
            {
                state = GrapplingState.Hit;
                hook.parent = null;
                hook.position = grappleHit.point;
            }
        }
    }
    public override void EquipmentPrimaryReleased() 
    {  
        if (state == GrapplingState.Hit)
        {
            state = GrapplingState.Idle;
            hook.position = Vector3.zero;
            hook.SetParent(defaultHookParent, false);
        }
    }

    public override void EquipmentSecondaryPressed() { Debug.Log("Grappling Hook Secondary Pressed"); }
    public override void EquipmentSecondaryReleased() { Debug.Log("Grappling Hook Secondary Released"); }

    private Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 direction, Vector3 point)
    {
        direction.Normalize();
        Vector3 lhs = point - origin;

        float dotP = Vector3.Dot(lhs, direction);
        return origin + direction * dotP;
    }

    #if UNITY_EDITOR
    [Header("Debug")] 
    public bool enable = false;
    void OnDrawGizmos()
    {
        if (enable && player)
        {
            source = player.playerCamera;
            float distanceFromPlayerToCam = Vector3.Distance(source.position, FindNearestPointOnLine(source.position, source.forward, exitPoint.position));
            if (Physics.Raycast(source.position, source.forward, out RaycastHit hit, distanceFromPlayerToCam + ropeLength, hookableLayers))
            {
                Gizmos.color = Color.green;
                Vector3 direction = hit.point - exitPoint.position;
                Gizmos.DrawRay(exitPoint.position, direction);
                Gizmos.DrawSphere(hit.point, 0.125f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(exitPoint.position, source.forward * (distanceFromPlayerToCam + ropeLength));
            }
        }
    }
    #endif
}