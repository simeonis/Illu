using System.Collections.Generic;

public class GrapplingHookStateFactory
{
    GrapplingHookStateMachine _context;
    Dictionary<string, GrapplingHookBaseState> _states = new Dictionary<string, GrapplingHookBaseState>();

    public GrapplingHookStateFactory(GrapplingHookStateMachine currentContext)
    {
        _context = currentContext;
        _states["idle"] = new GrapplingHookIdleState(_context, this);
        _states["fired"] = new GrapplingHookFiredState(_context, this);
        _states["grappled"] = new GrapplingHookGrappledState(_context, this);
    }

    public GrapplingHookBaseState Idle()
    {
        return _states["idle"];
    }

    public GrapplingHookBaseState Fired()
    {
        return _states["fired"];
    }

    public GrapplingHookBaseState Grappled()
    {
        return _states["grappled"];
    }
}