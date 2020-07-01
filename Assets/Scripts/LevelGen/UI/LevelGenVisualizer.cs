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
                case LevelGeneration.CODE_BOSS_ROOM: return new Color(0.25f, 0f, 0f);
                case LevelGeneration.CODE_SPAWNER: return new Color(0.0f, 0.0f, .25f);
                case LevelGeneration.CODE_PLAYER_SPAWN: return new Color(0f, 0.25f, 0f);
                case LevelGeneration.CODE_PROP: return new Color(0.25f, 0.125f, 0.05f);
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
                    t.SetPixel(x, /*(l.Size.y-1) - */ y, CodeToColor(l.GetCell(x, y)));
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