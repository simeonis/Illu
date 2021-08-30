using System.Collections;
using UnityEngine;

public class IronBar : Trigger
{
    [SerializeField] private bool closed = true;
    [SerializeField] private float transitionLength = 1f;

    private float defaultHeight;
    private BoxCollider hitbox;

    void Start()
    {
        // Setup
        defaultHeight = transform.localScale.y;
        hitbox = GetComponent<BoxCollider>();

        // Set door state
        gameObject.SetActive(closed);
        hitbox.enabled = closed;
        transform.localScale = new Vector3(transform.localScale.x, closed ? defaultHeight : 0f, transform.localScale.z);
    }

    public override void Activate(Illu_Interactable.Button button)
    {
        closed = !closed;

        // GameObject must be active to run coroutine
        gameObject.SetActive(true);
        StartCoroutine(Open(button));
    }

    private IEnumerator Open(Illu_Interactable.Button button)
    {
        Vector3 currentScale = transform.localScale;
        Vector3 targetScale = new Vector3(currentScale.x, closed ? defaultHeight : 0f, currentScale.z);

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / transitionLength;
            transform.localScale = Vector3.Lerp(currentScale, targetScale, percent);
            yield return null;
        }

        // Hides bar when fully opened
        if (!closed) gameObject.SetActive(false);

        // Toggles hitbox
        hitbox.enabled = closed;

        // Allows button to be pressed again
        button.Reset();
    }
}
