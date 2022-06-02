public class PlayerSwingState : PlayerBaseState
{
    public PlayerSwingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        InitializeSubState();
    }

    public override void CheckSwitchState()
    {
        
    }
}
