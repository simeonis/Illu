using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Linq;

namespace Illu.Networking {
    public struct CreateCharacterMessage : NetworkMessage
    {
        public string name;
    }

    public class NetworkManager : Mirror.NetworkManager
    {
        // Prefabs
        [Header("Room")]
        [SerializeField] private NetworkRoomPlayer roomPlayerPrefab = null;

        [Header("Game")]
        [SerializeField] private NetworkGamePlayer gamePlayerPrefab = null;
        [SerializeField] private GameObject playerSpawnSystem = null;

        private readonly string menuScene = "Main Menu";

        public List<NetworkRoomPlayer> RoomPlayers { get; } = new List<NetworkRoomPlayer>();
        public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();

        public static event Action<NetworkConnection> OnServerReadied;

        [HideInInspector] public static string HostAddress = "";

        /*  --------------------------
        *       Callback functions
        *   -------------------------- */

        // SERVER started by HOST
        public override void OnStartServer()
        {
            base.OnStartServer();
            spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
            NetworkServer.RegisterHandler<CreateCharacterMessage>(OnCreateCharacter);
            UIConsole.Log("Server Started");
        }

        // SERVER detects new connection
        public override void OnServerConnect(NetworkConnection conn)
        {
            int numConnections = NetworkServer.connections.Count;
            UIConsole.Log("[Server]: New connection detected.");
            UIConsole.Log("Number of players: " + numConnections);

            if (numConnections > maxConnections)
            {
                conn.Disconnect();
                UIConsole.Log("[Server]: Disconnected Client[" + conn.connectionId + "].");
                return;
            }

            if (SceneManager.GetActiveScene().name != menuScene)
            {
                conn.Disconnect();
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

            GameManager.TriggerEvent("ClientConnected");

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

            GameManager.TriggerEvent("ClientDisconnected");

            if (SceneManager.GetActiveScene().name != menuScene)
            {
                SceneManager.LoadScene(menuScene);
            }

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

        // CLIENT is changing scene
        private string previousScene;
        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            previousScene = SceneManager.GetActiveScene().name;
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
        }

        // CLIENT fully loaded next scene
        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            base.OnClientSceneChanged(conn);

            string sceneName = SceneManager.GetActiveScene().name;

            // Game Started
            if (previousScene == menuScene && sceneName != menuScene)
            {
                GameManager.TriggerEvent("GameStarted");
                UIConsole.Log("[Client]: Server has started the game");
            }

            UIConsole.Log("[Client]: Sucessfully loaded " + sceneName + ".");
        }

        // SERVER notified that CLIENT is ready
        public override void OnServerReady(NetworkConnection conn)
        {
            base.OnServerReady(conn);
            UIConsole.Log("[Server]: Client" + "[" + conn.connectionId + "]"
            + " has successfully loaded scene: " + SceneManager.GetActiveScene().name + ".");
            OnServerReadied?.Invoke(conn);  
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
        *        Regular functions
        *   -------------------------- */

        // SERVER changes scene for all CLIENTS
        [Server]
        public override void ServerChangeScene(string newSceneName)
        {
            UIConsole.Log("[Server]: Setting up new scene.");

            // Game Started
            if (SceneManager.GetActiveScene().name == menuScene && newSceneName != menuScene)
            {
                for (int i = RoomPlayers.Count - 1; i >= 0; i--)
                {
                    var conn = RoomPlayers[i].connectionToClient;
                    var gameplayerInstance = Instantiate(gamePlayerPrefab);

                    NetworkServer.Destroy(conn.identity.gameObject);
                    NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
                }
            }

            UIConsole.Log("[Server]: Scene setup completed.");

            base.ServerChangeScene(newSceneName);

            UIConsole.Log("[Server]: Changing scene for all.");
        }

        public override void StartClient()
        {
            networkAddress = HostAddress;
            base.StartClient();
        }

        /*  --------------------------
        *        Helper functions
        *   -------------------------- */

        private void OnCreateCharacter(NetworkConnection conn, CreateCharacterMessage characterMessage)
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

            UIConsole.Log("[Server]: Created character " + characterMessage.name +
            " for Client" + "[" + conn.connectionId + "].");
        }
    }
}
