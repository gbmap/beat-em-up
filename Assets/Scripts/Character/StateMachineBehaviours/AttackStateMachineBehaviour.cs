﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Catacumba.Entity;

public class AttackStateMachineBehaviour : StateMachineBehaviour
{
    // int hashWeakAttack = Animator.StringToHash("WeakAttack");
    // int hashStrongAttack = Animator.StringToHash("StrongAttack");
    int hashAttackTrigger = Animator.StringToHash("AttackType");

    int[] heavyAttackHashes = {
        Animator.StringToHash("H"),
        Animator.StringToHash("H 0"),
        Animator.StringToHash("H 1")
    };

    CharacterCombat _combat;
    private CharacterCombat GetCombat(Animator animator)
    {
        return _combat ?? (_combat = animator.GetComponentInParent<CharacterCombat>());
    }

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(hashAttackTrigger);
        //animator.ResetTrigger(hashAttackTrigger);

        if (heavyAttackHashes.Contains(stateInfo.shortNameHash))
        {
            var combat = GetCombat(animator);
            if (!combat) return;
            combat.IsOnHeavyAttack = true;
        }
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var combat = GetCombat(animator);
        if (combat && heavyAttackHashes.Contains(stateInfo.shortNameHash))
            combat.IsOnHeavyAttack = false;
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        var combat = GetCombat(animator);
        if (combat)
            combat.OnComboStarted?.Invoke();
    }

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        var combat = GetCombat(animator);
        if (combat)
            combat.OnComboEnded?.Invoke();

        animator.ResetTrigger(hashAttackTrigger);
        animator.ResetTrigger(hashAttackTrigger);
    }
}
