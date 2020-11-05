using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Data.Controllers
{
	[CreateAssetMenu(menuName="Data/Controllers/AI/States/Wander", fileName="AI_WanderState")]
    public abstract class ControllerAIStateWander : ControllerAIState
    {
		public float TimeStopped = 3f;
		public float WanderRadius = 4f;
		public bool RelativeToStartingPoint = true;

		private Vector3 _startingPoint = Vector3.zero;
		private float _movementTimer = 0f;

        private int currentPriority = 0;
        public override int CurrentPriority { get { return currentPriority;  } }

		CharacterMovementWalk movement;

        public override int UpdatePriority(ControllerComponent component)
        {
			return currentPriority = 1;
        }

        public override void OnCreate(ControllerComponent component)
		{
			base.OnCreate(component);
			movement = component.data.Components.Movement;
			if (!movement)
				Debug.LogError("No movement component in object.");

			_startingPoint = movement.transform.position;
		}

        public override void OnEnter(ControllerComponent component)
		{
			base.OnEnter(component);
		}

        public override void OnUpdate(ControllerComponent component)
		{
			base.OnUpdate(component);

			if (movement.NavMeshAgent.remaningDistance > 0.1f)
				return;

			_movementTimer += Time.deltaTime;
			if (_movementTimer >= TimeStopped)		
			{
				ChangePosition(component);
				_movementTimer = 0f;
			}
		}

		public void ChangePosition(CharacterMovementWalk movement)
		{
			Vector3 newPosition = Random.insideUnitSphere * WanderRadius;
			newPosition.y = 0f;

			movement.SetDestination(newPosition);
		}

        public override void OnExit(ControllerComponent component)
		{
			base.OnExit(component);
		}
        public override void OnDestroy(ControllerComponent component)
		{
			base.OnDestroy(component);
		}
    }

}
