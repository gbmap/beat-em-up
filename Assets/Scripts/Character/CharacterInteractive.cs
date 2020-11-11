using Catacumba.Data.Interactions;
using UnityEngine;

namespace Catacumba.Entity
{
    public class CharacterInteractive : MonoBehaviour
    {
        public bool IsOneShot;
        public Interaction Interaction;
        public System.Action<InteractionResult> OnInteraction;

        bool _wasInteracted = false;

        public void Interact(CharacterData data, System.Action<InteractionResult> callback)
        {
            if (IsOneShot && _wasInteracted) return;
            callback += OnInteraction;
            if (IsOneShot) callback += (InteractionResult res) => _wasInteracted = true;
            Interaction.Interact(new InteractionParams
            {
                Interactor = data,
                Interacted = this,
                Callback = callback
            });
            callback -= OnInteraction;
        } 
    }
}