using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Character;
using UnityEngine;

namespace Catacumba.Data.Items
{
    /*
        Describes the available slots in a character. Head, torso, arms, etc.
    */
    [CreateAssetMenu(menuName="Data/Inventory/Slots", fileName="InventorySlots")]
    public class InventorySlots : ScriptableObject
    {
        public List<BodyPart> Slots = new List<BodyPart>();
        [SerializeField] int WeaponSlotIndex = 0;

        public int Count { get { return Slots.Count; } }

        public bool Contains(BodyPart part)
        {
            return Slots.Contains(part);
        }

        public int IndexOf(BodyPart part)
        {
            return Slots.IndexOf(part);
        }

        public bool Remove(BodyPart part)
        {
            if (!Slots.Contains(part))
                return false;

            Slots.Remove(part);
            return true;
        }

        public BodyPart GetWeaponSlot()
        {
            return Slots[WeaponSlotIndex];
        }

        public int GetWeaponSlotIndex()
        {
            return WeaponSlotIndex;
        }

        public IEnumerable<(BodyPart, Item)> GetEnumerable(List<Item> items)
        {
            return Slots.Zip(items, (BodyPart b, Item i) => (b, i));
        }

    }
}