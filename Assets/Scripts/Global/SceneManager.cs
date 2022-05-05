using UnityEngine;

namespace Illu.Scene
{
    public class SceneManager : MonoBehaviour
    {
        public void ChangeScene(string scene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }
    }
}
