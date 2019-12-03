using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace Catacumba.Exploration
{
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private Transform character;
        [SerializeField] private float cameraCheckTime = 1f;
        [SerializeField] private GameObject firstCamera;
        
        private float timer;
        private Camera mainCamera;
        private GameObject currentActiveCamera;
        private List<CameraPathWaypoint> cameraPathWaypoints;

        private void Awake()
        {
            mainCamera = Camera.main;
            currentActiveCamera = firstCamera;
//            InitializeCameras();
        }

        private void InitializeCameras()
        {
            cameraPathWaypoints = new List<CameraPathWaypoint>();
            
            foreach (var vcam in GetComponentsInChildren<CinemachineVirtualCamera>(true))
            {
                var path = vcam.transform.parent.GetComponentInChildren<CinemachinePath>();

                foreach (var waypoint in path.m_Waypoints)
                {
                    cameraPathWaypoints.Add(new CameraPathWaypoint(
                        vcam.gameObject,
                        path.transform.TransformPoint(waypoint.position)));
                }
            }

            currentActiveCamera = firstCamera;
        }

        private void Update()
        {
//            timer -= Time.deltaTime;
//
//            if (timer <= 0f)
//            {
//                CheckForNearestCamera();
//                timer = cameraCheckTime;
//            }
        }

        public void ChangeCamera(GameObject newCamera)
        {
            currentActiveCamera.SetActive(false);
            currentActiveCamera = newCamera;
            currentActiveCamera.SetActive(true);
        }

        private void CheckForNearestCamera()
        {
            var closestWaypoint = cameraPathWaypoints[0];
            var closestDistance = 200f;
            
            foreach (var cpWaypoiint in cameraPathWaypoints)
            {
                var distance = Vector3.Distance(character.position, cpWaypoiint.waypointPosition);

                if (distance <= closestDistance)
                {
                    closestDistance = distance;
                    closestWaypoint = cpWaypoiint;
                }
            }

            // If the player moves closer to a new camera, change the camera (and path)
            if (!closestWaypoint.vcam.activeSelf)
            {
                closestWaypoint.vcam.SetActive(true);
                currentActiveCamera.SetActive(false);
                currentActiveCamera = closestWaypoint.vcam;
            }
        }

        private struct CameraPathWaypoint
        {
            public Vector3 waypointPosition;
            public GameObject vcam;

            public CameraPathWaypoint(GameObject vcam, Vector3 waypointPosition)
            {
                this.vcam = vcam;
                this.waypointPosition = waypointPosition;
            }
        }
    }
}