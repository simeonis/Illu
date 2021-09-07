using UnityEngine;
using Steamworks;
using Mirror;
using Mirror.FizzySteam;
using System.Collections.Generic;

public struct SteamUserRecord
{
    public SteamUserRecord(CSteamID id, string name, string status, int avatar)
    {
        ID = id;
        Name = name;
        Status = status;
        Avatar = avatar;
    }

    public CSteamID ID { get; }
    public string Name { get; }
    public string Status { get; }
    public int Avatar { get; }
}

public class SteamLobby : MonoBehaviour
{
    // UI
    [SerializeField] private UIManager UIManager;
    [SerializeField] private SteamFriendListCreator steamFriendListCreator;
    [SerializeField] private SteamFriendLobbyCreator steamFriendLobbyCreator;

    // Callbacks
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyInvite_t> lobbyInvited;
    protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;

    // Steam IDs
    [HideInInspector] public string SteamAppID;
    private CSteamID h_lobbyID; // HOST ONLY - Lobby ID
    private CSteamID c_lobbyID; // CLIENT ONLY - Lobby joined ID

    // Networking
    private const string HostAddressKey = "Host Address Key";
    private MyNetworkManager networkManager;

    private void Start()
    {
        networkManager = GetComponent<MyNetworkManager>();
        SteamAppID = GetComponent<FizzySteamworks>().SteamAppID;

        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreateAttempt);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyJoinAttempt);
        lobbyInvited = Callback<LobbyInvite_t>.Create(OnLobbyInvited);
        lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
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
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("Steam lobby created successfully.\nAttempting to host...");
            networkManager.StartHost();
            h_lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());

            // Adds Host UI to Host Lobby
            steamFriendLobbyCreator.AddLobbyFriend(GetSteamFriend(SteamUser.GetSteamID()), true);
        }
    }

    // CLIENT ONLY
    private void OnLobbyJoinAttempt(LobbyEnter_t callback)
    {
        // Hacky way of confirm successful join
        if (callback.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            // Checks if server has started
            if (NetworkServer.active) 
            { 
                Debug.Log("Failed to join lobby.\nReason: Game already started.");
                return;
            }

            // Successfully joined host
            string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            networkManager.networkAddress = hostAddress;
            networkManager.StartClient();

            // Adds Host UI to Client Lobby
            steamFriendLobbyCreator.AddLobbyFriend(GetSteamFriend(SteamMatchmaking.GetLobbyOwner(c_lobbyID)), true);
        }
    }

    // CLIENT invited by HOST
    private void OnLobbyInvited(LobbyInvite_t callback)
    {
        Debug.Log("Invite Received.");
        c_lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        
        // Will be used to notify CLIENT who invited them
        CSteamID hostID = new CSteamID(callback.m_ulSteamIDUser);

        // TODO: Pass SteamRecord
        UIManager.InviteReceived();
    }

    // CLIENT attemps to join via steam or invite
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Attempting to join...");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        // Entered
        if (callback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered) 
        {
            steamFriendLobbyCreator.AddLobbyFriend(GetSteamFriend(new CSteamID(callback.m_ulSteamIDMakingChange)), false);
        }
        // Left
        else if (callback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft)
        {

        }
        // Kicked
        else if (callback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeKicked)
        {

        }
        // Disconnected
        else if (callback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
        {

        }
    }

    /*  --------------------------
    *   Button activated functions
    *   -------------------------- */

    // USER becomes HOST
    public void HostLobby()
    {
        Debug.Log("Attempting to create steam lobby...");
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    // HOST invites CLIENT
    public void InviteToLobby(CSteamID friendID)
    {
        SteamMatchmaking.InviteUserToLobby(h_lobbyID, friendID);
    }

    // CLIENT joins HOST via invite
    public void JoinSteamLobby()
    {
        SteamMatchmaking.JoinLobby(c_lobbyID);
    }

    /*  --------------------------
    *        Helper functions
    *   -------------------------- */

    private Dictionary<EPersonaState, string> steamStatus = new Dictionary<EPersonaState, string> 
    {
        {EPersonaState.k_EPersonaStateOffline, "Offline"},
        {EPersonaState.k_EPersonaStateOnline, "Online"},
        {EPersonaState.k_EPersonaStateBusy, "Online"},
        {EPersonaState.k_EPersonaStateAway, "Online"},
        {EPersonaState.k_EPersonaStateSnooze, "Online"},
        {EPersonaState.k_EPersonaStateLookingToTrade, "Online"},
        {EPersonaState.k_EPersonaStateLookingToPlay, "Online"},
    };

    public SteamUserRecord GetSteamFriend(CSteamID steamID)
    {
        int friendAvatar = SteamFriends.GetMediumFriendAvatar(steamID);
        string friendName = SteamFriends.GetFriendPersonaName(steamID);
        string friendStatus = steamStatus[SteamFriends.GetFriendPersonaState(steamID)];
        bool friendPlaying = SteamFriends.GetFriendGamePlayed(steamID, out FriendGameInfo_t gameInfo_T);
        bool playingIllu = friendPlaying ? gameInfo_T.m_gameID.ToString() == SteamAppID : false;
        if (playingIllu) friendStatus = "Playing Illu";

        return new SteamUserRecord(steamID, friendName, friendStatus, friendAvatar);
    }

    public void GetSteamFriends()
    {
        string steamAppId = GetComponent<FizzySteamworks>().SteamAppID;
        List<List<SteamUserRecord>> steamFriends = new List<List<SteamUserRecord>>();
        List<SteamUserRecord> playingFriends = new List<SteamUserRecord>();
        List<SteamUserRecord> onlineFriends = new List<SteamUserRecord>();
        List<SteamUserRecord> offlineFriends = new List<SteamUserRecord>();

        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
        for (int i = 0; i < friendCount; i++)
        {
            CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
            SteamUserRecord steamFriend = GetSteamFriend(friendSteamID);

            if (steamFriend.Status == "Playing Illu") playingFriends.Add(steamFriend);
            else if (steamFriend.Status == "Online") onlineFriends.Add(steamFriend);
            else offlineFriends.Add(steamFriend);
        }

        // Sort Alphabetically
        offlineFriends.Sort((f1, f2) => f1.Name.CompareTo(f2.Name));
        onlineFriends.Sort((f1, f2) => f1.Name.CompareTo(f2.Name));
        playingFriends.Sort((f1, f2) => f1.Name.CompareTo(f2.Name));

        // Sort by Playing, Online, Offline
        steamFriends.Add(playingFriends);
        steamFriends.Add(onlineFriends);
        steamFriends.Add(offlineFriends);

        steamFriendListCreator.GenerateList(steamFriends, this);
    }

    public void ClearSteamFriends()
    {
        steamFriendListCreator.DeleteList();
    }
}