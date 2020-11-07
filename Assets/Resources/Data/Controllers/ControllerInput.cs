using Catacumba.Entity;
using Rewired;
using UnityEngine;

namespace Catacumba.Data.Controllers
{
    [CreateAssetMenu(menuName="Data/Controllers/Input", fileName="ControllerInput")]
    public class ControllerInput : ControllerBase
    {
        public override ECharacterBrainType BrainType { get { return ECharacterBrainType.Input; } }

        public int InputIndex = 0;
        Player RewiredInput;

        private float dropTimer = 0f;

        public override void Setup(ControllerComponent controller)
        {
            RewiredInput = ReInput.players.GetPlayer(InputIndex);
        }

        public override void Destroy(ControllerComponent controller) { }

        public override void OnUpdate(ControllerComponent controller,
                                      ref ControllerCharacterInput input)
        {
            CharacterCombat combat = controller.Data.Components.Combat;

            input.Direction = UpdateMovement(controller);
            input.Attack    = UpdateCombat(controller, out input.AttackType);
            input.Interact  = UpdateInteraction(controller);
            input.Dodge     = UpdateDodge(controller);
            input.DropItem  = UpdateDropItem(controller);
        }

        private Vector3 UpdateMovement(ControllerComponent controller)
        {
            float hAxis = RewiredInput.GetAxis("HorizontalMovement");
            float vAxis = RewiredInput.GetAxis("VerticalMovement");

            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;
            
            Vector3 cFwd = cameraForward * vAxis + cameraRight * hAxis;
            cFwd.y = 0;
            return cFwd;
        }

        private bool UpdateDodge(ControllerComponent controller)
        {
            return RewiredInput.GetButtonDown("Dodge");
        }

        private bool UpdateCombat(ControllerComponent controller, out EAttackType attackType)
        {
            if (RewiredInput.GetButtonDown("WeakAttack"))
            {
                attackType = EAttackType.Weak;
                return true;
            }
            else if (RewiredInput.GetButtonDown("StrongAttack"))
            {
                attackType = EAttackType.Strong;
                return true;
            }

            attackType = EAttackType.Weak;
            return false;
        }

        private bool UpdateInteraction(ControllerComponent controller)
        {
            return RewiredInput.GetButton("Submit");
        }

        private bool UpdateDropItem(ControllerComponent controller)
        {
            if (RewiredInput.GetButton("Submit"))
            {
                dropTimer += Time.deltaTime;
                if (dropTimer > 2f)
                {
                    dropTimer = 0f;
                    return true;

                    /*
                    InventorySlot slot = data.Stats.Inventory.GetWeaponSlot();
                    if (slot != null)
                    {
                        data.Stats.Inventory.Drop(new Data.Items.InventoryDropParams
                        {
                            Slot = slot.Part
                        });
                        dropTimer = 0f;
                    }
                    */
                }
            }

            return false;
        }
    }
}