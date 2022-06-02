using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Illu.Steam;

[System.Serializable]
public class SteamFriendLobby : MonoBehaviour
{
    // Background Color
    Image background;

    // Steam Avatar
    [SerializeField] Image avatar;

    // Steam Name
    [SerializeField] TMP_Text steamName;

    // Kick Button
    public Button removeButton;

    private bool canKick = true;

    public void Instantiate(SteamUserRecord user, Texture2D avatarTex, bool canKick, UnityEngine.Events.UnityAction kick)
    {
        // Prefab name
        this.name = user.id.ToString();


        avatar.sprite = Sprite.Create(
            avatarTex,
            new Rect(0.0f, 0.0f,
            avatarTex.width,
            -avatarTex.height),
             new Vector2(0.5f, 0.5f), 100.0f
            );

        // Steam Name
        steamName.text = user.name;
    }
}
