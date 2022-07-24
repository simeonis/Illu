using UnityEngine;

public class PlayerGroundState : PlayerBaseState
{
    public PlayerGroundState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
        InitializeSubState();
    }

    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        Debug.Log("Enter Ground State");
        Ctx.Animator.SetBool(Ctx.IsGroundedHash, true);
        
        Ctx.HasLandedFromSwinging = true;
        Ctx.CoyoteTimeCounter = Ctx.CoyoteTime;
        Ctx.Orientation.rotation = Quaternion.Euler(Vector3.Scale(Ctx.Orientation.eulerAngles, Ctx.transform.up));
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(Ctx.IsGroundedHash, false);
    }

    public override void CheckSwitchState()
    {
        if (Ctx.IsJumpPressed)
            SwitchState(Factory.GetState<PlayerJumpState>());
        else if (!Ctx.IsGrounded)
            SwitchState(Factory.GetState<PlayerFallState>());
    }
}