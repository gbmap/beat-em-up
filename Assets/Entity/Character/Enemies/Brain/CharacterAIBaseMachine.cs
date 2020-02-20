﻿using Catacumba.Character.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Catacumba.Character.AI
{
    public abstract class CharacterAIBaseMachine<T> : MonoBehaviour where T : System.Enum
    {
        protected CharacterAnimator animator;
        protected CharacterHealth health;
        protected CharacterCombat combat;
        protected CharacterData data;
        protected CharacterMovement movement;

        protected BaseState currentState;

        protected T CurrentAIState
        {
            get; private set;
        }

        protected virtual void SetCurrentState(T value, params object[] data)
        {
            animator.ResetAttackTrigger();

            if (currentState != null)
            {
                currentState.OnExit();
            }

            currentState = CreateNewState(CurrentAIState, value, data);
            CurrentAIState = value;
            currentState.OnEnter();
        }

        protected virtual void Awake()
        {
            animator = GetComponent<CharacterAnimator>();
            health = GetComponent<CharacterHealth>();
            combat = GetComponent<CharacterCombat>();
            data = GetComponent<CharacterData>();
            movement = GetComponent<CharacterMovement>();
        }

        protected virtual void Update()
        {
            if (currentState != null && !health.IsOnGround)
            {
                StateResult result = currentState.Update();
                HandleStateResult(CurrentAIState, result);
            }
        }

        protected abstract BaseState CreateNewState(T previousState, T currentState, params object[] data);
        protected abstract void HandleStateResult(T state, StateResult result);

        /*
         * DEBUGGING
         * */
#if UNITY_EDITOR

        protected virtual void OnDrawGizmos()
        {
            if (movement == null || !movement.HasPath) return;

            var path = movement.NavMeshAgent.path;
            for (int i = 1; i < path.corners.Length; i++)
            {
                var a = path.corners[i - 1];
                var b = path.corners[i];
                Gizmos.DrawLine(a, b);
            }

            if (currentState != null)
            {
                currentState.OnDebugDrawGizmos();
            }
        }


        protected virtual void OnGUI()
        {
            if (!Application.isEditor) return;

            Rect r = UIManager.WorldSpaceGUI(transform.position, Vector2.one * 100f);
            GUI.Label(r, "State: " + CurrentAIState);

            if (currentState != null)
            {
                currentState.OnGUI();
            }
        }
#endif

    }
}