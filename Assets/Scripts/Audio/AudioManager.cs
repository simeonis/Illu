using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{

    public Dictionary<string, Sound> sounds;

    public List<Sound> soundList;

    public static AudioManager instance;

    void Awake()
    {
        sounds = new Dictionary<string, Sound>();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        foreach (Sound sound in soundList)
        {
            sounds.Add(sound.name, sound);

            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;

            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }
    }

    public void Play(string name)
    {
        Sound sound;
        if (sounds.TryGetValue(name, out sound))
        {
            Debug.Log("Playing " + sound.name);

            sound.source.Play();
        }
        else
        {
            Debug.Log("Not a Sound in the List");
        }
    }
}