using UnityEngine;

namespace Catacumba.Character.AI
{
    public enum ERandomSkillAIStates
    {
        UseSkill
    }

    public class CharacterAIRandomSkill : CharacterAIBaseMachine<ERandomSkillAIStates>
    {
        [Range(1, 6)]
        public int NumberOfSkills = 6;
        private int lastSkill;

        [Range(1f, 10f)]
        public float Cooldown = 2f;
        private float lastSkillUsed = 0f;

        protected override void Update()
        {
            base.Update();

            if (currentState == null && Time.time > lastSkillUsed + Cooldown)
            {
                SetCurrentState(ERandomSkillAIStates.UseSkill, Random.Range(0, NumberOfSkills));
            }
        }

        protected override BaseState CreateNewState(ERandomSkillAIStates previousState, ERandomSkillAIStates currentState, params object[] data)
        {
            return new UseSkillState(gameObject, (int)data[0]);
        }

        protected override void HandleStateResult(ERandomSkillAIStates state, StateResult result)
        {
            switch (state)
            {
                case ERandomSkillAIStates.UseSkill:
                    switch (result.code)
                    {
                        case UseSkillState.RES_CONTINUE:
                            break;
                        case UseSkillState.RES_CASTING:
                            break;
                        case UseSkillState.RES_CASTED:
                            lastSkillUsed = Time.time;
                            currentState = null;
                            break;
                    }
                    break;
            }
        }
    }
}