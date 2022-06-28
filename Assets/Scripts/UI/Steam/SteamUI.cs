using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

namespace Illu.Steam
{
    public class SteamUI : Mirror.NetworkBehaviour
    {
        [Header("Page")]
        [SerializeField] GameObject friendPage;

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
            SteamManager.Instance.OnLobbyUpdated.AddListener(GenerateLobby);

            ReadyUpSystem.Instance.OneReady.AddListener(SetPlayerOneStatus);
            ReadyUpSystem.Instance.TwoReady.AddListener(SetPlayerTwoStatus);
        }

        void OnDisable()
        {
            SteamManager.Instance.OnLobbyUpdated.RemoveListener(GenerateLobby);

            ReadyUpSystem.Instance.OneReady.RemoveListener(SetPlayerOneStatus);
            ReadyUpSystem.Instance.TwoReady.RemoveListener(SetPlayerTwoStatus); 
        }

        void GenerateLobby()
        {
            DestroyLobby();
            GenerateEmptyLobby();

            List<SteamUserRecord> lobbyMembers = SteamManager.Instance.GetLobbyUsers();

            for(int i = 0; i < lobbyMembers.Count; i++)
                GenerateLobbyUser(lobbyMembers[i], isServer, i == 0);
        }

        void GenerateEmptyLobby()
        {
            GameObject lobbyEmpty = Instantiate(steamEmptyLobbyPrefab, lobbyClient);
            SteamEmptyLobby lobbyEmptyDetails = lobbyEmpty.GetComponent<SteamEmptyLobby>();
            lobbyEmptyDetails.addButton.onClick.AddListener(delegate
            {
                OpenFriendList();
            });
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
                foreach (Transform child in lobbyHost) Destroy(child.gameObject);
        }

        void DestroyLobbyClient()
        {
            if (lobbyClient.childCount > 0)
                foreach (Transform child in lobbyClient) Destroy(child.gameObject);
        }

        void GenerateLobbyUser(SteamUserRecord steamUser, bool isServer, bool isHost)
        {
            GameObject lobbyUser = Instantiate(steamLobbyPrefab, isHost ? lobbyHost : lobbyClient);
            if (!isHost && lobbyClient.childCount > 0) Destroy(lobbyClient.GetChild(0).gameObject); // Destroy EmptyLobbyFriend

            SteamFriendLobby lobbyUserDetails = lobbyUser.GetComponent<SteamFriendLobby>();
            Texture2D tex = GetSteamImageAsTexture2D(steamUser.avatar);

            lobbyUserDetails.Instantiate(
                steamUser,
                tex,
                (isServer && !isHost),
                delegate
                {
                    SteamManager.Instance.KickUser(steamUser.id);
                }
            );

            playerCards.Add(lobbyUserDetails);
        }

        public void OpenFriendList()
        {
            GenerateFriendList();
            friendPage.SetActive(true);
        }

        public void CloseFriendList()
        {
            friendPage.SetActive(false);
            DestroyFriendList();
        }

        void GenerateFriendList()
        {
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

        // *  -------------------------- * \\
        // *        Helper functions     * \\
        // *  -------------------------- * \\
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
