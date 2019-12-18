using System;
using UnityEngine;
using UnityEngine.Events;

namespace Catacumba.Exploration
{
    [RequireComponent(typeof(BoxCollider))]
    public class CameraChangeTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject virtualCamera;
        
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
            if (other.CompareTag("Player"))
                ChangeCamera();
        }

        private void ChangeCamera()
        {
            OnCameraChange(virtualCamera);
        }
    }
}