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
    /*
        Removes colliders from every door in the level.
        These colliders are used simply for the sake of not double-spawning doors
        during level creation.
    */
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
}