using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject VFX;

    #region IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler implementations

    public void OnPointerDown(PointerEventData eventData)
    {
        animator.SetBool("pressed", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MyOnClicktriggered();
        animator.SetBool("pressed", false);
        if (VFX) VFX.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool("hovered", true);
        if (VFX) VFX.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("hovered", false);
        if (VFX) VFX.SetActive(false);
    }

    #endregion

    // Custom Event
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
