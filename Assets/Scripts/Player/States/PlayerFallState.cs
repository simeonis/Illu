using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        _isRootState = true;
        InitializeSubState();
    }

    public override void EnterState()
    {
        // Start animation
        // _ctx.Animator.SetBool(_ctx.IsFallingHash, true);
    }

    public override void ExitState()
    {
        // Start animation
        // _ctx.Animator.SetBool(_ctx.IsFallingHash, false);
    }

    public override void CheckSwitchState()
    {
        if (_ctx.IsGrounded)
            SwitchState(_factory.GetState<PlayerGroundState>());
        else if ((_ctx.CoyoteTimeCounter > 0f) && _ctx.IsJumpPressed)
            SwitchState(_factory.GetState<PlayerJumpState>());
    }
}
