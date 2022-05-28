using UnityEngine;
using UnityEngine.UI;
using TMPro;
// using Illu.Steam;
// using Steamworks;

[System.Serializable]
public class SteamFriendInvite : MonoBehaviour
{
    // Background Color
    Image background;

    // Steam Avatar
    Image avatar;

    // Steam Name
    TMP_Text steamName;

    // Accept Button
    Button acceptButton;

    // Decline Button
    Button declineButton;

    public void Instantiate(string name, Texture2D avatarTex, UnityEngine.Events.UnityAction accept, UnityEngine.Events.UnityAction decline)
    {
        avatar.sprite = Sprite.Create(avatarTex, new Rect(0.0f, 0.0f, avatarTex.width, -avatarTex.height), new Vector2(0.5f, 0.5f), 100.0f);
        // Steam Name
        steamName.text = name;
        acceptButton.onClick.AddListener(accept);
        declineButton.onClick.AddListener(decline);
    }
}
