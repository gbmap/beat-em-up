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

    // Update is called once per frame
    void Update()
    {
        Vector3 cFwd = Camera.main.transform.forward * _rewiredPlayer.GetAxis("VerticalMovement") +
            Camera.main.transform.right * _rewiredPlayer.GetAxis("HorizontalMovement");
        cFwd.y = 0;

        movement.Direction = cFwd;

        if (_rewiredPlayer.GetButtonUp("Jump"))
        {
            movement.Jump();
        }

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
            characterData.Interact();
        }
    }
}
