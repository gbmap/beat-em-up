using System;
using System.Collections.Generic;

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
            t.Vigor += this[v].Attributes.Vigor;
            t.Strength += this[v].Attributes.Strength;
            t.Dexterity += this[v].Attributes.Dexterity;
            t.Magic += this[v].Attributes.Magic;
        }

        return t;
    }

    public TCharAttributes<float> GetTotalDamageScaling()
    {
        TCharAttributes<float> t = new TCharAttributes<float>();

        var values = (EInventorySlot[])Enum.GetValues(typeof(EInventorySlot));
        foreach (var v in values)
        {
            t.Vigor += this[v].DamageScaling.Vigor;
            t.Strength += this[v].DamageScaling.Strength;
            t.Dexterity += this[v].DamageScaling.Dexterity;
            t.Magic += this[v].DamageScaling.Magic;
        }

        return t;
    }
}

public class Skill
{
    // PLACEHOLDERRRRRR
}

public class Item
{
    public int Id;
    public TCharAttributes<int> Attributes;
    public TCharAttributes<float> DamageScaling;
    public Skill Skill;
}

public class CharacterStats
{
    public int Health;
    public int Mana;
    public TCharAttributes<int> Attributes;
    public Inventory Inventory;
}

