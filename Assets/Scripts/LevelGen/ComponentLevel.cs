using System;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.LevelGen 
{
    public class VisibilityMap 
    {
        private float[,] visibilityMap;
        public Vector2Int Size { get; private set; }

        public VisibilityMap(Vector2Int size) 
        {
            this.visibilityMap = new float[size.x, size.y];
            this.Size = size;
        }

        public void FillSector(System.Func<int, int, float> fillFunction, Vector2Int playerCell) 
        {
            Vector2Int xRange = new Vector2Int(
                Mathf.Max(0, playerCell.x-5),
                Mathf.Min(this.visibilityMap.GetLength(0), playerCell.x+5)
            );

            Vector2Int yRange = new Vector2Int(
                Mathf.Max(0, playerCell.y-5),
                Mathf.Min(this.visibilityMap.GetLength(1), playerCell.y+5)
            );

            try
            {
                for (int x = xRange.x; x < xRange.y; x++) 
                {
                    for (int y = yRange.x; y < yRange.y; y++) 
                    {
                        this.visibilityMap[x, y] = Mathf.Clamp01(fillFunction(x, y));
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                Debug.Log(ex.Data);
            }
        }

        public void SetVisibilityAt(Vector2Int position, float value) 
        {
            this.visibilityMap[position.x, position.y] = value;
        }

        public float GetVisibilityAt(Vector2Int position) 
        {
            return 1f;
            return this.visibilityMap[Mathf.Clamp(position.x, 0, Size.x), Mathf.Clamp(position.y, 0, Size.y)]; 
        }

        public void CalculateVisibility(Vector2Int playerCell, Vector3 playerPosition, Vector3 cellSize, float visionDistance = 150f, float visionDecay = 3f)
        {
            Vector2 pposition = new Vector2(playerPosition.x, playerPosition.z);

            FillSector((int x, int y) => { 
                Vector3 worldPos = (Mesh.Utils.LevelToWorldPos(new Vector2Int(x, y), cellSize));
                Vector2 objPos = new Vector2(worldPos.x, worldPos.z);
                objPos.x -= cellSize.x/2;
                objPos.y += cellSize.z/2;
                return Mathf.Pow( (objPos - pposition).sqrMagnitude/visionDistance, visionDecay);
            }, playerCell);
        }
       
    }

    public class LevelEvents 
    {
        public System.Action<Vector2Int> OnPlayerChangedCell;

        // Old sector, new sector
        public System.Action<Sector, Sector> OnPlayerChangedSector;
        public System.Action<VisibilityMap> OnVisibilityMapChanged;
    }

    public class ComponentLevel : MonoBehaviour
    {
        public Level Level { get; private set; }
        public LevelGenBiomeConfig BiomeConfig { get; private set;}
        public VisibilityMap VisibilityMap { get; private set; }
        public LevelEvents Events { get; private set; }

        // Objects that reveal cells in the map.
        private List<GameObject> ObjectsWithSight;

        public Vector2Int PlayerCell;

        public float VisionDistance = 150f;
        public float VisionDecay = 3f;

        public void SetLevel(Level l, LevelGenBiomeConfig config) 
        {
            this.Level            = l;
            this.BiomeConfig      = config;
            this.VisibilityMap    = new VisibilityMap(l.Size);
            this.Events           = new LevelEvents();
            this.ObjectsWithSight = new List<GameObject>();
        }

        public Vector2Int WorldPositionToLevelPosition(Vector3 position) 
        {
            return this.Level.WorldPositionToLevelPosition(position, this.BiomeConfig.CellSize());
        }
        
        void FixedUpdate() 
        {
            if (ObjectsWithSight.Count == 0) {
                var players = GameObject.FindGameObjectsWithTag("Player");
                ObjectsWithSight.AddRange(players);
                return;
            }

            // Temp retarded code
            PlayerCell = WorldPositionToLevelPosition(ObjectsWithSight[0].transform.position);
            this.VisibilityMap.CalculateVisibility(PlayerCell, ObjectsWithSight[0].transform.position, this.BiomeConfig.CellSize(), VisionDistance, VisionDecay);
            this.Events.OnVisibilityMapChanged?.Invoke(this.VisibilityMap);

        }
    }

}