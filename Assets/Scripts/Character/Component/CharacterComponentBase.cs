using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Entity
{
    public abstract class CharacterComponentBase : MonoBehaviour
    {
        private CharacterData _data;
        protected CharacterData data
        {
            get { return _data ?? (_data = GetComponentInParent<CharacterData>()); }
        }

        protected virtual void Awake()
        {
            //data = GetComponentInParent<CharacterData>();
        }

        protected virtual void Start()
        {
            //data.SignalComponentAdded(this);
            data.SignalComponentAdded(this);
        }

        protected virtual void OnDestroy()
        {
            data.SignalComponentRemoved(this);
        }

        protected virtual void OnEnable()
        {
            //data.SignalComponentAdded(this);
        }

        protected virtual void OnDisable()
        {
            //data.SignalComponentRemoved(this);
        }

        public virtual void OnConfigurationEnded() {}

        public virtual void OnComponentAdded(CharacterComponentBase component) {}
        public virtual void OnComponentRemoved(CharacterComponentBase component) {}

#if UNITY_EDITOR
        public virtual string GetDebugString() { return string.Empty; }
#endif
    }
}