using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIPulse : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] private float maximumPulse = 1f;
    [SerializeField][Range(0f, 1f)] private float minimumPulse = 0.75f;
    [SerializeField][Range(0f, 2f)] private float frequency = 0.5f;
    private TextMeshProUGUI textMeshPro;

    // Start is called before the first frame update
    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        if (!textMeshPro)
        {
            Debug.LogWarning("Object does not have a TextMeshPro component.\nDisabling this script.");
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float positiveSin = 0.5f * Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) + 0.5f;
        float pulse = positiveSin * (maximumPulse - minimumPulse) + minimumPulse;
        textMeshPro.color = Color.white * pulse;
    }
}