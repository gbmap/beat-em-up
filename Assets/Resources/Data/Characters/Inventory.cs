using System;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Data
{
    public enum EInventorySlot
    {
        Helm,
        Armor,
        Boot,
        Ring1,
        Ring2,
        Weapon
    }

    [Serializable]
    public class Inventory
    {
        [SerializeField]
        private int[] inventory;
        public int[] ItemIds
        {
            get { return inventory; }
        }

        [SerializeField]
        private List<ItemConfig> keyItems;
        public List<ItemConfig> KeyItems
        {
            get { return keyItems; }
        }

        public Inventory()
        {
            inventory = new int[((EInventorySlot[])Enum.GetValues(typeof(EInventorySlot))).Length];
            keyItems = new List<ItemConfig>();
        }

        public Inventory(ItemConfig[] keyItems)
        {
            inventory = new int[((EInventorySlot[])Enum.GetValues(typeof(EInventorySlot))).Length];
            this.keyItems = new List<ItemConfig>(keyItems);
        }

        public Inventory(int[] items, ItemConfig[] keyItems)
        {
            inventory = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                inventory[i] = items[i];
            }

            this.keyItems = new List<ItemConfig>(keyItems);
        }

        public ItemStats this[EInventorySlot slot]
        {
            get => ItemManager.Instance.GetItem( inventory[(int)slot] );
            set => inventory[(int)slot] = value.Id;
        }

        public CharAttributesI GetTotalAttributes()
        {
            CharAttributesI t = new CharAttributesI();
        
            foreach (var v in inventory)
            {
                if (v == 0) continue;
                t.Add(ItemManager.Instance.GetItem(v).Attributes);
            }

            return t;
        }

        public CharAttributesF GetTotalDamageScaling()
        {
            CharAttributesF t = new CharAttributesF();

            foreach (var v in inventory)
            {
                if (v == 0) continue;
                t.Add(ItemManager.Instance.GetItem(v).DamageScaling);
            }

            return t;
        }

        public void UnEquip(EInventorySlot slot)
        {
            inventory[(int)slot] = 0;
        }

        public bool HasEquip(EInventorySlot slot)
        {
            return inventory[(int)slot] != 0;
        }

        public bool HasKey(ItemConfig itemConfig)
        {
            return keyItems.Contains(itemConfig);
        }

        public void GrabKey(ItemConfig itemCfg)
        {
            if (HasKey(itemCfg)) return;
            keyItems.Add(itemCfg);
        }
    }

    public enum EItemType
    {
        Equip,
        Consumable,
        Key
    }

    public enum EItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }

    [Serializable]
    public class ItemStats
    {
        public int Id;
        public EItemType ItemType;
        public EItemRarity Rarity;
        public EInventorySlot Slot;
        public EWeaponType WeaponType;
        public CharAttributesI Attributes;
        public CharAttributesF DamageScaling;

        [Range(0f, 2f)]
        public float WeaponColliderScaling = 0f;

        public bool IsRanged { get { return WeaponType == EWeaponType.Bow || WeaponType == EWeaponType.Scepter; } }
    }

    public enum EWeaponType
    {
        Fists,
        Sword,
        Dagger,
        Scepter,
        TwoHandedSword,
        Bow
    }
}
