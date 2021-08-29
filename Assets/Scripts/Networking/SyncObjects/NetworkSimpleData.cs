using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public delegate void CustomEventHandler<T>(object sender, DataChangedEventArgs<T> args);

//public delegate void EventHandler<TEventArgs>(object? sender, TEventArgs e);

public class DataChangedEventArgs<T> : EventArgs
{
    public T data { get; set; }
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
    public Dictionary<string, Delegate> EventHandlersDictionary = new Dictionary<string, Delegate>();

    //public List<CustomEventHandler> _listEventHandlers;

    // public event CustomEventHandler<T> DataChanged;


    [Client]
    public void RegisterData()
    {
        //make param string key, Type type
        CustomEventHandler<bool> bob = null;
        EventHandlersDictionary.Add("yoo", bob);
    }



    [Client]
    public void SendData(bool data)
    {
        // Send key
        Debug.Log("Send Data");
        CmdSendData(data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendData(bool data)
    {
        Debug.Log("Cmd Send Data");
        RpcData(data);
    }

    [ClientRpc]
    private void RpcData(bool data)
    {
        Debug.Log("RpcData Data");
        DataChangedEventArgs<bool> args = new DataChangedEventArgs<bool>();
        args.data = data;
        args.TimeSent = DateTime.Now;
        OnDataChanged(args);
    }

    protected virtual void OnDataChanged(DataChangedEventArgs<bool> e)
    {
        Debug.Log("OnDataChanged");
        //CustomEventHandler<bool> handler = DataChanged;
        //handler?.Invoke(this, e);
        //------------------------
        CustomEventHandler<bool> handler = EventHandlersDictionary["yoo"] as CustomEventHandler<bool>;
        handler?.Invoke(this, e);
    }
}
