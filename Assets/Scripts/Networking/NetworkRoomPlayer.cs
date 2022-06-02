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

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log("I have Authority !!!!@@!!!! " + hasAuthority + " is local PLayer  " + netIdentity.isLocalPlayer);

    }

    ReadyUpSystem.ID myID;
    void Awake()
    {
        myID = Illu.Networking.NetworkManager.Instance.ReadyUpSystem.requestID();
    }

    [Client]
    public void ReadyUP()
    {
        Debug.Log("Ready up");
        //Illu.Networking.NetworkManager.Instance.ReadyUpSystem.SetReadyStatus(myID, true);
        CMDSetStatus();
    }
    public void CancelReadyUP()
    {
        //Illu.Networking.NetworkManager.Instance.ReadyUpSystem.SetReadyStatus(myID, false);
    }

    [Command]
    private void CMDSetStatus()
    {
        // if (id == ID.playerOne)
        // {
        //     playerOneReady = status;
        // }
        // else
        // {
        //     playerTwoReady = status;
        // }
    }

}
