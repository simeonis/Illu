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

    [Header("Target Parent")]
    [SerializeField] private RectTransform friendList;
    [SerializeField] private RectTransform inviteList;
    [SerializeField] private RectTransform lobbyHost;
    [SerializeField] private RectTransform lobbyClient;

    [Header("Prefabs")]
    [SerializeField] private GameObject steamStatusTitlePrefab;
    [SerializeField] private GameObject steamFriendListPrefab;
    [SerializeField] private GameObject steamLobbyPrefab;
    [SerializeField] private GameObject steamEmptyLobbyPrefab;
    [SerializeField] private GameObject steamInvitePrefab;
    [SerializeField] private GameObject errorPopupPrefab;

    [Header("Color")]
    [SerializeField] private Color statusPlaying;
    [SerializeField] private Color statusOnline;
    [SerializeField] private Color statusOffline;
    [SerializeField] private Color evenFriend;
    [SerializeField] private Color oddFriend;

    private static List<string> status = new List<string>() { "Playing Illu", "Online", "Offline" };
    private static Dictionary<string, GameObject> invites = new Dictionary<string, GameObject>();

    // Screen callbacks
    public static event Action OnRootScreen;
    public static event Action OnHostScreen;
    public static event Action OnFriendScreen;
    public static event Action OnJoinScreen;
    public static event Action<PlayerHUD> OnPlayScreen;
    public static event Action OnSettingsScreen;
    public static event Action OnConsoleScreen;
    public static event Action OnErrorScreen;

    // Game callbacks
    public static event Action OnStartLobby;
    public static event Action OnLeaveLobby;
    public static event Action OnStartGame;
    public static event Action OnLeaveGame;
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

    public void HostLobby()
    {
        screens.Root.SetActive(false);
        screens.Host.SetActive(true);
        OnHostScreen?.Invoke();
        OnStartLobby?.Invoke();
    }

    public void JoinLobby()
    {
        screens.Root.SetActive(false);
        screens.Join.SetActive(true);
        OnJoinScreen?.Invoke();
    }

    public void Settings()
    {
        screens.Root.SetActive(false);
        screens.Settings.SetActive(true);
        OnSettingsScreen?.Invoke();
    }

    public void BackToRoot()
    {
        screens.Host.SetActive(false);
        screens.Join.SetActive(false);
        screens.Settings.SetActive(false);
        screens.Play.SetActive(false);
        screens.Pause.SetActive(false);
        screens.Console.SetActive(false);
        screens.Root.SetActive(true);
        OnRootScreen?.Invoke();
    }

    public void Quit()
    {
        Application.Quit();
    }

    /*  --------------------------
    *          Host Screen
    *   -------------------------- */

    public void StartGame()
    {
        OnStartGame?.Invoke();
    }

    public void LeaveLobby()
    {
        BackToRoot();
        // DestroyLobby();
        OnLeaveLobby?.Invoke();
    }

    /*  --------------------------
    *          Friend Screen
    *   -------------------------- */

    private void ShowFriendList()
    {
        screens.Host.SetActive(false);
        screens.Friend.SetActive(true);
        OnFriendScreen?.Invoke();
    }

    public void CloseFriendList()
    {
        screens.Friend.SetActive(false);
        screens.Host.SetActive(true);
        // DestroyFriendList();
        OnHostScreen?.Invoke();
    }

    /*  --------------------------
    *          Join Screen
    *   -------------------------- */

    public void FindLobby()
    {

    }

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
        BackToRoot();
        OnLeaveGame?.Invoke();
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

        BackToRoot();
        screens.Error.SetActive(true);
        OnErrorScreen?.Invoke();
    }
}
