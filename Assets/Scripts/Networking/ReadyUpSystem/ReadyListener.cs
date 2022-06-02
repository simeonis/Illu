using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace Illu.Networking
{
    public class ReadyListener : MonoBehaviour
    {
        [SerializeField] Button ready;
        [SerializeField] Button start;

        void OnEnable()
        {
            NetworkManager.Instance.ReadyUpSystem.BothReady.AddListener(OnBothReady);

        }

        void OnBothReady()
        {
            start.enabled = false;
            ready.enabled = true;
        }

    }

}