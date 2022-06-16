using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public PlayerControls playerControls;

    void Awake()
    {
        if (Instance != null) 
        {
            // There exist an instance, and it is not me, kill...
            if (Instance != this) 
                Destroy(gameObject);
            return;
        }

        // There does not exist an instance, create...
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerControls = new PlayerControls();

        playerControls.Player.Menu.performed += context => GameManager.Instance.TriggerEvent("GamePaused");
        playerControls.Menu.Menu.performed += context => GameManager.Instance.TriggerEvent("GameResumed");
        // playerControls.Land.Console.performed += context => ShowConsole();
        // playerControls.Menu.Console.performed += context => HideConsole();
    }

    void OnDisable()
    {
        playerControls.Player.Menu.performed -= context => GameManager.Instance.TriggerEvent("GamePaused");
        playerControls.Menu.Menu.performed -= context => GameManager.Instance.TriggerEvent("GameResumed");
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
