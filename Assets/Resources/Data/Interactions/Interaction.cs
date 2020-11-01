using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Interactions
{
    public class InteractionParams
    {
        public CharacterData Interactor;
        public CharacterInteractive Interacted;
        public System.Action<InteractionResult> Callback;
    }

    public class InteractionResult {}

    public interface IInteraction
    {
        void Interact(InteractionParams parameters);
    }

    [CreateAssetMenu(menuName="Data/Interactions/Interaction")]
    public class Interaction : ScriptableObject, IInteraction
    {
        public ActionBase Action;

        public void Interact(InteractionParams parameters)
        {
            Action.Run(parameters);
        }
    }
}
