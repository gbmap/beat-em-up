using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

/********
*  Character component for instance-based data.
*  Holds stats, effects, inventory, etc.
*********/

public enum ECharacterBrainType
{
    AI,
    Input
}


public enum ECharacterType
{
    None,
    FantasyRivalsBegin,

    BarbarianGiant,
    Dwarf,
    AncientWarrior,
    AncientQueen,
    DarkElf,
    EvilGod,
    ForestGuardian,
    ForestWitch,
    Medusa,
    Mystic,
    SpiritDemon,
    BigOrk,
    ElementalGolem,
    FortGolem,
    MechanicalGolem,
    MutantGuy,
    PigButcher,
    RedDemon,
    Slayer,
    Troll,


    FantasyRivalsEnd,
    // ==============================

    KnightsBegin,

    Knight1,
    Knight2,
    Knight3,
    Soldier1,
    Soldier2,

    KnightsEnd,

    // ==============================

    AdventurePackBegin,

    Knight4,
    Peasant,
    Shopkeeper,
    Viking,
    Warrior,

    AdventurePackEnd,

    // ==============================

    DungeonPackBegin,

    Ghost1,
    Ghost2,
    GoblinFemale,
    GoblinMale,
    GoblinShaman,
    GoblinWarChief,
    GoblinWarriorFemale,
    GoblinWarriorMale,
    HeroKnightFemale,
    HeroKnightMale,
    RockGolem,
    SkeletonKnight,
    SkeletonSlave,
    SkeletonSoldier1,
    SkeletonSoldier2,
    TormentedSoul,


    DungeonPackEnd,

    // ==============================


    ModularCharacter
}

[Serializable]
public class CharacterData : ConfigurableObject<CharacterStats, ECharacterType>
{
    [Space]
    [Header("Configuration")]
    public Catacumba.Data.CharacterConfiguration CharacterCfg;

    public bool CanBeKnockedOut = true;

    [Space]
    [Header("Model")]
    public GameObject[] CharacterModelOverride;

    [Space]
    [Header("Inventory")]
    public ItemConfig[] StartingItems;
    public ECharacterBrainType BrainType { get; private set; }

    [Space]
    [Header("Skills")]
    public SkillData[] CharacterSkills;

    public List<ItemData> ItemsInRange { get { return itemsInRange; } }
    private List<ItemData> itemsInRange = new List<ItemData>();

    ECharacterType lastCharacterType;

    public System.Action<GameObject> OnCharacterModelUpdated;

    public bool IsInitialized { get; set; }

    void Awake()
    {
        BrainType = GetComponent<CharacterPlayerInput>() != null ? ECharacterBrainType.Input : ECharacterBrainType.AI;

        // Load basic cfg if no configuration is set.
        if (CharacterCfg == null) 
            CharacterCfg = Catacumba.Data.CharacterConfiguration.Default;

        // setup attribs
        Stats = new CharacterStats(CharacterCfg.Stats);
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        GameObject prefab = CharacterCfg.View.GetRandomModel();
        //StartCoroutine(CharacterManager.Instance.SetupCharacter(gameObject, prefab));
        /*
        if (CharacterModelOverride != null)
        {
            if (CharacterModelOverride.Length > 0)
            {
                var ch = CharacterModelOverride[UnityEngine.Random.Range(0, CharacterModelOverride.Length)];
                StartCoroutine(CharacterManager.Instance.SetupCharacter(gameObject, ch));
            }
            else if (TypeId != ECharacterType.None)
                StartCoroutine(CharacterManager.Instance.SetupCharacter(gameObject, TypeId));
            else
                StartCoroutine(CharacterManager.Instance.SetupInventory(gameObject, StartingItems));
        }
        */
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
        switch (item.Stats.ItemType)
        {
            case EItemType.Key:
                Stats.Inventory.GrabKey(item.itemConfig);
                return true;
            case EItemType.Equip:
                if (Stats.Inventory.HasEquip(item.Stats.Slot))
                {
                    UnEquip(item.Stats.Slot);
                }

                if (!CharacterManager.Instance.Interact(this, item.Stats)) return false;

                var animator = GetComponent<CharacterAnimator>();
                if (animator)
                {
                    animator.Equip(item);
                }
                return true;
        }

        return false;
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