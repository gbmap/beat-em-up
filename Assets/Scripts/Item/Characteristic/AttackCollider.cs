using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Data.Items.Characteristics
{
    [CreateAssetMenu(menuName = "Data/Item Characteristic/Weaponizable/Melee Collider Size", fileName="Collider Size")]
    public class AttackCollider : ScriptableObject
    {
        public Vector3 OrientationOffset = new Vector3(0f, 0f, 1.25f);
        public Vector3 Size;
    }
}