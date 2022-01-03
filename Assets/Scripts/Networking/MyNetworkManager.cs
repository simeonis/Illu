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
    // Prefabs
    [Header("Room")]
    [SerializeField] private NetworkRoomPlayer roomPlayerPrefab = null;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayer gamePlayerPrefab = null;
    [SerializeField] private GameObject playerSpawnSystem = null;

    private int numConnections = 0;
    private readonly string menuScene = "MainMenu";

    public List<NetworkRoomPlayer> RoomPlayers { get; } = new List<NetworkRoomPlayer>();
    public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action OnClientReadied;
    public static event Action<NetworkConnection> OnServerReadied;

    /*  --------------------------
    *       Callback functions
    *   -------------------------- */

    // SERVER started by HOST
    public override void OnStartServer()
    {
        base.OnStartServer();
        numConnections = 0;
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
        NetworkServer.RegisterHandler<CreateCharacterMessage>(OnCreateCharacter);

        UIConsole.Log("Server Started");
    }

    // SERVER detects new connection
    public override void OnServerConnect(NetworkConnection conn)
    {
        UIConsole.Log("[Server]: New connection detected.");
        numConnections++;
        UIConsole.Log("Number of players: " + numConnections);

        if (numConnections > maxConnections)
        {
            conn.Disconnect();
            numConnections--;
            UIConsole.Log("[Server]: Disconnected Client[" + conn.connectionId + "].");
            return;
        }

        if (SceneManager.GetActiveScene().name != menuScene)
        {
            conn.Disconnect();
            numConnections--;
            UIConsole.Log("[Server]: Disconnected Client[" + conn.connectionId + "].");
            return;
        }
    }

    // HOST and CLIENT started
    public override void OnStartClient()
    {
        base.OnStartClient();

        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }

        UIConsole.Log("Client Started");
    }

    // CLIENT connected to SERVER
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

        UIConsole.Log("[Client]: Connected to server.");
    }

    // CLIENT disconnects from SERVER
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();

        UIConsole.Log("[Client]: Disconnected from server.");
    }

    // SERVER gets "Add Player" request from CLIENT
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        UIConsole.Log("[Server]: \"Add Player\" request received.");

        if (SceneManager.GetActiveScene().name == menuScene)
        {
            UIConsole.Log("[Server]: Adding room player.");

            NetworkRoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    // SERVER changes scene for all CLIENTS
    [Server]
    public override void ServerChangeScene(string newSceneName)
    {
        UIConsole.Log("[Server]: Setting up new scene.");

        // From menu to game
        if (SceneManager.GetActiveScene().name == menuScene && newSceneName == "Testing Range")
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab);

                //if(conn.identity.gameObject)
                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
            }
        }

        UIConsole.Log("[Server]: Scene setup completed.");

        base.ServerChangeScene(newSceneName);

        UIConsole.Log("[Server]: Changing scene for all.");
    }

    // CLIENT fully loaded next scene
    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        UIConsole.Log("[Client]: Scene successfully changed.");
        OnClientReadied?.Invoke();
        //UIManager.HideUI();
    }

    // SERVER notified that CLIENT is ready
    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);

        UIConsole.Log("[Server]: Client" + "[" + conn.connectionId + "]"
        + " has successfully loaded scene: " + SceneManager.GetActiveScene().name + ".");
    }

    // SERVER finished loading scene
    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName != menuScene)
        {
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            NetworkServer.Spawn(playerSpawnSystemInstance);
        }


        UIConsole.Log("[Server]: Scene successfully changed.");
    }

    /*  --------------------------
    *   Button activated functions
    *   -------------------------- */

    public void StartGame()
    {
        ServerChangeScene("Testing Range");
    }

    /*  --------------------------
    *        Helper functions
    *   -------------------------- */

    private void OnCreateCharacter(NetworkConnection conn, CreateCharacterMessage characterMessage)
    {
        UIConsole.Log("[Server]: Created character " + characterMessage.name +
        " for Client" + "[" + conn.connectionId + "].");

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
}
