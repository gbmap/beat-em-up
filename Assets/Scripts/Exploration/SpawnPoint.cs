using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Exploration
{
    [RequireComponent(typeof(BoxCollider))]
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private List<GameObject> enemies;
        
        private bool activated;

        private void Start()
        {
            ResetSpawn();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!activated)
            {
                StartCoroutine(ActivateSpawn());
            }
        }

        private void ResetSpawn()
        {
            activated = false;
        }

        private IEnumerator ActivateSpawn()
        {
            activated = true;
            
            foreach (var spawnPoint in spawnPoints)
            {
                // TODO: Spawn looking at the player
                var enemy = Instantiate(enemies[0], spawnPoint.position, Quaternion.identity);
                
                yield return new WaitForSeconds(0.5f);
            }
            
            yield break;
        }
    }
}