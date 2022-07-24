using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        IsRootState = true;
        InitializeSubState();
    }

    Vector3 _velocityDir;
    Quaternion _targetRot;
    float _rotationSpeed = 180f;
    
    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        Debug.Log("Enter Fall State");
        // Start animation
        // Ctx.Animator.SetBool(Ctx.IsFallingHash, true);
    }

    public override void UpdateState()
    {   
        if (Ctx.HasLandedFromSwinging || Ctx.GrapplingHook.IsFired) return;
        // Rotate player to be upright and facing forward towards current velocity
        
        // Velocity direction (excluding "Up Vector")
        _velocityDir = Vector3.Scale(Ctx.Rigidbody.velocity, Ctx.transform.forward + Ctx.transform.right);
        _targetRot = Quaternion.LookRotation(_velocityDir, Ctx.transform.up);
        
        // Smooth rotation
        Ctx.Orientation.rotation = Quaternion.RotateTowards(Ctx.Orientation.rotation, _targetRot, _rotationSpeed * Time.deltaTime);
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
        else if (Ctx.GrapplingHook.IsGrappled)
            SwitchState(Factory.GetState<PlayerSwingState>());
        else if ((Ctx.CoyoteTimeCounter > 0f) && Ctx.IsJumpPressed)
            SwitchState(Factory.GetState<PlayerJumpState>());
    }
}
