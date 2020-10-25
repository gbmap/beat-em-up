using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Catacumba
{
    public class Spawner : MonoBehaviour
    {
        public GameObject[] ObjectPool;
        
        [Range(0f, 1f)]
        public float Probability;

        [Range(1, 10)]
        public int Instances = 1;

        [Range(0, 10)]
        public int RangeOfInstances = 0;

        public float RangeOfSpawn = 1f;

        public bool OneShot = true;

        private List<Spawner> childSpawners;
        private List<GameObject> spawnInstances;

        private void Awake()
        {
            childSpawners = new List<Spawner>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var s = transform.GetChild(i).GetComponent<Spawner>();
                if (s) childSpawners.Add(s);
            }

            spawnInstances = new List<GameObject>();
        }

        public void Spawn()
        {
            if (spawnInstances.Any(g => g != null)) return;

            spawnInstances.Clear();

            int count = Instances - Random.Range(0, RangeOfInstances);
            for (int i = 0; i < Instances; i++)
            {
                if (Random.value < Probability)
                {
                    Vector2 o2d = Random.insideUnitCircle * RangeOfSpawn;
                    Vector3 offset = new Vector3(o2d.x, 0f, o2d.y); 

                    var instance = Instantiate(ObjectPool[Random.Range(0, ObjectPool.Length)], transform.position + offset, Quaternion.identity);
                    spawnInstances.Add(instance);
                }
            }

            foreach (var s in childSpawners)
            {
                s.Spawn();
            }

            if (OneShot)
            {
                Destroy(this);
            }
        }

    }
}