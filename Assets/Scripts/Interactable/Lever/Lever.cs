using System.Collections;
using UnityEngine;

public class Lever : AnimatedInteractable
{
    [Header("Target Script")]
    [SerializeField] private Trigger target;
    
    private bool onState = false;
    private bool locked = false;
    private IEnumerator enumerator;

    public override void Interaction(Interactor interactor)
    {
        if (!enabled || locked || !target) return;

        locked = true;
        onState = !onState;
        if (enumerator != null) StopCoroutine(enumerator);
        StartCoroutine(enumerator = Switch());
    }

    public override void InteractionCancelled(Interactor interactor){}

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
