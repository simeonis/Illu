using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Mirror;

namespace Illu.Networking
{
    public struct CreateLobbyPlayerMessage : NetworkMessage {}

    public class NetworkManager : Mirror.NetworkManager
    {
        [SerializeField] string menuScene;

        [Header("Lobby")]
        [SerializeField] GameObject lobbyPlayerPrefab;

        [Header("Game")]
        [SerializeField] NetworkGamePlayer gamePlayerPrefab = null;
        [SerializeField] GameObject playerSpawnSystem = null;

        public List<NetworkLobbyPlayer> LobbyPlayers { get; } = new List<NetworkLobbyPlayer>();
        public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();

        public static event Action<NetworkConnection> OnServerReadied;
        public UnityEvent clientConnect    = new UnityEvent();
        public UnityEvent clientDisconnect = new UnityEvent();

        public string HostAddress { get => networkAddress; set => networkAddress = value; }
        public bool isLanConnection = false;

        public static NetworkManager Instance { get; private set; }

        // Holds all the different Transports for different connection types
        SwitchTransport switchTransport;

        override public void Awake()    
        {
            base.Awake();

            Instance = singleton as NetworkManager;

            switchTransport = (SwitchTransport)transport;

            isLanConnection = false;
        }

        void OnEnable() => GameManager.Instance.AddListener(GameManager.Event.GameStart, StartGame);
        void OnDisable() => GameManager.Instance.RemoveListener(GameManager.Event.GameStart, StartGame);

        /*  --------------------------
        *       Callback functions
        *   -------------------------- */

        // SERVER started by HOST
        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.RegisterHandler<CreateLobbyPlayerMessage>(OnCreateLobbyPlayer);
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

            clientConnect?.Invoke();
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            clientDisconnect?.Invoke();
        }

        // CLIENT connected to SERVER
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
   
            conn.Send(new CreateLobbyPlayerMessage());

            UIConsole.Log("[Client]: Connected to server.");
        }

        // CLIENT disconnects from SERVER
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);

            if (SceneManager.GetActiveScene().name != menuScene)
                SceneManager.LoadScene(menuScene);

            if (!isLanConnection)
                Steam.SteamManager.Instance.LobbyDisconnected();
            else
                EnableLAN(false);

            UIConsole.Log("[Client]: Disconnected from server.");
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
                GameManager.Instance.TriggerEvent(GameManager.Event.GameStart);
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
        
        [Server]
        void StartGame() => ServerChangeScene("LevelOne");

        // SERVER changes scene for all CLIENTS
        [Server]
        public override void ServerChangeScene(string newSceneName)
        {
            UIConsole.Log("[Server]: Setting up new scene.");

            // Game Started
            if (SceneManager.GetActiveScene().name == menuScene && newSceneName != menuScene)
            {
                for (int i = LobbyPlayers.Count - 1; i >= 0; i--)
                {
                    var conn = LobbyPlayers[i].connectionToClient;
                    var gameplayerInstance = Instantiate(gamePlayerPrefab);

                    NetworkServer.Destroy(conn.identity.gameObject);
                    NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
                }
            }

            UIConsole.Log("[Server]: Scene setup completed.");

            base.ServerChangeScene(newSceneName);

            UIConsole.Log("[Server]: Changing scene for all.");
        }

        public void EnableLAN(bool isLan)
        {
            isLanConnection = isLan;
            
            if (!isLan)
                switchTransport.PickTransport(0);
            else
            {
                switchTransport.PickTransport(1);
                HostAddress = "localhost";
            }
        }

        public void JoinLAN()
        {
            EnableLAN(true);
            StartClient();
        }

        public void StartHosting()
        {
            if (NetworkServer.active)
                StopHost();

            StartHost();

            if(!isLanConnection)
                Steam.SteamManager.Instance.HostLobby();
        }

        /*  --------------------------
        *        Helper functions
        *   -------------------------- */

        private void OnCreateLobbyPlayer(NetworkConnection conn, CreateLobbyPlayerMessage msg)
        {
            // playerPrefab is the one assigned in the inspector in Network
            // Manager but you can use different prefabs per race for example
            GameObject lobbyPlayer = Instantiate(lobbyPlayerPrefab);

            UIConsole.Log("Attempting OnCreateLobbyPlayer ");

            // call this to use this gameobject as the primary controller
            NetworkServer.AddPlayerForConnection(conn, lobbyPlayer);

            UIConsole.Log("[Server]: Created lobby player for Client" + "[" + conn.connectionId + "].");
        }
    }
}