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
    // UI
    [SerializeField] private UIManager UIManager;

    // Callbacks
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyInvite_t> lobbyInvited;

    // Steam IDs
    private CSteamID LobbyID;
    private CSteamID invitedLobbyID;
    private CSteamID hostID;

    // Networking
    private const string HostAddressKey = "Host Address Key";
    private MyNetworkManager networkManager;

    private void Start()
    {
        networkManager = GetComponent<MyNetworkManager>();

        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreateAttempt);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyJoinAttempt);
        lobbyInvited = Callback<LobbyInvite_t>.Create(OnLobbyInvited);
    }

    /*  --------------------------
    *       Callback functions
    *   -------------------------- */

    // HOST attempts to create lobby
    private void OnLobbyCreateAttempt(LobbyCreated_t callback)
    {
        // Error creating lobby
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.Log("Error creating steam lobby.");
            UIManager.HostGameFailed();
            return;
        }

        // Successfully created lobby
        Debug.Log("Steam lobby created successfully.\nAttempting to host...");
        networkManager.StartHost();
        LobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
    }

    // CLIENT ONLY
    private void OnLobbyJoinAttempt(LobbyEnter_t callback)
    {
        // Checks if server has started.
        if (NetworkServer.active) 
        { 
            Debug.Log("Failed to join lobby.\nReason: Game already started.");
            return;
        }

        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        UIManager.JoinedHost();
    }

    // CLIENT invited by HOST
    private void OnLobbyInvited(LobbyInvite_t callback)
    {
        Debug.Log("Invite Received.");
        invitedLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        hostID = new CSteamID(callback.m_ulSteamIDUser);

        // TODO: Pass SteamRecord
        UIManager.InviteReceived();
    }

    // CLIENT attemps to join via steam or invite
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Attempting to join...");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    /*  --------------------------
    *   Button activated functions
    *   -------------------------- */

    // USER becomes HOST
    public void HostLobby()
    {
        Debug.Log("Attempting to create steam lobby...");
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
        UIManager.HostGame(this, GetSteamFriends());
    }

    // HOST invites CLIENT
    public void InviteToLobby(CSteamID friendID)
    {
        SteamMatchmaking.InviteUserToLobby(LobbyID, friendID);
    }

    // CLIENT joins HOST via invite
    public void JoinSteamLobby()
    {
        SteamMatchmaking.JoinLobby(invitedLobbyID);
        UIManager.InviteAccepted();
    }

    /*  --------------------------
    *        Helper functions
    *   -------------------------- */

    private List<SteamUserRecord> GetSteamFriends()
    {
        List<SteamUserRecord> steamFriends = new List<SteamUserRecord>();

        int nFriend = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
        for (int i = 0; i < nFriend; i++)
        {
            CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);

            string friendName = SteamFriends.GetFriendPersonaName(friendSteamID);

            int friendAvatar = SteamFriends.GetMediumFriendAvatar(friendSteamID);

            steamFriends.Add(new SteamUserRecord(friendSteamID, friendName, friendAvatar));
        }

        return steamFriends;
    }
}