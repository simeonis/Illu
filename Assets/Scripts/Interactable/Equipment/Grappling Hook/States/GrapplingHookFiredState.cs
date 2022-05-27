using System.Collections;
using UnityEngine;

public class GrapplingHookFiredState : GrapplingHookBaseState
{
    public GrapplingHookFiredState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}

    bool _outOfRope, _collisionOccured;
    Vector3 _fakeProjectilePos;

    public override void EnterState()
    {
        DetatchGrapple();
        _fakeProjectilePos = NearestExitPointOnAimVector();
        _outOfRope = _collisionOccured = false;

        _ctx.Hook.AddListener(OnCollision);

        // TEMPORARY
        _ctx.RopeRenderer.positionCount = 2;
    }

    void OnCollision(Collision collision)
    {
        _collisionOccured = true;
        _ctx.HookTransform.position = _fakeProjectilePos;
        _ctx.GrappleTarget = _fakeProjectilePos;
    }

    public override void UpdateState()
    {
        // TEMPORARY
        _ctx.RopeRenderer.SetPosition(0, _ctx.ExitPoint.position);
        _ctx.RopeRenderer.SetPosition(1, _ctx.HookTransform.position);
        // TO-DO: ANIMATE ROPE
    }

    public override void FixedUpdateState()
    {
        if (_outOfRope) return;

        // Move projectile to target position
        float step = _ctx.ProjectileSpeed * Time.deltaTime;
        _fakeProjectilePos = Vector3.MoveTowards(_fakeProjectilePos, _ctx.GrappleTarget, step);
        _ctx.HookTransform.position = Vector3.MoveTowards(_ctx.HookTransform.position, _ctx.GrappleTarget, step);

        // Check if position of fake projectile and target is approximately equal
        if (!_collisionOccured && Vector3.Distance(_fakeProjectilePos, _ctx.GrappleTarget) < 0.001f)
            _outOfRope = true;
    }

    public override void ExitState()
    {
        if (!_ctx.IsPrimaryPressed || _outOfRope)
        {
            _ctx.RopeRenderer.positionCount = 0;
            AttachGrapple();
        }
    }

    public override void CheckSwitchState()
    {
        if (_collisionOccured)
            SwitchState(_factory.Grappled());
        else if (!_ctx.IsPrimaryPressed || _outOfRope)
            SwitchState(_factory.Idle());
    }

    #if UNITY_EDITOR
    public override void GizmosState()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_ctx.HookTransform.position, 0.125f);
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