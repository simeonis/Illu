using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkGamePlayer : NetworkBehaviour
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
        DontDestroyOnLoad(gameObject);

        Room.GamePlayers.Add(this);

        Debug.Log("isLocalPlayer" + isLocalPlayer);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }
}