using Mirror;
using UnityEngine.Events;

[System.Serializable]
public class MyBoolEvent : UnityEvent<bool> { }
public class ReadyUpSystem : NetworkBehaviour
{
    public UnityEvent BothReady = new UnityEvent();
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