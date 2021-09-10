using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamUI : MonoBehaviour
{
    [Header("Target Parent")]
    [SerializeField] private RectTransform friendList;
    [SerializeField] private RectTransform inviteList;
    [SerializeField] private RectTransform lobbyHost;
    [SerializeField] private RectTransform lobbyClient;

    [Header("Prefabs")]
    [SerializeField] private GameObject steamStatusTitlePrefab;
    [SerializeField] private GameObject steamFriendListPrefab;
    [SerializeField] private GameObject steamLobbyPrefab;
    [SerializeField] private GameObject steamInvitePrefab;

    [Header("Color")]
    [SerializeField] private Color statusPlaying;
    [SerializeField] private Color statusOnline;
    [SerializeField] private Color statusOffline;

    private List<string> status = new List<string>() { "Playing Illu", "Online", "Offline" };
    private Dictionary<CSteamID, GameObject> invites = new Dictionary<CSteamID, GameObject>();

    public void GenerateInvite(SteamLobby lobby, CSteamID lobbyID, SteamUserRecord steamFriend)
    {
        // Increase invite list vertical size
        inviteList.sizeDelta = new Vector2(inviteList.sizeDelta.x, inviteList.sizeDelta.y + 80);
        
        GameObject inviteUI = Instantiate(steamInvitePrefab, new Vector3(0, 80 - inviteList.sizeDelta.y), Quaternion.Euler(0, 0, 0));
        inviteUI.transform.SetParent(inviteList, false);
        SteamFriendInvite inviteDetails = inviteUI.GetComponent<SteamFriendInvite>();

        // Steam Avatar
        Texture2D tex = GetSteamImageAsTexture2D(steamFriend.avatar);
        inviteDetails.avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        // Steam Name
        inviteDetails.name.text = steamFriend.name;

        inviteDetails.acceptButton.onClick.AddListener(delegate { 
            lobby.JoinSteamLobby(lobbyID);
            DestroyInvite(inviteUI);
        });

        inviteDetails.declineButton.onClick.AddListener(delegate { 
            DestroyInvite(inviteUI);
        });
    }

    private void DestroyInvite(GameObject invite)
    {
        Destroy(invite);

        // Decrease invite list vertical size
        inviteList.sizeDelta = new Vector2(inviteList.sizeDelta.x, inviteList.sizeDelta.y - 80);
    }

    public void GenerateLobbyFriend(SteamUserRecord steamFriend, bool isHost)
    {
        GameObject friendUI = Instantiate(steamLobbyPrefab);
        if (isHost) friendUI.transform.SetParent(lobbyHost, false);
        else friendUI.transform.SetParent(lobbyClient, false);

        SteamFriendLobby lobbyFriendDetails = friendUI.GetComponent<SteamFriendLobby>();

        // Steam Avatar
        Texture2D tex = GetSteamImageAsTexture2D(steamFriend.avatar);
        lobbyFriendDetails.avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        // Steam Name
        lobbyFriendDetails.name.text = steamFriend.name;
    }

    public void DestroyLobbyFriend()
    {

    }

    public void GenerateFriendList(SteamLobby lobby, List<List<SteamUserRecord>> steamFriends)
    {
        float totalHeight = 0;
        float itemWidth = 500;
        float itemHeight = 90;
        int counterY = -1;

        float numberOfTypes = steamFriends.Count;
        for (int i = 0; i < numberOfTypes; i++)
        {
            totalHeight += itemHeight;

            // Create and position Title
            Vector3 titlePosition = new Vector3(0, ++counterY * -itemHeight, 0);
            GameObject titleUI = Instantiate(steamStatusTitlePrefab, titlePosition, Quaternion.Euler(0, 0, 0));
            titleUI.transform.SetParent(friendList, false);

            // Set Title text
            titleUI.GetComponent<SteamStatusTitle>().status.text = status[i];
            
            float numberOfItems = steamFriends[i].Count;
            for (int j = 0; j < numberOfItems; j++)
            {
                // Increase every 3 times
                if (j % 3 == 0)
                {
                    counterY++;
                    totalHeight += itemHeight;
                }
                float positionY = counterY * itemHeight;

                float paddingX = (j % 3 + 1) * 25;
                float positionX = (j % 3) * itemWidth + paddingX;

                Vector3 friendPosition = new Vector3(positionX, -positionY, 0);
                GameObject friendUI = Instantiate(steamFriendListPrefab, friendPosition, Quaternion.Euler(0, 0, 0));
                friendUI.transform.SetParent(friendList, false);
                
                SteamUserRecord steamFriend = steamFriends[i][j];
                SteamFriendList steamFriendDetails = friendUI.GetComponent<SteamFriendList>();

                // Steam Avatar
                Texture2D tex = GetSteamImageAsTexture2D(steamFriend.avatar);
                steamFriendDetails.avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

                // Steam Name
                steamFriendDetails.name.text = steamFriend.name;

                // Steam Status
                steamFriendDetails.status.text = steamFriend.status;
                if (steamFriend.status == "Online")
                    steamFriendDetails.status.color = statusOnline;
                else if (steamFriend.status == "Offline")
                    steamFriendDetails.status.color = statusOffline;
                else steamFriendDetails.status.color = statusPlaying;

                // Steam Invite Button
                CSteamID id = steamFriend.id;
                steamFriendDetails.inviteButton.onClick.AddListener(delegate { lobby.InviteToLobby(id); });
            }
        }

        friendList.sizeDelta = new Vector2(0, totalHeight);
    }

    public void DestroyFriendList()
    {
        foreach (Transform child in friendList.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /*  --------------------------
    *        Helper functions
    *   -------------------------- */

    private Texture2D GetSteamImageAsTexture2D(int iImage)
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
                ret = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
                ret.LoadRawTextureData(Image);
                ret.Apply();
            }
        }

        return ret;
    }
}
