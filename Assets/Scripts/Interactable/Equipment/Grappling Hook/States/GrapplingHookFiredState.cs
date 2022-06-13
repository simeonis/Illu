using UnityEngine;
using System.Collections;

public class GrapplingHookFiredState : GrapplingHookBaseState
{
    public GrapplingHookFiredState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}

    IEnumerator _ropeCoroutine;
    RaycastHit _recalcHit;
    float _initialRopeRemaining;
    bool _grappled, _outOfRope;

    public override void EnterState()
    {
        ReleaseHook();
        
        _initialRopeRemaining = Ctx.RopeRemaining;
        _grappled = _outOfRope = false;

        // Animate rope
        // TO-DO: Fix rope animation not properly sync with projectile movement
        if (_ropeCoroutine != null) Ctx.StopCoroutine(_ropeCoroutine);
        Ctx.StartCoroutine(_ropeCoroutine = AnimateRope());
    }

    public override void FixedUpdateState()
    {
        // TO-DO: Check for unexpected collisions

        // Move projectile to target position
        float step = Ctx.ProjectileSpeed * Time.deltaTime;
        Ctx.HookPosition = Vector3.MoveTowards(Ctx.HookPosition, Ctx.GrapplePoint, step);

        // Update rope remaining
        float currentDistance = Vector3.Distance(Ctx.HookPosition, Ctx.GrapplePoint);
        Ctx.RopeRemaining = _initialRopeRemaining - (Ctx.GrappleDistance - currentDistance);
        
        // Check if out of rope
        if (Ctx.RopeRemaining <= 0f)
        {
            _outOfRope = true;
            return;
        }

        // Arrived at target
        if (currentDistance < 0.001f)
        {
            // Check if target is still here
            if (CheckForCollision())
                _grappled = true;
            // Otherwise, recalculate target
            else
                CalculateGrappleTarget(out _);
        }
    }

    bool CheckForCollision() => Physics.CheckSphere(Ctx.HookPosition, 0.25f, Ctx.HookableLayers);

    public override void ExitState()
    {
        if (_grappled)
        {
            // Calculate the final grapple position distance
            Ctx.GrappleDistance = Vector3.Distance(Ctx.ExitPoint, Ctx.GrapplePoint);
            // Update rope remaining
            Ctx.RopeRemaining = _initialRopeRemaining - Ctx.GrappleDistance;
        }

        // Stop rope animation
        Ctx.StopCoroutine(_ropeCoroutine);
    }

    public override void CheckSwitchState()
    {
        if (_grappled)
            SwitchState(Factory.GetState<GrapplingHookGrappledState>());
        else if (!Ctx.IsPrimaryPressed || _outOfRope)
            SwitchState(Factory.GetState<GrapplingHookIdleState>());
    }

    private IEnumerator AnimateRope()
    {
        // Set the array size of the line renderer
        Ctx.RopeRenderer.positionCount = Ctx.Resolution;

        // Calculate the percentage rate increase based on projectile speed and
        // the distance from the projectile exit position to the target position
        float length = Vector3.Distance(Ctx.ExitPoint, Ctx.GrapplePoint);
        float speedOverLength = Ctx.ProjectileSpeed / length;

        float percent = 0f;
        while (percent <= 1f)
        {
            percent += Time.deltaTime * speedOverLength;
            SetRopePoints(percent, speedOverLength);
            yield return null;
        }

        SetRopePoints(1f, speedOverLength);
    }

    private void SetRopePoints(float percent, float speedOverLength)
    {
        // How far along from projectile exit position to target position based on percentage
        Vector3 ropeEnd = Vector3.Lerp(Ctx.ExitPoint, Ctx.GrapplePoint, percent);

        // Upwards vector from projectile exit position to target position
        Vector3 gunToGrapple = (Ctx.GrapplePoint - Ctx.ExitPoint);
        Vector3 up = Quaternion.LookRotation(gunToGrapple.normalized) * Vector3.up;

        // Percentage (1 -> 0) based on projectile speed and
        // the distance from the projectile exit position to target position
        float inversePercent = (1f - percent) / speedOverLength;

        for (var i = 0; i < Ctx.Resolution; i++)
        {
            // Calculate y-position based on resolution percentage
            float resolutionPercent = (float)i / Ctx.Resolution;
            float amplitude = Mathf.Sin(inversePercent * Ctx.WobbleCount * Mathf.PI) * ((1f - resolutionPercent) * Ctx.WaveHeight) * (percent * 2f);
            float deltaY = Mathf.Sin(Ctx.WaveCount * resolutionPercent * 2 * Mathf.PI * inversePercent) * amplitude;

            // Apply calculations to the upwards vector of the projectile trajectory
            Vector3 offsetY = up * deltaY;

            // Move the corresponding points according to their new y-offset
            Vector3 pos = Vector3.Lerp(Ctx.ExitPoint, ropeEnd, resolutionPercent) + offsetY;

            Ctx.RopeRenderer.SetPosition(i, pos);
        }
    }
}