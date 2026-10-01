using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Readable bunker-survival inventory pictograms: bold silhouettes, clear highlights, no asset-pack dependencies.</summary>
    public static class ItemIconFactory
    {
        static readonly Dictionary<ItemId,Texture2D> cache=new Dictionary<ItemId,Texture2D>();
        const int S=64;
        static Color32[] pixels;

        public static Texture2D Get(ItemId id)
        {
            if(cache.TryGetValue(id,out var texture))return texture;
            pixels=new Color32[S*S];
            for(int y=0;y<S;y++)
            for(int x=0;x<S;x++)
            {
                float backgroundShade=.75f+.25f*y/S;
                pixels[y*S+x]=(Color32)new Color(.045f*backgroundShade,.058f*backgroundShade,.05f*backgroundShade,1f);
            }
            DrawRect(1,1,62,62,C(10,15,13));DrawRect(3,3,58,58,C(83,83,66));DrawRect(4,4,56,56,C(20,26,22));
            DrawRect(5,5,54,1,C(146,126,78));DrawRect(5,57,54,1,C(56,65,53));
            DrawRect(5,6,1,50,C(52,61,51));DrawRect(58,6,1,50,C(52,61,51));
            DrawRect(8,8,48,46,C(16,22,20));

            Color32 main=(Color32)ItemCatalog.Get(id).iconColor;
            Color32 shade=Dark(main,.52f),dark=Dark(main,.34f),light=Light(main,1.30f),glint=C(222,204,145);
            switch(id)
            {
                case ItemId.Flashlight:
                    // Heavy metal torch with a lit reflector and a small beam wedge.
                    DrawPoly(new[]{new Vector2Int(20,14),new Vector2Int(47,20),new Vector2Int(45,31),new Vector2Int(19,26)},C(46,56,49));
                    DrawPoly(new[]{new Vector2Int(19,13),new Vector2Int(44,19),new Vector2Int(42,27),new Vector2Int(18,23)},shade);
                    DrawRect(18,14,5,8,light);DrawRect(21,15,19,5,main);DrawRect(25,21,16,2,dark);
                    DrawRect(31,17,3,2,C(199,191,120));DrawRect(38,18,3,2,C(160,163,116));
                    DrawPoly(new[]{new Vector2Int(44,18),new Vector2Int(55,13),new Vector2Int(55,31),new Vector2Int(43,26)},C(106,107,72));
                    DrawRect(23,23,4,9,dark);DrawRect(23,29,11,5,shade);DrawRect(25,31,7,2,light);DrawRect(20,24,3,3,C(14,21,19));
                    break;
                case ItemId.Water:
                    DrawRect(27,12,12,4,dark);DrawRect(28,8,10,6,C(156,142,94));
                    DrawRect(24,15,20,34,dark);DrawRect(22,17,20,30,shade);DrawRect(24,19,16,26,main);
                    DrawRect(26,22,4,19,C(151,190,178));DrawRect(31,24,8,16,C(49,99,103));
                    DrawRect(24,42,16,3,light);DrawRect(26,46,12,2,dark);DrawRect(28,10,7,2,glint);
                    break;
                case ItemId.CannedFood:
                    DrawRect(19,15,28,36,dark);DrawRect(17,17,30,32,shade);DrawRect(19,18,26,28,main);
                    DrawRect(18,14,29,4,light);DrawRect(19,47,27,4,C(159,148,104));DrawRect(21,20,22,3,C(207,182,119));
                    DrawRect(22,25,20,14,C(48,55,42));DrawRect(24,27,16,10,C(164,124,67));
                    DrawRect(27,28,10,2,C(218,191,126));DrawRect(27,32,10,2,C(100,79,48));DrawRect(25,41,16,2,shade);
                    break;
                case ItemId.Bandage:
                    // Rolled medical dressing with a bold red cross; unlike folded cloth.
                    DrawPoly(new[]{new Vector2Int(14,22),new Vector2Int(23,14),new Vector2Int(50,26),new Vector2Int(45,44),new Vector2Int(18,39)},dark);
                    DrawPoly(new[]{new Vector2Int(14,19),new Vector2Int(22,13),new Vector2Int(49,25),new Vector2Int(44,40),new Vector2Int(17,35)},shade);
                    DrawPoly(new[]{new Vector2Int(17,19),new Vector2Int(23,16),new Vector2Int(45,26),new Vector2Int(41,36),new Vector2Int(19,32)},light);
                    DrawRect(27,18,4,18,C(215,73,54));DrawRect(20,24,18,4,C(215,73,54));
                    DrawRect(38,28,5,8,C(237,219,174));DrawRect(17,31,8,4,C(181,163,126));
                    break;
                case ItemId.Cloth:
                    // Folded fabric with visible hems and a stitched patch.
                    DrawPoly(new[]{new Vector2Int(14,18),new Vector2Int(26,13),new Vector2Int(48,21),new Vector2Int(46,43),new Vector2Int(20,49),new Vector2Int(12,39)},dark);
                    DrawPoly(new[]{new Vector2Int(16,18),new Vector2Int(26,16),new Vector2Int(45,23),new Vector2Int(43,40),new Vector2Int(21,45),new Vector2Int(15,37)},main);
                    DrawRect(19,23,22,2,light);DrawRect(20,25,3,13,shade);DrawRect(24,36,18,2,shade);
                    DrawRect(29,26,10,7,C(118,124,95));DrawRect(31,27,6,1,glint);DrawRect(32,29,1,3,glint);
                    break;
                case ItemId.Scrap:
                    // One large bent metal plate: jagged edges, rust, and a torn bolt hole.
                    DrawPoly(new[]{new Vector2Int(12,23),new Vector2Int(22,15),new Vector2Int(32,18),new Vector2Int(39,13),new Vector2Int(52,22),new Vector2Int(48,42),new Vector2Int(36,49),new Vector2Int(17,43)},C(13,19,17));
                    DrawPoly(new[]{new Vector2Int(14,23),new Vector2Int(23,18),new Vector2Int(31,21),new Vector2Int(39,16),new Vector2Int(49,23),new Vector2Int(45,40),new Vector2Int(35,45),new Vector2Int(19,40)},shade);
                    DrawPoly(new[]{new Vector2Int(18,24),new Vector2Int(26,21),new Vector2Int(32,25),new Vector2Int(41,20),new Vector2Int(45,25),new Vector2Int(41,31),new Vector2Int(31,33),new Vector2Int(22,30)},main);
                    DrawRect(22,35,18,2,light);DrawRect(17,37,7,3,C(143,68,43));DrawRect(42,24,3,7,C(153,74,43));
                    DrawRect(26,27,4,3,C(28,35,31));DrawRect(35,22,3,2,glint);
                    break;
                case ItemId.MetalFragments:
                    // Three distinct steel shards, not the same silhouette as Scrap.
                    DrawPoly(new[]{new Vector2Int(13,42),new Vector2Int(23,17),new Vector2Int(31,38),new Vector2Int(24,45)},dark);
                    DrawPoly(new[]{new Vector2Int(14,40),new Vector2Int(23,20),new Vector2Int(27,39),new Vector2Int(22,43)},C(160,173,168));
                    DrawPoly(new[]{new Vector2Int(29,43),new Vector2Int(37,15),new Vector2Int(43,42),new Vector2Int(37,47)},shade);
                    DrawPoly(new[]{new Vector2Int(31,40),new Vector2Int(37,20),new Vector2Int(40,40),new Vector2Int(36,44)},light);
                    DrawPoly(new[]{new Vector2Int(39,35),new Vector2Int(48,23),new Vector2Int(50,40),new Vector2Int(45,44)},C(105,123,118));
                    DrawRect(18,25,3,7,C(221,218,180));DrawRect(34,25,3,9,C(214,215,187));
                    break;
                case ItemId.Wood:
                    // Three cut planks with end-grain rings.
                    DrawRect(15,18,34,9,dark);DrawRect(13,17,34,8,shade);DrawRect(15,18,29,3,light);DrawRect(17,22,25,2,main);
                    DrawRect(12,29,37,9,dark);DrawRect(15,28,35,8,main);DrawRect(18,29,27,2,light);DrawRect(16,34,30,1,shade);
                    DrawRect(17,40,33,8,dark);DrawRect(14,39,34,8,shade);DrawRect(17,40,25,2,light);DrawRect(18,44,23,1,main);
                    DrawRect(13,18,3,6,C(196,154,99));DrawRect(46,29,3,6,C(196,154,99));DrawRect(15,40,3,6,C(196,154,99));
                    break;
                case ItemId.Pistol:
                    DrawPoly(new[]{new Vector2Int(9,23),new Vector2Int(15,18),new Vector2Int(44,19),new Vector2Int(53,23),new Vector2Int(52,29),new Vector2Int(34,31),new Vector2Int(30,48),new Vector2Int(21,48),new Vector2Int(20,32),new Vector2Int(10,31)},C(9,14,14));
                    DrawRect(14,21,30,7,shade);DrawRect(16,19,24,6,main);DrawRect(19,19,23,2,light);
                    DrawRect(43,22,10,4,C(37,45,41));DrawRect(47,23,8,2,C(143,153,144));
                    DrawPoly(new[]{new Vector2Int(22,28),new Vector2Int(32,29),new Vector2Int(29,46),new Vector2Int(21,45)},dark);
                    DrawRect(23,30,5,11,shade);DrawRect(32,25,3,3,C(17,23,20));DrawRect(18,29,7,2,C(135,146,131));
                    break;
                case ItemId.Rifle:
                    DrawPoly(new[]{new Vector2Int(7,27),new Vector2Int(14,23),new Vector2Int(48,23),new Vector2Int(57,25),new Vector2Int(57,30),new Vector2Int(37,32),new Vector2Int(34,43),new Vector2Int(26,43),new Vector2Int(25,33),new Vector2Int(10,33)},C(11,15,14));
                    DrawRect(10,26,18,5,C(86,68,46));DrawRect(12,25,13,2,C(148,117,73));
                    DrawRect(24,24,23,5,shade);DrawRect(27,23,19,3,main);DrawRect(28,24,17,1,light);
                    DrawRect(44,24,12,2,C(128,137,121));DrawRect(53,23,5,4,C(76,88,77));
                    DrawRect(30,29,7,6,dark);DrawRect(31,31,3,10,shade);DrawRect(32,39,5,3,C(126,96,59));
                    DrawRect(35,20,9,3,C(45,57,48));DrawRect(37,19,5,1,light);
                    break;
                case ItemId.PistolAmmo:
                    for(int i=0;i<3;i++)
                    {
                        int x=17+i*12;DrawRect(x,23,8,22,C(27,34,31));DrawRect(x+1,25,6,18,C(194,157,69));
                        DrawPoly(new[]{new Vector2Int(x+1,25),new Vector2Int(x+4,18),new Vector2Int(x+7,25)},C(151,157,139));
                        DrawRect(x+1,40,6,3,C(231,194,94));DrawRect(x+2,28,2,8,C(232,202,118));
                    }
                    break;
                case ItemId.RifleAmmo:
                    for(int i=0;i<3;i++)
                    {
                        int x=17+i*12;DrawRect(x,19,8,28,C(25,32,30));DrawRect(x+1,22,6,22,C(177,126,48));
                        DrawPoly(new[]{new Vector2Int(x+1,22),new Vector2Int(x+4,13),new Vector2Int(x+7,22)},C(84,94,88));
                        DrawRect(x+1,41,6,3,C(228,184,80));DrawRect(x+2,25,2,11,C(224,172,89));
                    }
                    break;
                case ItemId.Pipe:
                    DrawPoly(new[]{new Vector2Int(15,47),new Vector2Int(18,50),new Vector2Int(50,18),new Vector2Int(47,14)},C(12,18,17));
                    DrawPoly(new[]{new Vector2Int(13,44),new Vector2Int(16,47),new Vector2Int(47,16),new Vector2Int(44,13)},shade);
                    DrawRect(18,40,7,7,C(138,62,43));DrawRect(40,18,7,7,C(151,72,45));
                    DrawRect(21,37,20,3,light);DrawRect(17,42,6,3,C(182,88,50));DrawRect(41,18,5,3,C(183,98,55));
                    DrawRect(29,29,8,3,dark);DrawRect(32,25,3,8,main);
                    break;
                case ItemId.Blueprint:
                    DrawRect(17,13,33,39,C(10,22,26));DrawRect(15,11,33,39,C(67,111,112));
                    DrawRect(19,15,25,30,C(47,89,95));DrawRect(19,15,25,2,C(153,187,160));DrawRect(19,15,2,30,C(153,187,160));
                    for(int x=23;x<44;x+=6)DrawRect(x,19,1,21,C(79,132,132));
                    for(int y=22;y<41;y+=6)DrawRect(21,y,21,1,C(79,132,132));
                    DrawRect(24,28,13,2,C(224,210,152));DrawRect(24,28,2,9,C(224,210,152));DrawRect(24,35,9,2,C(224,210,152));
                    DrawRect(32,24,8,2,C(224,210,152));DrawRect(38,24,2,7,C(224,210,152));DrawRect(28,40,14,2,C(224,210,152));
                    DrawRect(40,42,4,4,C(15,25,25));
                    break;
                case ItemId.WorkbenchI:
                    DrawRect(13,40,39,6,C(16,21,18));DrawRect(14,36,37,5,C(130,91,52));DrawRect(16,34,33,3,C(195,150,82));
                    DrawRect(18,40,4,12,C(94,66,42));DrawRect(44,40,4,12,C(94,66,42));
                    DrawRect(23,24,15,10,C(85,112,81));DrawRect(25,26,11,6,C(165,173,132));DrawRect(28,27,4,4,C(55,67,54));
                    DrawRect(17,18,3,17,C(164,169,137));DrawRect(14,17,9,3,C(164,169,137));
                    DrawRect(36,19,12,3,C(196,151,82));DrawRect(43,16,4,11,C(196,151,82));
                    DrawRect(21,32,25,2,C(49,39,29));
                    break;
                case ItemId.WorkbenchII:
                    DrawRect(12,40,41,6,C(12,20,19));DrawRect(14,36,37,5,C(72,95,87));DrawRect(16,34,33,3,C(149,172,153));
                    DrawRect(18,40,4,12,C(48,62,58));DrawRect(44,40,4,12,C(48,62,58));
                    DrawRect(21,22,22,11,C(24,43,40));DrawRect(24,25,16,5,C(64,125,104));DrawRect(29,23,8,9,C(182,175,108));
                    DrawRect(31,25,4,5,C(24,42,39));DrawRect(18,18,5,3,C(193,152,76));DrawRect(42,18,5,3,C(193,152,76));
                    DrawRect(25,31,17,2,C(114,138,112));DrawRect(16,47,32,2,C(30,42,37));
                    break;
                case ItemId.FieldJacket:
                    DrawPoly(new[]{new Vector2Int(22,14),new Vector2Int(29,18),new Vector2Int(36,18),new Vector2Int(43,14),new Vector2Int(53,21),new Vector2Int(47,32),new Vector2Int(43,29),new Vector2Int(44,50),new Vector2Int(19,50),new Vector2Int(20,29),new Vector2Int(15,33),new Vector2Int(10,22)},dark);
                    DrawPoly(new[]{new Vector2Int(23,16),new Vector2Int(29,20),new Vector2Int(35,20),new Vector2Int(42,16),new Vector2Int(49,22),new Vector2Int(44,29),new Vector2Int(41,27),new Vector2Int(42,47),new Vector2Int(21,47),new Vector2Int(22,27),new Vector2Int(18,29),new Vector2Int(14,23)},main);
                    DrawRect(30,20,3,26,light);DrawRect(24,27,5,8,shade);DrawRect(35,27,6,8,shade);
                    DrawRect(25,29,3,3,C(165,141,86));DrawRect(36,29,4,3,C(165,141,86));DrawRect(24,39,15,2,dark);
                    DrawRect(20,45,23,2,C(108,91,59));DrawRect(28,18,6,4,C(191,172,123));
                    break;
                case ItemId.FieldPants:
                    DrawRect(18,15,30,11,dark);DrawRect(20,16,26,8,main);DrawRect(30,17,3,27,shade);
                    DrawPoly(new[]{new Vector2Int(20,23),new Vector2Int(31,23),new Vector2Int(28,49),new Vector2Int(18,49)},shade);
                    DrawPoly(new[]{new Vector2Int(32,23),new Vector2Int(45,23),new Vector2Int(47,49),new Vector2Int(35,49)},main);
                    DrawRect(20,35,8,7,C(105,111,81));DrawRect(36,35,8,7,C(97,106,79));
                    DrawRect(21,36,6,2,light);DrawRect(37,36,6,2,light);DrawRect(18,47,11,3,C(47,44,35));DrawRect(35,47,13,3,C(47,44,35));
                    break;
                case ItemId.WorkBoots:
                    DrawPoly(new[]{new Vector2Int(13,20),new Vector2Int(27,20),new Vector2Int(28,37),new Vector2Int(50,42),new Vector2Int(53,49),new Vector2Int(12,49),new Vector2Int(10,44),new Vector2Int(18,39)},dark);
                    DrawPoly(new[]{new Vector2Int(15,21),new Vector2Int(24,22),new Vector2Int(25,37),new Vector2Int(47,42),new Vector2Int(50,46),new Vector2Int(14,46),new Vector2Int(13,43),new Vector2Int(19,39)},main);
                    DrawRect(18,23,5,12,light);DrawRect(20,28,8,2,C(162,143,98));DrawRect(25,37,18,3,C(76,77,63));
                    DrawRect(15,45,34,2,C(193,167,114));DrawRect(40,43,8,2,light);
                    break;
                case ItemId.Backpack:
                    DrawRect(20,16,26,34,dark);DrawRect(18,18,27,30,shade);DrawRect(22,19,18,26,main);
                    DrawRect(24,21,14,3,light);DrawRect(21,29,20,13,dark);DrawRect(23,30,16,10,main);
                    DrawRect(26,31,10,2,light);DrawRect(29,34,5,5,shade);DrawRect(17,21,4,21,C(166,145,93));
                    DrawRect(43,21,4,21,C(166,145,93));DrawRect(23,44,16,3,C(175,154,101));
                    break;
                case ItemId.Helmet:
                    DrawPoly(new[]{new Vector2Int(13,37),new Vector2Int(15,25),new Vector2Int(23,17),new Vector2Int(40,17),new Vector2Int(49,26),new Vector2Int(51,37),new Vector2Int(55,39),new Vector2Int(53,43),new Vector2Int(11,43),new Vector2Int(9,39)},dark);
                    DrawPoly(new[]{new Vector2Int(15,36),new Vector2Int(18,26),new Vector2Int(24,20),new Vector2Int(39,20),new Vector2Int(46,27),new Vector2Int(49,36),new Vector2Int(53,39),new Vector2Int(13,39)},main);
                    DrawRect(19,25,24,3,light);DrawRect(22,20,18,2,C(155,149,98));DrawRect(10,39,44,4,C(134,121,75));
                    DrawRect(31,34,7,5,C(211,180,91));DrawRect(32,35,5,2,C(237,202,115));
                    break;
                case ItemId.ArmorVest:
                    DrawPoly(new[]{new Vector2Int(18,14),new Vector2Int(27,18),new Vector2Int(37,18),new Vector2Int(46,14),new Vector2Int(53,23),new Vector2Int(46,31),new Vector2Int(45,49),new Vector2Int(19,49),new Vector2Int(18,31),new Vector2Int(11,23)},dark);
                    DrawPoly(new[]{new Vector2Int(21,17),new Vector2Int(28,21),new Vector2Int(36,21),new Vector2Int(43,17),new Vector2Int(48,23),new Vector2Int(43,29),new Vector2Int(42,46),new Vector2Int(22,46),new Vector2Int(21,29),new Vector2Int(16,23)},main);
                    DrawRect(29,21,5,22,shade);DrawRect(22,28,7,8,light);DrawRect(35,28,6,8,light);
                    DrawRect(21,38,21,3,C(180,153,92));DrawRect(23,40,5,5,dark);DrawRect(36,40,5,5,dark);
                    break;
                case ItemId.Medkit:
                    DrawRect(15,19,36,31,dark);DrawRect(14,17,36,30,C(157,137,104));DrawRect(17,20,30,24,C(204,198,171));
                    DrawRect(19,16,27,5,C(225,214,180));DrawRect(27,13,14,4,C(165,150,111));DrawRect(29,14,10,2,C(30,39,34));
                    DrawRect(28,25,8,16,C(158,54,46));DrawRect(23,29,18,8,C(158,54,46));
                    DrawRect(19,43,29,2,C(138,124,96));DrawRect(43,22,3,4,C(118,151,97));
                    break;
                case ItemId.Keycard:
                    // High-contrast green lift pass with a gold chip and access stripe.
                    DrawRect(15,13,36,40,C(8,14,12));DrawRect(17,11,34,40,C(29,79,49));
                    DrawRect(19,13,30,34,C(51,131,69));DrawRect(21,15,26,8,C(119,188,92));
                    DrawRect(22,27,11,9,C(189,166,91));DrawRect(24,29,7,5,C(91,109,65));
                    DrawRect(36,28,10,2,C(27,82,47));DrawRect(36,33,8,2,C(27,82,47));DrawRect(21,42,19,2,C(177,213,128));
                    DrawRect(43,17,4,4,C(31,79,44));DrawRect(44,18,2,2,C(191,221,132));
                    break;
                case ItemId.HazmatSuit:
                    // Yellow sealed hood and suit, with blue-gray glass and clear hazard bands.
                    DrawPoly(new[]{new Vector2Int(22,12),new Vector2Int(42,12),new Vector2Int(47,21),new Vector2Int(53,27),new Vector2Int(47,34),new Vector2Int(44,50),new Vector2Int(19,50),new Vector2Int(16,34),new Vector2Int(10,27),new Vector2Int(16,21)},dark);
                    DrawPoly(new[]{new Vector2Int(24,15),new Vector2Int(40,15),new Vector2Int(44,22),new Vector2Int(48,27),new Vector2Int(43,31),new Vector2Int(41,46),new Vector2Int(22,46),new Vector2Int(20,31),new Vector2Int(15,27),new Vector2Int(20,22)},main);
                    DrawRect(25,19,14,9,C(44,74,73));DrawRect(27,20,10,2,C(164,198,171));DrawRect(29,23,6,3,C(80,125,123));
                    DrawRect(22,32,20,3,C(224,205,117));DrawRect(22,37,18,2,C(113,111,55));DrawRect(17,24,5,3,light);
                    DrawRect(30,40,7,4,C(159,53,43));DrawRect(32,39,3,6,C(231,194,98));
                    break;
                case ItemId.CircuitBoard:
                    DrawRect(14,15,38,36,C(8,17,14));DrawRect(16,13,34,35,C(30,91,60));DrawRect(19,16,28,29,C(48,129,77));
                    DrawRect(26,22,14,14,C(17,43,34));DrawRect(28,24,10,10,C(21,31,28));DrawRect(29,25,8,8,C(116,121,93));
                    DrawRect(22,20,4,2,C(226,185,85));DrawRect(40,21,4,2,C(226,185,85));DrawRect(22,37,4,2,C(226,185,85));DrawRect(40,37,4,2,C(226,185,85));
                    DrawRect(20,22,2,15,C(204,164,78));DrawRect(43,22,2,15,C(204,164,78));DrawRect(21,27,6,2,C(204,164,78));DrawRect(37,32,7,2,C(204,164,78));
                    DrawRect(19,45,2,3,C(210,180,103));DrawRect(25,45,2,3,C(210,180,103));DrawRect(31,45,2,3,C(210,180,103));DrawRect(37,45,2,3,C(210,180,103));DrawRect(43,45,2,3,C(210,180,103));
                    break;
                default:
                    DrawRect(18,17,28,30,dark);DrawRect(20,15,24,30,main);DrawRect(23,19,18,4,light);DrawRect(23,27,18,3,shade);DrawRect(23,36,13,3,shade);
                    break;
            }
            texture=new Texture2D(S,S,TextureFormat.RGBA32,false)
            {filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave,name="ItemIcon_"+id};
            texture.SetPixels32(pixels);texture.Apply(false,true);cache[id]=texture;return texture;
        }

        static Color32 C(byte r,byte g,byte b)=>new Color32(r,g,b,255);
        static Color32 Dark(Color32 c,float f)=>new Color32((byte)(c.r*f),(byte)(c.g*f),(byte)(c.b*f),c.a);
        static Color32 Light(Color32 c,float f)=>new Color32((byte)Mathf.Min(255,c.r*f),(byte)Mathf.Min(255,c.g*f),(byte)Mathf.Min(255,c.b*f),c.a);
        static void DrawRect(int x,int y,int w,int h,Color32 c)
        {
            for(int yy=Mathf.Max(0,y);yy<Mathf.Min(S,y+h);yy++)
            for(int xx=Mathf.Max(0,x);xx<Mathf.Min(S,x+w);xx++)pixels[yy*S+xx]=c;
        }
        static void DrawPoly(Vector2Int[] points,Color32 c)
        {
            int minY=S,maxY=0,minX=S,maxX=0;
            foreach(var p in points){minY=Mathf.Min(minY,p.y);maxY=Mathf.Max(maxY,p.y);minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);}
            for(int y=Mathf.Max(0,minY);y<Mathf.Min(S,maxY+1);y++)
            for(int x=Mathf.Max(0,minX);x<Mathf.Min(S,maxX+1);x++)
            {
                bool inside=false;
                for(int i=0,j=points.Length-1;i<points.Length;j=i++)
                {
                    Vector2Int a=points[i],b=points[j];
                    if(((a.y>y)!=(b.y>y))&&(x<(b.x-a.x)*(float)(y-a.y)/(b.y-a.y)+a.x))inside=!inside;
                }
                if(inside)pixels[y*S+x]=c;
            }
        }
    }
}
