using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUtils
{
    public static float GetCritFactor(CharacterStats c)
    {
        return 2f;
    }

    public static float GetCritChance(CharacterStats c)
    {
        var attr = c.Inventory.GetTotalAttributes();
        float d = c.Attributes.Dexterity + attr.Dexterity;

        // esse 256 é arbitrário e significa o valor de dextreza que equivale a 50% de chance de crit.
        return 1f - (1f / (Mathf.Pow(d/256, 2f)+1f));
        //return Mathf.Pow(d/256, 1f/1.75f);
    }

    public static int GetMaxHealth(CharacterStats c)
    {
        return (c.Attributes.Vigor+c.Inventory.GetTotalAttributes().Vigor) * 19;
    }

    public static int GetMaxMana(CharacterStats c)
    {
        return (c.Attributes.Magic+c.Inventory.GetTotalAttributes().Magic) * 19;
    }

    public static int GetDamage(CharacterStats attacker, CharacterStats defender)
    {
        var dmgScaling = attacker.Inventory.GetTotalDamageScaling();

        float str = attacker.Attributes.Strength * dmgScaling.Strength;
        float dex = attacker.Attributes.Dexterity * dmgScaling.Dexterity;
        float mag = attacker.Attributes.Magic * dmgScaling.Magic;
        float crit = Random.value < GetCritChance(attacker) ? GetCritFactor(attacker) : 1f;
        return Mathf.FloorToInt((str + dex + mag) * crit);
    }
}