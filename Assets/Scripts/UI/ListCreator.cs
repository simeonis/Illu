using UnityEngine;
using System.Collections.Generic;
using Steamworks;

public class ListCreator : MonoBehaviour
{

    [SerializeField]
    private Transform SpawnPoint = null;

    [SerializeField]
    private GameObject item = null;

    [SerializeField]
    private RectTransform content = null;

    private int numberOfItems = 3;

    public void renderList(List<SteamUserRecord> steamFriends, SteamLobby lobby)
    {
        Debug.Log("List Creator render List Called");
        numberOfItems = steamFriends.Count;

        //setContent Holder Height;
        content.sizeDelta = new Vector2(0, numberOfItems * 60);

        for (int i = 0; i < numberOfItems; i++)
        {
            // 60 width of item
            float spawnY = i * 60;

            //newSpawn Position
            Vector3 pos = new Vector3(0, -spawnY, SpawnPoint.position.z);
            //instantiate item
            GameObject SpawnedItem = Instantiate(item, pos, SpawnPoint.rotation);
            //setParent
            SpawnedItem.transform.SetParent(SpawnPoint, false);
            //get ItemDetails Component
            ItemDetails itemDetails = SpawnedItem.GetComponent<ItemDetails>();
            //set name
            itemDetails.text.text = steamFriends[i].Name;

            Texture2D tex = GetSteamImageAsTexture2D(steamFriends[i].Avatar);
            //set image
            itemDetails.image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

            Debug.Log("steamFriends[i].ID" + steamFriends[i].ID);

            CSteamID id = steamFriends[i].ID;

            itemDetails.button.onClick.AddListener(delegate { lobby.InviteToLobby(id); });

        }
    }

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
                ret = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
                ret.LoadRawTextureData(Image);
                ret.Apply();
            }
        }

        return ret;
    }
}