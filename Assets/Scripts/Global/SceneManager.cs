using UnityEngine;

namespace Illu.Scene
{
    public class SceneManager : MonoBehaviourSingleton<SceneManager>
    {

        public void ChangeScene(string scene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }
    }
}
