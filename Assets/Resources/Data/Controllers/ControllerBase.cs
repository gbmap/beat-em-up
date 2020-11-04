using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Controllers
{
    public abstract class ControllerBase : ScriptableObject
    {
        public abstract ECharacterBrainType BrainType { get; } 

        public abstract void Setup(ControllerComponent controller);
        public abstract void OnUpdate(ControllerComponent controller);
        public abstract void Destroy(ControllerComponent controller);
    }
}