using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Linq;

namespace Illu.Networking
{
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
        [HideInInspector] public bool isLanConnection = false;

        public static NetworkManager Instance { get; private set; }

        //Holds all the different Transports for different connection types
        SwitchTransport switchTransport;

        override public void Awake()    
        {
            base.Awake();

            Instance = singleton as NetworkManager;

            // ReadyUpSystem = _readUpSystem;
            switchTransport = (SwitchTransport)transport;

            isLanConnection = false;
            //change this to a call
            //isLanConnection.AddListener(SetConnectionType);

            GameManager.Instance.AddListener(GameManager.Event.ServerStart,     StartHost);
            GameManager.Instance.AddListener(GameManager.Event.ClientStart,     StartClient);
            //GameManager.Instance.AddListener(GameManager.Event.ClientConnected, StartHost);
            GameManager.Instance.AddListener(GameManager.Event.ServerStop,      StopHost);
            GameManager.Instance.AddListener(GameManager.Event.ClientStop,      StopClient);


        }


        //ServerStart -> StartHost
        //ClientStart -> startClient 
        //clientconnected -> null
        //clientdisconnected -> steam lobby disconnect // move to steam 
        //Server Stop -> StartHOst????
        //CLient Stop -> StopCLient 
        //LanJoin -> ClientJoinSever  // should just call direct 

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

            GameManager.Instance.TriggerEvent(GameManager.Event.S_ClientConnected);
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

            GameManager.Instance.TriggerEvent(GameManager.Event.C_ClientConnected);

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

            GameManager.Instance.TriggerEvent(GameManager.Event.C_ClientDisconnected);

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
                GameManager.Instance.TriggerEvent(GameManager.Event.GameStarted);
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

        //
        //  Entry point for starting networking 
        //
        public void SetConnectionType(bool isLan)
        {
            isLanConnection = isLan;
            if (isLan)
            {
                switchTransport.PickTransport(1);
                HostAddress = "localhost";
                Steam.SteamManager.Instance.LobbyLeft();
            }
            else
            {
                switchTransport.PickTransport(0);
                Steam.SteamManager.Instance.HostLobby();
            }
        }

        public void StartHosting()
        {
            Debug.Log("StartHosting");

            if(NetworkServer.active)
                StopHost();

            StartHost();
            if (isLanConnection)
            {
                Steam.SteamManager.Instance.LobbyLeft();
            }
            else
            {
                Steam.SteamManager.Instance.HostLobby();
            }
        }

        //
        //  Entry point for joining a server  
        //
        public void ClientJoinServer()
        {
            //Holds all the different Transports for different connection types
            var switchTransport = (SwitchTransport)transport;

            if (!isLanConnection)
            {
                switchTransport.PickTransport(0);
                //HostAddress = "localhost";
            }
            else
            {
                switchTransport.PickTransport(1);
                HostAddress = "localhost";
            }

            StartClient();
        }

        /*  --------------------------
        *        Helper functions
        *   -------------------------- */

        private void OnCreateCharacter(NetworkConnection conn, CreateCharacterMessage characterMessage)
        {
            // playerPrefab is the one assigned in the inspector in Network
            // Manager but you can use different prefabs per race for example
            GameObject gameobject = Instantiate(playerPrefab);

            // call this to use this gameobject as the primary controller
            NetworkServer.AddPlayerForConnection(conn, gameobject);

            UIConsole.Log("[Server]: Created character " + characterMessage.name + " for Client" + "[" + conn.connectionId + "].");
        }
    }
}