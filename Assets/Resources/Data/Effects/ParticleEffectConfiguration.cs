using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
        public enum EStartingPosition
        {
            TransformOrigin,
            RendererBoundsOrigin,
            ColliderOrigin,
            NavMeshOrigin
        }

        // TODO: particle system changing should be supported on runtime.
        public ParticleSystem ParticleSystem;

        [Header("Local position inside the first-found renderer's bounding box.")]
        public Vector3 LocalPosition;
        public EStartingPosition StartingPosition = EStartingPosition.TransformOrigin;

        private static SystemPool<ParticleSystem> systemPool = new SystemPool<ParticleSystem>();

        private Vector3 CalculatePosition(MonoBehaviour obj, Vector3 pos)
        {
            switch (StartingPosition)
            {
                case EStartingPosition.RendererBoundsOrigin:
                    return CalculateRendererPosition(obj, pos);
                case EStartingPosition.TransformOrigin:
                    return CalculateTransformPosition(obj, pos);
                case EStartingPosition.NavMeshOrigin:
                    return CalculateNavAgentOrigin(obj, pos);
                case EStartingPosition.ColliderOrigin:
                    return CalculateColliderPosition(obj, pos);
                default:
                    return CalculateTransformPosition(obj, pos);
            }
        }

        private Vector3 CalculateRendererPosition(MonoBehaviour obj, Vector3 pos)
        {                
            var renderer = obj.GetComponentInChildren<Renderer>();
            return CalculateBoundsPosition(renderer.bounds, pos);
        }

        private Vector3 CalculateBoundsPosition(Bounds bounds, Vector3 pos)
        {
            float x = pos.x * bounds.extents.x;
            float y = pos.y * bounds.extents.y;
            float z = pos.z * bounds.extents.z;

            Vector3 origin = new Vector3(0f, bounds.extents.y, 0f);
            return origin + new Vector3(x,y,z);
        }

        private Vector3 CalculateTransformPosition(MonoBehaviour obj, Vector3 pos)
        {
            return pos; 
        }

        private Vector3 CalculateColliderPosition(MonoBehaviour obj, Vector3 pos)
        {                
            var collider = obj.GetComponent<Collider>();
            return CalculateBoundsPosition(collider.bounds, pos);
        }

        private Vector3 CalculateNavAgentOrigin(MonoBehaviour obj, Vector3 pos)
        {
            var navAgent = obj.GetComponentInParent<NavMeshAgent>();

            float r = navAgent.radius;
            float h = navAgent.height;

            float x = pos.x * r;
            float y = pos.y * h;
            float z = pos.z * r;

            Vector3 origin = new Vector3(0f, h/2, 0f);
            return origin + new Vector3(x,y,z);
        }

        public override ParticleSystem Setup(MonoBehaviour obj)
        {
            systemPool.CleanPool();

            ParticleSystem system = Instantiate(ParticleSystem);
            system.gameObject.name = string.Format("{0}_{1}", system.gameObject.name, obj.name); 
            system.transform.parent = obj.transform;
            system.transform.localPosition = CalculatePosition(obj, LocalPosition);
            system.transform.localRotation = Quaternion.identity;

            systemPool.Add(obj, system);
            return system;
        }

        public override void Destroy(MonoBehaviour obj)
        {
            ParticleSystem system = systemPool.Get(obj);
            if (system)
            {
                systemPool.Remove(obj);
                Destroy(system.gameObject);
            }
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

        public bool IsEmitting(MonoBehaviour obj) 
        {
            ParticleSystem system = systemPool.Get(obj);
            return system.isEmitting;
        }

        public void SetEmission(MonoBehaviour obj, bool v)
        {
            ParticleSystem system = systemPool.Get(obj);
            var emission = system.emission;
            emission.enabled = v;
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
