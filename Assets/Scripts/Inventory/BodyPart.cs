using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Data.Character
{
    [CreateAssetMenu(menuName="Data/Character/Body Part", fileName="BodyPart")]
    public class BodyPart : ScriptableObject 
    { 
        public string BoneName = "";
        public Vector3 LocalPosition = Vector3.zero;
        public Vector3 LocalRotationEuler = Vector3.zero;
    }
}