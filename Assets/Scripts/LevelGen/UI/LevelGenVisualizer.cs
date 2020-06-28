using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Catacumba.Level
{
    public class LevelGenVisualizer : MonoBehaviour
    {
        public RawImage Image;

        public Color CodeToColor(int code)
        {
            switch (code)
            {
                case LevelGeneration.CODE_EMPTY: return Color.black;
                case LevelGeneration.CODE_HALL: return Color.gray;
                case LevelGeneration.CODE_ROOM: return Color.white;
            }

            return Color.black;
        }

        public Texture2D LevelToTexture(Level l)
        {
            Texture2D t = new Texture2D(l.Size.x, l.Size.y, TextureFormat.RGBA32, false);
            for (int y = 0; y < l.Size.y; y++)
            {
                for (int x = 0; x < l.Size.x; x++)
                {
                    t.SetPixel(x, (l.Size.y-1) - y, CodeToColor(l.Map[x, y]));
                }
            }
            t.filterMode = FilterMode.Point;
            t.Apply();
            return t;
        }

        public void UpdateTexture(Level l)
        {
            // TODO: FIX MEM LEAK
            Texture2D txtr = LevelToTexture(l);
            //Rect r = new Rect(Vector2.zero, l.Size);
            //Sprite s = Sprite.Create(txtr, r, Vector2.zero, txtr.width);
            Image.texture = txtr;
        }
    }
}