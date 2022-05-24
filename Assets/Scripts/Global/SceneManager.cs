using UnityEngine;

namespace Illu.Scene
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public void ChangeScene(string scene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }
    }
}
