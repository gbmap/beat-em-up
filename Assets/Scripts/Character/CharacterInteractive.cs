using Catacumba.Data.Interactions;
using UnityEngine;

namespace Catacumba.Entity
{
    public class CharacterInteractive : MonoBehaviour
    {
        public Interaction Interaction;
        public System.Action<InteractionResult> OnInteraction;

        public void Interact(CharacterData data, System.Action<InteractionResult> callback)
        {
            callback += OnInteraction;
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