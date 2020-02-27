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

        private void Awake()
        {
            Animation.InitiatePositions(
                Vector3.up * Animation.obj.GetComponent<MeshRenderer>().bounds.extents.y*-2f, 
                Vector3.zero
            );
        }

        private void Update()
        {
            if (!Mathf.Approximately(T, targetT))
            {
                Animation.Sample(T);
                T = Mathf.Clamp01(T +  Mathf.Sign(targetT - T) * Time.deltaTime * (targetT == 1f?ASpeed:BSpeed));
            }
            else
            {
                if (timer >= SleepTime)
                {
                    targetT = targetT == 1f ? 0f : 1f;
                    timer = 0f;
                }
                timer += Time.deltaTime;
            }
        }
    }
}