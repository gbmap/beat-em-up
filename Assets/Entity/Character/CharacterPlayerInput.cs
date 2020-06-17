﻿using System;
using Catacumba;
using Catacumba.Exploration;
using Rewired;
using UnityEngine;

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

    float horizontalAxis;
    float verticalAxis;

    float lastHorizontalAxis;
    float lastVerticalAxis;

    public System.Action<CharacterData> OnInteract;

    private Vector3 cameraForward;
    private Vector3 cameraRight;
    private bool updateCameraDir = true;

    private float dropTimer;

    // Start is called before the first frame update
    void Awake()
    {
        characterData = GetComponent<CharacterData>();
        movement = GetComponent<CharacterMovement>();
        combat = GetComponent<CharacterCombat>();
    }

    void Start()
    {
        // Get first player as default
        //_rewiredPlayer = ReInput.players.GetPlayer(0);
        PlayerIndex = playerIndex;
    }

    private void OnEnable()
    {
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
            cameraForward = Vector3.Lerp(cameraForward, Camera.main.transform.forward, Time.deltaTime*0.5f);
            cameraRight = Vector3.Lerp(cameraRight, Camera.main.transform.right, Time.deltaTime*0.5f);
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
    }

    private void UpdateCameraDir()
    {
        MovementOrientation mo = CameraManager.Instance.MovementOrientation;
        cameraRight = mo.right;
        cameraForward = mo.forward;
    }

    bool AxisTappedDown(bool[] axis, bool[] axisCache)
    {
        return (axis[0] && !axisCache[0]) || (axis[1] && !axisCache[1]);
    }

    bool AxisTappedDown(bool axis, bool axisCache)
    {
        return axis && !axisCache;
    }

    bool IsPressing(float axis, float lastAxis)
    {
        float absAxis = Mathf.Abs(axis);
        float absLastAxis = Mathf.Abs(lastAxis);

        return absAxis > absLastAxis || Mathf.Approximately(absAxis + absLastAxis, 2f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (IsPressing(horizontalAxis, lastHorizontalAxis) || IsPressing(verticalAxis, lastVerticalAxis))
        {
            Gizmos.color = Color.green;
        }

        Vector3 a = transform.position + transform.up * transform.localScale.y;
        Vector3 b = a - new Vector3(horizontalAxis, 0f, verticalAxis);
        Gizmos.DrawLine(a, b);

        if (!Application.isPlaying) return;

        MovementOrientation mo = CameraManager.Instance.MovementOrientation;

        Gizmos.DrawLine(transform.position, transform.position + mo.right);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + mo.forward);
    }
}
