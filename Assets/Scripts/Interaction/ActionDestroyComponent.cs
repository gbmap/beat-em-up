using UnityEngine;

namespace Catacumba.Data.Interactions
{
    [CreateAssetMenu(menuName="Data/Interactions/Action Destroy Componetn", fileName="ActionDestroyComponent")]
    public class ActionDestroyComponent : ActionBase
    {
        public string ComponentName;

        public override Vector2Int Run(InteractionParams parameters)
        {
            Behaviour component = parameters.Interacted.GetComponent(ComponentName) as Behaviour;
            if (component == null) return Vector2Int.down;
            Destroy(component);
            return Vector2Int.down;
        }
    }
}