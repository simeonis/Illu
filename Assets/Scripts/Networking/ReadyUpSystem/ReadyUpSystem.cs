using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace  Illu.Networking
{
    public class ReadyUpSystem : NetworkBehaviour
    {
        [Header("Events")]
        [HideInInspector] public UnityEvent<bool> BothReady = new UnityEvent<bool>();
        [HideInInspector] public UnityEvent<bool> OneReady = new UnityEvent<bool>();
        [HideInInspector] public UnityEvent<bool> TwoReady = new UnityEvent<bool>();

        [SyncVar(hook = nameof(PlayerOneStatus))]
        public bool playerOneReady = false;

        [SyncVar(hook = nameof(PlayerTwoStatus))]
        public bool playerTwoReady = false;

        [SyncVar, SerializeField] int idCount = 0;

        public int AddPLayer()
        {
            var id = idCount;
            idCount++;
            return id;
        }

        public void RemovePlayer() => idCount--;
        public int  GetIDCount() => idCount;

        private void PlayerOneStatus(bool oldValue, bool newValue)
        {
            OneReady.Invoke(newValue);
            CheckReady();
        }

        private void PlayerTwoStatus(bool oldValue, bool newValue)
        {
            TwoReady.Invoke(newValue);
            CheckReady();
        }

        void CheckReady() => BothReady?.Invoke(playerOneReady && playerTwoReady);

    }
}
