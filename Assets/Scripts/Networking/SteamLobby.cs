using UnityEngine;
using Steamworks;
using Mirror;
using System.Collections.Generic;

public struct SteamUserRecord
{
    public SteamUserRecord(CSteamID id, string name, int avatar)
    {
        ID = id;
        Name = name;
        Avatar = avatar;
    }

    public CSteamID ID { get; }
    public string Name { get; }
    public int Avatar { get; }
}

public class SteamLobby : MonoBehaviour
{
    [SerializeField] private GameObject hostButton = null;
    [SerializeField] private GameObject joinButton = null;
    [SerializeField] private GameObject startButton = null;
    [SerializeField] private GameObject launchUI = null;
    [SerializeField] private GameObject scrollView = null;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyInvite_t> lobbyInvited;

    private const string HostAddressKey = "Host Address Key";

    private CSteamID LobbyID;
    private CSteamID invitedLobbyID;
    private CSteamID hostID;

    private MyNetworkManager networkManager;

    private ListCreator listCreator;

    private List<SteamUserRecord> steamFriends;

    private void Start()
    {
        networkManager = GetComponent<MyNetworkManager>();
        listCreator = scrollView.GetComponent<ListCreator>();

        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyInvited = Callback<LobbyInvite_t>.Create(OnLobbyInvited);
    }

    public void HostLobby()
    {
        hostButton.SetActive(false);

        Debug.Log("Host Lobby Called");

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);

        getSteamFriends();
        listCreator.renderList(steamFriends, this);
        scrollView.SetActive(true);

    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            hostButton.SetActive(true);
            return;
        }

        networkManager.StartHost();

        LobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) { return; }

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        hostButton.SetActive(false);

        if (networkManager.RoomPlayers.Count == 2)
        {
            Debug.Log("Game can start");
            startButton.SetActive(true);
        }
    }


    private void getSteamFriends()
    {
        steamFriends = new List<SteamUserRecord>();

        int nFriend = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

        for (int i = 0; i < nFriend; i++)
        {
            CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);

            string friendName = SteamFriends.GetFriendPersonaName(friendSteamID);

            int friendAvatar = SteamFriends.GetMediumFriendAvatar(friendSteamID);

            steamFriends.Add(new SteamUserRecord(friendSteamID, friendName, friendAvatar));
        }
    }

    public void InviteToLobby(CSteamID friendID)
    {
        //Debug.Log("ID" + id)
        SteamMatchmaking.InviteUserToLobby(LobbyID, friendID);
        scrollView.SetActive(false);

        //networkManager.StartGame();
    }

    private void OnLobbyInvited(LobbyInvite_t callback)
    {
        Debug.Log("Invite Received");
        joinButton.SetActive(true);
        invitedLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        hostID = new CSteamID(callback.m_ulSteamIDUser);
    }

    public void JoinSteamLobby()
    {
        SteamMatchmaking.JoinLobby(invitedLobbyID);
        joinButton.SetActive(false);
    }

    public void launchGame()
    {
        networkManager.StartGame();
        launchUI.SetActive(false);
    }
}