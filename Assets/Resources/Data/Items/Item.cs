using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items
{
    public enum EItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }

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

        public GameObject Model;

        public TItemCharacteristic[] GetCharacteristics<TItemCharacteristic>()
            where TItemCharacteristic : ItemCharacteristic
        {
            try
            {
                return Characteristics.Where(c => c is TItemCharacteristic).Cast<TItemCharacteristic>().ToArray();
            }
            catch (System.NullReferenceException)
            {
                return null;
            }
        }
    }

}