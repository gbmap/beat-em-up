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

    public class InteractionResult 
    {
        public enum ECode
        {
            Success,
            Failure
        }

        public ECode Code { get; set; }
    }

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
            InteractionResult.ECode resultCode = InteractionResult.ECode.Success;
            foreach (ActionBase action in Actions)
            {
                Vector2Int direction = action.Run(parameters);
                if (direction != Vector2Int.down)
                {
                    resultCode = InteractionResult.ECode.Failure;
                    break;
                }
            }

            parameters.Callback?.Invoke(new InteractionResult { Code = resultCode });
        }
    }
}
