using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Creates small, consistent survival-item pictograms without external asset dependencies.</summary>
    public static class ItemIconFactory
    {
        static readonly Dictionary<ItemId,Texture2D> cache=new Dictionary<ItemId,Texture2D>();
        const int S=64;
        static Color32[] pixels;
        public static Texture2D Get(ItemId id)
        {
            if(cache.TryGetValue(id,out var texture))return texture;
            pixels=new Color32[S*S];Color baseColor=new Color(.075f,.095f,.078f);
            for(int y=0;y<S;y++)for(int x=0;x<S;x++)
            {float shade=.78f+.22f*y/S;pixels[y*S+x]=(Color32)(baseColor*shade);}
            Color32 border=new Color32(105,107,79,255);DrawRect(2,2,60,60,border);DrawRect(3,3,58,58,new Color32(23,29,24,255));
            DrawRect(6,55,52,1,new Color32(168,149,94,255));
            Color32 main=(Color32)ItemCatalog.Get(id).iconColor;Color32 shade=Dark(main,.62f),light=Light(main,1.24f);int cx=32;
            switch(id)
            {
                case ItemId.Water:
                    DrawRect(26,13,12,35,shade);DrawRect(23,12,18,30,main);DrawRect(27,8,10,8,light);DrawRect(28,5,8,4,new Color32(157,147,98,255));DrawRect(27,22,10,13,new Color32(131,159,143,255));DrawRect(30,15,2,6,new Color32(205,206,165,255));break;
                case ItemId.CannedFood:case ItemId.Medkit:
                    DrawRect(17,15,31,35,shade);DrawRect(19,14,27,34,main);DrawRect(17,47,31,4,light);DrawRect(19,12,27,4,light);
                    DrawRect(24,25,17,id==ItemId.Medkit?16:12,new Color32(51,54,44,255));
                    if(id==ItemId.Medkit){DrawRect(29,25,6,16,new Color32(215,207,166,255));DrawRect(24,30,16,6,new Color32(215,207,166,255));}
                    else DrawRect(28,25,7,12,new Color32(189,151,83,255));break;
                case ItemId.Pistol:
                    DrawRect(12,23,37,9,shade);DrawRect(16,20,33,8,main);DrawRect(17,18,27,4,light);DrawRect(27,27,10,7,main);DrawPoly(new[]{new Vector2Int(29,29),new Vector2Int(40,29),new Vector2Int(37,47),new Vector2Int(29,46)},shade);DrawRect(44,21,6,5,light);break;
                case ItemId.Rifle:
                    DrawRect(9,27,46,6,shade);DrawRect(12,25,42,4,light);DrawRect(8,25,9,8,main);DrawRect(22,22,9,6,main);DrawPoly(new[]{new Vector2Int(25,30),new Vector2Int(34,30),new Vector2Int(31,43),new Vector2Int(23,43)},shade);DrawRect(45,24,9,4,main);break;
                case ItemId.PistolAmmo:case ItemId.RifleAmmo:
                    for(int i=0;i<4;i++){int x=16+i*8;DrawRect(x,20,6,26,shade);DrawRect(x+1,18,4,25,main);DrawPoly(new[]{new Vector2Int(x+1,18),new Vector2Int(x+3,13),new Vector2Int(x+5,18)},light);}break;
                case ItemId.Helmet:
                    DrawPoly(new[]{new Vector2Int(14,37),new Vector2Int(17,24),new Vector2Int(24,17),new Vector2Int(40,17),new Vector2Int(48,25),new Vector2Int(50,38)},shade);
                    DrawPoly(new[]{new Vector2Int(13,36),new Vector2Int(18,25),new Vector2Int(25,19),new Vector2Int(39,19),new Vector2Int(46,26),new Vector2Int(49,36)},main);DrawRect(11,36,42,5,light);break;
                case ItemId.ArmorVest:case ItemId.HazmatSuit:
                    DrawPoly(new[]{new Vector2Int(18,14),new Vector2Int(27,18),new Vector2Int(37,18),new Vector2Int(46,14),new Vector2Int(50,46),new Vector2Int(14,46)},shade);
                    DrawPoly(new[]{new Vector2Int(20,16),new Vector2Int(28,20),new Vector2Int(36,20),new Vector2Int(44,16),new Vector2Int(47,43),new Vector2Int(17,43)},main);
                    DrawRect(29,20,6,23,light);DrawRect(20,29,24,3,shade);break;
                case ItemId.Cloth:case ItemId.Bandage:
                    DrawPoly(new[]{new Vector2Int(15,21),new Vector2Int(21,15),new Vector2Int(48,26),new Vector2Int(44,36),new Vector2Int(18,43)},shade);
                    DrawPoly(new[]{new Vector2Int(16,20),new Vector2Int(22,16),new Vector2Int(46,26),new Vector2Int(43,34),new Vector2Int(18,40)},main);
                    DrawRect(27,19,5,21,light);DrawRect(38,24,4,14,light);break;
                case ItemId.FieldJacket:case ItemId.FieldPants:case ItemId.Backpack:
                    if(id==ItemId.FieldPants){DrawRect(19,14,26,12,main);DrawPoly(new[]{new Vector2Int(19,23),new Vector2Int(30,23),new Vector2Int(28,49),new Vector2Int(19,49)},shade);DrawPoly(new[]{new Vector2Int(31,23),new Vector2Int(44,23),new Vector2Int(45,49),new Vector2Int(35,49)},main);}
                    else{DrawRect(20,20,24,28,main);DrawRect(13,20,9,22,shade);DrawRect(42,20,9,22,shade);DrawRect(26,16,12,9,light);DrawRect(30,22,3,21,shade);DrawRect(44,25,12,26,shade);DrawRect(46,27,8,19,main);}
                    break;
                case ItemId.Scrap:case ItemId.MetalFragments:case ItemId.CircuitBoard:
                    DrawPoly(new[]{new Vector2Int(14,18),new Vector2Int(28,13),new Vector2Int(36,22),new Vector2Int(49,18),new Vector2Int(47,43),new Vector2Int(34,50),new Vector2Int(17,43)},shade);
                    DrawPoly(new[]{new Vector2Int(16,19),new Vector2Int(28,16),new Vector2Int(34,24),new Vector2Int(47,20),new Vector2Int(44,41),new Vector2Int(33,46),new Vector2Int(19,40)},main);DrawRect(24,27,20,3,light);break;
                case ItemId.Blueprint:case ItemId.WorkbenchI:case ItemId.WorkbenchII:case ItemId.Keycard:
                    DrawRect(17,13,31,38,shade);DrawRect(19,12,27,37,main);DrawRect(23,19,17,2,light);DrawRect(23,25,17,2,light);DrawRect(23,31,12,2,light);DrawRect(23,39,10,6,shade);break;
                case ItemId.Pipe:
                    DrawRect(19,12,8,38,shade);DrawRect(21,12,4,36,light);DrawRect(36,16,8,32,shade);DrawRect(38,16,4,30,main);DrawRect(21,12,23,5,main);break;
                case ItemId.Flashlight:
                    DrawPoly(new[]{new Vector2Int(18,16),new Vector2Int(46,19),new Vector2Int(43,45),new Vector2Int(20,43)},shade);DrawPoly(new[]{new Vector2Int(21,18),new Vector2Int(44,20),new Vector2Int(41,42),new Vector2Int(22,41)},main);DrawRect(19,13,26,8,light);DrawRect(26,27,10,10,shade);break;
                default:
                    DrawRect(18,18,28,30,shade);DrawRect(21,15,22,30,main);DrawRect(24,23,16,3,light);break;
            }
            texture=new Texture2D(S,S,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};texture.SetPixels32(pixels);texture.Apply(false,true);cache[id]=texture;return texture;
        }
        static void DrawRect(int x,int y,int w,int h,Color32 c)
        {for(int yy=Mathf.Max(0,y);yy<Mathf.Min(S,y+h);yy++)for(int xx=Mathf.Max(0,x);xx<Mathf.Min(S,x+w);xx++)pixels[yy*S+xx]=c;}
        static void DrawPoly(Vector2Int[] points,Color32 c)
        {
            int minY=S,maxY=0,minX=S,maxX=0;foreach(var p in points){minY=Mathf.Min(minY,p.y);maxY=Mathf.Max(maxY,p.y);minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);}
            for(int y=Mathf.Max(0,minY);y<Mathf.Min(S,maxY+1);y++)for(int x=Mathf.Max(0,minX);x<Mathf.Min(S,maxX+1);x++){bool inside=false;for(int i=0,j=points.Length-1;i<points.Length;j=i++){var a=points[i];var b=points[j];if(((a.y>y)!=(b.y>y))&&(x<(b.x-a.x)*(float)(y-a.y)/(b.y-a.y)+a.x))inside=!inside;}if(inside)pixels[y*S+x]=c;}
        }
        static Color32 Dark(Color32 c,float f)=>new Color32((byte)(c.r*f),(byte)(c.g*f),(byte)(c.b*f),c.a);
        static Color32 Light(Color32 c,float f)=>new Color32((byte)Mathf.Min(255,c.r*f),(byte)Mathf.Min(255,c.g*f),(byte)Mathf.Min(255,c.b*f),c.a);
    }
}
