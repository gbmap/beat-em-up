using System.Collections;
using System.Collections.Generic;
using Catacumba.Data;
using Catacumba.Effects;
using UnityEngine;

namespace Catacumba.Entity
{
    public class CharacterHealthQuad : CharacterComponentBase
    {
        public HealthEffectsConfiguration HealthEffects;

        public override void OnConfigurationEnded()
        {
            base.OnConfigurationEnded();
            data.Stats.OnStatsChanged += OnStatsChangedCallback;
            SetupEffects();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            data.Stats.OnStatsChanged -= OnStatsChangedCallback;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            HealthEffects?.Destroy(this);
        }

        private void SetupEffects()
        {
            if (!HealthEffects)
                HealthEffects = data.ConfigurationView.HealthQuad;
            
            if (HealthEffects)
            {
                HealthEffects.Setup(this);
                HealthEffects.SetHealth(this, data.Stats.HealthNormalized);
                HealthEffects.SetStamina(this, data.Stats.StaminaBar);
            }
        }

        private void OnStatsChangedCallback(CharacterStats stats)
        {
            // UpdateHealthQuad(stats.HealthNormalized, stats.StaminaBar);
            if (HealthEffects)
            {
                HealthEffects.SetHealth(this, stats.HealthNormalized);
                HealthEffects.SetStamina(this, stats.StaminaBar);
            }
        }

    }
}