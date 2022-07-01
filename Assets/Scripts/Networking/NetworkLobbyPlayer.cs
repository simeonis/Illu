using Mirror;
using UnityEngine;
using Illu.Steam;

public class NetworkLobbyPlayer : NetworkBehaviour
{
    [SerializeField] GameObject canvas;
 
    public override void OnStartAuthority()
    {  
        canvas.SetActive(true);
        bool isLan = Illu.Networking.NetworkManager.Instance.isLanConnection;
        GetComponent<SteamUI>().enabled = !isLan;
        GetComponent<LanUI>().enabled = isLan;
    }

    public override void OnStartClient() => Illu.Networking.NetworkManager.Instance.LobbyPlayers.Add(this);
    
    public override void OnStopClient() 
    {
        Illu.Networking.NetworkManager.Instance.LobbyPlayers.Remove(this);
        Destroy(gameObject);
    }
}
