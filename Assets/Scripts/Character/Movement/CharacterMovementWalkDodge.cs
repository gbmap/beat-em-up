using UnityEngine;

namespace Catacumba.Entity
{
    public class CharacterMovementWalkDodge : CharacterMovementWalk
    {
        public System.Action OnDodge;
        //public bool IsDodging { get; private set; }
        public bool IsDodging { get { return dodgeTimer > 0f; } }
        public override bool IsStopped => base.IsStopped && !IsDodging;

        private const float dodgeDuration = 1f;
        private float dodgeTimer;
        private float lastDodgeTime;
        private Vector3 dodgeDirection;

        protected override Vector3 UpdateVelocityWithDesiredDirection(Vector3 vel, Vector3 direction)
        {
            if (IsDodging)
            {
                float a = dodgeDuration - dodgeTimer;
                a = a * a * a * a;
                direction = Vector3.Slerp(dodgeDirection, direction, a);
            }
            float rollSpeed = 1f + Mathf.Clamp01(dodgeTimer * (dodgeDuration - dodgeTimer) * 4f);
            return base.UpdateVelocityWithDesiredDirection(vel, direction) * rollSpeed;
        }

        protected override void Update()
        {
            base.Update();
            dodgeTimer = Mathf.Clamp01(dodgeTimer - Time.deltaTime);
        }

        public void Dodge(Vector3 direction)
        {
            OnDodge?.Invoke();
            dodgeTimer = dodgeDuration;
            dodgeDirection = direction;
        }

        protected override void UpdateEffect()
        {
            base.UpdateEffect();

            if (!MovementEffect) return;

            MovementEffect.SetEmission(this, IsDodging);
            bool emission = MovementEffect.IsEmitting(this);
            if (emission)
                MovementEffect.PointSystemTowards(this, -dodgeDirection);
        }

        public void AnimationBeginDodge()
        {
            // IsDodging = true;
        }

        public void AnimationEndDodge()
        {
            // IsDodging = false;
        }
    }
}