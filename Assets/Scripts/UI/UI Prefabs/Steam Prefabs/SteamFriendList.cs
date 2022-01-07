using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Illu.Steam;

public class SteamFriendList : MonoBehaviour
{
    // Background Color
    public Image background;

    // Steam Avatar
    public Image avatar;

    // Steam Name
    public TMP_Text steamName;

    // Steam Status
    public TMP_Text status;

    // Invite Button
    public Button inviteButton;

    [Header("Color")]
    [SerializeField] private Color statusPlaying;
    [SerializeField] private Color statusOnline;
    [SerializeField] private Color statusOffline;
    [SerializeField] private Color evenFriend;
    [SerializeField] private Color oddFriend;

    public void Instantiate(SteamUserRecord steamFriend, bool even)
    {
        // Background Color
        background.color = even ? evenFriend : oddFriend;

        // Steam Avatar
        Texture2D tex = Illu.Steam.SteamUI.GetSteamImageAsTexture2D(steamFriend.avatar);
        avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        // Steam Name
        steamName.text = steamFriend.name;

        // Steam Status
        status.text = steamFriend.status;
        if (steamFriend.status == "Online")
            status.color = statusOnline;
        else if (steamFriend.status == "Offline")
            status.color = statusOffline;
        else status.color = statusPlaying;

        // Invite Button
        inviteButton.onClick.AddListener(delegate { 
            Illu.Steam.SteamManager.InviteToLobby(steamFriend.id);
        });
    }
}