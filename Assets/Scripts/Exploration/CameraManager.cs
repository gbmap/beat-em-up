using System;
using System.Collections.Generic;
using Catacumba;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace Catacumba.Exploration
{
    public class CameraManager : SimpleSingleton<CameraManager>
    {
        [SerializeField] private Transform character;
        [SerializeField] private float cameraCheckTime = 1f;
        [SerializeField] private GameObject menuCamera;
        [SerializeField] private GameObject gameCamera;

        private float timer;
        private Camera mainCamera;
        private CinemachineImpulseSource _impulseSource;
        private GameObject currentActiveCamera;
        public GameObject CurrentCamera { get { return currentActiveCamera; } }

        public CinemachineVirtualCamera CurrentVirtualCamera
        {
            get { return CurrentCamera.GetComponent<CinemachineVirtualCamera>(); }
        }

        private PlayerMovementOrientation currentMovementOrientation;
        public MovementOrientation MovementOrientation
        {
            get
            {
                if (currentMovementOrientation) return currentMovementOrientation.CalculateMovementOrientation();
                if (CurrentCamera != null)
                {
                    return new MovementOrientation
                    {
                        forward = CurrentCamera.transform.forward,
                        right = CurrentCamera.transform.right
                    };
                }

                return new MovementOrientation
                {
                    forward = Camera.main.transform.forward,
                    right = Camera.main.transform.right
                };
            }
        }

        private List<CameraPathWaypoint> cameraPathWaypoints;

        public System.Action OnCameraChange;

        private void Awake()
        {
            mainCamera = Camera.main;
            //currentActiveCamera = firstCamera;

            GameObject initialCamera = menuCamera;
            if (CheckpointManager.Retry)
            {
                // if there's a checkpoint, get its camera.
                var tempCamera = CheckpointManager.CheckpointCamera;
                if (tempCamera != null)
                    gameCamera = tempCamera;

                initialCamera = gameCamera;
            }
            ChangeCamera(initialCamera);
            _impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        public void Initialize()
        {
            cameraPathWaypoints = new List<CameraPathWaypoint>();
            //currentActiveCamera.SetActive(true);
            ChangeCamera(gameCamera);
        }

        public void ChangeCamera(GameObject newCamera)
        {
            if (newCamera == null)
                return;

            if (currentActiveCamera)
            {
                currentActiveCamera.SetActive(false);
            }
            currentActiveCamera = newCamera;
            currentActiveCamera.SetActive(true);

            currentMovementOrientation = currentActiveCamera.GetComponent<PlayerMovementOrientation>();

            OnCameraChange?.Invoke();
        }

        public void Shake()
        {
            if (_impulseSource)
            {
                _impulseSource.GenerateImpulse();
            }
            else
            {
                Debug.LogWarning("No impulse source.");
            }
        }

        public void LightShake(int nHits = 1)
        {
            if (_impulseSource)
            {
                _impulseSource.GenerateImpulse(Vector3.one * 0.0125f * nHits);
            }
            else
            {
                Debug.LogWarning("No impulse source.");
            }
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