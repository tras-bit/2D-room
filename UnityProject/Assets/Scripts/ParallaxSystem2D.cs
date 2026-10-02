using UnityEngine;

namespace Subsistence
{
    public static class ParallaxSystem2D
    {
        public static void Build(Transform parent, Camera camera)
        {
            if (camera == null || parent == null) return;
            Transform layersRoot = parent.Find("Depth · Parallax Layers");
            if (layersRoot != null) Object.Destroy(layersRoot.gameObject);
            layersRoot = new GameObject("Depth · Parallax Layers").transform;
            layersRoot.SetParent(parent, false);

            AddLayer(layersRoot,camera,"Far haze","Parallax/level0_far_haze",22f,5f,-38,.06f,.02f,new Color(.48f,.48f,.40f,.92f));
            AddLayer(layersRoot,camera,"Midground wallpaper","Parallax/level0_midground_walls",28f,6.1f,-24,.20f,.05f,new Color(.74f,.72f,.58f,1f));
            AddLayer(layersRoot,camera,"Soft foreground vignette","Parallax/level0_foreground_frame",16f,7.2f,24,.55f,.12f,Color.white);
        }

        static void AddLayer(Transform root, Camera camera, string objectName, string resourcePath, float width, float height, int sortOrder, float parallaxX, float parallaxY, Color tint)
        {
            Texture2D source = Resources.Load<Texture2D>(resourcePath);
            if (source == null) return;
            GameObject layerObject = new GameObject("Parallax layer · " + objectName);
            layerObject.transform.SetParent(root,false);
            SpriteRenderer renderer = layerObject.AddComponent<SpriteRenderer>();
            Texture2D copy = CreateReadableCopy(source,tint);
            copy.filterMode = FilterMode.Bilinear;
            copy.wrapMode = source.name.Contains("foreground") ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
            Sprite sprite = Sprite.Create(copy,new Rect(0,0,copy.width,copy.height),new Vector2(.5f,.5f),100f,0,SpriteMeshType.FullRect,Vector4.zero,false,new SecondarySpriteTexture[0]);
            sprite.name = "Parallax_"+objectName;
            renderer.sprite = sprite;
            renderer.sortingOrder = sortOrder;
            float heightScale = height/sprite.bounds.size.y;
            float widthScale = Mathf.Max(width/sprite.bounds.size.x,3.2f);
            layerObject.transform.localScale = new Vector3(widthScale,heightScale,1f);
            layerObject.transform.position = new Vector3(0f,height*.42f,0f);
            ParallaxLayer2D parallax = layerObject.AddComponent<ParallaxLayer2D>();
            parallax.Initialize(camera.transform,parallaxX,parallaxY);
            RendererLibrary2D.Configure(renderer);
        }

        static Texture2D CreateReadableCopy(Texture2D source, Color tint)
        {
            RenderTexture rt = RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.Default,RenderTextureReadWrite.Default);
            Graphics.Blit(source,rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D copy = new Texture2D(source.width,source.height,TextureFormat.RGBA32,false,false)
            {
                name = source.name+"_parallax",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            copy.ReadPixels(new Rect(0,0,source.width,source.height),0,0);
            if (tint.a < 1f || tint.r < 1f || tint.g < 1f || tint.b < 1f)
            {
                Color[] pixels = copy.GetPixels();
                for(int i=0;i<pixels.Length;i++) pixels[i] *= tint;
                copy.SetPixels(pixels);
            }
            copy.Apply(false,true);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return copy;
        }
    }
}
