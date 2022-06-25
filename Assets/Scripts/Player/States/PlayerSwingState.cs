using UnityEngine;

public class PlayerSwingState : PlayerBaseState
{
    public PlayerSwingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        IsRootState = true;
        InitializeSubState();
    }

    float _turnSpeed = 180f;
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
        _fixedGrapplePoint = Ctx.GrapplePoint;
    }

    public override void UpdateState()
    {
        // Up Vector
        _bodyToGrapple = _fixedGrapplePoint - Ctx.Body.position;

        // Forward Vector (Exclude "Up Vector" from rigidbody velocity)
        _velocityDirection = Ctx.PlayerRigidbody.velocity - Vector3.Project(Ctx.PlayerRigidbody.velocity, _bodyToGrapple);
        
        // Target Rotation
        targetRotation = Quaternion.LookRotation(_velocityDirection, _bodyToGrapple);
        
        // Smooth Rotation
        Ctx.Body.rotation = Quaternion.RotateTowards(Ctx.Body.rotation, targetRotation, _turnSpeed * Time.deltaTime);
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(Ctx.IsSwingingHash, false);
    }

    public override void CheckSwitchState()
    {
        if (Ctx.IsGrounded)
            SwitchState(Factory.GetState<PlayerGroundState>());
        else if (!Ctx.IsGrappled)
            SwitchState(Factory.GetState<PlayerFallState>());
    }
}
