using System;
using System.Text;
using UnityEngine;
using Mirror;

// a transport that can use different transports 
[DisallowMultipleComponent]
public class SwitchTransport : Transport
{
    public Transport[] transports;

    Transport selectedTransport;

    public void Awake()
    {
        if (transports == null || transports.Length == 0)
        {
            Debug.LogError("Switch transport requires at least 1 transport");
        }
        selectedTransport = transports[0];
    }

    public void PickTransport(int id)
    {
        selectedTransport = transports[id];
    }

    public override void ClientEarlyUpdate()
    {
        selectedTransport.ClientEarlyUpdate();
    }

    public override void ServerEarlyUpdate()
    {
        selectedTransport.ServerEarlyUpdate();
    }

    public override void ClientLateUpdate()
    {
        selectedTransport.ClientLateUpdate();
    }

    public override void ServerLateUpdate()
    {
        selectedTransport.ServerLateUpdate();
    }

    void OnEnable()
    {
        foreach (Transport transport in transports)
        {
            transport.enabled = true;
        }
    }

    void OnDisable()
    {
        foreach (Transport transport in transports)
        {
            transport.enabled = false;
        }
    }

    public override bool Available()
    {
        // available if any of the transports is available

        if (selectedTransport.Available())
        {
            return true;
        }

        return false;
    }

    #region Client

    public override void ClientConnect(string address)
    {

        if (selectedTransport.Available())
        {
            selectedTransport.OnClientConnected = OnClientConnected;
            selectedTransport.OnClientDataReceived = OnClientDataReceived;
            selectedTransport.OnClientError = OnClientError;
            selectedTransport.OnClientDisconnected = OnClientDisconnected;
            selectedTransport.ClientConnect(address);
            return;
        }

        throw new ArgumentException("Selected Transport not set");
    }

    public override void ClientConnect(Uri uri)
    {

        if (selectedTransport.Available())
        {
            try
            {
                selectedTransport.OnClientConnected = OnClientConnected;
                selectedTransport.OnClientDataReceived = OnClientDataReceived;
                selectedTransport.OnClientError = OnClientError;
                selectedTransport.OnClientDisconnected = OnClientDisconnected;
                selectedTransport.ClientConnect(uri);
                return;
            }
            catch (ArgumentException)
            {
                // transport does not support the schema, just move on to the next one
            }
        }

        throw new ArgumentException("Selected Transport not set");
    }

    public override bool ClientConnected()
    {
        return (object)selectedTransport != null && selectedTransport.ClientConnected();
    }

    public override void ClientDisconnect()
    {
        if ((object)selectedTransport != null)
            selectedTransport.ClientDisconnect();
    }

    public override void ClientSend(ArraySegment<byte> segment, int channelId)
    {
        selectedTransport.ClientSend(segment, channelId);
    }

    #endregion

    #region Server
    // connection ids get mapped to base transports
    // if we have 3 transports,  then
    // transport 0 will produce connection ids [0, 3, 6, 9, ...]
    // transport 1 will produce connection ids [1, 4, 7, 10, ...]
    // transport 2 will produce connection ids [2, 5, 8, 11, ...]
    // int FromBaseId(int transportId, int connectionId)
    // {
    //     return connectionId * transports.Length + transportId;
    // }

    // int ToBaseId(int connectionId)
    // {
    //     return connectionId / transports.Length;
    // }

    // int ToTransportId(int connectionId)
    // {
    //     return connectionId % transports.Length;
    // }



    void AddServerCallbacks()
    {
        // wire all the base transports to my events
        // for (int i = 0; i < transports.Length; i++)
        // {
        // this is required for the handlers,  if I use i directly
        // then all the handlers will use the last i



        selectedTransport.OnServerConnected = (baseConnectionId =>
        {
            OnServerConnected.Invoke(baseConnectionId);
        });

        selectedTransport.OnServerDataReceived = (baseConnectionId, data, channel) =>
        {
            OnServerDataReceived.Invoke(baseConnectionId, data, channel);
        };

        selectedTransport.OnServerError = (baseConnectionId, error) =>
        {
            OnServerError.Invoke(baseConnectionId, error);
        };
        selectedTransport.OnServerDisconnected = baseConnectionId =>
        {
            OnServerDisconnected.Invoke(baseConnectionId);
        };
        // }
    }

    // for now returns the first uri,
    // should we return all available uris?
    public override Uri ServerUri()
    {
        return transports[0].ServerUri();
    }


    public override bool ServerActive()
    {
        // avoid Linq.All allocations

        if (!selectedTransport.ServerActive())
        {
            return false;
        }

        return true;
    }

    public override string ServerGetClientAddress(int connectionId)
    {
        //int baseConnectionId = ToBaseId(connectionId);
        //int transportId = ToTransportId(connectionId);
        return selectedTransport.ServerGetClientAddress(connectionId);
    }

    public override void ServerDisconnect(int connectionId)
    {
        //int baseConnectionId = ToBaseId(connectionId);
        //int transportId = ToTransportId(connectionId);
        selectedTransport.ServerDisconnect(connectionId);
    }

    public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
    {
        // int baseConnectionId = ToBaseId(connectionId);
        // int transportId = ToTransportId(connectionId);

        // for (int i = 0; i < transports.Length; ++i)
        // {
        //     if (i == transportId)
        //     {
        selectedTransport.ServerSend(connectionId, segment, channelId);
        //     }
        // }
    }

    public override void ServerStart()
    {

        AddServerCallbacks();
        selectedTransport.ServerStart();

    }

    public override void ServerStop()
    {

        selectedTransport.ServerStop();

    }
    #endregion

    public override int GetMaxPacketSize(int channelId = 0)
    {
        return selectedTransport.GetMaxPacketSize(channelId);
    }

    public override void Shutdown()
    {

        selectedTransport.Shutdown();

    }

    public override string ToString()
    {
        return selectedTransport.ToString();
    }
}