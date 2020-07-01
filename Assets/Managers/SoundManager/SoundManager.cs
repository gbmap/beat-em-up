using System;
using UnityEngine;

public class SoundManager : SimpleSingleton<SoundManager>
{
    public AudioClip[] Wooshes;
    public AudioClip[] Hits;

    private AudioClip GetRandomClip(AudioClip[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[UnityEngine.Random.Range(0, arr.Length)];
    }

    public void PlayWoosh(Vector3 position)
    {
        PlayAudio(position, GetRandomClip(Wooshes));
    }

    public void PlayHit(Vector3 position)
    {
        PlayAudio(position, GetRandomClip(Hits));
    }

    private void PlayAudio(Vector3 position, AudioClip clip)
    {
        if (!clip) return;
        AudioSource.PlayClipAtPoint(clip, position);
    }
}
