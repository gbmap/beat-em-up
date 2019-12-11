
using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
public class CharacterPrefabConfig
{
    public ECharacterType type;
    public GameObject prefab;
}

public class CharacterManagerConfig : ScriptableObject
{
    [MenuItem("Assets/Create/CharacterManagerConfig")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<CharacterManagerConfig>();
    }

    public CharacterPrefabConfig[] CharacterPrefabs;

    public RuntimeAnimatorController CharacterAnimator;
    public AnimatorOverrideController DaggerOverrideController;
    public AnimatorOverrideController SwordOverrideController;
    public AnimatorOverrideController ScepterOverrideController;

    public RuntimeAnimatorController GetRuntimeAnimatorController(ItemStats item)
    {
        ItemConfig cfg = ItemManager.Instance.GetItemConfig(item.Id);

        if (cfg.AnimationOverride != null)
        {
            return cfg.AnimationOverride;
        }
        return GetRuntimeAnimatorController(cfg.Stats.WeaponType);
    }

    public RuntimeAnimatorController GetRuntimeAnimatorController(EWeaponType type)
    {
        switch (type)
        {
            case EWeaponType.Dagger: return DaggerOverrideController;
            case EWeaponType.Sword: return SwordOverrideController;
            case EWeaponType.Scepter: return ScepterOverrideController;
            case EWeaponType.Fists: return CharacterAnimator;
            default: return CharacterAnimator;
        }
    }

    public GameObject AdventureCharacterPack;
    public GameObject DungeonCharacterPack;
    public GameObject FantasyRivalsCharacterPack;
    public GameObject KnightsCharacterPack;
    public GameObject ModularCharacterPack;

    public CharacterPrefabConfig GetPrefab(ECharacterType type)
    {
        return CharacterPrefabs.FirstOrDefault(x => x.type == type);
    }

    private bool InPack(ECharacterType type, ECharacterType packBegin, ECharacterType packEnd)
    {
        int index = (int)type;
        int beginIndex = (int)packBegin;
        int endIndex = (int)packEnd;
        return index > beginIndex && index < endIndex;
    }

    private ECharacterType GetPack(ECharacterType type)
    {
        if (InPack(type, ECharacterType.AdventurePackBegin, ECharacterType.AdventurePackEnd))
        {
            return ECharacterType.AdventurePackBegin;
        }
        else if (InPack(type, ECharacterType.DungeonPackBegin, ECharacterType.DungeonPackEnd))
        {
            return ECharacterType.DungeonPackBegin;
        }
        else if (InPack(type, ECharacterType.FantasyRivalsBegin, ECharacterType.FantasyRivalsEnd))
        {
            return ECharacterType.FantasyRivalsBegin;
        }
        else if (InPack(type, ECharacterType.KnightsBegin, ECharacterType.KnightsEnd))
        {
            return ECharacterType.KnightsBegin;
        }
        else
            return ECharacterType.ModularCharacter;
    }

    private GameObject TypeToPrefab(ECharacterType type)
    {
        int index = (int)type;
        switch (GetPack(type))
        {
            case ECharacterType.AdventurePackBegin: return AdventureCharacterPack;
            case ECharacterType.DungeonPackBegin: return DungeonCharacterPack;
            case ECharacterType.FantasyRivalsBegin: return FantasyRivalsCharacterPack;
            case ECharacterType.KnightsBegin: return KnightsCharacterPack;
            case ECharacterType.ModularCharacter: return ModularCharacterPack;
            default: return ModularCharacterPack;
        }
    }

    public IEnumerator SetupCharacter(GameObject instance, GameObject model)
    {
        GameObject packPrefab = model;

        // REMOVE MODELO ATUAL
        for (int i = 0; i < instance.transform.childCount; i++)
        {
            var child = instance.transform.GetChild(i);
            if (child.name.Contains("Character_") ||
                child.name.Equals("Root"))
            {
                Destroy(child.gameObject);
            }
        }

        // MOVE O PACK PRA BAIXO DO ANIMATOR
        var packInstance = Instantiate(packPrefab);
        packInstance.transform.parent = instance.transform;
        packInstance.transform.localPosition = Vector3.zero;

        Avatar prefabAvatar = packInstance.GetComponent<Animator>().avatar;

        yield return null;

        Transform characterModel = null;

        // MOVE A INSTÂNCIA (primeira criança ativa) PRA BAIXO DO ANIMATOR
        for (int i = 0; i < packInstance.transform.childCount; i++)
        {
            characterModel = packInstance.transform.GetChild(i);
            if (characterModel.gameObject.activeSelf)
            {
                characterModel.parent = instance.transform;
                characterModel.localPosition = Vector3.zero;
            }
        }

        yield return null;

        // ATUALIZA O AVATAR DO ANIMATOR
        var animator = instance.GetComponent<CharacterAnimator>();
        animator.RefreshAnimator(prefabAvatar, animator.animator.runtimeAnimatorController);

        yield return null;

        CharacterModelInfo characterModelInfo = packInstance.GetComponent<CharacterModelInfo>();
        if (characterModelInfo == null)
        {
            characterModelInfo = instance.AddComponent<CharacterModelInfo>();
        }

        yield return null;

        Destroy(packInstance.gameObject);
    }

    public IEnumerator SetupCharacter(GameObject instance, ECharacterType characterType)
    {
        var pack = GetPrefab(characterType);
        if (pack == null)
        {
            Debug.LogError("Couldn't find model for character type:" + characterType.ToString());
            pack = GetPrefab(ECharacterType.Dwarf);
        }
        yield return SetupCharacter(instance, pack.prefab);
    }
}