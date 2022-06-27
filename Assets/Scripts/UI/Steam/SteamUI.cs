using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

namespace Illu.Steam
{
    public class SteamUI : Mirror.NetworkBehaviour
    {
        [Header("Target Parent")]
        [SerializeField] RectTransform friendList;
        [SerializeField] RectTransform inviteList;
        [SerializeField] RectTransform lobbyHost;
        [SerializeField] RectTransform lobbyClient;
        
        [Header("Prefabs")]
        [SerializeField] GameObject steamStatusTitlePrefab;
        [SerializeField] GameObject steamFriendListPrefab;
        [SerializeField] GameObject steamLobbyPrefab;
        [SerializeField] GameObject steamEmptyLobbyPrefab;
        [SerializeField] GameObject steamInvitePrefab;

        // Make a Enum
        List<string> _status = new List<string>() { "Playing Illu", "Online", "Offline" };
        Dictionary<string, GameObject> _invites = new Dictionary<string, GameObject>();
        List<SteamFriendLobby> playerCards = new List<SteamFriendLobby>();

        void OnEnable()
        {
            SteamManager.Instance.clientJoined += GenerateLobbyClient;
            SteamManager.Instance.onLobbyHost += GenerateLobbyHost;
            SteamManager.Instance.onInviteReceived += GenerateInvite;
            SteamManager.Instance.onClearLobby.AddListener(GenerateLobbyEmpty);
            SteamManager.Instance.onDestroyLobby.AddListener(DestroyLobby);
            SteamManager.Instance.OnLobbyClientRemoved.AddListener(RemoveLobbyClient);

            ReadyUpSystem.Instance.OneReady.AddListener(setPlayerOneStatus);
            ReadyUpSystem.Instance.TwoReady.AddListener(setPlayerTwoStatus);
        }

        void OnDisable()
        {
            SteamManager.Instance.clientJoined -= GenerateLobbyClient;
            SteamManager.Instance.onLobbyHost -= GenerateLobbyHost;
            SteamManager.Instance.onInviteReceived -= GenerateInvite;
            SteamManager.Instance.onClearLobby.RemoveListener(GenerateLobbyEmpty);
            SteamManager.Instance.onDestroyLobby.RemoveListener(DestroyLobby);
            SteamManager.Instance.OnLobbyClientRemoved.RemoveListener(RemoveLobbyClient);

            ReadyUpSystem.Instance.OneReady.RemoveListener(setPlayerOneStatus);
            ReadyUpSystem.Instance.TwoReady.RemoveListener(setPlayerTwoStatus); 
        }

        override public void OnStartClient()
        {
            Debug.Log("OnStartClient Steam UI");
            if(!Networking.NetworkManager.Instance.isLanConnection && !isServer)
            {
                Debug.Log("OnClientConnect not on LAN");
                Debug.Log("SteamManager.Instance.steamLobbyClient name " + SteamManager.Instance.steamLobbyClient.name);
                GenerateLobbyHost(SteamManager.Instance.steamLobbyHost, false);
                GenerateLobbyClient(SteamManager.Instance.steamLobbyClient, false);
            }
        }

        // Invited To lobby
        void GenerateInvite(CSteamID lobbyID, SteamUserRecord steamFriend)
        {
            Debug.Log("steamIDstring " + steamFriend.id);
            string steamIDstring = steamFriend.id.ToString();

            // Invite exists
            if (_invites.ContainsKey(steamIDstring))
            {
                // Re-arrange Order
                _invites[steamIDstring].transform.SetAsFirstSibling();
            }
            // New invite
            else
            {
                // Create GameObject
                GameObject invite = Instantiate(steamInvitePrefab, inviteList);
                invite.name = steamIDstring;
                invite.transform.SetAsFirstSibling();

                // Add to dictionary
                _invites.Add(invite.name, invite);

                // Retrieve Prefab's Script and fill out details
                SteamFriendInvite inviteDetails = invite.GetComponent<SteamFriendInvite>();

                Texture2D tex = GetSteamImageAsTexture2D(steamFriend.avatar);

                inviteDetails.Instantiate(
                    steamFriend.name,
                    tex,
                    delegate
                    {
                        SteamManager.Instance.JoinSteamLobby(lobbyID);
                        DestroyInvite(invite);
                    },
                    delegate
                    {
                        DestroyInvite(invite);
                    }
                 );
            }
        }

        //triggered on Lobby Join Attempt
        void GenerateLobbyHost(SteamUserRecord steamFriend, bool serverside)
        {
            Debug.Log("onLobbyHost => GenerateLobbyHost " + steamFriend.name);
            GenerateLobbyFriend(steamFriend, serverside, true);
        }

        // triggered on Lobby Join Attempt 
        void GenerateLobbyEmpty()
        {
            GameObject lobbyEmpty = Instantiate(steamEmptyLobbyPrefab, lobbyClient);
            SteamEmptyLobby lobbyEmptyDetails = lobbyEmpty.GetComponent<SteamEmptyLobby>();
            lobbyEmptyDetails.addButton.onClick.AddListener(delegate
            {
                GenerateFriendList();
            });
        }

        // Should be driven by event
        void RemoveLobbyClient()
        {
            DestroyLobbyClient();
            GenerateLobbyEmpty();
        }

        //***********************************************************************************************************

        void DestroyInvite(GameObject invite)
        {
            // Remove from dictionary
            _invites.Remove(invite.name);
            // Destroy GameObject
            Destroy(invite);
        }

        void GenerateLobbyClient(SteamUserRecord steamFriend, bool serverside)
        {
            Debug.Log("ClientJoined => GenerateLobbyClient");
            GenerateLobbyFriend(steamFriend, serverside, false);
        }

        void GenerateLobbyFriend(SteamUserRecord steamFriend, bool serverside, bool hostSlot)
        {
            GameObject lobbyUser = Instantiate(steamLobbyPrefab, hostSlot ? lobbyHost : lobbyClient);
            if (!hostSlot && lobbyClient.childCount > 0) Destroy(lobbyClient.GetChild(0).gameObject); // Destroy EmptyLobbyFriend

            SteamFriendLobby lobbyUserDetails = lobbyUser.GetComponent<SteamFriendLobby>();
            Texture2D tex = GetSteamImageAsTexture2D(steamFriend.avatar);

            lobbyUserDetails.Instantiate(
                steamFriend,
                tex,
                (serverside && !hostSlot),
                delegate
                {
                    SteamManager.Instance.KickUser(steamFriend.id);
                }
            );

            playerCards.Add(lobbyUserDetails);
        }

        void DestroyLobby()
        {
            DestroyLobbyHost();
            DestroyLobbyClient();

            playerCards.Clear();
        }

        void DestroyLobbyHost()
        {
            if (lobbyHost.childCount > 0)
            {
                foreach (Transform child in lobbyHost) Destroy(child.gameObject);
            }
        }

        void DestroyLobbyClient()
        {
            if (lobbyClient.childCount > 0)
            {
                foreach (Transform child in lobbyClient) Destroy(child.gameObject);
            }
        }

        public void GenerateFriendList()
        {
            DestroyFriendList();

            //access friend list 
            var steamFriends = SteamManager.Instance.GetSteamFriends();

            GameObject subList = null;
            float numberOfTypes = steamFriends.Count;
            for (int i = 0; i < numberOfTypes; i++)
            {
                // Create and position Title
                GameObject titleUI = Instantiate(steamStatusTitlePrefab, friendList);
                titleUI.GetComponent<SteamStatusTitle>().status.text = _status[i];

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
                        horizontalGroup.childAlignment = TextAnchor.UpperCenter;
                    }

                    SteamUserRecord steamFriend = steamFriends[i][j];
                    GameObject friendUI = Instantiate(steamFriendListPrefab, subList.transform);
                    SteamFriendList steamFriendDetails = friendUI.GetComponent<SteamFriendList>();

                    Texture2D avatar = GetSteamImageAsTexture2D(steamFriend.avatar);
                    steamFriendDetails.Instantiate(steamFriend, avatar, (j % 2 == 0));
                }
            }
        }

        void DestroyFriendList()
        {
            foreach (Transform child in friendList.transform)
            {
                Destroy(child.gameObject);
            }
        }

        void setPlayerOneStatus(bool status) => SetIndicatorOnPlayerCard(status, 0);
        void setPlayerTwoStatus(bool status) => SetIndicatorOnPlayerCard(status, 1);

        void SetIndicatorOnPlayerCard(bool status, int caller)
        {
            if (caller < playerCards.Count)
                playerCards[caller].SetIndicator(status);
        }

        // /*  --------------------------
        // *        Helper functions
        // *   -------------------------- */
        Texture2D GetSteamImageAsTexture2D(int iImage)
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
                    ret = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, false);
                    ret.LoadRawTextureData(Image);
                    ret.Apply();
                }
            }
            return ret;
        }
    }
}
