using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Entity
{
    public class CharacterMovementWalk : CharacterMovementBase
    {
        public float MoveSpeed { get { return data.Stats.MoveSpeed; } }

        protected override Vector3 UpdateVelocityWithDesiredDirection(Vector3 vel, Vector3 direction)
        {
            return direction * MoveSpeed;
        }
    }
}