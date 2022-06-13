using UnityEngine;

public class PlayerSprintState : PlayerBaseState
{
    public PlayerSprintState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        InitializeSubState();
    }

    public override void EnterState()
    {
        if (SuperState is PlayerSwingState)
        {
            Ctx.MoveSpeed = Ctx.SwingSpeed;
        }
        else
        {
            Ctx.MoveSpeed = Ctx.SprintSpeed;
        }

        Ctx.Animator.SetBool(Ctx.IsWalkingHash, true);
        Ctx.Animator.SetBool(Ctx.IsSprintingHash, true);
    }

    public override void CheckSwitchState()
    {
        if (!Ctx.IsMovementPressed)
            SwitchState(Factory.GetState<PlayerIdleState>());
        else if (Ctx.IsMovementPressed && !Ctx.IsSprintPressed)
            SwitchState(Factory.GetState<PlayerWalkState>());
    }
}
