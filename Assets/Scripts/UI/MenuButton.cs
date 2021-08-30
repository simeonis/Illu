using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Animator animator;

    #region IPointerDownHandler, IPointerUpHandler,  IPointerEnterHandler, IPointerExitHandler implementations

    public void OnPointerDown(PointerEventData eventData)
    {
        animator.SetBool("pressed", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MyOnClicktriggered();
        animator.SetBool("pressed", false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool("hovered", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("hovered", false);
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
