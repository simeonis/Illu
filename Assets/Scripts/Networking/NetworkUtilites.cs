using UnityEngine;
using Mirror;

public struct InteractableSyncData
{
    public Vector3 position { get; private set; }
    public Quaternion rotation { get; private set; }
    public long timeSent { get; private set; }

    public InteractableSyncData(Vector3 position, Quaternion rotation, long timeSent)
    {
        this.position = position;
        this.rotation = rotation;
        this.timeSent = timeSent;
    }
}

public struct PlayerSyncData
{
    public Vector3 position { get; private set; }
    public Quaternion headRot { get; private set; }
    public Quaternion bodyRot { get; private set; }
    public Quaternion rootRot { get; private set; }

    public PlayerSyncData(Vector3 position, Quaternion headRot, Quaternion bodyRot, Quaternion rootRot)
    {
        this.position = position;
        this.headRot = headRot;
        this.bodyRot = bodyRot;
        this.rootRot = rootRot;
    }
}

public static class CustomReadWriteFunctions
{
    public static void WriteInteractableSyncData(this NetworkWriter writer, InteractableSyncData interactableSyncData)
    {
        writer.WriteVector3(interactableSyncData.position);
        writer.WriteQuaternion(interactableSyncData.rotation);
        writer.WriteLong(interactableSyncData.timeSent);
    }

    public static InteractableSyncData ReadInteractableSyncData(this NetworkReader reader)
    {
        return new InteractableSyncData(reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadLong());
    }

    public static void WritePlayerSyncData(this NetworkWriter writer, PlayerSyncData playerSyncData)
    {
        writer.WriteVector3(playerSyncData.position);
        writer.WriteQuaternion(playerSyncData.headRot);
        writer.WriteQuaternion(playerSyncData.bodyRot);
        writer.WriteQuaternion(playerSyncData.rootRot);
    }
    public static PlayerSyncData ReadPlayerSyncData(this NetworkReader reader)
    {
        return new PlayerSyncData(reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadQuaternion(), reader.ReadQuaternion());
    }
}