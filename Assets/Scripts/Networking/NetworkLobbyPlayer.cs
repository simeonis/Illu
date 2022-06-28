using Mirror;
using UnityEngine;
using Illu.Steam;

public class NetworkLobbyPlayer : NetworkBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] ReadyUpSystem readyUpSystem;

    public override void OnStartAuthority()
    {  
        canvas.SetActive(true);
        RequestID(gameObject);

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

    [Client]
    public void ReadyUP() => CMDSetStatus(readyUpSystem.myID, true);
    [Client]
    public void CancelReadyUP() => CMDSetStatus(readyUpSystem.myID, false);

    [Command]
    public void CMDSetStatus(ReadyUpSystem.ID myID, bool status)
    {
        if (myID == ReadyUpSystem.ID.playerOne)
        {
            Debug.Log("Ready up player one");
            readyUpSystem.playerOneReady = status;
        }
        else
        {
            Debug.Log("Ready up player two");
            readyUpSystem.playerTwoReady = status;
        }
    }

    [Command]
    public void RequestID(GameObject go)
    {
        NetworkConnectionToClient conn = go.GetComponent<NetworkIdentity>().connectionToClient;

        if (readyUpSystem.assigned.Count == 0)
        {
            readyUpSystem.assigned.Add(ReadyUpSystem.ID.playerOne);
            //ReadyUpSystem.Instance.myID = ReadyUpSystem.ID.playerOne;
            TargetSetIdOnClient(conn, ReadyUpSystem.ID.playerOne);
        }
        else
        {
            readyUpSystem.assigned.Add(ReadyUpSystem.ID.playerTwo);
            // ReadyUpSystem.Instance.myID = ReadyUpSystem.ID.playerTwo;
            TargetSetIdOnClient(conn, ReadyUpSystem.ID.playerTwo);
        }
    }

    [TargetRpc]
    public void TargetSetIdOnClient(NetworkConnection target, ReadyUpSystem.ID id) => readyUpSystem.myID = id;

}
