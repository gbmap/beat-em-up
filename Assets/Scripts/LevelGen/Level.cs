using UnityEngine;

namespace Catacumba.LevelGen
{
    public class Level
    {
        public Vector2Int Size
        {
            get
            {
                return new Vector2Int(Map.GetLength(0), Map.GetLength(1));
            }
        }
        private LevelGeneration.ECellCode[,] Map;

        public Vector2Int SpawnPoint;
        public Sector SpawnSector;

        public Sector BaseSector { get; private set; }

        public void SetCell(Vector2Int p, LevelGeneration.ECellCode v, bool overwrite = false)
        {
            SetCell(p.x, p.y, v, overwrite);
        }

        public void SetCell(int x, int y, LevelGeneration.ECellCode v, bool overwrite = false)
        {
            if (overwrite)
                Map[x, y] = v;
            else
                Map[x, y] = (LevelGeneration.ECellCode)Mathf.Max((int)Map[x, y], (int)v);
        }

        public LevelGeneration.ECellCode GetCell(Vector2Int p)
        {
            return GetCell(p.x, p.y);
        }

        public LevelGeneration.ECellCode GetCell(int x, int y)
        {
            try
            {
                return Map[x, y];
            }
            catch (System.IndexOutOfRangeException ex)
            {
                return LevelGeneration.ECellCode.Error;
            }
        }

        public Level(Vector2Int size)
        {
            Map = new LevelGeneration.ECellCode[size.x, size.y];
            BaseSector = new Sector(this, Vector2Int.zero, size, null, LevelGeneration.CODE_EMPTY);
        }
    }
}