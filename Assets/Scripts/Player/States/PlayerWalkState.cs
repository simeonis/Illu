using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
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
            Ctx.MoveSpeed = Ctx.WalkSpeed;
        }

        Ctx.Animator.SetBool(Ctx.IsWalkingHash, true);
        Ctx.Animator.SetBool(Ctx.IsSprintingHash, false);
    }

    public override void CheckSwitchState()
    {
        if (!Ctx.IsMovementPressed)
            SwitchState(Factory.GetState<PlayerIdleState>());
        else if (Ctx.IsMovementPressed && Ctx.IsSprintPressed)
            SwitchState(Factory.GetState<PlayerSprintState>());
    }
}
