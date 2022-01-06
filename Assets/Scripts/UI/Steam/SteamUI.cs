using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

namespace Illu.Steam {
    public class SteamUI : MonoBehaviour
    {
        // Events
        [Header("Events")]
        [SerializeField] private Event E_HostScreen;
        [SerializeField] private Event E_ShowFriends;

        [Header("Target Parent")]
        [SerializeField] private RectTransform friendList;
        [SerializeField] private RectTransform inviteList;
        [SerializeField] private RectTransform lobbyHost;
        [SerializeField] private RectTransform lobbyClient;

        [Header("Prefabs")]
        [SerializeField] private GameObject steamStatusTitlePrefab;
        [SerializeField] private GameObject steamFriendListPrefab;
        [SerializeField] private GameObject steamLobbyPrefab;
        [SerializeField] private GameObject steamEmptyLobbyPrefab;
        [SerializeField] private GameObject steamInvitePrefab;

        private static List<string> status = new List<string>() { "Playing Illu", "Online", "Offline" };
        private static Dictionary<string, GameObject> invites = new Dictionary<string, GameObject>();

        public void GenerateInvite(CSteamID lobbyID, SteamUserRecord steamFriend)
        {
            string steamIDstring = steamFriend.id.ToString();

            // Invite exists
            if (invites.ContainsKey(steamIDstring))
            {
                // Re-arrange Order
                invites[steamIDstring].transform.SetAsFirstSibling();
            }
            // New invite
            else 
            {
                // Create GameObject
                GameObject invite = Instantiate(steamInvitePrefab, inviteList);
                invite.name = steamIDstring;
                invite.transform.SetAsFirstSibling();

                // Add to dictionary
                invites.Add(invite.name, invite);

                // Retrieve Prefab's Script and fill out details
                SteamFriendInvite inviteDetails = invite.GetComponent<SteamFriendInvite>();
                inviteDetails.Instantiate(steamFriend);

                // Accept Button
                inviteDetails.acceptButton.onClick.AddListener(delegate { 
                    SteamManager.JoinSteamLobby(lobbyID);
                    E_HostScreen.Trigger();
                    DestroyInvite(invite);
                });

                // Decline Button
                inviteDetails.declineButton.onClick.AddListener(delegate { 
                    DestroyInvite(invite);
                });
            }
        }

        private void DestroyInvite(GameObject invite)
        {
            // Remove from dictionary
            invites.Remove(invite.name);

            // Destroy GameObject
            Destroy(invite);
        }

        public void GenerateLobbyHost(SteamUserRecord steamFriend, bool serverside)
        {
            GenerateLobbyFriend(steamFriend, serverside, true);
        }

        public void GenerateLobbyClient(SteamUserRecord steamFriend, bool serverside)
        {
            GenerateLobbyFriend(steamFriend, serverside, false);
        }

        private void GenerateLobbyFriend(SteamUserRecord steamFriend, bool serverside, bool hostSlot)
        {
            GameObject lobbyUser = Instantiate(steamLobbyPrefab, hostSlot ? lobbyHost : lobbyClient);
            if (!hostSlot && lobbyClient.childCount > 0) Destroy(lobbyClient.GetChild(0).gameObject); // Destroy EmptyLobbyFriend

            SteamFriendLobby lobbyUserDetails = lobbyUser.GetComponent<SteamFriendLobby>();
            lobbyUserDetails.Instantiate(steamFriend);

            // Kick Button (Only visible server-side and only applies to clients)
            bool canKick = serverside && !hostSlot;
            lobbyUserDetails.removeButton.gameObject.SetActive(canKick);
            if (canKick)
            {
                lobbyUserDetails.removeButton.onClick.AddListener(delegate {
                    SteamManager.KickUser(steamFriend.id);
                });
            }
        }

        public void GenerateLobbyEmpty()
        {
            GameObject lobbyEmpty = Instantiate(steamEmptyLobbyPrefab, lobbyClient);
            SteamEmptyLobby lobbyEmptyDetails = lobbyEmpty.GetComponent<SteamEmptyLobby>();
            lobbyEmptyDetails.addButton.onClick.AddListener(delegate { 
                GenerateFriendList(SteamManager.GetSteamFriends());
                E_ShowFriends.Trigger();
            });
        }

        public void DestroyLobby()
        {
            DestroyLobbyHost();
            DestroyLobbyClient();
        }

        public void DestroyLobbyHost()
        {
            if (lobbyHost.childCount > 0)
            {
                foreach (Transform child in lobbyHost) Destroy(child.gameObject);
            }
        }

        public void DestroyLobbyClient()
        {
            if (lobbyClient.childCount > 0)
            {
                foreach (Transform child in lobbyClient) Destroy(child.gameObject);
            }
        }

        public void RemoveLobbyClient()
        {
            DestroyLobbyClient();
            GenerateLobbyEmpty();
        }

        private void GenerateFriendList(List<List<SteamUserRecord>> steamFriends)
        {
            GameObject subList = null;
            float numberOfTypes = steamFriends.Count;
            for (int i = 0; i < numberOfTypes; i++)
            {
                // Create and position Title
                GameObject titleUI = Instantiate(steamStatusTitlePrefab, friendList);
                titleUI.GetComponent<SteamStatusTitle>().status.text = status[i];
                
                float numberOfItems = steamFriends[i].Count;
                for (int j = 0; j < numberOfItems; j++)
                {
                    // Increase every 3 times
                    if (j % 3 == 0)
                    {
                        subList = new GameObject("Horizontal Group #" + (j + 1));
                        subList.transform.SetParent(friendList, false);
                        HorizontalLayoutGroup horizontalGroup  = subList.gameObject.AddComponent<HorizontalLayoutGroup>();
                        horizontalGroup.childForceExpandWidth = false;
                        horizontalGroup.childControlWidth = false;
                        horizontalGroup.childControlHeight = true;
                    }

                    SteamUserRecord steamFriend = steamFriends[i][j];
                    GameObject friendUI = Instantiate(steamFriendListPrefab, subList.transform);
                    SteamFriendList steamFriendDetails = friendUI.GetComponent<SteamFriendList>();
                    steamFriendDetails.Instantiate(steamFriend, (j % 2 == 0));
                }
            }
        }

        private void DestroyFriendList()
        {
            foreach (Transform child in friendList.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // /*  --------------------------
        // *        Helper functions
        // *   -------------------------- */
        public static Texture2D GetSteamImageAsTexture2D(int iImage)
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
