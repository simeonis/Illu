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

    protected void AttachGrapple()
    {
        _ctx.Hook.Disable();
        _ctx.HookTransform.SetParent(_ctx.DefaultHookParent);
        _ctx.HookTransform.localPosition = Vector3.zero;
        _ctx.HookTransform.localRotation = Quaternion.identity;
    }

    protected void DetatchGrapple()
    {
        _ctx.Hook.Enable();
        _ctx.HookTransform.SetParent(null);
    }

    /// <summary>
    /// Returns the grappling hook's exit point projected on the aim vector.
    /// </summary>
    protected Vector3 NearestExitPointOnAimVector()
    {
        return FindNearestPointOnLine(_ctx.Viewpoint.position, _ctx.Viewpoint.forward, _ctx.ExitPoint.position);
    }

    private Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 direction, Vector3 point)
    {
        direction.Normalize();
        Vector3 lhs = point - origin;

        float dotP = Vector3.Dot(lhs, direction);
        return origin + direction * dotP;
    }
}