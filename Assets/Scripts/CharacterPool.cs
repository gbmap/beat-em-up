using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Catacumba.Data
{
    [System.Serializable]
    public class CharacterPoolItem
    {
        /// <summary>
        /// CharacterConfiguration ScriptableObject 
        /// </summary>
        public CharacterConfiguration Config;

        /// <summary> Probability of it being chosen. </summary>
        public int Weight;
    }

    [CreateAssetMenu(menuName="Data/Character/Character Pool", fileName="CharacterPool")]
    public class CharacterPool : ScriptableObject
    {
        public List<CharacterPoolItem> PoolItems;

        public CharacterPoolItem GetRandom()
        {
            float value = Mathf.Abs(Random.value - 0.5f) * 2f;
            int totalWeight = PoolItems.Sum(item => item.Weight);
            IEnumerable<CharacterPoolItem> sortedItems = PoolItems.OrderByDescending(item => item.Weight);

            float totalChance = 0f;
            foreach (CharacterPoolItem item in sortedItems)
            {
                totalChance += ((float)item.Weight) / totalWeight;
                if (value <= totalChance)
                    return item;
            }

            // Algo went kaput, u're dumb
            return sortedItems.First();
        }

    }
}