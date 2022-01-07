using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerControls playerControls;
    public static event Action<InputActionMap> OnActionMapChanged;

    void Awake()
    {
        playerControls = new PlayerControls();

        playerControls.Land.Menu.performed += context => GameManager.TriggerEvent("GamePaused");
        playerControls.Menu.Menu.performed += context => GameManager.TriggerEvent("GameResumed");
        // playerControls.Land.Console.performed += context => ShowConsole();
        // playerControls.Menu.Console.performed += context => HideConsole();
    }

    void OnDestroy()
    {
        playerControls.Land.Menu.performed -= context => GameManager.TriggerEvent("GamePaused");
        playerControls.Menu.Menu.performed -= context => GameManager.TriggerEvent("GameResumed");
        // playerControls.Land.Console.performed -= context => ShowConsole();
        // playerControls.Menu.Console.performed -= context => HideConsole();

        playerControls = null;
    }

    public static void ToggleActionMap(InputActionMap actionMap)
    {
        if (actionMap.enabled) return;

        playerControls.Disable(); // Disable ALL action maps
        OnActionMapChanged?.Invoke(actionMap);
        actionMap.Enable(); // Enable ONLY one action map
    }
}
