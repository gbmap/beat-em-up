using UnityEngine;

namespace Catacumba.Data.Interactions
{
    [CreateAssetMenu(menuName="Data/Interactions/Action Set Component Enabled", fileName="ActionSetComponentEnabled")]
    public class ActionSetComponentEnabled : ActionBase
    {
        public string ComponentName;
        public bool Value;

        public override Vector2Int Run(InteractionParams parameters)
        {
            Behaviour component = parameters.Interacted.GetComponent(ComponentName) as Behaviour;
            if (component == null) return Vector2Int.down;
            component.enabled = Value;
            return Vector2Int.down;
        }
    }
}