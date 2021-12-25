using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab1 = null;
    [SerializeField] private GameObject playerPrefab2 = null;

    private static List<Transform> spawnPoints = new List<Transform>();

    private int nextIndex = 0;

    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }

    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    public override void OnStartServer() 
    {
        base.OnStartServer();
        MyNetworkManager.OnServerReadied += SpawnPlayer;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        MyNetworkManager.OnServerReadied -= SpawnPlayer;
        nextIndex = 0;
    }

    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(nextIndex);

        if (spawnPoint == null)
        {
            Debug.LogError($"Missing spawn point for player #{nextIndex + 1}");
            return;
        }

        Debug.Log("[Server]: Spawning player " + "at position: " + spawnPoint.position + " for Client[" + conn.connectionId + "].");

        GameObject playerPrefab = (nextIndex % 2 == 0) ? playerPrefab1 : playerPrefab2; 
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);
        NetworkServer.Spawn(playerInstance, conn);

        nextIndex++;
    }
}
