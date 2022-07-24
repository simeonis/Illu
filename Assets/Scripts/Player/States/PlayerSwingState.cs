using UnityEngine;

public class PlayerSwingState : PlayerBaseState
{
    public PlayerSwingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        IsRootState = true;
        InitializeSubState();
    }

    float _turnSpeed = 150f;
    Vector3 _fixedGrapplePoint = Vector3.zero;
    Vector3 _bodyToGrapple;
    Vector3 _velocityDirection;
    Quaternion targetRotation;
    
    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        Ctx.HasLandedFromSwinging = false;

        // Animation
        Ctx.Animator.SetBool(Ctx.IsSwingingHash, true);

        // Store grapple point to avoid non-smooth rotation when exiting SwingState
        _fixedGrapplePoint = Ctx.GrapplingHook.GrapplePoint;
    }

    public override void UpdateState()
    {
        // Up Vector
        _bodyToGrapple = _fixedGrapplePoint - Ctx.Orientation.position;

        // Forward Vector (Exclude "Up Vector" from rigidbody velocity)
        _velocityDirection = Ctx.Rigidbody.velocity - Vector3.Project(Ctx.Rigidbody.velocity, _bodyToGrapple);
        
        // Target Rotation
        targetRotation = Quaternion.LookRotation(_velocityDirection, _bodyToGrapple);
        
        // Smooth Rotation
        Ctx.Orientation.rotation = Quaternion.RotateTowards(Ctx.Orientation.rotation, targetRotation, _turnSpeed * Time.deltaTime);
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(Ctx.IsSwingingHash, false);
    }

    public override void CheckSwitchState()
    {
        if (Ctx.IsGrounded)
            SwitchState(Factory.GetState<PlayerGroundState>());
        else if (!Ctx.GrapplingHook.IsGrappled)
            SwitchState(Factory.GetState<PlayerFallState>());
    }
}
