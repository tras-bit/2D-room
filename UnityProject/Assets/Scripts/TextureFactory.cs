using UnityEngine;

namespace Subsistence
{
    public static class TextureFactory
    {
        static Texture2D lensDirtTexture;

        public static Texture2D LensDirt()
        {
            if (lensDirtTexture != null) return lensDirtTexture;
            const int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                name = "Runtime lens dirt",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            Color[] pixels = new Color[size*size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float cx = (x + .5f)/size - .5f;
                float cy = (y + .5f)/size - .5f;
                float vignette = Mathf.Clamp01(1f - new Vector2(cx, cy).magnitude*1.25f);
                float specks = Mathf.PerlinNoise(x*.09f, y*.11f)*Mathf.PerlinNoise(x*.22f+11f, y*.19f+7f);
                float dust = Mathf.Pow(Random01(x,y), 26f)*Random01(x*13, y*17);
                float streak = Mathf.Pow(Mathf.Abs(Mathf.Sin(x*.05f+y*.02f)),28f)*.04f;
                float value = Mathf.Clamp01(vignette*vignette*.12f + specks*.2f + dust*.9f + streak);
                pixels[y*size+x] = new Color(value,value,value,value);
            }
            tex.SetPixels(pixels);
            tex.Apply(false,true);
            lensDirtTexture = tex;
            return tex;
        }

        static float Random01(int x,int y)
        {
            uint seed=2166136261u;
            seed=(seed^(byte)(x*131))*16777619u;
            seed=(seed^(byte)(y*199))*16777619u;
            seed=(seed^(byte)((x*y+x+y)&255))*16777619u;
            return (seed&0xffffff)/(float)0xffffff;
        }
    }
}
