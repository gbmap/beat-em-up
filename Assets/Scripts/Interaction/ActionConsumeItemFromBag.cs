using Catacumba.Data.Items;
using Catacumba.Data.Items.Characteristics;
using UnityEngine;

namespace Catacumba.Data.Interactions
{
    [CreateAssetMenu(menuName="Data/Interactions/Action Consume Item From Bag", fileName="ActionConsumeItemFromBag")]
    public class ActionConsumeItemFromBag : ActionBase
    {
        public Item item;

        public override Vector2Int Run(InteractionParams parameters)
        {
            Inventory inventory = parameters.Interactor.Stats.Inventory;
            bool result = inventory.Bag.DropAmount(item, 1);
            if (result) return Vector2Int.down;
            return Vector2Int.zero;
        }
    }
}