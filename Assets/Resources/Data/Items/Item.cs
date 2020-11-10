using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Character;
using Catacumba.Data.Items.Characteristics;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items
{
    public abstract class ItemCharacteristic : ScriptableObject { }

    [CreateAssetMenu(menuName="Data/Item/Item", fileName="Item")]
    public class Item : ScriptableObject
    {
        public string Name;

        [Multiline]
        public string Description;
        public ItemRarity Rarity;
        public CharAttributesI Attributes;
        public List<AttributeValueI> AttributeStats;

        public List<ItemCharacteristic> Characteristics;

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
    }

}