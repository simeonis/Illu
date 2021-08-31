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
    public void RegisterKey(string key){
        _data.Add(key, null);
    }
    
    [Client]
    public void SendData(string key, object data = null)
    {
        // No authority
        if (!hasAuthority) return;

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
        else
        {
            // Useful for triggering without sending data
            // Example: button
            CmdSendNull(key);
        }
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendNull(string key)
    {
        Debug.Log("[Server]: Received NSD_Null(\"" + key + "\") from Client[" + connectionToClient.connectionId + "].");
        RpcNull(key);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendBool(string key, bool data)
    {
        Debug.Log("[Server]: Received NSD_Bool(\"" + key + "\") from Client[" + connectionToClient.connectionId + "].");
        RpcBool(key, data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendInt(string key, int data)
    {
        Debug.Log("[Server]: Received NSD_Int(\"" + key + "\") from Client[" + connectionToClient.connectionId + "].");
        RpcInt(key, data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendFloat(string key, float data)
    {
        Debug.Log("[Server]: Received NSD_Float(\"" + key + "\") from Client[" + connectionToClient.connectionId + "].");
        RpcFloat(key, data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendString(string key, string data)
    {
        Debug.Log("[Server]: Received NSD_String(\"" + key + "\") from Client[" + connectionToClient.connectionId + "].");
        RpcString(key, data);
    }

    [ClientRpc]
    private void RpcNull(string key)
    {
        OnDataChanged(new DataChangedEventArgs(key, DateTime.Now));
    }

   [ClientRpc]
    private void RpcBool(string key, bool data)
    {
        _data[key] = data;
        OnDataChanged(new DataChangedEventArgs(key, DateTime.Now));
    }

    [ClientRpc]
    private void RpcInt(string key, int data)
    {
        _data[key] = data;
        OnDataChanged(new DataChangedEventArgs(key, DateTime.Now));
    }

    [ClientRpc]
    private void RpcFloat(string key, float data)
    {
        _data[key] = data;
        OnDataChanged(new DataChangedEventArgs(key, DateTime.Now));
    }

    [ClientRpc]
    private void RpcString(string key, string data)
    {
        _data[key] = data;
        OnDataChanged(new DataChangedEventArgs(key, DateTime.Now));
    }

    [Client]
    protected virtual void OnDataChanged(DataChangedEventArgs e)
    {
        DataChanged?.Invoke(this, e);
    }

    public object GetData(string key){
        return _data[key];
    }
}