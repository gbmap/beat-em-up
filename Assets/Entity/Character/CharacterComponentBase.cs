using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Entity
{
    public abstract class CharacterComponentBase : MonoBehaviour
    {
        protected CharacterData data;

        protected virtual void Awake()
        {
            data = GetComponentInParent<CharacterData>();
        }

        protected virtual void Start()
        {
            data.Components.ForEachComponent(c => this.OnComponentAdded(c));
            data.OnComponentAdded?.Invoke(this);
        }

        protected virtual void Destroy()
        {
            data.OnComponentRemoved?.Invoke(this);
        }

        protected virtual void OnEnable()
        {
            data.OnComponentAdded += OnComponentAdded;
            data.OnComponentRemoved += OnComponentRemoved;
        }

        protected virtual void OnDisable()
        {
            data.OnComponentAdded -= OnComponentAdded;
            data.OnComponentRemoved -= OnComponentRemoved;
        }

        public virtual void OnConfigurationEnded() {}

        protected virtual void OnComponentAdded(CharacterComponentBase component) {}
        protected virtual void OnComponentRemoved(CharacterComponentBase component) {}

#if UNITY_EDITOR
        public virtual string GetDebugString() { return string.Empty; }
#endif
    }
}