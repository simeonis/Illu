using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippledGlass : MonoBehaviour
{
    [SerializeField] private float rippleDuration;
    private IEnumerator ripple;
    private Material material;

    // [SerializeField] private AudioSource glassAudioSource;
    // [SerializeField] private AudioEvent glassAudioEvent;


    void Start()
    {
        material = transform.GetComponent<Renderer>().material;
    }

    void OnTriggerEnter(Collider collider)
    {

        Vector3 worldPostion = collider.ClosestPointOnBounds(transform.position);
        Vector3 localPostion = transform.InverseTransformPoint(worldPostion);

        material.SetVector("_Center", localPostion);

        if (ripple != null) StopCoroutine(ripple);
        StartCoroutine(ripple = Ripple());

        //glassAudioEvent.Play(glassAudioSource);

    }

    private IEnumerator Ripple()
    {
        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / rippleDuration;
            material.SetFloat("_Percent", percent);
            yield return null;
        }
    }
}
