using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

public delegate void CustomEventHandler(object sender, DataChangedEventArgs args);

//public delegate void EventHandler<TEventArgs>(object? sender, TEventArgs e);

public class DataChangedEventArgs : EventArgs
{
    public bool data { get; set; }
    public DateTime TimeSent { get; set; }
}

// public interface INetworkSimpleData<T>
// {
//     //NetworkSimpleData _networkSimpleData { get; set; }
//     void Start();
//     //void CustomEventHandler(object sender, DataChangedEventArgs<T> e);
// }

public class NetworkSimpleData : NetworkBehaviour
{
    public event CustomEventHandler DataChanged;

    [Client]
    public void SendData(bool data)
    {
        CmdSendData(data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendData(bool data)
    {
        RpcData(data);
    }

    [ClientRpc]
    private void RpcData(bool data)
    {
        DataChangedEventArgs args = new DataChangedEventArgs();
        args.data = data;
        args.TimeSent = DateTime.Now;
        OnDataChanged(args);
    }

    protected virtual void OnDataChanged(DataChangedEventArgs e)
    {
        CustomEventHandler handler = DataChanged;
        handler?.Invoke(this, e);
    }
}