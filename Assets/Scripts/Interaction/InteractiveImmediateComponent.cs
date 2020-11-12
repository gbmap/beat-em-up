using UnityEngine;

namespace Catacumba.Entity
{
    public class InteractiveImmediateComponent : InteractiveComponent
    {
        protected override void OnTriggerStay(Collider other) { }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            CharacterData data = other.GetComponent<CharacterData>();
            Interact(data);
        }
    }
}