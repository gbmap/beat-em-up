using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace Catacumba.Data
{
    [CreateAssetMenu()]
    public class CharacterStatConfiguration : ScriptableObject
    {
        public MinMaxCurve VigorCurve;
        public MinMaxCurve StrengthCurve;
        public MinMaxCurve DexterityCurve;
        public MinMaxCurve MagicCurve;
    }
}