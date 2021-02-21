using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Catacumba.Data
{
    [CreateAssetMenu(menuName="Data/Stat Attribute", fileName="Attribute")]
    public class AttributeData : ScriptableObject
    {
        public string Identifier = "XXX";
        public string Description = "";
    }

    public abstract class AttributeValue<T> 
    {
        public AttributeData Definition;
        public T Value;
    }

    [System.Serializable]
    public class AttributeValueI : AttributeValue<int> { }

    public static class AttributeHelper
    {
        public static int GetSum(List<AttributeValueI> Attributes)
        {
            return Attributes.Sum(a => a.Value);
        }

    }
}