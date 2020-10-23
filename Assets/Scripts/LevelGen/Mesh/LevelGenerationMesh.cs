using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


namespace Catacumba.LevelGen.Mesh
{
    using NeighborCheckComparer = System.Func<LevelGeneration.ECellCode, Vector2Int, bool>;

    public static class LevelGenerationMesh
    {
        public static void Generate(Level l, LevelGenBiomeConfig cfg)
        {
            //////////////////
            /// Roots
            GameObject root = new GameObject("Level");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            root.isStatic = true;


            GameObject floorRoot = new GameObject("Floors");
            floorRoot.transform.parent = root.transform;
            floorRoot.transform.localPosition = Vector3.zero;
            floorRoot.transform.localRotation = Quaternion.identity;
            floorRoot.isStatic = true;

            GameObject wallRoot = new GameObject("Walls");
            wallRoot.transform.parent = root.transform;
            wallRoot.transform.localPosition = Vector3.zero;
            wallRoot.transform.localRotation = Quaternion.identity;
            wallRoot.isStatic = true;

            GameObject propRoot = new GameObject("Props");
            propRoot.transform.parent = root.transform;
            propRoot.transform.localPosition = Vector3.zero;
            propRoot.transform.localRotation = Quaternion.identity;
            propRoot.isStatic = true;


            ILevelGenerationMeshStep[] steps = new ILevelGenerationMeshStep[]
            {
                new LevelGenerationMeshStepRooms(),
                new LevelGenerationMeshStepHall(floorRoot, wallRoot),
                new LevelGenerationMeshStepGroupWalls(),
                new LevelGenerationMeshStepDoors()
            };

            foreach (ILevelGenerationMeshStep step in steps)
                step.Run(cfg, l, root);

            //////////////////
            /// Nav Mesh
            NavMeshSurface s = root.AddComponent<NavMeshSurface>();
            s.layerMask = LayerMask.GetMask(new string[]{ "Level" });
            s.BuildNavMesh();

            ComponentLevel levelComponent = root.AddComponent<ComponentLevel>();
            levelComponent.SetLevel(l, cfg);
        }

        private static GameObject[] CheckProp(LevelGenRoomConfig cfg,
                                              Vector3 cellSize,
                                              GameObject propRoot,
                                              Vector2Int pos, 
                                              LevelGeneration.ECellCode c)
        {
            if (c != LevelGeneration.ECellCode.Prop)
                return null;

            int nProps = UnityEngine.Random.Range(3, 6);
            GameObject[] propInstances = new GameObject[nProps];

            Vector3 hcsz = cellSize / 2;

            for (int i = 0; i < nProps; i++)
            {
                Vector3 randPos = UnityEngine.Random.insideUnitSphere;
                float x = (pos.x * cellSize.x) + (cellSize.x * 0.5f) + UnityEngine.Random.Range(-hcsz.x, hcsz.x);
                float z = (pos.y * cellSize.z) + (cellSize.z * 0.5f) + UnityEngine.Random.Range(-hcsz.z, hcsz.z);

                var prop = GameObject.Instantiate(cfg.Props[UnityEngine.Random.Range(0, cfg.Props.Length)], propRoot.transform);
                prop.transform.localPosition = new Vector3(x, 0f, z);
                prop.layer = LayerMask.NameToLayer("Entities");
            }
            return propInstances;
        }
    }

}