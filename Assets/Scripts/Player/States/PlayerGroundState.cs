using UnityEngine;

public class PlayerGroundState : PlayerBaseState
{
    public PlayerGroundState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        _isRootState = true;
        InitializeSubState();
    }

    Vector3 _targetVel = Vector3.zero;
    
    public override void InitializeSubState() => InitializeLocomotion();

    public override void EnterState()
    {
        Debug.Log("Enter Ground State");
        _ctx.CoyoteTimeCounter = _ctx.CoyoteTime;
    }

    public override void CheckSwitchState()
    {
        if (_ctx.IsJumpPressed)
            SwitchState(_factory.GetState<PlayerJumpState>());
        else if (!_ctx.IsGrounded)
        {
            if (_ctx.IsGrappled)
                SwitchState(_factory.GetState<PlayerSwingState>());
            else
                SwitchState(_factory.GetState<PlayerFallState>());
        }
    }
}