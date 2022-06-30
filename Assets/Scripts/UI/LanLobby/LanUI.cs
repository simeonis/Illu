using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class LanUI : NetworkBehaviour
{

    public struct LanPlayer{
        public string Name;
        public uint id;

        public LanPlayer(string name, uint id)
        {
            Name = name;
            this.id = id;
        }
    }

    Illu.Networking.ReadyUpSystemReference readyUpSystemReference;
    [SerializeField] RectTransform lobbyHostTarget;
    [SerializeField] RectTransform lobbyClientTarget;

    [SerializeField] GameObject _lobbyPrefab;

    List<LanLobbyMember> playerCards = new List<LanLobbyMember>();

    void Start()
    {
        readyUpSystemReference = FindObjectOfType<Illu.Networking.ReadyUpSystemReference>();
        readyUpSystemReference.OneReady.AddListener(setPlayerOneStatus);
        readyUpSystemReference.TwoReady.AddListener(setPlayerTwoStatus);
   
    }

    void OnEnable()
    {
        Illu.Networking.NetworkManager.Instance.clientConnect.AddListener(OnClientConnect);
        Illu.Networking.NetworkManager.Instance.clientDisconnect.AddListener(OnClientDisconnect);
    }

    void OnDisable()
    {
        Illu.Networking.NetworkManager.Instance.clientConnect.RemoveListener(OnClientConnect);
        Illu.Networking.NetworkManager.Instance.clientDisconnect.RemoveListener(OnClientDisconnect);

        if (readyUpSystemReference)
        {
            readyUpSystemReference.OneReady.RemoveListener(setPlayerOneStatus);
            readyUpSystemReference.TwoReady.RemoveListener(setPlayerTwoStatus);
        }
    }

    override public void OnStartServer() 
    {
        if(Illu.Networking.NetworkManager.Instance.isLanConnection)
        {
            GenerateLobbyMember(new LanPlayer("Player1", 0), true);
        }
    }

    void OnClientConnect()
    {
         if(Illu.Networking.NetworkManager.Instance.isLanConnection)
         {
            Debug.Log("SOnClientConnect");
            GenerateLobbyMember(new LanPlayer("Player2", 1), false);
         }
    }

    override public void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("OnStartClient");

        if(Illu.Networking.NetworkManager.Instance.isLanConnection && !isServer)
        {
            Debug.Log("OnCLientConnect and Is Lan");
            GenerateLobbyMember(new LanPlayer("Player1", 0), true);
            GenerateLobbyMember(new LanPlayer("Player2", 1), false);
        } 
    }
    
    public void OnClientDisconnect() => DestroyLobbyClient();  

    void DestroyLobbyClient()
    {
        if (Illu.Networking.NetworkManager.Instance.isLanConnection && lobbyClientTarget.childCount > 0)
        {
            foreach (Transform child in lobbyClientTarget) Destroy(child.gameObject);
        }
    }

    void GenerateLobbyMember(LanPlayer player, bool hostSlot)
    {
        GameObject lobbyUser = Instantiate(_lobbyPrefab, hostSlot ? lobbyHostTarget : lobbyClientTarget);
        LanLobbyMember lobbyUserDetails = lobbyUser.GetComponent<LanLobbyMember>();
        lobbyUserDetails.Create(player);
        playerCards.Add(lobbyUserDetails);
    }

    void setPlayerOneStatus(bool status) => SetIndicatorOnPlayerCard(status, 0);
    void setPlayerTwoStatus(bool status) => SetIndicatorOnPlayerCard(status, 1);

    void SetIndicatorOnPlayerCard(bool status, int caller)
    {
        if (Illu.Networking.NetworkManager.Instance.isLanConnection && caller < playerCards.Count)
            playerCards[caller].SetIndicator(status);
    }
}