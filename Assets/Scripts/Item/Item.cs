using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Character;
using Catacumba.Data.Items.Characteristics;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items
{
    public enum EItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }

    public abstract class ItemCharacteristic : ScriptableObject { }

    [CreateAssetMenu(menuName="Data/Item/Item", fileName="Item")]
    public class Item : ScriptableObject
    {
        public string Name;

        [Multiline]
        public string Description;
        public ItemRarity Rarity;
        public CharAttributesI Attributes;

        public ItemCharacteristic[] Characteristics;

        public GameObject Model;

        public bool OccupiesSlot = true;

        public bool Equip(CharacterData character, BodyPart slot)
        {
            if (!HasCharacteristic<CharacteristicEquippable>())
                return false;

            CharacteristicEquippable[] equippables = GetCharacteristics<CharacteristicEquippable>();
            equippables = equippables.Where(e => e.EquipsOnSlot(slot)).ToArray();
            if (equippables == null || equippables.Length == 0)
                return false;

            CharacteristicEquippable characteristic = equippables[0];
            return characteristic.Equip(character, this, slot);
        }

        public TItemCharacteristic[] GetCharacteristics<TItemCharacteristic>()
            where TItemCharacteristic : ItemCharacteristic
        {
            try
            {
                return Characteristics.Where(c => c is TItemCharacteristic).Cast<TItemCharacteristic>().ToArray();
            }
            catch (System.NullReferenceException)
            {
                return null;
            }
        }

        public TItemCharacteristic GetCharacteristic<TItemCharacteristic>()
            where TItemCharacteristic : ItemCharacteristic
        {
            try
            {
                return Characteristics.FirstOrDefault(c => c is TItemCharacteristic) as TItemCharacteristic;
            }
            catch (System.NullReferenceException)
            {
                return null;
            }
        }

        public bool HasCharacteristic<TCharacteristic>() 
            where TCharacteristic : ItemCharacteristic
        {
            return Characteristics.Any(c => c is TCharacteristic);
        }

        public static bool CompareInstance(Item a, Item b)
        {
            return a.GetInstanceID() == b.GetInstanceID();
        }

        public static bool Compare(Item a, Item b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null)  return false;
            return string.Equals(a.Name, b.Name);
        }

        public override bool Equals(object other)
        {
            if (other is Item) return Item.Compare(this, other as Item);
            return base.Equals(other);
        }

        public Item Clone()
        {
            Item instance = ScriptableObject.Instantiate(this);
            for (int i = 0; i < Characteristics.Length; i++)
            {
                ItemCharacteristic characteristic = (ItemCharacteristic)Characteristics[i];
                instance.Characteristics[i] = ScriptableObject.Instantiate(characteristic);
            }

            return instance;
        }
    }

} 