using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Data.Items
{
    [CreateAssetMenu(menuName="Data/Item/Weapon Type", fileName="Weapon Type")]
    public class WeaponType : ScriptableObject
    {
        public RuntimeAnimatorController animatorController;
    }
}