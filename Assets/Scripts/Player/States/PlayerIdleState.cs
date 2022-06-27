using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        InitializeSubState();
    }

    public override void EnterState()
    {
        Ctx.MoveSpeed = 0f;
        Ctx.Animator.SetBool(Ctx.IsWalkingHash, false);
        Ctx.Animator.SetBool(Ctx.IsSprintingHash, false);
    }

    public override void CheckSwitchState()
    {
        if (Ctx.IsMovementPressed && Ctx.IsSprintPressed)
            SwitchState(Factory.GetState<PlayerSprintState>());
        else if (Ctx.IsMovementPressed)
            SwitchState(Factory.GetState<PlayerWalkState>());
    }
}
