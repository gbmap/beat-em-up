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

        public static GameObject SpawnEnemy(
            Vector2Int cellPosition,
            LevelGenerationParams parameters, 
            CharacterPool pool=null
        ) {
            return SpawnEntityAtCellPosition(cellPosition, parameters, SpawnEnemy, pool);
        }

        ////////////////////////////////////////
        //      TRAPS

        [Command("spawn_trap")]
        public static GameObject SpawnTrap(
            CharacterConfiguration configuration, 
            Vector2Int position
        ) {
            Vector3  worldPosition = CalculateWorldPosition(position);
            return SpawnProp(configuration, worldPosition, false);
        }

        ////////////////////////////////////////
        //      PROPS

        [Command("spawn_prop")]
        public static GameObject SpawnProp(CharacterConfiguration configuration)
        {
            return SpawnProp(configuration, Vector3.zero);
        }

        [Command("spawn_prop")]
        public static GameObject SpawnProp(
            CharacterConfiguration configuration, 
            Vector2Int position
        ) {
            Vector3 worldPosition = CalculateWorldPosition(position);
            return SpawnProp(configuration, worldPosition);
        }

        [Command("spawn_prop")]
        public static GameObject SpawnProp(
            CharacterConfiguration configuration, 
            Vector3 worldPosition,
            bool addNavMeshObstacle = true
        ) {
            var instance = CreateEntityInstance(configuration, "Entity", "Entities", worldPosition);
            if (!instance) return null;

            if (addNavMeshObstacle)
            {
                NavMeshObstacle obs = instance.gameObject.AddComponent<NavMeshObstacle>();
            }
            return instance.gameObject;
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

            GameObject light = new GameObject("Light");
            Light l = light.AddComponent<Light>();
            l.range = 7f;
            l.intensity = 1f;
            l.color = new Color(0.4f, 0.2f, 0.3f);
            l.transform.parent = data.transform;
            l.transform.localPosition = Vector3.up * 1.25f;
            l.shadows = LightShadows.Hard;
            l.cullingMask = ~ (1 <<LayerMask.NameToLayer("Player"));

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

            //Debug.Log($"{name} created at {position}");

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
}