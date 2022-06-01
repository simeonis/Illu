using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
public class ToggleButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject switchTarget;
    [SerializeField] GameObject track;
    [SerializeField] GameObject start;
    [SerializeField] GameObject end;
    [SerializeField] Color startColor;
    [SerializeField] Color endColor;
    [SerializeField] Color knobColor;
    [SerializeField] float Speed = 0.5f;
    [SerializeField] UnityEvent onClickAction;

    Image trackImage;
    Image switchImage;
    //bool toggle = false;

    [SerializeField] BoolVariable toggle;

    void OnEnable()
    {
        switchTarget.transform.position = start.transform.position;

        startColor.a = 1;
        endColor.a = 1;
        knobColor.a = 1;

        switchImage = switchTarget.GetComponent<Image>();
        switchImage.color = knobColor;
        trackImage = track.GetComponent<Image>();
        trackImage.color = startColor;

        toggle.AddListener(OnToggle);

        OnToggle();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        toggle.Toggle();
        onClickAction?.Invoke();
    }


    void OnToggle()
    {
        if (toggle.Value)
        {
            switchTarget.transform.DOMoveX(end.transform.position.x, Speed);
            trackImage.DOColor(endColor, Speed);

        }
        else
        {
            switchTarget.transform.DOMoveX(start.transform.position.x, Speed);
            trackImage.DOColor(startColor, Speed);
        }

    }

}
