using UnityEngine;

public class GrapplingHook : Equipment
{
    [Header("Grappling Hook")]
    [SerializeField] private Transform hook;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private LayerMask hookableLayers;
    private Transform playerViewpoint;
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
    public override void EquipmentPrimaryPressed() 
    { 
        if (state == GrapplingState.Idle)
        {
            playerViewpoint = player.GetViewpoint();
            float distanceFromPlayerToCam = DistanceFromCameraToExitPoint();
            if (Physics.Raycast(playerViewpoint.position, playerViewpoint.forward, out RaycastHit hit, distanceFromPlayerToCam + ropeLength, hookableLayers))
            {
            }
            else
            {
            }
        }
    }

    public override void EquipmentPrimaryReleased() 
    {  
    }

    public override void EquipmentSecondaryPressed() { Debug.Log("Grappling Hook Secondary Pressed"); }
    public override void EquipmentSecondaryReleased() { Debug.Log("Grappling Hook Secondary Released"); }

    /// <summary>
    /// Calculates 
    /// </summary>
    private float DistanceFromCameraToExitPoint()
    {
        Vector3 nearestPointOnLine = FindNearestPointOnLine(playerViewpoint.position, playerViewpoint.forward, exitPoint.position);
        return Vector3.Distance(playerViewpoint.position, nearestPointOnLine);
    }

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
            playerViewpoint = player.GetViewpoint();
            float distanceFromPlayerToCam = DistanceFromCameraToExitPoint();
            if (Physics.Raycast(playerViewpoint.position, playerViewpoint.forward, out RaycastHit hit, distanceFromPlayerToCam + ropeLength, hookableLayers))
            {
                Gizmos.color = Color.green;
                Vector3 direction = hit.point - exitPoint.position;
                Gizmos.DrawRay(exitPoint.position, direction);
                Gizmos.DrawSphere(hit.point, 0.125f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(exitPoint.position, playerViewpoint.forward * (distanceFromPlayerToCam + ropeLength));
            }
        }
    }
    #endif
}