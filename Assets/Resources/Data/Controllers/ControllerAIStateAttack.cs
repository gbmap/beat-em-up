﻿using System;
using System.Linq;
using System.Text;
using Catacumba.Entity;
using UnityEngine;
using UnityEngine.AI;

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
        public float DistanceToAttack = 2.5f;
        public LayerMask SearchLayers;

        private CharacterData Target;
        private int TargetPriority = 10;

        private CharacterMovementBase movement;
        private CharacterCombat combat;
        private CharacterHealth health;

        private float attackTimer = 0f;

        private float checkTargetTimer = 0f;

        public override void OnCreate(ControllerComponent component)
        {
            movement = component.Data.Components.Movement;
            combat = component.Data.Components.Combat;
            health = component.Data.Components.Health;

            health.OnDamaged += Cb_OnDamaged;
        }

        public override void Destroy(ControllerComponent component)
        {
            if (health)
                health.OnDamaged -= Cb_OnDamaged;
        }

        private void Cb_OnDamaged(AttackResult obj)
        {
            /*
            if (!Target)
            {
                Target = obj.AttackerData;
                TargetPriority = InitialTargetPriority;
            }
            else */ if (Target == obj.AttackerData)
            {
                TargetPriority += 5;
            }
        }

        public override void OnEnter(ControllerComponent component)
        {
        }

        public override void OnExit(ControllerComponent component)
        {
        }

        public override void OnUpdate(ControllerComponent component, ref ControllerCharacterInput input)
        {
            if (Target)
            {
                Vector3 deltaToTarget = Target.transform.position - component.transform.position; 

                input.Direction = UpdateTargetPosition(component, deltaToTarget);
                input.LookDir = UpdateLookDirection(component, deltaToTarget);
                input.Attack = UpdateShouldAttack(component, out input.AttackType);

                UpdateNavMeshDestination(component);
            }
        }

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

        private Vector3 UpdateTargetPosition(ControllerComponent component, Vector3 deltaToTarget)
        {
            if (movement.NavAgentValid && movement.NavMeshAgent.remainingDistance > DistanceToAttack)
            {
                float velocityLimit = Mathf.Clamp01(movement.NavMeshAgent.remainingDistance);
                return movement.NavMeshAgent.desiredVelocity * velocityLimit;
            }
            return Vector3.zero; 
        }

        private void UpdateNavMeshDestination(ControllerComponent component)
        {
            float destinationToTargetDistance = Vector3.Distance(Target.transform.position, movement.NavMeshAgent.destination);
            if (destinationToTargetDistance > 1f)
                movement.SetDestination(Target.transform.position);
        }

        private Vector3 UpdateLookDirection(ControllerComponent component, Vector3 deltaToTarget)
        {
            return deltaToTarget.normalized;
        }

        private bool UpdateShouldAttack(ControllerComponent component, out EAttackType attackType)
        {
            attackType = EAttackType.Weak;
            float distanceToTarget = Vector3.Distance(Target.transform.position, component.transform.position);

            bool isCloseToTarget = distanceToTarget < DistanceToAttack;
            bool canAttack = attackTimer >= AttackDelay;

            if (isCloseToTarget && canAttack)
            {
                movement.StopForTime(1f);
                //combat?.RequestAttack(EAttackType.Weak);
                attackTimer = 0f;
                return true;
            }

            attackTimer += Time.deltaTime;
            return false;
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
    }
}