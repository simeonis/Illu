using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Mirror;

public delegate void CustomEventHandler(object sender, DataChangedEventArgs args);

public class DataChangedEventArgs : EventArgs
{ 
    public string Key { get; set; }
    public DateTime EventFired { get; set; }
}

public class NetworkSimpleData : NetworkBehaviour
{
    private Dictionary<string, object> _data = new Dictionary<string, object>();

    public event CustomEventHandler DataChanged;

    [Client]
    public void RegisterData(string key, object initialValue){
        //NetData netData = new NetData(initialValue, DateTime.Now);
        _data.Add(key, initialValue);
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
        RpcBool(key, data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendInt(string key, int data)
    {
        RpcInt(key, data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendFloat(string key, float data)
    {
        RpcFloat(key, data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendString(string key, string data)
    {
        RpcString(key, data);
    }

    [ClientRpc]
    private void RpcBool(string key, bool data)
    {
        _data[key] = data;
        RpcData(key);
    }

    [ClientRpc]
    private void RpcInt(string key, int data)
    {
        _data[key] = data;
        RpcData(key);
    }

    [ClientRpc]
    private void RpcFloat(string key, float data)
    {
        _data[key] = data;
        RpcData(key);
    }

    [ClientRpc]
    private void RpcString(string key, string data)
    {
        _data[key] = data;
        RpcData(key);
    }

    private void RpcData(string key)
    {
        DataChangedEventArgs args = new DataChangedEventArgs();
        args.Key = key;
        args.EventFired = DateTime.Now;
        OnDataChanged(args);
    }

    [Client]
    protected virtual void OnDataChanged(DataChangedEventArgs e)
    {
        Debug.Log("OnDataChanged");

        CustomEventHandler handler = DataChanged;
        handler?.Invoke(this, e);
    }

    public object GetData(string key){
        return _data[key];
    }

}

public class NetData {

    public DateTime TimeSent {get;set;}
    public object SingleData {get;set;}

    public NetData(){}
    public NetData(object data, DateTime time)
    {
        SingleData = data;
        TimeSent = time;
    }
    
    public object GetData(){
        return SingleData;
    }
}

// public static class DateTimeReaderWriter
// {
//       public static void WriteDateTime(this NetworkWriter writer, DateTime dateTime)
//       {
//           writer.WriteInt64(dateTime.Ticks);
//       }
     
//       public static DateTime ReadDateTime(this NetworkReader reader)
//       {
//           return new DateTime(reader.ReadInt64());
//       }
// }