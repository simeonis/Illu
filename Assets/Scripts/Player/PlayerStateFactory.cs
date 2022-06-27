using System;
using System.Collections.Generic;

public class PlayerStateFactory
{
    PlayerStateMachine _context;
    Dictionary<Type, PlayerBaseState> _states = new Dictionary<Type, PlayerBaseState>();

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        // Sub States
        _states[typeof(PlayerIdleState)] = new PlayerIdleState(_context, this);
        _states[typeof(PlayerWalkState)] = new PlayerWalkState(_context, this);
        _states[typeof(PlayerSprintState)] = new PlayerSprintState(_context, this);
        _states[typeof(PlayerSwingState)] = new PlayerSwingState(_context, this);
        
        // Root States
        //_states[typeof(PlayerGroundState)] = new PlayerGroundState(_context, this);
        _states[typeof(PlayerJumpState)] = new PlayerJumpState(_context, this);
        _states[typeof(PlayerFallState)] = new PlayerFallState(_context, this);
    }

    public PlayerBaseState GetState<T>() where T : PlayerBaseState
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
