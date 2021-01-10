using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items
{
    [CreateAssetMenu(menuName="Data/Item Characteristic/Consumable", fileName="Consumable")]
    public abstract class CharacteristicConsumable : ItemCharacteristic
    {
        public abstract void Consume(CharacterData character);
    }
}