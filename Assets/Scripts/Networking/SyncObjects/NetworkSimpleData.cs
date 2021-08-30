using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Mirror;

public delegate void CustomEventHandler(object sender, DataChangedEventArgs args);

public class DataChangedEventArgs : EventArgs
{ 
    public DataChangedEventArgs(string key, DateTime time)
    {
        Key = key;
        EventFired = time;
    }
    public string Key { get; set; }
    public DateTime EventFired { get; set; }
}

public struct NetworkData {
    public NetworkData(int nID, object nData)
    {
        networkID = nID;
        data = nData;
    }

    public int networkID;
    public object data;
}

public class NetworkSimpleData : NetworkBehaviour
{
    private Dictionary<string, NetworkData> _data = new Dictionary<string, NetworkData>();

    public event CustomEventHandler DataChanged;

    [Client]
    public void RegisterData(string key, object initialValue){
        _data.Add(key, new NetworkData(connectionToServer.connectionId, initialValue));
    }
    
    [Client]
    public void SendData(string key, object data)
    {
        //CmdSend(key, new NetworkData(connectionToServer.connectionId, data));
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
    private void CmdSend(string key, NetworkData networkData)
    {
        RpcData(key, networkData);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendBool(string key, bool data)
    {
        RpcData(key, new NetworkData(connectionToClient.connectionId, data));
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendInt(string key, int data)
    {
        RpcData(key, new NetworkData(connectionToClient.connectionId, data));
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendFloat(string key, float data)
    {
        RpcData(key, new NetworkData(connectionToClient.connectionId, data));
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendString(string key, string data)
    {
        RpcData(key, new NetworkData(connectionToClient.connectionId, data));
    }

    [ClientRpc]
    private void RpcData(string key, NetworkData networkData)
    {
        _data[key] = networkData;
        OnDataChanged(new DataChangedEventArgs(key, DateTime.Now));
    }

    [Client]
    protected virtual void OnDataChanged(DataChangedEventArgs e)
    {
        Debug.Log("OnDataChanged");
        DataChanged?.Invoke(this, e);
    }

    public NetworkData GetData(string key){
        return _data[key];
    }

}