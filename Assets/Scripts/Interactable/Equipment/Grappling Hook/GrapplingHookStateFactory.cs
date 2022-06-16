using System.Collections.Generic;
using System;

public class GrapplingHookStateFactory
{
    GrapplingHookStateMachine _context;
    Dictionary<Type, GrapplingHookBaseState> _states = new Dictionary<Type, GrapplingHookBaseState>();

    public GrapplingHookStateFactory(GrapplingHookStateMachine currentContext)
    {
        _context = currentContext;
        _states[typeof(GrapplingHookIdleState)] = new GrapplingHookIdleState(_context, this);
        _states[typeof(GrapplingHookFiredState)] = new GrapplingHookFiredState(_context, this);
        _states[typeof(GrapplingHookGrappledState)] = new GrapplingHookGrappledState(_context, this);
    }

    public GrapplingHookBaseState GetState<T>() where T : GrapplingHookBaseState
    {
        // State exist in the dictionary
        if (_states.ContainsKey(typeof(T)))
            return _states[typeof(T)];
        // Forgot to create state in dictionary, so make it!
        else
        {
            // Create state
            T newState = (T)Activator.CreateInstance(typeof(T), _context, this);
            // Cache state
            _states[typeof(T)] = newState;
            return newState;
        }
    }
}