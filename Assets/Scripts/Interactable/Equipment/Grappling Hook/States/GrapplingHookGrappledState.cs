using UnityEngine;

public class GrapplingHookGrappledState : GrapplingHookBaseState
{
    public GrapplingHookGrappledState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}

    SpringJoint _springJoint;
    float _springStrength = 4.5f;
    float _damperAmount = 7f;

    public override void EnterState()
    {
        _springJoint = _ctx.Player.AddComponent<SpringJoint>();
        _springJoint.autoConfigureConnectedAnchor = false;
        _springJoint.connectedAnchor = _ctx.GrappleTarget;
        _springJoint.spring = _springStrength;
        _springJoint.damper = _damperAmount;

        _ctx.RopeRenderer.positionCount = 2;
    }

    public override void UpdateState()
    {
        _ctx.RopeRenderer.SetPosition(0, _ctx.ExitPoint);
        _ctx.RopeRenderer.SetPosition(1, _ctx.GrappleTarget);
    }

    public override void ExitState()
    {
        GrapplingHookStateMachine.Destroy(_springJoint);
        _ctx.RopeRenderer.positionCount = 0;
        RetractHook();
    }

    public override void CheckSwitchState()
    {
        if (!_ctx.IsPrimaryPressed)
        {
            SwitchState(_factory.GetState<GrapplingHookIdleState>());
        }
    }
}