using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Character;
using Catacumba.Data.Items.Characteristics;
using UnityEngine;

namespace Catacumba.Data.Items
{
    [CreateAssetMenu(menuName="Data/Inventory/Inventory", fileName="Inventory")]
    public class Inventory : ScriptableObject
    {
        [SerializeField] private List<BodyPart> Slots; 
        private Dictionary<BodyPart, Item> EquippedItems = new Dictionary<BodyPart, Item>();

        public Item Amputate(BodyPart part)
        {
            if (!Slots.Contains(part))
                return null;

            Slots.Remove(part);

            Item item;
            if (!EquippedItems.TryGetValue(part, out item))
                return null;
            
            return item;
        }

        public bool IsSlotEmpty(BodyPart slot)
        {
            return !EquippedItems.ContainsKey(slot) || EquippedItems[slot] == null;
        }

        public bool Equip(Item item, BodyPart slot, bool checkEquippable = true)
        {
            if (!CanEquipOnSlot(slot))
                return false;

            CharacteristicEquippable[] characteristics = item.GetCharacteristics<CharacteristicEquippable>();
            if (characteristics == null || characteristics.Length == 0)
                return false;

            if (!characteristics.Any(c => c.EquipsOnSlot(slot)))
                return false;

            return CacheEquip(item, slot);
        }

        public bool Equip(Item item)
        {
            BodyPart[] slots = GetBodyParts(item);
            if (slots == null)
                return false;

            BodyPart slotToEquip = slots.FirstOrDefault(s => !EquippedItems.ContainsKey(s));
            if (!slotToEquip)
                return false;

            return CacheEquip(item, slotToEquip);
        }

        private bool CacheEquip(Item item, BodyPart slot)
        {
            if (!IsSlotEmpty(slot))
            {
                return false; // TODO: maybe drop item 
            }

            EquippedItems[slot] = item;
            return true;
        }

        private bool CanEquipOnSlot(BodyPart slot)
        {
            return Slots.Contains(slot);
        }

        private static BodyPart[] GetBodyParts(Item item)
        {
            CharacteristicEquippable[] characteristics = item.GetCharacteristics<CharacteristicEquippable>();
            if (characteristics == null || characteristics.Length == 0)
                return null;

            return characteristics.SelectMany(c => c.Slots).ToArray();
        }

    }

}
