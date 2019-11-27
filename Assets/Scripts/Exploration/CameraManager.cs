using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Catacumba.Exploration
{
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private Transform character;
        [SerializeField] private float cameraCheckTime = 1f;
        [SerializeField] private List<GameObject> virtualCameras;
        
        private float timer;
        private Camera mainCamera;
        private GameObject currentActiveCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
            currentActiveCamera = virtualCameras[0];
        }
        
        private void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                CheckForNearestCamera();
                timer = cameraCheckTime;
            }
        }

        private void CheckForNearestCamera()
        {
            var closestCamera = virtualCameras[0];
            var closestDistance = 200f;
            
            foreach (var virtualCamera in virtualCameras)
            {
                var distance = Vector3.Distance(character.position, virtualCamera.transform.position);

                if (distance <= closestDistance)
                {
                    closestDistance = distance;
                    closestCamera = virtualCamera;
                }
            }

            // If the player moves closer to a new camera, change the camera (and path)
            if (!closestCamera.activeSelf)
            {
                closestCamera.SetActive(true);
                currentActiveCamera.SetActive(false);
                currentActiveCamera = closestCamera;
            }
        }
    }
}