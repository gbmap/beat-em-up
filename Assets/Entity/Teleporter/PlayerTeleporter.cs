using Catacumba.Exploration;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba
{
    public class OnTeleportStarted : UnityEngine.Events.UnityEvent { }
    public class OnTeleportEnded : UnityEngine.Events.UnityEvent { }

    public class PlayerTeleporter : MonoBehaviour
    {
        public Transform destination;
        public float Radius = 2f;

        public CameraChangeTrigger Trigger;

        public OnTeleportStarted OnTeleportStarted;
        public OnTeleportEnded OnTeleportEnded;

        public void Teleport()
        {
            var players = FindObjectsOfType<CharacterPlayerInput>();
            foreach (var p in players)
            {
                Vector2 p2d = Random.insideUnitCircle;
                Vector3 tp = destination.position + new Vector3(p2d.x, 0f, p2d.y) * Radius;
                //p.GetComponent<NavMeshAgent>().Move(tp - transform.position);
                p.GetComponent<NavMeshAgent>().Warp(tp);
                //p.transform.position = ;
            }
        }

        public void TeleportWithFade()
        {
            StartCoroutine(TeleportCoroutine());
        }

        private IEnumerator TeleportCoroutine()
        {
            OnTeleportStarted?.Invoke();

            UIFade f = FindObjectOfType<UIFade>();
            f.Fade(true);

            yield return new WaitForSeconds(2f);

            // trigger
            Teleport();
            if (Trigger) Trigger.Trigger();

            yield return new WaitForSeconds(2f);
            
            f.Fade(false);

            yield return new WaitForSeconds(1f);

            OnTeleportEnded?.Invoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}