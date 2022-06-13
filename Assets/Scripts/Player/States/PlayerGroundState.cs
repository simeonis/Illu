using UnityEngine;

public class PlayerGroundState : PlayerBaseState
{
    public PlayerGroundState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        IsRootState = true;
        InitializeSubState();
    }

    Vector3 _targetVel = Vector3.zero;
    
    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        Debug.Log("Enter Ground State");
        Ctx.CoyoteTimeCounter = Ctx.CoyoteTime;
        Ctx.Resistance = Ctx.GroundResistance;
    }

    public override void ExitState()
    {
        Ctx.Resistance = Ctx.AirResistance;
    }

    public override void CheckSwitchState()
    {
        if (Ctx.IsJumpPressed)
            SwitchState(Factory.GetState<PlayerJumpState>());
        else if (!Ctx.IsGrounded)
        {
            if (Ctx.IsGrappled)
                SwitchState(Factory.GetState<PlayerSwingState>());
            else
                SwitchState(Factory.GetState<PlayerFallState>());
        }
    }
}