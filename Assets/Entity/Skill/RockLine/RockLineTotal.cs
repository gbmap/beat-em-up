using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Effects.Skills.RockLine {
    [ExecuteInEditMode]
    public class RockLineTotal : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float T;

        public RockLineRock[] rocks;
        FreezeAnimator freezeAnimator;

        private void Awake()
        {
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
        }

        float Remap(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }
    }
}