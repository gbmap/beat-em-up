using Rewired;
using UnityEngine;
using Catacumba.Data;
using Catacumba.Data.Items;

namespace Catacumba.Entity
{
    public class CharacterPlayerInput : CharacterComponentBase
    {
        [SerializeField] private int playerIndex;
        public int PlayerIndex
        {
            get { return playerIndex; }
            set { _rewiredPlayer = ReInput.players.GetPlayer(playerIndex = value); }
        }

        CharacterMovementBase movement;
        CharacterCombat combat;

        Player _rewiredPlayer;
        public Player RewiredInput 
        { 
            get 
            { 
                if (_rewiredPlayer == null)
                    _rewiredPlayer = ReInput.players.GetPlayer(PlayerIndex);
                return _rewiredPlayer; 
            } 
        }

        public System.Action<CharacterData> OnInteract;

        private Vector3 cameraForward;
        private Vector3 cameraRight;
        private bool updateCameraDir = true;

        private float dropTimer;

        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            // Get first player as default
            //_rewiredPlayer = ReInput.players.GetPlayer(0);
            PlayerIndex = playerIndex;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (movement)
                movement.Direction = Vector3.zero;

            //if (!CameraManager.Instance) return;
            //CameraManager.Instance.OnCameraChange -= OnCameraChange;
        }

        public override void OnComponentAdded(CharacterComponentBase component)
        {
            base.OnComponentAdded(component);

            if (component is CharacterMovementBase)
            {
                movement = component as CharacterMovementBase;
            }

            else if (component is CharacterCombat)
            {
                combat = component as CharacterCombat;
            }
        }

        public override void OnComponentRemoved(CharacterComponentBase component)
        {
            base.OnComponentAdded(component);

            if (component is CharacterMovementBase)
            {
                movement = null;
            }

            else if (component is CharacterCombat)
            {
                combat = null;
            }
        }

        private void OnCameraChange()
        {
            updateCameraDir = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (combat)
            {
                if (RewiredInput.GetButtonDown("WeakAttack"))
                {
                    combat.RequestAttack(EAttackType.Weak);
                }
                else if (RewiredInput.GetButtonDown("StrongAttack"))
                    combat.RequestAttack(EAttackType.Strong);
            }

            if (RewiredInput.GetButtonDown("Submit"))
            {
                GetComponent<CharacterInteract>()?.Interact();
            }

            if (RewiredInput.GetButton("Submit"))
            {
                dropTimer += Time.deltaTime;
                if (dropTimer > 2f)
                {
                    InventorySlot slot = data.Stats.Inventory.GetWeaponSlot();
                    if (slot != null)
                    {
                        data.Stats.Inventory.Drop(new Data.Items.InventoryDropParams
                        {
                            Slot = slot.Part
                        });
                        dropTimer = 0f;
                    }
                }
            }
            else
            {
                dropTimer = 0f;
            }

            if (movement)
            {
                float hAxis = RewiredInput.GetAxis("HorizontalMovement");
                float vAxis = RewiredInput.GetAxis("VerticalMovement");

                cameraForward = Camera.main.transform.forward;
                cameraRight = Camera.main.transform.right;
                
                Vector3 cFwd = cameraForward * vAxis + cameraRight * hAxis;
                cFwd.y = 0;

                movement.Direction = cFwd;

                if (RewiredInput.GetButtonDown("Dodge"))
                    (movement as CharacterMovementWalkDodge)?.Dodge(cFwd);
            }

            if ( Input.GetKeyDown(KeyCode.F6) ||
                RewiredInput.GetButtonDoublePressHold("Submit") )
            {
                data.Stats.Attributes.SetAttr(EAttribute.Vigor, 10000);
                data.Stats.Health = data.Stats.MaxHealth;
            }

        }

#if UNITY_EDITOR
        public override string GetDebugString()
        {
            return "Movement ref: " + (movement != null) + "\n" +
                   "Combat ref: " + (combat != null);
        }
#endif
    }
}