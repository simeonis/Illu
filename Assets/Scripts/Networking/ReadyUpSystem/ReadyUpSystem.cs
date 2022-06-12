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
    public bool playerOneReady;

    [SyncVar(hook = nameof(PlayerTwoStatus))]
    public bool playerTwoReady = true;

    public enum ID
    {   
        playerOne,
        playerTwo
    }
    ID[] assigned = new ID[2];

    public static ReadyUpSystem Instance { get; private set; }

     public void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance =  this;
        }
    }

    void OnEnable()
    {
        Debug.Log("I've been enabled");
    }


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

    public ID requestID()
    {
        if (assigned.Length == 0)
        {
            assigned[0] = ID.playerOne;
            return ID.playerOne;
        }
        else
        {
            assigned[1] = ID.playerTwo;
            return ID.playerOne;
        }
    }
}