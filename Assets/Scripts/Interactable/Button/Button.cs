using UnityEngine;

namespace Illu_Interactable
{
    public class Button : AnimatedInteractable
    {
        [Header("Target Script")]
        [SerializeField] private Trigger target;
        private bool locked = false;

        protected override void Awake() { base.Awake(); }

        void Start()
        {
            networkSimpleData.DataChanged += ButtonEventHandler;
        }

        public override void OnStartAuthority()
        {
            networkSimpleData.SendData("BUTTON_PRESSED");
        }

        private void ButtonEventHandler(object sender, DataChangedEventArgs e)
        {   
            if (e.key == "BUTTON_PRESSED" && enabled && !locked)
            {
                locked = true;
                animator.SetBool("Pressed", true);
                target.Activate(this);
            }
        }

        public override void Interaction(Interactor interactor)
        {
            if (!enabled || locked || !target) return;

            // Request authority
            base.Interaction(interactor);

            locked = true;
            animator.SetBool("Pressed", true);
            target.Activate(this);
        }

        public override void InteractionCancelled(Interactor interactor)
        {
            // Remove authority
            base.InteractionCancelled(interactor);
            animator.SetBool("Pressed", false);
        }

        public void Reset()
        {
            locked = false;
        }
    }
}