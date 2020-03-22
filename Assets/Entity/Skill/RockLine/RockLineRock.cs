using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Effects.Skills.RockLine
{
    [System.Serializable]
    public class ObjectPosAnim
    {
        [HideInInspector]
        public Vector3 posA, posB;

        public GameObject obj;
        public AnimationCurve curve;

        public void Sample(float t)
        {
            if (obj == null) return;
            obj.transform.localPosition = Vector3.LerpUnclamped(posA, posB, curve.Evaluate(t));
        }

        public void InitiatePositions(Vector3 positionA, Vector3 positionB)
        {
            if (obj == null) return;
            posA = positionA;
            posB = positionB;
        }
    }

    [ExecuteInEditMode]
    public class RockLineRock : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float T;
        private float lastT;

        //public GameObject rock;
        //public RockObjectAnim rockFrames;
        public ObjectPosAnim rock;
        public ObjectPosAnim littleRocks;
        public ObjectPosAnim sand;

        private float lastAttackCheck;

        public ParticleSystem[] Effects;
        private bool hasEmittedEffect;

        public System.Action<CharacterAttackData> OnAttack;

        // Start is called before the first frame update
        void OnEnable()
        {
            rock.InitiatePositions(Vector3.up * rock.obj.GetComponent<MeshRenderer>().bounds.extents.y * -2f, Vector3.zero);
            littleRocks.InitiatePositions(Vector3.up * littleRocks.obj.GetComponent<MeshRenderer>().bounds.extents.y * -2f, Vector3.zero);
            sand.InitiatePositions(Vector3.up * sand.obj.GetComponent<MeshRenderer>().bounds.extents.y * -2f, Vector3.zero);
        }

        // Update is called once per frame
        void Update()
        {
            rock.Sample(T);
            littleRocks.Sample(T);
            sand.Sample(T);

            if (Application.isPlaying)
            {
                AttackCheck();
            }

            if ( ((T > lastT && T >= 0.25f) ||
                  (T < lastT && T <= 0.25f) )
                && !hasEmittedEffect)
            {
                System.Array.ForEach(Effects, e => e.Emit(Random.Range(12, 17)));
                hasEmittedEffect = true;
            }

            if (T == 0f || T == 1f)
            {
                hasEmittedEffect = false;
            }


            lastT = T;
        }

        private void AttackCheck()
        {
            if (T != lastT && T > 0.5f && T < 1.0f)
            {
                CharacterAttackData ad = new CharacterAttackData(EAttackType.Weak, gameObject);
                Bounds b = GetBounds();
                CombatManager.Attack(ref ad, b.center, b.extents, transform.rotation);
                OnAttack?.Invoke(ad);
            }
        }

        Bounds GetBounds()
        {
            return rock.obj.GetComponent<MeshRenderer>().bounds;
            Bounds b = new Bounds();
            Bounds[] rs = {
                rock.obj.GetComponent<MeshRenderer>().bounds,
                littleRocks.obj.GetComponent<MeshRenderer>().bounds,
                sand.obj.GetComponent<MeshRenderer>().bounds
            };
            System.Array.ForEach(rs, r => b.Encapsulate(r));
            return b;
        }

        private void OnDrawGizmos()
        {
            Bounds b = GetBounds();
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
}