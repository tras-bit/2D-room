using System;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Small, crisp runtime pixel-art kit; no external art packs or network assets required.</summary>
    public static class PixelArtFactory
    {
        static Color32 C(byte r, byte g, byte b, byte a = 255) => new Color32(r, g, b, a);
        static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        static Sprite Build(string name, int width, int height, Action<Color32[]> paint, Vector2 pivot, float pixelsPerUnit = 16f)
        {
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Clear;
            paint(pixels);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = name + "_PixelTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, width, height), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
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
            return Build("Pixel_Block", 4, 4, p => Rect(p, 4, 4, 0, 0, 4, 4, color), new Vector2(.5f, .5f), 4f);
        }

        public static Sprite Survivor(int frame = 0)
        {
            int leftStep = frame == 1 ? 2 : frame == 3 ? 8 : 5;
            int rightStep = frame == 1 ? 20 : frame == 3 ? 15 : 18;
            int armSwing = frame == 1 ? 2 : frame == 3 ? -2 : 0;
            return Build("Survivor_Frame_" + frame, 32, 48, p =>
            {
                Rect(p,32,48,leftStep,0,10,4,C(63,61,48)); Rect(p,32,48,rightStep,0,9,4,C(54,58,46));
                Rect(p,32,48,6,4,8,10,C(76,79,60)); Rect(p,32,48,18,4,8,10,C(71,77,59));
                Rect(p,32,48,5,12,22,17,C(82,91,69)); Rect(p,32,48,4,15+armSwing,4,11,C(69,77,62));
                Rect(p,32,48,23,16-armSwing,4,10,C(71,78,62)); Rect(p,32,48,11,14,4,14,C(153,119,72));
                Rect(p,32,48,7,21,17,3,C(113,113,83)); Rect(p,32,48,8,27,6,4,C(171,136,91));
                Rect(p,32,48,7,29,15,8,C(79,83,66)); Rect(p,32,48,9,37,12,5,C(181,145,111));
                Rect(p,32,48,8,41,14,4,C(71,70,55)); Rect(p,32,48,10,39,2,2,C(36,41,35));
                Rect(p,32,48,18,39,2,2,C(36,41,35)); Rect(p,32,48,10,35,11,3,C(112,113,88));
                Rect(p,32,48,24,18,7,17,C(61,69,54)); Rect(p,32,48,27,12,4,6,C(124,105,66));
                Rect(p,32,48,2,17,3,3,C(196,158,113)); Rect(p,32,48,25,17,3,3,C(196,158,113));
                Rect(p,32,48,11,5,3,3,C(166,132,92)); Rect(p,32,48,20,5,3,3,C(161,128,88));
                if(frame==2)Rect(p,32,48,15,26,2,2,C(197,168,103));
            }, new Vector2(.5f, 0f), 32f);
        }

        public static Sprite Watcher(int frame = 0)
        {
            int lurch = frame % 2 == 0 ? 0 : 2;
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

        public static Sprite FlashlightBeam()
        {
            const int width=128,height=56;
            return Build("Flashlight_Beam",width,height,p=>
            {
                for(int x=0;x<width;x++)
                {
                    float t=x/(float)(width-1);
                    int halfHeight=Mathf.Max(1,Mathf.RoundToInt((1f-t)*height*.44f));
                    byte alpha=(byte)Mathf.Clamp(Mathf.RoundToInt(30*(1f-t)*(1f-t)),0,30);
                    for(int y=height/2-halfHeight;y<=height/2+halfHeight;y++)
                        if(y>=0&&y<height)p[y*width+x]=new Color32(210,216,151,alpha);
                }
            },new Vector2(.04f,.5f),32f);
        }

        public static Sprite Crate()
        {
            return Build("Weathered_Crate",48,48,p=>{
                Rect(p,48,48,4,2,40,40,C(94,81,54));Rect(p,48,48,8,6,32,31,C(123,101,62));
                Rect(p,48,48,8,33,32,4,C(75,69,50));Rect(p,48,48,8,17,32,4,C(160,132,78));
                Rect(p,48,48,11,7,4,27,C(72,68,51));Rect(p,48,48,33,7,4,27,C(76,69,49));
                Rect(p,48,48,6,4,5,5,C(171,157,106));Rect(p,48,48,36,4,5,5,C(165,145,98));
            },new Vector2(.5f,0),20f);
        }
    }
}
