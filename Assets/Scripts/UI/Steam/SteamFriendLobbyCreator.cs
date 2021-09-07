using UnityEngine;

public class SteamFriendLobbyCreator : MonoBehaviour
{
    [Header("Target Parent")]
    [SerializeField] private RectTransform host;
    [SerializeField] private RectTransform client;

    [Header("Prefabs")]
    [SerializeField] private GameObject steamFriendUIPrefab;

    public void AddLobbyFriend(SteamUserRecord steamFriend, bool isHost)
    {
        GameObject friendUI = Instantiate(steamFriendUIPrefab);
        if (isHost) friendUI.transform.SetParent(host, false);
        else friendUI.transform.SetParent(client, false);

        SteamFriendLobby steamFriendUI = friendUI.GetComponent<SteamFriendLobby>();

        // Steam Avatar
        Texture2D tex = SteamUtility.GetSteamImageAsTexture2D(steamFriend.Avatar);
        steamFriendUI.avatar.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        // Steam Name
        steamFriendUI.name.text = steamFriend.Name;
    }

    public void RemoveLobbyFriend()
    {

    }
}
