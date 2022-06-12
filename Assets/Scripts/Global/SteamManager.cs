using UnityEngine;
using Steamworks;
using Mirror;
using Mirror.FizzySteam;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Illu.Steam
{
    public class SteamManager : MonoBehaviourSingletonDontDestroy<SteamManager>
    {
        //Events raised by Steam Manager 
        public delegate void OnLobbyClientJoined(SteamUserRecord user, bool serverside);    
        public OnLobbyClientJoined clientJoined;

        public delegate void OnLobbyHost(SteamUserRecord user, bool serverside);
        public OnLobbyHost onLobbyHost;

        public delegate void OnInviteReceived(CSteamID id, SteamUserRecord user);
        public OnInviteReceived onInviteReceived;

        public UnityEvent onClearLobby = new UnityEvent();
        public UnityEvent onDestroyLobby = new UnityEvent();
        public UnityEvent OnLobbyClientRemoved = new UnityEvent();

        // Callbacks handled by steam Manager 
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
        //public Networking.NetworkManager networkManager;

        private SteamUserRecord _steamLobbyClientMember;

        void Start()
        {
            SteamAppID = new CGameID(uint.Parse(GetComponent<FizzySteamworks>().SteamAppID));
        }

        //Steam lobby create attempt -> hostLobby
        // stean lobby created _> null
        // steam lobby entered -> null
        // lobby left -> lobby left 
        // lbb kick -> kick 
        //disconecct -> lobby dis && Root screen
        // INvite -> null
        // Friends requested -> null

        void OnEnable()
        {
            if (!global::SteamManager.Initialized) { return; }

            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreateAttempt);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyJoinAttempt);
            lobbyInvited = Callback<LobbyInvite_t>.Create(OnLobbyInvited);
            lobbyChatMessage = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);
            lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);


            GameManager.Instance.AddListener(GameManager.Event.SteamLobbyCreateAttempt, HostLobby);
            GameManager.Instance.AddListener(GameManager.Event.GameLeft,                LobbyLeft);
            GameManager.Instance.AddListener(GameManager.Event.SteamLobbyKicked,        LobbyKicked);
            GameManager.Instance.AddListener(GameManager.Event.SteamLobbyDisconnected,  LobbyDisconnected);

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
                GameManager.Instance.TriggerEvent(GameManager.Event.SteamLobbyCreated);
                UIConsole.Log("Steam lobby created successfully.\nAttempting to host...");

                // Creating HOST -> host already created 
                //GameManager.Instance.TriggerEvent(GameManager.Event.ServerStart);

                // Setting HostAddress in Lobby Metadata
                lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
                SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
            }
        }

        // CLIENT ONLY
        private void OnLobbyJoinAttempt(LobbyEnter_t callback)
        {
            // Hacky way of confirming successful join
            if (callback.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
            {
                // Successfully joined host
                GameManager.Instance.TriggerEvent(GameManager.Event.SteamLobbyEntered);

                // Host is already connected to the server
                // No further networking work is needed
                // Create the lobbyUI and exit
                if (NetworkServer.active)
                {
                    Debug.Log("OnLobbyJoinAttempt callbck");

                    // Adds Host UI to Host Lobby
                    //SteamUI.GenerateLobbyHost(GetSteamFriend(SteamUser.GetSteamID()), true);
                    onLobbyHost?.Invoke(GetSteamFriend(SteamUser.GetSteamID()), true);
                    //SteamUI.GenerateLobbyEmpty();
                    onClearLobby?.Invoke();
                    return;
                }

                // Creating CLIENT
                string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
                Illu.Networking.NetworkManager.HostAddress = hostAddress;
                GameManager.Instance.TriggerEvent(GameManager.Event.ClientStart);

                // Lobby ID
                lobbyID = new CSteamID(callback.m_ulSteamIDLobby);

                // Adds Host UI to Client Lobby
                //SteamUI.GenerateLobbyHost(GetSteamFriend(SteamMatchmaking.GetLobbyOwner(lobbyID)), false);

                onLobbyHost?.Invoke(GetSteamFriend(SteamMatchmaking.GetLobbyOwner(lobbyID)), false);

                // Adds Clients UI to Client Lobby
                int lobbyCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
                for (int i = 0; i < lobbyCount; i++)
                {
                    CSteamID lobbyMember = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);

                    clientJoined?.Invoke(GetSteamFriend(lobbyMember), false);
                }
            }
        }

        // CLIENT invited by HOST
        private void OnLobbyInvited(LobbyInvite_t callback)
        {
            if (callback.m_ulGameID == SteamAppID.m_GameID)
            {
                GameManager.Instance.TriggerEvent(GameManager.Event.SteamLobbyInvited);
                SteamUserRecord steamFriend = GetSteamFriend(new CSteamID(callback.m_ulSteamIDUser));
                UIConsole.Log("Invite received from: " + steamFriend.name);
                //SteamUI.GenerateInvite(new CSteamID(callback.m_ulSteamIDLobby), steamFriend);
                onInviteReceived?.Invoke(new CSteamID(callback.m_ulSteamIDLobby), steamFriend);
            }
        }

        // CLIENT attemps to join via steam or steam invite
        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
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

                // Someone is trying to kick you
                if (message == SteamUser.GetSteamID().ToString())
                {
                    // The lobby owner has kicked you!
                    if (SteamMatchmaking.GetLobbyOwner(lobbyID) == new CSteamID(callback.m_ulSteamIDUser))
                    {
                        GameManager.Instance.TriggerEvent(GameManager.Event.SteamLobbyKicked);
                    }
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
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                    UIConsole.Log(string.Format("[Lobby]: {0} has joined.", steamUserChanged.name));
                    _steamLobbyClientMember = steamUserChanged;
                    //SteamUI.GenerateLobbyClient(steamUserChanged, true);
                    // OnLobbyClientJoined EVENT //
                    clientJoined?.Invoke(steamUserChanged, true);
                    break;
                // Left
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                    UIConsole.Log(string.Format("[Lobby]: {0} has left.", steamUserChanged.name));
                    //SteamUI.RemoveLobbyClient();
                    // OnLobbyClientRemoved EVENT //
                    OnLobbyClientRemoved?.Invoke();
                    break;
                // Kicked
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                    UIConsole.Log(string.Format("[Lobby]: {0} was kicked by {1}.", steamUserChanged.name, steamUserMakingChange.name));
                    //SteamUI.RemoveLobbyClient();
                    OnLobbyClientRemoved?.Invoke();
                    break;
                // Disconnected
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                    UIConsole.Log(string.Format("[Lobby]: {0} disconnected.", steamUserChanged.name));
                    //SteamUI.RemoveLobbyClient();
                    OnLobbyClientRemoved?.Invoke();
                    break;
                default:
                    break;
            }
        }

        /*  --------------------------
        *   Button activated functions
        *   -------------------------- */

        // USER becomes HOST
        public void HostLobby()
        {
            UIConsole.Log("Attempting to create steam lobby...");
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
        }

        private enum LobbyExitReason { Left, Kicked, Disconnected };
        public void LobbyLeft() => LeaveLobby(LobbyExitReason.Left);
        public void LobbyDisconnected()
        {
            ScreenController.Instance.ChangeScreen(ScreenController.Screen.Root);
            LeaveLobby(LobbyExitReason.Disconnected);
        }
        public void LobbyKicked() => LeaveLobby(LobbyExitReason.Kicked);

        // USER leaves lobby
        private void LeaveLobby(LobbyExitReason lobbyExitReason)
        {
            // Check if User is in a lobby
            if (lobbyID.m_SteamID == 0) return;

            // Check if User is the lobby owner
            bool lobbyOwner = SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(lobbyID);

            // Display the why the User left the lobby (if unprompted)
            switch (lobbyExitReason)
            {
                case LobbyExitReason.Left:
                    UIConsole.Log("You have left the lobby");
                    break;
                case LobbyExitReason.Kicked:
                    UIConsole.Log("You have been kicked from the lobby");
                    //UIManager.Error("Lobby Left", "You have been kicked from the lobby");
                    break;
                case LobbyExitReason.Disconnected:
                    UIConsole.Log("You have disconnected from the lobby");
                    //UIManager.Error("Lobby Left", "You have disconnected from the lobby");
                    break;
                default:
                    UIConsole.Log("You somehow left the lobby");
                    //UIManager.Error("Lobby Left", "You somehow left the lobby");
                    break;
            }

            // Leave the steam lobby
            SteamMatchmaking.LeaveLobby(lobbyID);
            lobbyID.Clear();

            // Destroy LobbyUI
            //SteamUI.DestroyLobby();
            onDestroyLobby?.Invoke();

            // Disconnect from any network connection (and close server if host)
            GameManager.Instance.TriggerEvent(lobbyOwner ? GameManager.Event.ServerStop : GameManager.Event.ClientStop);
           
     
        }

        // HOST invites CLIENT
        public void InviteToLobby(CSteamID friendID)
        {
            SteamMatchmaking.InviteUserToLobby(lobbyID, friendID);
        }

        // CLIENT joins HOST via invite
        public void JoinSteamLobby(CSteamID lobbyID)
        {
            SteamMatchmaking.JoinLobby(lobbyID);
        }

        // HOST kicks CLIENT
        public void KickUser(CSteamID steamID)
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

        public List<List<SteamUserRecord>> GetSteamFriends()
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
}