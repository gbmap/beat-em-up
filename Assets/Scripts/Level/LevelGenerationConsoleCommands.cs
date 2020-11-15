using UnityEngine;
using QFSW.QC;
using static Catacumba.LevelGen.LevelGenerationParams;
using System;
using Catacumba.Data.Level;
using Catacumba.Data;
using Catacumba.Entity;

namespace Catacumba.LevelGen
{
    public class QCBiomeParser : BasicQcParser<BiomeConfiguration>
    {
        public override BiomeConfiguration Parse(string value)
        {
            return LevelGenerationConsoleCommands.LoadBiome(value);
        }
    }

    public class QCCharacterPoolParser : BasicQcParser<CharacterPool>
    {
        public override CharacterPool Parse(string value)
        {
            return LevelGenerationConsoleCommands.LoadPool(value);
        }
    }

    [CommandPrefix("level.")]
    public static class LevelGenerationConsoleCommands
    {
        [Command("type")]         public static ELevelType LevelType = ELevelType.Dungeon;
        [Command("sz")]           public static Vector2Int LevelSize = new Vector2Int(30, 30);
        [Command("prop_chance")]  public static float PropChance = 0.65f;
        [Command("enemy_chance")] public static float EnemyChance = 0.65f;
        
        private static BiomeConfiguration _biomeConfig;
        [Command("biome")]        public static BiomeConfiguration BiomeConfig
        {
            get { return _biomeConfig ?? (_biomeConfig = LoadBiome("BiomeDungeon")); }
            set { _biomeConfig = value; }
        }

        private static CharacterPool _characterPool;
        [Command("enemy_pool")]   public static CharacterPool EnemyPool
        {
            get { return _characterPool ?? (_characterPool = LoadPool("CharacterPool_Goblins")); }
            set { _characterPool = value; }
        }

        public const string PATH_BIOMES     = "Data/Level/Biomes";
        public const string PATH_CHAR_POOLS = "Data/CharacterPools";

        private static Level Level;
        private static LevelGenerationParams Params;
        private static GameObject LevelObject;

        [Command("load_biome")]
        public static BiomeConfiguration LoadBiome(string name)
        {
            return Resources.Load<BiomeConfiguration>($"{PATH_BIOMES}/{name}");
        }

        [Command("load_character_pool")]
        public static CharacterPool LoadPool(string name)
        {
            return Resources.Load<CharacterPool>($"{PATH_CHAR_POOLS}/{name}");
        }

        [Command("create")]
        public static void CreateLevel()
        {
            System.Action<Level, LevelGenerationParams> OnLevelGenerated = (level, parameters) =>
            {
                Level = level;
                Params = parameters;
                GenerateMesh();
                SpawnEnemies();
                SpawnPlayer("Goblin_Archer");
            };

            LevelGeneration.Generate(new LevelGenerationParams
            {
                LevelType   = LevelType,
                LevelSize   = LevelSize,
                PropChance  = PropChance,
                EnemyChance = EnemyChance,
                BiomeConfig = BiomeConfig,
                EnemyPool   = EnemyPool
            }, (level, parameters) => { Level = level; Params = parameters; GenerateMesh(); SpawnEnemies(); });
        }

        [Command("generate")]
        public static void Generate()
        {
            LevelGeneration.Generate(new LevelGenerationParams
            {
                LevelType   = LevelType,
                LevelSize   = LevelSize,
                PropChance  = PropChance,
                EnemyChance = EnemyChance,
                BiomeConfig = BiomeConfig,
                EnemyPool   = EnemyPool
            }, (level, parameters) => { Level = level; Params = parameters; });
        }

        [Command("spawn")]
        [CommandDescription("Spawns object for current generated level.")]
        public static void GenerateMesh()
        {
            Log("Generating level geometry...");
            if (Level == null)
            {
                Debug.Log("No level generated. Generate level before spawning.");
                return;
            }

            GameObject existingLevel = GameObject.Find("Level");
            if (existingLevel)
            {
                Debug.Log("Existing level found. Destroying...");
                GameObject.DestroyImmediate(existingLevel);
            }

            try
            {
                LevelObject = Mesh.LevelGenerationMesh.Generate(Level, Params.BiomeConfig);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        [Command("spawn_enemies")]
        [CommandDescription("Spawns enemies according to enemy cells in the current level.")]
        public static void SpawnEnemies()
        {
            Log("Generating level geometry...");
            Mesh.Utils.IterateSector(Level.BaseSector, (it) => 
            { 
                LevelGeneration.SpawnEnemy(it, Params); 
            }, ELevelLayer.Enemies);
        }

        [Command("spawn_enemies")]
        [CommandDescription("Spawns enemies according to enemy cells in the current level.")]
        public static void SpawnEnemies(CharacterPool characterPool)
        {
            if (Level == null || LevelObject == null)
            {
                Log("No level generated. Generate and spawn a level before spawning characters.");
                return;
            }

            Mesh.Utils.IterateSector(Level.BaseSector, (it) => 
            { 
                LevelGeneration.SpawnEnemy(it, Params, characterPool); 
            }, ELevelLayer.Enemies);
        }

        [Command("spawn_player")]
        [CommandDescription("Spawns player in an automatically selected cell.")]
        public static void SpawnPlayer(string characterConfiguration)
        {
            if (Level == null || LevelObject == null)
            {
                Log("No level generated. Generate and spawn a level before spawning characters.");
                return;
            }

            Vector2Int position = LevelGeneration.SelectPlayerStartPosition(Level);
            Vector3 worldPosition = Mesh.Utils.LevelToWorldPos(position, Params.BiomeConfig.CellSize());
            CharacterFactory.SpawnPlayer(characterConfiguration, worldPosition);

            Log($"Player spawned at {worldPosition}");
        }

        private static void Log(string str)
        {
            QuantumConsole.Instance.LogToConsole(str);
        }
    }
}