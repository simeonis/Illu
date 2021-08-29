using System;
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
    private Dictionary<string, NetData> _data = new Dictionary<string, NetData>();

    public event CustomEventHandler DataChanged;
    
    [Client]
    public void SendData(string key, object data)
    {
        // Send key
        Debug.Log("Send Data");
        NetData netData = new NetData(data, DateTime.Now);
        CmdSendData(key, netData);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendData(string key, NetData data)
    {
        Debug.Log("Cmd Send Data");
        RpcData(key, data);
    }

    [ClientRpc]
    private void RpcData(string key, NetData data)
    {
        Debug.Log("RpcData Data");
        _data[key] = data;

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

public class NetData{

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

public static class DateTimeReaderWriter
{
      public static void WriteDateTime(this NetworkWriter writer, DateTime dateTime)
      {
          writer.WriteInt64(dateTime.Ticks);
      }
     
      public static DateTime ReadDateTime(this NetworkReader reader)
      {
          return new DateTime(reader.ReadInt64());
      }
}