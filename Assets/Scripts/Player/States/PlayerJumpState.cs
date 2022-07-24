using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        IsRootState = true;
        InitializeSubState(); 
    }

    float _verticalVelocity;
    
    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        Ctx.IsJumpPressed = false; // Prevent hold-down jump spam
            
        // Cancel vertical velocity, relative to player's orientation
        Ctx.Rigidbody.velocity -= Vector3.Project(Ctx.Rigidbody.velocity, Ctx.transform.up);
            
        // Add upwards force, relative to player's orientation
        Ctx.Rigidbody.AddForce(Ctx.transform.up * Ctx.InitialJumpVelocity, ForceMode.VelocityChange);
    }

    public override void FixedUpdateState()
    {
        _verticalVelocity = Vector3.Dot(Ctx.Rigidbody.velocity, Ctx.transform.up);
    }

    public override void CheckSwitchState()
    {
        if (_verticalVelocity < -Ctx.FallThresholdVelocity)
        {
            if (Ctx.GrapplingHook.IsGrappled)
                SwitchState(Factory.GetState<PlayerSwingState>());
            else
                SwitchState(Factory.GetState<PlayerFallState>());
        }
    }
}
