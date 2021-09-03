using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private GameObject hostButton = null;
    [SerializeField] private GameObject inviteButton = null;
    [SerializeField] private GameObject startButton = null;
    [SerializeField] private GameObject joinButton = null;
    [SerializeField] private GameObject backButton = null;
    [SerializeField] private GameObject scrollView = null;

    private ListCreator listCreator;

    void Start()
    {
        listCreator = scrollView.GetComponent<ListCreator>();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void HostGame(SteamLobby lobby, List<SteamUserRecord> friends) 
    {
        hostButton.SetActive(false);
        inviteButton.SetActive(true);
        startButton.SetActive(true);

        listCreator.renderList(friends, lobby);
    }

    public void HostGameFailed()
    {
        hostButton.SetActive(true);
        inviteButton.SetActive(false);
        startButton.SetActive(false);
    }

    // CLIENT joined HOST
    public void JoinedHost()
    {
        hostButton.SetActive(false);
        joinButton.SetActive(false);
        // Should replace with "Ready" button
        // startButton.SetActive(true);
    }

    // HOST received CLIENT
    public void ClientJoinedHost()
    {
        scrollView.SetActive(false);
        backButton.SetActive(false);
        inviteButton.SetActive(true);
        startButton.SetActive(true);
        // Should replace with "Ready" button
        // Once both players hit the "Ready" button, then activate start button
        // startButton.SetActive(true);
    }

    public void OpenFriendList()
    {
        scrollView.SetActive(true);
        backButton.SetActive(true);
        inviteButton.SetActive(false);
        startButton.SetActive(false);
    }

    public void ExitFriendList()
    {
        scrollView.SetActive(false);
        backButton.SetActive(false);
        inviteButton.SetActive(true);
        startButton.SetActive(true);
    }

    // TODO: Add SteamRecord parameter so CLIENT knows which HOST invited them
    public void InviteReceived()
    {
        joinButton.SetActive(true);
    }

    public void InviteAccepted()
    {
        joinButton.SetActive(false);
    }

    public void GameStarted()
    {
        lobbyUI.SetActive(false);
    }
}
