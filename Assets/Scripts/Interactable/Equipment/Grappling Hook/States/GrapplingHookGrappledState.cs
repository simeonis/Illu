using UnityEngine;

public class GrapplingHookGrappledState : GrapplingHookBaseState
{
    public GrapplingHookGrappledState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}

    private float currentRopeLength;

    public override void EnterState()
    {
        currentRopeLength = Vector3.Distance(NearestExitPointOnAimVector(), _ctx.GrappleTarget);
        _ctx.RopeRenderer.positionCount = 2;
    }

    public override void UpdateState()
    {
        _ctx.RopeRenderer.SetPosition(0, _ctx.ExitPoint.position);
        _ctx.RopeRenderer.SetPosition(1, _ctx.GrappleTarget);
    }

    public override void ExitState()
    {
        _ctx.RopeRenderer.positionCount = 0;
        AttachGrapple();
    }

    public override void CheckSwitchState()
    {
        if (!_ctx.IsPrimaryPressed)
        {
            SwitchState(_factory.Idle());
        }
    }
}