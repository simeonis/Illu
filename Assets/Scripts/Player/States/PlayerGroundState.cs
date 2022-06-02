using UnityEngine;

public class PlayerGroundState : PlayerBaseState
{
    public PlayerGroundState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        _isRootState = true;
        InitializeSubState();
    }

    public override void EnterState()
    {
        Debug.Log("Enter Ground State");
        _ctx.CoyoteTimeCounter = _ctx.CoyoteTime;
    }

    public override void InitializeSubState()
    {
        if (!_ctx.IsMovementPressed && !_ctx.IsSprintPressed)
            SetSubState(_factory.GetState<PlayerIdleState>());
        else if(_ctx.IsMovementPressed && !_ctx.IsSprintPressed)
            SetSubState(_factory.GetState<PlayerWalkState>());
        else
            SetSubState(_factory.GetState<PlayerSprintState>());
    }

    public override void CheckSwitchState()
    {
        if (_ctx.IsJumpPressed)
            SwitchState(_factory.GetState<PlayerJumpState>());
        else if (!_ctx.IsGrounded)
            SwitchState(_factory.GetState<PlayerFallState>());
    }
}