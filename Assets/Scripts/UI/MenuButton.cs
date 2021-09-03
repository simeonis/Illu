using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.VFX;

public class MenuButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Animator animator;
    [SerializeField] GameObject effects;

    #region IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler implementations

    public void OnPointerDown(PointerEventData eventData)
    {
        if (animator) animator.SetBool("pressed", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MyOnClicktriggered();
        if (animator) animator.SetBool("pressed", false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Entered");
        if (animator) animator.SetBool("hovered", true);
        if (effects) effects.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator) animator.SetBool("hovered", false);
        if (effects) effects.SetActive(false);
    }

    #endregion

    //my own event
    [Serializable]
    public class MyOwnEvent : UnityEvent { }

    [SerializeField]
    private MyOwnEvent myOnClick = new MyOwnEvent();
    public MyOwnEvent onMyOnClick { get { return myOnClick; } set { myOnClick = value; } }

    public void MyOnClicktriggered()
    {
        onMyOnClick.Invoke();
    }
}
