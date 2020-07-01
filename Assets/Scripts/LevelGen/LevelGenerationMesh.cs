using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Level
{

    /*
     *  Generates a given level inside unity with
     *  given a biome config 
     * 
     * */
    public static class LevelGenerationMesh
    {
        public static void Generate(Level l, LevelGenBiomeConfig cfg)
        {
            Vector3 cellSize = cfg.Floors[0].GetComponent<Renderer>().bounds.size;
            cellSize.y = 0f;

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

            //////////////////
            /// Meshes
            for (int x = 0; x < l.Size.x; x++)
            {
                for (int y = 0; y < l.Size.y; y++)
                {
                    Vector2Int p = new Vector2Int(x, y);

                    int c = l.GetCell(x, y);
                    if (c <= LevelGeneration.CODE_EMPTY)
                        continue;

                    // FLOOR
                    var floor = GameObject.Instantiate(cfg.Floors[Random.Range(0, cfg.Floors.Length)], floorRoot.transform);
                    //floor.isStatic = true;
                    floor.name = string.Format("F_{0}_{1}", x, y);
                    floor.transform.localPosition = new Vector3(x * cellSize.x, 0f, y * cellSize.z);
                    floor.layer = LayerMask.NameToLayer("Level");


                    // WALLS
                    CheckWalls(l, cfg, cellSize, wallRoot, x, y);

                    // DOORS
                    /*
                     * Algoritmo bosta pra colocar as portas.
                     * Com bitmask dá pra otimizar isso aqui.
                     * */
                    bool tl = l.GetCell(x - 1, y + 1) > LevelGeneration.CODE_EMPTY;
                    bool t = l.GetCell(x, y + 1) > LevelGeneration.CODE_EMPTY;
                    bool tr = l.GetCell(x + 1, y + 1) > LevelGeneration.CODE_EMPTY;
                    bool le = l.GetCell(x - 1, y) > LevelGeneration.CODE_EMPTY;
                    bool mid = l.GetCell(x, y) > LevelGeneration.CODE_EMPTY;
                    bool ri = l.GetCell(x + 1, y) > LevelGeneration.CODE_EMPTY;
                    bool bl = l.GetCell(x - 1, y - 1) > LevelGeneration.CODE_EMPTY;
                    bool b = l.GetCell(x, y - 1) > LevelGeneration.CODE_EMPTY;
                    bool br = l.GetCell(x + 1, y - 1) > LevelGeneration.CODE_EMPTY;

                    // top
                    if ( (tl && t && tr && !le && mid && !ri) ||
                         (tl && t && !le && mid && !ri ) ||
                         (t && tr && !le & mid && !ri) )
                    {
                        PutDoor(cfg, cellSize, root, x, y, 0);
                    }

                    // right 
                    if ( (tr && ri && br && !t && mid && !b) )
                    {
                        PutDoor(cfg, cellSize, root, x, y, 1);
                    }

                    // bot 
                    if ( (bl && b && br && !le && mid && !ri) )
                    {
                        PutDoor(cfg, cellSize, root, x, y, 2);
                    }

                    // left 
                    if ( (tl && le && bl && !t && mid && !b) )
                    {
                        PutDoor(cfg, cellSize, root, x, y, 3);
                    }

                }
            }

            //////////////////
            /// Nav Mesh
            NavMeshSurface s = root.AddComponent<NavMeshSurface>();
            s.layerMask = LayerMask.GetMask(new string[]{ "Level" });
            s.BuildNavMesh();
        }

        /*
         *  Isso aqui poderia ser trocado por um prefab de porta já pronto.
         *  Esse código espera meshes com o pivô no canto da parede/porta.
         * */
        private static GameObject PutDoor(LevelGenBiomeConfig cfg,
                                    Vector3 cellSize,
                                    GameObject root,
                                    int x, int y,
                                    int pos)
        {

            // DOOR WALL
            GameObject wallPrefab = cfg.DoorWalls[Random.Range(0, cfg.DoorWalls.Length)];

            GameObject wall = PutWall(wallPrefab, cfg, cellSize, root, x, y, pos, "DW");

            Vector2Int offset = Vector2Int.zero;
            switch (pos)
            {
                case 0: // top 
                    offset = new Vector2Int(0, 1);
                    break;
                case 1: // right
                    offset = new Vector2Int(1, 0);
                    break;
                case 2: // bot
                    offset = new Vector2Int(0, -1);
                    break;
                case 3: // left
                    offset = new Vector2Int(-1, 0);
                    break;
            }

            PutWall(wallPrefab, cfg, cellSize, root, x+offset.x, y+offset.y, (pos + 2) % 4, "DW");

            // DOOR FRAME
            GameObject wallFramePrefab = cfg.DoorFrame[Random.Range(0, cfg.DoorFrame.Length)];

            GameObject wallFrame = PutWall(wallFramePrefab, cfg, cellSize, wall, x, y, pos, "DF");
            wallFrame.transform.localPosition = Vector3.left * cellSize.x * 0.5f;
            wallFrame.transform.localRotation = Quaternion.identity;

            // DOOR
            GameObject doorPrefab = cfg.Door[Random.Range(0, cfg.Door.Length)];
            GameObject door = PutWall(doorPrefab, cfg, cellSize, wallFrame, x, y, pos, "D");
            door.transform.localPosition = Vector3.right * door.GetComponent<Renderer>().bounds.extents.x * 0.95f; // HACK
            door.transform.localRotation = Quaternion.identity;
            door.layer = LayerMask.NameToLayer("Entities");

            BoxCollider col = door.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size += Vector3.forward * 1.25f;

            Animator anim = door.AddComponent<Animator>();
            anim.runtimeAnimatorController = cfg.DoorAnimator;

            InteractableNeedsItem inter = door.AddComponent<InteractableNeedsItem>();
            inter.InteractionType = EInteractType.None;
            inter.EventHasItem.AddListener(delegate { anim.SetTrigger("Open"); });

            return wall;
        }

        private static void CheckWalls(Level l, 
                                       LevelGenBiomeConfig cfg, 
                                       Vector3 cellSize, 
                                       GameObject root, 
                                       int x, int y)
        {
            int down = l.GetCell(x, y - 1);
            if (down <= LevelGeneration.CODE_EMPTY) // nothing beyond the door 
            {
                PutWall(cfg.Walls[Random.Range(0, cfg.Walls.Length)], cfg, cellSize, root, x, y, 2);
            }

            int top = l.GetCell(x, y + 1);
            if (top <= LevelGeneration.CODE_EMPTY) // nothing beyond the door 
            {
                PutWall(cfg.Walls[Random.Range(0, cfg.Walls.Length)], cfg, cellSize, root, x, y, 0);
            }

            int left = l.GetCell(x - 1, y);
            if (left <= LevelGeneration.CODE_EMPTY)
            {
                PutWall(cfg.Walls[Random.Range(0, cfg.Walls.Length)], cfg, cellSize, root, x, y, 3);
            }

            int right = l.GetCell(x + 1, y);
            if (right <= LevelGeneration.CODE_EMPTY)
            {
                PutWall(cfg.Walls[Random.Range(0, cfg.Walls.Length)], cfg, cellSize, root, x, y, 1);
            }
        }

        private static GameObject PutWall(GameObject prefab,
                                  LevelGenBiomeConfig cfg,
                                  Vector3 cellSize,
                                  GameObject root,
                                  int x, int y,
                                  int pos,
                                  string namePreffix = "W") // 0 = top, 1 = right, 2 = down, 3 = left
        {
            Vector3 position = Vector3.zero;
            float angle = 0f;
            string suffix = "";

            switch (pos)
            {
                case 0: // top 
                    position = new Vector3((x - 1) * cellSize.x, 0f, (y + 1) * cellSize.z);
                    angle = 180f;
                    suffix = "top";
                    break;
                case 1: // right
                    position = new Vector3(x * cellSize.x, 0f, (y + 1) * cellSize.z);
                    angle = -90f;
                    suffix = "right";
                    break;
                case 2: // down
                    position = new Vector3(x * cellSize.x, 0f, y * cellSize.z);
                    angle = 0f;
                    suffix = "down";
                    break;
                case 3: // left
                    position = new Vector3((x - 1) * cellSize.x, 0f, y * cellSize.z);
                    angle = 90f;
                    suffix = "left";
                    break;
            }

            var obj = GameObject.Instantiate(prefab, root.transform);
            obj.name = string.Format("{0}_{1}_{2}_{3}", namePreffix, x, y, suffix);
            obj.transform.localPosition = position;
            obj.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            obj.layer = LayerMask.NameToLayer("Level");

            var renderers = obj.GetComponentsInChildren<Renderer>();
            System.Array.ForEach(renderers, r => r.material = cfg.EnvironmentMaterial);

            obj.AddComponent<BoxCollider>();
            obj.AddComponent<EnvironmentDissolveEffect>();

            return obj;
        }

    }



}