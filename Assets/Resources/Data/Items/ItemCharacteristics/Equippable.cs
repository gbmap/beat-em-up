using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Catacumba.Data.Items
{
    [CreateAssetMenu(menuName="Data/Item Characteristic/Equippable", fileName="Equippable")]
    public class EquippableCharacteristic : ItemCharacteristic
    {
        public BodyPart[] Slots;

        public bool EquipsOnSlot(BodyPart part)
        {
            return Slots.Any(s => s == part);
        }
    }
}