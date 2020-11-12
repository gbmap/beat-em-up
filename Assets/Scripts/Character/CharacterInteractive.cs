using Catacumba.Data.Interactions;
using UnityEngine;

namespace Catacumba.Entity
{
    public class CharacterInteractive : MonoBehaviour
    {
        public bool IsOneShot;
        public Interaction Interaction;
        public System.Action<InteractionResult> OnInteraction;
        public System.Action<bool> OnHighlight;

        bool _wasInteracted = false;
        Material _material;

        private bool _isHighlighted;
        public bool IsHighlighted 
        {
            get { return _isHighlighted; } 
            set
            {
                if (_isHighlighted != value)
                {
                    OnHighlight?.Invoke(value);
                    float factor = System.Convert.ToSingle(value);
                    if (_material)
                        _material.SetFloat("_Selected", factor);
                }

                _isHighlighted = value;
            }
        }

        private float lastCollisionStay = float.NegativeInfinity;

        void Awake()
        {
            _material = GetComponent<Renderer>()?.material;
        }

        void Update()
        {
            IsHighlighted = Mathf.Abs(Time.time - lastCollisionStay) < 0.05f;
        }

        void OnTriggerStay(Collider other)
        {
            lastCollisionStay = Time.time;
        }

        public void Interact(CharacterData data, System.Action<InteractionResult> callback)
        {
            /*
            if (IsOneShot && _wasInteracted) return;
            _wasInteracted = true;

            callback += OnInteraction;
            Interaction.Interact(new InteractionParams
            {
                Interactor = data,
                Interacted = this,
                Callback = callback
            });
            callback -= OnInteraction;
            */
        } 
    }
}