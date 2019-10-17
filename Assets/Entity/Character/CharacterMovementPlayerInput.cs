using Rewired;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterMovementPlayerInput : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    public int PlayerIndex
    {
        get { return playerIndex; }
        set { _rewiredPlayer = ReInput.players.GetPlayer(playerIndex = value); }
    }

    CharacterMovement _movement;
    CharacterCombat _combat;

    Player _rewiredPlayer;

    // Start is called before the first frame update
    void Awake()
    {
        _movement = GetComponent<CharacterMovement>();
        _combat = GetComponent<CharacterCombat>();
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

        _movement.direction = cFwd;

        if (_rewiredPlayer.GetButtonUp("Jump"))
        {
            _movement.Jump();
        }

        if (_rewiredPlayer.GetButtonDown("WeakAttack"))
        {
            _combat.RequestAttack(EAttackType.Weak);
        }
        else if (_rewiredPlayer.GetButtonDown("StrongAttack"))
        {
            _combat.RequestAttack(EAttackType.Strong);
        }
        
    }
}
