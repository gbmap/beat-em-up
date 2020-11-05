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

        public override void Setup(ControllerComponent controller)
        {
            RewiredInput = ReInput.players.GetPlayer(InputIndex);
        }

        public override void Destroy(ControllerComponent controller) { }

        public override void OnUpdate(ControllerComponent controller)
        {
            CharacterCombat combat = controller.Data.Components.Combat;

            UpdateMovement(controller);
            UpdateCombat(controller);
            UpdateInteraction(controller);
        }

        private void UpdateMovement(ControllerComponent controller)
        {
            CharacterMovementBase movement = controller.Data.Components.Movement;
            if (!movement) return;

            float hAxis = RewiredInput.GetAxis("HorizontalMovement");
            float vAxis = RewiredInput.GetAxis("VerticalMovement");

            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;
            
            Vector3 cFwd = cameraForward * vAxis + cameraRight * hAxis;
            cFwd.y = 0;

            movement.Direction = cFwd;

            if (RewiredInput.GetButtonDown("Dodge"))
                (movement as CharacterMovementWalkDodge)?.Dodge(cFwd);
        }

        private void UpdateCombat(ControllerComponent controller)
        {
            CharacterCombat combat = controller.Data.Components.Combat;
            if (!combat) return;

            if (RewiredInput.GetButtonDown("WeakAttack"))
                combat.RequestAttack(EAttackType.Weak);
            else if (RewiredInput.GetButtonDown("StrongAttack"))
                combat.RequestAttack(EAttackType.Strong);
        }

        private void UpdateInteraction(ControllerComponent controller)
        {
            if (RewiredInput.GetButton("Submit"))
                controller.GetComponent<CharacterInteract>()?.Interact();
        }
    }
}