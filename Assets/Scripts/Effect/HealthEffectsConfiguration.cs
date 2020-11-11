using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Effects
{
    [CreateAssetMenu(menuName="Effects/Character/Health Effects Configuration")]
    public class HealthEffectsConfiguration : EffectConfiguration, IHealthQuad
    {
        public HealthQuadConfiguration HealthQuad;

        public override void Destroy(MonoBehaviour component)
        {
            HealthQuad?.Destroy(component);
        }

        public override bool Setup(MonoBehaviour component)
        {
            HealthQuad?.Setup(component);
            return true;
        }

        public override void Play(MonoBehaviour component) { }
        public override void Stop(MonoBehaviour component) { }

        public void SetHealth(MonoBehaviour component, float value)
        {
            ((IHealthQuad)HealthQuad).SetHealth(component, value);
        }

        public void SetStamina(MonoBehaviour component, float value)
        {
            ((IHealthQuad)HealthQuad).SetStamina(component, value);
        }
    }
}