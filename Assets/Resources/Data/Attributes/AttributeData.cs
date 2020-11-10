using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Data
{
    [CreateAssetMenu(menuName="Data/Stat Attribute", fileName="Attribute")]
    public class AttributeData : ScriptableObject
    {
        public string Identifier = "XXX";
        public string Description = "";
    }

    public class AttributeValue<T> 
    {
        public AttributeData Attribute;
        public T Value;
    }

    [System.Serializable]
    public class AttributeValueI : AttributeValue<int> { }
}