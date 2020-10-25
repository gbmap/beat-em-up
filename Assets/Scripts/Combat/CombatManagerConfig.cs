using UnityEngine;
using Catacumba.Entity;
using Catacumba.Data;

public class CombatManagerConfig : ScriptableObject
{
    /*
    [MenuItem("Assets/Create/CombatManagerConfig")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<CombatManagerConfig>();
    }
    */

    [Header("Weapon Animator Overrides")]
    public RuntimeAnimatorController DefaultController;
    public AnimatorOverrideController TwoHandedSword;
    public AnimatorOverrideController DaggerController;
    public AnimatorOverrideController ScepterController;
    public AnimatorOverrideController SwordAndShieldController;

    public RuntimeAnimatorController WeaponTypeToController(EWeaponType type)
    {
        switch (type)
        {
            case EWeaponType.Dagger: return DaggerController;
            case EWeaponType.Scepter: return ScepterController;
            case EWeaponType.Sword: return SwordAndShieldController;
            case EWeaponType.TwoHandedSword: return TwoHandedSword;
            default: return DefaultController;
        }
    }
}
