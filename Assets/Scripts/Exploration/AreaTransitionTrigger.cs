using System;
using UnityEngine;

namespace Catacumba.Exploration
{
    public class AreaTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private int _destinationMapIndex = -1;
        
        private void OnTriggerEnter(Collider other)
        {
            // If player enters trigger
            if (_destinationMapIndex != -1 && other.CompareTag("Player"))
            {
                TransitionToMap(_destinationMapIndex);
            }
        }

        private void TransitionToMap(int mapIndex)
        {
            ScenarioManager.Instance.TransitionToMap(mapIndex);
        }
    }
}