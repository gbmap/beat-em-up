using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Catacumba.Exploration
{
    public class ScenarioManager : Singleton<ScenarioManager>
    {
        [SerializeField] GameObject fadeCamera;
        [SerializeField] Area firstArea;
        [SerializeField] GameObject currentCharacter;
        
        List<Area> areas;
        Area currentActiveArea;
        Transform transform;
        
        public void TransitionToArea(Area area, GameObject character)
        {
            StartCoroutine(FadeTransition(area, character));
        }
        
        void Awake()
        {
            areas = new List<Area>();

            // Get child areas
            foreach (var area in GetComponentsInChildren<Area>())
            {
                areas.Add(area);
            }

            transform = GetComponent<Transform>();
        }

        void Start()
        {
            StartCoroutine(FadeTransition(firstArea, currentCharacter));
        }

        IEnumerator FadeTransition(Area area, GameObject character)
        {
            // Enable fade camera
            fadeCamera.transform.position = character.transform.position;
            fadeCamera.transform.LookAt(character.transform);
            fadeCamera.SetActive(true);
            
            yield return new WaitForSeconds(1f);
            
            // Hide current area
            currentActiveArea?.gameObject.SetActive(false);
            
            currentActiveArea = area;
            currentCharacter = character;

            // Show new area
            currentActiveArea.gameObject.SetActive(true);

            // Move player to new area
            MovePlayerToNewArea(currentCharacter);
            
            fadeCamera.transform.position = currentActiveArea.VirtualCamera.gameObject.transform.position;
            fadeCamera.transform.rotation = currentActiveArea.VirtualCamera.gameObject.transform.rotation;

            // Disable fade camera
            currentActiveArea.VirtualCamera.gameObject.SetActive(true);
            fadeCamera.SetActive(false);
        }

        void MovePlayerToNewArea(GameObject character)
        {
            // Move character to new are
            var agent = character.GetComponent<NavMeshAgent>();

            agent.isStopped = true;
            agent.enabled = false;
            
            character.transform.position = currentActiveArea.PlayerSpawnPoint.position;
            currentActiveArea.SetCharacter(character);
            
            agent.enabled = true;
            agent.isStopped = false;
        }
    }
}