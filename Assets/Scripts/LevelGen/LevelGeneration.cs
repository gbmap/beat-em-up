using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Catacumba.Level
{
    public enum EDirection 
    {
        Up,
        Right,
        Down,
        Left
    } // DON'T CHANGE THIS ORDER 

    /*
     * Walks randomly around the dungeon until it explodes and
     * creates a room.
     */
    public abstract class BaseWalker
    {
        public EDirection Direction;
        public Vector2Int Position;

        public System.Action<BaseWalker> OnDeath;

        public BaseWalker(Vector2Int pos)
        {
            Direction = (EDirection)Random.Range(0, 4);
            Position = pos;
        }

        public bool Walk(Sector s)
        {
            s.SetCell(Position, LevelGeneration.CODE_HALL);
            Vector2Int newPos = GetNewPosition(s);

            if (s.IsIn(newPos))
            {
                Position = newPos;
                if (OnMoved(s))
                {
                    OnDeath?.Invoke(this);
                    return true;
                }
            }

            return false;
        }

        public abstract bool OnMoved(Sector s);

        //public abstract bool Walk(ref Level l);
        public abstract Vector2Int GetNewPosition(Sector s);

        public void ChangeDirection()
        {
            int d = (((int)Direction + (int)Mathf.Sign(Random.value - .5f)) % 4);
            if (d < 0) d = 3;
            Direction = (EDirection)d;
        }
    }

    public class KamikazeWalker : BaseWalker
    {
        private static Dictionary<EDirection, Vector2Int> DirectionVectors = new Dictionary<EDirection, Vector2Int>
        {
            { EDirection.Down, Vector2Int.down },
            { EDirection.Left, Vector2Int.left },
            { EDirection.Right, Vector2Int.right },
            { EDirection.Up, Vector2Int.up }
        };

        public int Life;
        public float TurnChance;
        public Vector2Int RoomSize;

        public KamikazeWalker(Vector2Int pos, int life, float turnChance, Vector2Int explosionSz)
            : base(pos)
        {
            Life = life;
            TurnChance = turnChance;
            RoomSize = explosionSz;
        }

        public override Vector2Int GetNewPosition(Sector s)
        {
            var newPos = Position + DirectionVectors[Direction];
            if (!s.IsIn(newPos))
            {
                Direction = (EDirection)(((int)Direction + 1) % 4);
                return GetNewPosition(s);
            }
            return newPos;
        }

        public override bool OnMoved(Sector s)
        {
            if (Random.value < TurnChance)
                ChangeDirection();

            if (s.GetCell(Position) == LevelGeneration.CODE_EMPTY)
            {
                Life--;
            }

            if (Life <= 0)
            {
                Sector sec = new Sector(s.Level, Position, RoomSize, s);
                s.CreateSector(sec);
                //LevelGeneration.CreateRoom(l, r);
                return true;
            }

            return false;
        }
    }

    public class TargetedWalker : BaseWalker
    {
        public Vector2Int TargetPos { get; private set; }
        public List<EDirection> Path { get; private set; }

        public Connector Connector { get; private set; }

        public TargetedWalker(Vector2Int pos, Vector2Int targetPos)
            : base(pos)
        {
            TargetPos = targetPos;
            Path = new List<EDirection>();

            Connector = new Connector(pos, targetPos);
        }

        public override Vector2Int GetNewPosition(Sector s)
        {
            Vector2Int delta = TargetPos - Position;
            Vector2Int mov = Vector2Int.zero;

            EDirection d;

            if (delta.x != 0 && delta.y != 0)
            {
                if (Random.value > 0.5f)
                    mov.x = (int)Mathf.Sign(delta.x);
                else
                    mov.y = (int)Mathf.Sign(delta.y);
            }
            else
            {
                if (delta.x != 0)
                    mov.x = (int)Mathf.Sign(delta.x);
                else
                    mov.y = (int)Mathf.Sign(delta.y);
            }

            if (mov.x != 0)
                d = mov.x > 0 ? EDirection.Right : EDirection.Left;
            else
                d = mov.y > 0 ? EDirection.Up : EDirection.Down;

            Connector.Path.Add(d);

            return Position + mov;
        }

        public override bool OnMoved(Sector s)
        {
            return TargetPos - Position == Vector2Int.zero;
        }
    }

    public class Room
    {
        public Vector2Int Position;
        public Vector2Int Size;
    }

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

        public Sector(Level l, Vector2Int p, Vector2Int sz, Sector parent)
        {
            Level = l;
            Pos = p;
            Size = sz;
            Parent = parent;
            Children = new List<Sector>();
            Connectors = new List<Connector>();
        }

        public bool IsIn(Vector2Int p)
        {
            return IsIn(p.x, p.y);
        }

        public bool IsIn(float x, float y)
        {
            return LevelGeneration.IsInBox(Pos.x + x, Pos.y + y, Pos, Size);
        }

        public Vector2Int GetAbsolutePosition(Vector2Int p)
        {
            if (Parent == null)
            {
                return Pos + p;
            }
            return Parent.GetAbsolutePosition(Pos + p);
        }

        public int GetCell(int x, int y)
        {
            return GetCell(new Vector2Int(x, y));
        }

        public int GetCell(Vector2Int p)
        {
            if (!IsIn(p))
                return LevelGeneration.CODE_ERROR;

            if (Parent == null)
                return Level.GetCell(Pos + p);
            else
                return Parent.GetCell(Pos + p);
        }

        public void SetCell(Vector2Int p, int c, bool overwrite = false)
        {
            if (!IsIn(p))
                return; // Do nothing.

            if (Parent == null)
                Level.SetCell(Pos + p, c, overwrite);
            else
                Parent.SetCell(Pos + p, c, overwrite);
        }

        public void CreateSector(Sector s, int code = LevelGeneration.CODE_ROOM)
        {
            for (int x = 0; x < s.Size.x; x++)
            {
                for (int y = 0; y < s.Size.y; y++)
                {
                    Vector2Int p = new Vector2Int(s.Pos.x + x, s.Pos.y + y);
                    SetCell(p, code);
                }
            }
            s.Parent = this;
            //Children.Add(s);
        }

        private void FillSector(int code = LevelGeneration.CODE_ROOM)
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
            int v = Level.GetCell(p);
            if (v == LevelGeneration.CODE_HALL)
                Level.SetCell(p, 0, true);

            for (int i = 0; i < c.Path.Count; i++)
            {
                p += LevelGeneration.DirectionToVector2(c.Path[i]);
                v = Level.GetCell(p);
                if (v == LevelGeneration.CODE_HALL)
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
    }
    
    public class Level
    {
        public Vector2Int Size
        {
            get
            {
                return new Vector2Int(Map.GetLength(0), Map.GetLength(1));
            }
        }
        private int[,] Map;

        public Vector2Int SpawnPoint;

        public Sector BaseSector { get; private set; }

        public void SetCell(Vector2Int p, int v, bool overwrite = false)
        {
            SetCell(p.x, p.y, v, overwrite);
        }

        public void SetCell(int x, int y, int v, bool overwrite = false)
        {
            if (overwrite)
                Map[x, y] = v;
            else
                Map[x, y] = Mathf.Max(Map[x, y], v);
        }

        public int GetCell(Vector2Int p)
        {
            return GetCell(p.x, p.y);
        }

        public int GetCell(int x, int y)
        {
            try
            {
                return Map[x, y];
            }
            catch (System.IndexOutOfRangeException ex)
            {
                return LevelGeneration.CODE_ERROR;
            }
        }

        public Level(Vector2Int size)
        {
            Map = new int[size.x, size.y];
            BaseSector = new Sector(this, Vector2Int.zero, size, null);
        }
    }


    [System.Serializable]
    public class LevelGenerationParams
    {
        public Vector2Int LevelSize = new Vector2Int(50, 50);
        public bool GenerateMesh = false;

        [Header("Sectors")]
        public bool AddSectors = true;
        //[Range(1, 10)]
        public Vector2Int Divisions = new Vector2Int(3, 1);

        public Vector2 ConnectorChances = new Vector2(0.5f, 0.5f);

        [Header("Walkers")]
        [Range(0, 10)]
        public int Walkers = 1;
        [Range(4, 20)]
        public int WalkerLife = 6;
        public Vector2Int WalkerRoomSz = new Vector2Int(5, 5);
        [Range(0f, 1f)]
        public float WalkerTurnChance = .25f;

        [Header("Mesh Configuration")]
        public LevelGenBiomeConfig BiomeConfig;

        [Header("Player")]
        public GameObject PlayerPrefab;
    }

    public class LevelGeneration : SimpleSingleton<LevelGeneration>
    {
        public const int CODE_ERROR = -1;
        public const int CODE_EMPTY = 0;
        public const int CODE_HALL = 1;
        public const int CODE_ROOM = 2;
        public const int CODE_BOSS_ROOM = 3;
        public const int CODE_SPAWNER = 4;
        public const int CODE_PLAYER_SPAWN = 5;
        public const int CODE_PROP = 6;

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

            /////////////////
            /// SECTORS
            if (p.AddSectors)
                yield return StepAddSectors(level, p.LevelSize, p.Divisions, p.ConnectorChances);

            UpdateVis(level);

            /////////////////
            /// WALKERS
            if (p.Walkers > 0)
                yield return StepAddWalkers(level, p.Walkers, p.WalkerLife, p.WalkerTurnChance, p.WalkerRoomSz);

            /////////////////
            /// PLAYER POSITION
            int count = level.BaseSector.Children.Count;
            var spawnSector = level.BaseSector.Children[Random.Range(0, count)];
            spawnSector.SetCell(Vector2Int.one, LevelGeneration.CODE_PLAYER_SPAWN);

            level.SpawnPoint = spawnSector.GetAbsolutePosition(Vector2Int.one);

            UpdateVis(level);

            /////////////////
            /// CLEAN UP
            HashSet<Sector> sectors = Sector.ListConnectedSectors(new HashSet<Sector>(), spawnSector);
            for (int i = 0; i < level.BaseSector.Children.Count; i++)
            {
                Sector sc = level.BaseSector.Children[i];
                if (sectors.Contains(sc))
                    continue;

                sc.DestroySector();
                level.BaseSector.Children.RemoveAt(i);
                i--;

                UpdateVis(level);
                yield return new WaitForSeconds(0.25f);
            }

            UpdateVis(level);
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

                    sec.SetCell(pos, CODE_PROP);
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

        private static System.Collections.IEnumerator StepAddSectors(Level level,
                                                                     Vector2Int size, 
                                                                     Vector2Int divisions,
                                                                     Vector2 connectorChances)
        {
            ////////////////////////
            // SECTION ROOMS
            Vector2Int d = divisions;
            Sector[,] sectors = new Sector[d.x, d.y];
            Vector2Int secSz = new Vector2Int(Mathf.Max(2, size.x / d.x), Mathf.Max(2, size.y / d.y)); // sector size
            for (int sx = 0; sx < d.x; sx++)
            {
                for (int sy = 0; sy < d.y; sy++)
                {
                    if (Random.value < 0.25f) continue;

                    int ri = sx + sy * d.y; // sector index

                    Vector2Int secPos = secSz * new Vector2Int(sx, sy);

                    Vector2Int rsz = new Vector2Int(Random.Range((int)(secSz.x*0.5f), secSz.x), 
                                                    Random.Range((int)(secSz.y*0.5f), secSz.y));
                    /*Vector2Int p = new Vector2Int(rsz.x / 2 + sx * secSz.x, rsz.y / 2 + sy * secSz.y) +
                        new Vector2Int(Random.Range(0, secSz.x - rsz.x / 2),
                                       Random.Range(0, secSz.y - rsz.y / 2));*/
                    Vector2Int p = new Vector2Int(Random.Range(0, (secSz - rsz).x), 
                                                  Random.Range(0, (secSz - rsz).y));
                    sectors[sx, sy] = new Sector(level, secPos + p, rsz, level.BaseSector);
                    level.BaseSector.CreateSector(sectors[sx, sy], CODE_ROOM);
                }
            }

            UpdateVis(level);

            ////////////////////////
            // CONNECTORS

            // connect sectors with targeted walkers
            List<TargetedWalker> walkers = new List<TargetedWalker>();
            for (int sx = 0; sx < d.x; sx++)
            {
                for (int sy = 0; sy < d.y; sy++)
                {
                    if (sectors[sx, sy] == null) continue;

                    if (sx + 1 < d.x && sectors[sx+1, sy] != null && Random.value < connectorChances.x)
                    {
                        Sector a = sectors[sx, sy];
                        Sector b = sectors[sx + 1, sy];

                        TargetedWalker w = new TargetedWalker(
                            a.Pos + new Vector2Int(Random.Range(0, a.Size.x), Random.Range(0, a.Size.y)),
                            b.Pos + new Vector2Int(Random.Range(0, b.Size.x), Random.Range(0, b.Size.y)));

                        w.OnDeath += (delegate (BaseWalker bw)
                        {
                            TargetedWalker tw = (bw as TargetedWalker);
                            tw.Connector.From = a;
                            tw.Connector.To = b;
                            a.Connectors.Add(tw.Connector);
                            b.Connectors.Add(tw.Connector);
                        });

                        walkers.Add(w);
                    }

                    if (sy + 1 < d.y && sectors[sx, sy+1] != null && Random.value < connectorChances.y)
                    {
                        Sector a = sectors[sx, sy];
                        Sector b = sectors[sx, sy+1];
                        Connector c = new Connector(a, b);
                        a.Connectors.Add(c);
                        b.Connectors.Add(c);

                        TargetedWalker w = new TargetedWalker(
                            a.Pos + new Vector2Int(Random.Range(0, a.Size.x), Random.Range(0, a.Size.y)),
                            b.Pos + new Vector2Int(Random.Range(0, b.Size.x), Random.Range(0, b.Size.y)));

                        w.OnDeath += (delegate (BaseWalker bw)
                        {
                            TargetedWalker tw = (bw as TargetedWalker);
                            tw.Connector.From = a;
                            tw.Connector.To = b;
                            a.Connectors.Add(tw.Connector);
                            b.Connectors.Add(tw.Connector);
                        });

                        walkers.Add(w);
                    }
                }
            }

            walkers.ForEach(w => w.OnDeath += delegate (BaseWalker bw) { walkers.Remove(bw as TargetedWalker); });

            // wait walkers do their thing
            while (walkers.Count > 0)
            {
                for (int i = 0; i < walkers.Count; i++)
                {
                    if (walkers[i].Walk(level.BaseSector))
                    {
                        i--;
                    }
                }

                UpdateVis(level);
                yield return new WaitForSeconds(0.1f);
            }

            // remove rooms with no connectors
            // we could also connect them to the closest room
            for (int i = 0; i < level.BaseSector.Children.Count; i++)
            {
                Sector c = level.BaseSector.Children[i];
                if (c.Connectors.Count == 0)
                {
                    c.DestroySector();
                    UpdateVis(level);
                }

                if (c == null || c.Connectors.Count == 0) {
                    level.BaseSector.RemoveChild(c);
                    i--;
                }
                yield return new WaitForSeconds(.1f);
            }

            yield break;
        }

        
        private static System.Collections.IEnumerator StepAddWalkers(Level l,
                                                       int nWalkers,
                                                       int life,                // walker life time
                                                       float turnChance,        // walker turn chance
                                                       Vector2Int roomSize)     // roomSize on walker explosion
        {
            List<BaseWalker> walkers = GenerateWalkers(l, nWalkers, life, turnChance, roomSize);

            while (walkers.Count > 0)
            {
                for (int i = 0; i < walkers.Count; i++)
                {
                    BaseWalker w = walkers[i];
                    if (w.Walk(l.BaseSector))
                    {
                        i--;
                    }
                }
                yield return new WaitForSeconds(0.1f);

                UpdateVis(l);
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
            body.m_FollowOffset = new Vector3(0f, 7f, -13f);

            var aim = vcam.AddCinemachineComponent<Cinemachine.CinemachineComposer>();
            //aim.
        }

        /////////////////////////////
        /// UTILS
        /// 
        public static List<BaseWalker> GenerateWalkers(Level l,
                                                       int nWalkers,
                                                       int life,                // walker life time
                                                       float turnChance,        // walker turn chance
                                                       Vector2Int roomSize)     // roomSize on walker explosion

        {
            List<BaseWalker> walkers = new List<BaseWalker>();
            for (int i = 0; i < nWalkers; i++)
            {
                Vector2Int pos = new Vector2Int(Random.Range(0, l.Size.x), Random.Range(0, l.Size.y));
                Vector2Int sz = roomSize;
                BaseWalker walker = new KamikazeWalker(
                    pos, 
                    Random.Range(Mathf.Max(2, life/2), life+1),
                    turnChance, 
                    new Vector2Int(Random.Range(sz.x/2, sz.x),
                                   Random.Range(sz.y/2, sz.y))
                );
                walker.OnDeath += delegate (BaseWalker w) { walkers.Remove(w); };
                walkers.Add(walker);
            }
            return walkers;
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

        public static void SetCell(Level l, Vector2Int p, int code)
        {
            if (!IsValidPosition(l, p) || code < l.GetCell(p.x, p.y))
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