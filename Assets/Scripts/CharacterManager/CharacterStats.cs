﻿using System;
using System.Collections.Generic;

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

public class Inventory : Dictionary<EInventorySlot, Item>
{
    public CharAttributesI GetTotalAttributes()
    {
        CharAttributesI t = new CharAttributesI();

        var values = (EInventorySlot[])Enum.GetValues(typeof(EInventorySlot));
        foreach (var v in values)
        {
            if (!this.ContainsKey(v)) continue;
            t.Add(this[v].Attributes);
        }

        return t;
    } 

    public CharAttributesF GetTotalDamageScaling()
    {
        CharAttributesF t = new CharAttributesF();

        var values = (EInventorySlot[])Enum.GetValues(typeof(EInventorySlot));
        foreach (var v in values)
        {
            if (!this.ContainsKey(v)) continue;
            t.Add(this[v].DamageScaling);
        }

        return t;
    }
}

public class Item
{
    public int Id;
    public CharAttributesI Attributes;
    public CharAttributesF DamageScaling;
    public Skill Skill;
}

public enum EWeaponType
{
    Fists,
    Sword,
    Dagger,
    Scepter
}

public class Weapon : Item
{
    public EWeaponType Type;
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

public class Skill
{
    // PLACEHOLDERRRRRR
}

public class CharacterStats
{
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

    public int Health { get; set; }

    public int MaxMana
    {
        get
        {
            return CombatManager.GetMaxMana(this);
        }
    }

    public int Mana { get; set; }

    public CharAttributesI Attributes;
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
    }
}

