using UnityEngine;

public abstract class GrapplingHookBaseState
{
    protected GrapplingHookStateMachine _ctx;
    protected GrapplingHookStateFactory _factory;

    public GrapplingHookBaseState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    {
        _ctx = currentContext;
        _factory = grapplingHookStateFactory;
    }

    /// <summary>
    /// Called upon entering current state.
    /// </summary>
    public virtual void EnterState() {}

    /// <summary>
    /// Called once per frame.
    /// </summary>
    public virtual void UpdateState() {}

    /// <summary>
    /// Called once per frame.
    /// </summary>
    public virtual void FixedUpdateState() {}
    
    /// <summary>
    /// Called upon exiting current state.
    /// </summary>
    public virtual void ExitState() {}

    /// <summary>
    /// Holds logic that determines when to switch frames. Called once per frame.
    /// </summary>
    public abstract void CheckSwitchState();
    
    #if UNITY_EDITOR
    /// <summary>
    /// Called inside OnDrawGizmos.
    /// </summary>
    public virtual void GizmosState() {}
    #endif

    /// <summary>
    /// Called upon exiting current state.
    /// </summary>
    protected void SwitchState(GrapplingHookBaseState newState) 
    {
        ExitState();
        newState.EnterState();
        _ctx.CurrentState = newState;
    }

    /// <summary>
    /// Resets the hook's transform.
    /// </summary>
    protected void RetractHook()
    {
        _ctx.Hook.SetParent(_ctx.HookParent, false);
        _ctx.Hook.localPosition = Vector3.zero;
        _ctx.Hook.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Resets the hook's transform.
    /// </summary>
    protected void ReleaseHook()
    {
        _ctx.Hook.parent = null;
    }

    /// <summary>
    /// 1. Shoots a raycast from the player's viewpoint forward until out of rope.
    /// <br/>
    /// 2. Calculates the new grapple target and sets the grapple distance.
    /// <br/>
    /// Returns true if a collision occured, otherwise false.
    /// </summary>
    protected bool CalculateGrappleTarget(out RaycastHit hit)
    {
        Vector3 startPos = NearestExitPointOnAimVector();
        if (Physics.Raycast(startPos, _ctx.Viewpoint.forward, out hit, _ctx.RopeRemaining, _ctx.HookableLayers))
        {
            _ctx.GrappleDistance = hit.distance;
            _ctx.GrappleTarget = hit.point;
            return true;
        }
        else
        {
            _ctx.GrappleDistance = _ctx.RopeRemaining;
            _ctx.GrappleTarget = startPos + _ctx.Viewpoint.forward * _ctx.RopeRemaining;
            return false;
        }
    }

    /// <summary>
    /// Returns the grappling hook's exit point projected on the aim vector.
    /// </summary>
    private Vector3 NearestExitPointOnAimVector()
    {
        return FindNearestPointOnLine(_ctx.Viewpoint.position, _ctx.Viewpoint.forward, _ctx.ExitPoint);
    }

    /// <summary>
    /// Returns the nearest point on a line for a target point.
    /// </summary>
    private Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 direction, Vector3 point)
    {
        direction.Normalize();
        Vector3 lhs = point - origin;

        float dotP = Vector3.Dot(lhs, direction);
        return origin + direction * dotP;
    }
}