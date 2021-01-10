using System;
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
            if (slots.Count == 0 || WeaponSlotIndex < 0 || WeaponSlotIndex > slots.Count)
                return null;

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

        public bool HasItem(Item item)
        {
            return slots.Any(s => s.Item == item);
        }

        public IEnumerator<InventorySlot> GetEnumerator()
        {
            return slots.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return slots.GetEnumerator();
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
            InventorySlot slot = GetSlot(parameters.Slot);
            slot.Item = parameters.Item;
            return EquipResult(parameters, InventoryEquipResult.EEquipResult.Success);
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

    }

    [System.Serializable]
    public class InventoryBag
    {
        [SerializeField] Item[] items;
        [SerializeField] int numberItems;

        public int NumberOfItems { get { return numberItems; } }

        public InventoryBag()
        {
            items = new Item[10];
            numberItems = 0;
        }

        public bool Grab(Item item)
        {
            if (!item.IsInstance) item = item.Clone();
            var stackable = item.GetCharacteristic<CharacteristicStackable>();
            if (!stackable)
                return AllocateItem(item);

            Item stackableInBag = items.FirstOrDefault(i => Item.Compare(item, i));
            if (!stackableInBag)
                return AllocateItem(item);

            CharacteristicStackable ss = stackableInBag.GetCharacteristic<CharacteristicStackable>();
            ss.Stack(ref stackableInBag, ref item);

            if (item != null) AllocateItem(item);
            return true;
        }

        public Item Get(int index)
        {
            if (index < 0 || index > items.Length-1) return null;
            return items[index];
        }

        public Item Get(string name)
        {
            return items.FirstOrDefault(i => i != null && i.Name == name);
        }

        public bool DropAmount(Item item, int amount)
        {
            Item itemInstance = Get(item.Name);
            if (!itemInstance) return false;

            int index = GetItemIndex(itemInstance);
            var stackable = itemInstance.GetCharacteristic<CharacteristicStackable>();
            if (!stackable)
            {
                if (amount == 1)
                    return DeallocateItem(itemInstance);
                return false;
            }

            if (amount <= stackable.CurrentAmount)
                stackable.CurrentAmount -= amount;

            if (stackable.CurrentAmount > 0)
                return true;

            return DeallocateItem(itemInstance);
            // return true;
        }

        private bool AllocateItem(Item item)
        {
            if (item == null) return false;

            int index = FindEmptySlot();
            if (index == -1) return false;

            items[index] = item;
            numberItems++;
            return true;
        }

        private bool DeallocateItem(Item item)
        {
            if (item == null) return false;

            int index = GetItemIndex(item);
            if (index == -1) return false;

            Item itemInstance = items[index];
            GameObject.DestroyImmediate(itemInstance);
            itemInstance = null;
            numberItems--;
            return true;
        }

        private int FindEmptySlot()
        {
            return GetItemIndex(null);
        }

        private int GetItemIndex(Item item)
        {
            for (int i = 0; i < items.Length; i++)
                if (Item.Compare(items[i], item)) return i;
            return -1;
        }
    }

    [CreateAssetMenu(menuName="Data/Inventory/Inventory", fileName="Inventory")]
    public class Inventory : ScriptableObject
    {
        public InventorySlots Slots = new InventorySlots();
        public InventoryBag Bag = new InventoryBag();

        public System.Action<InventoryEquipResult> OnItemEquipped;
        public System.Action<InventoryDropResult> OnItemDropped;

        public System.Action<InventoryEquipResult, CharacteristicWeaponizable> OnWeaponEquipped;

        void Awake()
        {
            OnItemEquipped += Cb_OnItemEquipped;
        }

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

        public InventorySlot GetSlot(BodyPart part)
        {
            return Slots.GetSlot(part);
        }

        public InventorySlot GetSlotByString(string name)
        {
            return Slots.GetSlot(name);
        }

        public InventorySlot GetWeaponSlot()
        {
            return Slots.GetWeaponSlot();
        }

        public Item GetWeapon()
        {
            return Slots.GetWeaponSlot()?.Item;
        }

        public bool HasItem(Item item)
        {
            return Slots.HasItem(item);
        }

        public InventoryEquipResult Equip(InventoryEquipParams parameters) 
        {
            var result = Slots.Equip(parameters);
            if (result.Result == InventoryEquipResult.EEquipResult.Success)
                OnItemEquipped?.Invoke(result);

            return result;
        }

        public bool Grab(Item item)
        {
            return Bag.Grab(item);
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
                Slots.EquipOnSlot(new InventoryEquipParams
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

        private void Cb_OnItemEquipped(InventoryEquipResult res)
        {
            if (res.Result != InventoryEquipResult.EEquipResult.Success) return;
            var weapon = res.Params.Item.GetCharacteristic<CharacteristicWeaponizable>();
            if (weapon != null)
                OnWeaponEquipped?.Invoke(res, weapon);
        }
    }

}
