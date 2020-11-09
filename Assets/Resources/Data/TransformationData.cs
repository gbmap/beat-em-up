using UnityEngine;

namespace Catacumba.Data
{
    [CreateAssetMenu(menuName="Data/Transformation", fileName="Transformation")]
    public class TransformationData : ScriptableObject
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
    }
}