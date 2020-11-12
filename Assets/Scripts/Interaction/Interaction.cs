using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Interactions
{
    public class InteractionParams
    {
        public CharacterData Interactor;
        public InteractiveBaseComponent Interacted;
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
        public ActionBase[] Actions;

        public void Interact(InteractionParams parameters)
        {
            foreach (ActionBase action in Actions)
            {
                Vector2Int direction = action.Run(parameters);
                if (direction != Vector2Int.down) break;
            }
        }
    }
}
