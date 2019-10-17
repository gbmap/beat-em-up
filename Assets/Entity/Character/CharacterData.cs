using System;
using UnityEngine;


[Serializable]
public class CharacterData : ConfigurableObject<CharacterStats, ECharacterType>
{
    void Awake()
    {
        if (InitData)
        {
            Stats = CharacterManager.RegisterCharacter(gameObject.GetInstanceID(), DataInit);
        }
        else
        {
            Stats = CharacterManager.RegisterCharacter(gameObject.GetInstanceID());
        }
    }

    private void Start()
    {
        if (TypeId != ECharacterType.None)
        {
            StartCoroutine(CharacterManager.Instance.SetupCharacter(gameObject, TypeId));
        }
    }

    private void OnDestroy()
    {
        CharacterManager.UnregisterCharacter(gameObject.GetInstanceID());
    }
}