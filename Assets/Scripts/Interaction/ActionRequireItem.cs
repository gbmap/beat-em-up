using System.Collections;
using System.Collections.Generic;
using Catacumba.Data.Items;
using UnityEngine;

namespace Catacumba.Data.Interactions
{
    [CreateAssetMenu(menuName="Data/Interactions/Action Require Item", fileName="ActionRequireItem")]
    public class ActionRequireItem : ActionBase
    {
        public Item item;

        public override Vector2Int Run(InteractionParams parameters)
        {
            bool hasItem = parameters.Interactor.Stats.Inventory.HasItem(item);
            if (hasItem) return Vector2Int.down;
            return Vector2Int.zero;
        }
    }
}