using UnityEngine;
using Catacumba.Entity;

public class UseSkillStateMachineBehavior : StateMachineBehaviour
{
    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        var anim = animator.GetComponent<CharacterAnimator>();
        anim.OnStartUsingSkill?.Invoke(anim);
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        var anim = animator.GetComponent<CharacterAnimator>();
        anim.OnEndUsingSkill?.Invoke(anim);
    }
}
