using System.Collections;
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

    public class InventoryDropParams
    {
        public BodyPart Slot;
        public System.Action<InventoryDropResult> Callback;
    }

    public class InventoryDropResult
    {
        public InventoryDropParams Params;
        public Item Item;
    }

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
    
    [System.Serializable]
    public class InventorySlots : IEnumerable<InventorySlot> 
    {
        [SerializeField] List<InventorySlot> slots = new List<InventorySlot>();
        public int WeaponSlotIndex = 0;

        public InventorySlot GetSlot(BodyPart part)
        {
            return slots.FirstOrDefault(s => s.Part == part);
        }

        public InventorySlot GetSlot(string partName)
        {
            return slots.FirstOrDefault(s => s.Part.name.Equals(partName));
        }

        public InventorySlot GetWeaponSlot()
        {
            return slots[WeaponSlotIndex];
        }

        public bool HasSlot(BodyPart part)
        {
            return slots.Any(s => s.Part == part);
        }

        public int IndexOf(BodyPart slot)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Part == slot)
                    return i;
            }

            return -1;
        }

        public void Remove(BodyPart slot)
        {
            int index = IndexOf(slot);
            slots.RemoveAt(index);
        }

        public bool IsEmpty(BodyPart part)
        {
            InventorySlot s = GetSlot(part);
            if (s == null)
                throw new KeyNotFoundException();

            return s.IsEmpty();
        }

        public IEnumerator<InventorySlot> GetEnumerator()
        {
            return slots.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return slots.GetEnumerator();
        }
    }

    [CreateAssetMenu(menuName="Data/Inventory/Inventory", fileName="Inventory")]
    public class Inventory : ScriptableObject
    {
        public InventorySlots Slots = new InventorySlots();

        public System.Action<InventoryEquipResult> OnItemEquipped;
        public System.Action<InventoryDropResult> OnItemDropped;

        public void DispatchItemEquippedForAllItems()
        {
            foreach (var slot in Slots)
            {
                if (slot.Item == null)
                    continue;

                OnItemEquipped?.Invoke(new InventoryEquipResult
                {
                    Params = new InventoryEquipParams
                    {
                        Item = slot.Item,
                        Slot = slot.Part,
                        Callback = null
                    },
                    Result = InventoryEquipResult.EEquipResult.Success
                });
            }
        }

        public Item Amputate(BodyPart part)
        {
            InventorySlot slot = Slots.GetSlot(part);
            if (slot == null)
                return null;

            Slots.Remove(slot.Part);

            Item item = slot.Item;
            if (!item)
                return null;
            
            return item;
        }

        public InventorySlot GetSlotByString(string name)
        {
            return Slots.GetSlot(name);
        }

        public InventorySlot GetWeaponSlot()
        {
            return Slots.GetWeaponSlot();
        }

        public bool IsSlotEmpty(BodyPart slot)
        {
            return Slots.IsEmpty(slot);
        }

        public InventoryEquipResult.EEquipResult Equip(InventoryEquipParams parameters) 
        {
            if (parameters.Slot == null)
                return EquipAnySlot(parameters);

            return EquipOnSlot(parameters);
        }

        public InventoryEquipResult.EEquipResult EquipOnSlot(InventoryEquipParams parameters)
        {
            if (!IsSlotEmpty(parameters.Slot))
                return EquipResult(parameters, InventoryEquipResult.EEquipResult.NoSlotsAvailable);

            return EquipItem(parameters);
        }

        public InventoryDropResult Drop(InventoryDropParams parameters)
        {
            if (parameters.Slot == null)
                return DropResult(parameters, null);

            InventorySlot s = Slots.GetSlot(parameters.Slot);

            if (s == null || s.IsEmpty())
                return DropResult(parameters, null);

            Item item = s.Item;
            s.Item = null;

            // EQUIP DEFAULT WEAPON
            if (GetWeaponSlot().Part == s.Part)
            {
                EquipOnSlot(new InventoryEquipParams
                {
                    Item = Resources.Load<Item>("Data/Items/Item_Fists"), 
                    Slot = s.Part
                });
            }

            return DropResult(parameters, item);
        }

        private InventoryDropResult DropResult(InventoryDropParams parameters, Item item)
        {
            var res = new InventoryDropResult
            {
                Params = parameters,
                Item = item
            };

            if (item != null)
                OnItemDropped?.Invoke(res);

            parameters.Callback?.Invoke(res);
            return res;
        }

        public InventoryEquipResult.EEquipResult EquipAnySlot(InventoryEquipParams parameters)
        {
            BodyPart[] slots = GetBodyParts(parameters.Item);
            if (slots == null || slots.Length == 0)
                return EquipResult(parameters, InventoryEquipResult.EEquipResult.ItemCantEquipOnSlot);

            BodyPart slotToEquip = slots.FirstOrDefault(s => s != null && Slots.HasSlot(s));
            if (!slotToEquip)
                return EquipResult(parameters, InventoryEquipResult.EEquipResult.NoSlotsAvailable);

            parameters.Slot = slotToEquip;
            return EquipItem(parameters);
        }

        private InventoryEquipResult.EEquipResult EquipItem(InventoryEquipParams parameters)
        {
            InventorySlot slot = Slots.GetSlot(parameters.Slot);
            slot.Item = parameters.Item;
            return EquipResult(parameters, InventoryEquipResult.EEquipResult.Success);
        }

        private InventoryEquipResult.EEquipResult EquipResult(
            InventoryEquipParams parameters, 
            InventoryEquipResult.EEquipResult result)
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
            return Slots.HasSlot(slot);
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
