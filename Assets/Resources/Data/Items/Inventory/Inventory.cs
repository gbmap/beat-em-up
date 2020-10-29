using System;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Data
{
    [CreateAssetMenu(menuName="Data/Inventory/Body Part", fileName="BodyPart")]
    public class BodyPart : ScriptableObject { }

    public class Inventory : ScriptableObject
    {
        public List<BodyPart> Slots; 
    }

    public enum EItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }
}
