﻿using System.Collections;
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
                velocity = UpdateVelocityWithDesiredDirection(velocity, Direction.normalized);
            return velocity;
        }
    }
}