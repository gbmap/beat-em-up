﻿using System;
using UnityEngine;
using Catacumba.Data.Items;

namespace Catacumba.Data
{
    [Serializable]
    public class CharacterStats
    {
        public const int MaxAttributeLevel = 256;

        public System.Action<CharacterStats> OnStatsChanged = delegate { };

        public int Level { get; set; }
        public CharAttributesI Attributes;

        private int health;
        public int Health
        {
            get { return health; }
            set
            {
                health = Mathf.Clamp(value, 0, MaxHealth);
                OnStatsChanged?.Invoke(this);
            } 
        }
        public int MaxHealth { get { return CombatManager.GetMaxHealth(this); } }
        public float HealthNormalized { get { return ((float)Health / MaxHealth); } }

        public int Mana { get; set; }
        public int MaxMana { get { return CombatManager.GetMaxMana(this); } }

        public int Stamina { get { return MaxHealth/5; } }
        public int CurrentStamina { get; set; }
        public float StaminaBar { get { return ((float)CurrentStamina)/Stamina; } }

        public float PoiseChance { get { return CombatManager.GetPoiseChance(this); } }
        public float MoveSpeed { get { return 5f; } }

        public bool CanBeKnockedOut { get; set; }

        public Inventory Inventory { get; private set; }

        public CharacterStats(Catacumba.Data.CharacterStatConfiguration stats, Inventory inventory=null)
        {
            Level = 1;
            Attributes = new CharAttributesI()
            {
                Strength  = Mathf.RoundToInt(stats.StrengthCurve.Evaluate(UnityEngine.Random.value)),
                Dexterity = Mathf.RoundToInt(stats.DexterityCurve.Evaluate(UnityEngine.Random.value)),
                Vigor     = Mathf.RoundToInt(stats.VigorCurve.Evaluate(UnityEngine.Random.value)),
                Magic     = Mathf.RoundToInt(stats.MagicCurve.Evaluate(UnityEngine.Random.value))
            };

            if (inventory == null)
                Inventory = ScriptableObject.CreateInstance<Inventory>();
            else
                // shallow copy is fine in this case
                Inventory = ScriptableObject.Instantiate(inventory);

            Health          = MaxHealth;
            Mana            = MaxMana;
            CurrentStamina  = Stamina;
            CanBeKnockedOut = true;
        }
    }

}