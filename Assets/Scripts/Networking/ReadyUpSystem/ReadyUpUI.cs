using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace Illu.Networking
{
    public class ReadyUpUI : NetworkBehaviour
    {
        [SerializeField] ReadyUpSystem readyUpSystem;
        [SerializeField] RectTransform readyUp;
        [SerializeField] RectTransform startGame;
        [SerializeField] Button startBTN;

        void OnEnable()
        {
            readyUpSystem.BothReady.AddListener(OnBothReady);
            startBTN.onClick.AddListener(handleOnClick);
        }
        void OnBothReady()
        {
            if(isServer)
            {
                readyUp.gameObject.SetActive(false);
                startBTN.gameObject.SetActive(true);
            }
        }

        void handleOnClick()
        {
            NetworkManager.singleton.ServerChangeScene("LevelOne");
        }
    }
}