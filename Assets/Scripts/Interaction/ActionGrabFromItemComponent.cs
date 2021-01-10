using Catacumba.Data.Interactions;
using Catacumba.Data.Items;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Interactions
{
    [CreateAssetMenu(menuName="Data/Interactions/Action Grab Item (ItemComponent)", fileName="ActionGrabItemFromItemComponent")]
    public class ActionGrabFromItemComponent : ActionBase
    {
        public override Vector2Int Run(InteractionParams parameters)
        {
            Item item = parameters.Interacted.GetComponent<ItemComponent>().Item;
            bool result = parameters.Interactor.Stats.Inventory.Grab(item);
            if (result) return Vector2Int.down;
            return Vector2Int.zero;
        }
    }
}