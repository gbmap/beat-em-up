using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum ECharacterBrainType
{
    AI,
    Input
}

[Serializable]
public class CharacterData : ConfigurableObject<CharacterStats, ECharacterType>
{
    public ECharacterBrainType BrainType;
    private List<ItemData> itemsInRange = new List<ItemData>();

    ECharacterType lastCharacterType;

    public GameObject CharacterModelOverride;

    void Awake()
    {
        BrainType = GetComponent<CharacterPlayerInput>() != null ? ECharacterBrainType.Input : ECharacterBrainType.AI;
        if (InitData)
        {
            Stats = CharacterManager.RegisterCharacter(gameObject.GetInstanceID(), DataInit);
        }
        else
        {
            Stats = CharacterManager.RegisterCharacter(gameObject.GetInstanceID());
        }

        Stats.Health = Stats.MaxHealth;
        Stats.Mana = Stats.MaxMana;

        lastCharacterType = TypeId;
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        if (CharacterModelOverride != null)
        {
            StartCoroutine(CharacterManager.Instance.SetupCharacter(CharacterModelOverride, TypeId));
        }
        else if (TypeId != ECharacterType.None)
        {
            StartCoroutine(CharacterManager.Instance.SetupCharacter(gameObject, TypeId));
        }
    }

    private void Update()
    {
        if (TypeId != lastCharacterType && TypeId != ECharacterType.None)
        {
            StartCoroutine(CharacterManager.Instance.SetupCharacter(gameObject, TypeId));
        }

        lastCharacterType = TypeId;
    }

    public System.Collections.IEnumerator Test_AllCharacters()
    {
        var values = Enum.GetValues(typeof(ECharacterType));
        foreach (ECharacterType type in values)
        {
            ECharacterType[] skip = {
                ECharacterType.AdventurePackBegin,
                ECharacterType.AdventurePackEnd,
                ECharacterType.None,
                ECharacterType.DungeonPackBegin,
                ECharacterType.DungeonPackEnd,
                ECharacterType.FantasyRivalsBegin,
                ECharacterType.FantasyRivalsEnd,
                ECharacterType.KnightsBegin,
                ECharacterType.KnightsEnd,
                ECharacterType.ModularCharacter
            };

            if (skip.Contains(type))
            {
                continue;
            }

            yield return StartCoroutine(CharacterManager.Instance.SetupCharacter(gameObject, type));
            yield return new WaitForSeconds(2f);
        }
    }

    private void OnDestroy()
    {
        CharacterManager.UnregisterCharacter(gameObject.GetInstanceID());
    }

    private bool ValidItem(ItemData item, bool enterExit)
    {
        return itemsInRange.Contains(item) ^ enterExit;
    }
    
    public void OnItemInRange(ItemData item)
    {
        if (ValidItem(item, true))
        {
            itemsInRange.Add(item);
        }
    }

    public void OnItemOutOfRange(ItemData item)
    {
        if (ValidItem(item, false))
        {
            itemsInRange.Remove(item);
        }
    }
    
    public bool Interact()
    {
        if (itemsInRange.Count == 0) return false;
        var item = itemsInRange[0];

        if (!CharacterManager.Instance.Interact(this, item)) return false;

        if (item.Stats.ItemType == EItemType.Equip)
        {
            var animator = GetComponent<CharacterAnimator>();
            if (animator)
            {
                animator.Equip(item);
            }
        }

        OnItemOutOfRange(item);
        Destroy(item.gameObject);
        return true;
    }
}