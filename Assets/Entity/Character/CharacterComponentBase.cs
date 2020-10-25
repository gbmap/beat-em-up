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
            foreach (var component in data.CharacterComponents)
                this.OnComponentAdded(component);

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

        protected virtual void OnComponentAdded(CharacterComponentBase component) {}
        protected virtual void OnComponentRemoved(CharacterComponentBase component) {}
    }
}