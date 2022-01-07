using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Illu.Steam;

public class SteamFriendLobby : MonoBehaviour
{
    // Background Color
    public Image background;

    // Steam Avatar
    public Image avatar;

    // Steam Name
    public TMP_Text steamName;

    // Kick Button
    public Button removeButton;

    public void Instantiate(SteamUserRecord user)
    {
        // Prefab name
        this.name = user.id.ToString();

        // Steam Avatar
        Texture2D tex = SteamUI.GetSteamImageAsTexture2D(user.avatar);
        avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        // Steam Name
        steamName.text = user.name;
    }
}
