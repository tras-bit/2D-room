using UnityEngine;

namespace Subsistence
{
    /// <summary>
    /// Builds only the Backrooms environment in strict 2D. It never creates a
    /// player base, home storage or workbench; those belong to player building.
    /// Pixel-art tiles span each level so their repeats stay continuous.
    /// </summary>
    public static class WorldBuilder2D
    {
        public const float Level0Start=-24f,Level0End=96f,Level1Start=112f,Level1End=232f;
        const float SegmentLength=30f;
        const float FloorHeight=.52f;

        public static void Build(Transform parent)
        {
            BuildLevelZero(parent);
            BuildIndustrialLevel(parent,Level1Start,1);
            BuildBoundary(parent,Level0End,"Level 0 anomalous lift threshold");
            BuildBoundary(parent,Level1End,"Level 1 corridor end");
        }

        // ------------------------------------------------------------------
        // Level 0 - the lobby
        // ------------------------------------------------------------------
        static void BuildLevelZero(Transform parent)
        {
            float levelLength=Level0End-Level0Start;
            float levelCenter=(Level0Start+Level0End)*.5f;
            // One renderer per material keeps the texture phase continuous across
            // the 30-unit decoration sections below (and aligns the 4-unit tile).
            Tiled(parent,"Level 0 · yellow patterned wallpaper",PixelArtFactory.WallTile(0),new Vector2(levelCenter,3.9f),new Vector2(levelLength,7.8f),-20);
            Tiled(parent,"Level 0 · damp old carpet",PixelArtFactory.FloorTile(0),new Vector2(levelCenter,-.25f),new Vector2(levelLength,.5f),-10);
            Tiled(parent,"Level 0 · drop ceiling panels",PixelArtFactory.CeilingTile(0),new Vector2(levelCenter,7.85f),new Vector2(levelLength,.33f),-15);
            for(int section=0;section<4;section++)
            {
                float x=Level0Start+section*SegmentLength;
                if(section==1||section==3)
                    SoftRect(parent,"Level 0 · weak-lamp shadow pool",new Vector2(x+12f,4.0f),new Vector2(16f,6.6f),new Color(.02f,.03f,.02f,.30f),-19);
                BuildLevelZeroDetails(parent,x,section);
            }
            var floor=new GameObject("Level 0 · continuous walkable carpet");
            floor.transform.SetParent(parent,false);
            floor.transform.position=new Vector3((Level0Start+Level0End)*.5f,-.26f,0);
            floor.AddComponent<BoxCollider2D>().size=new Vector2(Level0End-Level0Start,FloorHeight);

            // Repeating, slightly shifted wall mouths and false corridors create visual disorientation without blocking movement.
            BuildFalsePassage(parent,-2f,0);BuildFalsePassage(parent,28f,1);BuildFalsePassage(parent,69f,2);
            BuildManilaRoom(parent,39f);BuildDustMotes(parent);

            for(int i=0;i<5;i++)
            {
                float x=Level0Start+9f+i*23f;
                float width=.10f+(i%2)*.08f;
                SoftRect(parent,"Level 0 · damp carpet stain",new Vector2(x,.03f),new Vector2(1.2f+(i%3)*.45f,width),new Color(.17f,.18f,.10f,.62f),-7);
                SoftRect(parent,"Level 0 · carpet wet sheen",new Vector2(x+.28f,.05f),new Vector2(.31f,.025f),new Color(.58f,.52f,.30f,.42f),-6);
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
                var mote=SoftRect(parent,"Level 0 · drifting dust mote",new Vector2(x,y),new Vector2(size,size),
                    new Color(.84f,.76f,.56f,.22f+(i%3)*.035f),2);
                mote.AddComponent<AmbientDust2D>().Initialize(i);
            }
        }

        static void BuildLevelZeroDetails(Transform parent,float x,int section)
        {
            // Wide aged wooden skirting is a defining detail of the old retail back rooms.
            SpriteRenderer skirting=Solid(parent,"Level 0 · stained wood skirting",new Vector2(x+SegmentLength*.5f,.21f),new Vector2(SegmentLength,.25f),new Color(.40f,.28f,.12f),-7);
            Solid(parent,"Level 0 · skirting top bevel",new Vector2(x+SegmentLength*.5f,.335f),new Vector2(SegmentLength,.03f),new Color(.66f,.47f,.20f),-6);
            if(skirting!=null)skirting.color=new Color(.40f,.28f,.12f);
            SoftRect(parent,"Level 0 · wallpaper seam",new Vector2(x+15.05f,4.1f),new Vector2(.03f,7.1f),new Color(.42f,.35f,.19f,.45f),-6);

            // Old wall sockets, unevenly spaced.
            float outletX=x+(section%2==0?7f:21f);
            AddProp(parent,"Level 0 · old power socket",PixelArtFactory.WallOutlet(),new Vector2(outletX,1.35f),-5,false);
            if(section==1||section==3)AddProp(parent,"Level 0 · second power socket",PixelArtFactory.WallOutlet(),new Vector2(x+27f,1.55f),-5,false);

            // One unevenly spaced fluorescent fixture per room; half are dead so
            // Level 0 stays dim and relies on the torch.
            float lx=x+(section%2==0?9.5f:21.5f);
            bool activeTube=section==0||section==2;
            AddProp(parent,activeTube?"Level 0 · humming fluorescent fixture":"Level 0 · dead fluorescent fixture",
                PixelArtFactory.CeilingLamp(),new Vector2(lx,7.72f),-4,false);
            if(activeTube)
            {
                SoftRect(parent,"Level 0 · lamp spill",new Vector2(lx,7.35f),new Vector2(2.2f,.07f),new Color(1f,.91f,.63f,.34f),-3);
                AddLightFlicker(parent,new Vector2(lx,7.20f),new Color(1f,.89f,.57f,.16f));
            }
            else
            {
                SoftRect(parent,"Level 0 · ballast scorch",new Vector2(lx+.9f,7.42f),new Vector2(.45f,.05f),new Color(.10f,.09f,.07f,.85f),-3);
            }

            // Old pasted-over patches, vertical water marks and misaligned paper.
            if(section%2==0)
            {
                SoftRect(parent,"Level 0 · faded wallpaper repair",new Vector2(x+23f,3.7f),new Vector2(.76f,1.10f),new Color(.60f,.52f,.28f,.50f),-8);
                SoftRect(parent,"Level 0 · wallpaper tear shadow",new Vector2(x+24.2f,2.95f),new Vector2(.10f,.56f),new Color(.24f,.23f,.13f,.72f),-7);
            }
            else
            {
                SoftRect(parent,"Level 0 · old moisture run",new Vector2(x+4.2f,4.3f),new Vector2(.16f,1.7f),new Color(.30f,.31f,.18f,.38f),-8);
                SoftRect(parent,"Level 0 · water tide mark",new Vector2(x+4.35f,3.55f),new Vector2(.31f,.12f),new Color(.45f,.39f,.21f,.52f),-7);
            }

            // Near-wall returns break the long sightline; background art only.
            if(section>0)
            {
                float px=x+.6f;
                Solid(parent,"Level 0 · wallpapered return column",new Vector2(px,3.5f),new Vector2(.34f,6.8f),new Color(.62f,.54f,.32f),-5);
                SoftRect(parent,"Level 0 · column edge shadow",new Vector2(px+.21f,3.5f),new Vector2(.09f,6.8f),new Color(.28f,.26f,.15f,.72f),-4);
                SoftRect(parent,"Level 0 · column highlight",new Vector2(px-.17f,3.5f),new Vector2(.05f,6.8f),new Color(.80f,.71f,.42f,.45f),-4);
            }
        }

        static void BuildFalsePassage(Transform parent,float x,int variant)
        {
            float h=variant==1?4.7f:5.25f,w=variant==2?3.8f:3.05f;
            SoftRect(parent,"Level 0 · false corridor shadow",new Vector2(x,2.25f+h*.5f),new Vector2(w,h),new Color(.17f,.17f,.10f),-9);
            SoftRect(parent,"Level 0 · distant yellow room",new Vector2(x,2.0f+h*.5f),new Vector2(w-.34f,h-.5f),new Color(.44f,.40f,.24f),-8);
            // Receding wall corners give the mouth a sense of depth.
            for(int i=0;i<4;i++)
            {
                float inset=.22f+i*.30f;
                SoftRect(parent,"Level 0 · passage inner corner",new Vector2(x-w*.5f+inset,2.0f+h*.5f),new Vector2(.06f,h-inset*1.5f),new Color(.58f,.50f,.28f,.55f),-7+i);
                SoftRect(parent,"Level 0 · passage inner corner",new Vector2(x+w*.5f-inset,2.0f+h*.5f),new Vector2(.06f,h-inset*1.5f),new Color(.50f,.43f,.24f,.55f),-7+i);
            }
            Solid(parent,"Level 0 · left wallpaper return",new Vector2(x-w*.5f,2.0f+h*.5f),new Vector2(.15f,h),new Color(.80f,.70f,.41f),-5);
            Solid(parent,"Level 0 · right wallpaper return",new Vector2(x+w*.5f,2.0f+h*.5f),new Vector2(.15f,h),new Color(.69f,.59f,.34f),-5);
            Solid(parent,"Level 0 · passage ceiling shadow",new Vector2(x,2f+h),new Vector2(w+.2f,.16f),new Color(.71f,.62f,.38f),-5);
            SoftRect(parent,"Level 0 · weak false fluorescent glow",new Vector2(x,2.0f+h*.5f+1.4f),new Vector2(.72f,.05f),new Color(1f,.88f,.57f,.34f),-4);
            if(variant==2)AddProp(parent,"Level 0 · dead socket beside passage",PixelArtFactory.WallOutlet(),new Vector2(x+1.9f,1.35f),-4,false);
        }

        static void BuildManilaRoom(Transform parent,float x)
        {
            // The rare Manila Room provides a diegetic solo meeting/trading anomaly.
            Tiled(parent,"Level 0 · rare Manila Room wallpaper",PixelArtFactory.ManilaWallTile(),new Vector2(x,3.9f),new Vector2(10f,7.8f),-17);
            SoftRect(parent,"Manila Room · floor spill",new Vector2(x,.06f),new Vector2(9.4f,.09f),new Color(.52f,.40f,.20f,.45f),-8);
            Solid(parent,"Manila Room · left wood jamb",new Vector2(x-5f,3.65f),new Vector2(.23f,7.25f),new Color(.31f,.21f,.10f),-4);
            Solid(parent,"Manila Room · right wood jamb",new Vector2(x+5f,3.65f),new Vector2(.23f,7.25f),new Color(.31f,.21f,.10f),-4);
            SoftRect(parent,"Manila Room · warm ceiling fixture",new Vector2(x,7.4f),new Vector2(2.25f,.10f),new Color(.94f,.67f,.32f),-2);
            AddLightFlicker(parent,new Vector2(x,7.20f),new Color(1f,.66f,.31f,.16f));
            SoftRect(parent,"Manila Room · old table shadow",new Vector2(x,1.05f),new Vector2(3.0f,.15f),new Color(.22f,.15f,.08f),-3);
            Solid(parent,"Manila Room · wooden counter",new Vector2(x,1.18f),new Vector2(2.9f,.16f),new Color(.49f,.34f,.16f),-2);
            SoftRect(parent,"Manila Room · counter highlight",new Vector2(x,1.27f),new Vector2(2.76f,.03f),new Color(.75f,.53f,.25f),-1);
            Solid(parent,"Manila Room · left table leg",new Vector2(x-1.08f,.72f),new Vector2(.12f,.75f),new Color(.34f,.23f,.11f),-2);
            Solid(parent,"Manila Room · right table leg",new Vector2(x+1.08f,.72f),new Vector2(.12f,.75f),new Color(.34f,.23f,.11f),-2);
            SoftRect(parent,"Manila Room · paper on counter",new Vector2(x+.66f,1.39f),new Vector2(.37f,.03f),new Color(.80f,.74f,.54f),0);
        }

        // ------------------------------------------------------------------
        // Level 1 - industrial
        // ------------------------------------------------------------------
        static void BuildIndustrialLevel(Transform parent,float start,int level)
        {
            const float length=Level1End-Level1Start;
            float levelCenter=start+length*.5f;
            // Keep the 4-unit materials continuous; smaller sections only carry props.
            Tiled(parent,"Level 1 · concrete service wall",PixelArtFactory.WallTile(3),new Vector2(levelCenter,3.9f),new Vector2(length,7.8f),-20);
            Tiled(parent,"Level 1 · stained concrete floor",PixelArtFactory.FloorTile(3),new Vector2(levelCenter,-.25f),new Vector2(length,.5f),-10);
            Tiled(parent,"Level 1 · exposed ceiling",PixelArtFactory.CeilingTile(3),new Vector2(levelCenter,7.85f),new Vector2(length,.33f),-15);
            for(int section=0;section<4;section++)
            {
                float x=start+section*SegmentLength;
                BuildIndustrialDressing(parent,x,section,level);
            }
            var floor=new GameObject("Level 1 · continuous walkable floor");
            floor.transform.SetParent(parent,false);
            floor.transform.position=new Vector3(start+length*.5f,-.26f,0);
            floor.AddComponent<BoxCollider2D>().size=new Vector2(length,FloorHeight);
        }

        static void BuildIndustrialDressing(Transform parent,float x,int section,int level)
        {
            for(int i=0;i<5;i++)
            {
                float px=x+2f+i*5.5f;
                AddProp(parent,"Level 1 · exposed service pipe",PixelArtFactory.ServicePipe(),new Vector2(px+2.3f,7.05f),-6,false);
                if(i%2==0)
                {
                    AddProp(parent,"Level 1 · fluorescent fixture",PixelArtFactory.CeilingLamp(),new Vector2(px+2.2f,7.78f),-3,false);
                    SoftRect(parent,"Level 1 · lamp spill",new Vector2(px+2.2f,7.40f),new Vector2(1.8f,.06f),new Color(.94f,.86f,.60f,.30f),-2);
                    AddLightFlicker(parent,new Vector2(px+2.2f,7.34f),new Color(.92f,.84f,.57f,.18f));
                }
            }
            SoftRect(parent,"Level 1 · heavy wall base rail",new Vector2(x+15f,.53f),new Vector2(29f,.12f),new Color(.17f,.21f,.16f),-4);
            SoftRect(parent,"Level 1 · base rail highlight",new Vector2(x+15f,.60f),new Vector2(29f,.02f),new Color(.42f,.46f,.36f,.5f),-3);
            if(section==1||section==3)BuildIndustrialRecess(parent,x+18f);
            if(section==2)BuildUtilityCabinet(parent,x+23f);
            if(section%2==0)AddProp(parent,"Level 1 · warning placard",PixelArtFactory.WarningPlacard(3),new Vector2(x+23f,5.55f),-4,false);
            if(section%2==1)AddProp(parent,"Level 1 · rusted service door",PixelArtFactory.BunkerDoor(),new Vector2(x+8.5f,1.15f),-6,false);
        }

        static void BuildIndustrialRecess(Transform parent,float x)
        {
            SoftRect(parent,"Level 1 · service recess shadow",new Vector2(x,2.05f),new Vector2(2.15f,3.55f),new Color(.03f,.04f,.035f),-7);
            Solid(parent,"Level 1 · recess left jamb",new Vector2(x-1.15f,2.08f),new Vector2(.16f,3.7f),new Color(.25f,.30f,.23f),-5);
            Solid(parent,"Level 1 · recess right jamb",new Vector2(x+1.15f,2.08f),new Vector2(.16f,3.7f),new Color(.24f,.27f,.21f),-5);
            Solid(parent,"Level 1 · recess header",new Vector2(x,3.94f),new Vector2(2.4f,.18f),new Color(.27f,.30f,.23f),-5);
        }

        static void BuildUtilityCabinet(Transform parent,float x)
        {
            Solid(parent,"Level 1 · service panel",new Vector2(x,3.4f),new Vector2(.82f,2.35f),new Color(.09f,.13f,.12f),-6);
            Solid(parent,"Level 1 · panel rust edge",new Vector2(x,3.4f),new Vector2(.69f,2.16f),new Color(.28f,.18f,.13f),-5);
            Solid(parent,"Level 1 · panel inset",new Vector2(x,3.4f),new Vector2(.52f,1.94f),new Color(.06f,.10f,.09f),-4);
            Solid(parent,"Level 1 · panel green lamp",new Vector2(x+.10f,3.78f),new Vector2(.09f,.09f),new Color(.44f,.63f,.31f),-3);
            Solid(parent,"Level 1 · panel amber lamp",new Vector2(x+.10f,3.52f),new Vector2(.09f,.09f),new Color(.78f,.48f,.20f),-3);
        }

        // ------------------------------------------------------------------
        // helpers
        // ------------------------------------------------------------------
        static void BuildBoundary(Transform parent,float x,string name)
        {
            var go=new GameObject(name+" · collision boundary");go.transform.SetParent(parent,false);go.transform.position=new Vector3(x+.35f,3.4f,0);
            go.AddComponent<BoxCollider2D>().size=new Vector2(.45f,7.2f);
        }

        static SpriteRenderer AddProp(Transform parent,string name,Sprite sprite,Vector2 position,int order,bool flip)
        {
            if(sprite==null)return null;
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=new Vector3(position.x,position.y,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;renderer.flipX=flip;
            return renderer;
        }

        static void Tiled(Transform parent,string name,Sprite sprite,Vector2 position,Vector2 size,int order)
        {
            if(sprite==null)return;
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=new Vector3(position.x,position.y,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.drawMode=SpriteDrawMode.Tiled;renderer.size=size;renderer.sortingOrder=order;renderer.tileMode=SpriteTileMode.Continuous;
        }

        static SpriteRenderer Solid(Transform parent,string name,Vector2 center,Vector2 size,Color color,int order)
        {
            return SoftRect(parent,name,center,size,color,order);
        }

        static SpriteRenderer SoftRect(Transform parent,string name,Vector2 center,Vector2 size,Color color,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=new Vector3(center.x,center.y,0);go.transform.localScale=new Vector3(size.x,size.y,1);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Block(Color.white);renderer.color=color;renderer.sortingOrder=order;
            return renderer;
        }

        static void AddLightFlicker(Transform parent,Vector2 position,Color color)
        {
            var go=new GameObject("Fluorescent light · unstable buzz");go.transform.SetParent(parent,false);go.transform.position=new Vector3(position.x,position.y,0);go.transform.localScale=new Vector3(2.15f,.32f,1f);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=PixelArtFactory.Block(Color.white);renderer.color=new Color(color.r,color.g,color.b,color.a*.55f);renderer.sortingOrder=-1;var pulse=go.AddComponent<PixelLightFlicker>();pulse.SetIntensityScale(.78f);
        }
    }
}
