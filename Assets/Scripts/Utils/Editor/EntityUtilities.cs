using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

namespace Catacumba
{
    [System.Serializable]
    public class WIDSpawnObject
    {
        public GameObject Prefab;

        [Range(0f, 1f)]
        public float Probability;
    }

    public class WizardIntoDamageableEditor : EditorWindow
    {

        public int TargetLayer;
        public GameObject OnDestroyParticleSystem;
        public WIDSpawnObject[] SpawnObjects;
        public ParticleSystem.MinMaxCurve VigorCurve;

        private bool UseCustomParticles = true;
        private bool SpawnObjectsOnDestroy = false;

        [MenuItem("Catacumba/Change Object/Into Damageable Wizard %l")]
        static void Init()
        {
            WizardIntoDamageableEditor window = (WizardIntoDamageableEditor)GetWindow(typeof(WizardIntoDamageableEditor));
            window.titleContent.text = "Change Object Into Damageable";
            window.position = new Rect(Vector2.zero, Vector2.one * 512);
            window.Show();
        }

        private void OnEnable()
        {
            TargetLayer = LayerMask.NameToLayer("Entities");
            VigorCurve = new ParticleSystem.MinMaxCurve(5f);
            OnDestroyParticleSystem = EntityUtilities.LoadAsset<GameObject>("PS_WoodExplosionLarge");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

            TargetLayer = EditorGUILayout.LayerField("Target Layer", TargetLayer);

            var so = new SerializedObject(this as ScriptableObject);
            SerializedProperty vigorProp = so.FindProperty("VigorCurve");
            EditorGUILayout.PropertyField(vigorProp);

            UseCustomParticles = EditorGUILayout.BeginToggleGroup("Use Custom Particle On Destroy", UseCustomParticles);
            if (UseCustomParticles)
            {
                OnDestroyParticleSystem = (GameObject)EditorGUILayout.ObjectField("On Destroy Particle System", OnDestroyParticleSystem, typeof(GameObject), false);
            }
            EditorGUILayout.EndToggleGroup();

            SpawnObjectsOnDestroy = EditorGUILayout.BeginToggleGroup("Spawn Object On Death", SpawnObjectsOnDestroy);
            if (SpawnObjectsOnDestroy)
            {
                SerializedProperty prop = so.FindProperty("SpawnObjects");
                EditorGUILayout.PropertyField(prop);
                
            }
            EditorGUILayout.EndToggleGroup();

            so.ApplyModifiedProperties();

            if (GUILayout.Button("Apply"))
            {
                EntityUtilities.ChangeIntoDamageable(new EntityUtilities.ChangeIntoDamageableParams(
                    TargetLayer,
                    OnDestroyParticleSystem,
                    SpawnObjects,
                    VigorCurve
                ));
            }
        }
    }

    public static class EntityUtilities
    {
        /*
        [MenuItem("Catacumba/Change Object/Into Damageable")]
        public static void ChangeIntoDamageable()
        {
            if (!EditorUtility.DisplayDialog("Change Object Into Damageable", "This action is not undo-able. Make sure everything is setup properly.", "Ok", "Cancel"))
            {
                return;
            }

            //var go = Selection.activeGameObject;
            var gos = Selection.gameObjects;
            foreach (var go in gos)
            {
                ChangeLayerAndMaterial(go, LayerMask.NameToLayer("Entities"));

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
        }
        */

        public class ChangeIntoDamageableParams
        {
            public int Layer;
            public GameObject ParticleSystem;
            public WIDSpawnObject[] SpawnObjects;
            public ParticleSystem.MinMaxCurve VigorCurve;

            public ChangeIntoDamageableParams()
            {
                Layer = LayerMask.NameToLayer("Entities");
                ParticleSystem = LoadAsset<GameObject>("PS_WoodExplosionLarge");
                SpawnObjects = new WIDSpawnObject[0];
                VigorCurve = new ParticleSystem.MinMaxCurve(5f);
            }

            public ChangeIntoDamageableParams(int l, GameObject particleSystem, WIDSpawnObject[] spawnObjects, ParticleSystem.MinMaxCurve vigorCurve)
            {
                Layer = l;
                ParticleSystem = particleSystem;
                SpawnObjects = spawnObjects;
                VigorCurve = vigorCurve;
            }
        }

        public static void ChangeIntoDamageable(ChangeIntoDamageableParams param)
        {
            if (!EditorUtility.DisplayDialog("Change Object Into Damageable", "This action is not undo-able. Make sure everything is setup properly.", "Ok", "Cancel"))
            {
                return;
            }

            //var go = Selection.activeGameObject;
            var gos = Selection.gameObjects;
            foreach (var go in gos)
            {
                ChangeLayerAndMaterial(go, param.Layer);

                if (!go.GetComponent<NavMeshObstacle>())
                    go.AddComponent<NavMeshObstacle>();

                if (!go.GetComponent<BoxCollider>())
                    go.AddComponent<BoxCollider>();

                if (!go.GetComponent<CharacterData>())
                {
                    var data = go.AddComponent<CharacterData>();
                    data.VigorCurve = param.VigorCurve;
                }

                if (!go.GetComponent<CharacterHealth>())
                {
                    go.AddComponent<CharacterHealth>();
                }

                if (!go.GetComponent<SpawnParticleSystemOnDestroy>())
                {
                    var spawnParticle = go.AddComponent<SpawnParticleSystemOnDestroy>();
                    spawnParticle.Particles = param.ParticleSystem;
                }

                if (param.SpawnObjects.Length > 0)
                {
                    foreach (var so in param.SpawnObjects)
                    {
                        var sood = go.AddComponent<SpawnObjectOnDeath>();
                        sood.ObjectPool = new GameObject[] { so.Prefab };
                        sood.Probability = so.Probability;
                    }
                }

            }
        }

        [MenuItem("Catacumba/Change Object/Into Interactable %k")]
        public static void ChangeIntoInteractable()
        {
            if (!EditorUtility.DisplayDialog("Change Object Into Interactable", "This action is not undo-able. Make sure everything is setup properly.", "Ok", "Cancel"))
            {
                return;
            }

            var go = Selection.activeGameObject;
            ChangeLayerAndMaterial(go, LayerMask.NameToLayer("Entities"));

            if (!go.GetComponent<BoxCollider>())
            {
                var trigger = go.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size *= 1.25f;
            }

            var interactable = go.GetComponent<Interactable>();
            if (!interactable)
            {
                interactable = go.AddComponent<Interactable>();
            }

            if (interactable.renderers == null || interactable.renderers.Length == 0)
            {
                interactable.renderers = interactable.GetComponentsInChildren<Renderer>();
            }

            if (!go.GetComponent<InteractableNeedsItem>())
            {
                go.AddComponent<InteractableNeedsItem>();
            }
        }

        private static void ChangeLayerAndMaterial(GameObject go, int layer)
        {
            var target = go;
            for (int i = -1; i < go.transform.childCount; i++)
            {
                if (i > -1)
                {
                    target = go.transform.GetChild(i).gameObject;
                }

                target.layer = layer;
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

        public static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
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