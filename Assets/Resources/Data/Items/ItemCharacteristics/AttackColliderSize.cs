using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Data.Items.Characteristics
{
    [CreateAssetMenu(menuName = "Data/Item Characteristic/Weaponizable/Melee Collider Size", fileName="Collider Size")]
    public class AttackColliderSize : ScriptableObject
    {
        public Vector3 Size;
    }
}