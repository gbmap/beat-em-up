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
    public List<ItemData> ItemsInRange { get { return itemsInRange; } }

    ECharacterType lastCharacterType;

    public GameObject CharacterModelOverride;

    public ItemConfig[] StartingItems;

    void Awake()
    {
        BrainType = GetComponent<CharacterPlayerInput>() != null ? ECharacterBrainType.Input : ECharacterBrainType.AI;
        Stats = CharacterManager.RegisterCharacter(gameObject.GetInstanceID(), DataInit);

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
        else
        {
            StartCoroutine(CharacterManager.Instance.SetupCharacter(gameObject, ECharacterType.HeroKnightFemale));
        }

        lastCharacterType = TypeId;
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
        while (item == null)
        { 
            itemsInRange.RemoveAt(0);

            if (itemsInRange.Count == 0)
            {
                return false;
            }

            item = itemsInRange[0];
        }

        bool r = Equip(item);

        if (r)
        {
            OnItemOutOfRange(item);
            Destroy(item.gameObject);
        }
        return r;
    }

    public bool Equip(ItemData item)
    {
        if (Stats.Inventory.HasEquip(item.Stats.Slot))
        {
            UnEquip(item.Stats.Slot);
        }

        if (!CharacterManager.Instance.Interact(this, item.Stats)) return false;
        if (item.Stats.ItemType == EItemType.Equip)
        {
            var animator = GetComponent<CharacterAnimator>();
            if (animator)
            {
                animator.Equip(item);
            }
        }
        return true;
    }

    public bool Equip(ItemConfig itemConfig)
    {
        if (Stats.Inventory.HasEquip(itemConfig.Stats.Slot))
        {
            UnEquip(itemConfig.Stats.Slot);
        }

        if (!CharacterManager.Instance.Interact(this, itemConfig.Stats)) return false;
        if (itemConfig.Stats.ItemType == EItemType.Equip)
        {
            var animator = GetComponent<CharacterAnimator>();
            if (animator)
            {
                animator.Equip(itemConfig);
            }
        }
        return true;
    }

    public void UnEquip(EInventorySlot slot)
    {
        UnEquip(slot, Vector3.zero);
    }

    public void UnEquip(EInventorySlot slot, Vector3 dropDir)
    {
        if (slot != EInventorySlot.Weapon)
        {
            return;
        }

        var animator = GetComponent<CharacterAnimator>();
        if (animator)
        {
            animator.UnEquip(slot);
        }

        ItemStats item = Stats.Inventory[EInventorySlot.Weapon];
        if (item != null)
        {
            GameObject itemObj = ItemManager.Instance.SpawnItem(transform.position, item.Id);
            itemObj.GetComponent<ItemData>().Push(dropDir);
        }

        CharacterManager.Instance.UnEquip(this, slot);
    }
}