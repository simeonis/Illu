using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippledGlass : MonoBehaviour
{
    [SerializeField] private float rippleDuration;
    [SerializeField] private float rippleStrength;
    private IEnumerator ripple;

    private Material material;

    void Start()
    {
        material = transform.GetComponent<Renderer>().material;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            Debug.Log(collider.gameObject.name + " entered Rippled Glass");

            Vector3 worldPostion = collider.ClosestPointOnBounds(transform.position);
            Debug.Log("World Position: " + worldPostion);
            Vector3 localPostion = transform.InverseTransformPoint(worldPostion);
            Debug.Log("Local Position: " + localPostion);

            material.SetVector("_Center", localPostion);

            // if (ripple != null) StopCoroutine(ripple);
            // StartCoroutine(ripple = Ripple());
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            Debug.Log(collider.gameObject.name + " exited Rippled Glass");
        }
    }

    // private IEnumerator Ripple()
    // {
    //     float current = rippleStrength;
    //     float currentCount = 3;
    //     float target = 0;

    //     float percent = 0.0f;
    //     while (percent < 1.0f)
    //     {
    //         percent += Time.deltaTime / rippleDuration;
    //         material.SetFloat("_RippleCount", Mathf.Lerp(currentCount, target, percent));
    //         material.SetFloat("_RippleStrength", Mathf.Lerp(current, target, percent));
    //         yield return null;
    //     }
    // }
}
