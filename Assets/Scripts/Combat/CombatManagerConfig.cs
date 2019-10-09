using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CombatManagerConfig : ScriptableObject
{
    [MenuItem("Assets/Create/CombatManagerConfig")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<CombatManagerConfig>();
    }

    [Header("Weapon Animator Overrides")]
    public RuntimeAnimatorController DefaultController;
    public AnimatorOverrideController SwordController;
    public AnimatorOverrideController DaggerController;
    public AnimatorOverrideController ScepterController;

    public RuntimeAnimatorController WeaponTypeToController(EWeaponType type)
    {
        switch (type)
        {
            case EWeaponType.Dagger: return DaggerController;
            case EWeaponType.Scepter: return ScepterController;
            case EWeaponType.Sword: return SwordController;
            default: return DefaultController;
        }
    }
}
