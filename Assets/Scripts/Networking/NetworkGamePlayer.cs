using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkGamePlayer : NetworkBehaviour
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
        DontDestroyOnLoad(gameObject);

        Room.GamePlayers.Add(this);

        Debug.Log("isLocalPlayer" + isLocalPlayer);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }
}