using UnityEngine;
using Catacumba.Effects;

namespace Catacumba.Data
{
    [CreateAssetMenu()]
    public class CharacterViewConfiguration : ScriptableObject
    {
        public GameObject[] Models;
        public AnimationConfig AnimationConfig;

        public ParticleEffectConfiguration DamageEffect;
        public ParticleEffectConfiguration MovementEffect;

        public GameObject GetRandomModel()
        {
            return Models[Random.Range(0, Models.Length)];
        }

        public void Configure(Entity.CharacterData character, int modelIndex = -1)
        {
            GameObject instance = character.gameObject;

            RemoveExistingModel(instance);
            GameObject modelPrefab = SelectModel(modelIndex);
            GameObject modelInstance = AddModelToInstance(instance, modelPrefab);

            if (AnimationConfig != null)
                SetupModelAnimator(modelInstance, AnimationConfig);
        }

        private static void RemoveExistingModel(GameObject instance)
        {
            for (int i = 0; i < instance.transform.childCount; i++)
            {
                var child = instance.transform.GetChild(i);
                if (child.name.Contains("Character_") ||
                    child.name.Equals("Root"))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private GameObject SelectModel(int modelIndex)
        {
            if (modelIndex == -1)
                return GetRandomModel();
            return Models[Mathf.Clamp(modelIndex, 0, Models.Length)];
        }

        private GameObject AddModelToInstance(GameObject instance, GameObject model)
        {
            GameObject modelInstance = Instantiate(
                model, 
                Vector3.zero, 
                Quaternion.identity, 
                instance.transform
            );
            modelInstance.transform.localPosition = Vector3.zero; // Somehow Instantiate with Vector3.zero is not working.

            return modelInstance;
        }

        private void SetupModelAnimator(GameObject modelInstance, AnimationConfig animationConfig)
        {
            var anim = modelInstance.GetComponent<Animator>();
            if (anim == null)
                anim = modelInstance.AddComponent<Animator>();

            anim.runtimeAnimatorController = animationConfig.AnimatorController;
            anim.avatar = animationConfig.Avatar;
            anim.applyRootMotion = false;

            // maybe we should remove this
            modelInstance.AddComponent<Entity.CharacterAnimator>();
        }
    }
}