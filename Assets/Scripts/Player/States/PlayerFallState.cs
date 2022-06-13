using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        IsRootState = true;
        InitializeSubState();
    }
    
    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        Debug.Log("Enter Fall State");
        // Start animation
        // Ctx.Animator.SetBool(Ctx.IsFallingHash, true);
    }

    public override void ExitState()
    {
        // Start animation
        // Ctx.Animator.SetBool(Ctx.IsFallingHash, false);
    }

    public override void CheckSwitchState()
    {
        if (Ctx.IsGrounded)
            SwitchState(Factory.GetState<PlayerGroundState>());
        else if (Ctx.IsGrappled)
            SwitchState(Factory.GetState<PlayerSwingState>());
        else if ((Ctx.CoyoteTimeCounter > 0f) && Ctx.IsJumpPressed)
            SwitchState(Factory.GetState<PlayerJumpState>());
    }
}
