using System;
using UnityEngine;

public class CharacterData : MonoBehaviour
{
    public ECharacterType Type = ECharacterType.None;

    public CharacterStats Stats { get; private set; }

    [Header("Attribute Override")]
    public bool OverrideAttributes = false;
    public CharAttributesI AttributeOverride;

    // Start is called before the first frame update
    void Awake()
    {
        if (OverrideAttributes)
        {
            Stats = CharacterManager.RegisterCharacter(
                gameObject.GetInstanceID(), 
                new CharacterStats { Attributes = AttributeOverride }
            );
        }
        else
        {
            Stats = CharacterManager.RegisterCharacter(gameObject.GetInstanceID());
        }
    }

    private void Start()
    {
        if (Type != ECharacterType.None)
        {
            StartCoroutine(CharacterManager.Instance.SetupCharacter(gameObject, Type));
        }
    }

    private void OnDestroy()
    {
        CharacterManager.UnregisterCharacter(gameObject.GetInstanceID());
    }
}
