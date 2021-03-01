using System;
using UnityEngine;

[CreateAssetMenu(menuName="SoundManager", fileName="SoundManager")]
public class SoundManager : ScriptableObject
{
    public AudioClip[] Wooshes;
    public AudioClip[] Hits;

    public AudioClip[] Roll;
    public AudioClip[] Walk;

    private AudioClip GetRandom(AudioClip[] clips)
    {
        if (clips.Length == 0) return null;
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }

    public void PlayWoosh(Vector3 position)
    {
        PlayRandomAudio(position, GetRandom(Wooshes));
    }

    public void PlayHit(Vector3 position)
    {
        PlayRandomAudio(position, GetRandom(Hits));
    }

    public void PlayRoll(Vector3 position)
    {
        PlayRandomAudio(position, GetRandom(Roll));
    }

    public void PlayStep(Vector3 position)
    {
        PlayRandomAudio(position, GetRandom(Walk));
    }

    private void PlayRandomAudio(Vector3 position, AudioClip clip)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position);
    }
}
