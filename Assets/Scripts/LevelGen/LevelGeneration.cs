﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Catacumba.LevelGen
{
    public enum EDirection 
    {
        Up,
        Right,
        Down,
        Left
    } // DON'T CHANGE THIS ORDER 

    public class Connector
    {
        public Sector From; 
        public Sector To;
        public Vector2Int StartPosition; // Absolute position
        public Vector2Int EndPosition;  // absolute position
        public List<EDirection> Path; // Path is relative movements.

        public Connector(Vector2Int start, Vector2Int end)
        {
            StartPosition = start;
            EndPosition = end;
            Path = new List<EDirection>();
        }

        public Connector(Sector from, Sector to)
        {
            From = from;
            To = to;
            Path = new List<EDirection>();
        }
    }

    ////////////////////////////// 
    // Represents a sector inside a level. It has
    // also sub-sectors.
    //
    public class Sector
    {
        public Level Level { get; private set; }

        private Sector _parent;
        public Sector Parent
        {
            get { return _parent; }
            private set
            {
                if (_parent != null)
                    _parent.RemoveChild(this);

                _parent = value;
                if (_parent != null)
                    _parent.AddChild(this);
            }
        }

        public Vector2Int Pos { get; private set; }
        public Vector2Int Size { get; private set; }

        public List<Sector> Children;

        public List<Connector> Connectors;

        public LevelGeneration.ECellCode Code { get; private set; }

        public Sector(Level l,
                      Vector2Int p, 
                      Vector2Int sz, 
                      Sector parent, 
                      LevelGeneration.ECellCode code)
        {
            Level = l;
            Pos = p;
            Size = sz;
            Parent = parent;
            Children = new List<Sector>();
            Connectors = new List<Connector>();
            Code = code;
        }
        
        public bool IsIn(Vector2Int p)
        {
            return IsIn(p.x, p.y);
        }

        /*
         * Checks if a local space point is inside the sector.
         * */
        public bool IsIn(float x, float y)
        {
            return LevelGeneration.IsInBox(Pos.x + x, Pos.y + y, Pos, Size);
        }

        /*
         * Returns 
         * */
        public Sector GetChildSectorAtPosition(Vector2Int p)
        {
            return null;
        }

        public Vector2Int GetAbsolutePosition(Vector2Int p)
        {
            if (Parent == null)
            {
                return Pos + p;
            }
            return Parent.GetAbsolutePosition(Pos + p);
        }

        public LevelGeneration.ECellCode GetCell(int x, int y)
        {
            return GetCell(new Vector2Int(x, y));
        }

        public LevelGeneration.ECellCode GetCell(Vector2Int p)
        {
            if (!IsIn(p))
                return LevelGeneration.ECellCode.Error;

            if (Parent == null)
                return Level.GetCell(Pos + p);
            else
                return Parent.GetCell(Pos + p);
        }

        /*
         * Sets a point in local space to the desired value.
         * If overwrite is true, the value will be set even if c < current value in map.
         * */
        public void SetCell(Vector2Int p,
                            LevelGeneration.ECellCode c, 
                            bool overwrite = false)
        {
            if (!IsIn(p))
                return; // Do nothing.

            if (Parent == null)
                Level.SetCell(Pos + p, c, overwrite);
            else
                Parent.SetCell(Pos + p, c, overwrite);
        }

        public void CreateSector(Sector s)
        {
            for (int x = 0; x < s.Size.x; x++)
            {
                for (int y = 0; y < s.Size.y; y++)
                {
                    Vector2Int p = new Vector2Int(s.Pos.x + x, s.Pos.y + y);
                    SetCell(p, s.Code);
                }
            }
            s.Parent = this;
            //Children.Add(s);
        }

        private void FillSector(LevelGeneration.ECellCode code = LevelGeneration.ECellCode.Room)
        {
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    Vector2Int p = new Vector2Int(x, y);
                    SetCell(p, code, true);
                }
            }
        }

        public void DestroySector()
        {
            FillSector(LevelGeneration.CODE_EMPTY);
            if (Parent != null) Parent.RemoveChild(this);
            for (int i = 0; i < Connectors.Count; i++)
            {
                Connector c = Connectors[i];
                DestroyConnector(c);
                i--;
            }
        }

        public void DestroyConnector(Connector c)
        {
            if (!Connectors.Contains(c))
                return;

            c.From.Connectors.Remove(c);
            c.To.Connectors.Remove(c);

            Vector2Int p = c.StartPosition;
            LevelGeneration.ECellCode v = Level.GetCell(p);
            if (v == LevelGeneration.ECellCode.Hall)
                Level.SetCell(p, 0, true);

            for (int i = 0; i < c.Path.Count; i++)
            {
                p += LevelGeneration.DirectionToVector2(c.Path[i]);
                v = Level.GetCell(p);
                if (v == LevelGeneration.ECellCode.Hall)
                    Level.SetCell(p, 0, true);
            }
        }

        public void AddChild(Sector s)
        {
            Children.Add(s);
        }

        public void RemoveChild(Sector s)
        {
            Children.Remove(s);
        }

        /*
         * Returns the closest sector with the same parent.
         * */
        public Sector GetClosestSiblingSector()
        {
            if (Parent == null) return null;
            if (Parent.Children.Count == 1) return null;

            return Parent.Children.OrderBy(s => Vector2.Distance(Pos, s.Pos)).ElementAt(1);
        }

        public Sector[] GetSiblings()
        {
            if (Parent == null) return null;
            if (Parent.Children.Count == 1) return null;

            return Parent.Children.OrderBy(s => Vector2.Distance(Pos, s.Pos)).ToArray();
        }

        public Sector GetSectorAt(Vector2Int pos)
        {
            return Children.Where(s => pos.x > s.Pos.x && pos.y > s.Pos.y &&
                                       pos.x < s.Pos.x + s.Size.x && pos.y < s.Pos.y + s.Size.y).FirstOrDefault();
        }

        public HashSet<Sector> ListConnectedSectors()
        {
            return ListConnectedSectors(new HashSet<Sector>(), this);
        }

        public static HashSet<Sector> ListConnectedSectors(HashSet<Sector> sl, Sector sec)
        {
            if (sl.Contains(sec))
                return sl;

            sl.Add(sec);
            foreach (var connector in sec.Connectors)
            {
                if (connector.To != sec)
                    sl.UnionWith(ListConnectedSectors(sl, connector.To));
                else if (connector.From != sec)
                    sl.UnionWith(ListConnectedSectors(sl, connector.From));
            }

            return sl;
        }

        public static int GetNumberOfConnectedSectors(Sector sec)
        {
            return ListConnectedSectors(new HashSet<Sector>(), sec).Count;
        }
    }
    
    [System.Serializable]
    public class LevelGenerationParams
    {
        public enum ELevelType
        {
            Dungeon,
            Cave
        }

        public ELevelType LevelType;

        public Vector2Int LevelSize = new Vector2Int(50, 50);

        [Range(0f, 1f)]
        public float PropChance = 0.65f;

        [Range(0f, 1f)]
        public float EnemyChance = 0.65f;

        public bool GenerateMesh = false;

        [Header("Mesh Configuration")]
        public LevelGenBiomeConfig BiomeConfig;

        [Header("Player")]
        public GameObject PlayerPrefab;
    }

    public class LevelGeneration : SimpleSingleton<LevelGeneration>
    {
        public enum ECellCode
        {
            Error = -1,
            Empty,
            Hall,
            Room,
            BossRoom,
            Spawner,
            PlayerSpawn,
            Prop,
            RoomItem,
            RoomPrison,
            RoomEnemies,
            RoomDice,
            RoomBloodOath,
            RoomKillChallenge,
            RoomChase,
            Enemy
        }

        public const int CODE_ERROR               = -1;
        public const int CODE_EMPTY               = 0;
        public const int CODE_HALL                = 1;
        public const int CODE_ROOM                = 2;
        public const int CODE_BOSS_ROOM           = 3;
        public const int CODE_SPAWNER             = 4;
        public const int CODE_PLAYER_SPAWN        = 5;
        public const int CODE_PROP                = 6;
        public const int CODE_ROOM_ITEM           = 7;
        public const int CODE_ROOM_PRISON         = 8;
        public const int CODE_ROOM_ENEMIES        = 9;
        public const int CODE_ROOM_DICE           = 10;
        public const int CODE_ROOM_BLOOD_OATH     = 11;
        public const int CODE_ROOM_KILL_CHALLENGE = 12;
        public const int CODE_ROOM_CHASE          = 13;
        public const int CODE_ENEMY               = 14;

        public LevelGenerationParams Params;

        private void Awake()
        {
            if (Params.GenerateMesh)
                Generate(Params, OnLevelGenerationComplete);
            else
                Generate(Params, null);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (Params.GenerateMesh)
                    Generate(Params, OnLevelGenerationComplete);
                else
                    Generate(Params, null);
            }
        }

        ///////////////////////
        /// GENERATION
        /// 
        public static void Generate(LevelGenerationParams p,
                                    System.Action<Level, LevelGenerationParams> OnCompleted)
        {
            Instance.StartCoroutine(CGenerate(p, OnCompleted));
        }

        public static System.Collections.IEnumerator CGenerate(LevelGenerationParams p,
                                                       System.Action<Level, LevelGenerationParams> OnCompleted)
        {
            Debug.Log("Level generation started.");

            Level level = new Level(p.LevelSize);

            UpdateVis(level);

            ILevelGenAlgo[] genSteps = {
                GetLevelGenerationAlgorithm(p),
                new LevelGenAlgoPerlinMaskAdd(ECellCode.Hall, ECellCode.Prop, 1f - p.PropChance, 0.5f, 0.5f),
                new LevelGenAlgoPerlinMaskAdd(ECellCode.Hall, ECellCode.Enemy, 1f - p.EnemyChance, 0.5f, 0.5f)
            };
            
            foreach (ILevelGenAlgo algo in genSteps)
                yield return algo.Run(level, UpdateVis);

            /////////////////
            /// PLAYER POSITION
            Sector[] secs = StepSelectPlayerSpawnPoint(level);
            UpdateVis(level);

            int secsL = secs.Length;
            yield return new WaitForSeconds(0.5f);

            //////////////////
            /// PROPS
            foreach (Sector sec in level.BaseSector.Children)
            {
                int propCells = Random.Range(0, ((sec.Size.x + sec.Size.y) / 2));
                for (int i = 0; i < propCells; i++)
                {
                    Vector2Int pos = new Vector2Int(Random.Range(0, sec.Size.x),
                                                  Random.Range(0, sec.Size.y));

                    sec.SetCell(pos, LevelGeneration.ECellCode.Prop);
                }
            }

            UpdateVis(level);

            OnCompleted?.Invoke(level, p);

            string s = "";
            for (int y = 0; y < level.Size.y; y++)
            {
                for (int x = 0; x < level.Size.x; x++)
                {
                    if (level.GetCell(x, y) > 0) s += "1";
                    else s += "0";
                }
                s += System.Environment.NewLine;
            }
            Debug.Log(s);

            Debug.Log("Level generation ended.");
            yield break;
        }

        private static ILevelGenAlgo GetLevelGenerationAlgorithm(LevelGenerationParams p)
        {
            switch (p.LevelType)
            {
                case LevelGenerationParams.ELevelType.Dungeon: return new LevelGenAlgoWalkers();
                case LevelGenerationParams.ELevelType.Cave: return new LevelGenAlgoPerlin();
                default: return new LevelGenAlgoWalkers();
            }
        }

        /*
         * Returns a list of connected Sectors starting by the spawn sector.
         * */
        private static Sector[] StepSelectPlayerSpawnPoint(Level l)
        {
            IOrderedEnumerable<Sector> sectors = l.BaseSector.Children
                                                  .OrderByDescending(s => Sector.GetNumberOfConnectedSectors(s));

            if (sectors == null || sectors.Count() == 0)
            {
                return new Sector[] { l.BaseSector };
            }
            
            // select the sector with most connected sectors to it
            // i'm dumb as a door
            var spawnSector = sectors.First();

            spawnSector.SetCell(Vector2Int.one, LevelGeneration.ECellCode.PlayerSpawn, true);

            l.SpawnSector = spawnSector;
            l.SpawnPoint = spawnSector.GetAbsolutePosition(Vector2Int.one);

            return Sector.ListConnectedSectors(new HashSet<Sector>(), spawnSector)
                .OrderBy(s => Vector2.Distance(spawnSector.Pos, s.Pos)).ToArray();
        }

        private static void StepAddProps(Level l)
        {
            for (int x = 0; x < l.Size.x; x++)
            {
                for (int y = 0; y < l.Size.y; y++)
                {
                    LevelGeneration.ECellCode cell = l.GetCell(x, y);
                    if (cell != LevelGeneration.ECellCode.Hall)
                        continue;
                }
            }
        }

        private static System.Collections.IEnumerator StepCleanup(Level l)
        {
            HashSet<Sector> sectors = Sector.ListConnectedSectors(new HashSet<Sector>(), l.SpawnSector);
            for (int i = 0; i < l.BaseSector.Children.Count; i++)
            {
                Sector sc = l.BaseSector.Children[i];
                if (sectors.Contains(sc))
                    continue;

                sc.DestroySector();
                i--;

                UpdateVis(l);
                yield return new WaitForSeconds(0.25f);
            }
        }

        ///////////////////////////
        /// MESH GENERATION
        /// 
        static void OnLevelGenerationComplete(Level l, LevelGenerationParams p)
        {
            // Se já tem geometria de um level anterior, destruir.
            var obj = GameObject.Find("Level");
            if (obj)
            {
                DestroyImmediate(obj);
            }

            // Mesh
            LevelGenerationMesh.Generate(l, p.BiomeConfig);

            // Criar Player
            var cellSize = p.BiomeConfig.CellSize();
            cellSize.x *= l.SpawnPoint.x;
            cellSize.z *= l.SpawnPoint.y;
            cellSize.y = 0f;
            GameObject player = Instantiate(p.PlayerPrefab, cellSize, Quaternion.identity);

            // Setar câmera
            Camera.main.transform.position = cellSize;
            var c = Camera.main.gameObject.AddComponent<CameraHideEnvironmentInFront>();
            c.Target = player.transform;

            GameObject virtualCamera = new GameObject("VCam");
            Cinemachine.CinemachineVirtualCamera vcam = virtualCamera.AddComponent<Cinemachine.CinemachineVirtualCamera>();
            vcam.Follow = player.transform;
            vcam.LookAt = player.transform;

            var body = vcam.AddCinemachineComponent<Cinemachine.CinemachineTransposer>();
            body.m_BindingMode = Cinemachine.CinemachineTransposer.BindingMode.WorldSpace;
            body.m_FollowOffset = new Vector3(0f, 12f, -12f);

            var aim = vcam.AddCinemachineComponent<Cinemachine.CinemachineComposer>();
        }

        public static bool IsValidPosition(Level l, Vector2Int p)
        {
            return (p.x >= 0 && p.x < l.Size.x) &&
                 (p.y >= 0 && p.y < l.Size.y);
        }

        public static bool IsInBox(Vector2Int p, Vector2Int bp, Vector2Int bsz)
        {
            return IsInBox(p.x, p.y, bp, bsz);
        }

        public static bool IsInBox(float x, float y, Vector2Int bp, Vector2Int bsz)
        {
            return ((x >= bp.x && x < bp.x + bsz.x) &&
                    (y >= bp.y && y < bp.y + bsz.y));
        }

        public static bool AABB(Rect a, Rect b)
        {
            return a.x < b.x + b.width &&
                   a.x + a.width > b.x &&
                   a.y < b.y + b.height &&
                   a.y + a.height > b.y;
        }

        public static void SetCell(Level l, Vector2Int p, LevelGeneration.ECellCode code)
        {
            if (!IsValidPosition(l, p) || (int)code < (int)l.GetCell(p.x, p.y))
                return;

            l.SetCell(p.x, p.y, code);
        }


        public static void UpdateVis(Level l)
        {
#if UNITY_EDITOR
            LevelGenVisualizer[] vis = FindObjectsOfType<LevelGenVisualizer>();
            System.Array.ForEach(vis, v => v.UpdateTexture(l));
#endif
        }

        public static Vector2Int DirectionToVector2(EDirection d)
        {
            // TODO: make this array a const
            return new Vector2Int[] {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            }[(int)d];
        }
    }
}