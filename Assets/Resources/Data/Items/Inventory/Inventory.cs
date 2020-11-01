using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Character;
using Catacumba.Data.Items.Characteristics;
using UnityEngine;

namespace Catacumba.Data.Items
{
    public class InventoryEquipParams
    {
        public Item Item;
        public BodyPart Slot;

        // bool = success or not
        public System.Action<InventoryEquipResult> Callback;
    }

    public class InventoryEquipResult : Interactions.InteractionResult
    {
        public enum EEquipResult
        {
            Success,
            ItemCantEquipOnSlot,
            ItemNotEquippable,
            NoSlotsAvailable
        }

        public InventoryEquipParams Params;
        public EEquipResult Result;
    }

    [CreateAssetMenu(menuName="Data/Inventory/Inventory", fileName="Inventory")]
    public class Inventory : ScriptableObject
    {
        public List<BodyPart> Slots; 
        public Dictionary<BodyPart, Item> EquippedItems = new Dictionary<BodyPart, Item>();

        void Awake()
        {

        }

        public System.Action<InventoryEquipResult> OnItemEquipped;

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

        public InventoryEquipResult.EEquipResult Equip(InventoryEquipParams parameters) 
        {
            if (parameters.Slot == null)
                return EquipAnySlot(parameters);

            return EquipOnSlot(parameters);
        }

        /*
        private InventoryEquipResult.EEquipResult Equip(Item item, BodyPart slot, bool checkEquippable = true)
        {
            if (!CanEquipOnSlot(slot))
            {
                return InventoryEquipResult.EEquipResult.ItemCantEquipOnSlot;
            }

            CharacteristicEquippable[] characteristics = item.GetCharacteristics<CharacteristicEquippable>();

            if (characteristics == null || characteristics.Length == 0)
                return false;

            if (!characteristics.Any(c => c.EquipsOnSlot(slot)))
                return false;

            foreach (CharacteristicEquippable equippable in characteristics)
            {
                //if (equippable.Equip())

            }
            return CacheEquip(item, slot);
        }
        */

        public InventoryEquipResult.EEquipResult EquipOnSlot(InventoryEquipParams parameters)
        {
            if (!IsSlotEmpty(parameters.Slot))
                return EquipResult(parameters, InventoryEquipResult.EEquipResult.NoSlotsAvailable);

            return EquipItem(parameters);
        }

        public InventoryEquipResult.EEquipResult EquipAnySlot(InventoryEquipParams parameters)
        {
            BodyPart[] slots = GetBodyParts(parameters.Item);
            if (slots == null || slots.Length == 0)
                return EquipResult(parameters, InventoryEquipResult.EEquipResult.ItemCantEquipOnSlot);

            BodyPart slotToEquip = slots.FirstOrDefault(s => s != null && !EquippedItems.ContainsKey(s));
            if (!slotToEquip)
                return EquipResult(parameters, InventoryEquipResult.EEquipResult.NoSlotsAvailable);

            parameters.Slot = slotToEquip;
            return EquipItem(parameters);
        }

        private InventoryEquipResult.EEquipResult EquipItem(InventoryEquipParams parameters)
        {
            EquippedItems[parameters.Slot] = parameters.Item;
            return EquipResult(parameters, InventoryEquipResult.EEquipResult.Success);
        }

        private InventoryEquipResult.EEquipResult EquipResult(InventoryEquipParams parameters, InventoryEquipResult.EEquipResult result)
        {
            InventoryEquipResult res = new InventoryEquipResult()
            {
                Params = parameters,
                Result = result
            };

            if (result == InventoryEquipResult.EEquipResult.Success)
                OnItemEquipped?.Invoke(res);

            parameters.Callback?.Invoke(res);
            return result;
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
