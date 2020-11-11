using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Effects
{
    public class EffectSetupResult {}
    public class EffectParams {}

    public abstract class EffectConfiguration : ScriptableObject 
    {

        // I wanna pull my eyes out with alcohol-drenched forks.
        public abstract bool Setup(MonoBehaviour component);
        public abstract void Play(MonoBehaviour component);
        public abstract void Stop(MonoBehaviour component);
        public abstract void Destroy(MonoBehaviour component);
    }
}