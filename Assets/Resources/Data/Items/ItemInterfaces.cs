using System.Collections;
using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items
{
    [CreateAssetMenu(menuName="Data/Body Part", fileName="BodyPart")]
    public class BodyPart : ScriptableObject { }

    public abstract class ItemCharacteristic : ScriptableObject { }

    [CreateAssetMenu(menuName="Data/Item", fileName="Item")]
    public class Item : ScriptableObject
    {
        public string Name;

        [Multiline]
        public string Description;
        public EItemRarity Rarity;
        public CharAttributesI Attributes;

        public ItemCharacteristic[] Characteristics;
    }

}