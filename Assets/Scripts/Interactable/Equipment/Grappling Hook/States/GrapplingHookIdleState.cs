using UnityEngine;

public class GrapplingHookIdleState : GrapplingHookBaseState
{
    public GrapplingHookIdleState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}
    
    RaycastHit hit, gizmosHit;
    float distance = 0f;

    public override void EnterState()
    {
        _ctx.IsPrimaryPressed = false; // Setter prevents value from being set to true
        _ctx.RopeRemaining = _ctx.MaxRopeLength;
        _ctx.RopeRenderer.positionCount = 0;
        RetractHook();
    }

    public override void UpdateState() => CalculateGrappleTarget(out _);

    public override void CheckSwitchState()
    {
        if (_ctx.IsPrimaryPressed)
        {
            SwitchState(_factory.GetState<GrapplingHookFiredState>());
        }
    }

    #if UNITY_EDITOR
    public override void GizmosState()
    {
        if (CalculateGrappleTarget(out gizmosHit))
        {
            Gizmos.color = Color.green;
            Vector3 direction = gizmosHit.point - _ctx.ExitPoint;
            Gizmos.DrawRay(_ctx.ExitPoint, direction);
            Gizmos.DrawSphere(gizmosHit.point, 0.125f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_ctx.ExitPoint, _ctx.Viewpoint.forward * distance);
        }
    }
    #endif
}