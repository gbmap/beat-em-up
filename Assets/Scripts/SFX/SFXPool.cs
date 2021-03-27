using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Catacumba/SFX/SFX Pool", fileName="SFXPool")]
public class SFXPool : ScriptableObject
{
    public AudioClip[] Clips;

    public AudioClip GetRandomClip()
    {
        return Clips[Random.Range(0, Clips.Length)];
    }
}
