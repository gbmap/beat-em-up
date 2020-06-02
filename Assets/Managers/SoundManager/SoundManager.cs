using System;
using UnityEngine;

public class SoundManager : SimpleSingleton<SoundManager>
{
    public AudioClip[] Wooshes;
    public AudioClip[] Hits;

    private AudioClip GetRandomWoosh()
    {
        return Wooshes[UnityEngine.Random.Range(0, Wooshes.Length)];
    }

    private AudioClip GetRandomHit()
    {
        return Hits[UnityEngine.Random.Range(0, Hits.Length)];
    }

    public void PlayWoosh(Vector3 position)
    {
        PlayRandomAudio(position, GetRandomWoosh);
    }

    public void PlayHit(Vector3 position)
    {
        PlayRandomAudio(position, GetRandomHit);
    }

    private void PlayRandomAudio(Vector3 position, Func<AudioClip> selector)
    {
        AudioSource.PlayClipAtPoint(selector(), position);
    }
}
