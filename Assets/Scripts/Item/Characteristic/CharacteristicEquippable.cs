using System.Linq;
using UnityEngine;
using Catacumba.Entity;
using Catacumba.Data.Character;

namespace Catacumba.Data.Items.Characteristics
{
    [CreateAssetMenu(menuName="Data/Item/Characteristic/Equippable", fileName="Equippable")]
    public class CharacteristicEquippable : ItemCharacteristic
    {
        [System.Serializable]
        public class SlotData
        {
            public BodyPart BodyPart;
            public TransformationData ModelOffset;
        }
        public SlotData[] Slots;

        public bool EquipsOnSlot(BodyPart part)
        {
            return Slots.Any(s => s.BodyPart == part);
        }

        public virtual bool Equip(CharacterData data, Item item, BodyPart slot)
        {
            if (!EquipsOnSlot(slot))
                return false;
            return true;
        }

        public SlotData GetSlot(BodyPart part)
        {
            return Slots.FirstOrDefault(s => s.BodyPart = part);
        }
    }
}