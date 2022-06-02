using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace Illu.Networking
{
    public class ReadyUpInterface : NetworkBehaviour
    {
        ReadyUpSystem.ID myID;
        [SerializeField] Button readyUp;
        [SerializeField] Button startGame;

        void OnEnable()
        {
            NetworkManager.Instance.ReadyUpSystem.BothReady.AddListener(OnBothReady);

            //readyUp.onClick.AddListener(ReadyUP);
        }

        void Awake()
        {
            myID = NetworkManager.Instance.ReadyUpSystem.requestID();
        }

        public void ReadyUP()
        {
            Debug.Log("Ready up");
            NetworkManager.Instance.ReadyUpSystem.SetReadyStatus(myID, true);
        }
        public void CancelReadyUP()
        {
            NetworkManager.Instance.ReadyUpSystem.SetReadyStatus(myID, false);
        }

        void OnBothReady()
        {
            if (myID == ReadyUpSystem.ID.playerOne)
            {
                readyUp.enabled = false;
                startGame.enabled = true;
            }
        }
    }
}