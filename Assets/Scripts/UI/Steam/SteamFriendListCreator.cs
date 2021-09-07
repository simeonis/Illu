using UnityEngine;
using System.Collections.Generic;
using Steamworks;

public class SteamFriendListCreator : MonoBehaviour
{
    [Header("Target Parent")]
    [SerializeField] private RectTransform content;

    [Header("Prefabs")]
    [SerializeField] private GameObject steamStatusTitlePrefab;
    [SerializeField] private GameObject steamFriendListPrefab;

    [Header("Color")]
    [SerializeField] private Color statusPlaying;
    [SerializeField] private Color statusOnline;
    [SerializeField] private Color statusOffline;

    private List<string> status = new List<string>() { "Playing Illu", "Online", "Offline" };

    public void GenerateList(List<List<SteamUserRecord>> steamFriends, SteamLobby lobby)
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
            titleUI.transform.SetParent(content, false);

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
                friendUI.transform.SetParent(content, false);
                
                SteamUserRecord steamFriend = steamFriends[i][j];
                SteamFriendList steamFriendUI = friendUI.GetComponent<SteamFriendList>();

                // Steam Avatar
                Texture2D tex = SteamUtility.GetSteamImageAsTexture2D(steamFriend.Avatar);
                steamFriendUI.avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

                // Steam Name
                steamFriendUI.name.text = steamFriend.Name;

                // Steam Status
                steamFriendUI.status.text = steamFriend.Status;
                if (steamFriend.Status == "Online")
                    steamFriendUI.status.color = statusOnline;
                else if (steamFriend.Status == "Offline")
                    steamFriendUI.status.color = statusOffline;
                else steamFriendUI.status.color = statusPlaying;

                // Steam Invite Button
                CSteamID id = steamFriend.ID;
                steamFriendUI.inviteButton.onClick.AddListener(delegate { lobby.InviteToLobby(id); });
            }
        }

        content.sizeDelta = new Vector2(0, totalHeight);
    }

    public void DeleteList()
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
    }
}