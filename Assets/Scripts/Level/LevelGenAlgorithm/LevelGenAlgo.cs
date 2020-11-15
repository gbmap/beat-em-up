using System.Collections;

namespace Catacumba.LevelGen 
{
    interface ILevelGenAlgo
    {
        IEnumerator Run(Level l, System.Action<Level> updateVis=null);
    }
}