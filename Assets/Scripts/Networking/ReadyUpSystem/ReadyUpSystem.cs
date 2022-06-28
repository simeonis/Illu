using Mirror;
using UnityEngine.Events;
using UnityEngine;

public class MyBoolEvent : UnityEvent<bool> { }
public class ReadyUpSystem : NetworkBehaviour
{
    [HideInInspector] public UnityEvent BothReady = new UnityEvent();
    public MyBoolEvent OneReady = new MyBoolEvent();
    public MyBoolEvent TwoReady = new MyBoolEvent();

    [SyncVar(hook = nameof(PlayerOneStatus))]
    public bool playerOneReady = false;

    [SyncVar(hook = nameof(PlayerTwoStatus))]
    public bool playerTwoReady = false;

    [System.Serializable]
    public enum ID
    {   
        playerOne,
        playerTwo
    }

    public SyncList<ID> assigned = new SyncList<ID>();

    public ID myID;

    private void PlayerOneStatus(bool oldValue, bool newValue)
    {
        OneReady.Invoke(newValue);
        CheckReady();
    }

    private void PlayerTwoStatus(bool oldValue, bool newValue)
    {
        TwoReady.Invoke(newValue);
        CheckReady();
    }

    private void CheckReady()
    {

        if (playerOneReady && playerTwoReady)
        {
            BothReady?.Invoke();
        }
    }

}