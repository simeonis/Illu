using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        InitializeSubState(); 
    }

    public override void EnterState()
    {
        if (_currentSuperState is PlayerSwingState)
        {
            _ctx.MoveSpeed = _ctx.SwingSpeed;
        }
        else
        {
            _ctx.MoveSpeed = _ctx.WalkSpeed;
        }
    }

    public override void CheckSwitchState()
    {
        if (!_ctx.IsMovementPressed)
            SwitchState(_factory.GetState<PlayerIdleState>());
        else if (_ctx.IsMovementPressed && _ctx.IsSprintPressed)
            SwitchState(_factory.GetState<PlayerSprintState>());
    }
}
