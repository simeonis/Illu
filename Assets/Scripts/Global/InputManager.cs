using UnityEngine.InputSystem;
using UnityEngine;

public class InputManager : MonoBehaviourSingletonDontDestroy<InputManager>
{
    public PlayerControls playerControls;

    public override void Awake()
    {
        base.Awake();
        playerControls = new PlayerControls();
        ToggleActionMap(playerControls.Player);
        playerControls.Player.Menu.performed +=  OnMenuPlayer;
        playerControls.Menu.Menu.performed += OnMenuMenu;
        // playerControls.Land.Console.performed += context => ShowConsole();
        // playerControls.Menu.Console.performed += context => HideConsole();
    }

    void OnDisable()
    {
        if(playerControls != null)
        {
            playerControls.Player.Menu.performed -= OnMenuPlayer;
            playerControls.Menu.Menu.performed -= OnMenuMenu;
        }
        // playerControls.Land.Console.performed -= context => ShowConsole();
        // playerControls.Menu.Console.performed -= context => HideConsole();
    }

    void OnMenuPlayer(InputAction.CallbackContext context)
    {
        GameManager.Instance.TriggerEvent(GameManager.Event.GamePaused);
        ToggleActionMap(playerControls.Menu);
    }

    void OnMenuMenu(InputAction.CallbackContext context)
    {
        GameManager.Instance.TriggerEvent(GameManager.Event.GameResumed);
        ToggleActionMap(playerControls.Player);
    }

    private void ToggleActionMap(InputActionMap actionMap)
    {
        if (actionMap.enabled) return;

        playerControls.Disable(); // Disable ALL action maps
        actionMap.Enable(); // Enable ONLY one action map
    }
}
