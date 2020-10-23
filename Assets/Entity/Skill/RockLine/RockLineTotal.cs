using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Catacumba.Entity;

namespace Catacumba.Effects.Skills.RockLine {
    [ExecuteInEditMode]
    public class RockLineTotal : SkillData
    {
        [Range(0f, 1f)]
        public float T;

        public RockLineRock[] rocks;
        Animator animator;
        FreezeAnimator freezeAnimator;

        private float timer = 0f;

        private void Awake()
        {
            T = 0f;
            animator = GetComponent<Animator>();
            freezeAnimator = GetComponent<FreezeAnimator>();
        }

        private void OnEnable()
        {
            for (int i = 0; i < rocks.Length; i++)
            {
                var r = rocks[i];
                r.OnAttack += OnAttack;
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < rocks.Length; i++)
            {
                var r = rocks[i];
                r.OnAttack -= OnAttack;
            }
        }

        private void OnAttack(CharacterAttackData data)
        {
            if (data.Defender == null || freezeAnimator == null) return;
            freezeAnimator.Freeze();
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < rocks.Length; i++)
            {
                float a = ((float)i) / rocks.Length;
                float b = ((float)(i+1)) / rocks.Length;
                RockLineRock r = rocks[i];
                if (r == null) continue;
                r.T = Mathf.Clamp01(Remap(T, a, b, 0f, 1f));
            }

            if (Mathf.Approximately(T, 1f))
            {
                if (timer >= 4f)
                {
                    animator.SetTrigger("Destroy");
                    timer = float.NegativeInfinity;
                }

                timer += Time.deltaTime;
            }
        }

        float Remap(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        void OnDestroyAnimationEnd()
        {
            System.Array.ForEach(rocks, r => r.GetComponent<UnparentParticleSystemOnDeath>().Detach());
            Destroy(gameObject);
        }

        public override void Cast()
        {

        }
    }
}