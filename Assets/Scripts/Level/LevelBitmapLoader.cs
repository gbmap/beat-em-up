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

            ECellCode lastCell = ECellCode.Error;

            for (int x = 0; x < t.width; x++)
            {
                for (int y = 0; y < t.height; y++)
                {
                    Color clr = t.GetPixel(x, y);
                    ECellCode cell = LevelGenVisualizer.ColorToCode(clr);
                    if (cell != ECellCode.Empty)
                        l.SetCell(x, y, cell);

                    int ncell = (int)cell;
                    bool isRoom = (ncell >= 2 && ncell <= 3)
                               || (ncell >= 7 && ncell <= 13);

                    if (isRoom && cell != lastCell && l.GetSectorAt(new Vector2Int(x, y)) == l.BaseSector)
                    {
                        int height = 0;
                        int y2 = y;
                        while (LevelGenVisualizer.ColorToCode(t.GetPixel(x, y2)) == cell)
                        {
                            height++;
                            y2++;
                        }

                        int width = 0;
                        int x2 = x;
                        while (LevelGenVisualizer.ColorToCode(t.GetPixel(x2, y)) == cell)
                        {
                            width++;
                            x2++;
                        }

                        new Sector(
                            l, 
                            new Vector2Int(x, y), 
                            new Vector2Int(width, height), 
                            cell 
                        );
                    }

                    lastCell = cell;
                }
            }
            return l;
        }
    }
}