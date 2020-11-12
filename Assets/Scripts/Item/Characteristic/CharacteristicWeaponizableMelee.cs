using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items.Characteristics
{
    [CreateAssetMenu(menuName = "Data/Item Characteristic/Weaponizable/Melee", fileName = "WeaponizableMelee")]
    public class CharacteristicWeaponizableMelee : CharacteristicWeaponizable
    {
        public AttackColliderSize AttackColliderSize;

        public override AttackResult[] Attack(CharacterData character, Transform origin,  EAttackType attackType)
        {
            Collider[] colliders = CollectColliders(character, origin, attackType);
            if (colliders.Length == 0) return null;

            AttackResult[] attackResults = new AttackResult[colliders.Length];

            int hits = 0;
            foreach (var c in colliders)
            {
                if (c.gameObject == character.gameObject) continue;

                CharacterData defender = c.GetComponent<CharacterData>();

                AttackRequest request = new AttackRequest(character, defender, attackType);
                AttackResult attackData = CombatManager.AttackCharacter(request);
                if (attackData == null) continue;

                attackResults[hits] = attackData;
                hits++;
            }

            return attackResults;
        }

        protected Collider[] CollectColliders(CharacterData character, Transform origin, EAttackType attackType)
        {
            Collider[] colliders = Physics.OverlapBox(
                GetColliderPosition(origin), 
                GetColliderSize(character, attackType), 
                GetColliderRotation(origin),
                character.Components.Combat.TargetLayer.value
            );

            return colliders;
        }

        protected Vector3 GetColliderPosition(Transform origin)
        {
            return origin.transform.position + (origin.transform.forward*1.25f + Vector3.up);
        }

        private Vector3 GetColliderSize(CharacterData character, EAttackType attackType)
        {
            Vector3 attackColliderSize = (Vector3.one * 0.65f + Vector3.right * 0.65f); 
            attackColliderSize.x *= AttackColliderSize.Size.x;
            attackColliderSize.y *= AttackColliderSize.Size.y;
            attackColliderSize.z *= AttackColliderSize.Size.z;

            attackColliderSize *= attackType == EAttackType.Weak ? 1.0f : 1.5f;
            return (attackColliderSize + Vector3.one)/2f;
        }

        private Quaternion GetColliderRotation(Transform origin)
        {
            return origin.transform.rotation;
        }
        
        public override void DebugDraw(CharacterData data, EAttackType type)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(
                GetColliderPosition(data.transform),
                GetColliderRotation(data.transform),
                GetColliderSize(data, type)
            );
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}