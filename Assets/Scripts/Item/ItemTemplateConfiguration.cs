using System.Collections;
using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items
{
    [CreateAssetMenu(menuName="Data/Item/Item Template Configuration", fileName="ItemTemplateConfiguration")]
    public class ItemTemplateConfiguration : ScriptableObject
    {
        private const string PATH_DEFAULT = "Data/Items/ItemTemplate/ItemTemplate";

        private static ItemTemplateConfiguration _default;
        public static ItemTemplateConfiguration Default
        {
            get { return _default ?? (_default = Resources.Load<ItemTemplateConfiguration>(PATH_DEFAULT)); }
        }

        public RuntimeAnimatorController AnimatorController;
        public GameObject Highlight;
    }
}