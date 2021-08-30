using Mirror;
using UnityEngine;

public class NetworkRoomPlayer : NetworkBehaviour
{

    private MyNetworkManager room;
    private MyNetworkManager Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as MyNetworkManager;
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

}
