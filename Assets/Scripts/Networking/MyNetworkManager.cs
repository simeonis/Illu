using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Linq;

public struct CreateCharacterMessage : NetworkMessage
{
    public string name;
}

public class MyNetworkManager : NetworkManager
{
    [Header("Room")]
    [SerializeField] private NetworkRoomPlayer roomPlayerPrefab = null;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayer gamePlayerPrefab = null;
    [SerializeField] private GameObject playerSpawnSystem = null;

    [SerializeField] private GameObject launchUI = null;

    private readonly string menuScene = "LaunchScreen";

    public List<NetworkRoomPlayer> RoomPlayers { get; } = new List<NetworkRoomPlayer>();
    public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();


    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;
    public static event Action OnServerStopped;


    public override void OnStartServer()
    {
        Debug.Log("Server Started");
        base.OnStartServer();

        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

        NetworkServer.RegisterHandler<CreateCharacterMessage>(OnCreateCharacter);
    }

    public override void OnStartClient()
    {
        Debug.Log("Client Started");
        base.OnStartClient();

        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();

        // you can send the message here, or wherever else you want
        CreateCharacterMessage characterMessage = new CreateCharacterMessage
        {
            name = "Joe Gaba Gaba",
        };

        conn.Send(characterMessage);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        //if (numPlayers >= maxConnections)
        //{
        //    conn.Disconnect();
        //    return;
        //}

        //if (SceneManager.GetActiveScene().name != menuScene)
        //{
        //    conn.Disconnect();
        //    return;
        //}
    }

    //[Server]
    public override void ServerChangeScene(string newSceneName)
    {
        Debug.Log("Server Change Scene");
        // From menu to game
        if (SceneManager.GetActiveScene().name == "LaunchScreen" && newSceneName == "Hub")
        {
            Debug.Log("RoomPlayers Count" + RoomPlayers.Count);
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab);


                //if(conn.identity.gameObject)
                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
            }
        }

        Debug.Log("Before Base ServerChangeScene");

        base.ServerChangeScene(newSceneName);
    }


    public void StartGame()
    {
        ServerChangeScene("Hub");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("OnServerAddPlayer: called");

        if (SceneManager.GetActiveScene().name == menuScene)
        {
            Debug.Log("OnServerAddPlayer: In Menu");

            NetworkRoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName != menuScene)
        {
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            NetworkServer.Spawn(playerSpawnSystemInstance);
        }
    }

    void OnCreateCharacter(NetworkConnection conn, CreateCharacterMessage message)
    {
        // playerPrefab is the one assigned in the inspector in Network
        // Manager but you can use different prefabs per race for example
        GameObject gameobject = Instantiate(playerPrefab);

        // Apply data from the message however appropriate for your game
        // Typically Player would be a component you write with syncvars or properties
        //Player player = gameobject.GetComponent();
        //player.hairColor = message.hairColor;
        //player.eyeColor = message.eyeColor;
        //player.name = message.name;
        //player.race = message.race;

        // call this to use this gameobject as the primary controller
        NetworkServer.AddPlayerForConnection(conn, gameobject);
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);

        launchUI.SetActive(false);
    }


}
