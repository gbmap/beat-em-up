using System.Collections;
using UnityEngine;

namespace Catacumba.LevelGen
{
    class LevelGenAlgoPerlin : ILevelGenAlgo
    {
        public IEnumerator Run(Level level, System.Action<Level> updateVis=null)
        {
            Vector2Int center = level.Size/2;
            Vector2 pc = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));

            for (int x=0; x < level.Size.x; x++)
            {
                for (int y = 0; y < level.Size.y; y++)
                {
                    if (CheckBounds(x, y, center))
                        continue;

                    float psx = .15f;
                    float psy = .15f;
                    
                    float px = x * psx;
                    float py = y * psy; 

                    float pv = Mathf.PerlinNoise(pc.x + px, pc.y + py);
                    if (pv > 0.35f)
                        level.BaseSector.SetCell(new Vector2Int(x, y), LevelGeneration.ECellCode.Room);

                    updateVis?.Invoke(level);
                    yield return null;
                }
            }
        }

        private bool CheckBounds(int x, int y, Vector2Int center)
        {
            Vector2Int p = new Vector2Int(x, y);
            Vector2Int p2c = p - center;
            float a = Vector2.Angle(Vector2Int.up, p2c);

            return (p2c.magnitude > center.magnitude/2f + Mathf.Sin(a*0.15f) * 2f );
        }        
    }
}