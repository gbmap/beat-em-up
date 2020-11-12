using Catacumba.Data.Items;
using Catacumba.Data.Items.Characteristics;
using UnityEngine;

namespace Catacumba.Entity
{
    public static class ItemSpawner
    {
        private const string ITEM_TEMPLATE_PATH = "Data/Items/ItemTemplate/ItemTemplate";
        private static ItemTemplate _template;
        private static ItemTemplate Template
        {
            get 
            { 
                return _template ?? (_template = Resources.Load<ItemTemplate>(ITEM_TEMPLATE_PATH)); 
            }
        }

        public static GameObject SpawnItem(Item item, 
                                           Vector3 position, 
                                           Quaternion rotation, 
                                           Transform parent = null)
        {
            if (item == null) return null;

            GameObject instance = new GameObject(item.Name);

            CharacteristicEquippable equippable = item.GetCharacteristic<CharacteristicEquippable>();

            // instance.AddComponent<In

            if (equippable) LoadInteractive(item, instance);
            else LoadInteractiveImmediate(item, instance);

            ItemComponent itemComponent = instance.AddComponent<ItemComponent>();
            itemComponent.Item = item;

            return instance;
        }

        private static void LoadInteractive(Item item, GameObject instance)
        {
            instance.AddComponent<InteractiveComponent>();
        }

        private static void LoadInteractiveImmediate(Item item, GameObject instance)
        {
            instance.AddComponent<InteractiveImmediateComponent>();
        }
    }
}