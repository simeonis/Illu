using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UnityEvent OnClick;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (!animator) 
        {
            gameObject.AddComponent<Animator>();
            Debug.LogWarning($"{this.name} does not have an Animator attatched.");
        }
    }

    #region IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler implementations

    public void OnPointerDown(PointerEventData eventData)
    {
        animator.SetBool("pressed", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnClick.Invoke();
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
}
