using UnityEngine;
using System;
using System.Linq;
using Catacumba.Entity;
using Catacumba.Data.Level;
using Catacumba.Data;
using System.Collections.Generic;
using System.Collections;
using static Catacumba.LevelGen.LevelGeneration;
using static Catacumba.LevelGen.Mesh.Utils;
using Random = UnityEngine.Random;

namespace Catacumba.LevelGen.Mesh
{
    public interface ILevelGenerationMeshStep
    {
        IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root);
    }

    public class LevelGenerationMeshStepRooms : ILevelGenerationMeshStep
    {
        IEnumerator ILevelGenerationMeshStep.Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            foreach (Sector sec in level.BaseSector.Children)
            {
                GenerateRoom(sec, cfg.GetRoomConfig(sec.Code), root);
            }
            yield return null;
        }

        void GenerateRoom(Sector sec, RoomConfiguration cfg, GameObject root)
        {
            string log = "GENERATING ROOM:\n";
            log += "Position: " + sec.Pos + "\n";
            log += "Size: " + sec.Size + "\n";
            Debug.Log(log);

            Vector3 cellSize = cfg.Floors[0].GetComponent<Renderer>().bounds.size;
            cellSize.y = 0f;

            GameObject roomObject = new GameObject("Room");
            roomObject.transform.SetParent(root.transform);
            roomObject.transform.localPosition = Vector3.zero;
            roomObject.transform.localPosition = Utils.LevelToWorldPos(sec.Pos, cellSize);

            Material roomMaterial = new Material(cfg.EnvironmentMaterial);

            Action<Utils.SectorCellIteration> ac = delegate(Utils.SectorCellIteration param) {
                int x = param.cellPosition.x;
                int y = param.cellPosition.y;
                Vector2Int p = param.cellPosition;
                Vector2Int pos = param.sector.Pos;
                Vector2Int sz = param.sector.Size;

                var obj = Utils.PutFloor(new Utils.PutFloorParams
                {
                    sector        = sec,
                    cellSize      = cellSize,
                    floorMaterial = roomMaterial,
                    floorPrefab   = cfg.Floors[0],
                    floorRoot     = roomObject,
                    position      = p
                });

                // Borders
                if (x == 0 || x == sz.x-1 ||
                    y == 0 || y == sz.y-1)
                {
                    Utils.CheckOneSidedWalls(sec, cfg, cellSize, roomObject, p, roomMaterial);
                    Utils.CheckTwoSidedWalls(sec, cfg, cellSize, roomObject, p, material: roomMaterial);
                }
            };

            Utils.IterateSector(sec, new Action<Utils.SectorCellIteration>[] { ac });
        }
    }

    public class LevelGenerationMeshStepHall : ILevelGenerationMeshStep
    {
        private GameObject floorRoot;
        private GameObject wallRoot;

        public LevelGenerationMeshStepHall(GameObject floorRoot, GameObject wallRoot)
        {
            this.floorRoot = floorRoot;
            this.wallRoot = wallRoot;
        }

        IEnumerator ILevelGenerationMeshStep.Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            GenerateHall(level, cfg, this.floorRoot, this.wallRoot);
            yield return null;
        }

        private static void GenerateHall(Level l,
                                         BiomeConfiguration cfg,
                                         GameObject floorRoot,
                                         GameObject wallRoot)
        {
            var hallCfg = cfg.GetRoomConfig(LevelGeneration.ECellCode.Hall);
            Vector3 cellSize = hallCfg.Floors[0].GetComponent<Renderer>().bounds.size;

            Func<Utils.CheckNeighborsComparerParams, bool> comparer = delegate(Utils.CheckNeighborsComparerParams p)
            {
                bool put = p.neighborCell > LevelGeneration.ECellCode.Empty &&
                           p.neighborCell != LevelGeneration.ECellCode.Hall;
                return put;
            };

            Action<Utils.SectorCellIteration> hallStep = delegate(Utils.SectorCellIteration param)
            {
                if (param.cell == LevelGeneration.ECellCode.Hall ||
                    param.cell == LevelGeneration.ECellCode.Prop ||
                    param.cell == LevelGeneration.ECellCode.Enemy)
                {

                    Utils.PutFloor(new Utils.PutFloorParams
                    {
                        sector        = param.sector,
                        position      = param.cellPosition,
                        cellSize      = cfg.CellSize(),
                        floorPrefab   = hallCfg.Floors[0],
                        floorMaterial = hallCfg.EnvironmentMaterial,
                        floorRoot     = floorRoot
                    });

                    Utils.CheckOneSidedWalls(param.sector.Level.BaseSector, 
                                             hallCfg, 
                                             cellSize, 
                                             wallRoot, 
                                             param.cellPosition, 
                                             hallCfg.EnvironmentMaterial);
                    
                    
                    Utils.CheckTwoSidedWalls(param.sector.Level.BaseSector, 
                                             hallCfg, 
                                             cellSize, 
                                             wallRoot, 
                                             param.cellPosition, 
                                             ELevelLayer.Hall | ELevelLayer.Rooms,
                                             //param.layer,
                                             comparer, hallCfg.EnvironmentMaterial);
                }
            };

            Utils.IterateSector(l.BaseSector, new Action<Utils.SectorCellIteration>[] { hallStep }, ELevelLayer.All &~ ELevelLayer.Doors);
        }
    }

    public class LevelGenerationMeshStepGroupWalls : ILevelGenerationMeshStep
    {
        IEnumerator ILevelGenerationMeshStep.Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            var walls = GameObject.FindGameObjectsWithTag("Level").Where(g => g.gameObject.name.Contains("WD")).ToArray();
            foreach (GameObject wallObj in walls)
            {
                if (wallObj.transform.parent.name.Contains("WD"))
                    continue;

                Vector3 center = wallObj.GetComponent<Renderer>().bounds.center;
                Collider[] colliders = Physics.OverlapSphere(center, 1.5f, 1 << LayerMask.NameToLayer("Entities")).Where(g => g.name.Contains("WD")).ToArray();

                foreach (Collider c in colliders)
                {
                    if (c.gameObject == wallObj)
                        continue;

                    c.transform.parent = wallObj.transform;
                    
                    var childObject = c.gameObject;
                    var health = childObject.GetComponent<CharacterHealth>();
                    if (health)
                        GameObject.Destroy(health);

                    var data = childObject.GetComponent<CharacterData>();
                    if (data)
                        GameObject.Destroy(data);
                }
            }
            yield return null;
        }
    }

    public class LevelGenerationMeshStepDoors : ILevelGenerationMeshStep
    {
        IEnumerator ILevelGenerationMeshStep.Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            System.Collections.Generic.List<GameObject> doorsSpawned = new System.Collections.Generic.List<GameObject>();

            Action<Utils.SectorCellIteration> CheckDoors = delegate(Utils.SectorCellIteration iteration)
            {
                if (iteration.cell != LevelGeneration.ECellCode.Door)
                    return;

                var cell    = iteration.sector.Level.GetSectorAt(iteration.cellPosition).Code;
                var roomCfg = cfg.GetRoomConfig(cell);

                EDirectionBitmask directions = Utils.CheckNeighbors(iteration.sector,
                                                                    iteration.cellPosition, 
                                                                    SelectDoors, 
                                                                    iteration.layer);

                Utils.PutWallParams p = new Utils.PutWallParams()
                {
                    sector         = iteration.sector,
                    root           = root,
                    cfg            = roomCfg,
                    directions     = directions,
                    prefab         = roomCfg.Doors[0],
                    cellSize       = roomCfg.Floors[0].GetComponent<Renderer>().bounds.size,
                    namePreffix    = "D",
                    position       = iteration.cellPosition,
                    removeExisting = true,
                    material       = roomCfg.EnvironmentMaterial
                };
                NeighborObjects doors = Utils.PutWall(p);

                foreach (var value in doors.Values) {
                    doorsSpawned.Add(value);
                }
            };

            Utils.IterateSector(level.BaseSector, CheckDoors, ELevelLayer.Doors);

            foreach (GameObject door in doorsSpawned) {
                Vector3 pos = door.GetComponentInChildren<Renderer>().bounds.center;
                Collider[] collisions = Physics.OverlapSphere(pos, 0.1f,  1<< LayerMask.NameToLayer("Level"));

                foreach (var collider in collisions) {
                    if (collider.gameObject.name[0] != 'D') {
                        GameObject.Destroy(collider.gameObject);
                    }
                }
            }

            yield return null;
        }

        bool SelectDoors(Utils.CheckNeighborsComparerParams param)
        {
            var level = param.sector.Level;
            
            Vector2Int globalOriginalPos = param.sector.GetAbsolutePosition(param.originalPosition);
            Vector2Int globalNeighborPos = param.sector.GetAbsolutePosition(param.neighborPosition);

            bool bothDoors = param.originalCell == LevelGeneration.ECellCode.Door && 
                             param.neighborCell == LevelGeneration.ECellCode.Door;

            bool differentSectors = level.GetSectorAt(globalOriginalPos).Id != level.GetSectorAt(globalNeighborPos).Id;

            return bothDoors && differentSectors;
        }
    }

    public class LevelGenerationMeshStepGeometry : ILevelGenerationMeshStep
    {
        public IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            Action<Utils.SectorCellIteration> Iterator = delegate (Utils.SectorCellIteration it)
            {
                var roomCfg = cfg.GetRoomConfig(it.cell);

                bool floor = it.cell != LevelGeneration.ECellCode.Empty;
                if (floor)
                {
                    Utils.PutFloor(new Utils.PutFloorParams
                    {
                        sector = it.sector,
                        floorPrefab = roomCfg.Floors[Random.Range(0, roomCfg.Floors.Length)],
                        floorMaterial = roomCfg.EnvironmentMaterial,
                        cellSize = roomCfg.CellSize(),
                        floorRoot = root,
                        position = it.cellPosition,
                        rotation = new Vector3(0f, Random.Range(0, 4)*90, 0f)
                    });
                }

                Func<Utils.CheckNeighborsComparerParams, bool> WallCheck = delegate(Utils.CheckNeighborsComparerParams p)
                {
                    bool differentSectors = level.GetSectorAt(p.originalPosition) != level.GetSectorAt(p.neighborPosition);
                    bool differentTypes = p.originalCell != p.neighborCell;

                    bool isOriginDoor = p.sector.GetCell(p.originalPosition, ELevelLayer.Doors) == LevelGeneration.ECellCode.Door;
                    bool isNeighborDoor = p.sector.GetCell(p.neighborPosition, ELevelLayer.Doors) == LevelGeneration.ECellCode.Door;

                    bool isDoor = isOriginDoor && isNeighborDoor && differentSectors;

                    if ((differentTypes || differentSectors) && !isDoor)
                    {
                        Utils.PutWall(new Utils.PutWallParams
                        {
                            sector          = it.sector,
                            root            = root,
                            cfg             = roomCfg,
                            directions      = p.direction,
                            prefab          = roomCfg.Walls[0],
                            cellSize        = roomCfg.CellSize(),
                            namePreffix     = "W",
                            position        = it.cellPosition,
                            material        = roomCfg.EnvironmentMaterial,
                            shouldCollide   = true
                        });
                    }
                    else if (isDoor)
                    {
                        Utils.PutWall(new Utils.PutWallParams
                        {
                            sector          = it.sector,
                            root            = root,
                            cfg             = roomCfg,
                            directions      = p.direction,
                            prefab          = roomCfg.Doors[0],
                            cellSize        = roomCfg.CellSize(),
                            namePreffix     = "D",
                            position        = it.cellPosition,
                            material        = roomCfg.EnvironmentMaterial,
                            shouldCollide   = true,
                            addNavMeshObstacle = false
                        });
                    }

                    return (differentTypes || differentSectors) && !isDoor;
                };
                Utils.CheckNeighbors(it.sector, 
                                     it.cellPosition, 
                                     WallCheck, 
                                     ELevelLayer.Hall | ELevelLayer.Rooms);
            };

            Utils.IterateSector(level.BaseSector, Iterator, ELevelLayer.All);
            yield return null;
        }

    }

    public class LevelGenerationMeshStepCleanColliders : ILevelGenerationMeshStep
    {
        public IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            var colliders = root.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.gameObject.name[0] == 'D')
                    GameObject.Destroy(collider);
            }
            yield return null;
        }
    }

    public class LevelGenerationMeshStepProps : ILevelGenerationMeshStep
    {
        private class PropPlacementBufferItem
        {
            public List<GameObject> PropInstances = new List<GameObject>();
            public GameObject FloorObject;
            public EDirectionBitmask Directions;
        }

        public LevelGenerationMeshStepProps()
        {
        }

        public IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            List<PropPlacementBufferItem> bufferItems = new List<PropPlacementBufferItem>();

            Mesh.Utils.IterateSector(level.BaseSector, (it) =>
            {
                if (it.cell != LevelGeneration.ECellCode.Prop) return;

                RoomConfiguration roomCfg = cfg.GetRoomConfig(level.GetCell(it.cellPosition, ELevelLayer.Hall | ELevelLayer.Rooms));
                
                GameObject floorObject = GameObject.Find($"F_{it.cellPosition.x}_{it.cellPosition.y}");
                if (floorObject == null) return;

                EDirectionBitmask differentNeighbors = GetDifferentNeighbors(it.sector, it.cellPosition);
                List<GameObject>  propInstances      = new List<GameObject>();
                for (int i = 0; i < 3; i++)
                {
                    CharacterPoolItem propCfg = roomCfg.PropPool.GetRandom();
                    propInstances.Add(CharacterManager.SpawnProp(propCfg.Config, Vector2Int.zero));
                }

                bufferItems.Add(new PropPlacementBufferItem
                {
                    PropInstances = propInstances,
                    FloorObject   = floorObject,
                    Directions    = differentNeighbors
                });
            }, ELevelLayer.Props);
            yield return new WaitForSeconds(0.25f);

            foreach (PropPlacementBufferItem item in bufferItems)
                PropPlacement.OrganizeProps(item.FloorObject, item.Directions, item.PropInstances.ToArray());
        }

        private EDirectionBitmask GetDifferentNeighbors(Sector sector, Vector2Int cellPosition)
        {
            ECellCode cell = sector.GetCell(cellPosition);
            EDirectionBitmask differentCells = EDirectionBitmask.None;
            System.Func<Utils.CheckNeighborsComparerParams, bool> checker = delegate(Utils.CheckNeighborsComparerParams arg) 
            { 
                return CheckNeighbors(arg, ref differentCells);
            };
            Utils.CheckNeighbors(sector, cellPosition, checker, ELevelLayer.Hall | ELevelLayer.Rooms); 
            return differentCells;
        }

        private bool CheckNeighbors(Utils.CheckNeighborsComparerParams arg, ref EDirectionBitmask directions)
        {
            bool isDoor = arg.sector.GetCell(arg.neighborPosition, ELevelLayer.Doors) > ECellCode.Empty;
            if (arg.neighborCell != arg.originalCell && !isDoor)
                DirectionHelper.Set(ref directions, arg.direction);
            return true;
        }
    }

    public class LevelGenerationMeshStepTraps : ILevelGenerationMeshStep
    {
        BiomeTrapConfiguration _config;
        float _trapChance;

        public LevelGenerationMeshStepTraps(
            BiomeTrapConfiguration config, 
            float trapChance = 1.0f
        ) {
            _config = config;
            _trapChance = trapChance;
        }

        public IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            Utils.IterateSector(level.BaseSector, delegate(SectorCellIteration it)
            {
                TrapConfiguration[] traps = _config.GetAvailableTrapsAt(level, it.cellPosition, ECellCode.Hall);
                if (traps.Length > 0 && Random.value < _trapChance)
                {
                    TrapConfiguration t = traps[Random.Range(0, traps.Length)];
                    CharacterManager.SpawnTrap(t.Trap, it.cellPosition);
                }
            });

            yield return null;
        }
    }

    public class LevelGenerationMeshStepBoss : ILevelGenerationMeshStep
    {
        private CharacterConfiguration _bossConfig;

        public LevelGenerationMeshStepBoss(CharacterConfiguration bossConfiguration)
        {
            _bossConfig = bossConfiguration;
        }

        public LevelGenerationMeshStepBoss(CharacterPool possibleBosses)
        {
            _bossConfig = possibleBosses.GetRandom().Config;
        }

        public IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            Sector bossSector = null;
            foreach (Sector sector in level.BaseSector.Children)
            {
                if (sector.Code != ECellCode.BossRoom)
                    continue;

                bossSector = sector;
                break;
            }

            if (bossSector == null)
                yield break;

            Vector2Int center = bossSector.GetAbsolutePosition(bossSector.Size/2);
            Entity.CharacterManager.SpawnEnemy(_bossConfig, center.x, center.y);
        }
    }

}