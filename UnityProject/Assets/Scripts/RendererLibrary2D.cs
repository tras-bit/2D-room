using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    public static class RendererLibrary2D
    {
        static readonly Dictionary<Texture2D,Texture2D> NormalCache = new Dictionary<Texture2D,Texture2D>();
        static Material spriteLitMaterial;

        public static void PrepareRenderers(Transform root)
        {
            if (root == null) return;
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i=0;i<renderers.Length;i++) Configure(renderers[i]);
        }

        public static void Configure(SpriteRenderer renderer)
        {
            if (renderer == null) return;
            if (spriteLitMaterial == null)
            {
                Shader lit = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
                if (lit != null) spriteLitMaterial = new Material(lit) { name = "Runtime Sprite-Lit (instanced)" };
            }
            if (spriteLitMaterial == null) return;
            renderer.sharedMaterial = spriteLitMaterial;
            if (renderer.sprite == null || renderer.sprite.texture == null) return;

            Texture2D spriteTexture = renderer.sprite.texture;
            if (!NormalCache.TryGetValue(spriteTexture, out Texture2D normalMap))
            {
                normalMap = BuildNormalMap(spriteTexture);
                NormalCache[spriteTexture] = normalMap;
            }

            Material instance = renderer.material;
            if (instance == null) return;
            instance.SetTexture("_NormalMap", normalMap);
            instance.EnableKeyword("_NORMALMAP");
            instance.SetFloat("_AlphaClip", 0f);
        }

        static Texture2D BuildNormalMap(Texture2D source)
        {
            int width = source.width, height = source.height;
            Color[] src = source.GetPixels();
            Color[] dst = new Color[src.Length];
            const float bump = 2.2f;
            for (int y=0;y<height;y++)
            for (int x=0;x<width;x++)
            {
                int xm = x>0?x-1:width-1;
                int xp = x<width-1?x+1:0;
                int ym = y>0?y-1:height-1;
                int yp = y<height-1?y+1:0;
                float l = src[y*width+xm].grayscale*src[y*width+xm].a;
                float r = src[y*width+xp].grayscale*src[y*width+xp].a;
                float d = src[ym*width+x].grayscale*src[ym*width+x].a;
                float u = src[yp*width+x].grayscale*src[yp*width+x].a;
                float dx = (l-r)*bump;
                float dy = (d-u)*bump;
                Vector3 n = Vector3.Normalize(new Vector3(-dx,-dy,1f));
                dst[y*width+x] = new Color(n.x*.5f+.5f, n.y*.5f+.5f, n.z, src[y*width+x].a);
            }
            Texture2D normal = new Texture2D(width,height,TextureFormat.RGBA32,false,true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = source.name + "_generated_normal"
            };
            normal.SetPixels(dst);
            normal.Apply(false,true);
            return normal;
        }
    }
}
