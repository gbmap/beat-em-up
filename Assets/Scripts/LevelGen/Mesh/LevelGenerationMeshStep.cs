using UnityEngine;
using System;
using System.Linq;
using Catacumba.Entity;

namespace Catacumba.LevelGen.Mesh
{
    public interface ILevelGenerationMeshStep
    {
        void Run(LevelGenBiomeConfig cfg, Level level, GameObject root);
    }

    public class LevelGenerationMeshStepRooms : ILevelGenerationMeshStep
    {
        void ILevelGenerationMeshStep.Run(LevelGenBiomeConfig cfg, Level level, GameObject root)
        {
            foreach (Sector sec in level.BaseSector.Children)
            {
                GenerateRoom(sec, cfg.GetRoomConfig(sec.Code), root);
            }
        }

        void GenerateRoom(Sector sec, LevelGenRoomConfig cfg, GameObject root)
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

        void ILevelGenerationMeshStep.Run(LevelGenBiomeConfig cfg, Level level, GameObject root)
        {
            GenerateHall(level, cfg, this.floorRoot, this.wallRoot);
        }

        private static void GenerateHall(Level l,
                                         LevelGenBiomeConfig cfg,
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
                    Utils.CheckOneSidedWalls(param.sector.Level.BaseSector, hallCfg, cellSize, wallRoot, param.cellPosition, hallCfg.EnvironmentMaterial);
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
        void ILevelGenerationMeshStep.Run(LevelGenBiomeConfig cfg, Level level, GameObject root)
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
        }
    }

    public class LevelGenerationMeshStepDoors : ILevelGenerationMeshStep
    {
        void ILevelGenerationMeshStep.Run(LevelGenBiomeConfig cfg, Level level, GameObject root)
        {
            System.Collections.Generic.List<GameObject> doorsSpawned = new System.Collections.Generic.List<GameObject>();

            Action<Utils.SectorCellIteration> checkDoors = delegate(Utils.SectorCellIteration iteration)
            {
                if (iteration.cell != LevelGeneration.ECellCode.Door)
                    return;


                var cell    = iteration.sector.Level.GetSectorAt(iteration.cellPosition).Code;
                var roomCfg = cfg.GetRoomConfig(cell);

                EDirectionBitmask directions = Utils.CheckNeighbors(iteration.sector, iteration.cellPosition, SelectDoors, iteration.layer);

                Utils.PutWallParams p = new Utils.PutWallParams()
                {
                    sector      = iteration.sector,
                    root        = root,
                    cfg         = roomCfg,
                    directions  = directions,
                    prefab      = roomCfg.DoorWalls[0],
                    cellSize    = roomCfg.Floors[0].GetComponent<Renderer>().bounds.size,
                    namePreffix = "D",
                    position    = iteration.cellPosition,
                    material    = roomCfg.EnvironmentMaterial
                };
                NeighborObjects doors = Utils.PutWall(p);

                foreach (var value in doors.Values) {
                    doorsSpawned.Add(value);
                }
            };

            Utils.IterateSector(level.BaseSector, checkDoors, ELevelLayer.Doors);

            foreach (GameObject door in doorsSpawned) {
                Vector3 pos = door.GetComponentInChildren<Renderer>().bounds.center;
                Collider[] collisions = Physics.OverlapSphere(pos, 0.1f,  1<< LayerMask.NameToLayer("Entities"));

                foreach (var collider in collisions) {
                    if (collider.gameObject.name[0] != 'D') {
                        GameObject.Destroy(collider.gameObject);
                    }
                }
            }
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

}