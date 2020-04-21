using System;
using UnityEngine;
using UnityEngine.Events;

namespace Catacumba.Exploration
{
    [RequireComponent(typeof(BoxCollider))]
    public class CameraChangeTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject virtualCamera;
        [SerializeField] private bool revertOnExit = false;

        private GameObject oldCamera;

        private UnityAction<GameObject> OnCameraChange;
        
        private void OnEnable()
        {
            OnCameraChange += CameraManager.Instance.ChangeCamera;
        }

        private void OnDisable()
        {
            OnCameraChange -= CameraManager.Instance.ChangeCamera;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            oldCamera = CameraManager.Instance.CurrentCamera;
            ChangeCamera(virtualCamera);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") || !revertOnExit) return;

            var newCamera = oldCamera;
            oldCamera = CameraManager.Instance.CurrentCamera;
            ChangeCamera(newCamera);
        }

        private void ChangeCamera(GameObject camera)
        {
            OnCameraChange(camera);
        }
    }
}