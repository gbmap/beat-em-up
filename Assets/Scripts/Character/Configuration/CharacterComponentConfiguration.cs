using System;
using System.Collections.Generic;
using System.Reflection;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data
{
    [CreateAssetMenu(menuName="Data/Character/Component Configuration", fileName="ComponentConfiguration")]
    public class CharacterComponentConfiguration : ScriptableObject
    {
        public List<string> ComponentFullNames = new List<string>();

        public void AddComponentsToObject(GameObject instance)
        {
            foreach (var component in ComponentFullNames)
            {
                Type componentType = Type.GetType(component);
                instance.AddComponent(componentType);
            }
        }

    }
}