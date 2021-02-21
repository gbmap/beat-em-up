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

    /////////////////////////
    //  EVENT DATA

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
    [CreateAssetMenu(menuName="Data/Inventory/Inventory", fileName="Inventory")]
    public class Inventory : ScriptableObject
    {
        public InventoryItems Items; // = new InventoryItems();
        public InventoryBag Bag; //= new InventoryBag();

        public System.Action<InventoryEquipResult> OnItemEquipped;
        public System.Action<InventoryDropResult> OnItemDropped;

        public System.Action<InventoryEquipResult, CharacteristicWeaponizable> OnWeaponEquipped;

        void Awake()
        {
            //Items = ScriptableObject.Instantiate<InventoryItems>(Items);
            //Bag   = ScriptableObject.Instantiate<InventoryBag>(Bag);
            OnItemEquipped += Cb_OnItemEquipped;
        }

        public void DispatchItemEquippedForAllItems()
        {
            if (!Items) return;
            foreach((BodyPart, Item) slot in Items.GetEnumerable())
            {
                if (slot.Item2 == null) continue;
                OnItemEquipped?.Invoke(new InventoryEquipResult
                {
                    Params = new InventoryEquipParams
                    {
                        Item = slot.Item2,
                        Slot = slot.Item1,
                        Callback = null
                    },
                    Result = InventoryEquipResult.EEquipResult.Success
                });
            }
        }

        public Item Amputate(BodyPart part)
        {
            return null;
            /*
            if (!Items.HasSlot(part))
                return null;

            Item item = Items.GetSlot(part);
            Items.Remove(part);
            if (slot == null)
                return null;

            Items.Remove(slot.Part);

            Item item = slot.Item;
            if (!item)
                return null;
            
            return item;
            */
        }

        public Item GetSlot(BodyPart part)
        {
            return Items.GetSlotItem(part, out bool hasSlot);
        }

        public BodyPart GetWeaponSlot()
        {
            return Items.GetWeaponSlot();
        }

        public Item GetWeapon()
        {
            return Items.GetWeapon();
        }

        public bool HasItem(Item item)
        {
            return Items.HasItem(item);
        }

        public InventoryEquipResult Equip(InventoryEquipParams parameters) 
        {
            var result = Items.Equip(parameters);
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

            Item item = Items.RemoveItem(parameters.Slot);

            if (item == null)
                return DropResult(parameters, null);

            // EQUIP DEFAULT WEAPON
            if (GetWeaponSlot() == parameters.Slot)
            {
                Equip(new InventoryEquipParams
                {
                    Item = Resources.Load<Item>("Data/Items/Item_Fists"),
                    Slot = parameters.Slot
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
