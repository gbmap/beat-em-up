using Catacumba.Data.Controllers;
using Catacumba.Data.Items;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Entity
{
    public class ControllerComponent : CharacterComponentBase
    {
        public ControllerBase Controller;
        public CharacterData Data { get { return data; } }

        private ControllerCharacterInput input;

        public void SetController(ControllerBase newController)
        {
            if (!newController)
                throw new System.Exception("controller passed == null");

            if (Controller)
                Controller.Destroy(this);

            Controller = Instantiate<ControllerBase>(newController);
            Controller.Setup(this);
        }
        
        ////////////////////////////// 
        //      MONOBEHAVIOUR
#region MONOBEHAVIOUR

        protected override void Start()
        {
            base.Start();
            if (Controller)
                SetController(Controller);   
            else
                Debug.LogError("ControllerComponent with no Controller set.");

            input = new ControllerCharacterInput();
        }

        void Update()
        {
            if (!Controller) return;
            input.Reset();
            Controller?.OnUpdate(this, ref input);

            var movement = Data.Components.Movement; 
            if (movement)
            {
                movement.Direction = input.Direction;
                if (input.Dodge)
                    (movement as CharacterMovementWalkDodge)?.Dodge(input.Direction);

                movement.LookDir = input.LookDir;
            }

            if (input.Attack)
            {
                var combat = Data.Components.Combat;
                if (combat)
                {
                        combat.RequestAttack(input.AttackType);
                }
            }

            if (input.Interact)
                Data.GetComponent<CharacterInteract>()?.Interact();

            if (input.DropItem)
            {
                InventorySlot slot = Data.Stats.Inventory.GetWeaponSlot();
                if (slot != null)
                {
                    data.Stats.Inventory.Drop(new Data.Items.InventoryDropParams
                    {
                        Slot = slot.Part
                    });
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Controller?.Destroy(this);
        }

        #endregion

        public override string GetDebugString()
        {
            if (!Controller) return string.Empty;
            return input?.ToString();
            //return Controller.GetDebugString(this);
        }

    }
}