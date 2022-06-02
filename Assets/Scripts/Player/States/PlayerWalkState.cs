using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        InitializeSubState(); 
    }

    Vector3 _targetVel = Vector3.zero;

    public override void EnterState()
    {
        Debug.Log("Enter Walk SubState");
    }

    public override void FixedUpdateState()
    {
        float velDot = Vector3.Dot(_ctx.MoveDirection, _targetVel);

        // Increases acceleration based on target velocity angle difference to the current velocity
        float accel = _ctx.Acceleration * _ctx.AccelerationDotFactor.Evaluate(velDot);

        // Move previous target velocity to current target velocity
        _targetVel = Vector3.MoveTowards(_targetVel, _ctx.MoveDirection * _ctx.WalkSpeed, accel * Time.fixedDeltaTime);
        
        // Rigidbody's current velocity (excluding upwards axis)
        Vector3 currVel = _ctx.PlayerBody.velocity - Vector3.Project(_ctx.PlayerBody.velocity, _ctx.transform.up);
        
        // Acceleration needed to achieve target velocity from current velocity
        Vector3 targetAcceleration = (_targetVel - currVel) / Time.fixedDeltaTime;
        targetAcceleration = Vector3.ClampMagnitude(targetAcceleration, _ctx.MaxAcceleration);
        
        _ctx.PlayerBody.AddForce(targetAcceleration, ForceMode.Acceleration);
    }

    public override void CheckSwitchState()
    {
        if (!_ctx.IsMovementPressed)
            SwitchState(_factory.GetState<PlayerIdleState>());
        else if (_ctx.IsMovementPressed && _ctx.IsSprintPressed)
            SwitchState(_factory.GetState<PlayerSprintState>());
    }
}
