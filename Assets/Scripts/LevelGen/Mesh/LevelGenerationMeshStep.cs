using UnityEngine;
using System;
using System.Linq;

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

            Action<Utils.SectorCellIteration> ac = delegate(Utils.SectorCellIteration param) {
                int x = param.cellPosition.x;
                int y = param.cellPosition.y;
                Vector2Int p = param.cellPosition;
                Vector2Int pos = param.sector.Pos;
                Vector2Int sz = param.sector.Size;

                Utils.PutFloor(cfg, cellSize, roomObject, p);

                // Borders
                if (x == pos.x || x == pos.x+sz.x-1 ||
                    y == pos.y || y == pos.y+sz.y-1)
                {
                    Utils.CheckOneSidedWalls(sec, cfg, cellSize, roomObject, p-pos);
                    Utils.CheckTwoSidedWalls(sec, cfg, cellSize, roomObject, p-pos);
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

            Func<LevelGeneration.ECellCode, Vector2Int, bool> comparer = delegate(LevelGeneration.ECellCode cellCode, Vector2Int p)
            {
                bool put = cellCode != LevelGeneration.ECellCode.Empty &&
                           cellCode != LevelGeneration.ECellCode.Enemy &&
                           cellCode != LevelGeneration.ECellCode.Hall &&
                           cellCode != LevelGeneration.ECellCode.Prop;
                return put;
            };

            Action<Utils.SectorCellIteration> hallStep = delegate(Utils.SectorCellIteration param)
            {
                if (param.cell == LevelGeneration.ECellCode.Hall ||
                    param.cell == LevelGeneration.ECellCode.Prop ||
                    param.cell == LevelGeneration.ECellCode.Enemy)
                {
                    Utils.PutFloor(hallCfg, cellSize, floorRoot, param.cellPosition);
                    Utils.CheckOneSidedWalls(param.sector.Level.BaseSector, hallCfg, cellSize, wallRoot, param.cellPosition);
                    Utils.CheckTwoSidedWalls(param.sector.Level.BaseSector, 
                                       hallCfg, 
                                       cellSize, 
                                       wallRoot, 
                                       param.cellPosition, 
                                       comparer);
                }
            };

            Utils.IterateSector(l.BaseSector, new Action<Utils.SectorCellIteration>[] { hallStep }, ELevelLayer.Hall);
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
            
        }
    }

}