using Catacumba.Data.Interactions;
using UnityEngine;

namespace Catacumba.Entity
{
    public class CharacterInteractive : MonoBehaviour
    {
        public Interaction Interaction;

        public void Interact(CharacterData data, System.Action<InteractionResult> callback)
        {
            Interaction.Interact(new InteractionParams
            {
                Interactor = data,
                Interacted = this,
                Callback = callback
            });
        }
    }
}