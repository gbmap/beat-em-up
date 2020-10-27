using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Effects
{
    public class EffectSetupResult {}
    public class EffectParams {}

    public abstract class EffectConfiguration<T, Q> : ScriptableObject 
        where T : EffectParams
    {

        // I wanna pull my eyes out with alcohol-drenched forks.
        public abstract Q Setup(MonoBehaviour component);
        public abstract void Play(T parameters);
        public abstract void Stop(T parameters);

    }
}