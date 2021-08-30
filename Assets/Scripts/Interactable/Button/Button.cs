using UnityEngine;

namespace Illu_Interactable
{
    public class Button : AnimatedInteractable
    {
        [Header("Target Script")]
        [SerializeField] private Trigger target;
        private bool locked = false;
        private NetworkSimpleData networkSimpleData;

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            networkSimpleData = GetComponent<NetworkSimpleData>();
            networkSimpleData.RegisterData("BUTTON_PRESSED", locked);
            networkSimpleData.DataChanged += ButtonEventHandler;
        }

        private void ButtonEventHandler(object sender, DataChangedEventArgs e)
        {   
            if (e.key == "BUTTON_PRESSED")
            {

            }
        }

        public override void OnStartAuthority()
        {
            throw new System.NotImplementedException();
        }

        public override void Interaction(Interactor interactor)
        {
            if (!enabled || locked || !target) return;

            locked = true;
            animator.SetBool("Pressed", true);
            target.Activate(this);
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