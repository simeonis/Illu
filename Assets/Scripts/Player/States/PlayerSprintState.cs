using UnityEngine;

public class PlayerSprintState : PlayerBaseState
{
    public PlayerSprintState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        InitializeSubState();
    }

    public override void EnterState()
    {
        Debug.Log("Enter Sprint SubState");
    }

    public override void FixedUpdateState()
    {
        _ctx.PlayerBody.AddForce(_ctx.MoveDirection * _ctx.SprintSpeed * 10f, ForceMode.Acceleration);
    }

    public override void CheckSwitchState()
    {
        if (!_ctx.IsMovementPressed)
            SwitchState(_factory.GetState<PlayerIdleState>());
        else if (_ctx.IsMovementPressed && !_ctx.IsSprintPressed)
            SwitchState(_factory.GetState<PlayerWalkState>());
    }
}
