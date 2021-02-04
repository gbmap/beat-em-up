using UnityEngine;
using QFSW.QC;
using static Catacumba.LevelGen.LevelGenerationParams;
using System;
using Catacumba.Data.Level;
using Catacumba.Data;
using Catacumba.Entity;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using Catacumba.LevelGen.Mesh;

namespace Catacumba.LevelGen
{
    [CommandPrefix("level.")]
    public static class LevelGenerationManager
    {
        [Command("type")]         public static ELevelType LevelType = ELevelType.Dungeon;
        [Command("sz")]           public static Vector2Int LevelSize = new Vector2Int(30, 30);
        [Command("prop_chance")]  public static float PropChance = 0.45f;
        [Command("enemy_chance")] public static float EnemyChance = 0.55f;
        [Command("trap_chance")]  public static float TrapChance = 0.5f;
        
        private static BiomeConfiguration _biomeConfig;
        [Command("biome")]        public static BiomeConfiguration BiomeConfig
        {
            get { return _biomeConfig ?? (_biomeConfig = LoadBiome("BiomeDungeon")); }
            set { _biomeConfig = value; }
        }

        private static CharacterPool _characterPool;
        [Command("enemy_pool")]   public static CharacterPool EnemyPool
        {
            get { return _characterPool ?? (_characterPool = CharacterManager.LoadPool("CharacterPool_Goblins")); }
            set { _characterPool = value; }
        }

        private static CharacterPool _propPool;
        [Command("prop_pool")]   public static CharacterPool PropPool
        {
            get { return _propPool ?? (_propPool = CharacterManager.LoadPool("CharacterPool_Props_Dungeon")); }
            set { _propPool = value; }
        }

        [Command("player")] public static GameObject Player { get; set; }

        [Command("preloaded_file")] public static string PreloadedLevel { get; set; }

        public const string PATH_BIOMES     = "Data/Level/Biomes";

        public static Level                 Level;
        public static LevelGenerationParams Params;
        public static GameObject            LevelObject;

        [Command("load_biome")]
        public static BiomeConfiguration LoadBiome(string name)
        {
            return Resources.Load<BiomeConfiguration>($"{PATH_BIOMES}/{name}");
        }

        [Command("generate")]
        public static async Task Generate()
        {
            bool hasEnded = false;
            LevelGeneration.Generate(new LevelGenerationParams
            {
                LevelType      = LevelType,
                LevelSize      = LevelSize,
                PropChance     = PropChance,
                EnemyChance    = EnemyChance,
                BiomeConfig    = BiomeConfig,
                EnemyPool      = EnemyPool,
                PropPool       = PropPool,
                PreloadedLevel = PreloadedLevel
            }, (level, parameters) => 
            { 
                Level = level; 
                Params = parameters; 
                hasEnded = true;
            });

            while (!hasEnded)
                await Task.Delay(100);
        }

        [Command("load_from_bitmap")]
        public static async Task LoadBitmap(string file)
        {
            PreloadedLevel = file;
            await Generate();
        }

        [Command("spawn")]
        [CommandDescription("Spawns object for current generated level.")]
        public static async Task GenerateMesh()
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
                bool hasEnded = false;
                System.Action<GameObject> OnLevelGenerationEnded = delegate(GameObject obj) { LevelObject = obj; hasEnded = true; };
                QuantumConsole.Instance
                              .StartCoroutine(Mesh.LevelGenerationMesh
                                                  .Generate(Level, 
                                                            Params.BiomeConfig, 
                                                            OnLevelGenerationEnded));

                while (!hasEnded)
                    await Task.Delay(100);

                Log("Finished generating geometry.");
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
            SpawnEnemies(Params.EnemyPool);
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
                if (it.cell != LevelGeneration.ECellCode.Enemy) return;
                CharacterManager.SpawnEnemy(it.cellPosition, Params, characterPool); 
            }, ELevelLayer.Enemies);
        }

        [Command("spawn_boss")]
        public async static void SpawnBoss(CharacterConfiguration boss)
        {
            bool hasEnded = false;
            QuantumConsole.Instance.StartCoroutine(
                LevelGenerationMesh.RunSteps(new ILevelGenerationMeshStep[] { new LevelGenerationMeshStepBoss(boss) },
                                             Level, BiomeConfig, LevelObject, () => { hasEnded = true; } )
            );

            while (!hasEnded)
                await Task.Delay(100);
        }

        [Command("spawn_props")]
        public static async Task SpawnProps()
        {
            await SpawnProps(Params.PropPool);
        }

        [Command("spawn_props")]
        [CommandDescription("Spawns props according to prop cells in the current level.")]
        public static async Task SpawnProps(CharacterPool characterPool)
        {
            if (Level == null || LevelObject == null)
            {
                Log("No level generated. Generate and spawn a level before spawning characters.");
                return;
            }

            bool hasEnded = false;
            QuantumConsole.Instance.StartCoroutine(
                LevelGenerationMesh.RunSteps(new ILevelGenerationMeshStep[] { new LevelGenerationMeshStepProps() },
                                             Level, BiomeConfig, LevelObject, () => { hasEnded = true; } )
            );

            while (!hasEnded)
                await Task.Delay(100);

            Vector3 cellSize = Params.BiomeConfig.CellSize();

            Mesh.Utils.IterateSector(Level.BaseSector, (it) =>
            {
                if (it.cell != LevelGeneration.ECellCode.Prop) return;

                CharacterPoolItem propCfg = characterPool.GetRandom();

                Vector3 worldPosition = Mesh.Utils.LevelToWorldPos(it.cellPosition, cellSize);
                float   randX         = PropDistanceAdjust(Random.value);
                float   randZ         = PropDistanceAdjust(Random.value);
                Vector3 offset        = new Vector3(randX*cellSize.x, 0f, randZ*cellSize.z);

                CharacterManager.SpawnProp(propCfg.Config, worldPosition + offset);
            }, ELevelLayer.Props);
        }

        [Command("spawn_traps")]
        public static async Task SpawnTraps(BiomeTrapConfiguration trapConfiguration)
        {
            if (Level == null || LevelObject == null)
            {
                Log("No level generated. Generate and spawn a level before spawning characters.");
                return;
            }

            bool hasEnded = false;
            QuantumConsole.Instance.StartCoroutine(
                LevelGenerationMesh.RunSteps(
                    new ILevelGenerationMeshStep[] { 
                        new LevelGenerationMeshStepTraps(trapConfiguration, TrapChance) 
                        }, Level, BiomeConfig, LevelObject, () => { hasEnded = true; } 
                )
            );

            while (!hasEnded)
                await Task.Delay(100);
        }

        private static float PropDistanceAdjust(float v)
        {
            return Mathf.Pow(Random.value, 1f/5f);
        }

        [Command("spawn_player")]
        [CommandDescription("Spawns player in an automatically selected cell.")]
        public static GameObject SpawnPlayer(CharacterConfiguration characterConfiguration)
        {
            if (Level == null || LevelObject == null)
            {
                Log("No level generated. Generate and spawn a level before spawning characters.");
                return null;
            }

            Vector2Int position = LevelGeneration.SelectPlayerStartPosition(Level);
            Vector3 worldPosition = Mesh.Utils.LevelToWorldPos(position, Params.BiomeConfig.CellSize());
            GameObject player = CharacterManager.SpawnPlayer(characterConfiguration, worldPosition);

            Log($"Player spawned at {worldPosition}");
            CameraManager.Target = player.transform;
            return player;
        }

        private static void Log(string str)
        {
            QuantumConsole.Instance.LogToConsole(str);
        }
    }
}