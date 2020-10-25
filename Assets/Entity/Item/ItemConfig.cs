using UnityEngine;
using Catacumba.Entity;
using Catacumba.Data;

namespace Catacumba.Data
{
    public enum EWeaponHand
    {
        Left, Right
    }

    public class ItemConfig : ScriptableObject
    {
        /*[MenuItem("Assets/Create/Item/ItemConfig")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<ItemConfig>();
        }*/

        [Header("UI Config")]
        public string Name;
        public string Description;

        [Space]
        [Header("Stats")]
        public ItemStats Stats;
        
        [Space]
        public GameObject Prefab;
        public AnimatorOverrideController AnimationOverride;

        [Space]
        [Header("Weapon Configuration")]

        public bool CustomSlashColors = false;
        public Gradient SlashColors;
        public ParticleSystem.MinMaxCurve StartSize = new ParticleSystem.MinMaxCurve(12, 17);

        public float DistanceFromCharacter = 1.4f;

        [Space]
        public bool OverrideHand = false;
        public EWeaponHand Hand;
    }
}