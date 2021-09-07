using UnityEngine;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public struct SCREENS {
        public GameObject Root;
        public GameObject Play;
        public GameObject Host;
        public GameObject Friend;
        public GameObject Join;
        public GameObject Settings;
    }

    [SerializeField] private SCREENS screens;

    public void Quit()
    {
        Application.Quit();
    }

    public void HostGameFailed()
    {
        screens.Host.SetActive(false);
        screens.Play.SetActive(true);
    }

    // CLIENT joined HOST
    public void JoinedHost()
    {
        // Show HOST in CLIENT's Lobby (top right)
    }

    // HOST received CLIENT
    public void ClientJoinedHost()
    {
        // Show CLIENT in HOST's Lobby (top right)
    }

    // TODO: Add SteamRecord parameter so CLIENT knows which HOST invited them
    public void InviteReceived()
    {
        // Enable "Accept Invite Button"
    }

    public void GameStarted()
    {
        //lobbyUI.SetActive(false);
    }
}
