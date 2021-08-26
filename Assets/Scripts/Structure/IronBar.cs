using System.Collections;
using UnityEngine;

public class IronBar : Trigger
{
    [Header("Functionality")]
    [SerializeField] private float openDuration = 1f;

    public override void Activate(Illu_Interactable.Button button)
    {
        StartCoroutine(Open(button));
    }

    private IEnumerator Open(Illu_Interactable.Button button)
    {
        Vector3 currentScale = transform.localScale;
        Vector3 targetScale = new Vector3(currentScale.x, 0f, currentScale.z);

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / openDuration;
            transform.localScale = Vector3.Lerp(currentScale, targetScale, percent);
            yield return null;
        }

        Destroy(gameObject);
        button.Reset();
    }
}
