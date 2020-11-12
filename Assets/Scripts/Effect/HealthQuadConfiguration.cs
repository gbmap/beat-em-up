using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Effects
{
    public interface IHealthQuad
    {
        void SetHealth(MonoBehaviour component, float value);
        void SetStamina(MonoBehaviour component, float value);
    }

    public class GameObjectSystemPool<T> : Dictionary<GameObject, T> where T : Component 
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
            if (!TryGetValue(key.gameObject, out ret))
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

    [CreateAssetMenu(menuName="Effects/Character/Health Quad")]
    public class HealthQuadConfiguration : EffectConfiguration, IHealthQuad
    {
        public GameObject Prefab;
        public Color Color;

        public static GameObjectSystemPool<Renderer> effectPool = new GameObjectSystemPool<Renderer>();

        public void SetHealth(MonoBehaviour component, float value)
        {
            SetFloat(component, "_Health", value);
        }

        public void SetStamina(MonoBehaviour component, float value)
        {
            SetFloat(component, "_Poise", value);
        }

        private void SetColor(Renderer quad, Color color)
        {
            quad.material.SetColor("_Color", color);
        }

        private void SetFloat(MonoBehaviour component, string name, float value)
        {
            Renderer quad = effectPool.Get(component);
            quad.material.SetFloat(name, value);
        }

        public override void Destroy(MonoBehaviour component)
        {
            Renderer quad = effectPool.Get(component);
            if (!quad) return;

            effectPool.Remove(component.gameObject);
            Destroy(quad.gameObject);
        }

        public override bool Setup(MonoBehaviour component)
        {
            effectPool.CleanPool();

            var quad = Instantiate(Prefab);
            Renderer renderer = quad.GetComponent<Renderer>();

            quad.transform.parent = component.transform;
            quad.transform.localPosition = Vector3.zero;
            quad.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            Vector3 scale = new Vector3(renderer.bounds.size.x * 2f, renderer.bounds.size.z * 2f, 1f);
            quad.transform.localScale = scale;

            SetColor(renderer, Color);
            effectPool.Add(component.gameObject, renderer);
            return true;
        }

        public override void Play(MonoBehaviour component) { }
        public override void Stop(MonoBehaviour component) { }

    }
}