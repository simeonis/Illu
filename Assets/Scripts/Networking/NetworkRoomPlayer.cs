using Mirror;
using UnityEngine;

public class NetworkRoomPlayer : NetworkBehaviour
{
    private Illu.Networking.NetworkManager room;
    private Illu.Networking.NetworkManager Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as Illu.Networking.NetworkManager;
        }
    }

    public override void OnStartClient()
    {
        Debug.Log("OnStartClient -> NetworkRoomPlayerLobby");
        Room.RoomPlayers.Add(this);

    }

    public override void OnStopClient()
    {
        Room.RoomPlayers.Remove(this);
    }

    ReadyUpSystem.ID myID;
    void Awake()
    {
        myID = ReadyUpSystem.Instance.requestID();
    }

    [Client]
    public void ReadyUP()
    {
        CMDSetStatus(myID, true);
    }
    public void CancelReadyUP()
    {
        //Illu.Networking.NetworkManager.Instance.ReadyUpSystem.SetReadyStatus(myID, false);
    }

    [Command]
    private void CMDSetStatus(ReadyUpSystem.ID id, bool status)
    {
        if (id == ReadyUpSystem.ID.playerOne)
        {
            ReadyUpSystem.Instance.playerOneReady = status;
        }
        else
        {
            ReadyUpSystem.Instance.playerTwoReady = status;
        }
    }
}
