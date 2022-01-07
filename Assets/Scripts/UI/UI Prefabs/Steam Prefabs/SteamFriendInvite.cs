using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Illu.Steam;
using Steamworks;

public class SteamFriendInvite : MonoBehaviour
{
    // Background Color
    public Image background;

    // Steam Avatar
    public Image avatar;

    // Steam Name
    public TMP_Text steamName;

    // Accept Button
    public Button acceptButton;

    // Decline Button
    public Button declineButton;

    public void Instantiate(SteamUserRecord user)
    {
        // Steam Avatar
        Texture2D tex = SteamUI.GetSteamImageAsTexture2D(user.avatar);
        avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        // Steam Name
        steamName.text = user.name;
    }
}
