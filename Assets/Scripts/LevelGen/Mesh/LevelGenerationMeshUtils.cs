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

        /*
        *   Iterates over neighbors at position <p> and compares
        *   with neighbor by using the provided <comparer> function.
        *
        *   Returns a bitmask with directions where comparer == true.
        */
        public static EDirectionBitmask CheckNeighbors(Sector l,
                                                        Vector2Int p,
                                                        Func<LevelGeneration.ECellCode, Vector2Int, bool> comparer)
        {
            EDirectionBitmask directions = EDirectionBitmask.None;

            Vector2Int[] vecDirections = {
                new Vector2Int(p.x, p.y - 1),
                new Vector2Int(p.x, p.y + 1),
                new Vector2Int(p.x-1, p.y),
                new Vector2Int(p.x+1, p.y)
            };

            EDirectionBitmask[] enumDirections = {
                EDirectionBitmask.Down,
                EDirectionBitmask.Up,
                EDirectionBitmask.Left,
                EDirectionBitmask.Right
            };

            for (int i = 0; i < vecDirections.Length; i++)
            {
                LevelGeneration.ECellCode eDir = l.GetCell(vecDirections[i]);
                if (comparer(eDir, vecDirections[i]))
                    DirectionHelper.Set(ref directions, enumDirections[i]);
            }

            return directions;
        }

        public class SectorCellIteration
        {
            public LevelGeneration.ECellCode cell {get; set;}
            public Vector2Int cellPosition { get; set; }
            public Sector sector {get; set; }
            public Vector3 cellSize {get; set;}
            public LevelGenRoomConfig cfg {get; set;}
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
                                         Action<SectorCellIteration>[] functions,
                                         ELevelLayer layer = LevelBitmap.AllLayers)
        {
            Vector2Int pos = sector.Pos; //sec.GetAbsolutePosition(sec.Pos);
            Vector2Int sz = sector.Size;

            for (int x = pos.x; x < pos.x + sz.x; x++)
            {
                for (int y = pos.y; y < pos.y + sz.y; y++)
                {
                    Vector2Int cellPosition = new Vector2Int(x, y);
                    LevelGeneration.ECellCode cell = sector.GetCell(cellPosition, layer);

                    SectorCellIteration param = new SectorCellIteration()
                    {
                        cell = cell,
                        cellPosition = cellPosition,
                        sector = sector
                    };
                    Array.ForEach(functions, function => function(param));
                }
            }
        }

        public struct PutWallParams
        {
            public LevelGenRoomConfig cfg; 
            public GameObject prefab; 
            public Vector3 cellSize;
            public GameObject root;
            public Vector2Int position;
            public EDirectionBitmask directions;
            public string namePreffix; 
            public bool shouldCollide;
        }

        public static NeighborObjects PutWall(PutWallParams p)
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

                var renderers = obj.GetComponentsInChildren<Renderer>();
                System.Array.ForEach(renderers, r => r.material = p.cfg.EnvironmentMaterial);

                if (p.shouldCollide)
                {
                    obj.AddComponent<BoxCollider>();
                    var obstacle = obj.AddComponent<NavMeshObstacle>();
                    obstacle.carving = true;
                }

                walls[dir] = obj;
            }

            SetMaterialInObjects(walls.Values, p.cfg.EnvironmentMaterial);

            return walls;
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
                                                          Vector2Int position)
        {
            EDirectionBitmask directions = CheckNeighbors(
                sector.Level.BaseSector, 
                sector.GetAbsolutePosition(position),
                (LevelGeneration.ECellCode c, Vector2Int pos) => c <= LevelGeneration.ECellCode.Empty
            );

            PutWallParams param = new PutWallParams
            {
                cellSize      = cellSize,
                cfg           = cfg,
                directions    = directions,
                namePreffix   = "W",
                prefab        = cfg.Walls[UnityEngine.Random.Range(0, cfg.Walls.Length)],
                position      = sector.GetAbsolutePosition(position),
                root          = root,
                shouldCollide = false
            };
            return PutWall(param);
        }

        public static NeighborObjects CheckTwoSidedWalls(Sector sector,
                                                          LevelGenRoomConfig cfg,
                                                          Vector3 cellSize,
                                                          GameObject root,
                                                          Vector2Int position, // relative to sector position
                                                          Func<LevelGeneration.ECellCode, Vector2Int, bool> selector = null)
        {
            if (selector == null)
                selector = (LevelGeneration.ECellCode c, Vector2Int pos) => !sector.IsIn(pos); 

            EDirectionBitmask directions = CheckNeighbors(
                sector, 
                position, 
                selector
            );

            PutWallParams param = new PutWallParams
            {
                prefab        = cfg.Walls[0],
                cfg           = cfg,
                cellSize      = cellSize,
                root          = root,
                position      = sector.GetAbsolutePosition(position),
                directions    = directions,
                namePreffix   = "WD",
                shouldCollide = true
            };
            NeighborObjects walls = PutWall(param); 

            foreach (var kvp in walls)
            {
                GameObject wall = kvp.Value;
                wall.layer = LayerMask.NameToLayer("Entities");
                //wall.transform.localPosition = wall.transform.localPosition * 0.99f;
                var data = wall.AddComponent<CharacterData>();
                data.Stats.Attributes.Vigor = 1;

                wall.AddComponent<CharacterHealth>();
            }

            return walls;
        }
    }
}