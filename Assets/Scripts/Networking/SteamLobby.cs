using UnityEngine;
using Steamworks;
using Mirror;
using Mirror.FizzySteam;
using System.Collections.Generic;

public struct SteamUserRecord
{
    public SteamUserRecord(CSteamID id, string name, string status, int avatar)
    {
        this.id = id;
        this.name = name;
        this.status = status;
        this.avatar = avatar;
    }

    public CSteamID id { get; }
    public string name { get; }
    public string status { get; }
    public int avatar { get; }
}

public struct SteamInvite
{
    public SteamInvite(SteamUserRecord sender)
    {
        this.sender = sender;
        this.amount = 1;
        this.seen = false;
    }

    public SteamUserRecord sender;
    public int amount;
    public bool seen;
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
    protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;

    // Steam IDs
    [HideInInspector] public CGameID SteamAppID;
    private CSteamID h_lobbyID; // HOST ONLY - Lobby ID

    // Networking
    private const string HostAddressKey = "Host Address Key";
    private MyNetworkManager networkManager;

    private void Start()
    {
        networkManager = GetComponent<MyNetworkManager>();
        SteamAppID = new CGameID(uint.Parse(GetComponent<FizzySteamworks>().SteamAppID));

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
            UIManager.GenerateLobbyFriend(GetSteamFriend(SteamUser.GetSteamID()), true);
            UIManager.GenerateLobbyEmpty(this);
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

            // Lobby UI
            CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);

            // Adds Host UI to Client Lobby
            UIManager.GenerateLobbyFriend(GetSteamFriend(SteamMatchmaking.GetLobbyOwner(lobbyID)), true);

            // Adds Clients UI to Client Lobby
            int lobbyCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
            for (int i=0; i<lobbyCount; i++)
            {
                CSteamID lobbyMember = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
                UIManager.GenerateLobbyFriend(GetSteamFriend(lobbyMember), false);
            }
        }
    }

    // CLIENT invited by HOST
    private void OnLobbyInvited(LobbyInvite_t callback)
    {
        Debug.Log("Invite Received.");
        if (callback.m_ulGameID == SteamAppID.m_GameID)
        {
            UIManager.GenerateInvite(this, new CSteamID(callback.m_ulSteamIDLobby), GetSteamFriend(new CSteamID(callback.m_ulSteamIDUser)));
        }
        
        // Will be used to notify CLIENT who invited them
        CSteamID hostID = new CSteamID(callback.m_ulSteamIDUser);
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
            UIManager.GenerateLobbyFriend(GetSteamFriend(new CSteamID(callback.m_ulSteamIDMakingChange)), false);
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
    public void JoinSteamLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
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
        bool playingIllu = friendPlaying ? (gameInfo_T.m_gameID.m_GameID == SteamAppID.m_GameID) : false;
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

            if (steamFriend.status == "Playing Illu") playingFriends.Add(steamFriend);
            else if (steamFriend.status == "Online") onlineFriends.Add(steamFriend);
            else offlineFriends.Add(steamFriend);
        }

        // Sort Alphabetically
        offlineFriends.Sort((f1, f2) => f1.name.CompareTo(f2.name));
        onlineFriends.Sort((f1, f2) => f1.name.CompareTo(f2.name));
        playingFriends.Sort((f1, f2) => f1.name.CompareTo(f2.name));

        // Sort by Playing, Online, Offline
        steamFriends.Add(playingFriends);
        steamFriends.Add(onlineFriends);
        steamFriends.Add(offlineFriends);

        UIManager.GenerateFriendList(this, steamFriends);
    }

    public void ClearSteamFriends()
    {
        UIManager.DestroyFriendList();
    }
}