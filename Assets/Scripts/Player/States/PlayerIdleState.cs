using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        InitializeSubState();
    }

    public override void EnterState()
    {
        Debug.Log("Enter Idle SubState");
        _ctx.MoveSpeed = 0f;
        // _ctx.Animator.SetBool(_ctx.IsMovingHash, false);
    }

    public override void CheckSwitchState()
    {
        if (_ctx.IsMovementPressed && _ctx.IsSprintPressed)
            SwitchState(_factory.GetState<PlayerSprintState>());
        else if (_ctx.IsMovementPressed)
            SwitchState(_factory.GetState<PlayerWalkState>());
    }
}
