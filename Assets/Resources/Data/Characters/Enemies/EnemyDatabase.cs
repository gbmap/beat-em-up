using System.Collections.Generic;
using UnityEngine;

using Catacumba.Entity;

namespace Catacumba.Data
{
    public static class EntitySpawner
    {
        public const string CHARACTER_PREFAB_PATH = "";
        private static GameObject characterPrefab;

        static EntitySpawner() 
        {
            characterPrefab = Resources.Load<GameObject>(CHARACTER_PREFAB_PATH);
        } 

        public static GameObject InstantiateEmptyEntity(string name, 
                                                        Vector3 position,
                                                        Quaternion rotation,
                                                        Transform parent = null)
        {
            return GameObject.Instantiate(characterPrefab, position, rotation, parent);
        }

        public struct CharacterSpawnParams
        {
            public string Name;
            public Vector3 Position;
            public Quaternion Rotation;
            public Transform Parent;
            public GameObject Prefab;
            public CharacterConfiguration Configuration;
            public bool IsDamageable;
            public bool IsAI;
        }

        public static CharacterData InstantiateCharacter(CharacterSpawnParams p)
        {
            // TODO: separate method into InstantiateObject
            var instance = GameObject.Instantiate(characterPrefab, p.Position, p.Rotation, p.Parent);
            instance.name = p.Name;
            // endtodo

            CharacterData characterData = instance.AddComponent<CharacterData>();
            //characterData.CharacterCfg = p.Configuration;

            return characterData;
        }
    }

    [CreateAssetMenu()]
    public class EnemyDatabase : ScriptableObject
    {
        public const string FILENAME = "Data/Characters/Enemies/EnemyDatabase.asset";
        public List<CharacterConfiguration> Enemies; 

        private static EnemyDatabase _instance;
        private static EnemyDatabase Instance
        {
            get 
            { 
                return _instance ?? (_instance = Resources.Load<EnemyDatabase>(FILENAME));
            }
        }

        public static CharacterConfiguration Get(int index) 
        {
            return Instance.Enemies[index];
        }
    }
}