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

    float horizontalAxis;
    float verticalAxis;

    float lastHorizontalAxis;
    float lastVerticalAxis;

    bool[] isPressing = { false, false };
    bool[] isPressingCache = { false, false };

    float[] lastPress = { 0f, 0f };
    float[] lastDoublePress = { 0f, 0f };

    float doublePressTime = 0.25f;

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
        float hAxis = _rewiredPlayer.GetAxis("HorizontalMovement");
        float vAxis = _rewiredPlayer.GetAxis("VerticalMovement");

        Vector3 cFwd = Camera.main.transform.forward * vAxis +
            Camera.main.transform.right * hAxis;
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
            characterData.Interact();
        }

        lastHorizontalAxis = horizontalAxis;
        lastVerticalAxis = verticalAxis;

        horizontalAxis = hAxis;
        verticalAxis = vAxis;

        isPressingCache[0] = isPressing[0];
        isPressingCache[1] = isPressing[1];
        isPressing[0] = IsPressing(horizontalAxis, lastHorizontalAxis);
        isPressing[1] = IsPressing(verticalAxis, lastVerticalAxis);

        // 0 ou 1
        for (int axis = 0; axis < 2; axis++)
        {
            if (!AxisTappedDown(isPressing[axis], isPressingCache[axis]))
            {
                continue;
            }

            if (Time.time < lastPress[axis] + doublePressTime) // double tap
            {
                Debug.Log("Double tap");
                lastDoublePress[axis] = Time.time;

                movement.Roll();
            }

            Debug.Log("Tap");
            lastPress[axis] = Time.time;
        }
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

    private void OnGUI()
    {
        GUILayout.TextArea("horizontal: " + isPressing[0] + " vertical: " + isPressing[1]);
        GUILayout.TextArea("horizontal: " + isPressingCache[0] + " vertical: " + isPressingCache[1]);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (IsPressing(horizontalAxis, lastHorizontalAxis) || IsPressing(verticalAxis, lastVerticalAxis))
        {
            Gizmos.color = Color.green;
        }

        for (int i = 0; i < 1; i++)
        {
            if (Time.time < lastDoublePress[i] + 0.25f)
            {
                Gizmos.color = Color.yellow;
                break;
            }
        }

        Vector3 a = transform.position + transform.up * transform.localScale.y;
        Vector3 b = a - new Vector3(horizontalAxis, 0f, verticalAxis);
        Gizmos.DrawLine(a, b);
    }
}
