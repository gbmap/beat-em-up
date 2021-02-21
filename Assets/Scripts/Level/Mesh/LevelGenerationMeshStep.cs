using UnityEngine;
using Catacumba.Data.Level;
using System.Collections;

namespace Catacumba.LevelGen.Mesh
{
    public interface ILevelGenerationMeshStep
    {
        IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root);
    }

}