using System.Collections.Generic;
using UnityEngine;
using Illu.Networking;
using System;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public struct SCREENS {
        public GameObject Root;
        public GameObject Host;
        public GameObject Friend;
        public GameObject Join;
        public GameObject Play;
        public GameObject Pause;
        public GameObject Settings;
        public GameObject Console;
        public GameObject Error;
    }

    [SerializeField] private SCREENS screens;

    [Header("UI Components")]
    [SerializeField] private PlayerHUD playerHUD;

    [Header("Prefabs")]
    [SerializeField] private GameObject errorPopupPrefab;

    // Screen callbacks
    public static event Action OnRootScreen;
    public static event Action OnHostScreen;
    public static event Action OnFriendScreen;
    //public static event Action OnJoinScreen;
    public static event Action<PlayerHUD> OnPlayScreen;
    public static event Action OnSettingsScreen;
    public static event Action OnConsoleScreen;
    public static event Action OnErrorScreen;

    // Game callbacks
    public static event Action OnResumeGame;
    public static event Action OnPauseGame;

    void Awake()
    {
        NetworkManager.OnGameStarted += GameStarted;
        InputManager.playerControls.Land.Menu.performed += context => ShowMenu();
        InputManager.playerControls.Menu.Menu.performed += context => HideMenu();
        InputManager.playerControls.Land.Console.performed += context => ShowConsole();
        InputManager.playerControls.Menu.Console.performed += context => HideConsole();
    }

    void OnDestroy()
    {
        NetworkManager.OnGameStarted -= GameStarted;
        InputManager.playerControls.Land.Menu.performed -= context => ShowMenu();
        InputManager.playerControls.Menu.Menu.performed -= context => HideMenu();
        InputManager.playerControls.Land.Console.performed -= context => ShowConsole();
        InputManager.playerControls.Menu.Console.performed -= context => HideConsole();
    }

    /*  --------------------------
    *          Root Screen
    *   -------------------------- */

    /*  --------------------------
    *          Host Screen
    *   -------------------------- */

    /*  --------------------------
    *          Friend Screen
    *   -------------------------- */

    /*  --------------------------
    *          Join Screen
    *   -------------------------- */

    /*  --------------------------
    *          Play Screen
    *   -------------------------- */

    private void GameStarted()
    {
        screens.Host.SetActive(false);
        screens.Play.SetActive(true);
        OnPlayScreen?.Invoke(playerHUD);
        GameResumed();
    }

    private void GameResumed()
    {
        InputManager.ToggleActionMap(InputManager.playerControls.Land);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        OnResumeGame?.Invoke();
    }

    /*  --------------------------
    *          Pause Screen
    *   -------------------------- */

    public void ShowMenu()
    {
        GamePaused();
        screens.Pause.SetActive(true);
    }

    public void HideMenu()
    {
        GameResumed();
        screens.Pause.SetActive(false);
    }

    public void LeaveGame()
    {
        // BackToRoot();
        // OnLeaveGame?.Invoke();
    }

    private void GamePaused()
    {
        InputManager.ToggleActionMap(InputManager.playerControls.Menu);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnPauseGame?.Invoke();
    }

    /*  --------------------------
    *          Settings Screen
    *   -------------------------- */

    /*  --------------------------
    *          Console Screen
    *   -------------------------- */

    private void ShowConsole()
    {
        GamePaused();
        screens.Console.SetActive(true);
        OnConsoleScreen?.Invoke();
    }

    private void HideConsole()
    {
        GameResumed();
        screens.Console.SetActive(false);
    }

    /*  --------------------------
    *          Error Screen
    *   -------------------------- */

    public void Error(string title, string message)
    {
        GameObject popup = Instantiate(errorPopupPrefab, screens.Error.transform);

        ErrorPopup popupDetails = popup.GetComponent<ErrorPopup>();

        popupDetails.title.text = title;
        popupDetails.message.text = message;
        popupDetails.dismissButton.onClick.AddListener(delegate {
            screens.Error.SetActive(false);
            Destroy(popup.gameObject);
        });

        //BackToRoot();
        screens.Error.SetActive(true);
        OnErrorScreen?.Invoke();
    }
}
