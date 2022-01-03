using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using System;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public struct SCREENS
    {
        public GameObject Root;
        public GameObject Host;
        public GameObject Friend;
        public GameObject Join;
        public GameObject Play;
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

    [Header("Color")]
    [SerializeField] private Color statusPlaying;
    [SerializeField] private Color statusOnline;
    [SerializeField] private Color statusOffline;
    [SerializeField] private Color evenFriend;
    [SerializeField] private Color oddFriend;

    private static List<string> status = new List<string>() { "Playing Illu", "Online", "Offline" };
    private static Dictionary<string, GameObject> invites = new Dictionary<string, GameObject>();

    public static event Action<PlayerHUD> OnPlayScreen;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void OnEnable()
    {
        MyNetworkManager.OnClientReadied += Play;
        InputManager.playerControls.Land.Menu.performed += context => ShowMenu();
        InputManager.playerControls.Menu.Menu.performed += context => HideMenu();
        InputManager.playerControls.Land.Console.performed += context => ShowConsole();
        InputManager.playerControls.Menu.Console.performed += context => HideConsole();
    }

    void OnDisable()
    {
        MyNetworkManager.OnClientReadied -= Play;
        InputManager.playerControls.Land.Menu.performed -= context => ShowMenu();
        InputManager.playerControls.Menu.Menu.performed -= context => HideMenu();
        InputManager.playerControls.Land.Console.performed -= context => ShowConsole();
        InputManager.playerControls.Menu.Console.performed -= context => HideConsole();
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void Play()
    {
        screens.Host.SetActive(false);
        screens.Play.SetActive(true);
        OnPlayScreen?.Invoke(playerHUD);
    }

    public void LobbyExited()
    {
        screens.Host.SetActive(false);
        screens.Root.SetActive(true);
        DestroyLobby();
    }

    private void ShowMenu()
    {
        PauseGame();
        screens.Play.SetActive(false);
        screens.Host.SetActive(true);

    }

    private void HideMenu()
    {
        ResumeGame();
        screens.Host.SetActive(false);
        screens.Play.SetActive(true);
    }

    private void ShowConsole()
    {
        PauseGame();
        screens.Console.SetActive(true);
    }

    private void HideConsole()
    {
        ResumeGame();
        screens.Console.SetActive(false);
    }

    private void PauseGame()
    {
        InputManager.ToggleActionMap(InputManager.playerControls.Menu);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        InputManager.ToggleActionMap(InputManager.playerControls.Land);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /*  --------------------------
    *       Steam UI functions
    *   -------------------------- */

    public void GenerateInvite(CSteamID lobbyID, SteamUserRecord steamFriend)
    {
        string steamIDstring = steamFriend.id.ToString();

        // Invite exists
        if (invites.ContainsKey(steamIDstring))
        {
            // Re-arrange Order
            invites[steamIDstring].transform.SetAsFirstSibling();
        }
        // New invite
        else
        {
            // Create GameObject
            GameObject invite = Instantiate(steamInvitePrefab, inviteList);
            invite.name = steamIDstring;
            invite.transform.SetAsFirstSibling();

            // Add to dictionary
            invites.Add(invite.name, invite);

            SteamFriendInvite inviteDetails = invite.GetComponent<SteamFriendInvite>();

            // Steam Avatar
            Texture2D tex = GetSteamImageAsTexture2D(steamFriend.avatar);
            inviteDetails.avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

            // Steam Name
            inviteDetails.name.text = steamFriend.name;

            // Accept Button
            inviteDetails.acceptButton.onClick.AddListener(delegate
            {
                SteamLobby.JoinSteamLobby(lobbyID);
                screens.Join.SetActive(false);
                screens.Host.SetActive(true);
                DestroyInvite(invite);
            });

            // Decline Button
            inviteDetails.declineButton.onClick.AddListener(delegate
            {
                DestroyInvite(invite);
            });
        }
    }

    private void DestroyInvite(GameObject invite)
    {
        // Remove from dictionary
        invites.Remove(invite.name);

        // Destroy GameObject
        Destroy(invite);
    }

    public void GenerateLobbyHost(SteamUserRecord steamFriend, bool serverside)
    {
        GenerateLobbyFriend(steamFriend, serverside, true);
    }

    public void GenerateLobbyClient(SteamUserRecord steamFriend, bool serverside)
    {
        GenerateLobbyFriend(steamFriend, serverside, false);
    }

    private void GenerateLobbyFriend(SteamUserRecord steamFriend, bool serverside, bool hostSlot)
    {
        GameObject lobbyUser = Instantiate(steamLobbyPrefab, hostSlot ? lobbyHost : lobbyClient);
        if (!hostSlot) Destroy(lobbyClient.GetChild(0).gameObject); // Destroy EmptyLobbyFriend
        lobbyUser.name = steamFriend.id.ToString();

        SteamFriendLobby lobbyUserDetails = lobbyUser.GetComponent<SteamFriendLobby>();

        // Steam Avatar
        Texture2D tex = GetSteamImageAsTexture2D(steamFriend.avatar);
        lobbyUserDetails.avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        // Steam Name
        lobbyUserDetails.name.text = steamFriend.name;

        // Kick Button
        bool canKick = serverside && !hostSlot;
        lobbyUserDetails.removeButton.gameObject.SetActive(canKick);
        if (!hostSlot)
        {
            lobbyUserDetails.removeButton.onClick.AddListener(delegate
            {
                SteamLobby.KickUser(steamFriend.id);
            });
        }
    }

    public void GenerateLobbyEmpty()
    {
        GameObject lobbyEmpty = Instantiate(steamEmptyLobbyPrefab, lobbyClient);

        SteamEmptyLobby lobbyEmptyDetails = lobbyEmpty.GetComponent<SteamEmptyLobby>();

        lobbyEmptyDetails.addButton.onClick.AddListener(delegate
        {
            GenerateFriendList(SteamLobby.GetSteamFriends());
            screens.Friend.SetActive(true);
            screens.Host.SetActive(false);
        });
    }

    public void DestroyLobby()
    {
        DestroyLobbyHost();
        DestroyLobbyClient();
    }

    public void DestroyLobbyHost()
    {
        if (lobbyHost.childCount > 0)
        {
            foreach (Transform child in lobbyHost) Destroy(child.gameObject);
        }
    }

    public void DestroyLobbyClient()
    {
        if (lobbyClient.childCount > 0)
        {
            foreach (Transform child in lobbyClient) Destroy(child.gameObject);
        }
    }

    public void RemoveLobbyClient()
    {
        DestroyLobbyClient();
        GenerateLobbyEmpty();
    }

    private void GenerateFriendList(List<List<SteamUserRecord>> steamFriends)
    {
        GameObject subList = null;
        float numberOfTypes = steamFriends.Count;
        for (int i = 0; i < numberOfTypes; i++)
        {
            // Create and position Title
            GameObject titleUI = Instantiate(steamStatusTitlePrefab, friendList);

            // Set Title text
            titleUI.GetComponent<SteamStatusTitle>().status.text = status[i];

            float numberOfItems = steamFriends[i].Count;
            for (int j = 0; j < numberOfItems; j++)
            {
                // Increase every 3 times
                if (j % 3 == 0)
                {
                    subList = new GameObject("Horizontal Group #" + (j + 1));
                    subList.transform.SetParent(friendList, false);
                    HorizontalLayoutGroup horizontalGroup = subList.gameObject.AddComponent<HorizontalLayoutGroup>();
                    horizontalGroup.childForceExpandWidth = false;
                    horizontalGroup.childControlWidth = false;
                    horizontalGroup.childControlHeight = true;
                }

                GameObject friendUI = Instantiate(steamFriendListPrefab, subList.transform);

                SteamUserRecord steamFriend = steamFriends[i][j];
                SteamFriendList steamFriendDetails = friendUI.GetComponent<SteamFriendList>();

                // Background
                steamFriendDetails.background.color = (j % 2 == 0) ? evenFriend : oddFriend;

                // Steam Avatar
                Texture2D tex = GetSteamImageAsTexture2D(steamFriend.avatar);
                steamFriendDetails.avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

                // Steam Name
                steamFriendDetails.name.text = steamFriend.name;

                // Steam Status
                steamFriendDetails.status.text = steamFriend.status;
                if (steamFriend.status == "Online")
                    steamFriendDetails.status.color = statusOnline;
                else if (steamFriend.status == "Offline")
                    steamFriendDetails.status.color = statusOffline;
                else steamFriendDetails.status.color = statusPlaying;

                // Steam Invite Button
                CSteamID id = steamFriend.id;
                steamFriendDetails.inviteButton.onClick.AddListener(delegate
                {
                    SteamLobby.InviteToLobby(id);
                });
            }
        }
    }

    public void DestroyFriendList()
    {
        foreach (Transform child in friendList.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /*  --------------------------
    *        Helper functions
    *   -------------------------- */

    private Texture2D GetSteamImageAsTexture2D(int iImage)
    {
        Texture2D ret = null;
        uint ImageWidth;
        uint ImageHeight;
        bool bIsValid = SteamUtils.GetImageSize(iImage, out ImageWidth, out ImageHeight);

        if (bIsValid)
        {
            byte[] Image = new byte[ImageWidth * ImageHeight * 4];

            bIsValid = SteamUtils.GetImageRGBA(iImage, Image, (int)(ImageWidth * ImageHeight * 4));
            if (bIsValid)
            {
                ret = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
                ret.LoadRawTextureData(Image);
                ret.Apply();
            }
        }

        return ret;
    }
}
