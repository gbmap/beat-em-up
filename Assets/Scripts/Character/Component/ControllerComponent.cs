using System;
using Catacumba.Data.Character;
using Catacumba.Data.Controllers;
using Catacumba.Data.Items;
using Catacumba.Data.Items.Characteristics;
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

        public override void OnConfigurationEnded()
        {
            base.OnConfigurationEnded();
            data.Stats.Inventory.OnWeaponEquipped += Cb_OnWeaponEquipped;
        }

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
                Item weapon = Data.Stats.Inventory.GetWeapon();
                BodyPart slot = Data.Stats.Inventory.GetWeaponSlot();
                if (weapon != null)
                {
                    data.Stats.Inventory.Drop(new Data.Items.InventoryDropParams
                    {
                        Slot = slot
                    });
                }

                input.DropItem = false;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Controller?.Destroy(this);
            data.Stats.Inventory.OnWeaponEquipped -= Cb_OnWeaponEquipped;
        }

        private void Cb_OnWeaponEquipped(InventoryEquipResult result, CharacteristicWeaponizable weaponizable)
        {
            if (weaponizable.Behavior == null)
                return;

            SetController(weaponizable.Behavior);
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