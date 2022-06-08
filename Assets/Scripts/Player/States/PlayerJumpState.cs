using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        _isRootState = true;
        InitializeSubState(); 
    }
    
    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        _ctx.IsJumpPressed = false; // Prevent hold-down jump spam
        // Start animation
        //_ctx.Animator.SetBool(_ctx.IsJumpingHash, true);
            
        // Cancel upwards velocity, relative to player's orientation
        _ctx.PlayerBody.velocity -= Vector3.Project(_ctx.PlayerBody.velocity, _ctx.transform.up);
            
        // Add upwards force, relative to player's orientation
        _ctx.PlayerBody.AddForce(_ctx.transform.up * _ctx.InitialJumpVelocity, ForceMode.VelocityChange);
    }

    public override void ExitState()
    {
        // Cancel animation
        //_ctx.Animator.SetBool(_ctx.IsJumpingHash, false);
    }

    public override void CheckSwitchState()
    {
        if (_ctx.VerticalVelocity < -_ctx.FallThresholdVelocity)
        {
            if (_ctx.IsGrappled)
                SwitchState(_factory.GetState<PlayerSwingState>());
            else
                SwitchState(_factory.GetState<PlayerFallState>());
        }
    }
}
