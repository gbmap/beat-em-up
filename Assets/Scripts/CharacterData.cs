using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData : MonoBehaviour
{
    public CharacterStats Stats { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        Stats = CharacterManager.RegisterCharacter(gameObject.GetInstanceID());
    }

    private void OnDestroy()
    {
        CharacterManager.UnregisterCharacter(gameObject.GetInstanceID());
    }
}
