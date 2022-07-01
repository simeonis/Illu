using UnityEngine.InputSystem;

public class InputManager : MonoBehaviourSingletonDontDestroy<InputManager>
{
    public PlayerControls playerControls;

    public override void Awake()
    {
        base.Awake();
        playerControls = new PlayerControls();

        playerControls.Player.Menu.performed += context => GameManager.Instance.TriggerEvent(GameManager.Event.GamePaused);
        playerControls.Menu.Menu.performed += context => GameManager.Instance.TriggerEvent(GameManager.Event.GameResumed);
        // playerControls.Land.Console.performed += context => ShowConsole();
        // playerControls.Menu.Console.performed += context => HideConsole();
    }

    void OnDisable()
    {
        playerControls.Player.Menu.performed -= context => GameManager.Instance.TriggerEvent(GameManager.Event.GamePaused);
        playerControls.Menu.Menu.performed -= context => GameManager.Instance.TriggerEvent(GameManager.Event.GameResumed);
        // playerControls.Land.Console.performed -= context => ShowConsole();
        // playerControls.Menu.Console.performed -= context => HideConsole();
    }

    public void TogglePlayer()
    {
        ToggleActionMap(playerControls.Player);
    }

    public void ToggleMenu()
    {
        ToggleActionMap(playerControls.Menu);
    }

    private void ToggleActionMap(InputActionMap actionMap)
    {
        if (actionMap.enabled) return;

        Instance.playerControls.Disable(); // Disable ALL action maps
        actionMap.Enable(); // Enable ONLY one action map
    }
}
