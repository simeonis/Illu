using System.Collections;
using UnityEngine;


public class Lever : AnimatedInteractable
{
    [Header("Target Script")]
    [SerializeField] private Trigger target;

    private bool onState = false;
    private bool locked = false;
    private IEnumerator enumerator;
    private NetworkSimpleData networkSimpleData;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        networkSimpleData = GetComponent<NetworkSimpleData>();
        networkSimpleData.RegisterData("LEVER_PULL", onState);
        networkSimpleData.DataChanged += LeverEventHandler;
    }

    public override void OnStartAuthority()
    {
        networkSimpleData.SendData("LEVER_PULL", onState);
    }

    void LeverEventHandler(object sender, DataChangedEventArgs e)
    {
        Debug.Log("Received data from other client");
        if (e.key == "LEVER_PULL")
        {
            onState = (bool)networkSimpleData.GetData(e.key);
            if (enumerator != null) StopCoroutine(enumerator);
            StartCoroutine(enumerator = Switch());
        }
    }

    public override void Interaction(Interactor interactor)
    {
        if (!enabled || locked || !target) return;

        Debug.Log("Yeet1");

        // Request Authority
        base.Interaction(interactor);

        locked = true;
        onState = !onState;
        if (enumerator != null) StopCoroutine(enumerator);
        StartCoroutine(enumerator = Switch());
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