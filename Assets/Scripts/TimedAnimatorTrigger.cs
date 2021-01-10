using UnityEngine;

namespace Catacumba.Entity
{
    public class TimedAnimatorTrigger : MonoBehaviour
    {
        public string Trigger;
        public float Delay = 2f;

        private Animator _animator;
        private float _timer;

        void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _timer = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (!_animator) return;
            if (_timer >= Delay)
            {
                _animator.SetTrigger(Trigger);
                _timer = 0f;
            }

            _timer += Time.deltaTime;
        }
    }
}