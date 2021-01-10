using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Interactions
{
    [CreateAssetMenu(menuName="Data/Interactions/Action Equip Item (ItemComponent)", fileName="ActionEquipFromItemComponent")]
    public class ActionEquipFromItemComponent : ActionBase
    {
        public override Vector2Int Run(InteractionParams parameters)
        {
            CharacterData data = parameters.Interactor;
            ItemComponent item = parameters.Interacted.GetComponent<ItemComponent>();
            var result = data.Stats.Inventory.Equip(new Items.InventoryEquipParams
            {
                Item = item.Item
            });

            if (result.Result != Items.InventoryEquipResult.EEquipResult.Success) return Vector2Int.zero;
            return Vector2Int.down;
        }
    }
}
