using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Controllers
{
	[CreateAssetMenu(menuName="Data/Controllers/AI/States/Wander", fileName="AI_WanderState")]
    public class ControllerAIStateWander : ControllerAIState
    {
		public float TimeStopped = 3f;
		public float WanderRadius = 4f;
		public bool RelativeToStartingPoint = true;

		private Vector3 _startingPoint = Vector3.zero;
		private float _movementTimer = 0f;

        private int currentPriority = 0;
        public override int CurrentPriority { get { return currentPriority;  } }

		CharacterMovementBase movement;

        public override int UpdatePriority(ControllerComponent component)
        {
			return currentPriority = 1;
        }

        public override void OnCreate(ControllerComponent component)
		{
			movement = component.Data.Components.Movement;
			if (!movement)
				Debug.LogError("No movement component in object.");

			_startingPoint = movement.transform.position;
		}

        public override void OnEnter(ControllerComponent component)
		{
		}

        public override void OnUpdate(ControllerComponent component, ref ControllerCharacterInput input)
		{
			_movementTimer += Time.deltaTime;
			if (_movementTimer >= TimeStopped)		
			{
				ChangePosition(movement);
				_movementTimer = 0f;
			}

			float remainingDist = movement.NavMeshAgent.remainingDistance;
			input.Direction = movement.NavMeshAgent.desiredVelocity * remainingDist;
		}

		public void ChangePosition(CharacterMovementBase movement)
		{
			Vector3 newPosition = Random.insideUnitSphere * WanderRadius;
			newPosition.y = 0f;

			if (RelativeToStartingPoint)
				newPosition += _startingPoint;
			else
				newPosition += movement.transform.position;

			movement.SetDestination(newPosition);
		}

        public override void OnExit(ControllerComponent component)
		{

		}

        public override void Destroy(ControllerComponent component)
		{

		}
    }

}
