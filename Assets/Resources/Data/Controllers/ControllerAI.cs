using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Controllers
{
    public abstract class ControllerAIState : ScriptableObject
    {
        public abstract int CurrentPriority { get; }

        public abstract int UpdatePriority(ControllerComponent component);

        public abstract void OnCreate(ControllerComponent component);
        public abstract void OnEnter(ControllerComponent component);
        public abstract void OnUpdate(ControllerComponent component, ref ControllerCharacterInput input);
        public abstract void OnExit(ControllerComponent component);
        public abstract void Destroy(ControllerComponent component);

        public virtual Vector3 GetMovementDirection(ControllerComponent component) 
        {
            return component.Data.Components.Movement.NavMeshAgent.desiredVelocity;
        }
    }

    [CreateAssetMenu(menuName="Data/Controllers/ControllerAI", fileName="ControllerAI")]
    public class ControllerAI : ControllerBase
    {
        public override ECharacterBrainType BrainType => ECharacterBrainType.AI;

        public List<ControllerAIState> States = new List<ControllerAIState>();

        protected ControllerAIState currentState;
        protected int CurrentStatePriority 
        {
            get { return currentState.CurrentPriority; }
        }

        protected int currentStateIndex;

        public override void Destroy(ControllerComponent controller)
        {
            currentState?.OnExit(controller);

            foreach (ControllerAIState state in States)
                state.Destroy(controller);
        }

        public override void OnUpdate(ControllerComponent controller, ref ControllerCharacterInput input)
        {
            int maxPriority = CurrentStatePriority; 
            int maxPriorityIndex = currentStateIndex;
            for (int stateIndex = 0; stateIndex < States.Count; stateIndex++)
            {
                ControllerAIState state = States[stateIndex];
                int priority = state.UpdatePriority(controller);
                if (priority > maxPriority && currentStateIndex != stateIndex)
                {
                    maxPriority = priority;
                    maxPriorityIndex = stateIndex;
                }
            }

            if (maxPriorityIndex != currentStateIndex)
                ChangeState(controller, States[maxPriorityIndex], maxPriorityIndex);

            currentState.OnUpdate(controller, ref input);
        }

        public override void Setup(ControllerComponent controller)
        {
            List<ControllerAIState> statesBlueprint = States;
            States = new List<ControllerAIState>();

            foreach(ControllerAIState state in statesBlueprint)
            {
                var stateInstance = Instantiate(state);
                stateInstance.OnCreate(controller);
                States.Add(stateInstance);
            }

            currentState = States[0];
            currentStateIndex = 0;
        }

        private ControllerAIState ChangeState(ControllerComponent controller, ControllerAIState newState, int stateIndex)
        {
            currentState?.OnExit(controller);
            currentState = newState;
            currentStateIndex = stateIndex;
            currentState.OnEnter(controller);
            return currentState;
        }
    }
}
