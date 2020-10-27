using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Effects
{
    public class SystemPool<T> : Dictionary<MonoBehaviour, T> where T : Component
    {
        public void CleanPool()
        {
            foreach (var key in Keys)
            {
                if (this[key] == null)
                    Remove(key);
            }
        }

        public T Get(MonoBehaviour key)
        {
            T ret;
            if (!TryGetValue(key, out ret))
            {
                ret = AttemptReload(key);
                if (ret) return ret;

                Debug.LogError("No such key.");
                return default(T);
            }

            return ret;
        }

        private T AttemptReload(MonoBehaviour key)
        {
            T[] components = key.GetComponentsInChildren<T>();
            foreach (T component in components)
            {
                if (component.gameObject.name.Contains(key.name))
                    return component;
            }

            return default(T);
        }
    }

    public class ParticleEffectParams : EffectParams
    {
        public MonoBehaviour component;
    }

    [CreateAssetMenu(menuName="Effects/Particle Effect Configuration")]
    public class ParticleEffectConfiguration : EffectConfiguration<ParticleEffectParams, ParticleSystem>
    {
        // TODO: particle system changing should be supported on runtime.

        public ParticleSystem ParticleSystem;

        [Header("Local position inside the first-found renderer's bounding box.")]
        public Vector3 LocalPosition;

        private static SystemPool<ParticleSystem> systemPool = new SystemPool<ParticleSystem>();

        /*
            Gets local position inside first-fount renderer.
        */
        private Vector3 CalculatePosition(MonoBehaviour obj, Vector3 pos)
        {
            var renderer = obj.GetComponentInChildren<Renderer>();
            float x = pos.x * renderer.bounds.extents.x;
            float y = pos.y * renderer.bounds.extents.y;
            float z = pos.z * renderer.bounds.extents.z;
            return renderer.bounds.center + new Vector3(x,y,z);
        }

        public override ParticleSystem Setup(MonoBehaviour obj)
        {
            systemPool.CleanPool();

            ParticleSystem system = Instantiate(ParticleSystem);
            system.gameObject.name = string.Format("{0}_{1}", system.gameObject.name, obj.name); 
            system.transform.parent = obj.transform;
            system.transform.position = CalculatePosition(obj, LocalPosition);

            systemPool.Add(obj, system);
            return system;
        }

        public override void Play(ParticleEffectParams parameters)
        {
            ParticleSystem system = systemPool.Get(parameters.component);
            system.Play(true);
        }

        public override void Stop(ParticleEffectParams parameters)
        {
            ParticleSystem system = systemPool.Get(parameters.component);
            system.Stop();
        }

        public void EmitRing(MonoBehaviour instance, 
                             int nParticles, 
                             int nDeviation = 10, 
                             float velocity = 13f)
        {
            ParticleSystem systemInstance = systemPool.Get(instance);

            int range = UnityEngine.Random.Range(nParticles-nDeviation, nParticles+nDeviation);
            for (int i = 0; i < range; i++)
            {
                Vector3 vel = UnityEngine.Random.insideUnitSphere;
                vel.y = 0f;
                vel.Normalize();
                vel *= velocity;
                systemInstance.Emit(new ParticleSystem.EmitParams
                {
                    velocity = vel
                }, 1);
            }
        }

        public void EmitBurst(MonoBehaviour instance, int nParticles)
        {
            ParticleSystem systemInstance = systemPool.Get(instance);
            systemInstance.Emit(nParticles);
        }

        public void PointSystemTowards(MonoBehaviour instance, Vector3 direction)
        {
            ParticleSystem systemInstance = systemPool.Get(instance);
            systemInstance.transform.rotation = Quaternion.LookRotation(direction);
        }

    }
}
