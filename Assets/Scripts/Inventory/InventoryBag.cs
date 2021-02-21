using System.Linq;
using Catacumba.Data.Items.Characteristics;
using UnityEngine;

namespace Catacumba.Data.Items
{
    /*
        Holds items that are not equipped. May hold gold, potions,
        keys, misc items, unequipped items, etc.
    */
    [CreateAssetMenu(menuName="Data/Inventory/Bag", fileName="InventoryBag")]
    public class InventoryBag : ScriptableObject
    {
        [SerializeField] Item[] items = new Item[10];

        public int NumberOfItems { get { return items.Where(i=>i != null).Count(); } }

        /*
        public InventoryBag()
        {
            items = new Item[10];
            numberItems = 0;
        }
        */

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
}