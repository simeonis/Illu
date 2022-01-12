using UnityEngine;

[CreateAssetMenu(menuName = "Audio Events/ImpactAudioEvent")]
public class ImpactAudioEvent : ScriptableObject
{
    public AudioClip[] clips_low;
    public AudioClip[] clips_med;
    public AudioClip[] clips_high;

    public RangedFloat volume_1;
    public RangedFloat volume_2;
    public RangedFloat volume_3;

    [MinMaxRange(0, 2)]
    public RangedFloat pitch;

    public void Play(AudioSource source, float velocity)
    {
        if (clips_low.Length == 0 || clips_med.Length == 0 || clips_high.Length == 0)
        {
            Debug.Log("Wrong Clip Length");
            return;
        }

        source.pitch = Random.Range(pitch.minValue, pitch.maxValue);

        if (velocity < 4)
        {
            source.clip = clips_low[Random.Range(0, clips_low.Length)];
            source.volume = Random.Range(volume_1.minValue, volume_1.maxValue);
            source.Play();

        }
        else if (velocity < 6)
        {
            source.clip = clips_med[Random.Range(0, clips_med.Length)];
            source.volume = Random.Range(volume_2.minValue, volume_2.maxValue);
            source.Play();

        }
        else
        {
            source.clip = clips_high[Random.Range(0, clips_high.Length)];
            source.volume = Random.Range(volume_3.minValue, volume_3.maxValue);
            source.Play();

        }
    }
}