using System;
using UnityEngine;

public class GrapplingHookFiredState : GrapplingHookBaseState
{
    public GrapplingHookFiredState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}

    RaycastHit recalcHit;
    float _ropeRemaining;
    bool _grappled;
    bool _outOfRope;

    public override void EnterState()
    {
        _ropeRemaining = _ctx.RopeRemaining;
        _grappled = false;
        _outOfRope = false;
        ReleaseHook();

        // TEMPORARY
        _ctx.RopeRenderer.positionCount = 2;
    }

    public override void UpdateState()
    {
        // TEMPORARY
        _ctx.RopeRenderer.SetPosition(0, _ctx.ExitPoint);
        _ctx.RopeRenderer.SetPosition(1, _ctx.HookPosition);
        // TO-DO: ANIMATE ROPE
    }

    public override void FixedUpdateState()
    {
        // TO-DO: Check for unexpected collisions

        // Move projectile to target position
        float step = _ctx.ProjectileSpeed * Time.deltaTime;
        _ctx.HookPosition = Vector3.MoveTowards(_ctx.HookPosition, _ctx.GrappleTarget, step);

        // Update rope remaining
        float currentDistance = Vector3.Distance(_ctx.HookPosition, _ctx.GrappleTarget);
        _ctx.RopeRemaining = _ropeRemaining - (_ctx.GrappleDistance - currentDistance);
        
        // Check if out of rope
        if (_ctx.RopeRemaining <= 0f)
        {
            _outOfRope = true;
            return;
        }

        // Arrived at target
        if (currentDistance < 0.001f)
        {
            // Check if target is still here
            if (CheckForCollision())
            {
                _grappled = true;
            }
            // Otherwise, recalculate target
            else
            {
                CalculateGrappleTarget(out _);
            }
        }
    }

    bool CheckForCollision()
    {
        return Physics.CheckSphere(_ctx.HookPosition, 0.25f, _ctx.HookableLayers);
    }

    public override void CheckSwitchState()
    {
        if (_grappled)
            SwitchState(_factory.Grappled());
        else if (!_ctx.IsPrimaryPressed || _outOfRope)
            SwitchState(_factory.Idle());
    }

    #if UNITY_EDITOR
    public override void GizmosState()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_ctx.HookPosition, 0.25f);
    }
    #endif

    // private IEnumerator RopeAnimation(Vector3 targetPos)
    // {
    //     // Set the array size of the line renderer
    //     _ctx.RopeRenderer.positionCount = _ctx.Resolution;

    //     // Calculate the percentage rate increase based on projectile speed and
    //     // the distance from the projectile exit position to the target position
    //     float length = Vector3.Distance(targetPos, _ctx.ExitPoint.position);
    //     float speedOverLength = _ctx.ProjectileSpeed / length;

    //     // Prevents rope length from passing the maximum length
    //     // if (length > maxRopeLength) ShotFired();

    //     float percent = 0f;
    //     while (percent <= 1f)
    //     {
    //         percent += Time.deltaTime * speedOverLength;
    //         SetPoints(targetPos, percent, speedOverLength);
    //         yield return null;
    //     }

    //     SetPoints(targetPos, 1f, speedOverLength);

    //     // Once animation completed, perform necessary action
    //     // if (hit.collider) HookCollided(hit);
    //     // else ShotFired();
    // }

    // private void SetPoints(Vector3 targetPos, float percent, float speedOverLength)
    // {
    //     // How far along from projectile exit position to target position based on percentage
    //     Vector3 ropeEnd = Vector3.Lerp(_ctx.ExitPoint.position, targetPos, percent);

    //     // Upwards vector from projectile exit position to target position
    //     Vector3 gunToGrapple = (targetPos - _ctx.ExitPoint.position);
    //     var up = Quaternion.LookRotation(gunToGrapple.normalized) * Vector3.up;

    //     // Percentage (1 -> 0) based on projectile speed and
    //     // the distance from the projectile exit position to target position
    //     float inversePercent = (1f - percent) / speedOverLength;

    //     _ctx.RopeRenderer.SetPosition(0, _ctx.ExitPoint.position);

    //     for (var i = 0; i < _ctx.Resolution; i++)
    //     {
    //         // Calculate y-position based on resolution percentage
    //         float resolutionPercent = (float)i / _ctx.Resolution;
    //         float amplitude = Mathf.Sin(inversePercent * _ctx.WobbleCount * Mathf.PI) * ((1f - resolutionPercent) * _ctx.WaveHeight) * (percent * 2f);
    //         float deltaY = Mathf.Sin(_ctx.WaveCount * resolutionPercent * 2 * Mathf.PI * inversePercent) * amplitude;

    //         // Apply calculations to the upwards vector of the projectile trajectory
    //         Vector3 offsetY = up * deltaY;

    //         // Move the corresponding points according to their new y-offset
    //         Vector3 pos = Vector3.Lerp(_ctx.ExitPoint.position, ropeEnd, resolutionPercent) + offsetY;

    //         // Set the position of the projectile and add it to the line renderer
    //         _ctx.Hook.position = pos;
    //         _ctx.RopeRenderer.SetPosition(i, pos);
    //     }
    // }
}