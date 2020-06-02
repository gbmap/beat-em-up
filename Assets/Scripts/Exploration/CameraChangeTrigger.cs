using System;
using UnityEngine;
using UnityEngine.Events;

namespace Catacumba.Exploration
{
    [System.Serializable]
    public class OnCameraChangeEvent : UnityEvent { }

    [RequireComponent(typeof(Collider))]
    public class CameraChangeTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject virtualCamera;
        [SerializeField] private bool revertOnExit = false;

        private GameObject oldCamera;

        public OnCameraChangeEvent OnCameraChange;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            Trigger();
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
            OnCameraChange.Invoke();
        }

        public void Trigger()
        {
            oldCamera = CameraManager.Instance.CurrentCamera;
            ChangeCamera(virtualCamera);
        }
    }
}