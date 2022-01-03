using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerControls playerControls;
    public static event Action<InputActionMap> OnActionMapChanged;

    void Awake()
    {
        DontDestroyOnLoad(this);
        playerControls = new PlayerControls();
        ToggleActionMap(playerControls.Land);
    }

    public static void ToggleActionMap(InputActionMap actionMap)
    {
        if (actionMap.enabled) return;

        playerControls.Disable(); // Disable ALL action maps
        OnActionMapChanged?.Invoke(actionMap);
        actionMap.Enable(); // Enable ONLY one action map
    }
}
