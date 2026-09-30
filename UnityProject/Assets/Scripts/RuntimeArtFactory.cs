using UnityEngine;

namespace Subsistence
{
    /// <summary>Reusable 3D meshes/materials for the 2.5D Backrooms rooms, crates and character rigs.</summary>
    public static class RuntimeArtFactory
    {
        static Material wall, carpet, ceiling, stainedWall, tile, concrete, metal, rubber, wood, crateT1, crateT2, crateT3, fluorescent, trim;
        static Texture2D wallpaperTexture, carpetTexture;
        static bool ready;

        public static void Initialize()
        {
            if(ready)return;ready=true;
            wallpaperTexture=Resources.Load<Texture2D>("Art/wallpaper_backrooms");
            carpetTexture=Resources.Load<Texture2D>("Art/carpet_backrooms");
            wall=MakeMaterial(new Color(.79f,.68f,.32f),wallpaperTexture,.05f,0);
            stainedWall=MakeMaterial(new Color(.53f,.49f,.33f),wallpaperTexture,.12f,0);
            carpet=MakeMaterial(new Color(.82f,.72f,.27f),carpetTexture,.02f,0);
            ceiling=MakeMaterial(new Color(.72f,.68f,.43f),null,.1f,0);
            tile=MakeMaterial(new Color(.35f,.40f,.36f),null,.25f,.12f);
            concrete=MakeMaterial(new Color(.22f,.25f,.22f),null,.3f,.05f);
            metal=MakeMaterial(new Color(.25f,.30f,.27f),null,.68f,.34f);
            rubber=MakeMaterial(new Color(.13f,.15f,.14f),null,.18f,0);
            wood=MakeMaterial(new Color(.33f,.25f,.15f),null,.24f,0);
            crateT1=MakeMaterial(new Color(.48f,.42f,.27f),null,.25f,.02f);
            crateT2=MakeMaterial(new Color(.34f,.43f,.34f),null,.47f,.16f);
            crateT3=MakeMaterial(new Color(.40f,.32f,.24f),null,.54f,.28f);
            fluorescent=MakeMaterial(new Color(1f,.91f,.55f),null,.1f,0);
            trim=MakeMaterial(new Color(.79f,.72f,.48f),null,.34f,.05f);
            if(wallpaperTexture!=null){wallpaperTexture.wrapMode=TextureWrapMode.Repeat;wall.mainTextureScale=new Vector2(4,2);stainedWall.mainTextureScale=new Vector2(4,2);}
            if(carpetTexture!=null){carpetTexture.wrapMode=TextureWrapMode.Repeat;carpet.mainTextureScale=new Vector2(16,10);}
        }

        public static Material Wall=>wall;
        public static Material Carpet=>carpet;
        public static Material Ceiling=>ceiling;
        public static Material StainedWall=>stainedWall;
        public static Material Tile=>tile;
        public static Material Concrete=>concrete;
        public static Material Metal=>metal;
        public static Material Rubber=>rubber;
        public static Material Wood=>wood;
        public static Material CrateTier1=>crateT1;
        public static Material CrateTier2=>crateT2;
        public static Material CrateTier3=>crateT3;
        public static Material Fluorescent=>fluorescent;
        public static Material Trim=>trim;

        static Material MakeMaterial(Color color,Texture texture,float smoothness,float metallic)
        {
            Shader shader=Shader.Find("Standard");if(shader==null)shader=Shader.Find("Diffuse");
            var m=new Material(shader){color=color};if(texture!=null)m.mainTexture=texture;
            if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",smoothness);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic);
            return m;
        }

        public static GameObject Cube(Transform parent,string name,Vector3 position,Vector3 size,Material material,bool keepCollider=false)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
            if(!keepCollider){var c=go.GetComponent<Collider>();if(c!=null)Object.Destroy(c);}
            return go;
        }

        public static GameObject Sphere(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            go.GetComponent<MeshRenderer>().sharedMaterial=material;var c=go.GetComponent<Collider>();if(c!=null)Object.Destroy(c);return go;
        }

        public static GameObject Capsule(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Capsule);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            go.GetComponent<MeshRenderer>().sharedMaterial=material;var c=go.GetComponent<Collider>();if(c!=null)Object.Destroy(c);return go;
        }
        public static GameObject Cylinder(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            go.GetComponent<MeshRenderer>().sharedMaterial=material;var c=go.GetComponent<Collider>();if(c!=null)Object.Destroy(c);return go;
        }

        public static Light PointLight(Transform parent,string name,Vector3 position,Color color,float intensity,float range)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;
            var light=go.AddComponent<Light>();light.type=LightType.Point;light.color=color;light.intensity=intensity;light.range=range;light.shadows=LightShadows.None;return light;
        }
    }
}
