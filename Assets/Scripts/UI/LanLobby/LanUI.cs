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

    [SerializeField] private RectTransform lobbyHostTarget;
    [SerializeField] private RectTransform lobbyClientTarget;

    [SerializeField] private GameObject _lobbyPrefab;

    List<LanLobbyMember> playerCards = new List<LanLobbyMember>();

    [SerializeField] BoolVariable isLanConnection;


    [SerializeField] Event clientConnected;
    [SerializeField] Event clientDisconnected;


    void OnEnable()
    {
        Illu.Networking.NetworkManager.Instance.ReadyUpSystem.OneReady.AddListener(setPlayerOneStatus);
        Illu.Networking.NetworkManager.Instance.ReadyUpSystem.TwoReady.AddListener(setPlayerTwoStatus);
    }

    void OnDisable()
    {
        Illu.Networking.NetworkManager.Instance.ReadyUpSystem.OneReady.RemoveListener(setPlayerOneStatus);
        Illu.Networking.NetworkManager.Instance.ReadyUpSystem.TwoReady.RemoveListener(setPlayerTwoStatus);
    }

    override public void OnStartServer() 
    {
        if(isLanConnection.Value)
        {
            GenerateLobbyMember(new LanPlayer("Player1", 0), true);
        }
    }

    public void OnClientConnect()
    {
        if(isLanConnection.Value)
        {
            GenerateLobbyMember(new LanPlayer("Player2", 1), false);
        } 
    }
    
    public void OnClientDisconnect() => DestroyLobbyClient();  

    void DestroyLobbyClient()
    {
        if (lobbyClientTarget.childCount > 0)
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
        if (caller < playerCards.Count)
            playerCards[caller].SetIndicator(status);
    }
}