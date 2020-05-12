using System;
using UnityEngine;
using UnityEngine.Events;

namespace Catacumba.Exploration
{
    [RequireComponent(typeof(Collider))]
    public class CameraChangeTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject virtualCamera;
        [SerializeField] private bool revertOnExit = false;

        private GameObject oldCamera;

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
            CameraManager.Instance.ChangeCamera(camera);
        }
    }
}