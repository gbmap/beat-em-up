using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Interactions
{
    [CreateAssetMenu(menuName="Data/Interactions/Action Animation", fileName="ActionAnimation")]
    public class ActionAnimationTrigger : ActionBase
    {
        public string Trigger;

        public override Vector2Int Run(InteractionParams parameters)
        {
            Animator animator = parameters.Interacted.GetComponent<Animator>();
            animator.SetTrigger(Trigger);
            return Vector2Int.down;
        }
    }
}
