using System;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Small, crisp runtime pixel-art kit; no external art packs or network assets required.</summary>
    public static class PixelArtFactory
    {
        static Color32 C(byte r, byte g, byte b, byte a = 255) => new Color32(r, g, b, a);
        static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        static readonly System.Collections.Generic.Dictionary<string,Sprite> cache=new System.Collections.Generic.Dictionary<string,Sprite>();
        static readonly System.Collections.Generic.Dictionary<Texture2D,Texture2D> generatedNormals=new System.Collections.Generic.Dictionary<Texture2D,Texture2D>();

        static Sprite AtlasSlice(string name,int x,int top,int width,int height,float pixelsPerUnit)
        {
            string key="piskel_"+name;
            if(cache.TryGetValue(key,out var cached))return cached;
            Texture2D atlas=Resources.Load<Texture2D>("PixelArt/individual_models");
            if(atlas==null)return null;
            atlas.filterMode=FilterMode.Point;atlas.wrapMode=TextureWrapMode.Clamp;
            const int detailScale=2;
            int cropWidth=width*detailScale,cropHeight=height*detailScale;
            var rect=new Rect(x*detailScale,atlas.height-(top+height)*detailScale,cropWidth,cropHeight);
            Texture2D slice=CropTexture(atlas,rect);
            Texture2D normal=BuildNormalMap(slice);
            // Release CPU readability on the slice after normal extraction to save GPU memory.
            if(slice!=null){try{slice.Apply(false,true);}catch{}}
            SecondarySpriteTexture[] secondary=normal!=null?new[]{new SecondarySpriteTexture{name="_NormalMap",texture=normal}}:Array.Empty<SecondarySpriteTexture>();
            var sprite=Sprite.Create(slice,new Rect(0,0,slice.width,slice.height),new Vector2(.5f,0f),pixelsPerUnit*detailScale,0,SpriteMeshType.FullRect,Vector4.zero,false,secondary);
            sprite.name=name;cache[key]=sprite;return sprite;
        }

        static Sprite Build(string name, int width, int height, Action<Color32[]> paint, Vector2 pivot, float pixelsPerUnit = 16f,int detailScale=1)
        {
            detailScale=Mathf.Max(1,detailScale);
            int paintWidth=Mathf.Max(1,width/detailScale),paintHeight=Mathf.Max(1,height/detailScale);
            var painted=new Color32[paintWidth*paintHeight];
            for(int i=0;i<painted.Length;i++)painted[i]=Clear;
            paint(painted);
            Color32[] pixels=painted;
            if(detailScale>1)
            {
                pixels=new Color32[width*height];
                for(int y=0;y<paintHeight;y++)for(int x=0;x<paintWidth;x++)
                {
                    Color32 color=painted[y*paintWidth+x];
                    for(int sy=0;sy<detailScale;sy++)for(int sx=0;sx<detailScale;sx++)
                        pixels[(y*detailScale+sy)*width+x*detailScale+sx]=color;
                    int hash=(x*37+y*61+name.Length*13)&15;
                    if(color.a>0&&y>=6&&y<=34&&x>=7&&x<=25&&hash<3)
                    {
                        int dx=x*detailScale+(hash&1),dy=y*detailScale+((hash>>1)&1);
                        Color32 glint=new Color32((byte)Mathf.Min(255,color.r+10),(byte)Mathf.Min(255,color.g+9),(byte)Mathf.Min(255,color.b+6),color.a);
                        pixels[dy*width+dx]=glint;
                    }
                }
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = name + "_PixelTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            // Build the normal map directly from the CPU pixel buffer BEFORE marking the texture
            // non-readable; otherwise the subsequent GetPixels() call throws.
            Texture2D normal=BuildNormalMapFromPixels(pixels,width,height);
            texture.Apply(false, false);
            SecondarySpriteTexture[] secondary=normal!=null?new[]{new SecondarySpriteTexture{name="_NormalMap",texture=normal}}:Array.Empty<SecondarySpriteTexture>();
            return Sprite.Create(texture, new Rect(0, 0, width, height), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect,Vector4.zero,false,secondary);
        }

        static Texture2D CropTexture(Texture2D source,Rect rect)
        {
            int rw=Mathf.RoundToInt(rect.width),rh=Mathf.RoundToInt(rect.height);
            RenderTexture rt=RenderTexture.GetTemporary(rw,rh,0,RenderTextureFormat.Default,RenderTextureReadWrite.Default);
            Graphics.Blit(source,rt,new Vector2(rect.width/source.width,rect.height/source.height),new Vector2(rect.x/source.width,rect.y/source.height));
            RenderTexture prev=RenderTexture.active;RenderTexture.active=rt;
            // Create as CPU-readable (linear=false mipChain=false) so BuildNormalMap can read pixels.
            Texture2D copy=new Texture2D(rw,rh,TextureFormat.RGBA32,false,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,name=source.name+"_crop"};
            copy.ReadPixels(new Rect(0,0,rw,rh),0,0);copy.Apply(false,false);
            RenderTexture.active=prev;RenderTexture.ReleaseTemporary(rt);return copy;
        }

        static Texture2D BuildNormalMapFromPixels(Color32[] pixels,int w,int h)
        {
            try
            {
                Color[] src=new Color[pixels.Length];
                for(int i=0;i<pixels.Length;i++){Color c=pixels[i];src[i]=new Color(c.r/255f,c.g/255f,c.b/255f,c.a/255f);}
                Color[] dst=new Color[src.Length];const float bump=2.2f;
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    int xm=x>0?x-1:w-1,xp=x<w-1?x+1:0,ym=y>0?y-1:h-1,yp=y<h-1?y+1:0;
                    float l=src[y*w+xm].grayscale*src[y*w+xm].a,r=src[y*w+xp].grayscale*src[y*w+xp].a,d=src[ym*w+x].grayscale*src[ym*w+x].a,u=src[yp*w+x].grayscale*src[yp*w+x].a;
                    Vector3 n=Vector3.Normalize(new Vector3((r-l)*bump,(u-d)*bump,1f));
                    dst[y*w+x]=new Color(n.x*.5f+.5f,n.y*.5f+.5f,n.z,src[y*w+x].a);
                }
                Texture2D normal=new Texture2D(w,h,TextureFormat.RGBA32,false,true){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                normal.SetPixels(dst);normal.Apply(false,true);return normal;
            }
            catch{return null;}
        }

        static Texture2D BuildNormalMap(Texture2D source)
        {
            if(source==null)return null;
            if(generatedNormals.TryGetValue(source,out var cached))return cached;
            // Guard against non-readable source textures (common for imported art / post-Apply(true) data).
            // Returning null means the sprite renders without a normal map instead of crashing.
            try
            {
                if(!source.isReadable){generatedNormals[source]=null;return null;}
                int w=source.width,h=source.height;Color[] src=source.GetPixels();Color[] dst=new Color[src.Length];const float bump=2.2f;
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    int xm=x>0?x-1:w-1,xp=x<w-1?x+1:0,ym=y>0?y-1:h-1,yp=y<h-1?y+1:0;
                    float l=src[y*w+xm].grayscale*src[y*w+xm].a,r=src[y*w+xp].grayscale*src[y*w+xp].a,d=src[ym*w+x].grayscale*src[ym*w+x].a,u=src[yp*w+x].grayscale*src[yp*w+x].a;
                    Vector3 n=Vector3.Normalize(new Vector3((r-l)*bump,(u-d)*bump,1f));
                    dst[y*w+x]=new Color(n.x*.5f+.5f,n.y*.5f+.5f,n.z,src[y*w+x].a);
                }
                Texture2D normal=new Texture2D(w,h,TextureFormat.RGBA32,false,true){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,name=source.name+"_Normal"};
                normal.SetPixels(dst);normal.Apply(false,true);generatedNormals[source]=normal;return normal;
            }
            catch(UnityException){generatedNormals[source]=null;return null;}
            catch(System.Exception){generatedNormals[source]=null;return null;}
        }

        static void Rect(Color32[] pixels, int width, int height, int x, int y, int w, int h, Color32 color)
        {
            int minX = Mathf.Clamp(x, 0, width), minY = Mathf.Clamp(y, 0, height);
            int maxX = Mathf.Clamp(x + w, 0, width), maxY = Mathf.Clamp(y + h, 0, height);
            for (int py = minY; py < maxY; py++)
                for (int px = minX; px < maxX; px++) pixels[py * width + px] = color;
        }

        public static Sprite Block(Color color)
        {
            Color32 c=color;string key=$"block_{c.r}_{c.g}_{c.b}_{c.a}";
            if(cache.TryGetValue(key,out var sprite))return sprite;
            sprite=Build(key,4,4,p=>Rect(p,4,4,0,0,4,4,c),new Vector2(.5f,.5f),4f);cache[key]=sprite;return sprite;
        }

        public static Sprite Survivor(int frame=0,ItemStack[] gear=null,bool hostile=false)
        {
            int head=GetGear(gear,GearSlot.Head),chest=GetGear(gear,GearSlot.Chest),legs=GetGear(gear,GearSlot.Legs),feet=GetGear(gear,GearSlot.Feet),back=GetGear(gear,GearSlot.Back);
            string key=$"survivor_{frame}_{head}_{chest}_{legs}_{feet}_{back}_{hostile}";
            if(cache.TryGetValue(key,out var cached))return cached;
            bool jacket=chest==(int)ItemId.FieldJacket,vest=chest==(int)ItemId.ArmorVest,hazmat=chest==(int)ItemId.HazmatSuit;
            bool fieldPants=legs==(int)ItemId.FieldPants,boots=feet==(int)ItemId.WorkBoots,pack=back==(int)ItemId.Backpack,helmet=head==(int)ItemId.Helmet;
            int stride=frame==1?2:frame==3?-2:0,arm=frame==1?-1:frame==3?1:0;
            Color32 outline=hostile?C(31,41,35):C(39,43,36),skin=hostile?C(84,97,76):C(188,143,105);
            Color32 shirt=hazmat?C(196,157,49):jacket?C(91,110,71):C(102,112,100);
            Color32 trousers=hazmat?C(182,144,42):fieldPants?C(108,119,83):C(68,78,69);
            Color32 leather=boots?C(112,78,49):C(45,49,44),hair=hostile?C(48,57,48):C(48,43,36);
            var sprite=Build("Survivor_"+key,64,96,p=>
            {
                // Boots and articulated trouser legs.
                Rect(p,32,48,5+stride,0,11,4,outline);Rect(p,32,48,18-stride,0,10,4,outline);
                Rect(p,32,48,6+stride,1,8,2,leather);Rect(p,32,48,19-stride,1,7,2,leather);
                Rect(p,32,48,7+stride,4,8,13,trousers);Rect(p,32,48,18-stride,4,8,13,trousers);
                Rect(p,32,48,8+stride,6,2,8,hostile?C(56,67,56):C(83,92,69));Rect(p,32,48,18-stride,6,2,8,hostile?C(49,60,51):C(78,87,66));
                Rect(p,32,48,6+stride,2,10,1,C(87,73,54));Rect(p,32,48,19-stride,2,8,1,C(87,73,54));
                Rect(p,32,48,8+stride,3,2,2,C(152,122,76));Rect(p,32,48,21-stride,3,2,2,C(152,122,76));
                if(fieldPants){Rect(p,32,48,8+stride,10,6,3,C(93,97,69));Rect(p,32,48,19-stride,10,6,3,C(93,97,69));Rect(p,32,48,10+stride,10,3,2,C(148,137,88));Rect(p,32,48,20-stride,10,3,2,C(148,137,88));Rect(p,32,48,8+stride,12,6,1,C(54,62,52));Rect(p,32,48,19-stride,12,6,1,C(54,62,52));}
                Rect(p,32,48,6,16,19,3,outline);Rect(p,32,48,8,17,15,2,hostile?C(68,76,58):C(117,105,75));
                Rect(p,32,48,14,17,3,3,C(187,155,91));Rect(p,32,48,15,18,1,1,C(41,46,37));
                // Jacket cut, shoulder seams, cuffs, front pockets and a readable zipper line.
                Rect(p,32,48,6,19,20,15,outline);Rect(p,32,48,7,20,18,13,shirt);
                Rect(p,32,48,3,22+arm,5,10,outline);Rect(p,32,48,4,23+arm,3,8,shirt);
                Rect(p,32,48,24,22-arm,5,10,outline);Rect(p,32,48,25,23-arm,3,8,shirt);
                Rect(p,32,48,4,23+arm,3,1,C(157,148,101));Rect(p,32,48,25,23-arm,3,1,C(157,148,101));
                Rect(p,32,48,4,21+arm,4,2,C(50,59,48));Rect(p,32,48,24,21-arm,4,2,C(50,59,48));
                Rect(p,32,48,3,20+arm,4,3,skin);Rect(p,32,48,25,20-arm,4,3,skin);
                if(jacket){Rect(p,32,48,14,20,2,12,C(177,160,109));Rect(p,32,48,8,24,5,4,C(75,91,61));Rect(p,32,48,19,24,5,4,C(75,91,61));Rect(p,32,48,9,25,3,1,C(158,147,102));Rect(p,32,48,20,25,3,1,C(158,147,102));Rect(p,32,48,11,20,2,3,C(123,137,92));Rect(p,32,48,8,31,4,2,C(52,62,51));Rect(p,32,48,20,31,4,2,C(52,62,51));}
                if(vest){Rect(p,32,48,8,21,17,11,C(58,67,61));Rect(p,32,48,10,22,13,8,C(91,98,82));Rect(p,32,48,15,22,3,8,C(54,62,59));Rect(p,32,48,8,29,17,2,C(133,116,77));Rect(p,32,48,7,20,4,3,C(80,89,72));Rect(p,32,48,22,20,4,3,C(80,89,72));Rect(p,32,48,10,25,4,5,C(112,111,83));Rect(p,32,48,19,25,4,5,C(112,111,83));Rect(p,32,48,11,26,2,2,C(178,146,85));Rect(p,32,48,20,26,2,2,C(178,146,85));}
                if(hazmat){Rect(p,32,48,9,19,14,14,C(192,153,42));Rect(p,32,48,15,20,2,12,C(236,198,78));Rect(p,32,48,4,22+arm,3,9,C(192,153,42));Rect(p,32,48,25,22-arm,3,9,C(192,153,42));Rect(p,32,48,12,27,8,3,C(228,193,78));Rect(p,32,48,10,22,2,8,C(80,115,107));Rect(p,32,48,20,22,2,8,C(80,115,107));}
                // Fine tailoring: shoulder piping, pocket stitching, worn cloth flecks and reinforced hems.
                Rect(p,32,48,8,20,1,8,hostile?C(91,100,73):C(143,140,103));
                Rect(p,32,48,23,20,1,8,hostile?C(84,96,71):C(130,135,105));
                Rect(p,32,48,10,28,4,1,C(158,147,105));Rect(p,32,48,19,28,4,1,C(151,140,101));
                Rect(p,32,48,11,29,1,3,C(71,81,62));Rect(p,32,48,21,29,1,3,C(68,79,61));
                Rect(p,32,48,13,22,1,7,C(128,128,101));Rect(p,32,48,17,22,1,7,C(53,64,53));
                Rect(p,32,48,8+stride,7,6,1,C(136,126,91));Rect(p,32,48,19-stride,7,6,1,C(125,123,91));
                Rect(p,32,48,8+stride,14,5,1,C(48,56,49));Rect(p,32,48,20-stride,14,5,1,C(47,56,49));
                Rect(p,32,48,7+stride,3,6,1,C(191,148,91));Rect(p,32,48,20-stride,3,5,1,C(181,136,84));
                Rect(p,32,48,13,38,1,2,C(215,178,129));Rect(p,32,48,21,39,1,1,hostile?C(227,91,45):C(69,56,43));
                Rect(p,32,48,12,42,5,1,C(71,64,48));Rect(p,32,48,18,42,3,1,C(66,61,48));
                // Neck, face in profile and swept hair.
                Rect(p,32,48,13,32,7,4,skin);Rect(p,32,48,10,35,13,10,outline);Rect(p,32,48,11,36,11,8,skin);
                Rect(p,32,48,10,43,12,4,hair);Rect(p,32,48,9,41,4,5,hair);Rect(p,32,48,19,43,3,3,hair);
                Rect(p,32,48,19,39,2,2,hostile?C(232,83,37):C(46,44,35));Rect(p,32,48,22,38,2,2,skin);
                Rect(p,32,48,9,37,2,3,hostile?C(73,81,64):C(143,105,75));
                if(pack){Rect(p,32,48,2,18,8,17,outline);Rect(p,32,48,3,20,6,13,hostile?C(60,73,60):C(105,94,66));Rect(p,32,48,3,28,6,2,C(153,129,80));Rect(p,32,48,4,23,4,4,C(77,87,64));Rect(p,32,48,5,24,2,2,C(174,147,88));Rect(p,32,48,9,20,2,11,C(151,131,87));Rect(p,32,48,4,18,4,2,C(57,67,54));}
                if(helmet){Rect(p,32,48,8,43,16,3,C(79,87,67));Rect(p,32,48,10,45,12,2,C(127,119,69));Rect(p,32,48,10,46,10,2,C(148,139,83));Rect(p,32,48,19,45,5,2,C(80,89,68));Rect(p,32,48,21,45,3,2,C(230,192,70));}
                if(hazmat){Rect(p,32,48,9,35,15,11,C(191,151,42));Rect(p,32,48,17,37,7,5,C(54,64,57));Rect(p,32,48,21,35,4,3,C(135,143,119));Rect(p,32,48,18,34,6,3,C(121,125,100));}
                if(frame==2){Rect(p,32,48,12,13,6,3,trousers);Rect(p,32,48,18,13,7,3,trousers);}
                if(frame==1||frame==3){Rect(p,32,48,3,20+arm,4,3,skin);Rect(p,32,48,25,20-arm,4,3,skin);}
            },new Vector2(.5f,0f),64f,2);
            cache[key]=sprite;return sprite;
        }
        static int GetGear(ItemStack[] gear,GearSlot slot)=>gear!=null&&gear.Length>(int)slot?(int)gear[(int)slot].id:0;


        public static Sprite WallTile(int theme)
        {
            string key="wall_tile_"+theme;if(cache.TryGetValue(key,out var found))return found;
            if(theme==0)
            {
                var wallpaper=Build(key,32,32,p=>
                {
                    Rect(p,32,32,0,0,32,32,C(190,173,108));
                    Rect(p,32,32,0,0,32,2,C(164,145,82));Rect(p,32,32,0,30,32,2,C(172,153,91));
                    for(int x=3;x<32;x+=8)
                    {
                        Rect(p,32,32,x,0,1,32,C(163,144,82));Rect(p,32,32,x+1,0,1,32,C(207,189,121));
                        for(int y=3;y<32;y+=10)
                        {
                            Rect(p,32,32,x-1,y+2,4,3,C(190,170,101));Rect(p,32,32,x,y,2,3,C(204,184,115));
                            Rect(p,32,32,x-2,y+2,2,1,C(140,124,75));Rect(p,32,32,x+2,y+2,2,1,C(140,124,75));
                            Rect(p,32,32,x-1,y+4,2,2,C(151,135,79));Rect(p,32,32,x+1,y+4,2,2,C(151,135,79));
                            Rect(p,32,32,x,y+2,1,2,C(219,196,125));
                        }
                    }
                    // Uneven age stains and tiny torn-paper marks keep the repeating floral print from looking clean or tiled.
                    Rect(p,32,32,1,9,2,5,C(137,122,72));Rect(p,32,32,2,11,3,1,C(201,178,105));
                    Rect(p,32,32,24,1,3,2,C(139,124,72));Rect(p,32,32,26,2,1,3,C(146,130,75));
                    Rect(p,32,32,14,19,2,1,C(133,120,73));Rect(p,32,32,15,20,1,3,C(134,119,72));
                    Rect(p,32,32,5,28,2,1,C(203,182,112));Rect(p,32,32,6,27,1,2,C(203,182,112));
                },new Vector2(.5f,.5f),16f);
                cache[key]=wallpaper;return wallpaper;
            }
            Color32 paper=theme==1?C(70,80,67):theme==2?C(43,70,66):C(56,65,59);
            Color32 tileLight=theme==0?C(123,119,70):theme==1?C(94,105,83):theme==2?C(65,91,82):C(83,91,78);
            Color32 grout=theme==0?C(39,47,35):C(29,41,37);
            var sprite=Build(key,32,32,p=>
            {
                Rect(p,32,32,0,0,32,32,paper);
                // Hard-edged square ceramic blocks with chipped faces, soot and rust clusters.
                for(int row=0;row<2;row++)for(int col=0;col<2;col++)
                {
                    int x=col*16,y=row*16;Color32 shade=((row+col+theme)%3)==0?paper:tileLight;
                    Rect(p,32,32,x+1,y+1,14,14,shade);Rect(p,32,32,x+2,y+2,11,1,C((byte)Mathf.Min(255,shade.r+12),(byte)Mathf.Min(255,shade.g+10),(byte)Mathf.Min(255,shade.b+7)));
                    Rect(p,32,32,x+1,y+2,1,12,grout);Rect(p,32,32,x+14,y+2,1,13,grout);Rect(p,32,32,x+2,y+14,12,1,grout);
                }
                Rect(p,32,32,0,0,32,1,C(22,31,28));Rect(p,32,32,0,16,32,1,C(22,31,28));Rect(p,32,32,0,31,32,1,C(22,31,28));
                Rect(p,32,32,0,0,1,32,C(22,31,28));Rect(p,32,32,16,0,1,32,C(22,31,28));Rect(p,32,32,31,0,1,32,C(22,31,28));
                Rect(p,32,32,4,6,3,1,C(156,75,43));Rect(p,32,32,6,7,1,3,C(112,55,39));Rect(p,32,32,22,23,4,1,C(153,66,43));
                Rect(p,32,32,25,8,1,4,C(28,37,32));Rect(p,32,32,9,25,3,1,C(20,31,29));Rect(p,32,32,11,11,1,2,C(147,125,77));
                if(theme==2){Rect(p,32,32,18,3,4,2,C(28,53,51));Rect(p,32,32,19,5,2,3,C(62,94,83));Rect(p,32,32,24,22,4,2,C(32,57,54));}
                if(theme==3){Rect(p,32,32,5,0,2,32,C(33,47,45));Rect(p,32,32,25,0,2,32,C(33,47,45));Rect(p,32,32,6,4,1,2,C(160,65,41));}
            },new Vector2(.5f,.5f),16f);
            cache[key]=sprite;return sprite;
        }
        public static Sprite FloorTile(int theme)
        {
            string key="floor_tile_"+theme;if(cache.TryGetValue(key,out var found))return found;
            if(theme==0)
            {
                var carpet=Build(key,32,16,p=>
                {
                    Rect(p,32,16,0,0,32,16,C(141,121,74));
                    Rect(p,32,16,0,0,32,2,C(157,136,84));Rect(p,32,16,0,14,32,2,C(101,84,50));
                    for(int y=2;y<14;y+=3)Rect(p,32,16,0,y,32,1,(y%2==0)?C(151,130,79):C(123,105,67));
                    for(int x=2;x<32;x+=5)
                    {
                        Rect(p,32,16,x,3,1,2,C(148,119,65));Rect(p,32,16,x+1,6,1,1,C(74,65,43));
                        Rect(p,32,16,x+2,9,1,2,C(135,107,59));Rect(p,32,16,x-1,12,2,1,C(86,74,45));
                    }
                    // Dark damp patches, scattered carpet fibers, and dull amber reflections.
                    Rect(p,32,16,3,7,7,3,C(79,72,46));Rect(p,32,16,5,6,4,1,C(85,78,49));
                    Rect(p,32,16,4,8,4,1,C(64,69,49));Rect(p,32,16,7,9,3,1,C(145,119,68));
                    Rect(p,32,16,20,3,5,2,C(89,79,48));Rect(p,32,16,21,5,8,2,C(73,70,49));
                    Rect(p,32,16,23,6,5,1,C(126,115,70));Rect(p,32,16,24,7,3,1,C(55,67,51));
                    Rect(p,32,16,11,12,4,1,C(151,121,66));Rect(p,32,16,15,10,2,1,C(62,67,48));
                    Rect(p,32,16,28,12,3,1,C(136,108,60));
                },new Vector2(.5f,.5f),16f);
                cache[key]=carpet;return carpet;
            }
            Color32 baseColor=theme==1?C(57,64,55):theme==2?C(34,55,53):C(42,51,49);
            var sprite=Build(key,32,16,p=>
            {
                Rect(p,32,16,0,0,32,16,baseColor);
                for(int y=1;y<16;y+=4)Rect(p,32,16,0,y,32,1,theme==0?C(92,74,44):C(67,75,63));
                for(int x=3;x<32;x+=9){Rect(p,32,16,x,0,1,16,C(25,34,31));Rect(p,32,16,x+1,2,1,11,theme==2?C(48,78,71):C(75,70,51));}
                Rect(p,32,16,5,3,3,1,C(135,92,53));Rect(p,32,16,22,11,5,1,C(24,35,32));
                Rect(p,32,16,8,7,2,1,C(106,79,47));Rect(p,32,16,26,4,3,1,C(31,44,41));
                if(theme==2){Rect(p,32,16,11,4,8,2,C(49,82,75));Rect(p,32,16,18,7,6,1,C(70,105,91));Rect(p,32,16,4,13,12,1,C(29,69,65));}
                if(theme==3){Rect(p,32,16,0,14,32,2,C(68,79,69));Rect(p,32,16,15,0,2,16,C(32,43,40));Rect(p,32,16,17,1,1,13,C(112,55,39));}
            },new Vector2(.5f,.5f),16f);
            cache[key]=sprite;return sprite;
        }
        public static Sprite CeilingTile(int theme=3)
        {
            string key="ceiling_tile_"+theme;if(cache.TryGetValue(key,out var found))return found;
            var sprite=Build(key,32,16,p=>
            {
                if(theme==0)
                {
                    Rect(p,32,16,0,0,32,16,C(177,169,131));Rect(p,32,16,0,0,32,1,C(103,98,77));Rect(p,32,16,0,15,32,1,C(110,104,80));
                    Rect(p,32,16,0,0,1,16,C(112,105,80));Rect(p,32,16,31,0,1,16,C(98,94,74));
                    Rect(p,32,16,15,0,2,16,C(129,121,91));Rect(p,32,16,1,7,30,2,C(146,138,106));
                    Rect(p,32,16,3,3,8,1,C(197,188,150));Rect(p,32,16,20,11,8,1,C(197,188,150));
                    Rect(p,32,16,6,4,1,2,C(111,107,84));Rect(p,32,16,24,10,1,2,C(116,109,84));
                    Rect(p,32,16,2,12,4,2,C(149,140,105));Rect(p,32,16,25,2,5,2,C(139,131,98));
                }
                else
                {
                    Rect(p,32,16,0,0,32,16,C(40,52,48));Rect(p,32,16,0,0,32,1,C(17,26,24));Rect(p,32,16,0,15,32,1,C(17,26,24));
                    Rect(p,32,16,0,0,1,16,C(17,26,24));Rect(p,32,16,31,0,1,16,C(17,26,24));
                    Rect(p,32,16,14,0,2,16,C(25,38,35));Rect(p,32,16,3,5,4,2,C(67,78,64));Rect(p,32,16,5,6,1,1,C(138,75,44));
                    Rect(p,32,16,21,3,7,1,C(48,62,54));Rect(p,32,16,23,10,4,2,C(31,42,38));
                }
            },new Vector2(.5f,.5f),16f);
            cache[key]=sprite;return sprite;
        }

        public static Sprite ManilaWallTile()
        {
            const string key="manila_wall_tile";if(cache.TryGetValue(key,out var found))return found;
            var sprite=Build(key,32,32,p=>
            {
                Rect(p,32,32,0,0,32,32,C(186,168,121));
                Rect(p,32,32,0,0,32,1,C(146,128,92));Rect(p,32,32,0,31,32,1,C(151,133,96));
                for(int y=4;y<32;y+=8)
                {
                    Rect(p,32,32,3,y,25,1,C(202,184,134));Rect(p,32,32,6,y+2,2,1,C(151,132,93));
                    Rect(p,32,32,11,y+3,1,2,C(165,145,102));Rect(p,32,32,22,y+1,2,2,C(208,189,139));
                    Rect(p,32,32,26,y+3,1,1,C(137,120,86));
                }
                Rect(p,32,32,8,7,3,2,C(203,183,129));Rect(p,32,32,19,24,4,2,C(157,137,99));
                Rect(p,32,32,2,17,2,5,C(168,149,108));Rect(p,32,32,28,11,2,4,C(174,154,111));
            },new Vector2(.5f,.5f),16f);
            cache[key]=sprite;return sprite;
        }

        public static Sprite WallOutlet()
        {
            const string key="level0_wall_outlet";if(cache.TryGetValue(key,out var found))return found;
            var sprite=Build(key,24,16,p=>
            {
                Rect(p,24,16,3,2,18,12,C(113,94,56));Rect(p,24,16,4,1,16,12,C(205,187,137));
                Rect(p,24,16,6,3,12,8,C(185,165,113));Rect(p,24,16,8,5,2,4,C(54,54,43));Rect(p,24,16,14,5,2,4,C(54,54,43));
                Rect(p,24,16,9,5,1,2,C(226,210,165));Rect(p,24,16,15,5,1,2,C(226,210,165));
                Rect(p,24,16,11,10,2,1,C(111,93,56));Rect(p,24,16,5,2,2,1,C(231,214,167));
            },new Vector2(.5f,.5f),64f);
            cache[key]=sprite;return sprite;
        }
        public static Sprite BunkerDoor()
        {
            const string key="bunker_door_pixel";if(cache.TryGetValue(key,out var found))return found;
            var piskelModel=AtlasSlice("security_door",113,8,40,66,20f);
            if(piskelModel!=null){cache[key]=piskelModel;return piskelModel;}
            var sprite=Build(key,64,96,p=>
            {
                Rect(p,64,96,4,0,56,96,C(14,22,20));
                for(int row=0;row<7;row++){int inset=row*3;Rect(p,64,96,4+inset,row*4,56-inset*2,4,row<3?C(116,53,39):C(59,69,58));Rect(p,64,96,7+inset,row*4+1,50-inset*2,1,row<3?C(201,117,57):C(94,106,81));}
                Rect(p,64,96,12,25,40,67,C(23,35,31));Rect(p,64,96,15,28,34,63,C(111,57,39));Rect(p,64,96,18,31,28,60,C(90,50,37));
                Rect(p,64,96,22,38,20,46,C(34,48,40));Rect(p,64,96,25,41,14,39,C(22,36,33));Rect(p,64,96,28,45,8,31,C(14,27,26));
                Rect(p,64,96,20,34,24,3,C(203,131,62));Rect(p,64,96,22,36,20,1,C(239,192,98));
                Rect(p,64,96,17,83,30,5,C(147,66,43));Rect(p,64,96,19,84,26,2,C(209,144,69));
                Rect(p,64,96,49,47,4,15,C(196,132,57));Rect(p,64,96,50,50,2,9,C(234,184,95));Rect(p,64,96,51,53,1,3,C(255,221,133));
                Rect(p,64,96,29,31,6,3,C(212,142,62));Rect(p,64,96,31,32,2,7,C(212,142,62));
                for(int y=31;y<86;y+=12){Rect(p,64,96,15,y,2,2,C(197,151,82));Rect(p,64,96,47,y,2,2,C(197,151,82));}
                Rect(p,64,96,7,91,50,5,C(23,30,27));
            },new Vector2(.5f,0),32f);
            cache[key]=sprite;return sprite;
        }
        public static Sprite TraderSprite()
        {
            var sprite=AtlasSlice("corridor_trader",200,10,32,48,32f);
            return sprite!=null?sprite:Survivor(0);
        }
        public static Sprite ElevatorDoor(bool open)
        {
            var sprite=AtlasSlice(open?"elevator_open":"elevator_closed",open?280:236,8,42,64,28f);
            return sprite!=null?sprite:BunkerDoor();
        }
        public static Sprite GreenAccessCard()
        {
            var sprite=AtlasSlice("green_access_card",333,27,19,27,48f);
            return sprite!=null?sprite:ItemSprite(ItemId.Keycard);
        }
        public static Sprite WarningPlacard(int theme)
        {
            string key="warning_placard_"+theme;if(cache.TryGetValue(key,out var found))return found;
            var sprite=Build(key,48,28,p=>
            {
                Rect(p,48,28,1,1,46,26,C(12,19,18));Rect(p,48,28,3,3,42,22,theme==0?C(107,59,38):C(48,68,59));
                Rect(p,48,28,4,4,40,2,C(181,111,55));Rect(p,48,28,6,9,36,2,C(194,164,104));Rect(p,48,28,6,14,26,2,C(147,144,102));
                Rect(p,48,28,6,19,31,2,C(111,131,99));Rect(p,48,28,4,24,40,1,C(28,40,35));
                Rect(p,48,28,39,8,4,4,theme==3?C(186,52,38):C(222,169,73));Rect(p,48,28,40,9,2,2,C(24,33,29));
                Rect(p,48,28,7,6,9,1,C(228,191,111));Rect(p,48,28,7,7,2,1,C(228,191,111));
            },new Vector2(.5f,.5f),24f);
            cache[key]=sprite;return sprite;
        }
        public static Sprite Crate2D(int tier)
        {
            // Supply containers are battered shipping cartons, not wooden treasure chests.
            string key="cardboard_box_2d_"+tier;if(cache.TryGetValue(key,out var found))return found;
            Color32 face=tier==1?C(157,119,72):tier==2?C(125,119,88):C(106,115,91);
            Color32 side=tier==1?C(111,78,48):tier==2?C(83,81,63):C(68,79,66);
            Color32 edge=tier==1?C(190,149,89):tier==2?C(160,147,101):C(145,153,113);
            Color32 tape=tier==3?C(164,132,67):C(207,178,111);
            var sprite=Build(key,56,48,p=>
            {
                Rect(p,56,48,5,2,47,43,C(30,32,27));
                // Uneven carton silhouette with dark, scuffed lower edge and a shaded right plane.
                Rect(p,56,48,5,8,45,33,C(77,57,39));Rect(p,56,48,7,7,40,33,face);
                Rect(p,56,48,44,10,7,28,side);Rect(p,56,48,8,38,42,3,C(85,67,48));
                Rect(p,56,48,9,8,18,5,edge);Rect(p,56,48,28,8,15,5,face);
                Rect(p,56,48,9,6,17,3,C(174,135,81));Rect(p,56,48,29,6,13,3,C(132,99,63));
                // Folded top flaps and a continuous strip of aged packing tape.
                Rect(p,56,48,23,6,9,34,tape);Rect(p,56,48,25,7,2,30,C(230,199,130));
                Rect(p,56,48,9,13,14,1,C(112,81,52));Rect(p,56,48,32,13,11,1,C(107,78,52));
                Rect(p,56,48,11,15,3,1,C(203,163,101));Rect(p,56,48,37,15,4,1,C(183,144,88));
                // Shipping label with large, legible handling marks rather than tiny fake text.
                Rect(p,56,48,11,19,19,13,C(216,198,158));Rect(p,56,48,12,20,17,1,C(239,226,185));
                Rect(p,56,48,13,22,10,1,C(102,80,54));Rect(p,56,48,13,25,7,1,C(127,99,65));
                Rect(p,56,48,14,28,2,3,C(57,65,53));Rect(p,56,48,17,27,2,4,C(57,65,53));
                Rect(p,56,48,21,26,1,5,C(57,65,53));Rect(p,56,48,24,28,2,3,C(57,65,53));
                if(tier>1){Rect(p,56,48,32,21,13,10,C(177,157,112));Rect(p,56,48,35,23,7,2,C(99,74,49));Rect(p,56,48,37,25,3,4,C(99,74,49));}
                // Damp bloom, crushed corners, staple marks, frayed seams and tape wrinkles.
                Rect(p,56,48,8,34,12,2,C(103,85,59));Rect(p,56,48,10,35,7,1,C(189,145,82));
                Rect(p,56,48,38,34,8,2,C(102,78,53));Rect(p,56,48,47,17,2,9,C(91,67,47));
                Rect(p,56,48,6,10,2,5,C(102,72,47));Rect(p,56,48,8,9,2,2,C(213,174,108));
                Rect(p,56,48,45,37,6,3,C(49,48,37));Rect(p,56,48,8,39,10,2,C(203,158,94));
                Rect(p,56,48,18,40,7,1,C(122,89,56));Rect(p,56,48,30,41,11,1,C(119,86,55));
                Rect(p,56,48,11,17,2,1,C(63,57,43));Rect(p,56,48,39,17,2,1,C(65,58,43));
                Rect(p,56,48,26,12,1,6,C(149,118,76));Rect(p,56,48,27,31,1,6,C(177,142,89));
                Rect(p,56,48,5,41,46,2,C(29,33,29));Rect(p,56,48,8,44,9,1,C(210,169,105));
            },new Vector2(.5f,0),32f);
            cache[key]=sprite;return sprite;
        }
        public static Sprite Workbench2D(int tier)
        {
            string key="bench_2d_"+tier;if(cache.TryGetValue(key,out var found))return found;
            var sprite=Build(key,64,48,p=>
            {
                Color32 wood=tier==1?C(116,83,48):C(72,84,74),metal=C(62,72,68);
                Rect(p,64,48,5,28,54,7,wood);Rect(p,64,48,8,24,48,4,C(62,55,41));
                Rect(p,64,48,10,3,5,22,metal);Rect(p,64,48,49,3,5,22,metal);
                Rect(p,64,48,17,35,11,7,C(92,93,75));Rect(p,64,48,34,35,12,8,metal);Rect(p,64,48,36,37,8,4,tier==3?C(201,103,49):C(143,151,102));
                Rect(p,64,48,8,40,47,3,C(171,143,89));Rect(p,64,48,7,1,49,3,C(48,53,48));
                if(tier>1){Rect(p,64,48,25,16,14,11,C(56,65,59));Rect(p,64,48,28,18,8,7,C(102,118,82));}
            },new Vector2(.5f,0),32f);
            cache[key]=sprite;return sprite;
        }
        public static Sprite ItemSprite(ItemId id)
        {
            string key="item_world_"+id;if(cache.TryGetValue(key,out var found))return found;
            Texture2D tex=ItemIconFactory.Get(id);
            Texture2D normal=BuildNormalMap(tex);
            SecondarySpriteTexture[] secondary=normal!=null?new[]{new SecondarySpriteTexture{name="_NormalMap",texture=normal}}:Array.Empty<SecondarySpriteTexture>();
            var sprite=Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),new Vector2(.5f,0),48f,0,SpriteMeshType.FullRect,Vector4.zero,false,secondary);
            cache[key]=sprite;return sprite;
        }
        public static Sprite MenuCorridor()
        {
            const string key="pixel_menu_corridor";if(cache.TryGetValue(key,out var found))return found;
            const int w=320,h=180;
            var sprite=Build(key,w,h,p=>
            {
                Rect(p,w,h,0,0,w,h,C(26,31,27));
                Rect(p,w,h,0,112,w,68,C(54,61,48));
                Rect(p,w,h,0,0,w,42,C(74,60,36));
                // Perspective corridor: broad near walls narrow to the far doorway.
                for(int y=42;y<132;y+=3)
                {
                    float t=(y-42)/90f;int half=Mathf.RoundToInt(Mathf.Lerp(22,143,t));int mid=160;
                    Color32 wallpaper=(y/3)%2==0?C(108,101,65):C(121,110,68);
                    Rect(p,w,h,0,y,mid-half,3,wallpaper);Rect(p,w,h,mid+half,y,mid-half,3,wallpaper);
                    Rect(p,w,h,mid-half,y,half*2,3,y<66?C(42,46,37):((y/6)%2==0?C(71,60,41):C(79,67,43)));
                    if(y>64&&y%12==4)Rect(p,w,h,mid-half+3,y,Mathf.Max(1,half*2-6),1,C(101,82,50));
                }
                for(int y=0;y<42;y+=4){Rect(p,w,h,0,y,w,2,C(51,42,29));Rect(p,w,h,0,y+2,320,1,C(98,80,45));}
                Rect(p,w,h,127,42,66,92,C(17,22,20));Rect(p,w,h,135,47,50,78,C(7,11,12));
                for(int i=0;i<7;i++){int yy=47+i*12;int half=Mathf.Max(3,26-i*3);Rect(p,w,h,160-half,yy,half*2,3,C(57,62,48));Rect(p,w,h,160-half+4,yy+3,half*2-8,2,C(26,31,28));}
                // Fluorescent panels and amber spill.
                for(int i=0;i<6;i++){int yy=117-i*13;int half=Mathf.Max(8,48-i*6);Rect(p,w,h,160-half,yy,half*2,5,C(188,183,132));Rect(p,w,h,160-half+4,yy+1,half*2-8,2,C(222,216,162));}
                Rect(p,w,h,0,108,67,72,C(15,19,16));Rect(p,w,h,253,110,67,70,C(14,18,15));
                for(int x=30;x<w;x+=38){Rect(p,w,h,x,48,3,74,C(69,70,49));Rect(p,w,h,x+3,50,2,69,C(136,125,75));}
                Rect(p,w,h,0,0,w,2,C(8,11,10));Rect(p,w,h,0,0,3,h,C(8,11,10));Rect(p,w,h,w-3,0,3,h,C(8,11,10));
            },new Vector2(.5f,.5f),1f);
            cache[key]=sprite;return sprite;
        }
        public static Sprite Watcher(int frame = 0)
        {
            int lurch = frame % 2 == 0 ? 0 : 2;
            if(frame%2==0)
            {
                var piskelModel=AtlasSlice("watcher",69,10,36,48,32f);
                if(piskelModel!=null)return piskelModel;
            }
            return Build("Watcher_Frame_" + frame, 36, 48, p =>
            {
                Rect(p,36,48,8+lurch,0,10,4,C(56,64,52)); Rect(p,36,48,21-lurch,0,8,4,C(51,59,48));
                Rect(p,36,48,7,4,12,13,C(81,91,69)); Rect(p,36,48,20,4,10,14,C(72,82,62));
                Rect(p,36,48,6,17,25,15,C(79,88,68)); Rect(p,36,48,10,30,21,10,C(132,137,109));
                Rect(p,36,48,8,37,25,6,C(164,158,126)); Rect(p,36,48,6,42,28,4,C(77,84,65));
                Rect(p,36,48,4,22,5,12,C(100,110,81)); Rect(p,36,48,28,22,5,13,C(98,108,78));
                Rect(p,36,48,12,16,4,5,C(207,171,99)); Rect(p,36,48,22,16,4,5,C(207,171,99));
                Rect(p,36,48,13,8,3,3,C(43,51,42)); Rect(p,36,48,23,8,3,3,C(43,51,42));
                Rect(p,36,48,12,30,4,8,C(205,196,156)); Rect(p,36,48,21,32,4,5,C(49,60,49));
                Rect(p,36,48,5,3,8,3,C(122,121,89)); Rect(p,36,48,24,3,9,3,C(114,117,83));
            }, new Vector2(.5f, 0f), 32f);
        }

        public static Sprite Supply(SupplyPickup.Kind kind)
        {
            return Build("Supply_" + kind, 24, 24, p =>
            {
                Color32 main = kind == SupplyPickup.Kind.Water ? C(92,151,137) : kind == SupplyPickup.Kind.Food ? C(151,113,66) : kind == SupplyPickup.Kind.Cloth ? C(153,158,126) : C(117,128,102);
                Rect(p,24,24,4,2,16,3,C(66,73,60));
                if (kind == SupplyPickup.Kind.Water)
                {
                    Rect(p,24,24,8,5,9,13,main); Rect(p,24,24,10,18,5,3,C(190,177,125));
                    Rect(p,24,24,10,8,3,7,C(145,188,165)); Rect(p,24,24,9,21,7,2,C(168,162,122));
                }
                else if (kind == SupplyPickup.Kind.Food)
                {
                    Rect(p,24,24,5,5,14,13,main); Rect(p,24,24,6,18,12,3,C(196,170,106));
                    Rect(p,24,24,8,9,8,5,C(191,170,105)); Rect(p,24,24,10,10,4,3,C(112,126,87));
                }
                else if (kind == SupplyPickup.Kind.Cloth)
                {
                    Rect(p,24,24,5,5,14,11,main); Rect(p,24,24,8,16,10,3,C(196,181,133));
                    Rect(p,24,24,3,8,4,7,C(111,125,101)); Rect(p,24,24,12,8,3,3,C(198,183,139));
                }
                else
                {
                    Rect(p,24,24,4,8,16,9,main); Rect(p,24,24,6,17,11,3,C(165,154,104));
                    Rect(p,24,24,7,5,10,3,C(171,161,116)); Rect(p,24,24,9,11,4,3,C(62,72,59));
                    Rect(p,24,24,17,9,3,9,C(84,101,80));
                }
                Rect(p,24,24,3,3,3,2,C(189,177,119));
            }, new Vector2(.5f, 0f), 24f);
        }

        public static Sprite Shelter()
        {
            return Build("Maintenance_Shelter", 160, 128, p =>
            {
                Rect(p,160,128,8,0,144,83,C(54,63,54)); Rect(p,160,128,13,5,134,73,C(73,79,62));
                Rect(p,160,128,8,82,144,5,C(139,134,91)); Rect(p,160,128,2,87,155,5,C(46,55,50));
                Rect(p,160,128,18,15,43,47,C(27,37,33)); Rect(p,160,128,22,19,34,38,C(99,117,93));
                Rect(p,160,128,39,19,3,38,C(59,76,63)); Rect(p,160,128,71,14,67,53,C(29,39,35));
                Rect(p,160,128,76,19,57,43,C(69,86,72)); Rect(p,160,128,101,19,3,43,C(39,56,48));
                Rect(p,160,128,70,12,4,59,C(137,122,82)); Rect(p,160,128,72,74,65,5,C(116,122,94));
                Rect(p,160,128,15,0,131,15,C(62,73,63));
                for (int x=0;x<8;x++) Rect(p,160,128,12+x*18,0,11,13,x%3==0?C(132,112,69):C(61,76,68));
                Rect(p,160,128,4,90,151,4,C(108,116,88)); Rect(p,160,128,17,96,7,26,C(70,80,69));
                Rect(p,160,128,137,96,6,26,C(70,80,69)); Rect(p,160,128,37,98,3,10,C(198,177,111));
                Rect(p,160,128,39,107,17,3,C(176,156,101)); Rect(p,160,128,107,105,7,8,C(144,93,69));
                for (int i=0;i<6;i++) Rect(p,160,128,12+i*24,0,14,7,i%2==0?C(103,109,83):C(53,66,59));
            }, new Vector2(.5f, 0f), 20f);
        }

        public static Sprite Bulkhead()
        {
            return Build("Sealed_Bulkhead", 192, 144, p =>
            {
                Rect(p,192,144,4,0,184,120,C(49,58,52));Rect(p,192,144,11,7,170,105,C(75,82,66));
                Rect(p,192,144,20,15,152,95,C(43,54,49));Rect(p,192,144,31,22,130,88,C(28,39,35));
                for(int i=0;i<7;i++){Rect(p,192,144,39+i*17,25,3,77,C(91,104,79));Rect(p,192,144,41+i*17,29,1,68,C(131,133,97));}
                Rect(p,192,144,17,113,156,7,C(131,131,94));Rect(p,192,144,12,5,5,112,C(133,128,91));Rect(p,192,144,175,5,5,112,C(133,128,91));
                Rect(p,192,144,56,120,81,6,C(105,112,84));Rect(p,192,144,86,128,22,12,C(69,81,69));
                Rect(p,192,144,94,137,5,7,C(139,149,99));
                for(int x=28;x<168;x+=27){Rect(p,192,144,x,9,3,3,C(192,174,116));Rect(p,192,144,x,105,3,3,C(192,174,116));}
            }, new Vector2(.5f,0),20f);
        }

        public static Sprite HandFlashlight()
        {
            return Build("Handheld_Flashlight",28,14,p=>
            {
                Rect(p,28,14,4,3,20,8,C(12,19,17));Rect(p,28,14,7,4,15,5,C(65,76,65));
                Rect(p,28,14,2,2,8,10,C(31,42,37));Rect(p,28,14,1,4,8,6,C(106,116,90));
                Rect(p,28,14,0,5,4,4,C(184,163,106));Rect(p,28,14,1,5,2,4,C(237,207,129));
                Rect(p,28,14,9,4,2,5,C(142,136,96));Rect(p,28,14,13,4,1,5,C(39,53,47));
                Rect(p,28,14,18,4,3,5,C(90,99,76));Rect(p,28,14,21,4,3,5,C(44,57,48));
                Rect(p,28,14,9,8,7,5,C(47,56,47));Rect(p,28,14,11,10,3,3,C(30,38,34));
                Rect(p,28,14,9,12,7,1,C(157,125,71));Rect(p,28,14,5,2,4,1,C(194,164,99));
                Rect(p,28,14,24,5,2,2,C(153,68,44));
            },new Vector2(.05f,.5f),40f);
        }

        public static Sprite FlashlightBeam()
        {
            const int width=192,height=88,center=height/2;
            return Build("Flashlight_Beam",width,height,p=>
            {
                for(int x=0;x<width;x++)
                {
                    float t=x/(float)(width-1);
                    int halfHeight=Mathf.Max(2,Mathf.RoundToInt(Mathf.Lerp(3f,35f,t)));
                    for(int y=center-halfHeight;y<=center+halfHeight;y++)
                    {
                        if(y<0||y>=height)continue;
                        float edge=Mathf.Abs(y-center)/(float)halfHeight;
                        float core=1f-edge;
                        float falloff=(.34f+.66f*core*core)*(1f-.62f*t);
                        byte alpha=(byte)Mathf.Clamp(Mathf.RoundToInt(92f*falloff),10,92);
                        Color32 tint=edge<.28f?new Color32(255,245,205,alpha):new Color32(218,210,157,alpha);
                        p[y*width+x]=tint;
                    }
                }
            },new Vector2(.025f,.5f),32f);
        }

        public static Sprite Crate()=>Crate2D(1);
    }
}
