using UnityEngine;

namespace Subsistence
{
    /// <summary>Builds the yellow, non-linear-feeling Level 0 lobby and its industrial Level 1 destination in strict 2D.</summary>
    public static class WorldBuilder2D
    {
        public const float Level0Start=-24f,Level0End=96f,Level1Start=112f,Level1End=232f;
        const float SegmentLength=30f;

        public static void Build(Transform parent)
        {
            BuildLevelZero(parent);
            BuildIndustrialLevel(parent,Level1Start,1);
            BuildBoundary(parent,Level0End,"Level 0 anomalous lift threshold");
            BuildBoundary(parent,Level1End,"Level 1 corridor end");
        }

        static void BuildLevelZero(Transform parent)
        {
            for(int section=0;section<4;section++)
            {
                float x=Level0Start+section*SegmentLength,center=x+SegmentLength*.5f;
                Tiled(parent,"Level 0 · yellow patterned wallpaper",PixelArtFactory.WallTile(0),new Vector2(center,3.9f),new Vector2(SegmentLength,7.8f),-20);
                Tiled(parent,"Level 0 · damp old carpet",PixelArtFactory.FloorTile(0),new Vector2(center,-.18f),new Vector2(SegmentLength,.52f),-10);
                Tiled(parent,"Level 0 · drop ceiling panels",PixelArtFactory.CeilingTile(0),new Vector2(center,7.92f),new Vector2(SegmentLength,.34f),-15);
                if(section==1||section==3)
                    PixelRect(parent,"Level 0 · weak-lamp shadow pool",new Vector2(x+12f,4.0f),new Vector2(16f,6.6f),new Color(.025f,.032f,.024f,.20f),-19);
                BuildLevelZeroDetails(parent,x,section);
            }
            var floor=new GameObject("Level 0 · continuous walkable carpet");floor.transform.SetParent(parent,false);
            floor.transform.position=new Vector3((Level0Start+Level0End)*.5f,-.26f,0);
            floor.AddComponent<BoxCollider2D>().size=new Vector2(Level0End-Level0Start,.52f);

            // Repeating, slightly shifted wall mouths and false corridors create visual disorientation without blocking movement.
            BuildFalsePassage(parent,-2f,0);BuildFalsePassage(parent,28f,1);BuildFalsePassage(parent,69f,2);
            BuildManilaRoom(parent,39f);BuildDustMotes(parent);

            for(int i=0;i<5;i++)
            {
                float x=Level0Start+9f+i*23f;
                float width=.10f+(i%2)*.08f;
                PixelRect(parent,"Level 0 · damp carpet stain",new Vector2(x,.035f),new Vector2(1.2f+(i%3)*.45f,width),new Color(.18f,.19f,.105f,.68f),-7);
                PixelRect(parent,"Level 0 · carpet wet sheen",new Vector2(x+.28f,.055f),new Vector2(.31f,.025f),new Color(.60f,.54f,.31f,.48f),-6);
            }
            BuildBoundary(parent,Level0Start-1.5f,"Level 0 starting wall");
        }

        static void BuildDustMotes(Transform parent)
        {
            for(int i=0;i<18;i++)
            {
                float x=Level0Start+3.5f+i*6.4f;
                float y=1.15f+(i*17%49)*.105f;
                float size=.035f+(i%3)*.012f;
                var mote=PixelRect(parent,"Level 0 · drifting dust mote",new Vector2(x,y),new Vector2(size,size),
                    new Color(.84f,.76f,.56f,.22f+(i%3)*.035f),2);
                mote.AddComponent<AmbientDust2D>().Initialize(i);
            }
        }

        static void BuildLevelZeroDetails(Transform parent,float x,int section)
        {
            // Wide aged wooden skirting is a defining detail of the old retail back rooms.
            PixelRect(parent,"Level 0 · stained wood skirting",new Vector2(x+SegmentLength*.5f,.21f),new Vector2(SegmentLength,.25f),new Color(.42f,.30f,.13f),-7);
            PixelRect(parent,"Level 0 · skirting highlight",new Vector2(x+SegmentLength*.5f,.34f),new Vector2(SegmentLength,.045f),new Color(.69f,.50f,.22f),-6);
            PixelRect(parent,"Level 0 · wallpaper seam",new Vector2(x+15.05f,4.1f),new Vector2(.035f,7.1f),new Color(.53f,.44f,.24f,.48f),-6);

            float outletX=x+(section%2==0?7f:21f);
            AddWallOutlet(parent,new Vector2(outletX,1.0f));
            if(section==1||section==3)AddWallOutlet(parent,new Vector2(x+27f,1.35f));

            // One unevenly spaced panel per room; half are weak/dead so Level 0 stays dim and relies on the torch.
            float lx=x+(section%2==0?9.5f:21.5f);
            bool activeTube=section==0||section==2;
            PixelRect(parent,"Level 0 · stained fluorescent fixture housing",new Vector2(lx,7.48f),new Vector2(3.05f,.23f),new Color(.34f,.32f,.24f),-4);
            PixelRect(parent,"Level 0 · weak fluorescent tube",new Vector2(lx,7.35f),new Vector2(2.56f,.10f),
                activeTube?new Color(.69f,.64f,.47f):new Color(.20f,.20f,.16f),-3);
            PixelRect(parent,"Level 0 · dusty fluorescent diffuser",new Vector2(lx,7.29f),new Vector2(2.12f,.045f),
                activeTube?new Color(.90f,.82f,.60f,.24f):new Color(.23f,.22f,.16f,.40f),-2);
            if(activeTube)AddLightFlicker(parent,new Vector2(lx,7.20f),new Color(1f,.89f,.57f,.16f));
            else PixelRect(parent,"Level 0 · dead ballast scorch",new Vector2(lx+.9f,7.20f),new Vector2(.45f,.06f),new Color(.13f,.12f,.09f),-1);

            // Old pasted-over patches, vertical water marks and slightly misaligned paper repeats.
            if(section%2==0)
            {
                PixelRect(parent,"Level 0 · faded wallpaper repair",new Vector2(x+23f,3.7f),new Vector2(.76f,1.10f),new Color(.66f,.57f,.32f,.55f),-8);
                PixelRect(parent,"Level 0 · wallpaper tear shadow",new Vector2(x+24.2f,2.95f),new Vector2(.10f,.56f),new Color(.30f,.29f,.16f,.72f),-7);
            }
            else
            {
                PixelRect(parent,"Level 0 · old moisture run",new Vector2(x+4.2f,4.3f),new Vector2(.16f,1.7f),new Color(.34f,.35f,.20f,.40f),-8);
                PixelRect(parent,"Level 0 · water tide mark",new Vector2(x+4.35f,3.55f),new Vector2(.31f,.12f),new Color(.48f,.42f,.23f,.56f),-7);
            }

            // A handful of near-wall returns break the long sightline; these are background art only.
            if(section>0)
            {
                float px=x+.6f;
                PixelRect(parent,"Level 0 · wallpapered return column",new Vector2(px,3.5f),new Vector2(.34f,6.8f),new Color(.67f,.59f,.36f),-5);
                PixelRect(parent,"Level 0 · column edge shadow",new Vector2(px+.21f,3.5f),new Vector2(.09f,6.8f),new Color(.31f,.29f,.17f,.72f),-4);
            }
        }

        static void BuildFalsePassage(Transform parent,float x,int variant)
        {
            float h=variant==1?4.7f:5.25f,w=variant==2?3.8f:3.05f;
            PixelRect(parent,"Level 0 · false corridor shadow",new Vector2(x,2.25f+h*.5f),new Vector2(w,h),new Color(.20f,.20f,.12f),-9);
            PixelRect(parent,"Level 0 · distant yellow room",new Vector2(x,2.0f+h*.5f),new Vector2(w-.34f,h-.5f),new Color(.49f,.45f,.28f),-8);
            PixelRect(parent,"Level 0 · left wallpaper return",new Vector2(x-w*.5f,2.0f+h*.5f),new Vector2(.15f,h),new Color(.84f,.73f,.43f),-5);
            PixelRect(parent,"Level 0 · right wallpaper return",new Vector2(x+w*.5f,2.0f+h*.5f),new Vector2(.15f,h),new Color(.73f,.62f,.36f),-5);
            PixelRect(parent,"Level 0 · passage ceiling shadow",new Vector2(x,2f+h),new Vector2(w+.2f,.16f),new Color(.75f,.65f,.40f),-5);
            PixelRect(parent,"Level 0 · weak false fluorescent glow",new Vector2(x,2.0f+h*.5f+1.4f),new Vector2(.72f,.055f),new Color(1f,.88f,.57f,.34f),-4);
        }

        static void BuildManilaRoom(Transform parent,float x)
        {
            // The rare Manila Room provides a diegetic solo meeting/trading anomaly without adding online wanderers.
            Tiled(parent,"Level 0 · rare Manila Room wallpaper",PixelArtFactory.ManilaWallTile(),new Vector2(x,3.9f),new Vector2(10f,7.8f),-17);
            PixelRect(parent,"Manila Room · left wood jamb",new Vector2(x-5f,3.65f),new Vector2(.23f,7.25f),new Color(.34f,.24f,.12f),-4);
            PixelRect(parent,"Manila Room · right wood jamb",new Vector2(x+5f,3.65f),new Vector2(.23f,7.25f),new Color(.34f,.24f,.12f),-4);
            PixelRect(parent,"Manila Room · warm ceiling fixture",new Vector2(x,7.35f),new Vector2(2.25f,.14f),new Color(.92f,.63f,.30f),-2);
            AddLightFlicker(parent,new Vector2(x,7.20f),new Color(1f,.66f,.31f,.16f));
            PixelRect(parent,"Manila Room · old table shadow",new Vector2(x,1.05f),new Vector2(3.0f,.15f),new Color(.26f,.18f,.10f),-3);
            PixelRect(parent,"Manila Room · wooden counter",new Vector2(x,1.18f),new Vector2(2.9f,.16f),new Color(.53f,.37f,.18f),-2);
            PixelRect(parent,"Manila Room · counter highlight",new Vector2(x,1.27f),new Vector2(2.76f,.035f),new Color(.78f,.55f,.26f),-1);
            PixelRect(parent,"Manila Room · left table leg",new Vector2(x-1.08f,.72f),new Vector2(.12f,.75f),new Color(.37f,.25f,.12f),-2);
            PixelRect(parent,"Manila Room · right table leg",new Vector2(x+1.08f,.72f),new Vector2(.12f,.75f),new Color(.37f,.25f,.12f),-2);
            PixelRect(parent,"Manila Room · paper on counter",new Vector2(x+.66f,1.39f),new Vector2(.37f,.035f),new Color(.83f,.76f,.56f),0);
        }

        static void BuildIndustrialLevel(Transform parent,float start,int level)
        {
            const float length=Level1End-Level1Start;
            for(int section=0;section<4;section++)
            {
                float x=start+section*SegmentLength,center=x+SegmentLength*.5f;
                Tiled(parent,"Level 1 · concrete service wall",PixelArtFactory.WallTile(3),new Vector2(center,3.9f),new Vector2(SegmentLength,7.8f),-20);
                Tiled(parent,"Level 1 · stained concrete floor",PixelArtFactory.FloorTile(3),new Vector2(center,-.18f),new Vector2(SegmentLength,.52f),-10);
                Tiled(parent,"Level 1 · exposed ceiling",PixelArtFactory.CeilingTile(3),new Vector2(center,7.92f),new Vector2(SegmentLength,.34f),-15);
                BuildIndustrialDressing(parent,x,section,level);
            }
            var floor=new GameObject("Level 1 · continuous walkable floor");floor.transform.SetParent(parent,false);
            floor.transform.position=new Vector3(start+length*.5f,-.26f,0);
            floor.AddComponent<BoxCollider2D>().size=new Vector2(length,.52f);
        }

        static void BuildIndustrialDressing(Transform parent,float x,int section,int level)
        {
            for(int i=0;i<5;i++)
            {
                float px=x+2f+i*5.5f;
                PixelRect(parent,"Level 1 · exposed service pipe",new Vector2(px+2.3f,7.08f),new Vector2(4.85f,.17f),new Color(.10f,.15f,.14f),-6);
                PixelRect(parent,"Level 1 · rust pipe seam",new Vector2(px+2.3f,7.16f),new Vector2(4.72f,.035f),new Color(.36f,.29f,.17f),-5);
                PixelRect(parent,"Level 1 · pipe bracket",new Vector2(px+.32f,7.03f),new Vector2(.14f,.32f),new Color(.36f,.39f,.30f),-4);
                if(i%2==0)
                {
                    PixelRect(parent,"Level 1 · fluorescent fixture housing",new Vector2(px+2.2f,7.54f),new Vector2(2.5f,.19f),new Color(.15f,.19f,.16f),-3);
                    PixelRect(parent,"Level 1 · fluorescent tube",new Vector2(px+2.2f,7.42f),new Vector2(1.95f,.07f),new Color(.80f,.66f,.39f),-2);
                    AddLightFlicker(parent,new Vector2(px+2.2f,7.34f),new Color(.92f,.84f,.57f,.18f));
                }
            }
            PixelRect(parent,"Level 1 · heavy wall base rail",new Vector2(x+15f,.53f),new Vector2(29f,.15f),new Color(.20f,.24f,.18f),-4);
            if(section==1||section==3)BuildIndustrialRecess(parent,x+18f);
            if(section==2)BuildUtilityCabinet(parent,x+23f);
            if(section%2==0)AddPlacard(parent,x+23f,5.55f,3);
        }

        static void BuildIndustrialRecess(Transform parent,float x)
        {
            PixelRect(parent,"Level 1 · service recess shadow",new Vector2(x,2.05f),new Vector2(2.15f,3.55f),new Color(.035f,.047f,.041f),-7);
            PixelRect(parent,"Level 1 · recess left jamb",new Vector2(x-1.15f,2.08f),new Vector2(.16f,3.7f),new Color(.28f,.33f,.26f),-5);
            PixelRect(parent,"Level 1 · recess right jamb",new Vector2(x+1.15f,2.08f),new Vector2(.16f,3.7f),new Color(.27f,.30f,.24f),-5);
            PixelRect(parent,"Level 1 · recess header",new Vector2(x,3.94f),new Vector2(2.4f,.18f),new Color(.30f,.33f,.26f),-5);
        }

        static void BuildUtilityCabinet(Transform parent,float x)
        {
            PixelRect(parent,"Level 1 · service panel",new Vector2(x,3.4f),new Vector2(.82f,2.35f),new Color(.10f,.15f,.14f),-6);
            PixelRect(parent,"Level 1 · panel rust edge",new Vector2(x,3.4f),new Vector2(.69f,2.16f),new Color(.31f,.20f,.15f),-5);
            PixelRect(parent,"Level 1 · panel inset",new Vector2(x,3.4f),new Vector2(.52f,1.94f),new Color(.07f,.11f,.10f),-4);
            PixelRect(parent,"Level 1 · panel green lamp",new Vector2(x+.10f,3.78f),new Vector2(.09f,.09f),new Color(.48f,.69f,.34f),-3);
            PixelRect(parent,"Level 1 · panel amber lamp",new Vector2(x+.10f,3.52f),new Vector2(.09f,.09f),new Color(.82f,.52f,.22f),-3);
        }

        static void AddWallOutlet(Transform parent,Vector2 position)
        {
            var go=new GameObject("Level 0 · old wall outlet");go.transform.SetParent(parent,false);go.transform.position=new Vector3(position.x,position.y,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.WallOutlet();renderer.sortingOrder=-5;
        }

        static void BuildBoundary(Transform parent,float x,string name)
        {
            var go=new GameObject(name+" · collision boundary");go.transform.SetParent(parent,false);go.transform.position=new Vector3(x+.35f,3.4f,0);
            go.AddComponent<BoxCollider2D>().size=new Vector2(.45f,7.2f);
        }

        static void AddPlacard(Transform parent,float x,float y,int theme)
        {
            var go=new GameObject("Level 1 · warning placard");go.transform.SetParent(parent,false);go.transform.position=new Vector3(x,y,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.WarningPlacard(theme);renderer.sortingOrder=-4;
        }

        static void Tiled(Transform parent,string name,Sprite sprite,Vector2 position,Vector2 size,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=new Vector3(position.x,position.y,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.drawMode=SpriteDrawMode.Tiled;renderer.size=size;renderer.sortingOrder=order;renderer.tileMode=SpriteTileMode.Continuous;
        }

        static GameObject PixelRect(Transform parent,string name,Vector2 center,Vector2 size,Color color,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=new Vector3(center.x,center.y,0);go.transform.localScale=new Vector3(size.x,size.y,1);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Block(Color.white);renderer.color=color;renderer.sortingOrder=order;
            return go;
        }

        static void AddLightFlicker(Transform parent,Vector2 position,Color color)
        {
            var go=new GameObject("Fluorescent light · unstable buzz");go.transform.SetParent(parent,false);go.transform.position=new Vector3(position.x,position.y,0);go.transform.localScale=new Vector3(2.15f,.32f,1f);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Block(Color.white);renderer.color=color;renderer.sortingOrder=-1;go.AddComponent<PixelLightFlicker>();
        }
    }
}
