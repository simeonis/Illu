using UnityEngine.Audio;
using UnityEngine;
using System;

public enum SoundType
{
    Music,
    SoundEffects,
    Menu
}

[Serializable]
public class Sound
{
    public AudioClip clip;

    public string name;

    public SoundType tag;

    [Range(0f, 1f)]
    public float volume;

    [Range(-1.0f, 1.0f)]
    public float pitch = 1.0f;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}