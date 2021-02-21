using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items.Characteristics
{
    [CreateAssetMenu(menuName = "Data/Item/Characteristic/Weaponizable/Melee", fileName = "WeaponizableMelee")]
    public class CharacteristicWeaponizableMelee : CharacteristicWeaponizable
    {
        public AttackCollider AttackCollider;
        public bool IgnoreFacingDirection = false;

        public override AttackResult[] Attack(
            CharacterData character, 
            Transform origin,  
            EAttackType attackType
        ) {
            Collider[] colliders = CollectColliders(character, origin, attackType);
            if (colliders.Length == 0) return null;

            AttackResult[] attackResults = new AttackResult[colliders.Length];

            int hits = 0;
            foreach (var c in colliders)
            {
                if (c.gameObject == character.gameObject) continue;

                CharacterData defender = c.GetComponent<CharacterData>();
                if (defender) {
                    AttackCharacter(character, defender, attackType, ref attackResults, ref hits);
                    continue;
                }

                ProjectileComponent projectile = c.GetComponent<ProjectileComponent>();
                if (projectile) {
                    if (projectile.Caster == character)
                        continue;

                    AttackProjectile(character, projectile);
                    continue;
                }
            }

            return attackResults;
        }

        protected void AttackCharacter(
            CharacterData attacker, 
            CharacterData defender, 
            EAttackType attackType, 
            ref AttackResult[] attackResults, 
            ref int hits
        ) {
            AttackRequest request = new AttackRequest(attacker, defender, attackType, IgnoreFacingDirection);
            AttackResult attackData = CombatManager.AttackCharacter(request);
            if (attackData == null) return;

            attackResults[hits] = attackData;
            hits++;
        }

        private void AttackProjectile(CharacterData attacker, ProjectileComponent projectile) {
            projectile.Reflect(attacker);
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
            Vector3 up  = origin.transform.up * (1 + AttackCollider.OrientationOffset.y);
            Vector3 fwd = origin.transform.forward * AttackCollider.OrientationOffset.z;
            //fwd = Vector3.zero;
            return origin.transform.position + fwd + up;
        }

        private Vector3 GetColliderSize(CharacterData character, EAttackType attackType)
        {
            Vector3 attackColliderSize = (Vector3.one * 0.65f + Vector3.right * 0.65f); 
            attackColliderSize.x *= AttackCollider.Size.x;
            attackColliderSize.y *= AttackCollider.Size.y;
            attackColliderSize.z *= AttackCollider.Size.z;

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
                GetColliderSize(data, type)*2f
            );
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}