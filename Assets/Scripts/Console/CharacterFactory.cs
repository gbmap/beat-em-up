using UnityEngine;
using QFSW.QC;
using Catacumba.Data;
using Catacumba.Data.Controllers;
using Catacumba.LevelGen;
using UnityEngine.AI;

namespace Catacumba.Entity
{
    [CommandPrefix("entity.")]
    public static class CharacterFactory
    {
        private static uint EntityCount = 0;

        [Command("spawn_enemy")]
        public static GameObject SpawnEnemy(string characterConfiguration)
        {
            return SpawnEnemy(characterConfiguration, Vector3.zero);
        }

        [Command("spawn_enemy")]
        public static GameObject SpawnEnemy(string characterConfiguration, Vector3 worldPosition)
        {
            CharacterData data = CreateEntityInstance(characterConfiguration, "Enemy", "Entities", worldPosition);
            ControllerComponent component = data.gameObject.AddComponent<ControllerComponent>();
            component.Controller = Resources.Load<ControllerAI>("Data/Controllers/ControllerAI");
            return data.gameObject;
        }

        public static GameObject SpawnEnemy(Vector2Int cellPosition, LevelGenerationParams parameters, CharacterPool pool=null)
        {
            return SpawnEntityAtCellPosition(cellPosition, parameters, SpawnEnemy, pool);
        }

        private static GameObject SpawnEntityAtCellPosition(
            Vector2Int cellPosition, 
            LevelGenerationParams parameters,
            System.Func<string, Vector3, GameObject> SpawnFunction,
            CharacterPool pool=null)
        {
            if (pool == null)
                pool = parameters.EnemyPool;

            CharacterPoolItem entity = pool.GetRandom();

            Vector3 worldPosition = LevelGen.Mesh.Utils.LevelToWorldPos(cellPosition, parameters.BiomeConfig.CellSize());
            worldPosition -= parameters.BiomeConfig.CellSize()/2f;
            worldPosition.y = 0f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(worldPosition, out hit, 5f, NavMesh.AllAreas))
            {
                worldPosition = hit.position;
            };

            return SpawnFunction(entity.Name, worldPosition);
        }

        [Command("spawn_prop")]
        public static GameObject SpawnProp(string configuration)
        {
            return SpawnProp(configuration, Vector3.zero);
        }

        [Command("spawn_prop")]
        public static GameObject SpawnProp(string configuration, Vector3 worldPosition)
        {
            return CreateEntityInstance(configuration, "Entity", "Entities", worldPosition)?.gameObject;
        }

        public static GameObject SpawnProp(Vector2Int cellPosition, LevelGenerationParams parameters, CharacterPool pool=null)
        {
            return SpawnEntityAtCellPosition(cellPosition, parameters, SpawnProp, pool);
        }

        [Command("spawn_player")]
        public static GameObject SpawnPlayer(string characterConfiguration)
        {
            return SpawnPlayer(characterConfiguration, Vector3.zero);
        }

        [Command("spawn_player")]
        public static GameObject SpawnPlayer(string characterConfiguration, Vector3 worldPosition)
        {
            CharacterData data = CreateEntityInstance(characterConfiguration, "Player", "Player", worldPosition);
            ControllerComponent component = data.gameObject.AddComponent<ControllerComponent>();
            component.Controller = Resources.Load<ControllerInput>("Data/Controllers/ControllerInputPlayer1");
            return data.gameObject;
        }

        private static CharacterData CreateEntityInstance(string characterConfiguration, string tag, string layer, Vector3 position)
        {
            string path = $"{CharacterConfiguration.DEFAULT_FOLDER}/{characterConfiguration}";
            CharacterConfiguration configuration = Resources.Load<CharacterConfiguration>(path);
            if (!configuration)
            { 
                QuantumConsole.Instance.LogToConsole($"Couldn't load character: {characterConfiguration}");
                return null;
            }

            string name = $"{characterConfiguration}_{tag}_{EntityCount}";
            GameObject instance = new GameObject(name);
            instance.transform.position      = position;
            instance.transform.localRotation = Quaternion.identity;
            instance.layer                   = LayerMask.NameToLayer(layer);
            instance.tag                     = tag;

            CharacterData data = instance.AddComponent<CharacterData>();
            configuration.Configure(data);

            Debug.Log($"{name} created at {position}");

            EntityCount++;
            return data;
        }
    }

    public class QCTransformParser : BasicQcParser<Transform>
    {
        public override Transform Parse(string value)
        {
            return GameObject.Find(value)?.transform;
        }
    }

    [CommandPrefix("camera.")]
    public static class CameraManager
    {
        private static GameObject CameraObject;
        private static GameObject VirtualCameraObject;
        private static Cinemachine.CinemachineVirtualCamera VirtualCamera;
        private static Cinemachine.CinemachineTransposer Transposer;

        [Command("follow_target")] 
        public static Transform Follow
        {
            get { return VirtualCamera.Follow; }
            set { VirtualCamera.Follow = value; }
        }

        [Command("follow_offset")]
        public static Vector3 FollowOffset
        {
            get { return Transposer.m_FollowOffset; }
            set { Transposer.m_FollowOffset = value; }

        }

        [Command("look_target")]  
        public static Transform LookAt
        {
            get { return VirtualCamera.LookAt; }
            set { VirtualCamera.LookAt = value; }
        }

        [Command("target")]
        public static Transform Target
        {
            set 
            { 
                Follow = value;
                LookAt = value;  
            }
        }

        [Command("spawn")]
        public static GameObject Spawn()
        {
            CameraObject = Camera.main?.gameObject;
            if (CameraObject == null)
            {
                CameraObject = new GameObject("MainCamera");
                CameraObject.AddComponent<Camera>();
                CameraObject.tag = "MainCamera";
            }

            if (VirtualCameraObject)
                GameObject.Destroy(VirtualCameraObject);

            VirtualCameraObject = new GameObject("VCam");
            VirtualCamera = VirtualCameraObject.AddComponent<Cinemachine.CinemachineVirtualCamera>();

            Transposer = VirtualCamera.AddCinemachineComponent<Cinemachine.CinemachineTransposer>();
            Transposer.m_BindingMode = Cinemachine.CinemachineTransposer.BindingMode.WorldSpace;
            Transposer.m_FollowOffset = new Vector3(0f, 16f, -9f);

            var aim = VirtualCamera.AddCinemachineComponent<Cinemachine.CinemachineComposer>();
            return VirtualCameraObject;
        }
    }
}