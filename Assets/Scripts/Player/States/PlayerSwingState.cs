using UnityEngine;

public class PlayerSwingState : PlayerBaseState
{
    public PlayerSwingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        _isRootState = true;
        InitializeSubState();
    }
    
    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        _ctx.Drag = 0.25f;
    }

    public override void ExitState()
    {
        _ctx.Drag = _ctx.DefaultDrag;
    }


    public override void CheckSwitchState()
    {
        if (_ctx.IsGrounded)
            SwitchState(_factory.GetState<PlayerGroundState>());
        else if (!_ctx.IsGrappled)
            SwitchState(_factory.GetState<PlayerFallState>());
    }
}
