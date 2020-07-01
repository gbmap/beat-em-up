using Frictionless;
using UnityEngine;

namespace Catacumba {
    public class SpawnParticleSystemOnDestroy : MonoBehaviour
    {
        public GameObject Particles;

        private bool isQuitting;

        private void Start()
        {
            if (!Particles)
                Destroy(this);

            ServiceFactory.Instance.Resolve<MessageRouter>()?.AddHandler<StateManager.MsgOnSceneChangeRequest>(OnSceneChangeRequest);
        }

        private void OnSceneChangeRequest(StateManager.MsgOnSceneChangeRequest msg)
        {
            isQuitting = true;
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying || !Particles || isQuitting) return;
            var particle = Instantiate(Particles, transform.position, Quaternion.identity);
        }
    }
}