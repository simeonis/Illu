using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ReadyUpSystem : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] RectTransform readyUp;
    [SerializeField] RectTransform startGame;
    [SerializeField] Button startBTN;

    Illu.Networking.ReadyUpSystemReference readyUpSystemReference;

    bool receivedAuthority = false;

    [SyncVar]
    [SerializeField] int myID = -1;

    void Start()
    {
        readyUpSystemReference = FindObjectOfType<Illu.Networking.ReadyUpSystemReference>();
        readyUpSystemReference.BothReady.AddListener(OnBothReady);

        startBTN.onClick.AddListener(OnStartBtnClick);

        if (receivedAuthority)
            RequestID();
    }

    void OnBothReady(bool ready)
    {
        if (ready && isServer)
        {
            readyUp.gameObject.SetActive(false);
            startBTN.gameObject.SetActive(true);

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
    public void CancelReadyUP() => CMDSetStatus(myID, false);

    [Command]
    public void CMDSetStatus(int myID, bool status)
    {
        if (myID == 0)
        {
            readyUpSystemReference.playerOneReady = status;
        }
        else
        {
            readyUpSystemReference.playerTwoReady = status;
        }
    }

    [Command]
    public void RequestID()
    {
        myID = readyUpSystemReference.AddPLayer();
    }
}