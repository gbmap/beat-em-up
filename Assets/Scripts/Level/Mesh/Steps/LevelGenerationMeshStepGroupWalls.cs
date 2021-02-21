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
        Removes duplicate walls created from room and hall generation step.
    */
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
}