using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Controllers
{
    public class ControllerCharacterInput
    {
        public Vector3 Direction;
        public bool Attack;
        public EAttackType AttackType;
        public bool Dodge; 
        public bool Interact;
        public bool DropItem;

        public void Reset()
        {
            Direction = Vector3.zero;
            Attack    = false;
            Dodge     = false;
            Interact  = false;
            DropItem  = false;
        }
    }

    public abstract class ControllerBase : ScriptableObject
    {
        public abstract ECharacterBrainType BrainType { get; } 

        public abstract void Setup(ControllerComponent controller);
        public abstract void OnUpdate(ControllerComponent controller, ref ControllerCharacterInput input);
        public abstract void Destroy(ControllerComponent controller);
    }
}