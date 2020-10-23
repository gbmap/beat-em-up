using UnityEngine;

namespace Catacumba.Data
{
    [CreateAssetMenu()]
    public class CharacterViewConfiguration : ScriptableObject
    {
        public GameObject[] Models;

        public GameObject GetRandomModel()
        {
            return Models[Random.Range(0, Models.Length)];
        }
    }
}