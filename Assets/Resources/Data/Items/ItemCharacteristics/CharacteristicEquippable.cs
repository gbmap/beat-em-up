using System.Linq;
using UnityEngine;
using Catacumba.Entity;
using Catacumba.Data.Character;

namespace Catacumba.Data.Items.Characteristics
{
    [CreateAssetMenu(menuName="Data/Item Characteristic/Equippable", fileName="Equippable")]
    public class CharacteristicEquippable : ItemCharacteristic
    {
        public BodyPart[] Slots;

        public bool EquipsOnSlot(BodyPart part)
        {
            return Slots.Any(s => s == part);
        }

        public bool Equip(CharacterData data, Item item, BodyPart slot)
        {
            return false;
        }
    }
}