using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class NetworkRoomPlayer : NetworkBehaviour
{
    [SerializeField] GameObject canvas;
    // [SerializeField] ReadyUpSystem.ID myID;

     override public void OnStartAuthority()
    {  
        canvas.SetActive(true);
     
        RequestID(this.gameObject);
    }

    public override void OnStartClient()
    {
        Debug.Log("OnStartClient -> NetworkRoomPlayerLobby");
        Illu.Networking.NetworkManager.Instance.RoomPlayers.Add(this);
    }

    public override void OnStopClient()
    {
         Illu.Networking.NetworkManager.Instance.RoomPlayers.Remove(this);
    }


    [Client]
    public void ReadyUP()
    {
        Debug.Log("ReadyUP");
        CMDSetStatus(ReadyUpSystem.Instance.myID, true);
    }

    [Client]
    public void CancelReadyUP()
    {
        CMDSetStatus(ReadyUpSystem.Instance.myID, false);
    }


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

        Debug.Log("Request ID" + ReadyUpSystem.Instance.assigned.Count + " " + conn.connectionId);
        if (ReadyUpSystem.Instance.assigned.Count == 0)
        {
            ReadyUpSystem.Instance.assigned.Add(ReadyUpSystem.ID.playerOne);
            //ReadyUpSystem.Instance.myID = ReadyUpSystem.ID.playerOne;
            TargetDoMagic(conn, ReadyUpSystem.ID.playerOne);
        }
        else
        {
            ReadyUpSystem.Instance.assigned.Add(ReadyUpSystem.ID.playerTwo);
            // ReadyUpSystem.Instance.myID = ReadyUpSystem.ID.playerTwo;
            TargetDoMagic(conn, ReadyUpSystem.ID.playerTwo);
        }
    }

    [TargetRpc]
    public void TargetDoMagic(NetworkConnection target, ReadyUpSystem.ID id)
    {
        // This will appear on the opponent's client, not the attacking player's
         ReadyUpSystem.Instance.myID = id;
    }
}
