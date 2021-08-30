using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public delegate void CustomEventHandler(object sender, DataChangedEventArgs args);

public class DataChangedEventArgs : EventArgs
{ 
    public DataChangedEventArgs(string key, DateTime time)
    {
        this.key = key;
        this.eventFired = time;
    }
    public string key { get; set; }
    public DateTime eventFired { get; set; }
}

public class NetworkSimpleData : NetworkBehaviour
{
    private Dictionary<string, object> _data = new Dictionary<string, object>();

    public event CustomEventHandler DataChanged;

    [Client]
    public void RegisterData(string key, object temporaryData){
        _data.Add(key, temporaryData);
    }
    
    [Client]
    public void SendData(string key, object data)
    {
        if (data is bool)
        {
            CmdSendBool(key, (bool)data);
        }
        else if (data is int)
        {
            CmdSendInt(key, (int)data);
        }
        else if (data is float)
        {
            CmdSendFloat(key, (float)data);
        }
        else if (data is string)
        {
            CmdSendString(key, (string)data);
        }
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendBool(string key, bool data)
    {
        RpcBool(connectionToClient.connectionId, key, data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendInt(string key, int data)
    {
        RpcInt(connectionToClient.connectionId, key, data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendFloat(string key, float data)
    {
        RpcFloat(connectionToClient.connectionId, key, data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendString(string key, string data)
    {
        RpcString(connectionToClient.connectionId, key, data);
    }

   [ClientRpc]
    private void RpcBool(int netID, string key, bool data)
    {
        if (netID == connectionToServer.connectionId)
        {
            _data[key] = data;
            RpcData(key);
        }
    }

    [ClientRpc]
    private void RpcInt(int netID, string key, int data)
    {
        if (netID == connectionToServer.connectionId)
        {
            _data[key] = data;
            RpcData(key);
        }
    }

    [ClientRpc]
    private void RpcFloat(int netID, string key, float data)
    {
        if (netID == connectionToServer.connectionId)
        {
            _data[key] = data;
            RpcData(key);
        }
    }

    [ClientRpc]
    private void RpcString(int netID, string key, string data)
    {
        if (netID == connectionToServer.connectionId)
        {
            _data[key] = data;
            RpcData(key);
        }
    }

    private void RpcData(string key)
    {
        OnDataChanged(new DataChangedEventArgs(key, DateTime.Now));
    }

    [Client]
    protected virtual void OnDataChanged(DataChangedEventArgs e)
    {
        Debug.Log("OnDataChanged");
        DataChanged?.Invoke(this, e);
    }

    public object GetData(string key){
        return _data[key];
    }

}