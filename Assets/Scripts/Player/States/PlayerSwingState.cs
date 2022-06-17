using UnityEngine;

public class PlayerSwingState : PlayerBaseState
{
    public PlayerSwingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        IsRootState = true;
        InitializeSubState();
    }
    
    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        Ctx.HasLandedFromSwinging = false;
    }

    public override void CheckSwitchState()
    {
        if (Ctx.IsGrounded)
            SwitchState(Factory.GetState<PlayerGroundState>());
        else if (!Ctx.IsGrappled)
            SwitchState(Factory.GetState<PlayerFallState>());
    }
}
