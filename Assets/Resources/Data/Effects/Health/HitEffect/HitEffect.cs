using System.Collections;
using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Effects 
{
    public interface IHitEffect 
    {
        void Update();
        void OnHit(CharacterHealth health);
    }

    public class HitEffect
    {
        private Material[] Materials { get; set; }

        private float hitEffectFactor;
        private float HitEffectFactor
        {
            get { return hitEffectFactor; }
            set
            {
                hitEffectFactor = value;
                if (Materials == null) return;
                for (int i = 0; i < Materials.Length; i++)
                {
                    Material m = Materials[i];
                    if (m == null) continue;
                    m.SetFloat("_HitFactor", value);
                }
            }
        }

        public HitEffect(CharacterHealth health)
        {
            RefreshMaterials(health);
        }

        void RefreshMaterials(CharacterHealth health)
        {
            List<Material> materials = new List<Material>();
            foreach (var r in health.GetComponentsInChildren<Renderer>())
                materials.Add(r.material);

            Materials = materials.ToArray();
        }

        public void Update()
        {
            if (!Mathf.Approximately(HitEffectFactor, 0f))
                HitEffectFactor = Mathf.Max(0f, HitEffectFactor - Time.deltaTime * 2f);
        }

        public void OnHit()
        {
            HitEffectFactor = 1f;
        }
    }
}