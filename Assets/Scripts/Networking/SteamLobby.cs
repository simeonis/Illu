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
    protected Callback<LobbyChatMsg_t> lobbyChatMessage;

    // Steam IDs
    [HideInInspector] static public CGameID SteamAppID;
    private static CSteamID lobbyID;

    // Networking
    private const string HostAddressKey = "Host Address Key";
    private MyNetworkManager networkManager;

    private void Start()
    {
        networkManager = GetComponent<MyNetworkManager>();
        SteamAppID = new CGameID(uint.Parse(GetComponent<FizzySteamworks>().SteamAppID));
    }

    void OnEnable()
    {
        MyNetworkManager.OnClientDisconnected += LobbyDisconnected;
        UIManager.OnStartLobby += HostLobby;
        UIManager.OnLeaveLobby += LobbyLeft;
        UIManager.OnLeaveGame += LobbyLeft;

        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreateAttempt);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyJoinAttempt);
        lobbyInvited = Callback<LobbyInvite_t>.Create(OnLobbyInvited);
        lobbyChatMessage = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);
        lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }

    void OnDisable()
    {
        MyNetworkManager.OnClientDisconnected -= LobbyDisconnected;
        UIManager.OnStartLobby -= HostLobby;
        UIManager.OnLeaveLobby -= LobbyLeft;
        UIManager.OnLeaveGame -= LobbyLeft;

        lobbyCreated.Dispose();
        gameLobbyJoinRequested.Dispose();
        lobbyEntered.Dispose();
        lobbyInvited.Dispose();
        lobbyChatMessage.Dispose();
        lobbyChatUpdate.Dispose();
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
            UIConsole.Log("Error creating steam lobby.");
            return;
        }

        // Successfully created lobby
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            UIConsole.Log("Steam lobby created successfully.\nAttempting to host...");
            networkManager.StartHost();
            lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());

            // Adds Host UI to Host Lobby
            UIManager.GenerateLobbyHost(GetSteamFriend(SteamUser.GetSteamID()), true);
            UIManager.GenerateLobbyEmpty();
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
                UIConsole.Log("Failed to join lobby.\nReason: Game already started.");
                return;
            }

            // Successfully joined host
            string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            networkManager.networkAddress = hostAddress;
            networkManager.StartClient();

            // Lobby UI
            lobbyID = new CSteamID(callback.m_ulSteamIDLobby);

            // Adds Host UI to Client Lobby
            UIManager.GenerateLobbyHost(GetSteamFriend(SteamMatchmaking.GetLobbyOwner(lobbyID)), false);

            // Adds Clients UI to Client Lobby
            int lobbyCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
            for (int i=0; i<lobbyCount; i++)
            {
                CSteamID lobbyMember = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
                UIManager.GenerateLobbyClient(GetSteamFriend(lobbyMember), false);
            }
        }
    }

    // CLIENT invited by HOST
    private void OnLobbyInvited(LobbyInvite_t callback)
    {
        UIConsole.Log("Invite Received.");
        if (callback.m_ulGameID == SteamAppID.m_GameID)
        {
            UIManager.GenerateInvite(new CSteamID(callback.m_ulSteamIDLobby), GetSteamFriend(new CSteamID(callback.m_ulSteamIDUser)));
        }
    }

    // CLIENT attemps to join via steam or invite
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        UIConsole.Log("Attempting to join...");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyChatMessage(LobbyChatMsg_t callback)
    {
        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID sentByUser;
        byte[] msgBuffer = new byte[4000];
        EChatEntryType chatType;

        int numBytes = SteamMatchmaking.GetLobbyChatEntry(lobbyID, (int)callback.m_iChatID, out sentByUser, msgBuffer, 4000, out chatType);
        if (numBytes > 0)
        {
            string message = System.Text.Encoding.ASCII.GetString(msgBuffer).TrimEnd('\0');

            // You have been kicked!
            if (message == SteamUser.GetSteamID().ToString())
            {
                LobbyKicked();
            }
        }
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        SteamUserRecord steamUserMakingChange = GetSteamFriend(new CSteamID(callback.m_ulSteamIDMakingChange));
        SteamUserRecord steamUserChanged = GetSteamFriend(new CSteamID(callback.m_ulSteamIDUserChanged));
        
        switch (callback.m_rgfChatMemberStateChange)
        {
            // Entered
            case (uint) EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                UIConsole.Log(string.Format("[Lobby]: {0} has joined.", steamUserChanged.name));
                UIManager.GenerateLobbyClient(steamUserChanged, true);
                break;
            // Left
            case (uint) EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                UIConsole.Log(string.Format("[Lobby]: {0} has left.", steamUserChanged.name));
                UIManager.RemoveLobbyClient();
                break;
            // Kicked
            case (uint) EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                UIConsole.Log(string.Format("[Lobby]: {0} was kicked by {1}.", steamUserChanged.name, steamUserMakingChange.name));
                UIManager.RemoveLobbyClient();
                break;
            // Disconnected
            case (uint) EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                UIConsole.Log(string.Format("[Lobby]: {0} disconnected.", steamUserChanged.name));
                UIManager.RemoveLobbyClient();
                break;
            default:
                break;
        }
    }

    /*  --------------------------
    *   Button activated functions
    *   -------------------------- */

    // USER becomes HOST
    public static void HostLobby()
    {
        UIConsole.Log("Attempting to create steam lobby...");
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
    }

    private enum LobbyExitReason { Left, Kicked, Disconnected };
    private void LobbyLeft() => LeaveLobby(LobbyExitReason.Left);
    private void LobbyDisconnected() => LeaveLobby(LobbyExitReason.Disconnected);
    private void LobbyKicked() => LeaveLobby(LobbyExitReason.Kicked);

    // USER leaves lobby
    private void LeaveLobby(LobbyExitReason lobbyExitReason)
    {
        // Check if User is in a lobby
        if (lobbyID.m_SteamID == 0) return;

        bool lobbyOwner = SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(lobbyID);

        switch (lobbyExitReason)
        {
            case LobbyExitReason.Left:
                UIConsole.Log("You have left the lobby");
                break;
            case LobbyExitReason.Kicked:
                UIConsole.Log("You have been kicked from the lobby");
                UIManager.Error("Lobby Left", "You have been kicked from the lobby");
                break;
            case LobbyExitReason.Disconnected:
                UIConsole.Log("You have disconnected from the lobby");
                UIManager.Error("Lobby Left", "You have disconnected from the lobby");
                break;
            default:
                UIConsole.Log("You somehow left the lobby");
                UIManager.Error("Lobby Left", "You somehow left the lobby");
                break;
        }
        
        SteamMatchmaking.LeaveLobby(lobbyID);
        lobbyID.Clear();

        if (lobbyOwner) {
            Debug.Log("Host Left");
            networkManager.StopHost();
        }
        else {
            Debug.Log("Client Left.");
            networkManager.StopClient();
        }
    }

    // HOST invites CLIENT
    public static void InviteToLobby(CSteamID friendID)
    {
        SteamMatchmaking.InviteUserToLobby(lobbyID, friendID);
    }

    // CLIENT joins HOST via invite
    public static void JoinSteamLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    // HOST kicks CLIENT
    public static void KickUser(CSteamID steamID)
    {
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(steamID.ToString());
        if (!SteamMatchmaking.SendLobbyChatMsg(lobbyID, bytes, bytes.Length))
        {
            UIConsole.Log("Kick command was unable to send.");
        }
    }

    /*  --------------------------
    *        Helper functions
    *   -------------------------- */

    private static Dictionary<EPersonaState, string> steamStatus = new Dictionary<EPersonaState, string> 
    {
        {EPersonaState.k_EPersonaStateOffline, "Offline"},
        {EPersonaState.k_EPersonaStateOnline, "Online"},
        {EPersonaState.k_EPersonaStateBusy, "Online"},
        {EPersonaState.k_EPersonaStateAway, "Online"},
        {EPersonaState.k_EPersonaStateSnooze, "Online"},
        {EPersonaState.k_EPersonaStateLookingToTrade, "Online"},
        {EPersonaState.k_EPersonaStateLookingToPlay, "Online"},
    };

    public static SteamUserRecord GetSteamFriend(CSteamID steamID)
    {
        int friendAvatar = SteamFriends.GetMediumFriendAvatar(steamID);
        string friendName = SteamFriends.GetFriendPersonaName(steamID);
        string friendStatus = steamStatus[SteamFriends.GetFriendPersonaState(steamID)];
        
        bool friendPlaying = SteamFriends.GetFriendGamePlayed(steamID, out FriendGameInfo_t gameInfo_T);
        bool playingIllu = friendPlaying ? (gameInfo_T.m_gameID.m_GameID == SteamAppID.m_GameID) : false;
        if (playingIllu) friendStatus = "Playing Illu";

        return new SteamUserRecord(steamID, friendName, friendStatus, friendAvatar);
    }

    public static List<List<SteamUserRecord>> GetSteamFriends()
    {
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

        // Sort by: Playing, Online, Offline
        steamFriends.Add(playingFriends);
        steamFriends.Add(onlineFriends);
        steamFriends.Add(offlineFriends);

        return steamFriends;
    }
}