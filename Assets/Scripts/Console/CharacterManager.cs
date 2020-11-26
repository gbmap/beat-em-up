using UnityEngine;
using QFSW.QC;
using Catacumba.Data;
using Catacumba.Data.Controllers;
using Catacumba.LevelGen;
using UnityEngine.AI;
using System;

namespace Catacumba.Entity
{
    public class QCCharacterPoolParser : BasicQcParser<CharacterPool>
    {
        public override CharacterPool Parse(string value)
        {
            return CharacterManager.LoadPool(value);
        }
    }

    public class QCTransformParser : BasicQcParser<Transform>
    {
        public override Transform Parse(string value)
        {
            return GameObject.Find(value)?.transform;
        }
    }

    [CommandPrefix("entity.")]
    public static class CharacterManager
    {
        private static uint EntityCount = 0;
        public const string PATH_CHAR_POOLS = "Data/CharacterPools";

        ////////////////////////////////////////
        //      ENEMIES

        [Command("spawn_enemy")]
        public static GameObject SpawnEnemy(CharacterConfiguration configuration)
        {
            return SpawnEnemy(configuration, Vector3.zero);
        }

        [Command("spawn_enemy")]
        public static GameObject SpawnEnemy(CharacterConfiguration configuration, int x, int y)
        {
            Vector2Int pos = new Vector2Int(x, y);
            Vector3 worldPosition = CalculateWorldPosition(pos);
            return SpawnEnemy(configuration, worldPosition);
        }

        [Command("spawn_enemy")]
        public static GameObject SpawnEnemy(CharacterConfiguration configuration, Vector3 worldPosition)
        {
            CharacterData data = CreateEntityInstance(configuration, "Enemy", "Entities", worldPosition);
            ControllerComponent component = data.gameObject.AddComponent<ControllerComponent>();
            component.Controller = Resources.Load<ControllerAI>("Data/Controllers/ControllerAI");
            return data.gameObject;
        }

        public static GameObject SpawnEnemy(Vector2Int cellPosition, 
                                            LevelGenerationParams parameters, 
                                            CharacterPool pool=null)
        {
            return SpawnEntityAtCellPosition(cellPosition, parameters, SpawnEnemy, pool);
        }


        ////////////////////////////////////////
        //      PROPS

        [Command("spawn_prop")]
        public static GameObject SpawnProp(CharacterConfiguration configuration)
        {
            return SpawnProp(configuration, Vector3.zero);
        }

        [Command("spawn_prop")]
        public static GameObject SpawnProp(CharacterConfiguration configuration, Vector2Int position)
        {
            Vector3 worldPosition = CalculateWorldPosition(position);
            return SpawnProp(configuration, worldPosition);
        }

        [Command("spawn_prop")]
        public static GameObject SpawnProp(CharacterConfiguration configuration, Vector3 worldPosition)
        {
            return CreateEntityInstance(configuration, "Entity", "Entities", worldPosition)?.gameObject;
        }

        ////////////////////////////////////////
        //      PLAYER

        [Command("spawn_player")]
        public static GameObject SpawnPlayer(CharacterConfiguration configuration)
        {
            return SpawnPlayer(configuration, Vector3.zero);
        }

        [Command("spawn_player")]
        public static GameObject SpawnPlayer(CharacterConfiguration configuration, int x, int y)
        {
            return SpawnPlayer(configuration, CalculateWorldPosition(new Vector2Int(x, y)));
        }

        [Command("spawn_player")]
        public static GameObject SpawnPlayer(CharacterConfiguration configuration, Vector3 worldPosition)
        {
            CharacterData data = CreateEntityInstance(configuration, "Player", "Player", worldPosition);
            ControllerComponent component = data.gameObject.AddComponent<ControllerComponent>();
            component.Controller = Resources.Load<ControllerInput>("Data/Controllers/ControllerInputPlayer1");

            CharacterInteract interact = data.gameObject.AddComponent<CharacterInteract>();
            interact.TargetLayer = (1 << LayerMask.NameToLayer("Level")) | 
                                   (1 << LayerMask.NameToLayer("Item") | 
                                   (1 << LayerMask.NameToLayer("Entities")));
            return data.gameObject;
        }


        ////////////////////////////////////////
        //      MISC

        [Command("load_pool")]
        [CommandDescription("Loads a pool of characters that can be randomly picked.")]
        public static CharacterPool LoadPool(string name)
        {
            return Resources.Load<CharacterPool>($"{PATH_CHAR_POOLS}/{name}");
        }

        ////////////////////////////////////////
        //      UTILITIES

        private static GameObject SpawnEntityAtCellPosition(
            Vector2Int cellPosition, 
            LevelGenerationParams parameters,
            System.Func<CharacterConfiguration, Vector3, GameObject> SpawnFunction,
            CharacterPool pool=null)
        {
            return SpawnEntityAtCellPosition(cellPosition, parameters, SpawnFunction, Vector3.zero, pool);
        }

        private static GameObject SpawnEntityAtCellPosition(
            Vector2Int cellPosition, 
            LevelGenerationParams parameters,
            System.Func<CharacterConfiguration, Vector3, GameObject> SpawnFunction,
            Vector3 positionOffset,
            CharacterPool pool=null)
        {
            if (pool == null)
                pool = parameters.EnemyPool;

            CharacterPoolItem entity = pool.GetRandom();

            Vector3 worldPosition = LevelGen.Mesh.Utils.LevelToWorldPos(cellPosition, parameters.BiomeConfig.CellSize());
            worldPosition += positionOffset;
            worldPosition.y = 0f;

            /*
            NavMeshHit hit;
            if (NavMesh.SamplePosition(worldPosition, out hit, 5f, NavMesh.AllAreas))
            {
                worldPosition = hit.position;
            };
            */

            return SpawnFunction(entity.Config, worldPosition);
        }

        private static CharacterData CreateEntityInstance(CharacterConfiguration configuration, string tag, string layer, Vector3 position)
        {
            //string path = $"{CharacterConfiguration.DEFAULT_FOLDER}/{characterConfiguration}";
            //CharacterConfiguration configuration = Resources.Load<CharacterConfiguration>(path);
            if (!configuration)
            { 
                QuantumConsole.Instance.LogToConsole($"Couldn't load character.");
                return null;
            }
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, 5f, NavMesh.AllAreas))
            {
                position = hit.position;
            };

            string name = $"{configuration.name}_{tag}_{EntityCount}";
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

        private static Vector3 CalculateWorldPosition(Vector2Int position)
        {
            if (LevelGenerationManager.Level == null)
            {
                Debug.LogWarning("No level loaded, world position calculations are most likely wrong.");
                return Vector3.zero;
            }

            Vector3 cellSize = LevelGenerationManager.Params.BiomeConfig.CellSize();
            return LevelGen.Mesh.Utils.LevelToWorldPos(position, cellSize);
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