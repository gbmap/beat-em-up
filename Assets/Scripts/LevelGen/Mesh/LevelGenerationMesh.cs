using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


namespace Catacumba.LevelGen.Mesh
{
    using NeighborCheckComparer = System.Func<LevelGeneration.ECellCode, Vector2Int, bool>;


#region DIRECTION BITMASK

    [Flags]
    public enum EDirectionBitmask
    {
        None = 0,
        Up = 1 << 1,
        Right = 1 << 2,
        Down = 1 << 3,
        Left = 1 << 4
    }

    public static class BitmaskHelper
    {
        public static bool IsSet<T>(T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        public static void Set<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        public static void Unset<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }
    }

    public static class DirectionHelper
    {
        public static bool IsSet(EDirectionBitmask flags, EDirectionBitmask flag) 
        {
            return BitmaskHelper.IsSet<EDirectionBitmask>(flags, flag);
        }

        public static void Set(ref EDirectionBitmask flags, EDirectionBitmask flag) 
        {
            BitmaskHelper.Set<EDirectionBitmask>(ref flags, flag);
        }

        public static void Unset(ref EDirectionBitmask flags, EDirectionBitmask flag) 
        {
            BitmaskHelper.Unset<EDirectionBitmask>(ref flags, flag);
        }

        private static System.Collections.Generic.Dictionary<EDirectionBitmask, float> DictDirectionToAngle
            = new System.Collections.Generic.Dictionary<EDirectionBitmask, float>
        {
            { EDirectionBitmask.Up, 180f },
            { EDirectionBitmask.Right, -90f },
            { EDirectionBitmask.Down, 0f },
            { EDirectionBitmask.Left, 90f }
        };

        private static System.Collections.Generic.Dictionary<EDirectionBitmask, Vector2Int> DictDirectionToPrefabOffset 
            = new System.Collections.Generic.Dictionary<EDirectionBitmask, Vector2Int>
        {
            { EDirectionBitmask.Up, Vector2Int.up+Vector2Int.left },
            { EDirectionBitmask.Right, Vector2Int.up },
            { EDirectionBitmask.Down, Vector2Int.zero },
            { EDirectionBitmask.Left, Vector2Int.left }
        };

        public static float ToAngle(EDirectionBitmask dir)
        {
            return DictDirectionToAngle[dir];
        }

        public static Vector2Int ToPrefabOffset(EDirectionBitmask dir)
        {
            return DictDirectionToPrefabOffset[dir];
        }

        public static EDirectionBitmask[] GetValues()
        {
            return Enum.GetValues(typeof(EDirectionBitmask)).Cast<EDirectionBitmask>().ToArray();
        }

        public static string GetName(EDirectionBitmask direction)
        {
            return direction.ToString();
        }

        public static string ToString(EDirectionBitmask mask)
        {
            string str = "";
            foreach (var value in GetValues())
            {
                int v = IsSet(mask, value) ? 1 : 0;
                str += v;
            }
            return str;
        }
    }

#endregion 

    public static class LevelGenerationMesh
    {
        public static void Generate(Level l, LevelGenBiomeConfig cfg)
        {
            // Vector3 cellSize = cfg.Floors[0].GetComponent<Renderer>().bounds.size;
            // cellSize.y = 0f;

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


            // foreach (Sector sec in l.BaseSector.Children)
            //  GenerateRoom(sec, cfg.GetRoomConfig(sec.Code), root);

            //GenerateHall(l, cfg, floorRoot, wallRoot);

            ILevelGenerationMeshStep[] steps = new ILevelGenerationMeshStep[]
            {
                new LevelGenerationMeshStepRooms(),
                new LevelGenerationMeshStepHall(floorRoot, wallRoot),
                new LevelGenerationMeshStepGroupWalls()
            };

            foreach (ILevelGenerationMeshStep step in steps)
                step.Run(cfg, l, root);

            //////////////////
            /// Nav Mesh
            NavMeshSurface s = root.AddComponent<NavMeshSurface>();
            s.layerMask = LayerMask.GetMask(new string[]{ "Level" });
            s.BuildNavMesh();
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

        private static bool GetShouldPutTwoSidedWall(Sector l, 
                                                     Vector3 cellSize, 
                                                     LevelGeneration.ECellCode cellCode,
                                                     Vector2Int pos)
        {
            Vector3 worldPosition = new Vector3(pos.x*cellSize.x, 0f, pos.y*cellSize.z);
            worldPosition += cellSize * 0.5f;

            return !l.IsIn(pos);
        }

    }

}