using UnityEngine;

namespace IlluInteractable
{
    public class Button : AnimatedInteractable
    {
        [Header("Target Script")]
        [SerializeField] private Trigger target;
        private bool locked = false;

        protected override void Awake() { base.Awake(); }

        public override void Interaction(Interactor interactor)
        {
            if (!enabled || locked || !target) return;

            locked = true;
            animator.SetBool("Pressed", true);
            target.Activate(this);
            //audioEvent[0].Play(audioSource);
        }

        public override void InteractionCancelled(Interactor interactor)
        {
            animator.SetBool("Pressed", false);
        }

        public void Reset()
        {
            locked = false;
        }
    }
}