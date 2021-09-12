using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;
using System.Collections;

///////////////////////////////////////////////////////////////////////////
/// in equimpent if released from player
/// start sending position and rotation to all client copies
/// other client -> receive pos and rot and follows 
/// after send once more and stop syncing  
///////////////////////////////////////////////////////////////////////////

//TODO
// add on collision with player pass auth 
// add on interacte change owner 

// start filling a buffer of positions
//try sending a time / by distance to get speed = inerpolate with that 

public class SyncEquipment : NetworkBehaviour
{
    [Header("Networking Parameters")]
    [Tooltip("How much the local player moves before triggering an Update to all clients")]
    public float moveTriggerSensitivity = 0.01f;

    [Tooltip("How far the remote player can be off before snapping to remote position")]
    public float allowedLagDistance = 2.0f;
    public float allowedRotationAngle = 5f;

    [SerializeField] private bool debug;

    //Store the Remote Obj data on the client it was sent too
    private Vector3 RemoteObjPosition;
    private Quaternion RemoteObjRotation;

    private Equipment _equipment;
    private Rigidbody equipmentBody;

    //local (Not Synced)
    private float lastClientSendTime;

    private bool simulation = false;
    private float oldMagnitude = 0f;

    private List<MyNetworkData> receivedPositions;
    private long triggerTimeStamp;

    private int index = 0;

    void Start()
    {
        _equipment = GetComponent<Equipment>();
        equipmentBody = _equipment.equipmentBody;
        receivedPositions = new List<MyNetworkData>();
    }

    //Update loop called on both Authority and other Clients 
    //Checks who it's on internally 
    void FixedUpdate()
    {
        if (simulation && hasAuthority)
        {
            bool increasing = equipmentBody.velocity.magnitude > oldMagnitude;
            if (Time.time - lastClientSendTime >= syncInterval)
            {
                MyNetworkData currentPos = new MyNetworkData(transform.position, DateTime.Now.Ticks);
                CmdSendPositionRotation(currentPos, transform.rotation);

                // Update old data for checking against in the next loop
                lastClientSendTime = Time.time;
                oldMagnitude = equipmentBody.velocity.magnitude;
            }
            // Simulation ended
            else if (!increasing && equipmentBody.velocity.magnitude <= 0.1)
            {
                // Send one more position
                // CmdSendPosition(transform.position);
                CmdOnStop();
            }
        }
    }

    private bool isDisabled = false;
    void Update()
    {
        
        if (!hasAuthority && simulation)
        {
            if(!isDisabled && receivedPositions.Count > 0)
            {
                equipmentBody.isKinematic = true;
                equipmentBody.interpolation = RigidbodyInterpolation.None;
                isDisabled = true;
            }
            if(isDisabled)
            {
                HandleRemotePositionUpdates();
            }
                
            //HandleRemoteRotationUpdates();
        }
    }

    //Trigger the action being sent
    //Entry point for Syncing 
    public void Trigger(long now)
    {
        CmdTrigger(now);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdTrigger(long now)
    {
        RPCTrigger(now);
    }

    [ClientRpc]
    private void RPCTrigger(long now)
    {
        Debug.Log("Trigger called!");
        if (!hasAuthority) 
        {
            percent = 0;
        }
        triggerTimeStamp = now;

        simulation = true;
    }

    //Command
    [Command(channel = Channels.Unreliable)]
    void CmdOnStop()
    {
        RpcOnStopped();
    }

    //Called when the moving has stopped on Authority 
    [ClientRpc]
    protected void RpcOnStopped()
    {
        simulation = false;
        
        if (!hasAuthority)
        {
            index = 0;
            equipmentBody.isKinematic = false;
            isDisabled = false;
            receivedPositions.Clear();
            equipmentBody.interpolation = RigidbodyInterpolation.Interpolate;
            equipmentBody.velocity = new Vector3();
        }
        else RemoveAuthority();
    }

    [Command]
    private void RemoveAuthority()
    {
        GetComponent<NetworkIdentity>().RemoveClientAuthority();
    }

    // Authority sends Pos and Rot 
    [Command(channel = Channels.Unreliable)]
    void CmdSendPositionRotation(MyNetworkData position, Quaternion rotation)
    {
        RPCSyncPosition(position, rotation);
    }

    //Server broadcasts Pos and Rot to all clients  
    [ClientRpc]
    public void RPCSyncPosition(MyNetworkData position, Quaternion rotation)
    {
        if(!hasAuthority)
        {
            receivedPositions.Add(position);
            //RemoteObjPosition = position;
            RemoteObjRotation = rotation;
            // RemoteObjSpeed = Speed;
        }
    }


    //While moving on the client with authority that triggered the action
    //Handle the received positional updates

    private float percent = 0.0f;

    [Client]
    private void HandleRemotePositionUpdates()
    {
        int count = receivedPositions.Count;
      
        if(count > 0 && index <= count - 1)
        {
            //get difference 
            //fixedDeltaTime

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 toOther = receivedPositions[index].position - transform.position;
        
            if(Vector3.Dot(forward, toOther) < 0)
            {
               StartCoroutine(FindIndexAhead());
            }

            double timeDiff = 0;

            if(count > 1 && index > 0)
            {
                timeDiff = new TimeSpan(receivedPositions[index].timeSent - receivedPositions[index - 1].timeSent).TotalSeconds;
            }
            else //dont have a first value
            {
                timeDiff = new TimeSpan(receivedPositions[index].timeSent - triggerTimeStamp).TotalSeconds;
            }
           
            percent += Time.deltaTime / (float)timeDiff;
            var currentTarget = receivedPositions[index].position;
            var LagDistance = currentTarget- transform.position;

            // High distance => sync is to much off => send to position
            // if (LagDistance.magnitude > allowedLagDistance)
            // {
            //     if (debug) Debug.LogWarning("Sync Position too Great! Teleporting equipment.");
            //     transform.position = RemoteObjPosition;
            //     LagDistance = Vector3.zero;
            // }

            
            transform.position = Vector3.Lerp(transform.position, currentTarget, percent);
            
            if(percent >= 1)
            {
                percent = 0;
                index++;
            }
        }
    }

    IEnumerator FindIndexAhead() 
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 toOther = receivedPositions[index].position - transform.position;

        while(Vector3.Dot(forward, toOther) < 0)
        {
            index++;
            yield return null;
        }
    }

    // [Client]
    // private void HandleRemoteRotationUpdates()
    // {
    //     var LagRotation = Quaternion.Angle(RemoteObjRotation, transform.rotation);
  
    //     if (LagRotation > allowedRotationAngle)
    //     {
    //         if (debug) Debug.LogWarning("Sync Rotation too Great! Snapping equipment rotation.");
    //         transform.rotation = RemoteObjRotation;
    //         LagRotation = 0f;
    //     }
    //     if (LagRotation >= 0.5f)
    //     {
    //         transform.rotation = Quaternion.Lerp(transform.rotation, RemoteObjRotation, 0.5f);
    //     }
    // }


    //Gizmo stuff for testing 
    //---------------------------------------------------------------------------------------------

    static void DrawDataPointGizmo(Vector3 pos, Color color)
    {
        // use a little offset because transform.localPosition might be in
        // the ground in many cases
        Vector3 offset = Vector3.up * 0.01f;

        // draw position
        Gizmos.color = color;
        Gizmos.DrawSphere(pos + offset, 0.05f);

    }

    static void DrawLineBetweenDataPoints(Vector3 start, Vector3 end, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);
    }

    // draw the data points for easier debugging
    void OnDrawGizmos()
    {
        // if (simulation)
        // {
            foreach(MyNetworkData data in receivedPositions)
            {
                // draw start and goal points
                DrawDataPointGizmo(data.position, Color.yellow);
                // draw line between them
                //DrawLineBetweenDataPoints(transform.position, RemoteObjPosition, Color.cyan);
            }
            
       // }
    }
}

public struct MyNetworkData
{
    public Vector3 position { get; private set; }
    public long timeSent { get; private set; }

    public MyNetworkData(Vector3 position, long timeSent)
    {
        this.position = position;
        this.timeSent = timeSent;
    }
}

public static class CustomReadWriteFunctions 
{
    public static void WriteMyNetworkData(this NetworkWriter writer, MyNetworkData MyNetworkData)
    {
        writer.WriteVector3(MyNetworkData.position);
        writer.WriteLong(MyNetworkData.timeSent);
    }

    public static MyNetworkData ReadMyNetworkData(this NetworkReader reader)
    {

        return new MyNetworkData(reader.ReadVector3(), reader.ReadLong());
    }
}