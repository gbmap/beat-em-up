using UnityEngine;

namespace Catacumba.Data
{
    [CreateAssetMenu()]
    public class AnimationConfig : ScriptableObject
    {
        public RuntimeAnimatorController AnimatorController;
        public Avatar Avatar; 
    }
}