using UnityEngine;

namespace Catacumba.Entity
{
    [RequireComponent(typeof(SphereCollider))]
    public class InteractiveComponent : InteractiveBaseComponent
    {
        public System.Action<bool> OnHighlight;

        private Material _material;
        private SphereCollider _colliderInteractionArea;

        // private Animator _animator;
        // private static int hashHighlighted = Animator.StringToHash("Highlighted");
        private static int hashTaken       = Animator.StringToHash("Taken");

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
                    _material?.SetFloat("_Selected", factor);
                }

                _isHighlighted = value;
            }
        }

        private float lastCollisionStay = float.NegativeInfinity;

        protected virtual void Start()
        {
            _material = GetComponentInChildren<Renderer>()?.material;
            _colliderInteractionArea = GetComponent<SphereCollider>();
            _colliderInteractionArea.radius = 1f;
            _colliderInteractionArea.isTrigger = true;
        }

        protected virtual void Update()
        {
            IsHighlighted = Mathf.Abs(Time.time - lastCollisionStay) < 0.05f;
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            lastCollisionStay = Time.time;
        }

        public void AnimTakenAnimationEnded()
        {
            Destroy(this.gameObject);
        }
    }
}