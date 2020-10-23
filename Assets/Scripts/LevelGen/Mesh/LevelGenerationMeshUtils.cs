using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Catacumba.LevelGen.Mesh
{
    public class NeighborObjects : System.Collections.Generic.Dictionary<EDirectionBitmask, GameObject>
    { }

    public static class Utils
    {
        public static Vector3 LevelToWorldPos(Vector2Int pos, Vector3 cellSize)
        {
            return new Vector3(pos.x * cellSize.x, 0f, pos.y * cellSize.z);
        }

        public static void SetMaterialInObjects(IEnumerable<GameObject> instances, Material mat)
        {
            foreach (GameObject obj in instances)
                SetMaterialInObject(obj, mat);
        }

        public static void SetMaterialInObject(GameObject obj, Material mat)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            System.Array.ForEach(renderers, r => r.material = mat);
        }

        public class CheckNeighborsComparerParams
        {
            public Sector sector;
            public ELevelLayer layer;
            public LevelGeneration.ECellCode originalCell;
            public LevelGeneration.ECellCode neighborCell;
            public Vector2Int originalPosition;
            public Vector2Int neighborPosition;
            public EDirectionBitmask direction;

            public CheckNeighborsComparerParams() {}

            public CheckNeighborsComparerParams(Sector sector, 
                                                Vector2Int originalPosition,
                                                LevelGeneration.ECellCode originalCell,
                                                Vector2Int neighborPosition,
                                                LevelGeneration.ECellCode neighborCell,
                                                ELevelLayer layer,
                                                EDirectionBitmask direction)
            {
                this.sector           = sector;
                this.layer            = layer;
                this.originalPosition = originalPosition;
                this.neighborPosition = neighborPosition;
                this.originalCell     = originalCell;
                this.neighborCell     = neighborCell;
                this.direction        = direction;
            }
        }

        /*
        *   Iterates over neighbors at position <p> and compares
        *   with neighbor by using the provided <comparer> function.
        *
        *   Returns a bitmask with directions where comparer == true.
        */
        public static EDirectionBitmask CheckNeighbors(Sector l,
                                                       Vector2Int p,
                                                       Func<CheckNeighborsComparerParams, bool> comparer,
                                                       ELevelLayer layer = ELevelLayer.All)
        {
            EDirectionBitmask   directions     = EDirectionBitmask.None;
            Vector2Int[]        vecDirections  = GenerateNeighborOffsets(p);
            EDirectionBitmask[] enumDirections = GenerateNeighborDirections();
            
            LevelGeneration.ECellCode originalCell = l.GetCell(p, layer);

            for (int i = 0; i < vecDirections.Length; i++)
            {
                LevelGeneration.ECellCode eDir = l.Level.GetCell(l.GetAbsolutePosition(vecDirections[i]), layer);
                CheckNeighborsComparerParams param = new CheckNeighborsComparerParams
                {
                    sector           = l,
                    layer            = layer,
                    originalPosition = p,
                    originalCell     = originalCell,
                    neighborPosition = vecDirections[i],
                    neighborCell     = eDir,
                    direction        = enumDirections[i]
                };

                if (comparer(param))
                    DirectionHelper.Set(ref directions, enumDirections[i]);
            }

            return directions;
        }

        public static EDirectionBitmask CheckNeighborsGlobal(Sector sec,
                                                            Vector2Int pos,
                                                            Func<CheckNeighborsComparerParams, bool> comparer,
                                                            ELevelLayer layer = ELevelLayer.All)
        {
            return CheckNeighbors(sec.Level.BaseSector, sec.GetAbsolutePosition(pos), comparer, layer);
        }

        private static Vector2Int[] GenerateNeighborOffsets(Vector2Int p)
        {
            Vector2Int[] vecDirections = {
                new Vector2Int(p.x, p.y - 1),
                new Vector2Int(p.x, p.y + 1),
                new Vector2Int(p.x-1, p.y),
                new Vector2Int(p.x+1, p.y)
            };
            return vecDirections;
        }

        private static EDirectionBitmask[] GenerateNeighborDirections()
        {
            EDirectionBitmask[] enumDirections = {
                EDirectionBitmask.Down,
                EDirectionBitmask.Up,
                EDirectionBitmask.Left,
                EDirectionBitmask.Right
            };
            return enumDirections;
        }

        public class SectorCellIteration
        {
            public int iterationNumber { get; set; }
            public LevelGeneration.ECellCode cell {get; set;}
            public Vector2Int cellPosition { get; set; }
            public Sector sector {get; set; }
            public ELevelLayer layer { get; set; }
        }

        public class SectorIterationArgs
        {
            public Sector Sector;
            public Action<SectorCellIteration>[] Functions;
            public ELevelLayer Layer;
        }

        /*
        *   Iterates over every cell in <sector> and
        *   runs <functions> with the cell.
        */
        public static void IterateSector(Sector sector,
                                         Action<SectorCellIteration> iterator,
                                         ELevelLayer layer = ELevelLayer.All)
        {
            IterateSector(sector, new Action<SectorCellIteration>[] { iterator }, layer);
        }

        public static void IterateSector(Sector sector, 
                                         Action<SectorCellIteration>[] functions,
                                         ELevelLayer layer = ELevelLayer.All)
        {
            Vector2Int pos = sector.Pos; //sec.GetAbsolutePosition(sec.Pos);
            Vector2Int sz = sector.Size;
            
            int i = 0;
            for (int x = 0; x < sector.Size.x; x++)
            {
                for (int y = 0; y < sector.Size.y; y++)
                {
                    Vector2Int cellPosition = new Vector2Int(x, y);
                    LevelGeneration.ECellCode cell = sector.GetCell(cellPosition, layer);

                    SectorCellIteration param = new SectorCellIteration()
                    {
                        iterationNumber = i,
                        cell = cell,
                        cellPosition = cellPosition,
                        sector = sector,
                        layer = layer
                    };
                    Array.ForEach(functions, function => function(param));

                    i++;
                }
            }
        }

        public struct PutWallParams
        {
            //public Level level;
            public Sector sector;
            public LevelGenRoomConfig cfg; 
            public GameObject prefab; 
            public Vector3 cellSize;
            public GameObject root;
            public Vector2Int position;
            public EDirectionBitmask directions;
            public Material material;
            public string namePreffix; 
            public bool shouldCollide;
        }

        public static NeighborObjects PutWall(PutWallParams p, bool removeExisting = false)
        {
            Vector3 position = Vector3.zero;
            float angle = 0f;
            string suffix = "";
            
            NeighborObjects walls = new NeighborObjects();

            EDirectionBitmask[] possibleDirections = DirectionHelper.GetValues();
            foreach (var dir in possibleDirections)
            {
                if (!DirectionHelper.IsSet(p.directions, dir))
                    continue;

                position = LevelToWorldPos(p.position+DirectionHelper.ToPrefabOffset(dir), p.cellSize);
                angle = DirectionHelper.ToAngle(dir);
                suffix = DirectionHelper.GetName(dir);

                var obj = GameObject.Instantiate(p.prefab, p.root.transform);
                obj.name = string.Format("{0}_{1}_{2}_{3}", p.namePreffix, p.position.x, p.position.y, suffix);
                obj.transform.localPosition = position;
                obj.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                obj.layer = LayerMask.NameToLayer("Level");
                obj.tag = "Level";

                SetMaterialInObject(obj, p.material);
                AddVisibilityComponent(obj, p.sector.GetAbsolutePosition(p.position));


                if (p.shouldCollide)
                {
                    obj.AddComponent<BoxCollider>();
                    var obstacle = obj.AddComponent<NavMeshObstacle>();
                    obstacle.carving = true;
                }

                if (removeExisting) {
                    Vector3  checkCollisionWithExisting = obj.GetComponentInChildren<Renderer>().bounds.center;
                    Collider[] collisions               = Physics.OverlapSphere(checkCollisionWithExisting, 0.1f, 1 << LayerMask.NameToLayer("Entities"));
                    foreach( var collision in collisions) {
                        // Hack imbecil pra impedir que o outro lado da porta seja removido
                        if (collision.gameObject.name[0] != p.namePreffix[0]) {
                            GameObject.Destroy(collision.gameObject);
                        }
                    }
                }

                walls[dir] = obj;
            }

            return walls;
        }

        public struct PutFloorParams
        {
            public Sector sector;
            public GameObject floorPrefab;
            public Material floorMaterial;
            public Vector3 cellSize;
            public GameObject floorRoot;
            public Vector2Int position;
        }

        public static GameObject PutFloor(PutFloorParams p)
        {
            var floor = GameObject.Instantiate(p.floorPrefab, p.floorRoot.transform);
            floor.name = string.Format("F_{0}_{1}", p.position.x, p.position.y);
            floor.transform.localPosition = new Vector3(p.position.x * p.cellSize.x, 0f, p.position.y * p.cellSize.z);
            floor.layer = LayerMask.NameToLayer("Level");

            SetMaterialInObject(floor, p.floorMaterial);
            AddVisibilityComponent(floor, p.sector.GetAbsolutePosition(p.position));

            return floor;

        }

        private static ComponentVisibility AddVisibilityComponent(GameObject gameObject, Vector2Int cellPosition) 
        {
            return null;
            var vis = gameObject.AddComponent<ComponentVisibility>();
            vis.cellPosition = cellPosition;
            return vis;
        }

        public static GameObject PutFloor(LevelGenRoomConfig cfg,
                                           Vector3 cellSize,
                                           GameObject floorRoot, 
                                           Vector2Int pos,
                                           int floorIndex=-1)
        {
            GameObject prefab = null;
            if (floorIndex == -1)
                prefab = cfg.Floors[UnityEngine.Random.Range(0, cfg.Floors.Length)];
            else
                prefab = cfg.Floors[floorIndex];

            var floor = GameObject.Instantiate(prefab, floorRoot.transform);
            //floor.isStatic = true;
            floor.name = string.Format("F_{0}_{1}", pos.x, pos.y);
            floor.transform.localPosition = new Vector3(pos.x * cellSize.x, 0f, pos.y * cellSize.z);
            floor.layer = LayerMask.NameToLayer("Level");

            SetMaterialInObject(floor, cfg.EnvironmentMaterial);
            return floor;
        }

        public static NeighborObjects CheckOneSidedWalls(Sector sector, 
                                                         LevelGenRoomConfig cfg, 
                                                         Vector3 cellSize, 
                                                         GameObject root, 
                                                         Vector2Int position,
                                                         Material material = null)
        {
            EDirectionBitmask directions = CheckNeighbors(
                sector.Level.BaseSector, 
                sector.GetAbsolutePosition(position),
                (CheckNeighborsComparerParams p) => p.neighborCell <= LevelGeneration.ECellCode.Empty
            );

            PutWallParams param = new PutWallParams
            {
                sector        = sector,
                cellSize      = cellSize,
                cfg           = cfg,
                directions    = directions,
                namePreffix   = "W",
                prefab        = cfg.Walls[UnityEngine.Random.Range(0, cfg.Walls.Length)],
                position      = position,
                root          = root,
                shouldCollide = false,
                material      = material 
            };
            return PutWall(param);
        }

        public static NeighborObjects CheckTwoSidedWalls(Sector sector,
                                                         LevelGenRoomConfig cfg,
                                                         Vector3 cellSize,
                                                         GameObject root,
                                                         Vector2Int position, // relative to sector position
                                                         ELevelLayer layer = ELevelLayer.All,
                                                         Func<CheckNeighborsComparerParams, bool> selector = null,
                                                         Material material = null)
        {
            if (selector == null)
            {
                selector = (CheckNeighborsComparerParams p) => !sector.IsIn(p.neighborPosition); 
                /*selector = (CheckNeighborsComparerParams p) => !p.sector.IsIn(p.neighborPosition) &&
                                                                p.neighborCell > LevelGeneration.ECellCode.Empty);*/
            }

            EDirectionBitmask directions = CheckNeighbors(
                sector, 
                position, 
                selector,
                layer
            );

            PutWallParams param = new PutWallParams
            {
                sector        = sector,
                prefab        = cfg.Walls[0],
                cfg           = cfg,
                cellSize      = cellSize,
                root          = root,
                position      = position,
                directions    = directions,
                namePreffix   = "WD",
                shouldCollide = true,
                material      = material
            };
            NeighborObjects walls = PutWall(param); 

            foreach (var kvp in walls)
            {
                GameObject wall = kvp.Value;
                wall.layer = LayerMask.NameToLayer("Entities");
                //wall.transform.localPosition = wall.transform.localPosition * 0.99f;
                //var data = wall.AddComponent<CharacterData>();
                //data.Stats.Attributes.Vigor = 1;

                //wall.AddComponent<CharacterHealth>();
            }

            return walls;
        }
    }
}