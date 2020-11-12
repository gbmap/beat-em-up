using System.Text;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Controllers
{
    public class ControllerCharacterInput
    {
        public Vector3 Direction;
        public Vector3 LookDir; // This won't be explicitly controlled by the player
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

        public override string ToString()
        {
            StringBuilder db = new StringBuilder();
            db.AppendFormat("Direction: {0}\n", Direction);
            db.AppendFormat("Attack: {0}\n", Attack);
            db.AppendFormat("AttackType: {0}\n", AttackType);
            db.AppendFormat("Dodge: {0}\n", Dodge);
            db.AppendFormat("Interact: {0}\n", Interact);
            db.AppendFormat("DropItem: {0}\n", DropItem);
            return db.ToString();
        }
    }

    public abstract class ControllerBase : ScriptableObject
    {
        public abstract ECharacterBrainType BrainType { get; } 

        public abstract void Setup(ControllerComponent controller);
        public abstract void OnUpdate(ControllerComponent controller, ref ControllerCharacterInput input);
        public abstract void Destroy(ControllerComponent controller);

        public virtual string GetDebugString(ControllerComponent controller) { return string.Empty; }
    }
}