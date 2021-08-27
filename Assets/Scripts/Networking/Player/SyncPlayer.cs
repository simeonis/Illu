using UnityEngine;
using Mirror;

public class SyncPlayer : NetworkBehaviour
{

    [Header("Authority")]
    [Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
    public bool clientAuthority;

    [Header("Networking Parameters")]
    [Tooltip("How much the local player moves before triggering an Update to all clients")]
    public float moveTriggerSensitivity = 0.01f;

    [Tooltip("How far the remote player can be off before snapping to remote position")]
    public float allowedLagDistance = 10.0f;

    [Header("Debug Position")]
    public bool debug;

    public Transform controller;

    protected Vector3 RemotePlayerPosition;
    protected Quaternion RemotePlayerRotation;

    Vector3 lastPosition;
    Quaternion lastRotation;
    Vector3 transPosition;

    // local authority send time
    float lastClientSendTime;

    private NetworkPlayerController networkPlayerController;

    // Start is called before the first frame update
    void Start()
    {
        networkPlayerController = GetComponentInChildren<NetworkPlayerController>();

        if (isClient)
        {
            // send to server if we are local player
            if (hasAuthority)
            {
                networkPlayerController.LocalPlayerControls.Land.Crouch.performed += context => CmdHandleCrouch(true);
                networkPlayerController.LocalPlayerControls.Land.Crouch.canceled += context => CmdHandleCrouch(false);

                networkPlayerController.LocalPlayerControls.Land.Sprint.performed += context => CmdHandleSprint(true);
                networkPlayerController.LocalPlayerControls.Land.Sprint.canceled += context => CmdHandleSprint(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isClient)
        {
            // send to server if we are local player
            if (hasAuthority)
            {
                // check only each 'syncInterval'
                if (Time.time - lastClientSendTime >= syncInterval)
                {
                    if (HasEitherMovedRotated())
                    {
                        // send to position to clients
                        CmdSyncTransform(transform.position, transform.rotation);
                    }
                    lastClientSendTime = Time.time;
                }
            }
            else
            {
                HandleRemotePositionUpdates();
                transform.rotation = RemotePlayerRotation;
            }
        }
        transPosition = transform.position;
    }

    bool HasEitherMovedRotated()
    {
        // moved or rotated?
        bool moved = Vector3.Distance(lastPosition, transform.position) > moveTriggerSensitivity;
        bool rotated = Quaternion.Angle(lastRotation, transform.rotation) > moveTriggerSensitivity;


        bool change = moved || rotated;

        if (change)
        {
            //position/rotation
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

        return change;
    }

    // local authority client sends sync message to server for broadcasting
    [Command(channel = Channels.Unreliable)]
    void CmdSyncTransform(Vector3 Position, Quaternion Rotation)
    {
        // Ignore messages from client if notCmdClientToServerSync in client authority mode
        if (!clientAuthority)
            return;

        RPCSyncPosition(Position, Rotation);
    }

    [ClientRpc]
    public void RPCSyncPosition(Vector3 Position, Quaternion Rotation)
    {
        if (debug)
            Debug.Log("RPC Position: " + Position);
        RemotePlayerPosition = Position;
        RemotePlayerRotation = Rotation;
    }

    [Client]
    public void HandleRemotePositionUpdates()
    {
        var LagDistance = RemotePlayerPosition - transform.position;

        //High distance => sync is to much off => send to position
        if (LagDistance.magnitude > allowedLagDistance)
        {
            if (debug) 
                Debug.LogWarning("Sync Position to Great");
            controller.position = RemotePlayerPosition;
            LagDistance = Vector3.zero;
        }

        //ignore the y distance
        LagDistance.y = 0;

        if (LagDistance.magnitude < 0.5f)
        {
            //Player is nearly at the point
            networkPlayerController.moveDirection = Vector3.zero;
        }
        else
        {
            //Player has to go to the point
            networkPlayerController.moveDirection = new Vector3(LagDistance.x, 0, LagDistance.z);
        }

        //jump if the remote player is higher than the player on the current client
        if (RemotePlayerPosition.y - transform.position.y > 0.2f)
            networkPlayerController.PerformJump();


    }

    [Command(channel = Channels.Unreliable)]
    private void CmdHandleCrouch(bool CrouchState)
    {
        RpcCrouch(CrouchState);
    }

    [ClientRpc]
    private void RpcCrouch(bool CrouchState)
    {
        if (CrouchState)
        {
            networkPlayerController.PerformCrouch();
        }
        else
        {
            networkPlayerController.PerformUnCrouch();
        }
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdHandleSprint(bool SprintState)
    {
        RpcSprint(SprintState);
    }

    [ClientRpc]
    private void RpcSprint(bool SprintState)
    {
        if (SprintState)
        {
            networkPlayerController.PerformSprint();
        }
        else
        {
            networkPlayerController.PerformWalk();
        }
    }


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
        DrawDataPointGizmo(RemotePlayerPosition, Color.green);
        DrawDataPointGizmo(transPosition, Color.red);

        // draw line between them
        DrawLineBetweenDataPoints(transPosition, RemotePlayerPosition, Color.cyan);
    }
}
