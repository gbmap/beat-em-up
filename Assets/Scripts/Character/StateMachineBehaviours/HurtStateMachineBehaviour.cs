using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Catacumba.Entity;

public class HurtStateMachineBehaviour : StateMachineBehaviour
{
    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.shortNameHash == Animator.StringToHash("Knockdown"))
        {
            Debug.Log("Fall");
            animator.GetComponentInParent<CharacterHealth>().OnFall?.Invoke();
        }
        else
        {
            //animator.GetComponentInParent<CharacterAnimator>().FreezeAnimator();
        }
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.shortNameHash == Animator.StringToHash("StandUp"))
        {
            Debug.Log("GetUp");
            animator.GetComponentInParent<CharacterHealth>().OnGetUp?.Invoke();
        }
    }

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        animator.GetComponentInParent<CharacterHealth>().IsBeingDamaged = true;
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        //base.OnStateMachineExit(animator, stateMachinePathHash);
        animator.GetComponentInParent<CharacterHealth>().IsBeingDamaged = false;
    }
}
