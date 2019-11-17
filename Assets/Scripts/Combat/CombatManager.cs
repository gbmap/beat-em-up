using UnityEngine;

public enum EAttackType
{
    Weak,
    Strong
}

public struct CharacterAttackData
{
    public float Time;
    public EAttackType Type;
    public GameObject Attacker;
    public CharacterStats AttackerStats;
    public GameObject Defender;
    public CharacterStats DefenderStats;
    public int Damage;
    public int HitNumber;

    public bool Poised;
    public bool Knockdown;
}

public class CombatManager : ConfigurableSingleton<CombatManager, CombatManagerConfig>
{
    protected override string Path => "Data/CombatManagerConfig";

    public static float GetCritFactor(CharacterStats c)
    {
        return 2f;
    }

    public static float GetCritChance(CharacterStats c)
    {
        var attr = c.Inventory.GetTotalAttributes();
        float d = c.Attributes.Dexterity + attr.Dexterity;

        // esse 256 é arbitrário e significa o valor de destreza que equivale a 50% de chance de crit.
        return 1f - (1f / (Mathf.Pow(d/256, 2f)+1f));
        //return Mathf.Pow(d/256, 1f/1.75f);
    }

    public static float GetPoiseChance(CharacterStats c)
    {
        var attr = c.Inventory.GetTotalAttributes();
        float d = c.Attributes.Dexterity + attr.Dexterity;
        return 1f - (1f / (Mathf.Pow(d / 256, 2f) + 1f));
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
        var inventoryAttributes = attacker.Inventory.GetTotalAttributes();

        float str = (attacker.Attributes.Strength + inventoryAttributes.Strength) * (1f + dmgScaling.Strength);
        float dex = (attacker.Attributes.Dexterity + inventoryAttributes.Dexterity) * dmgScaling.Dexterity;
        float mag = (attacker.Attributes.Magic + inventoryAttributes.Magic) * dmgScaling.Magic;
        float crit = Random.value < GetCritChance(attacker) ? GetCritFactor(attacker) : 1f;
        return Mathf.FloorToInt((str + dex + mag) * crit);
    }

    public static void Attack(CharacterStats attacker, CharacterStats defender, ref CharacterAttackData attackData)
    {
        attackData.AttackerStats = attacker;
        attackData.DefenderStats = defender;

        // calcula dano cru
        int damage = GetDamage(attacker, defender);

        // rola o dado pra poise
        attackData.Poised = Random.value < defender.PoiseChance;
        if (attackData.Poised)
        {
            damage = (int)(damage * 0.9f);
        }

        defender.PoiseBar -= ((float)damage) / defender.Poise;

        // vê se derrubou o BONECO
        attackData.Knockdown = Mathf.Approximately(defender.PoiseBar, 0);

        defender.Health -= damage;

        attackData.Damage = damage;
    }

    public static void Attack(GameObject attacker, GameObject defender, ref CharacterAttackData attackData)
    {
        Attack(CharacterManager.GetCharacterStats(attacker.GetInstanceID()),
               CharacterManager.GetCharacterStats(defender.GetInstanceID()),
               ref attackData);
    }
}