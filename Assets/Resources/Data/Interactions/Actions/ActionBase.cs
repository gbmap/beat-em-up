using UnityEngine;

namespace Catacumba.Data.Interactions
{
    public interface IAction
    {
        Vector2Int Run(InteractionParams parameters);
    }

    public abstract class ActionBase : ScriptableObject, IAction
    {
        public abstract Vector2Int Run(InteractionParams parameters);
    }
}