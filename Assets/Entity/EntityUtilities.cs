using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

namespace Catacumba
{
    public static class EntityUtilities
    {
        [MenuItem("Catacumba/Change Object/Into Damageable %l")]
        public static void ChangeIntoDamageable()
        {
            var go = Selection.activeGameObject;
            //go.layer = LayerMask.NameToLayer("Entities");

            ChangeLayerAndMaterial(go);

            if (!go.GetComponent<NavMeshObstacle>())
                go.AddComponent<NavMeshObstacle>();

            if (!go.GetComponent<BoxCollider>())
                go.AddComponent<BoxCollider>();

            if (!go.GetComponent<CharacterData>())
            {
                var data = go.AddComponent<CharacterData>();
                data.VigorCurve = new ParticleSystem.MinMaxCurve(1f);
            }

            if (!go.GetComponent<CharacterHealth>())
            {
                go.AddComponent<CharacterHealth>();
            }

            if (!go.GetComponent<SpawnParticleSystemOnDestroy>())
            {
                var spawnParticle = go.AddComponent<SpawnParticleSystemOnDestroy>();
                spawnParticle.Particles = LoadAsset<GameObject>("PS_WoodExplosionLarge");

            }
        }

        [MenuItem("Catacumba/Change Object/Into Interactable")]
        public static void ChangeIntoInteractable()
        {
            var go = Selection.activeGameObject;
            ChangeLayerAndMaterial(go);

            if (!go.GetComponent<BoxCollider>())
            {
                var trigger = go.AddComponent<BoxCollider>();
                trigger.size *= 1.25f;
            }

            if (!go.GetComponent<Interactable>())
            {
                go.AddComponent<Interactable>();
            }

            if (!go.GetComponent<InteractableNeedsItem>())
            {
                go.AddComponent<InteractableNeedsItem>();
            }
        }

        private static void ChangeLayerAndMaterial(GameObject go)
        {
            var target = go;
            for (int i = -1; i < go.transform.childCount; i++)
            {
                if (i > -1)
                {
                    target = go.transform.GetChild(i).gameObject;
                }

                target.layer = LayerMask.NameToLayer("Entities");
                var renderer = target.GetComponent<Renderer>();

                var currentName = renderer.sharedMaterial.name;
                if (!currentName.Contains("_Damageable"))
                {

                    currentName = currentName.Replace(" (Instance)", string.Empty);

                    string materialName = currentName + "_Damageable";

                    var materials = AssetDatabase.FindAssets(materialName);
                    if (materials.Length == 0)
                    {
                        Debug.LogError("No equivalent damageable material! Duplicate the current material e set its shader to Shader Graphs/M_Character");
                        return;
                    }

                    var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materials[0]));
                    renderer.sharedMaterial = material;
                }
            }
        }

        [MenuItem("Catacumba/Change Object/Drop Item On Death")]
        public static void DropItemOnDeath()
        {
            var go = Selection.activeGameObject;
            go.AddComponent<SpawnObjectOnDeath>();
        }

        [MenuItem("Catacumba/Change Object/Drop Small Food On Death")]
        public static void DropFoodSmall()
        {
            DropObject("Food_Small", 0.5f);
        }

        [MenuItem("Catacumba/Change Object/Drop Medium Food On Death")]
        public static void DropFoodMedium()
        {
            DropObject("Food_Medium", 0.5f);
        }

        public static void DropObject(string prefabName, float probability)
        {
            var prefab = LoadAsset<GameObject>(prefabName);
            var go = Selection.activeGameObject;
            var so = go.AddComponent<SpawnObjectOnDeath>();
            so.ObjectPool = new GameObject[] { prefab };
            so.Probability = probability;
        }

        private static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            var assetsFound = AssetDatabase.FindAssets(assetName);
            if (assetsFound.Length == 0)
            {
                Debug.LogError("No Asset found as " + assetName);
                return default(T);
            }

            var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(assetsFound[0]));
            return asset;
        }

    }
}