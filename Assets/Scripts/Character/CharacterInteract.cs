using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Interactions;
using Catacumba.Data.Items;
using Catacumba.Data.Items.Characteristics;
using UnityEngine;

namespace Catacumba.Entity
{
    public class CharacterInteract : CharacterComponentBase
    {
        public float Radius = 2f;
        public LayerMask TargetLayer;

        public void Interact()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, Radius, TargetLayer);
            if (colliders == null || colliders.Length == 0)
                return;

            InteractiveBaseComponent interactive = colliders
                .OrderBy(c => Vector3.Distance(c.transform.position, transform.position))
                .Select(c => c.GetComponentInChildren<InteractiveBaseComponent>())?
                .First();

            if (!interactive)
                return;

            interactive.Interact(data, OnInteractionEnded);
        }

        private void OnInteractionEnded(InteractionResult result) { }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}