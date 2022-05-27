using UnityEngine;

public class GrapplingHookIdleState : GrapplingHookBaseState
{
    public GrapplingHookIdleState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}
    
    RaycastHit hit, gizmosHit;
    float distance = 0f;
    bool didHit = false;

    public override void EnterState()
    {
        // Setter prevents value from being set to true
        _ctx.IsPrimaryPressed = false;
    }

    public override void UpdateState()
    {
        distance = RopeLengthWithCameraOffset();
        didHit = Physics.Raycast(_ctx.Viewpoint.position, _ctx.Viewpoint.forward, out hit, distance, _ctx.HookableLayers);
        _ctx.GrappleDistance = didHit ? hit.distance : _ctx.RopeRemaining;
        _ctx.GrappleTarget = _ctx.Viewpoint.position + _ctx.Viewpoint.forward * distance;
    }

    public override void CheckSwitchState()
    {
        if (_ctx.IsPrimaryPressed)
        {
            SwitchState(_factory.Fired());
        }
    }

    #if UNITY_EDITOR
    public override void GizmosState()
    {
        float distance = RopeLengthWithCameraOffset();
        if (Physics.Raycast(_ctx.Viewpoint.position, _ctx.Viewpoint.forward, out gizmosHit, distance, _ctx.HookableLayers))
        {
            Gizmos.color = Color.green;
            Vector3 direction = hit.point - _ctx.ExitPoint.position;
            Gizmos.DrawRay(_ctx.ExitPoint.position, direction);
            Gizmos.DrawSphere(hit.point, 0.125f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_ctx.ExitPoint.position, _ctx.Viewpoint.forward * distance);
        }
    }
    #endif

    private float RopeLengthWithCameraOffset()
    {
        Vector3 nearestPointOnLine = NearestExitPointOnAimVector();
        return _ctx.RopeRemaining + Vector3.Distance(_ctx.Viewpoint.position, nearestPointOnLine);
    }
}