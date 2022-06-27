using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        IsRootState = true;
        InitializeSubState(); 
    }
    
    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        Ctx.IsJumpPressed = false; // Prevent hold-down jump spam
        // Start animation
        //Ctx.Animator.SetBool(Ctx.IsJumpingHash, true);
            
        // Cancel upwards velocity, relative to player's orientation
        Ctx.PlayerBody.velocity -= Vector3.Project(Ctx.PlayerBody.velocity, Ctx.transform.up);
            
        // Add upwards force, relative to player's orientation
        Ctx.PlayerBody.AddForce(Ctx.transform.up * Ctx.InitialJumpVelocity, ForceMode.VelocityChange);
    }

    public override void ExitState()
    {
        // Cancel animation
        //Ctx.Animator.SetBool(Ctx.IsJumpingHash, false);
    }

    public override void CheckSwitchState()
    {
        if (Ctx.VerticalVelocity < -Ctx.FallThresholdVelocity)
        {
            if (Ctx.IsGrappled)
                SwitchState(Factory.GetState<PlayerSwingState>());
            else
                SwitchState(Factory.GetState<PlayerFallState>());
        }
    }
}
