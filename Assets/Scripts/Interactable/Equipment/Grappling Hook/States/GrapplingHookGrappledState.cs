using UnityEngine;

public class GrapplingHookGrappledState : GrapplingHookBaseState
{
    public GrapplingHookGrappledState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}

    public override void EnterState()
    {
        _ctx.RopeRenderer.positionCount = 2;
    }

    public override void UpdateState()
    {
        _ctx.RopeRenderer.SetPosition(0, _ctx.ExitPoint);
        _ctx.RopeRenderer.SetPosition(1, _ctx.GrappleTarget);
    }

    public override void ExitState()
    {
        _ctx.RopeRenderer.positionCount = 0;
        RetractHook();
    }

    public override void CheckSwitchState()
    {
        if (!_ctx.IsPrimaryPressed)
        {
            SwitchState(_factory.Idle());
        }
    }
}