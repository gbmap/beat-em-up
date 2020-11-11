using System.Collections;
using System.Collections.Generic;
using Catacumba.Data.Items.Weapons.Strategies;
using UnityEngine;

namespace Catacumba.Data.Items
{
    [CreateAssetMenu(menuName="Data/Item/Weapon Type", fileName="Weapon Type")]
    public class WeaponType : ScriptableObject
    {
        public RuntimeAnimatorController animatorController;
        public WeaponAttackStrategy AttackStrategy;
    }
}