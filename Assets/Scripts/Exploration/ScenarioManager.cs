using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Catacumba.Exploration
{
    public class ScenarioManager : Singleton<ScenarioManager>
    {
        [SerializeField] private GameObject fadeCamera;
        [SerializeField] private Area firstArea;
        
        private List<Area> areas;
        private Area currentActiveArea;
        private GameObject currentCharacter;

        private void Awake()
        {
            areas = new List<Area>();

            // Get child areas
            foreach (var area in GetComponentsInChildren<Area>())
            {
                areas.Add(area);
            }
        }

        private void Start()
        {
            InitializeFirstArea();
        }

        private void InitializeFirstArea()
        {
            currentActiveArea = firstArea;
            
            // TODO: Do fade in and move player to new area
        }

        public void TransitionToArea(Area area, GameObject character)
        {
            StartCoroutine(FadeTransition(area, character));
        }

        private IEnumerator FadeTransition(Area area, GameObject character)
        {
            // Enable fade camera
            fadeCamera.SetActive(true);
            
            yield return new WaitForSeconds(1f);
            
            // Hide current area
            currentActiveArea?.gameObject.SetActive(false);
            
            currentActiveArea = area;
            currentCharacter = character;
            
            // Move player to new area
            MovePlayerToNewArea(currentCharacter);
            
            // Show new area
            currentActiveArea.gameObject.SetActive(true);
            
            // Disable fade camera
            fadeCamera.SetActive(false);
        }

        private void MovePlayerToNewArea(GameObject character)
        {
            // Move character to new are
            character.transform.position = currentActiveArea.PlayerSpawnPoint.position;
        }
    }
}