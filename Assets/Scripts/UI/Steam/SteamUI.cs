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
        [SerializeField] RectTransform lobbyHost;
        [SerializeField] RectTransform lobbyClient;
        
        [Header("Prefabs")]
        [SerializeField] GameObject steamStatusTitlePrefab;
        [SerializeField] GameObject steamFriendListPrefab;
        [SerializeField] GameObject steamLobbyPrefab;
        [SerializeField] GameObject steamEmptyLobbyPrefab;

        // Make a Enum
        List<string> _status = new List<string>() { "Playing Illu", "Online", "Offline" };
        List<SteamFriendLobby> playerCards = new List<SteamFriendLobby>();

        void OnEnable()
        {
            SteamManager.Instance.OnLobbyUserJoined.AddListener(GenerateLobbyClient);
            SteamManager.Instance.OnLobbyUserLeft.AddListener(GenerateLobbyUsers);

            ReadyUpSystem.Instance.OneReady.AddListener(SetPlayerOneStatus);
            ReadyUpSystem.Instance.TwoReady.AddListener(SetPlayerTwoStatus);
        }

        void OnDisable()
        {

            SteamManager.Instance.OnLobbyUserJoined.RemoveListener(GenerateLobbyClient);
            SteamManager.Instance.OnLobbyUserLeft.RemoveListener(GenerateLobbyUsers);

            ReadyUpSystem.Instance.OneReady.RemoveListener(SetPlayerOneStatus);
            ReadyUpSystem.Instance.TwoReady.RemoveListener(SetPlayerTwoStatus); 
        }

        override public void OnStartClient()
        {
            base.OnStartClient();

            if(!Networking.NetworkManager.Instance.isLanConnection)
                GenerateLobbyUsers();
        }

        void GenerateLobbyUsers()
        {
            DestroyLobby();
            List<SteamUserRecord> lobbyMembers = SteamManager.Instance.GetLobbyUsers();
            for(int i = 0; i < lobbyMembers.Count; i++)
                GenerateLobbyFriend(lobbyMembers[i], isServer, i == 0);
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

        void GenerateLobbyClient(SteamUserRecord steamFriend) => GenerateLobbyFriend(steamFriend, isServer, false);

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

        void SetPlayerOneStatus(bool status) => SetIndicatorOnPlayerCard(status, 0);
        void SetPlayerTwoStatus(bool status) => SetIndicatorOnPlayerCard(status, 1);

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
