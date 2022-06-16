using UnityEngine;

public abstract class GrapplingHookBaseState
{
    private GrapplingHookStateMachine _ctx;
    private GrapplingHookStateFactory _factory;

    protected GrapplingHookStateMachine Ctx { get { return _ctx; } }
    protected GrapplingHookStateFactory Factory { get { return _factory; } }

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
        _ctx.Hook.SetParent(_ctx.HookDefaultParent, false);
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
    /// 1. Shoots a raycast in "direction" starting at "origin".
    /// <br/>
    /// 2. Calculates the new grapple target and sets the grapple distance.
    /// <br/>
    /// Returns true if a collision occured, otherwise false.
    /// </summary>
    protected bool SimulateGrapple(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, _ctx.RopeRemaining, _ctx.HookableLayers))
        {
            _ctx.GrappleDistance = hit.distance;
            _ctx.GrapplePoint = hit.point;
            return true;
        }
        else
        {
            _ctx.GrappleDistance = _ctx.RopeRemaining;
            _ctx.GrapplePoint = origin + direction * _ctx.RopeRemaining;
            return false;
        }
    }
}