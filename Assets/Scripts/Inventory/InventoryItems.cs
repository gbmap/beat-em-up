using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Character;
using Catacumba.Data.Items.Characteristics;
using UnityEngine;

namespace Catacumba.Data.Items
{
    [System.Serializable]
    public class InventorySlot
    {
        public BodyPart Part;
        public Item Item;

        public bool IsEmpty()
        {
            return Item == null || !Item.OccupiesSlot;
        }
    }

    [CreateAssetMenu(menuName="Data/Inventory/Items", fileName="InventoryItems")]
    public class InventoryItems : ScriptableObject
    {
        public InventorySlots Slots;
        public List<Item> Items;

        public System.Tuple<BodyPart, Item> GetSlot(BodyPart slot, out bool hasSlot)
        {
            Item item = GetSlotItem(slot, out hasSlot);
            if (hasSlot)
                return new System.Tuple<BodyPart, Item>(slot, item);
            return null;
        }

        public Item GetSlotItem(BodyPart part, out bool hasSlot)
        {
            int index = GetSlotIndex(part);
            hasSlot = index != -1;
            if (!hasSlot) return null;
            if (Items.Count < index) return null;
            return Items[index];
        }
        
        public int GetSlotIndex(BodyPart part)
        {
            return Slots.IndexOf(part);
        }

        private bool SetSlot(BodyPart part, Item item)
        {
            int index = GetSlotIndex(part);
            if (index == -1) return false;
            Items[index] = item;
            return true;
        }

        public BodyPart GetWeaponSlot()
        {
            return Slots.GetWeaponSlot();
        }

        public Item GetWeapon()
        {
            return GetSlotItem(Slots.GetWeaponSlot(), out bool hasSlot);
        }

        public bool HasSlot(BodyPart part)
        {
            return Slots.Contains(part);
        }

        public Item RemoveSlot(BodyPart slot)
        {
            Item item = GetSlotItem(slot, out bool hasSlot);
            if (hasSlot)
                Slots.Remove(slot);
            return item;
        }

        public Item RemoveItem(BodyPart slot)
        {
            Item item = GetSlotItem(slot, out bool hasSlot);
            if (item != null)
                SetSlot(slot, null);
            return item;
        }

        public bool IsEmpty(BodyPart part)
        {
            return GetSlotItem(part, out bool hasSlot) == null;
        }

        public bool HasItem(Item item)
        {
            return Items.Contains(item);
        }

        public InventoryEquipResult Equip(InventoryEquipParams parameters) 
        {
            if (parameters.Slot == null)
                return EquipAnySlot(parameters);

            return EquipOnSlot(parameters);
        }

        public InventoryEquipResult EquipOnSlot(InventoryEquipParams parameters)
        {
            if (!IsEmpty(parameters.Slot))
                return EquipResult(parameters, InventoryEquipResult.EEquipResult.NoSlotsAvailable);

            return EquipItem(parameters);
        }

        InventoryEquipResult EquipAnySlot(InventoryEquipParams parameters)
        {
            BodyPart[] slots = GetBodyParts(parameters.Item);
            if (slots == null || slots.Length == 0)
                return EquipResult(parameters, InventoryEquipResult.EEquipResult.ItemCantEquipOnSlot);

            BodyPart slotToEquip = slots.FirstOrDefault(s => s != null && CanEquipOnSlot(s));
            if (!slotToEquip)
                return EquipResult(parameters, InventoryEquipResult.EEquipResult.NoSlotsAvailable);

            parameters.Slot = slotToEquip;
            return EquipItem(parameters);
        }

        private InventoryEquipResult EquipItem(InventoryEquipParams parameters)
        {
            if (SetSlot(parameters.Slot, parameters.Item))
                return EquipResult(parameters, InventoryEquipResult.EEquipResult.Success);
            return EquipResult(parameters, InventoryEquipResult.EEquipResult.ItemCantEquipOnSlot);
        }

        private static BodyPart[] GetBodyParts(Item item)
        {
            CharacteristicEquippable[] characteristics = item.GetCharacteristics<CharacteristicEquippable>();
            if (characteristics == null || characteristics.Length == 0)
                return null;

            return characteristics.SelectMany(c => c.Slots.Select(s => s.BodyPart)).ToArray();
        }

        private InventoryEquipResult EquipResult(
            InventoryEquipParams parameters, 
            InventoryEquipResult.EEquipResult result)
        {
            InventoryEquipResult res = new InventoryEquipResult()
            {
                Params = parameters,
                Result = result
            };

            parameters.Callback?.Invoke(res);
            return res;
        }

        private bool CanEquipOnSlot(BodyPart slot)
        {
            return HasSlot(slot);
        }

        public IEnumerable<(BodyPart, Item)> GetEnumerable()
        {
            return Slots.GetEnumerable(Items);
        }
    }
}