
using UnityEngine;
using System.Collections.Generic;

namespace Illu.UI
{
    public class ScreenController : MonoBehaviourSingleton<ScreenController>
    {
        [SerializeField] GameObject Root;
        [SerializeField] GameObject GameMode;
        [SerializeField] GameObject HostOrJoin;
        [SerializeField] GameObject Host;
        [SerializeField] GameObject Join;
        [SerializeField] GameObject Settings;

        Dictionary<Screen, GameObject> screens = new Dictionary<Screen, GameObject>();

        override public void Awake()
        {
            base.Awake();
            screens[Screen.Root]       = Root;
            screens[Screen.GameMode]   = GameMode;
            screens[Screen.HostOrJoin] = HostOrJoin;
            screens[Screen.Host]       = Host;
            screens[Screen.Join]       = Join;
            screens[Screen.Settings]   = Settings;
        }

        public enum Screen
        {
            Root,
            GameMode,
            HostOrJoin,
            Host,
            Join,
            Settings
        }

        public void ChangeScreen(Screen name)
        {
            Debug.Log("Calling Change Screen " + name);
            foreach(GameObject screen in screens.Values)
                screen.SetActive(false);
            
            screens[name].SetActive(true);
        }

        public void ChangeScreen(string name)
        {
            Debug.Log("Calling Change Screen " + name);
            Screen.TryParse(name, out Screen requestedScreen);
            ChangeScreen(requestedScreen);
        }
    }
}