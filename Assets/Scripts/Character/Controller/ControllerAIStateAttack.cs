using System.Linq;
using System.Text;
using Catacumba.Data.Items;
using Catacumba.Data.Items.Characteristics;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Controllers
{
    [CreateAssetMenu(menuName="Data/Controllers/AI/States/Attack", fileName="AI_AttackState")]
    public class ControllerAIStateAttack : ControllerAIState
    {
        private int _currentPriority = 0;
        public override int CurrentPriority => _currentPriority;

        public int InitialTargetPriority = 10;
        public float AttackDelay = 3f;
        public float SearchRadius = 3f;
        public LayerMask SearchLayers;

        private CharacterData Target;
        private int TargetPriority = 10;

        private CharacterMovementBase _movement;
        private CharacterHealth health;

        private float attackTimer = 0f;

        private float checkTargetTimer = 0f;

        //////////////////////////////
        //    CONTROLLER AI STATE

        public override void OnCreate(ControllerComponent component)
        {
            health = component.Data.Components.Health;
            if (health)
                health.OnDamaged += Cb_OnDamaged;
        }

        public override void Destroy(ControllerComponent component)
        {
            if (health)
                health.OnDamaged -= Cb_OnDamaged;
        }

        public override void OnEnter(ControllerComponent component) { }
        public override void OnExit(ControllerComponent component) { }

        public override void OnUpdate(ControllerComponent component, ref ControllerCharacterInput input)
        {
            if (!Target) return;

            Vector3 deltaToTarget = Target.transform.position - component.transform.position; 

            CharacterData data = component.Data;
            CharacterMovementBase movement = component.Data.Components.Movement;

            Item weapon = component.Data.Stats.Inventory.GetWeapon();
            float distanceToAttack = weapon.GetCharacteristic<CharacteristicWeaponizable>().WeaponType.AttackStrategy.DistanceToAttack; 
            if (movement)
            {
                input.Direction = UpdateTargetPosition(movement, deltaToTarget, distanceToAttack);
                UpdateNavMeshDestination(data, movement, weapon);
            }
            input.LookDir = UpdateLookDirection(deltaToTarget);
            input.Attack = weapon && UpdateShouldAttack(movement, distanceToAttack, out input.AttackType);
        }

        public override int UpdatePriority(ControllerComponent component)
        {
            if (!Target)
                CheckNewTargets(component);

            if (Target)
                _currentPriority = TargetPriority;
            else
                _currentPriority = 0;

            return _currentPriority;
        }

        public override string GetDebugString(ControllerComponent component)
        {
            StringBuilder sb = new StringBuilder();
            if (Target)
                sb.AppendLine("Target: {Target.name}");

            return sb.ToString();
        }

        //////////////////////////////  
        //      CALLBACKS 
        private void Cb_OnDamaged(AttackResult obj)
        {
            if (Target == obj.AttackerData)
                TargetPriority += 5;
        }

        //////////////////////////////
        //     PRIVATE METHODS
        private void CheckNewTargets(ControllerComponent component)
        {
            checkTargetTimer += Time.deltaTime;
            if (checkTargetTimer >= 1f)
            {
                Collider[] targets = Physics.OverlapSphere(component.transform.position, SearchRadius, SearchLayers.value);
                if (targets.Length > 0)
                    Target = targets.Select(c => c.GetComponent<CharacterData>()).FirstOrDefault();
                
                checkTargetTimer = 0f;
            }
        }

        private Vector3 UpdateTargetPosition(CharacterMovementBase movement, Vector3 deltaToTarget, float distanceToAttack)
        {
            if (movement.NavAgentValid && movement.NavMeshAgent.remainingDistance >= distanceToAttack)
            {
                float velocityLimit = Mathf.Clamp01(movement.NavMeshAgent.remainingDistance);
                return movement.NavMeshAgent.desiredVelocity * velocityLimit;
            }
            return Vector3.zero; 
        }

        private void UpdateNavMeshDestination(CharacterData data, CharacterMovementBase movement, Item weapon)
        {
            float destinationToTargetDistance = Vector3.Distance(Target.transform.position, 
                                                                 movement.NavMeshAgent.destination);
            if (destinationToTargetDistance > 1f)
            {
                Vector3 desiredDestination = GetDesiredPositionFromItem(data, weapon, Target.transform.position);
                movement.SetDestination(Target.transform.position);
            }
        }

        private Vector3 GetDesiredPositionFromItem(CharacterData data, Item item, Vector3 position)
        {
            CharacteristicWeaponizable weapon = item.GetCharacteristic<CharacteristicWeaponizable>();
            if (weapon == null)
                return position;

            return weapon.WeaponType
                         .AttackStrategy
                         .ModulateDestinationPosition(item, data, Target);
        }

        private Vector3 UpdateLookDirection(Vector3 deltaToTarget)
        {
            return deltaToTarget.normalized;
        }

        private bool UpdateShouldAttack(CharacterMovementBase movement, float distanceToAttack, out EAttackType attackType)
        {
            attackType = EAttackType.Weak;
            float distanceToTarget = Vector3.Distance(Target.transform.position, movement.transform.position);

            bool isCloseToTarget = distanceToTarget <= distanceToAttack;
            bool canAttack = attackTimer >= AttackDelay;

            if (isCloseToTarget && canAttack)
            {
                if (movement)
                    movement.StopForTime(1f);
                attackTimer = 0f;
                return true;
            }

            attackTimer += Time.deltaTime;
            return false;
        }

        private float GetDistanceToAttack(Item weapon)
        {
            return weapon.GetCharacteristic<CharacteristicWeaponizable>()?.WeaponType?.AttackStrategy?.DistanceToAttack ?? 2.5f;
        }


    }
}