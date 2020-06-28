using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Level
{
    /*
     * Walks randomly around the dungeon until it explodes and
     * creates a room.
     */
    public class LevelWalker
    {
        public EDirection Direction;
        public Vector2Int Position;
        public int Life;
        public float TurnChance;
        public Vector2Int RoomSize;

        public enum EDirection
        {
            Up,
            Right,
            Down,
            Left
        }

        public System.Action<LevelWalker> OnDeath;

        public LevelWalker(Vector2Int pos, int life, float turnChance, Vector2Int explosionSz)
        {
            Direction = (EDirection)Random.Range(0, 4);
            Position = pos;
            Life = life;
            TurnChance = turnChance;
            RoomSize = explosionSz;
        }

        public bool Walk(ref Level l)
        {
            try
            {
                //l.Map[Position.x, Position.y] = 1;
                LevelGeneration.SetCell(l, Position, LevelGeneration.CODE_HALL);

                Vector2Int newPos = Position + DirectionVectors[Direction];
                if (LevelGeneration.IsValidPosition(l, newPos))
                {
                    Position = newPos;
                    Life--;

                    if (Life <= 0)
                    {
                        Room r = new Room()
                        {
                            Position = Position,
                            Size = RoomSize
                        };
                        LevelGeneration.CreateRoom(l, r);

                        OnDeath?.Invoke(this);
                        return true;
                    }

                    if (Random.value < TurnChance)
                        ChangeDirection();
                }
                else
                {
                    Direction = (EDirection)(((int)Direction + 1) % 4);
                }
                return false;
            }
            catch
            {
                Life = 0;
                return false;
            }
        }

        public void ChangeDirection()
        {
            int d = (((int)Direction + (int)Mathf.Sign(Random.value - .5f)) % 4);
            if (d < 0) d = 3;
            Direction = (EDirection)d;
        }

        private static Dictionary<EDirection, Vector2Int> DirectionVectors = new Dictionary<EDirection, Vector2Int>
        {
            { EDirection.Down, Vector2Int.down },
            { EDirection.Left, Vector2Int.left },
            { EDirection.Right, Vector2Int.right },
            { EDirection.Up, Vector2Int.up }
        };
    }

    public class Room
    {
        public Vector2Int Position;
        public Vector2Int Size;
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
        public int[,] Map;

        public List<Room> Rooms = new List<Room>();

        public Level(Vector2Int size)
        {
            Map = new int[size.x, size.y];
        }
    }

    public class LevelGeneration : SimpleSingleton<LevelGeneration>
    {
        public const int CODE_EMPTY = 0;
        public const int CODE_HALL = 1;
        public const int CODE_ROOM = 2;
        
        public Vector2Int LevelSize = new Vector2Int(50, 50);

        [Range(1, 10)]
        public int Walkers = 1;

        public Vector2Int WalkerRoomSize = new Vector2Int(5, 5);

        private void Awake()
        {
            Generate(LevelSize, Walkers, null, true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                Generate(LevelSize, Walkers, null, true);
            }
        }

        public static List<LevelWalker> GenerateWalkers(Level l, int nWalkers)
        {
            List<LevelWalker> walkers = new List<LevelWalker>();
            for (int i = 0; i < nWalkers; i++)
            {
                Vector2Int pos = new Vector2Int(Random.Range(0, l.Size.x), Random.Range(0, l.Size.y));
                Vector2Int sz = Instance.WalkerRoomSize;
                LevelWalker walker = new LevelWalker(pos, 
                    Random.Range(8, 20), 
                    0.25f, 
                    new Vector2Int(Random.Range(sz.x/2, sz.x),
                                   Random.Range(sz.y/2, sz.y))
                );
                walker.OnDeath += delegate (LevelWalker w) { walkers.Remove(w); };
                walkers.Add(walker);
            }
            return walkers;
        }

        public static System.Collections.IEnumerator CGenerate(Vector2Int size,
                                                       int nWalkers, 
                                                       System.Action<Level> OnCompleted,
                                                       bool waitInput = true)
        {

            Debug.Log("Level generation started.");

#if UNITY_EDITOR
            LevelGenVisualizer[] vis = FindObjectsOfType<LevelGenVisualizer>();
#endif

            Level level = new Level(size);
            List<LevelWalker> walkers = GenerateWalkers(level, nWalkers);

            while (walkers.Count > 0)
            {
                for (int i = 0; i < walkers.Count; i++)
                {
                    LevelWalker w = walkers[i];
                    if (w.Walk(ref level))
                    {
                        i--;
                    }
                }
                /*
                while (Wait)
                {
                    Wait = true;
                    yield return null;
                }
                */
                //yield return new WaitForSeconds(1f);

#if UNITY_EDITOR
                System.Array.ForEach(vis, v => v.UpdateTexture(level));
#endif
            }

            OnCompleted?.Invoke(level);

            string s = "";
            for (int y = 0; y < level.Size.y; y++)
            {
                for (int x = 0; x < level.Size.x; x++)
                {
                    if (level.Map[x, y] == 1) s += "1";
                    else s += "0";
                }
                s += System.Environment.NewLine;
            }
            Debug.Log(s);

            Debug.Log("Level generation ended.");
            yield break;
        }

        public static void Generate(Vector2Int size,
                                    int nWalkers,
                                    System.Action<Level> OnCompleted,
                                    bool waitInput = true)
        {
            Instance.StartCoroutine(CGenerate(size, nWalkers, OnCompleted, waitInput));
        }

        /////////////////////////////
        /// UTILS

        public static bool IsValidPosition(Level l, Vector2Int p)
        {
            return (p.x >= 0 && p.x < l.Size.x) &&
                 (p.y >= 0 && p.y < l.Size.y);
        }

        public static void SetCell(Level l, Vector2Int p, int code)
        {
            if (!IsValidPosition(l, p))
                return;

            l.Map[p.x, p.y] = code;
        }

        public static void CreateRoom(Level l, Room r, int code = CODE_ROOM)
        {
            for (int x = -r.Size.x/2; x < r.Size.x/2; x++)
            {
                for (int y = -r.Size.y/2; y < r.Size.y/2; y++)
                {
                    Vector2Int p = new Vector2Int(r.Position.x + x, r.Position.y + y);
                    SetCell(l, p, code);
                }
            }
            l.Rooms.Add(r);
        }

    }
}