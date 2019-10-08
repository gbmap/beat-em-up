using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    [HideInInspector] public bool IsOnCombo;

    private int _nComboHits;

    public System.Action<EAttackType> OnRequestCharacterAttack;
    public System.Action<CharacterAttackData> OnCharacterAttack;
    public System.Action OnComboStarted;
    public System.Action OnComboEnded;

    private void OnEnable()
    {
        OnComboStarted += OnComboStartedCallback;
        OnComboEnded += OnComboEndedCallback;
    }

    private void OnDisable()
    {
        OnComboStarted -= OnComboStartedCallback;
        OnComboEnded -= OnComboEndedCallback;
    }

    public void RequestAttack(EAttackType type)
    {
        OnRequestCharacterAttack?.Invoke(type);
    }

    /*
    * CALLBACKS
    */
    private void OnComboStartedCallback()
    {
        IsOnCombo = true;
    }

    private void OnComboEndedCallback()
    {
        IsOnCombo = false;
        _nComboHits = 0;
    }

    /*
    *  CHAMADO PELO ANIMATOR!!!1111
    */
    public void Attack(EAttackType type)
    {
        Attack(new CharacterAttackData { type = type, attacker = gameObject, hitNumber = ++_nComboHits });
    }

    private void Attack(CharacterAttackData attack)
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position + transform.forward + Vector3.up, 
            Vector3.one*0.5f, 
            Quaternion.identity, 
            1 << LayerMask.NameToLayer("Entities")
        );

        foreach (var c in colliders)
        {
            if (c.gameObject == gameObject) continue;
            attack.defender = c.gameObject;
            CombatManager.Attack(gameObject, c.gameObject, ref attack);
            c.gameObject.GetComponent<CharacterHealth>()?.TakeDamage(attack);
        }

        OnCharacterAttack?.Invoke(attack);
    }

}
