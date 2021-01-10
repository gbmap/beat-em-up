
using Catacumba.Data.Interactions;
using UnityEngine;

namespace Catacumba.Entity
{
    public abstract class InteractiveBaseComponent : MonoBehaviour
    {
        public Interaction Interaction;
        public System.Action<InteractionResult> OnInteraction;

        public virtual void Interact(CharacterData data, System.Action<InteractionResult> callback = null)
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