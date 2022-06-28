using Mirror;
using UnityEngine;
using Illu.Steam;

public class NetworkLobbyPlayer : NetworkBehaviour
{
    [SerializeField] GameObject canvas;

    override public void OnStartAuthority()
    {  
        canvas.SetActive(true);
        RequestID(gameObject);
    }

    public override void OnStartClient() 
    {
        Illu.Networking.NetworkManager.Instance.LobbyPlayers.Add(this);
        bool isLan = Illu.Networking.NetworkManager.Instance.isLanConnection;
        GetComponent<SteamUI>().enabled = !isLan;
        GetComponent<LanUI>().enabled = isLan;
    }
    public override void OnStopClient() 
    {
        Illu.Networking.NetworkManager.Instance.LobbyPlayers.Remove(this);
        Destroy(gameObject);
    }

    [Client]
    public void ReadyUP() => CMDSetStatus(ReadyUpSystem.Instance.myID, true);
    [Client]
    public void CancelReadyUP() => CMDSetStatus(ReadyUpSystem.Instance.myID, false);
    
    [Command]
    public void CMDSetStatus(ReadyUpSystem.ID myID, bool status)
    {
        if (myID == ReadyUpSystem.ID.playerOne)
        {
            Debug.Log("Ready up player one");
            ReadyUpSystem.Instance.playerOneReady = status;
        }
        else
        {
            Debug.Log("Ready up player two");
            ReadyUpSystem.Instance.playerTwoReady = status;
        }
    }

    [Command]
    public void RequestID(GameObject go)
    {
        NetworkConnectionToClient conn = go.GetComponent<NetworkIdentity>().connectionToClient;

        if (ReadyUpSystem.Instance.assigned.Count == 0)
        {
            ReadyUpSystem.Instance.assigned.Add(ReadyUpSystem.ID.playerOne);
            //ReadyUpSystem.Instance.myID = ReadyUpSystem.ID.playerOne;
            TargetSetIdOnClient(conn, ReadyUpSystem.ID.playerOne);
        }
        else
        {
            ReadyUpSystem.Instance.assigned.Add(ReadyUpSystem.ID.playerTwo);
            // ReadyUpSystem.Instance.myID = ReadyUpSystem.ID.playerTwo;
            TargetSetIdOnClient(conn, ReadyUpSystem.ID.playerTwo);
        }
    }

    [TargetRpc]
    public void TargetSetIdOnClient(NetworkConnection target, ReadyUpSystem.ID id) => ReadyUpSystem.Instance.myID = id;
    
}
