using System;
using Catacumba;
using Catacumba.Exploration;
using Rewired;
using UnityEngine;


namespace Catacumba.Entity 
{

[RequireComponent(typeof(CharacterMovement))]
public class CharacterPlayerInput : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    public int PlayerIndex
    {
        get { return playerIndex; }
        set { _rewiredPlayer = ReInput.players.GetPlayer(playerIndex = value); }
    }

    private CharacterData characterData;
    private CharacterMovement movement;
    private CharacterCombat combat;

    Player _rewiredPlayer;
    public Player RewiredInput { get { return _rewiredPlayer; } }

    public System.Action<CharacterData> OnInteract;

    private Vector3 cameraForward;
    private Vector3 cameraRight;
    private bool updateCameraDir = true;

    private float dropTimer;

    private bool useCameraManager;

    // Start is called before the first frame update
    void Awake()
    {
        characterData = GetComponent<CharacterData>();
        movement = GetComponent<CharacterMovement>();
        combat = GetComponent<CharacterCombat>();

        useCameraManager = FindObjectOfType<CameraManager>() != null;
    }

    void Start()
    {
        // Get first player as default
        //_rewiredPlayer = ReInput.players.GetPlayer(0);
        PlayerIndex = playerIndex;
    }

    private void OnEnable()
    {
        if (useCameraManager)
            CameraManager.Instance.OnCameraChange += OnCameraChange;
    }

    private void OnDisable()
    {
        movement.Direction = Vector3.zero;

        //if (!CameraManager.Instance) return;
        //CameraManager.Instance.OnCameraChange -= OnCameraChange;
    }

    private void OnCameraChange()
    {
        updateCameraDir = false;
    }

    // Update is called once per frame
    void Update()
    {
        float hAxis = _rewiredPlayer.GetAxis("HorizontalMovement");
        float vAxis = _rewiredPlayer.GetAxis("VerticalMovement");

        if (useCameraManager)
        {

            if (updateCameraDir)
            {
                UpdateCameraDir();
            }
            else if (Mathf.Abs(hAxis) < 0.2f && Mathf.Abs(vAxis) < 0.2f)
            {
                updateCameraDir = true;
            }
            else
            {
                cameraForward = Vector3.Lerp(cameraForward, Camera.main.transform.forward, Time.deltaTime * 0.5f);
                cameraRight = Vector3.Lerp(cameraRight, Camera.main.transform.right, Time.deltaTime * 0.5f);
            }
        }
        else
        {
            cameraForward = Camera.main.transform.forward;
            cameraRight = Camera.main.transform.right;
        }
        
        Vector3 cFwd = cameraForward * vAxis + cameraRight * hAxis;
        cFwd.y = 0;

        movement.Direction = cFwd;

        if (_rewiredPlayer.GetButtonDown("WeakAttack"))
        {
            combat.RequestAttack(EAttackType.Weak);
        }
        else if (_rewiredPlayer.GetButtonDown("StrongAttack"))
        {
            combat.RequestAttack(EAttackType.Strong);
        }

        if (_rewiredPlayer.GetButtonDown("Submit"))
        {
            OnInteract?.Invoke(characterData);
            characterData.Interact();
        }

        if (_rewiredPlayer.GetButton("Submit"))
        {
            if (characterData.Stats.Inventory.HasEquip(EInventorySlot.Weapon))
            {
                dropTimer += Time.deltaTime;
                if (dropTimer > 2f)
                {
                    characterData.UnEquip(EInventorySlot.Weapon);
                    dropTimer = 0f;
                }
            }
        }
        else
        {
            dropTimer = 0f;
        }

        if (_rewiredPlayer.GetButtonDown("Dodge"))
        {
            movement.Roll(cFwd.normalized);
        }

        if ( Input.GetKeyDown(KeyCode.F6) ||
            _rewiredPlayer.GetButtonDoublePressHold("Submit") )
        {
            characterData.Stats.Attributes.SetAttr(EAttribute.Vigor, 10000);
            characterData.Stats.Health = characterData.Stats.MaxHealth;
        }

    }

    private void UpdateCameraDir()
    {
        MovementOrientation mo = CameraManager.Instance.MovementOrientation;
        cameraRight = mo.right;
        cameraForward = mo.forward;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        MovementOrientation mo = CameraManager.Instance.MovementOrientation;

        Gizmos.DrawLine(transform.position, transform.position + mo.right);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + mo.forward);
    }
}
}