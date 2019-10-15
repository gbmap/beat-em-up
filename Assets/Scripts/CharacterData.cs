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
        if (Id != ECharacterType.None)
        {
            StartCoroutine(CharacterManager.Instance.SetupCharacter(gameObject, Id));
        }
    }

    private void OnDestroy()
    {
        CharacterManager.UnregisterCharacter(gameObject.GetInstanceID());
    }
}