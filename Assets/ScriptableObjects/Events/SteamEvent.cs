using UnityEngine;
using Illu.Steam;

[CreateAssetMenu(fileName = "Steam Event", menuName = "Event/Steam")]
public class SteamEvent : Event
{
    public SteamUserRecord user;

    public void Trigger(SteamUserRecord user)
    {
        Debug.Log($"{this.name} has been triggered!");
        for(int i = listeners.Count -1; i >= 0; i--)
        {
            listeners[i].OnEventTriggered();
        }
    }
}
