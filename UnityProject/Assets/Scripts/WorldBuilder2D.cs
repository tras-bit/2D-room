using UnityEngine;

namespace Subsistence
{
    /// <summary>Builds the whole playable world from crisp pixel sprites and 2D colliders only.</summary>
    public static class WorldBuilder2D
    {
        static readonly float[] starts={-24f,6f,36f,66f};
        const float SectorLength=30f;

        public static void Build(Transform parent)
        {
            for(int sector=0;sector<4;sector++)BuildSector(parent,starts[sector],sector);
            var ground=new GameObject("Continuous 2D floor collision");ground.transform.SetParent(parent,false);ground.transform.position=new Vector3(36f,-.26f,0);
            var groundCollider=ground.AddComponent<BoxCollider2D>();groundCollider.size=new Vector2(120f,.52f);
            BuildDoorway(parent,6f,1);BuildDoorway(parent,36f,2);BuildDoorway(parent,66f,3);
            BuildBackgroundDecor(parent);
        }

        static void BuildSector(Transform parent,float x,int theme)
        {
            float center=x+SectorLength*.5f;
            Tiled(parent,"Pixel wallpaper · sector "+theme,PixelArtFactory.WallTile(theme),new Vector2(center,3.9f),new Vector2(SectorLength,7.8f),-20);
            Tiled(parent,"Pixel carpet · sector "+theme,PixelArtFactory.FloorTile(theme),new Vector2(center,-.18f),new Vector2(SectorLength,.52f),-10);
            Tiled(parent,"Pixel ceiling · sector "+theme,PixelArtFactory.CeilingTile(),new Vector2(center,7.92f),new Vector2(SectorLength,.34f),-15);
            // Side-view service corridor dressing: exposed runs, support clips and discrete sodium fixtures.
            for(int i=0;i<6;i++)
            {
                float px=x+2.4f+i*5.0f;
                PixelRect(parent,"Overhead service pipe",new Vector2(px+2.3f,7.08f),new Vector2(4.85f,.17f),new Color(.10f,.15f,.14f),-6);
                PixelRect(parent,"Pipe highlight",new Vector2(px+2.3f,7.16f),new Vector2(4.72f,.035f),theme==0?new Color(.36f,.29f,.17f):new Color(.31f,.38f,.31f),-5);
                PixelRect(parent,"Pipe clamp",new Vector2(px+.32f,7.03f),new Vector2(.14f,.32f),new Color(.36f,.39f,.30f),-4);
                PixelRect(parent,"Ceiling light housing",new Vector2(px+2.2f,7.54f),new Vector2(2.5f,.19f),new Color(.15f,.19f,.16f),-3);
                PixelRect(parent,"Warm pixel light",new Vector2(px+2.2f,7.42f),new Vector2(1.95f,.07f),new Color(.80f,.66f,.39f),-2);
                AddLightFlicker(parent,new Vector2(px+2.2f,7.34f));
                PixelRect(parent,"Lower wall rail",new Vector2(px+2.2f,.53f),new Vector2(4.6f,.15f),theme==3?new Color(.18f,.25f,.23f):new Color(.20f,.24f,.18f),-4);
                if(i%2==0)AddPlacard(parent,px+3.6f,5.55f,theme);
            }

            if(theme==0)BuildLiminalProps(parent,x);
            else if(theme==1)BuildOfficeProps(parent,x);
            else if(theme==2)BuildWetProps(parent,x);
            else BuildMaintenanceProps(parent,x);
        }

        static void BuildLiminalProps(Transform parent,float x)
        {
            PixelRect(parent,"Water damage",new Vector2(x+19,5.7f),new Vector2(4.2f,1.4f),new Color(.30f,.29f,.19f),-4);
            PixelRect(parent,"Loose wallpaper",new Vector2(x+21,4.8f),new Vector2(1.2f,1.8f),new Color(.69f,.59f,.32f),-3);
            PixelRect(parent,"Old pipe",new Vector2(x+12,6.35f),new Vector2(.17f,2.6f),new Color(.30f,.32f,.27f),-2);
        }

        static void BuildOfficeProps(Transform parent,float x)
        {
            for(int i=0;i<3;i++)
            {
                float px=x+8+i*8.3f;
                PixelRect(parent,"Cubicle desk top",new Vector2(px,.92f),new Vector2(3.1f,.18f),new Color(.28f,.22f,.16f),-1);
                PixelRect(parent,"Desk left leg",new Vector2(px-1.25f,.43f),new Vector2(.12f,.82f),new Color(.23f,.26f,.23f),-2);
                PixelRect(parent,"Desk right leg",new Vector2(px+1.25f,.43f),new Vector2(.12f,.82f),new Color(.23f,.26f,.23f),-2);
                PixelRect(parent,"Office partition",new Vector2(px,1.85f),new Vector2(3.1f,1.65f),new Color(.38f,.38f,.29f),-4);
                PixelRect(parent,"CRT monitor",new Vector2(px,1.30f),new Vector2(.72f,.58f),new Color(.12f,.15f,.14f),-1);
                PixelRect(parent,"Dead monitor glass",new Vector2(px,1.32f),new Vector2(.55f,.34f),new Color(.20f,.28f,.26f),0);
            }
        }

        static void BuildWetProps(Transform parent,float x)
        {
            for(int i=0;i<4;i++)
            {
                float px=x+4+i*7.1f;
                PixelRect(parent,"Vertical water pipe",new Vector2(px,4.2f),new Vector2(.22f,6.8f),new Color(.20f,.28f,.27f),-2);
                PixelRect(parent,"Pipe collar",new Vector2(px,6.6f),new Vector2(.42f,.18f),new Color(.39f,.43f,.36f),-1);
                PixelRect(parent,"Leak stain",new Vector2(px+1.2f,5.4f),new Vector2(1.45f,2.2f),new Color(.22f,.30f,.24f),-4);
            }
            PixelRect(parent,"Water on carpet",new Vector2(x+19,.10f),new Vector2(6.2f,.06f),new Color(.21f,.34f,.32f),-7);
        }

        static void BuildMaintenanceProps(Transform parent,float x)
        {
            for(int i=0;i<5;i++)
            {
                float px=x+3+i*5.7f;
                PixelRect(parent,"Industrial wall rib",new Vector2(px,3.9f),new Vector2(.25f,7.8f),new Color(.17f,.21f,.19f),-3);
                PixelRect(parent,"Cable tray",new Vector2(px+1.8f,6.85f),new Vector2(3.3f,.24f),new Color(.20f,.25f,.23f),-2);
            }
            PixelRect(parent,"Generator body",new Vector2(x+23,1.12f),new Vector2(2.6f,1.7f),new Color(.19f,.24f,.22f),0);
            PixelRect(parent,"Generator face",new Vector2(x+23,1.2f),new Vector2(1.25f,.66f),new Color(.11f,.15f,.14f),1);
            PixelRect(parent,"Generator warning",new Vector2(x+23.35f,1.34f),new Vector2(.16f,.14f),new Color(.83f,.29f,.13f),2);
        }

        static void BuildDoorway(Transform parent,float x,int sector)
        {
            PixelRect(parent,"Door frame left",new Vector2(x,.3f),new Vector2(.30f,3.55f),new Color(.11f,.16f,.15f),-3);
            PixelRect(parent,"Door frame right",new Vector2(x+1.7f,.3f),new Vector2(.30f,3.55f),new Color(.11f,.16f,.15f),-3);
            PixelRect(parent,"Door lintel",new Vector2(x+.85f,3.95f),new Vector2(2.05f,.30f),new Color(.20f,.26f,.22f),-3);
            var door=new GameObject("Backrooms security door · zone "+sector);door.transform.SetParent(parent,false);door.transform.position=new Vector3(x+.85f,0,0);
            var renderer=door.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.BunkerDoor();renderer.sortingOrder=-2;
            PixelRect(parent,"Sector warning sign",new Vector2(x+.85f,4.42f),new Vector2(1.45f,.40f),new Color(.08f,.12f,.11f),-1);
            PixelRect(parent,"Sector sign stripe",new Vector2(x+.85f,4.42f),new Vector2(.95f,.075f),sector==3?new Color(.75f,.22f,.12f):new Color(.75f,.54f,.27f),0);
            PixelRect(parent,"Door status lamp",new Vector2(x+1.13f,2.02f),new Vector2(.08f,.22f),new Color(.86f,.35f,.17f),1);
        }

        static void AddPlacard(Transform parent,float x,float y,int theme)
        {
            var go=new GameObject("Hand-painted sector placard");go.transform.SetParent(parent,false);go.transform.position=new Vector3(x,y,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.WarningPlacard(theme);renderer.sortingOrder=-4;
        }

        static void BuildBackgroundDecor(Transform parent)
        {
            // Pixel silhouettes add depth while remaining on 2D sprite layers.
            for(int i=0;i<12;i++)
            {
                float x=-22+i*10.6f;
                PixelRect(parent,"Near shadow column",new Vector2(x,2.4f),new Vector2(.30f,4.8f),new Color(.08f,.11f,.10f,.42f),-8);
                PixelRect(parent,"Column cap",new Vector2(x,4.9f),new Vector2(.55f,.14f),new Color(.17f,.21f,.17f),-7);
                PixelRect(parent,"Column base",new Vector2(x,.13f),new Vector2(.52f,.18f),new Color(.13f,.16f,.14f),-7);
            }
        }

        static void Tiled(Transform parent,string name,Sprite sprite,Vector2 position,Vector2 size,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=new Vector3(position.x,position.y,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.drawMode=SpriteDrawMode.Tiled;renderer.size=size;renderer.sortingOrder=order;renderer.tileMode=SpriteTileMode.Continuous;
        }

        static void PixelRect(Transform parent,string name,Vector2 center,Vector2 size,Color color,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=new Vector3(center.x,center.y,0);
            go.transform.localScale=new Vector3(size.x,size.y,1);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Block(Color.white);renderer.color=color;renderer.sortingOrder=order;
        }

        static void AddLightFlicker(Transform parent,Vector2 position)
        {
            var go=new GameObject("Pixel light flicker");go.transform.SetParent(parent,false);go.transform.position=new Vector3(position.x,position.y,0);
            go.transform.localScale=new Vector3(2.15f,.32f,1f);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Block(Color.white);renderer.color=new Color(.92f,.84f,.57f,.18f);renderer.sortingOrder=-1;
            go.AddComponent<PixelLightFlicker>();
        }
    }
}
