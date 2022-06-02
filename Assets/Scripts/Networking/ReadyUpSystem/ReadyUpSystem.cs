using Mirror;
using UnityEngine.Events;
public class ReadyUpSystem : NetworkBehaviour
{
    public UnityEvent BothReady = new UnityEvent();
    [SyncVar] bool playerOneReady;
    [SyncVar] bool playerTwoReady;

    public enum ID
    {
        playerOne,
        playerTwo
    }
    ID[] assigned = new ID[2];

    public void SetReadyStatus(ID id, bool status)
    {
        //CMDSetStatus(id, status);
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