using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Catacumba.Entity;

public class CharacterAnimatorStateMachine : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.shortNameHash == Animator.StringToHash("Roll"))
        {
            animator.GetComponent<CharacterMovement>().BeginRoll();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.shortNameHash == Animator.StringToHash("Roll"))
        {
            animator.GetComponent<CharacterMovement>().EndRoll();
        }
    }
}
