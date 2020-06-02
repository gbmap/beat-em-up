using System;
using System.Collections.Generic;
using UnityEngine;

/**
  *
  * */
#region TCharAttributes

public enum EAttribute
{
    Vigor,
    Strength,
    Dexterity,
    Magic
}

public class TCharAttributes<T>
{
    public T Vigor;
    public T Strength;
    public T Dexterity;
    public T Magic;

    public T GetAttr(EAttribute attr)
    {
        switch (attr)
        {
            case EAttribute.Vigor: return Vigor;
            case EAttribute.Strength: return Strength;
            case EAttribute.Magic: return Magic;
            case EAttribute.Dexterity: return Dexterity;
            default: throw new NotImplementedException(string.Format("Couldn't find attribute: {0}", attr.ToString()));
        }
    }

    public void SetAttr(EAttribute attr, T v)
    {
        switch (attr)
        {
            case EAttribute.Vigor: Vigor = v; break;
            case EAttribute.Strength: Strength = v; break;
            case EAttribute.Magic: Magic = v; break;
            case EAttribute.Dexterity: Dexterity = v; break;
            default: throw new NotImplementedException(string.Format("Couldn't find attribute: {0}", attr.ToString()));
        }
    }

    public static TCharAttributes<T> Empty {
        get
        {
            return new TCharAttributes<T>
            {
                Vigor = default,
                Strength = default,
                Dexterity = default,
                Magic = default
            };
        }
    }
}

[Serializable]
public class CharAttributesI : TCharAttributes<int>
{

}

[Serializable]
public class CharAttributesF : TCharAttributes<float>
{
    
}

public static class TCharAttributeExtension
{
    public static void Add(this TCharAttributes<float> a, TCharAttributes<float> b)
    {
        a.Vigor += b.Vigor;
        a.Strength += b.Strength;
        a.Dexterity += b.Dexterity;
        a.Magic += b.Magic;
    }

    public static void Add(this TCharAttributes<int> a, TCharAttributes<int> b)
    {
        a.Vigor += b.Vigor;
        a.Strength += b.Strength;
        a.Dexterity += b.Dexterity;
        a.Magic += b.Magic;
    }
}

#endregion

/**
 * 
 * */
#region Inventory

public enum EInventorySlot
{
    Helm,
    Armor,
    Boot,
    Ring1,
    Ring2,
    Weapon
}

[Serializable]
public class Inventory
{
    [SerializeField]
    private int[] inventory;

    [SerializeField]
    private List<ItemConfig> keyItems;

    public Inventory()
    {
        inventory = new int[((EInventorySlot[])Enum.GetValues(typeof(EInventorySlot))).Length];
        keyItems = new List<ItemConfig>();
    }

    public ItemStats this[EInventorySlot slot]
    {
        get => ItemManager.Instance.GetItem( inventory[(int)slot] );
        set => inventory[(int)slot] = value.Id;
    }

    public CharAttributesI GetTotalAttributes()
    {
        CharAttributesI t = new CharAttributesI();
       
        foreach (var v in inventory)
        {
            if (v == 0) continue;
            t.Add(ItemManager.Instance.GetItem(v).Attributes);
        }

        return t;
    }

    public CharAttributesF GetTotalDamageScaling()
    {
        CharAttributesF t = new CharAttributesF();

        foreach (var v in inventory)
        {
            if (v == 0) continue;
            t.Add(ItemManager.Instance.GetItem(v).DamageScaling);
        }

        return t;
    }

    public void UnEquip(EInventorySlot slot)
    {
        inventory[(int)slot] = 0;
    }

    public bool HasEquip(EInventorySlot slot)
    {
        return inventory[(int)slot] != 0;
    }

    public bool HasKey(ItemConfig itemConfig)
    {
        return keyItems.Contains(itemConfig);
    }

    public void GrabKey(ItemConfig itemCfg)
    {
        if (HasKey(itemCfg)) return;
        keyItems.Add(itemCfg);
    }
}

public enum EItemType
{
    Equip,
    Consumable,
    Key
}

public enum EItemRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

[Serializable]
public class ItemStats
{
    public int Id;
    public EItemType ItemType;
    public EItemRarity Rarity;
    public EInventorySlot Slot;
    public EWeaponType WeaponType;
    public CharAttributesI Attributes;
    public CharAttributesF DamageScaling;
    public SkillData[] Skills;

    public bool IsRanged { get { return WeaponType == EWeaponType.Bow || WeaponType == EWeaponType.Scepter; } }
}

public enum EWeaponType
{
    Fists,
    Sword,
    Dagger,
    Scepter,
    TwoHandedSword,
    Bow
}

#endregion

/**
 * 
 * */
#region CharacterSkillTree

public class CharacterSkillTree
{
    public const int MaxLevel = 10;

    CharAttributesI AttributeLevels;

    public CharacterSkillTree()
    {
        AttributeLevels = new CharAttributesI
        {
            Vigor = 0,
            Strength = 0,
            Dexterity = 0,
            Magic = 0
        };
    }

    public void UpgradeAttribute(EAttribute attribute)
    {
        int attr = AttributeLevels.GetAttr(attribute);
        if (attr < MaxLevel)
        {
            AttributeLevels.SetAttr(attribute, attr+1);
        }
    }
    
}

#endregion

[System.Serializable]
public class BaseSkill
{
    public GameObject Prefab;
    public float forwardOffset = 1.5f;
}

[Serializable]
public class CharacterStats
{
    public const int MaxAttributeLevel = 256;

    public System.Action<CharacterStats> OnStatsChanged = delegate { };

    public int Level { get; set; }

    // provável bottleneck aqui, health provavelmente vai ser acessado constantemente
    public int MaxHealth
    {
        get
        {
            return CombatManager.GetMaxHealth(this);
        }
    }

    private int health;
    public int Health
    {
        get
        {
            return health;
        }

        set
        {
            health = Mathf.Clamp(value, 0, MaxHealth);
            OnStatsChanged?.Invoke(this);
        }
    }

    public float HealthNormalized
    {
        get { return ((float)Health / MaxHealth); }
    }

    public int MaxMana
    {
        get
        {
            return CombatManager.GetMaxMana(this);
        }
    }

    public int Mana { get; set; }

    public CharAttributesI Attributes;
    public int GetAttributeTotal(EAttribute attribute)
    {
        switch (attribute)
        {
            case EAttribute.Dexterity: return (Attributes.Dexterity + Inventory.GetTotalAttributes().Dexterity);
            case EAttribute.Magic: return (Attributes.Magic + Inventory.GetTotalAttributes().Magic);
            case EAttribute.Strength: return (Attributes.Strength + Inventory.GetTotalAttributes().Strength);
            case EAttribute.Vigor: return (Attributes.Vigor + Inventory.GetTotalAttributes().Vigor);
            default: throw new NotImplementedException("Requested attributed not implemented!");
        }
    }

    public int Stagger
    {
        get { return Attributes.Vigor; }
    }

    public int Poise
    {
        get { return Attributes.Vigor; }
    }

    public float PoiseChance
    {
        get { return CombatManager.GetPoiseChance(this); }
    }

    public float PoiseBar
    {
        get
        {
            return ((float)CurrentPoise)/Poise;
        }
    }

    public int CurrentPoise
    {
        get; set;
    }

    public bool CanBeKnockedOut
    {
        get; set;
    }

    public Inventory Inventory;

    public CharacterStats()
    {
        Level = 1;
        Attributes = new CharAttributesI()
        {
            Strength = 10,
            Dexterity = 10,
            Vigor = 10,
            Magic = 10
        };
        Inventory = new Inventory();

        Health = MaxHealth;
        Mana = MaxMana;
        CanBeKnockedOut = true;
        CurrentPoise = Poise;
    }
}

