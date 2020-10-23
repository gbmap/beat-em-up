using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Catacumba.Data;
using UnityEngine.AI;

namespace Tests
{
    public class EnemyTests
    {
        private GameObject CreateTestLevel()
        {
            var level = GameObject.CreatePrimitive(PrimitiveType.Cube);
            level.transform.localScale = new Vector3(50f, 1f, 50f);

            var navMeshSurface = level.AddComponent<NavMeshSurface>();
            navMeshSurface.BuildNavMesh();

            return level;
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator SpawnEnemyTest()
        {
            var level = CreateTestLevel();

            CharacterConfiguration cfg = EnemyDatabase.Get(0);
            EntitySpawner.InstantiateCharacter(new EntitySpawner.CharacterSpawnParams
            {
                Configuration = cfg
            });

            GameObject instance = EntitySpawner.InstantiateEmptyEntity("test", Vector3.zero, Quaternion.identity);
            instance.GetComponent<CharacterData>().CharacterCfg = cfg;

            yield return new WaitForSeconds(5f);

            Assert.IsTrue(true);
        }
    }
}
