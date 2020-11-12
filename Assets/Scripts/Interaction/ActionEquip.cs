using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Interactions
{
    [CreateAssetMenu(menuName="Data/Interactions/Action Equip", fileName="ActionEquip")]
    public class ActionEquip : ActionBase
    {
        public override Vector2Int Run(InteractionParams parameters)
        {
            CharacterData data = parameters.Interactor;
            ItemComponent item = parameters.Interacted.GetComponent<ItemComponent>();
            data.Stats.Inventory.Equip(new Items.InventoryEquipParams
            {
                Item = item.Item,
                Callback = parameters.Callback
            });
            return Vector2Int.down;
        }
    }
}
