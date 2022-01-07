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

}
