using System.Collections;
using Mirror;
using UnityEngine;


public class Lever : AnimatedInteractable
{
    [Header("Target Script")]
    [SerializeField] private Trigger target;

    private bool onState = false;
    private bool locked = false;
    private IEnumerator enumerator;
    NetworkSimpleData _networkSimpeData;

    protected override void Start()
    {
        base.Start();
        _networkSimpeData = GetComponent<NetworkSimpleData>();
        _networkSimpeData.DataChanged += l_EventHandler;
    }

    void l_EventHandler(object sender, DataChangedEventArgs e)
    {
        Debug.Log("From lever " + e.data + " changed at " + e.TimeSent);

        onState = e.data;
        if (enumerator != null) StopCoroutine(enumerator);
        StartCoroutine(enumerator = Switch());
    }

    public override void Interaction(Interactor interactor)
    {
        if (!enabled || locked || !target) return;

        locked = true;
        onState = !onState;
        if (enumerator != null) StopCoroutine(enumerator);
        StartCoroutine(enumerator = Switch());

        Debug.Log("Interact: Lever");

        if (interactor is Player)
        {
            Player player = interactor as Player;
            NetworkIdentity ni = GetComponent<NetworkIdentity>();
            player.GetAuthority(ni);
        }

        _networkSimpeData.SendData(onState);
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
        Debug.Log("I the lever, the not so mighty lever, have lost authority");
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log("I the lever, the mighty lever, have been granted authority.");
    }

    public override void InteractionCancelled(Interactor interactor) 
    { 
        if (interactor is Player)
        {
            Player player = interactor as Player;
            NetworkIdentity ni = GetComponent<NetworkIdentity>();
            player.RemoveAuthority(ni);
        }
    }

    private IEnumerator Switch()
    {
        float currentState = onState ? 0 : 1;
        float targetState = onState ? 1 : 0;

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime * animationSpeed;

            animator.SetFloat("Pulled", Mathf.Lerp(currentState, targetState, percent));

            yield return null;
        }

        locked = false;
        target.Activate();
    }
}
