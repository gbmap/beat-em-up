using System.Collections;
using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items
{
    [CreateAssetMenu(menuName="Data/Item/Item Template", fileName="ItemTemplate")]
    public class ItemTemplate : ScriptableObject
    {
        private const string PATH_DEFAULT = "Data/Items/ItemTemplate/ItemTemplate";
        public GameObject Prefab;

        private static ItemTemplate _default;
        private static ItemTemplate Default
        {
            get { return _default ?? (_default = Resources.Load<ItemTemplate>(PATH_DEFAULT)); }
        }

        public static GameObject Create(Item item, Vector3 worldPosition)
        {
            GameObject obj = Instantiate(Default.Prefab, worldPosition, Quaternion.identity);
            obj.GetComponent<ItemComponent>().Item = item;
            return obj;
        }
    }
}