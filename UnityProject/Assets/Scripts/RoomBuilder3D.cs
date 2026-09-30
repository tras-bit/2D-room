using UnityEngine;

namespace Subsistence
{
    /// <summary>Builds a readable, perspective 2.5D sequence of distinct Backrooms spaces at runtime.</summary>
    public static class RoomBuilder3D
    {
        static Material officeFloor,serviceFloor,wetFloor,officeWall,greenWall,serviceWall,partition,furniture;
        public static void Build(Transform parent)
        {
            RuntimeArtFactory.Initialize();CreateMaterials();
            float[] starts={-24f,6f,36f,66f};float length=30f;
            BuildSection(parent,starts[0],length,0,"YELLOW LIMINAL HALL",RuntimeArtFactory.Wall,RuntimeArtFactory.Carpet);
            BuildSection(parent,starts[1],length,1,"OFFICE MAZE",officeWall,officeFloor);
            BuildSection(parent,starts[2],length,2,"LEAKING SERVICE WING",greenWall,wetFloor);
            BuildSection(parent,starts[3],length,3,"MAINTENANCE SECTOR",serviceWall,serviceFloor);
            BuildPortal(parent,6f,"SECTION 02");BuildPortal(parent,36f,"SECTION 03");BuildPortal(parent,66f,"SECTION 04");
            CreateDistantBlack(parent);
        }
        static void CreateMaterials()
        {
            officeFloor=Mat(new Color(.34f,.35f,.29f),null,.18f,0);
            serviceFloor=Mat(new Color(.25f,.29f,.27f),null,.42f,.14f);
            wetFloor=Mat(new Color(.30f,.34f,.30f),null,.82f,.18f);
            officeWall=Mat(new Color(.69f,.60f,.36f),Resources.Load<Texture2D>("Art/wallpaper_backrooms"),.10f,0);
            greenWall=Mat(new Color(.47f,.52f,.39f),Resources.Load<Texture2D>("Art/wallpaper_backrooms"),.10f,0);
            serviceWall=Mat(new Color(.29f,.34f,.31f),null,.42f,.17f);
            partition=Mat(new Color(.42f,.43f,.34f),null,.12f,0);
            furniture=Mat(new Color(.31f,.27f,.19f),null,.12f,0);
            if(officeWall.mainTexture!=null){officeWall.mainTexture.wrapMode=TextureWrapMode.Repeat;officeWall.mainTextureScale=new Vector2(3,2);greenWall.mainTexture.wrapMode=TextureWrapMode.Repeat;greenWall.mainTextureScale=new Vector2(3,2);}
        }
        static Material Mat(Color c,Texture tex,float smooth,float metallic)
        {var m=new Material(Shader.Find("Standard")){color=c};if(tex!=null)m.mainTexture=tex;if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",smooth);if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic);return m;}

        static void BuildSection(Transform parent,float x,float length,int theme,string title,Material wall,Material floor)
        {
            var root=new GameObject("Backrooms sector · "+title).transform;root.SetParent(parent,false);
            RuntimeArtFactory.Cube(root,"Carpet / floor",new Vector3(x+length/2,-.24f,3),new Vector3(length,.45f,14),floor);
            RuntimeArtFactory.Cube(root,"Ceiling",new Vector3(x+length/2,7.35f,3),new Vector3(length,.32f,14),theme==3?RuntimeArtFactory.Concrete:RuntimeArtFactory.Ceiling);
            RuntimeArtFactory.Cube(root,"Endless rear wall",new Vector3(x+length/2,3.62f,9.6f),new Vector3(length,7.5f,.32f),wall);
            RuntimeArtFactory.Cube(root,"Wall skirting",new Vector3(x+length/2,.18f,9.36f),new Vector3(length,.27f,.13f),theme==3?RuntimeArtFactory.Metal:RuntimeArtFactory.Trim);
            RuntimeArtFactory.Cube(root,"Ceiling perimeter",new Vector3(x+length/2,7.08f,9.30f),new Vector3(length,.15f,.16f),RuntimeArtFactory.Trim);
            // Repeated ribs give the flat gameplay lane a deep, perspective vanishing point.
            for(int i=0;i<5;i++)
            {
                float px=x+3+i*6.1f;
                RuntimeArtFactory.Cube(root,"Distant wallpaper pier",new Vector3(px,3.54f,7.9f),new Vector3(.85f,6.95f,.78f),theme==3?RuntimeArtFactory.Metal:wall);
                RuntimeArtFactory.Cube(root,"Pier foot",new Vector3(px,.23f,7.42f),new Vector3(1.13f,.30f,1.2f),theme==3?RuntimeArtFactory.Metal:RuntimeArtFactory.Trim);
                RuntimeArtFactory.Cube(root,"Pier crown",new Vector3(px,6.96f,7.55f),new Vector3(1.15f,.22f,1.0f),theme==3?RuntimeArtFactory.Metal:RuntimeArtFactory.Trim);
            }
            // A low foreground divider suggests the room's true depth without occluding the player.
            for(int i=0;i<4;i++)
            {
                float px=x+4+i*7.3f;
                RuntimeArtFactory.Cube(root,"Near room column",new Vector3(px,2.4f,2.2f),new Vector3(.56f,4.8f,.48f),theme==3?RuntimeArtFactory.Metal:(theme==0?RuntimeArtFactory.StainedWall:partition));
                RuntimeArtFactory.Cube(root,"Column base",new Vector3(px,.18f,2.2f),new Vector3(.80f,.28f,.65f),RuntimeArtFactory.Metal);
            }
            for(int i=0;i<4;i++)
            {
                float lx=x+4+i*7.0f;
                var lamp=RuntimeArtFactory.Cube(root,"Flickering fluorescent fixture",new Vector3(lx,7.07f,2.8f),new Vector3(2.35f,.10f,1.15f),RuntimeArtFactory.Fluorescent);
                var light=RuntimeArtFactory.PointLight(root,"Fluorescent spill",new Vector3(lx,6.68f,1.6f),theme==2?new Color(.70f,.80f,.53f):new Color(1f,.88f,.57f),theme==2 ? .68f : 1.05f,13f);
                light.gameObject.AddComponent<LightFlicker3D>();
            }
            if(theme==0)BuildLiminalProps(root,x);
            else if(theme==1)BuildOfficeProps(root,x);
            else if(theme==2)BuildWetProps(root,x);
            else BuildMaintenanceProps(root,x);
        }
        static void BuildLiminalProps(Transform root,float x)
        {
            // Old water marks and a missing wallpaper patch, built from low-contrast geometry.
            var stain=Mat(new Color(.52f,.46f,.27f),null,.02f,0);
            RuntimeArtFactory.Cube(root,"Faded wallpaper stain",new Vector3(x+20,4.25f,9.38f),new Vector3(4.2f,2.0f,.025f),stain);
            RuntimeArtFactory.Cube(root,"Wallpaper remnant",new Vector3(x+20,4.35f,9.34f),new Vector3(2.45f,1.7f,.03f),RuntimeArtFactory.Wall);
            for(int i=0;i<3;i++)RuntimeArtFactory.Cube(root,"Pipe seam",new Vector3(x+12+i*1.4f,6.72f,8.6f),new Vector3(.05f,.36f,.06f),RuntimeArtFactory.Metal);
        }
        static void BuildOfficeProps(Transform root,float x)
        {
            for(int i=0;i<3;i++)
            {
                float px=x+8+i*8.2f;float z=3.5f+(i%2)*1.1f;
                RuntimeArtFactory.Cube(root,"Cubicle desk",new Vector3(px,.87f,z),new Vector3(3.3f,.16f,1.3f),furniture);
                RuntimeArtFactory.Cube(root,"Desk support",new Vector3(px-1.25f,.42f,z),new Vector3(.12f,.86f,.12f),RuntimeArtFactory.Metal);
                RuntimeArtFactory.Cube(root,"Desk support",new Vector3(px+1.25f,.42f,z),new Vector3(.12f,.86f,.12f),RuntimeArtFactory.Metal);
                RuntimeArtFactory.Cube(root,"Cubicle divider",new Vector3(px,1.75f,z+1.0f),new Vector3(3.3f,1.6f,.16f),partition);
                RuntimeArtFactory.Cube(root,"Old CRT monitor",new Vector3(px,.99f,z+.34f),new Vector3(.64f,.48f,.42f),RuntimeArtFactory.Rubber);
                RuntimeArtFactory.Cube(root,"Dead screen",new Vector3(px,.99f,z+.10f),new Vector3(.53f,.34f,.025f),RuntimeArtFactory.Concrete);
            }
        }
        static void BuildWetProps(Transform root,float x)
        {
            for(int i=0;i<4;i++)
            {
                float px=x+5+i*6.3f;
                RuntimeArtFactory.Cylinder(root,"Water main",new Vector3(px,5.95f,8.7f),new Vector3(.18f,6.0f,.18f),RuntimeArtFactory.Metal);
                RuntimeArtFactory.Cube(root,"Pipe elbow",new Vector3(px,6.0f,8.3f),new Vector3(.55f,.18f,.7f),RuntimeArtFactory.Metal);
                RuntimeArtFactory.Cube(root,"Leak-stained wall patch",new Vector3(px+1.15f,3.95f,9.35f),new Vector3(1.55f,2.6f,.04f),RuntimeArtFactory.StainedWall);
            }
            var puddle=Mat(new Color(.25f,.36f,.34f),null,.94f,.26f);
            RuntimeArtFactory.Cube(root,"Shallow reflective puddle",new Vector3(x+19,.01f,1.9f),new Vector3(6f,.018f,3.2f),puddle);
        }
        static void BuildMaintenanceProps(Transform root,float x)
        {
            for(int i=0;i<4;i++)
            {
                float px=x+5+i*6.4f;
                RuntimeArtFactory.Cube(root,"Industrial wall rib",new Vector3(px,3.55f,9.28f),new Vector3(.22f,7f,.19f),RuntimeArtFactory.Metal);
                RuntimeArtFactory.Cube(root,"Cable tray",new Vector3(px+1.4f,6.7f,8.8f),new Vector3(2.9f,.18f,.28f),RuntimeArtFactory.Metal);
            }
            RuntimeArtFactory.Cube(root,"Generator",new Vector3(x+24,.95f,4.2f),new Vector3(2.5f,1.75f,1.9f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(root,"Generator panel",new Vector3(x+24,.96f,3.2f),new Vector3(1.0f,.7f,.08f),RuntimeArtFactory.Concrete);
            RuntimeArtFactory.PointLight(root,"Generator status lamp",new Vector3(x+24,1.6f,3.0f),new Color(.78f,.24f,.12f),.45f,3.5f);
        }
        static void BuildPortal(Transform parent,float x,string label)
        {
            var frame=RuntimeArtFactory.Metal;
            RuntimeArtFactory.Cube(parent,"Sector doorway left",new Vector3(x,3.2f,4.2f),new Vector3(.30f,6.4f,6.0f),frame);
            RuntimeArtFactory.Cube(parent,"Sector doorway right",new Vector3(x+1.6f,3.2f,4.2f),new Vector3(.30f,6.4f,6.0f),frame);
            RuntimeArtFactory.Cube(parent,"Sector doorway lintel",new Vector3(x+.8f,6.3f,4.2f),new Vector3(1.9f,.28f,6.0f),frame);
            RuntimeArtFactory.Cube(parent,"Sector sign",new Vector3(x+.8f,6.65f,3.0f),new Vector3(2.0f,.32f,.12f),RuntimeArtFactory.Concrete);
            var tm=new GameObject(label).AddComponent<TextMesh>();tm.transform.SetParent(parent,false);tm.transform.position=new Vector3(x+.8f,6.63f,2.91f);tm.transform.rotation=Quaternion.Euler(0,180,0);tm.text=label;tm.characterSize=.07f;tm.fontSize=34;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=new Color(.83f,.78f,.55f);
        }
        static void CreateDistantBlack(Transform parent)
        {
            var back=RuntimeArtFactory.Cube(parent,"Darkness beyond the corridors",new Vector3(36,5,13.1f),new Vector3(190,20,.4f),RuntimeArtFactory.Concrete);
        }
    }
}
