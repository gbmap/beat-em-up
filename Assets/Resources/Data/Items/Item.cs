using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [CreateAssetMenu(menuName="Data/Item", fileName="Item")]
    public class Item : ScriptableObject
    {
        public string Name;

        [Multiline]
        public string Description;
        public EItemRarity Rarity;
        public CharAttributesI Attributes;

        public ItemCharacteristic[] Characteristics;

        public GameObject Model;

        public bool Equip(CharacterData character, BodyPart slot)
        {
            if (!HasCharacteristic<EquippableCharacteristic>())
                return false;

            EquippableCharacteristic[] equippables = GetCharacteristics<EquippableCharacteristic>();
            equippables = equippables.Where(e => e.EquipsOnSlot(slot)).ToArray();
            if (equippables == null || equippables.Length == 0)
                return false;

            EquippableCharacteristic characteristic = equippables[0];
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

        public bool HasCharacteristic<TCharacteristic>() 
            where TCharacteristic : ItemCharacteristic
        {
            return Characteristics.Any(c => c is TCharacteristic);
        }
    }

}