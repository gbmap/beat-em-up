using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.LevelGen
{
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


            foreach (Sector sec in l.BaseSector.Children)
            {
                GenerateRoom(sec, cfg.GetRoomConfig(sec.Code));
            }

            /*

            //////////////////
            /// Meshes
            for (int x = 0; x < l.Size.x; x++)
            {
                for (int y = 0; y < l.Size.y; y++)
                {
                    Vector2Int p = new Vector2Int(x, y);

                    LevelGeneration.ECellCode c = l.GetCell(p);
                    if (c <= LevelGeneration.ECellCode.Empty)
                        continue;

                    AddFloor(cfg, cellSize, floorRoot, p);
                    CheckProp(cfg, cellSize, propRoot, p, c);
                    CheckWalls(l, cfg, cellSize, wallRoot, p);
                    CheckDoors(l, cfg, cellSize, root, p);
                }
            }

            */

            //////////////////
            /// Nav Mesh
            NavMeshSurface s = root.AddComponent<NavMeshSurface>();
            s.layerMask = LayerMask.GetMask(new string[]{ "Level" });
            s.BuildNavMesh();
        }

        private static GameObject AddFloor(LevelGenRoomConfig cfg,
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
            return floor;
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

        private static EDirectionBitmask CheckOneSidedWalls(Level l, 
                                       LevelGenRoomConfig cfg, 
                                       Vector3 cellSize, 
                                       GameObject root, 
                                       Vector2Int p)
        {
            EDirectionBitmask directions = CheckNeighbors(l.BaseSector, p, (LevelGeneration.ECellCode c, Vector2Int pos) => c <= LevelGeneration.ECellCode.Empty);
            PutWall(cfg.Walls[UnityEngine.Random.Range(0, cfg.Walls.Length)], cfg, cellSize, root, p, directions);
            return directions;
        }

        private static EDirectionBitmask CheckTwoSidedWalls(Sector l,
                                                            LevelGenRoomConfig cfg,
                                                            Vector3 cellSize,
                                                            GameObject root,
                                                            Vector2Int localPos,
                                                            Vector2Int globalPos)
        {
            EDirectionBitmask directions = CheckNeighbors(l, localPos, (LevelGeneration.ECellCode c, Vector2Int pos) => !l.IsIn(pos));
            PutWall(cfg.Walls[1], cfg, cellSize, root, globalPos, directions, "WD");
            return directions;

        }

        private static EDirectionBitmask CheckNeighbors(Sector l,
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

//            LevelGeneration.ECellCode down = l.GetCell(p.x, p.y - 1);
//            if (comparer(down, p))
//                DirectionHelper.Set(ref directions, EDirectionBitmask.Down);
//            //if (down <= LevelGeneration.ECellCode.Empty) // nothing beyond the door 
//
//            LevelGeneration.ECellCode top = l.GetCell(p.x, p.y + 1);
//            if (comparer(top, p))
//                DirectionHelper.Set(ref directions, EDirectionBitmask.Up);
//            //if (top <= LevelGeneration.ECellCode.Empty) // nothing beyond the door 
//
//            LevelGeneration.ECellCode left = l.GetCell(p.x - 1, p.y);
//            //if (left <= LevelGeneration.ECellCode.Empty)
//            if (comparer(left, p))
//                DirectionHelper.Set(ref directions, EDirectionBitmask.Left);
//
//            LevelGeneration.ECellCode right = l.GetCell(p.x + 1, p.y);
//            //if (right <= LevelGeneration.ECellCode.Empty)
//            if (comparer(right, p))
//                DirectionHelper.Set(ref directions, EDirectionBitmask.Right);
//
            //PutWall(cfg.Walls[UnityEngine.Random.Range(0, cfg.Walls.Length)], cfg, cellSize, root, p, directions);
            return directions;
        }


        //private static void CheckDoors(Level l,
        //                               LevelGenRoomConfig cfg, 
        //                               Vector3 cellSize, 
        //                               GameObject root, 
        //                               Vector2Int p)
        //{
        //    int x = p.x;
        //    int y = p.y;

        //    /*
        //    * Algoritmo bosta pra colocar as portas.
        //    * Com bitmask dá pra otimizar isso aqui.
        //    * */
        //    bool tl = l.GetCell(x - 1, y + 1) > LevelGeneration.CODE_EMPTY;
        //    bool t = l.GetCell(x, y + 1) > LevelGeneration.CODE_EMPTY;
        //    bool tr = l.GetCell(x + 1, y + 1) > LevelGeneration.CODE_EMPTY;
        //    bool le = l.GetCell(x - 1, y) > LevelGeneration.CODE_EMPTY;
        //    bool mid = l.GetCell(x, y) > LevelGeneration.CODE_EMPTY;
        //    bool ri = l.GetCell(x + 1, y) > LevelGeneration.CODE_EMPTY;
        //    bool bl = l.GetCell(x - 1, y - 1) > LevelGeneration.CODE_EMPTY;
        //    bool b = l.GetCell(x, y - 1) > LevelGeneration.CODE_EMPTY;
        //    bool br = l.GetCell(x + 1, y - 1) > LevelGeneration.CODE_EMPTY;

        //    // top
        //    if ((tl && t && tr && !le && mid && !ri) ||
        //         (tl && t && !le && mid && !ri) ||
        //         (t && tr && !le & mid && !ri))
        //    {
        //        PutDoor(cfg, cellSize, root, p, 0);
        //    }

        //    // right 
        //    if ((tr && ri && br && !t && mid && !b))
        //    {
        //        PutDoor(cfg, cellSize, root, p, 1);
        //    }

        //    // bot 
        //    if ((bl && b && br && !le && mid && !ri))
        //    {
        //        PutDoor(cfg, cellSize, root, p, 2);
        //    }

        //    // left 
        //    if ((tl && le && bl && !t && mid && !b))
        //    {
        //        PutDoor(cfg, cellSize, root, p, 3);
        //    }
        //}

        

        /*
         *  Isso aqui poderia ser trocado por um prefab de porta já pronto.
         *  Esse código espera meshes com o pivô no canto da parede/porta.
         * */
        //private static GameObject PutDoor(LevelGenRoomConfig cfg,
        //                            Vector3 cellSize,
        //                            GameObject root,
        //                            Vector2Int p, 
        //                            int pos)
        //{

        //    // DOOR WALL
        //    GameObject wallPrefab = cfg.DoorWalls[UnityEngine.Random.Range(0, cfg.DoorWalls.Length)];

        //    GameObject wall = PutWall(wallPrefab, cfg, cellSize, root, p, pos, "DW");

        //    Vector2Int offset = Vector2Int.zero;
        //    switch (pos)
        //    {
        //        case 0: // top 
        //            offset = new Vector2Int(0, 1);
        //            break;
        //        case 1: // right
        //            offset = new Vector2Int(1, 0);
        //            break;
        //        case 2: // bot
        //            offset = new Vector2Int(0, -1);
        //            break;
        //        case 3: // left
        //            offset = new Vector2Int(-1, 0);
        //            break;
        //    }

        //    PutWall(wallPrefab, cfg, cellSize, root, p+offset, (pos + 2) % 4, "DW");

        //    // DOOR FRAME
        //    GameObject wallFramePrefab = cfg.DoorFrame[UnityEngine.Random.Range(0, cfg.DoorFrame.Length)];

        //    GameObject wallFrame = PutWall(wallFramePrefab, cfg, cellSize, wall, p, pos, "DF");
        //    wallFrame.transform.localPosition = Vector3.left * cellSize.x * 0.5f;
        //    wallFrame.transform.localRotation = Quaternion.identity;

        //    // DOOR
        //    GameObject doorPrefab = cfg.Door[UnityEngine.Random.Range(0, cfg.Door.Length)];
        //    GameObject door = PutWall(doorPrefab, cfg, cellSize, wallFrame, p, pos, "D");
        //    door.transform.localPosition = Vector3.right * door.GetComponent<Renderer>().bounds.extents.x * 0.95f; // HACK
        //    door.transform.localRotation = Quaternion.identity;
        //    door.layer = LayerMask.NameToLayer("Entities");

        //    BoxCollider col = door.AddComponent<BoxCollider>();
        //    col.isTrigger = true;
        //    col.size += Vector3.forward * 1.25f;

        //    Animator anim = door.AddComponent<Animator>();
        //    anim.runtimeAnimatorController = cfg.DoorAnimator;

        //    InteractableNeedsItem inter = door.AddComponent<InteractableNeedsItem>();
        //    inter.InteractionType = EInteractType.None;
        //    inter.EventHasItem.AddListener(delegate { anim.SetTrigger("Open"); });

        //    return wall;
        //}

        private static GameObject[] PutWall(GameObject prefab,
                                  LevelGenRoomConfig cfg,
                                  Vector3 cellSize,
                                  GameObject root,
                                  Vector2Int p,
                                  EDirectionBitmask direction,
                                  string namePreffix = "W") // 0 = top, 1 = right, 2 = down, 3 = left
        {
            Vector3 position = Vector3.zero;
            float angle = 0f;
            string suffix = "";
            
            System.Collections.Generic.List<GameObject> wallInstances = new System.Collections.Generic.List<GameObject>();

            EDirectionBitmask[] possibleDirections = DirectionHelper.GetValues();
            foreach (var dir in possibleDirections)
            {
                if (!DirectionHelper.IsSet(direction, dir))
                    continue;

                position = LevelToWorldPos(p+DirectionHelper.ToPrefabOffset(dir), cellSize);
                angle = DirectionHelper.ToAngle(dir);
                suffix = DirectionHelper.GetName(dir);

                var obj = GameObject.Instantiate(prefab, root.transform);
                obj.name = string.Format("{0}_{1}_{2}_{3}", namePreffix, p.x, p.y, suffix);
                obj.transform.localPosition = position;
                obj.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                obj.layer = LayerMask.NameToLayer("Level");

                var renderers = obj.GetComponentsInChildren<Renderer>();
                System.Array.ForEach(renderers, r => r.material = cfg.EnvironmentMaterial);

                obj.AddComponent<BoxCollider>();
                obj.AddComponent<EnvironmentDissolveEffect>();

                wallInstances.Add(obj);
            }

            return wallInstances.ToArray();

            //if (DirectionHelper.IsSet<EDirectionBitmask>(direction, EDirectionBitmask.Up))
            //{
                //position = LevelToWorldPos(p+Vector2Int.up+Vector2Int.left, cellSize);
                //angle = 180f;
                //suffix = "top";
            //}
            //else if (DirectionHelper.IsSet<EDirectionBitmask>(direction, EDirectionBitmask.Right))
            //{
                //position = LevelToWorldPos(p + Vector2Int.up, cellSize);
                //angle = -90f;
                //suffix = "right";
            //}
            //else if (DirectionHelper.IsSet<EDirectionBitmask>(direction, EDirectionBitmask.Down))
            //{
                //position = LevelToWorldPos(p, cellSize);
                //angle = 0f;
                //suffix = "down";
            //}
            //else if (DirectionHelper.IsSet<EDirectionBitmask>(direction, EDirectionBitmask.Left))
            //{
                //position = LevelToWorldPos(p + Vector2Int.left, cellSize);
                //angle = 90f;
                //suffix = "left";
            //}

            //var obj = GameObject.Instantiate(prefab, root.transform);
            //obj.name = string.Format("{0}_{1}_{2}_{3}", namePreffix, p.x, p.y, suffix);
            //obj.transform.localPosition = position;
            //obj.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            //obj.layer = LayerMask.NameToLayer("Level");

            //var renderers = obj.GetComponentsInChildren<Renderer>();
            //System.Array.ForEach(renderers, r => r.material = cfg.EnvironmentMaterial);

            //obj.AddComponent<BoxCollider>();
            //obj.AddComponent<EnvironmentDissolveEffect>();

            //return obj;
        }

        public static Vector3 LevelToWorldPos(Vector2Int pos, Vector3 cellSize)
        {
            return new Vector3(pos.x * cellSize.x, 0f, pos.y * cellSize.z);
        }


        public static void GenerateRoom(Sector sec, LevelGenRoomConfig cfg)
        {
            string log = "GENERATING ROOM:\n";
            log += "Position: " + sec.Pos + "\n";
            log += "Size: " + sec.Size + "\n";
            Debug.Log(log);

            Vector3 cellSize = cfg.Floors[0].GetComponent<Renderer>().bounds.size;
            cellSize.y = 0f;

            Vector2Int pos = sec.Pos; //sec.GetAbsolutePosition(sec.Pos);
            Vector2Int sz = sec.Size;

            GameObject roomObject = new GameObject("Room");

            for (int x = pos.x; x < pos.x + sz.x; x++)
            {
                for (int y = pos.y; y < pos.y + sz.y; y++)
                {
                    LevelGeneration.ECellCode cell = sec.GetCell(x, y);
                    Vector2Int p = new Vector2Int(x, y);

                    AddFloor(cfg, cellSize, roomObject, p);

                    // Borders
                    if (x == pos.x || x == pos.x+sz.x-1 ||
                        y == pos.y || y == pos.y+sz.y-1)
                    {
                        //AddFloor(cfg, cellSize, roomObject, p);
                        EDirectionBitmask addedWalls = CheckOneSidedWalls(sec.Level, cfg, cellSize, roomObject, p);
                        CheckTwoSidedWalls(sec, cfg, cellSize, roomObject, p-pos, p);

                        EDirectionBitmask mask = CheckNeighbors(sec, p-pos, (LevelGeneration.ECellCode c, Vector2Int lpos) => sec.IsIn(lpos));
                        string str = DirectionHelper.ToString(mask);
                        Debug.Log((p-pos).ToString() + " mask: " + str);
                    }
                }
            }
        }


    }
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


}