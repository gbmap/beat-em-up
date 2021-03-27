using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Catacumba/SFX/SFX Bank", fileName="SFXBank")]
public class SFXBank : ScriptableObject
{
    public List<string> Names;
    public List<SFXPool> Sounds;

    public bool HasSound(string sound)
    {
        return Names.Contains(sound);
    }

    public SFXPool GetSound(string sound)
    {
        int index = Names.IndexOf(sound);
        if (index < 0 || index > Names.Count - 1) return null;
        return Sounds[index];
    }
}