using Mirror;
using UnityEngine;

public class NetworkRoomPlayer : NetworkBehaviour
{
    [SerializeField] GameObject canvas;

     override public void OnStartAuthority()
    {  
        canvas.SetActive(true);
        RequestID(this.gameObject);
    }

    public override void OnStartClient() => Illu.Networking.NetworkManager.Instance.RoomPlayers.Add(this);
    public override void OnStopClient() => Illu.Networking.NetworkManager.Instance.RoomPlayers.Remove(this);

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
