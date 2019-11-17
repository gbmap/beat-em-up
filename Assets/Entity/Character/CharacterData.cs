using System;
using System.Collections.Generic;
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