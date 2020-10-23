
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[Serializable]
public class CharacterPrefabConfig
{
    public ECharacterType type;
    public GameObject prefab;
}

public class CharacterManagerConfig : ScriptableObject
{
    /*[MenuItem("Assets/Create/CharacterManagerConfig")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<CharacterManagerConfig>();
    }*/

    public CharacterPrefabConfig[] CharacterPrefabs;

    public RuntimeAnimatorController CharacterAnimator;
    public AnimatorOverrideController DaggerOverrideController;
    public AnimatorOverrideController SwordOverrideController;
    public AnimatorOverrideController ScepterOverrideController;
    public AnimatorOverrideController TwoHandedSwordController;
    public AnimatorOverrideController BowController;

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
            case EWeaponType.TwoHandedSword: return TwoHandedSwordController;
            case EWeaponType.Bow: return BowController;
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

    public static IEnumerator SetupCharacter(GameObject instance, GameObject model)
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

        yield return null;

        // MOVE O PACK PRA BAIXO DO ANIMATOR
        var packInstance = Instantiate(packPrefab);

        Avatar prefabAvatar = packInstance.GetComponent<Animator>().avatar;
        Transform characterModel = null;

        string children = string.Empty;
        // MOVE A INSTÂNCIA (primeira criança ativa) PRA BAIXO DO ANIMATOR

        var root = packInstance.transform.Find("Root");
        root.parent = instance.transform;
        root.localPosition = Vector3.zero;
        root.gameObject.SetActive(true);

        Transform characterTransform = null;

        for (int i = 0; i < packInstance.transform.childCount; i++)
        {
            characterModel = packInstance.transform.GetChild(i);
            children += characterModel.name + "\n";

            if (characterModel.gameObject.activeSelf)
            {
                Vector3 localRot = characterModel.localRotation.eulerAngles;
                Vector3 localPos = characterModel.localPosition;

                characterModel.parent = instance.transform;

                if (characterModel.name.Contains("Character") || characterModel.name.Contains("Root"))
                {
                    if (characterModel.name.Contains("Character")) characterTransform = characterModel;
                    characterModel.localPosition = Vector3.zero;
                }
                else
                {
                    characterModel.localRotation = Quaternion.Euler(localRot);
                    characterModel.localPosition = localPos;
                }
            }
        }

        Debug.Log(children);

        Destroy(packInstance);

        yield return null;

        // ATUALIZA O AVATAR DO ANIMATOR
        var animator = instance.GetComponent<CharacterAnimator>();
        animator.RefreshAnimator(prefabAvatar, animator.animator.runtimeAnimatorController, animator.animator.applyRootMotion);

        yield return null;

        CharacterModelInfo characterModelInfo = instance.GetComponent<CharacterModelInfo>();
        if (characterModelInfo == null)
        {
            characterModelInfo = instance.AddComponent<CharacterModelInfo>();
        }

        yield return null;

        Transform lhb = characterModelInfo.LeftHandBone.Bone;
        Transform rhb = characterModelInfo.RightHandBone.Bone;

        GameObject weaponHolder = new GameObject("WeaponHolder");
        weaponHolder.transform.SetParent(lhb);
        weaponHolder.transform.localPosition = Vector3.zero;
        weaponHolder.transform.localRotation = Quaternion.identity;
        weaponHolder.transform.localScale = Vector3.one;

        weaponHolder = new GameObject("WeaponHolder");
        weaponHolder.transform.SetParent(rhb);
        weaponHolder.transform.localPosition = Vector3.zero;
        weaponHolder.transform.localRotation = Quaternion.identity;
        weaponHolder.transform.localScale = Vector3.one;

        yield return null;

        if (characterTransform)
        {
            var cd = instance.GetComponent<CharacterData>();
            cd.OnCharacterModelUpdated?.Invoke(characterTransform.gameObject);

            // HACK: Atualizar Skill Datas que podem vir dentro do modelo novo.
            SkillData[] sd = characterTransform.GetComponentsInChildren<SkillData>();
            foreach (SkillData skill in sd)
            {
                skill.Caster = cd;
            }
        }

        yield return EquipInventory(instance, instance.GetComponent<CharacterData>().StartingItems);

        instance.GetComponent<CharacterData>().IsInitialized = true;
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

    public static IEnumerator EquipInventory(GameObject instance, ItemConfig[] items)
    {
        CharacterData d = instance.GetComponent<CharacterData>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null) continue;
            d.Equip(items[i]);
        }
        yield return null;
    }
}