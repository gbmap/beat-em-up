using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.LevelGen
{
    public abstract class BaseWalker
    {
        public Sector Sector;
        public EDirection Direction;
        public Vector2Int Position;

        public System.Action<BaseWalker> OnDeath;

        private int code;

        public BaseWalker(Sector s, Vector2Int pos, int code = LevelGeneration.CODE_HALL)
        {
            this.Direction = (EDirection)Random.Range(0, 4);
            this.Position  = pos;
            this.Sector    = s;
            this.code      = code;
        }

        public bool Walk()
        {
            Sector.SetCell(Position, LevelGeneration.ECellCode.Hall);
            Vector2Int newPos = GetNewPosition();

            if (Sector.IsIn(newPos))
            {
                Position = newPos;
                if (OnMoved())
                {
                    OnDeath?.Invoke(this);
                    return true;
                }
            }

            return false;
        }

        public abstract bool OnMoved();

        public abstract Vector2Int GetNewPosition();

        public void ChangeDirection()
        {
            int d = (((int)Direction + (int)Mathf.Sign(Random.value - .5f)) % 4);
            if (d < 0) d = 3;
            Direction = (EDirection)d;
        }
    }

    /*
        Walks randomly until it explodes and creates a room.
    */
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
        
        private LevelGeneration.ECellCode roomCode;

        public KamikazeWalker(Sector s, 
                              Vector2Int pos,
                              int life,
                              float turnChance,
                              Vector2Int explosionSz, 
                              LevelGeneration.ECellCode roomCode = LevelGeneration.ECellCode.Room,
                              int walkCode = LevelGeneration.CODE_HALL)
            : base(s, pos, walkCode)
        {
            this.Life       = life;
            this.TurnChance = turnChance;
            this.RoomSize   = explosionSz;
            this.roomCode   = roomCode;

            this.OnDeath += SpawnSector;
        }

        public override Vector2Int GetNewPosition()
        {
            var newPos = Position + DirectionVectors[Direction];
            if (!Sector.IsIn(newPos))
            {
                Direction = (EDirection)(((int)Direction + 1) % 4);
                return GetNewPosition();
            }
            return newPos;
        }

        public override bool OnMoved()
        {
            if (Random.value < TurnChance)
                ChangeDirection();

            if (Sector.GetCell(Position) == LevelGeneration.CODE_EMPTY)
                Life--;

            bool isCollidingWithRoom = false;
            if (Life <= 0)
            {
                Rect a = new Rect(this.Position, this.RoomSize);
                foreach (Sector room in this.Sector.Children)
                {
                    Rect b = new Rect(room.Pos, room.Size);
                    isCollidingWithRoom |= LevelGeneration.AABB(a, b);
                }
            }
            return Life <= 0 && !isCollidingWithRoom;
        }

        protected void SpawnSector(BaseWalker w)
        {
            Sector sec = new Sector(Sector.Level, this.Position, this.RoomSize, this.Sector, this.roomCode);
            Sector.CreateSector(sec);
        }
    }

    /*
        Walks from a position towards another position leaving a corridor.
    */
    public class TargetedWalker : BaseWalker
    {
        public Vector2Int TargetPos { get; private set; }
        public List<EDirection> Path { get; private set; }

        public Connector Connector { get; private set; }

        public TargetedWalker(Sector s, 
                              Vector2Int pos,
                              Vector2Int targetPos, 
                              int code = LevelGeneration.CODE_HALL)
            : base(s, pos, code)
        {
            this.TargetPos = targetPos;
            this.Path      = new List<EDirection>();
            this.Connector = new Connector(pos, targetPos);
        }

        public override Vector2Int GetNewPosition()
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

        public override bool OnMoved()
        {
            return TargetPos - Position == Vector2Int.zero;
        }
    }

    /*
        Spawns a room where it was spawned then walks
        towards <targetPos>.
    */    
    public class WalkerInverseKamikazeTargeted : TargetedWalker
    {
        public Vector2Int RoomSize;

        private LevelGeneration.ECellCode roomCode;

        public WalkerInverseKamikazeTargeted(Sector s,
                                             Vector2Int pos,
                                             Vector2Int targetPos,
                                             Vector2Int roomSize,
                                             LevelGeneration.ECellCode roomCode = LevelGeneration.ECellCode.Room,
                                             int walkCode = LevelGeneration.CODE_HALL)
            : base(s, pos, targetPos, walkCode)
        {
            this.roomCode = roomCode;
            this.RoomSize = roomSize;

            SpawnSector(this);
        }

        void SpawnSector(BaseWalker w)
        {
            Sector sec = new Sector(Sector.Level, 
                                    this.Position, 
                                    this.RoomSize, 
                                    this.Sector, 
                                    this.roomCode);
            Sector.CreateSector(sec);
        }
    }

}