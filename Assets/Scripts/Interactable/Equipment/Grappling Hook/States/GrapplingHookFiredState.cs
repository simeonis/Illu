using UnityEngine;

public class GrapplingHookFiredState : GrapplingHookBaseState
{
    public GrapplingHookFiredState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}

    Vector3 _previousHookPos;
    bool _grappled, _outOfRope;
    float _initialRopeRemaining;
    float _speedOverLength;
    float _percent;

    public override void EnterState()
    {
        ReleaseHook();
        
        // Initialize variables
        _previousHookPos = Ctx.HookPosition;
        _initialRopeRemaining = Ctx.RopeRemaining;
        _grappled = _outOfRope = false;
        _speedOverLength = _percent = 0f;

        // Set the array size of the line renderer
        Ctx.RopeRenderer.positionCount = Ctx.Resolution;
    }

    public override void UpdateState()
    {
        // Calculate every frame incase of unexpected collisions or move platforms
        _speedOverLength = Ctx.ProjectileSpeed / Ctx.GrappleDistance;

        if (_percent < 1.0f)
        {
            _percent += Time.deltaTime * _speedOverLength;
            SetRopePoints(_percent, _speedOverLength);
        }
    }

    public override void FixedUpdateState()
    {
        // Target reached
        if (Vector3.Distance(Ctx.HookPosition, Ctx.GrapplePoint) < 0.01f)
        {
            // Expected collision occured
            if (CheckForCollision())
            {
                _grappled = true;
                return;
            }
            // No collision occured, re-calculating...
            else
            {
                RecalculateGrappleTarget();
                _percent = 0f; // Reset rope animation percentage
            }
        }

        // TODO: Differentiate moving platform from normal collision
        // Check for unexpected collisions
        // else if (CheckForCollision()) {
        //     Debug.Log("Unexpected collision occured");
        //     Ctx.GrapplePoint = Ctx.HookPosition;
        //     Ctx.GrappleDistance = Ctx.RopeRemaining;
        //     _grappled = true;
        //     return;
        // }

        // Save hook position to calculate direction in case target moves before arrival
        _previousHookPos = Ctx.HookPosition;
        
        // Move projectile to target position
        Ctx.HookPosition = Vector3.MoveTowards(Ctx.HookPosition, Ctx.GrapplePoint, Ctx.ProjectileSpeed * Time.deltaTime);

        // Update rope remaining
        Ctx.RopeRemaining = _initialRopeRemaining - Vector3.Distance(Ctx.ExitPoint, Ctx.HookPosition);

        // Check if out of rope
        if (Ctx.RopeRemaining <= 0f)
            _outOfRope = true;
    }

    public override void ExitState()
    {
        if (_grappled)
        {
            // Calculate the final grapple position distance
            Ctx.GrappleDistance = Vector3.Distance(Ctx.ExitPoint, Ctx.GrapplePoint);
            // Update rope remaining
            Ctx.RopeRemaining = _initialRopeRemaining - Ctx.GrappleDistance;
        }
    }

    public override void CheckSwitchState()
    {
        if (_grappled)
            SwitchState(Factory.GetState<GrapplingHookGrappledState>());
        else if (!Ctx.IsPrimaryPressed || _outOfRope)
            SwitchState(Factory.GetState<GrapplingHookIdleState>());
    }

    // Helper Functions
    bool RecalculateGrappleTarget()
    {
        Vector3 direction = (Ctx.HookPosition - _previousHookPos).normalized;
        return SimulateGrapple(Ctx.HookPosition, direction);
    }

    bool CheckForCollision()
    {
        return Physics.CheckSphere(Ctx.HookPosition, 0.25f, Ctx.HookableLayers);
    }

    void SetRopePoints(float percent, float speedOverLength)
    {
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

            // Move the corresponding points according to their new y-offset
            Vector3 newPosition = Vector3.Lerp(Ctx.ExitPoint, Ctx.HookPosition, resolutionPercent) + (up * deltaY);

            Ctx.RopeRenderer.SetPosition(i, newPosition);
        }
    }
}