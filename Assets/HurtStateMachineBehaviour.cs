using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtStateMachineBehaviour : StateMachineBehaviour
{
    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.shortNameHash == Animator.StringToHash("HurtFall"))
        {
            Debug.Log("Fall");
            animator.GetComponent<CharacterHealth>().OnFall?.Invoke();
        }
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.shortNameHash == Animator.StringToHash("StandUp"))
        {
            Debug.Log("GetUp");
            animator.GetComponent<CharacterHealth>().OnGetUp?.Invoke();
        }
    }
}
