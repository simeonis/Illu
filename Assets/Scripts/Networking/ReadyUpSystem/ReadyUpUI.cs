using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ReadyUpUI : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] RectTransform readyUp;
    [SerializeField] RectTransform startGame;
    [SerializeField] Button startBtn;

    Illu.Networking.ReadyUpSystem readyUpSystem;

    bool receivedAuthority = false;

    [SyncVar]
    [SerializeField] int myID = -1;

    void Start()
    {
        readyUpSystem = FindObjectOfType<Illu.Networking.ReadyUpSystem>();
        readyUpSystem.BothReady.AddListener(OnBothReady);

        startBtn.onClick.AddListener(OnStartBtnClick);

        if (receivedAuthority)
            RequestID();
    }

    public override void OnStopClient() => readyUpSystem.RemovePlayer();

    void OnBothReady(bool ready)
    {
        if (ready && isServer)
        {
            readyUp.gameObject.SetActive(false);
            startBtn.gameObject.SetActive(true);
        }
    }

    void OnStartBtnClick() => GameManager.Instance.TriggerEvent(GameManager.Event.GameStart);

    public override void OnStartAuthority() => receivedAuthority = true;

    [Client]
    public void ReadyUP()
    {
        if(myID != -1)
            CMDSetStatus(myID, true);
    }

    [Client]
    public void CancelReadyUP() 
    {
        if(myID != -1)
            CMDSetStatus(myID, false);
    } 

    [Command]
    public void CMDSetStatus(int myID, bool status)
    {
        if (myID == 0)
        {
            readyUpSystem.playerOneReady = status;
        }
        else
        {
            readyUpSystem.playerTwoReady = status;
        }

        var conn = gameObject.GetComponent<NetworkIdentity>();
    }

    [Command]
    public void RequestID()
    {
        myID = readyUpSystem.AddPLayer();
    }
}