using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Level;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.LevelGen.Mesh
{
    using NeighborCheckComparer = System.Func<LevelGeneration.ECellCode, Vector2Int, bool>;

    public static class LevelGenerationMesh
    {
        public static IEnumerator Generate(Level l, BiomeConfiguration cfg, System.Action<GameObject> OnGenerationEnded)
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
                new LevelGenerationMeshStepGeometry(),
                new LevelGenerationMeshStepCleanColliders()
            };

            yield return RunSteps(steps, l, cfg, root);

            //////////////////
            /// Nav Mesh
            NavMeshSurface s = root.AddComponent<NavMeshSurface>();
            s.layerMask = LayerMask.GetMask(new string[]{ "Level" });
            s.BuildNavMesh();

            ComponentLevel levelComponent = root.AddComponent<ComponentLevel>();
            levelComponent.SetLevel(l, cfg);

            OnGenerationEnded?.Invoke(root);
        }

        public static IEnumerator RunSteps(ILevelGenerationMeshStep[] steps, 
                                           Level level, 
                                           BiomeConfiguration biome,
                                           GameObject levelRoot,
                                           System.Action OnEnded = null)
        {
            foreach (ILevelGenerationMeshStep step in steps)
                yield return step.Run(biome, level, levelRoot);
            OnEnded?.Invoke();
        }
    }

}