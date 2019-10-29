using System;
using UnityEngine;

namespace Catacumba.Exploration
{
    public class AreaTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private Area destinationArea;
        
        private void OnTriggerEnter(Collider other)
        {
            // If player enters trigger
            if (destinationArea != null && other.CompareTag("Player"))
            {
                ScenarioManager.Instance.TransitionToArea(destinationArea, other.gameObject);
            }
        }
    }
}