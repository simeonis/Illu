using UnityEngine;
using Steamworks;
using Mirror.FizzySteam;
using System.Collections.Generic;
using UnityEngine.Events;
using Illu.Networking;
using Illu.UI;

namespace Illu.Steam
{
    public class SteamManager : MonoBehaviourSingletonDontDestroy<SteamManager>
    {
        // Callbacks handled by SteamUI
        // [HideInInspector] public UnityEvent<SteamUserRecord> OnLobbyUserJoined = new UnityEvent<SteamUserRecord>();
        [HideInInspector] public UnityEvent OnLobbyUpdated = new UnityEvent();

        // Callbacks handled by Steam 
        protected Callback<LobbyCreated_t>           lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
        protected Callback<LobbyEnter_t>             lobbyEntered;
        protected Callback<LobbyChatUpdate_t>        lobbyChatUpdate;
        protected Callback<LobbyChatMsg_t>           lobbyChatMessage;

        // Networking
        const string hostAddressKey = "Host Address Key";

        // Steam IDs
        CGameID steamAppID;
        CSteamID lobbyID;

        void Start() => steamAppID = new CGameID(uint.Parse(GetComponent<FizzySteamworks>().SteamAppID));
        
        void OnEnable()
        {
            if (!global::SteamManager.Initialized) { return; }

            lobbyCreated       = Callback<LobbyCreated_t>.Create(OnLobbyCreateAttempt);
            lobbyEntered       = Callback<LobbyEnter_t>.Create(OnLobbyEnterAttempt);
            lobbyChatMessage   = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);
            lobbyChatUpdate    = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);

            GameManager.Instance.AddListener(GameManager.Event.GameLeft, LobbyLeft);
        }

        void OnDisable()
        {
            lobbyCreated.Dispose();
            lobbyEntered.Dispose();
            lobbyChatMessage.Dispose();
            lobbyChatUpdate.Dispose();
            lobbyJoinRequested.Dispose();

            GameManager.Instance.RemoveListener(GameManager.Event.GameLeft, LobbyLeft);
        }

        /*  --------------------------
        *       Callback functions
        *   -------------------------- */

        // HOST attempts to create lobby
        void OnLobbyCreateAttempt(LobbyCreated_t callback)
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

                // Setting HostAddress in Lobby Metadata
                lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
                SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAddressKey, SteamUser.GetSteamID().ToString());
                
                OnLobbyUpdated?.Invoke(); // Notify SteamUI to rebuild the lobby
            }
        }

        // CLIENT ONLY
        void OnLobbyEnterAttempt(LobbyEnter_t callback)
        {
            // Hacky way of confirming successful join
            if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
            {
                UIConsole.Log("Error occured while joining steam lobby.");
                return;
            }
            
            // Lobby ID
            lobbyID = new CSteamID(callback.m_ulSteamIDLobby);

            // Get lobby host
            var steamLobbyHost = GetSteamUserRecord(SteamMatchmaking.GetLobbyOwner(lobbyID));
            
            // User is not lobby host, START CLIENT
            if (steamLobbyHost.id != SteamUser.GetSteamID())
            {
                NetworkManager.Instance.HostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAddressKey);
                NetworkManager.Instance.StartClient();
                OnLobbyUpdated?.Invoke(); // Notify SteamUI to rebuild the lobby
            }
        }

        // CLIENT attemps to join via steam or steam invite (On Steam)
        void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        void OnLobbyChatMessage(LobbyChatMsg_t callback)
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
                        LobbyKicked();
                    }
                }
            }
        }

        void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            SteamUserRecord steamUserMakingChange = GetSteamUserRecord(new CSteamID(callback.m_ulSteamIDMakingChange));
            SteamUserRecord steamUserChanged = GetSteamUserRecord(new CSteamID(callback.m_ulSteamIDUserChanged));

            switch (callback.m_rgfChatMemberStateChange)
            {
                // Entered
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                    UIConsole.Log(string.Format("[Lobby]: {0} has joined.", steamUserChanged.name));
                    OnLobbyUpdated?.Invoke(); // Notify SteamUI to rebuild the lobby
                    break;
                // Left
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                    UIConsole.Log(string.Format("[Lobby]: {0} has left.", steamUserChanged.name));
                    OnLobbyUpdated?.Invoke(); // Notify SteamUI to rebuild the lobby
                    break;
                // Kicked
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                    UIConsole.Log(string.Format("[Lobby]: {0} was kicked by {1}.", steamUserChanged.name, steamUserMakingChange.name));
                    OnLobbyUpdated?.Invoke(); // Notify SteamUI to rebuild the lobby
                    break;
                // Disconnected
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                    UIConsole.Log(string.Format("[Lobby]: {0} disconnected.", steamUserChanged.name));
                    OnLobbyUpdated?.Invoke(); // Notify SteamUI to rebuild the lobby
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

        enum LobbyExitReason { Left, Kicked, Disconnected };
        void LobbyLeft() => LeaveLobby(LobbyExitReason.Left);
        public void LobbyDisconnected() => LeaveLobby(LobbyExitReason.Disconnected);
        void LobbyKicked() => LeaveLobby(LobbyExitReason.Kicked);

        // USER leaves lobby
        void LeaveLobby(LobbyExitReason lobbyExitReason)
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
                    // TODO: Show error message
                    break;
                case LobbyExitReason.Disconnected:
                    UIConsole.Log("You have disconnected from the lobby");
                    // TODO: Show error message
                    break;
                default:
                    UIConsole.Log("You somehow left the lobby");
                    // TODO: Show error message
                    break;
            }

            // Leave the steam lobby
            SteamMatchmaking.LeaveLobby(lobbyID);
            lobbyID.Clear();

            // Disconnect from any network connection (and close server if host)
            if (lobbyOwner)
                NetworkManager.Instance.StopHost();
            else
                NetworkManager.Instance.StopClient();
           
            ScreenController.Instance.ChangeScreen(ScreenController.Screen.Root);
        }

        // HOST invites CLIENT
        public void InviteToLobby(CSteamID friendID) => SteamMatchmaking.InviteUserToLobby(lobbyID, friendID);
    
        // CLIENT joins HOST via invite
        public void JoinSteamLobby(CSteamID lobbyID) => SteamMatchmaking.JoinLobby(lobbyID);
        

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

        Dictionary<EPersonaState, string> steamStatus = new Dictionary<EPersonaState, string>
        {
            {EPersonaState.k_EPersonaStateOffline, "Offline"},
            {EPersonaState.k_EPersonaStateOnline, "Online"},
            {EPersonaState.k_EPersonaStateBusy, "Online"},
            {EPersonaState.k_EPersonaStateAway, "Online"},
            {EPersonaState.k_EPersonaStateSnooze, "Online"},
            {EPersonaState.k_EPersonaStateLookingToTrade, "Online"},
            {EPersonaState.k_EPersonaStateLookingToPlay, "Online"},
        };

        SteamUserRecord GetSteamUserRecord(CSteamID steamID)
        {
            int userAvatar = SteamFriends.GetMediumFriendAvatar(steamID);
            string userName = SteamFriends.GetFriendPersonaName(steamID);
            string userStatus = steamStatus[SteamFriends.GetFriendPersonaState(steamID)];

            bool userPlaying = SteamFriends.GetFriendGamePlayed(steamID, out FriendGameInfo_t gameInfo_T);
            bool playingIllu = userPlaying ? (gameInfo_T.m_gameID.m_GameID == steamAppID.m_GameID) : false;
            if (playingIllu) userStatus = "Playing Illu";

            return new SteamUserRecord(steamID, userName, userStatus, userAvatar);
        }

        public List<SteamUserRecord> GetLobbyUsers()
        {
            // Lobby doesn't exist
            if (lobbyID == null) return new List<SteamUserRecord>();
            
            List<SteamUserRecord> lobbyUsers = new List<SteamUserRecord>();
            int lobbyCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
            
            // Loop over all users in the lobby
            for (int i = 0; i < lobbyCount; i++)
            {
                CSteamID lobbyMember = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
                lobbyUsers.Add(GetSteamUserRecord(lobbyMember));
            }

            return lobbyUsers;
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
                SteamUserRecord steamFriend = GetSteamUserRecord(friendSteamID);

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