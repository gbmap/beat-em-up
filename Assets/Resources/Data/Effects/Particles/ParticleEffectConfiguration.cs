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
    public class ParticleEffectConfiguration : EffectConfiguration
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

        public Vector3 CalculatePosition(GameObject obj, Vector3 pos)
        {
            switch (StartingPosition)
            {
                case EStartingPosition.RendererBoundsOrigin:
                    return CalculateRendererPosition(obj.GetComponentInChildren<Renderer>(), pos);
                case EStartingPosition.TransformOrigin:
                    return CalculateTransformPosition(pos);
                case EStartingPosition.NavMeshOrigin:
                    return CalculateNavAgentOrigin(obj.GetComponentInParent<NavMeshAgent>(), pos);
                case EStartingPosition.ColliderOrigin:
                    return CalculateColliderPosition(obj.GetComponent<Collider>(), pos);
                default:
                    return CalculateTransformPosition(pos);
            }
        }

        private Vector3 CalculateRendererPosition(Renderer renderer, Vector3 pos)
        {                
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

        private Vector3 CalculateTransformPosition(Vector3 pos)
        {
            return pos; 
        }

        private Vector3 CalculateColliderPosition(Collider collider, Vector3 pos)
        {                
            return CalculateBoundsPosition(collider.bounds, pos);
        }

        private Vector3 CalculateNavAgentOrigin(NavMeshAgent navAgent, Vector3 pos)
        {
            float r = navAgent.radius;
            float h = navAgent.height;

            float x = pos.x * r;
            float y = pos.y * h;
            float z = pos.z * r;

            Vector3 origin = new Vector3(0f, h/2, 0f);
            return origin + new Vector3(x,y,z);
        }

        public override bool Setup(MonoBehaviour obj)
        {
            systemPool.CleanPool();

            ParticleSystem system = Instantiate(ParticleSystem);
            system.gameObject.name = string.Format("{0}_{1}", system.gameObject.name, obj.name); 
            system.transform.parent = obj.transform;
            system.transform.localPosition = CalculatePosition(obj.gameObject, LocalPosition);
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

        public override void Play(MonoBehaviour component)
        {
            ParticleSystem system = systemPool.Get(component);
            system.Play(true);
        }

        public override void Stop(MonoBehaviour component)
        {
            ParticleSystem system = systemPool.Get(component);
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

        public void EmitBurst(MonoBehaviour instance, int nParticles, Vector3 position)
        {
            ParticleSystem systemInstance = systemPool.Get(instance);
            systemInstance.Emit(new ParticleSystem.EmitParams
            {
                position = position
            }, nParticles);
        }

        public void PointSystemTowards(MonoBehaviour instance, Vector3 direction)
        {
            ParticleSystem systemInstance = systemPool.Get(instance);
            systemInstance.transform.rotation = Quaternion.LookRotation(direction);
        }

    }
}
