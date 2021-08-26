using System.Collections;
using UnityEngine;

public abstract class AnimatedInteractable : Interactable
{
    [Header("Animation")]
    [SerializeField, Range(0.01f, 10.00f)] protected float animationSpeed = 1f;
    
    protected Animator animator;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = animationSpeed;
    }
}
