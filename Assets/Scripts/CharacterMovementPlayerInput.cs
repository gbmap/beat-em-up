using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterMovementPlayerInput : MonoBehaviour
{
    CharacterMovement _movement;
    CharacterCombat _combat;

    // Start is called before the first frame update
    void Awake()
    {
        _movement = GetComponent<CharacterMovement>();
        _combat = GetComponent<CharacterCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cFwd = Camera.main.transform.forward * Input.GetAxis("Vertical") +
            Camera.main.transform.right * Input.GetAxis("Horizontal");
        cFwd.y = 0;

        _movement.direction = cFwd;

        if (Input.GetKeyUp(KeyCode.Space))
        {
            _movement.Jump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            _combat.RequestAttack(EAttackType.Weak);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            _combat.RequestAttack(EAttackType.Strong);
        }
        
    }
}
