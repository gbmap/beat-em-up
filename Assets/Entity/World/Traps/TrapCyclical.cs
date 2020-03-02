using Catacumba.Effects.Skills.RockLine;
using UnityEngine;


namespace Catacumba.Entity.World.Traps
{
    public class TrapCyclical : MonoBehaviour
    {
        public float ASpeed = 2f;
        public float BSpeed = 0.75f;
        public float SleepTime = 3f;
        private float timer;
        
        [Range(0f, 1f)]
        public float T;
        private float targetT;

        public ObjectPosAnim Animation;

        bool attackCheck;

        private void Awake()
        {
            Animation.InitiatePositions(
                Vector3.up * Animation.obj.GetComponent<MeshRenderer>().bounds.extents.y*-2f, 
                Vector3.zero
            );
        }

        private void Update()
        {
            // set T diferente do alvo, lerpa pra posição alvo
            if (!Mathf.Approximately(T, targetT))
            {
                Animation.Sample(T);
                T = Mathf.Clamp01(T +  Mathf.Sign(targetT - T) * Time.deltaTime * (targetT == 1f?ASpeed:BSpeed));
            }
            // T == targetT
            else
            {
                // se não atacou ainda, ataca
                if (!attackCheck)
                {
                    CharacterAttackData ad = new CharacterAttackData(EAttackType.Weak, gameObject)
                    {
                        Damage = 100
                    };
                    Bounds b = Animation.obj.GetComponent<MeshRenderer>().bounds;
                    CombatManager.Attack(ref ad, b.center, b.extents, transform.rotation);
                    attackCheck = true;
                }

                // se ja esperou o suficiente, reseta timer e flag de attack check
                if (timer >= SleepTime)
                {
                    targetT = targetT == 1f ? 0f : 1f;
                    attackCheck = false;
                    timer = 0f;
                }
                
                // incrementa timer
                timer += Time.deltaTime;
            }
        }
    }
}