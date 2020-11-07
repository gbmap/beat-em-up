using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Entity
{
    public class CharacterMovementWalk : CharacterMovementBase
    {
        public float MoveSpeed { get { return data.Stats.MoveSpeed; } }

        protected virtual Vector3 UpdateVelocityWithDesiredDirection(Vector3 vel, Vector3 direction)
        {
            return direction * MoveSpeed;
        }

        protected override Vector3 UpdateVelocity(Vector3 velocity, ref bool updatedValue)
        {
            velocity = base.UpdateVelocity(velocity, ref updatedValue);
            if (!updatedValue)
            {
                /*
                if (brainType == ECharacterBrainType.AI)
                    Direction = NavMeshAgent.desiredVelocity * NavMeshAgent.remainingDistance;
                */

                Direction = Vector3.ClampMagnitude(Direction, 1f);
                velocity = UpdateVelocityWithDesiredDirection(velocity, Vector3.ClampMagnitude(Direction, 1f));
            }
            return velocity;
        }
    }
}