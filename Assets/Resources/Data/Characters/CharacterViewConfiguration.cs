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

        public void Configure(Entity.CharacterData character, int modelIndex = -1)
        {
            GameObject instance = character.gameObject;

            RemoveExistingModel(instance);
            GameObject modelPrefab = SelectModel(modelIndex);
            GameObject modelInstance = AddModelToInstance(instance, modelPrefab);
        }


        private static void RemoveExistingModel(GameObject instance)
        {
            for (int i = 0; i < instance.transform.childCount; i++)
            {
                var child = instance.transform.GetChild(i);
                if (child.name.Contains("Character_") ||
                    child.name.Equals("Root"))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private GameObject SelectModel(int modelIndex)
        {
            if (modelIndex == -1)
                return GetRandomModel();
            return Models[Mathf.Clamp(modelIndex, 0, Models.Length)];
        }

        private GameObject AddModelToInstance(GameObject instance, GameObject model)
        {
            GameObject modelInstance = Instantiate(
                model, 
                Vector3.zero, 
                Quaternion.identity, 
                instance.transform
            );

            return modelInstance;
        }

    }
}