using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Catacumba.LevelGen.LevelGeneration;

namespace Catacumba.LevelGen
{
    public class LevelBitmapLoader 
    {
        public static Level Load(string file)
        {
            Texture2D t = Resources.Load<Texture2D>(file);
            Level l = new Level(new Vector2Int(t.width, t.height));

            for (int x = 0; x < t.width; x++)
            {
                for (int y = 0; y < t.height; y++)
                {
                    Color clr = t.GetPixel(x, y);
                    ECellCode cell = LevelGenVisualizer.ColorToCode(clr);
                    if (cell == ECellCode.Empty) continue;
                    l.SetCell(x, y, cell);
                }
            }

            return l;
        }
    }
}