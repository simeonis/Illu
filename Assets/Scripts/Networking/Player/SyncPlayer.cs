using UnityEngine;
using Mirror;

//////////////////////////////////////////////////////////////////////////
///Sync Player
///Handles sending data over the network and smoothing the recieved values
//////////////////////////////////////////////////////////////////////////

///To Do: Top level transform send the rotation for death in player its (player -> )

public class SyncPlayer : NetworkBehaviour
{

    [Header("Authority")]
    [Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
    [SerializeField] private bool clientAuthority = true;

    [Header("Networking Parameters")]
    [Tooltip("How much the local player moves before triggering an Update to all clients")]
    [SerializeField] private float moveTriggerSensitivity = 0.01f;

    [Tooltip("How far the remote player can be off before snapping to remote position")]
    [SerializeField] private float allowedLagDistance = 10.0f;

    [Tooltip("Anything less then this value will snap to Zero vector and player won't attempt to move closer")]
    [SerializeField] private float maximumAcceptableDeviance = 0.5f;

    [Tooltip("How fast a remote player corrects its look")]
    [SerializeField] private float correntRotSpeed = 2.0f;

    [Tooltip("How fast a remote player corrects its look")]
    [SerializeField] private float allowRotLagAmount = 0.25f;

    [Tooltip("Apply Rotation Smoothing")]
    [SerializeField] private bool smoothRot = false;

    [Header("Debug Position")]
    public bool debug = false;

    //Transforms remember to pass in in Inspector
    [Header("Players Transforms")]
    [Tooltip("Pass in the Controller")]
    public Transform controller;

    [Tooltip("Pass in the Player Camera")]
    public Transform playerCamera;

    [Tooltip("Pass in Orientation")]
    public Transform orientation;

    private PlayerSyncData remotePlayerSyncData;

    Vector3 lastPosition;
    Quaternion lastRotation;
    Quaternion lastBodyRotation;
    Quaternion lastRootRotation;
    Vector3 transPosition;

    // local authority send time
    float lastClientSendTime;

    private NetworkPlayerController networkPlayerController;

    void Awake()
    {
        networkPlayerController = GetComponent<NetworkPlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        // send to server if we are local player and have authority
        if (isClient)
        {
            if (hasAuthority)
            {
                // check only each 'syncInterval'
                if (Time.time - lastClientSendTime >= syncInterval)
                {
                    if (HasEitherMovedRotated())
                    {
                        PlayerSyncData playerSyncData = new PlayerSyncData(transform.position, playerCamera.rotation, orientation.rotation, networkPlayerController.GetRotation());
                        CMDSendPosAndRot(playerSyncData);
                    }
                    lastClientSendTime = Time.time;
                }
            }
            else
            {
                HandleRemotePositionUpdates();
                HandleRemoteRotationUpdates();
            }

            transPosition = transform.position;
        }
    }

    //return whether there's been a change in position or rotation 
    //to prevent spamming the network when not moving 
    bool HasEitherMovedRotated()
    {
        Quaternion RootRotation = networkPlayerController.GetRotation();
        // moved or rotated?
        bool moved = Vector3.Distance(lastPosition, transform.position) > moveTriggerSensitivity;
        bool headRotated = Quaternion.Angle(lastRotation, playerCamera.rotation) > moveTriggerSensitivity;
        bool bodyRotated = Quaternion.Angle(lastBodyRotation, orientation.rotation) > moveTriggerSensitivity;
        bool rootRotated = Quaternion.Angle(lastRootRotation, RootRotation) > moveTriggerSensitivity;

        bool change = moved || headRotated || bodyRotated || rootRotated;

        if (change)
        {
            //position/rotation/bodyrot
            lastPosition = transform.position;
            lastRotation = playerCamera.rotation;
            lastBodyRotation = orientation.rotation;
            lastRootRotation = RootRotation;
        }

        return change;
    }

    // local authority client sends sync message to server for broadcasting
    [Command(channel = Channels.Unreliable)]
    void CMDSendPosAndRot(PlayerSyncData playerSyncData)
    {
        // Ignore messages from client if notCmdClientToServerSync in client authority mode
        if (!clientAuthority)
            return;

        RPCSyncPosition(playerSyncData);
    }

    [ClientRpc]
    public void RPCSyncPosition(PlayerSyncData playerSyncData)
    {
        if (debug)
            Debug.Log("RPC Position: " + playerSyncData.position + "HeadRot " + playerSyncData.headRot + "bodyRot " + playerSyncData.bodyRot);

        remotePlayerSyncData = playerSyncData;
    }

    [Client]
    public void HandleRemotePositionUpdates()
    {
        var LagDistance = remotePlayerSyncData.position - transform.position;

        //High distance => sync is to much off => send to position
        if (LagDistance.magnitude > allowedLagDistance)
        {
            if (debug)
                Debug.LogWarning("Sync Position to Great");
            controller.position = remotePlayerSyncData.position;
            LagDistance = Vector3.zero;
        }

        //ignore the y distance
        //LagDistance -= LagDistance.up;

        Vector3 unwantedUp = Vector3.Dot(LagDistance, orientation.up) * orientation.up;
        Vector3 FinalLagDistance = LagDistance - unwantedUp;

        if (LagDistance.magnitude < maximumAcceptableDeviance)
        {   //Player is nearly at the point
            networkPlayerController.moveDirection = Vector3.zero;
        }
        else //Player has to go to the point
        {
            networkPlayerController.moveDirection = FinalLagDistance;
        }
    }


    [Client]
    private void HandleRemoteRotationUpdates()
    {
        // The step size is equal to speed times frame time.
        var step = correntRotSpeed * Time.deltaTime;

        float camDiff = Quaternion.Dot(playerCamera.rotation, remotePlayerSyncData.headRot);
        float bodyDiff = Quaternion.Dot(orientation.rotation, remotePlayerSyncData.bodyRot);
        float rootDiff = Quaternion.Dot(networkPlayerController.GetRotation(), remotePlayerSyncData.rootRot);

        if (debug)
            Debug.Log("camDiff " + camDiff + " bodyDiff" + bodyDiff);

        if (smoothRot && camDiff <= allowRotLagAmount)
        {
            // Rotate our playerCamera a step closer to the target's.
            playerCamera.rotation = Quaternion.RotateTowards(playerCamera.rotation, remotePlayerSyncData.headRot, step);
        }
        else
        {
            playerCamera.rotation = remotePlayerSyncData.headRot;
        }

        if (smoothRot && bodyDiff <= allowRotLagAmount)
        {
            // Rotate our orientation a step closer to the target's.
            orientation.rotation = Quaternion.RotateTowards(orientation.rotation, remotePlayerSyncData.bodyRot, step);
        }
        else
        {
            orientation.rotation = remotePlayerSyncData.bodyRot;
        }

        if (smoothRot && rootDiff <= allowRotLagAmount)
        {
            // Rotate our orientation a step closer to the target's.
            networkPlayerController.SetRotation(Quaternion.RotateTowards(networkPlayerController.GetRotation(), remotePlayerSyncData.rootRot, step));
        }
        else
        {
            networkPlayerController.SetRotation(remotePlayerSyncData.rootRot);

        }
    }


    //Handle Sending Crouch, Jump, Sprint
    //--------------------------------------------------------------
    [Command(channel = Channels.Unreliable)]
    public void CmdHandleCrouch(bool CrouchState)
    {
        RpcCrouch(CrouchState);
    }

    [ClientRpc]
    private void RpcCrouch(bool CrouchState)
    {
        if (CrouchState)
        {
            networkPlayerController.NetworkCrouch();
        }
        else
        {
            networkPlayerController.NetworkUnCrouch();
        }
    }

    [Command(channel = Channels.Unreliable)]
    public void CmdHandleSprint(bool SprintState) { RpcSprint(SprintState); }

    [ClientRpc]
    private void RpcSprint(bool SprintState)
    {
        if (SprintState)
        {
            networkPlayerController.NetworkSprint();
        }
        else
        {
            networkPlayerController.NetworkWalk();
        }
    }

    [Command(channel = Channels.Unreliable)]
    public void CmdSendJump() { RpcSendJump(); }

    [ClientRpc]
    private void RpcSendJump() { networkPlayerController.NetworkJump(); }

    //Static Draw Methods
    //--------------------------------------------------------------
    static void DrawDataPointGizmo(Vector3 pos, Color color)
    {
        // use a little offset because transform.localPosition might be in
        // the ground in many cases
        Vector3 offset = Vector3.up * 0.01f;

        // draw position
        Gizmos.color = color;
        Gizmos.DrawSphere(pos + offset, 0.5f);

    }

    static void DrawLineBetweenDataPoints(Vector3 start, Vector3 end, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);
    }

    // draw the data points for easier debugging
    void OnDrawGizmos()
    {
        // draw start and goal points
        DrawDataPointGizmo(remotePlayerSyncData.position, Color.green);
        DrawDataPointGizmo(transPosition, Color.red);

        // draw line between them
        DrawLineBetweenDataPoints(transPosition, remotePlayerSyncData.position, Color.cyan);
    }
}
