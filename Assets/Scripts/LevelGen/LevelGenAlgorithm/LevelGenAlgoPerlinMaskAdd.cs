using System.Collections;
using UnityEngine;

namespace Catacumba.LevelGen
{

    class LevelGenAlgoPerlinMaskAdd : ILevelGenAlgo
    {
        private LevelGeneration.ECellCode targetCellCode;
        private LevelGeneration.ECellCode cellCode;
        private float perlinThreshold;
        private Vector2 perlinScale;
        private ELevelLayer layer;


        public LevelGenAlgoPerlinMaskAdd(LevelGeneration.ECellCode targetCellCode, 
                                         LevelGeneration.ECellCode cellCode,
                                         float perlinThreshold = .35f,
                                         float perlinScaleX = .15f,
                                         float perlinScaleY = .15f,
                                         ELevelLayer targetLayer = ELevelLayer.All) {
            this.targetCellCode  = targetCellCode;
            this.cellCode        = cellCode;
            this.perlinThreshold = perlinThreshold;
            this.perlinScale     = new Vector2(perlinScaleX, perlinScaleY);
            this.layer           = targetLayer;
        }

        public IEnumerator Run(Level level, System.Action<Level> updateVis=null)
        {
            Vector2 pc = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));

            for (int x=0; x < level.Size.x; x++)
            {
                for (int y = 0; y < level.Size.y; y++)
                {
                    if (level.BaseSector.GetCell(x, y) != this.targetCellCode)
                        continue;

                    float psx = this.perlinScale.x;
                    float psy = this.perlinScale.y;
                    
                    float px = x * psx;
                    float py = y * psy; 

                    float pv = Mathf.PerlinNoise(pc.x + px, pc.y + py);
                    if (pv > this.perlinThreshold)
                        level.BaseSector.SetCell(new Vector2Int(x, y), this.cellCode);

                    updateVis?.Invoke(level);
                    yield return null;
                }
            }
            yield break;
        }
    }
}