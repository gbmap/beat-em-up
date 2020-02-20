using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Effects.Skills.RockLine
{
    [System.Serializable]
    public class RockObjectAnim
    {
        [HideInInspector]
        public Vector3 posA, posB;

        public GameObject obj;
        public AnimationCurve curve;

        public void Sample(float t)
        {
            if (obj == null) return;
            obj.transform.localPosition = Vector3.Lerp(posA, posB, curve.Evaluate(t));
        }

        public void InitiatePositions()
        {
            if (obj == null) return;

            posA = Vector3.up * obj.GetComponent<MeshRenderer>().bounds.extents.y * -2f;
            posB = Vector3.zero;
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
        public RockObjectAnim rock;
        public RockObjectAnim littleRocks;
        public RockObjectAnim sand;

        private float lastAttackCheck;

        public System.Action<CharacterAttackData> OnAttack;


        // Start is called before the first frame update
        void OnEnable()
        {
            rock.InitiatePositions();
            littleRocks.InitiatePositions();
            sand.InitiatePositions();
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