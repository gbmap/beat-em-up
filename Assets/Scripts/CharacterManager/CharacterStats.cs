using System;
using System.Collections.Generic;

/**
  *
  * */
#region TCharAttributes

public class TCharAttributes<T>
{
    public T Vigor;
    public T Strength;
    public T Dexterity;
    public T Magic;

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
    public TCharAttributes<int> GetTotalAttributes()
    {
        TCharAttributes<int> t = new TCharAttributes<int>();

        var values = (EInventorySlot[])Enum.GetValues(typeof(EInventorySlot));
        foreach (var v in values)
        {
            if (!this.ContainsKey(v)) continue;
            t.Add(this[v].Attributes);
        }

        return t;
    } 

    public TCharAttributes<float> GetTotalDamageScaling()
    {
        TCharAttributes<float> t = new TCharAttributes<float>();

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
    public TCharAttributes<int> Attributes;
    public TCharAttributes<float> DamageScaling;
    public Skill Skill;
}

#endregion

public class Skill
{
    // PLACEHOLDERRRRRR
}

public class CharacterStats
{
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

    public TCharAttributes<int> Attributes;
    public Inventory Inventory;

    public CharacterStats()
    {
        Level = 1;
        Attributes = new TCharAttributes<int>()
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

