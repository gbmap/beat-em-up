using Catacumba.Character.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Catacumba.Character.AI
{
    public abstract class CharacterAIBaseMachine<T> : MonoBehaviour where T : System.Enum
    {
        protected NavMeshAgent navMeshAgent;

        protected CharacterAnimator characterAnimator;
        protected CharacterHealth characterHealth;
        protected CharacterCombat characterCombat;

        protected BaseState currentState;

        protected T currentAIState;
        protected T CurrentAIState
        {
            get { return currentAIState; }
            set
            {
                /*if (currentAIState.Equals(value)) return;*/

                characterAnimator.ResetAttackTrigger();

                if (currentState != null)
                {
                    currentState.OnExit();
                }

                currentAIState = value;
                currentState = CreateNewState(currentAIState, value);
                currentState.OnEnter();
            }
        }

        protected virtual void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();

            characterAnimator = GetComponent<CharacterAnimator>();
            characterHealth = GetComponent<CharacterHealth>();
            characterCombat = GetComponent<CharacterCombat>();
        }

        protected abstract BaseState CreateNewState(T previousState, T currentState);
        protected abstract void HandleStateResult(T state, StateResult result);


        /*
         * DEBUGGING
         * */
#if UNITY_EDITOR

        protected virtual void OnDrawGizmos()
        {
            if (navMeshAgent == null || !navMeshAgent.hasPath) return;

            var path = navMeshAgent.path;
            for (int i = 1; i < path.corners.Length; i++)
            {
                var a = path.corners[i - 1];
                var b = path.corners[i];
                Gizmos.DrawLine(a, b);
            }
        }


        protected virtual void OnGUI()
        {
            if (!Application.isEditor) return;

            Rect r = UIManager.WorldSpaceGUI(transform.position, Vector2.one * 100f);
            GUI.Label(r, "State: " + CurrentAIState);
        }
#endif

    }
}