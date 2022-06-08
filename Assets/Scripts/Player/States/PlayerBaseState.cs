public abstract class PlayerBaseState
{
    protected bool _isRootState = false;
    protected PlayerStateMachine _ctx;
    protected PlayerStateFactory _factory;
    protected PlayerBaseState _currentSubState;
    protected PlayerBaseState _currentSuperState;
    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        _ctx = currentContext;
        _factory = playerStateFactory;
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
    /// Called once per physics update.
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

    /// <summary>
    /// Initialize substates for the current state.
    /// </summary>
    public virtual void InitializeSubState() {}

    /// <summary>
    /// Called upon entering current state and all substates.
    /// </summary>
    public void EnterStates()
    {
        EnterState();
        if (_currentSubState != null)
        {
            _currentSubState.SetSuperState(this);
            _currentSubState.EnterState();
        }
    }

    /// <summary>
    /// Called once per frame (including substates).
    /// </summary>
    public void UpdateStates() 
    {
        UpdateState();
        _currentSubState?.UpdateStates();
    }

    /// <summary>
    /// Called once per physics update (including substates).
    /// </summary>
    public void FixedUpdateStates()
    {
        FixedUpdateState();
        _currentSubState?.FixedUpdateState();
    }

    /// <summary>
    /// Called upon exiting current state and all substates.
    /// </summary>
    public void ExitStates()
    {
        ExitState();
        _currentSubState?.ExitState();
    }

    /// <summary>
    /// Holds logic that determines when to switch frames. Called once per frame (including substates).
    /// </summary>
    public void CheckSwitchStates()
    {
        CheckSwitchState();
        _currentSubState?.CheckSwitchState();
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Called inside OnDrawGizmos.
    /// </summary>
    public virtual void GizmosState() {}
    #endif

    /// <summary>
    /// Called upon exiting current state.
    /// </summary>
    protected void SwitchState(PlayerBaseState newState) 
    {
        ExitStates();
        newState.EnterStates();

        if (_isRootState)
            _ctx.CurrentState = newState;
        else if (_currentSuperState != null)
            _currentSuperState.SetSubState(newState);
    }

    /// <summary>
    /// Sets substate's superstate.
    /// </summary>
    protected void SetSuperState(PlayerBaseState newSuperState) 
    {
        _currentSuperState = newSuperState;
    }

    /// <summary>
    /// Sets superstate's substate.
    /// </summary>
    protected void SetSubState(PlayerBaseState newSubState) 
    {
        _currentSubState = newSubState;
        _currentSubState.SetSuperState(this);
    }

    protected void InitializeLocomotion()
    {
        if (!_ctx.IsMovementPressed && !_ctx.IsSprintPressed)
            SetSubState(_factory.GetState<PlayerIdleState>());
        else if(_ctx.IsMovementPressed && !_ctx.IsSprintPressed)
            SetSubState(_factory.GetState<PlayerWalkState>());
        else
            SetSubState(_factory.GetState<PlayerSprintState>());
    }
}
