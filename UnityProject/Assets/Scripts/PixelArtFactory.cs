using System;
using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>
    /// Sprite library for the 2D game. Everything is loaded from the painted
    /// pixel-art atlases in Assets/Resources/Art (built by Tools/art2d), so the
    /// runtime never synthesises placeholder art unless an asset is missing.
    /// </summary>
    public static class PixelArtFactory
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        static Color32 C(byte r, byte g, byte b, byte a = 255) => new Color32(r, g, b, a);
        static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        // ------------------------------------------------------------------
        // asset access
        // ------------------------------------------------------------------
        public static Sprite Atlas(string name)
        {
            if (cache.TryGetValue(name, out Sprite found) && found != null) return found;
            Sprite sprite = PixelArtAtlas.Load(name);
            if (sprite != null) cache[name] = sprite;
            return sprite;
        }

        static Sprite AtlasOr(string name, Sprite fallback)
        {
            Sprite sprite = Atlas(name);
            return sprite != null ? sprite : fallback;
        }

        public static Texture2D TileTexture(string name)
        {
            if (textures.TryGetValue(name, out Texture2D found) && found != null) return found;
            Texture2D texture = Resources.Load<Texture2D>("Art/Tiles/" + name);
            if (texture == null) return null;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;
            textures[name] = texture;
            return texture;
        }

        public static Sprite Tile(string name)
        {
            string key = "tile:" + name;
            if (cache.TryGetValue(key, out Sprite found) && found != null) return found;
            Texture2D texture = TileTexture(name);
            if (texture == null) return null;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(.5f, .5f), 96f, 0, SpriteMeshType.FullRect, Vector4.zero, false);
            sprite.name = "tile_" + name;
            cache[key] = sprite;
            return sprite;
        }

        public static Texture2D ItemIcon(ItemId id)
        {
            string key = "icon:" + id;
            if (textures.TryGetValue(key, out Texture2D found) && found != null) return found;
            string file = IconName(id);
            Texture2D texture = Resources.Load<Texture2D>("Art/PixelArt/icons/" + file);
            if (texture == null) texture = ItemIconFactory.Procedural(id);
            if (texture == null) return null;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            textures[key] = texture;
            return texture;
        }

        public static string IconName(ItemId id)
        {
            switch (id)
            {
                case ItemId.Flashlight: return "flashlight";
                case ItemId.Cloth: return "cloth";
                case ItemId.Scrap: return "scrap";
                case ItemId.MetalFragments: return "metal_fragments";
                case ItemId.Wood: return "wood";
                case ItemId.Water: return "water";
                case ItemId.CannedFood: return "canned_food";
                case ItemId.Bandage: return "bandage";
                case ItemId.Pistol: return "pistol";
                case ItemId.PistolAmmo: return "pistol_ammo";
                case ItemId.Pipe: return "pipe";
                case ItemId.Blueprint: return "blueprint";
                case ItemId.WorkbenchI: return "workbench_1";
                case ItemId.WorkbenchII: return "workbench_2";
                case ItemId.FieldJacket: return "field_jacket";
                case ItemId.FieldPants: return "field_pants";
                case ItemId.WorkBoots: return "work_boots";
                case ItemId.Backpack: return "backpack";
                case ItemId.Helmet: return "helmet";
                case ItemId.ArmorVest: return "armor_vest";
                case ItemId.Medkit: return "medkit";
                case ItemId.Keycard: return "keycard";
                case ItemId.Rifle: return "rifle";
                case ItemId.RifleAmmo: return "rifle_ammo";
                case ItemId.HazmatSuit: return "hazmat";
                case ItemId.CircuitBoard: return "circuit_board";
                case ItemId.BuildingPlan: return "building_plan";
                case ItemId.Hammer: return "hammer";
                case ItemId.Stone: return "stone";
                case ItemId.HighQualityMetal: return "high_quality_metal";
                case ItemId.StorageBox: return "storage_box";
                case ItemId.WorkbenchStationI: return "workbench_station_1";
                case ItemId.WorkbenchStationII: return "workbench_station_2";
                case ItemId.ToolCupboard: return "tool_cupboard";
                default: return "scrap";
            }
        }

        // ------------------------------------------------------------------
        // flat colour block (colliders, shadow shapes, light pools)
        // ------------------------------------------------------------------
        public static Sprite Block(Color color)
        {
            string key = $"block_{color.r}_{color.g}_{color.b}_{color.a}";
            if (cache.TryGetValue(key, out Sprite sprite) && sprite != null) return sprite;
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                name = "pixel_block",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            Color32 c = color;
            var pixels = new Color32[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(.5f, .5f), 4f, 0, SpriteMeshType.FullRect);
            sprite.name = "block";
            cache[key] = sprite;
            return sprite;
        }

        // ------------------------------------------------------------------
        // characters
        // ------------------------------------------------------------------
        public static Sprite Survivor(int frame = 0, ItemStack[] gear = null, bool hostile = false)
        {
            if (hostile) return Watcher(frame);
            Sprite variant = GearVariant(frame, gear);
            return variant != null ? variant : Atlas("survivor_idle");
        }

        /// <summary>Optional specialised frames for worn gear (falls back to the base frame).</summary>
        static Sprite GearVariant(int frame, ItemStack[] gear)
        {
            string suffix = null;
            if (gear != null)
            {
                if (Has(gear, GearSlot.Chest, ItemId.HazmatSuit)) suffix = "hazmat";
                else if (Has(gear, GearSlot.Head, ItemId.Helmet)) suffix = "helmet";
            }
            string baseName = FrameName(frame);
            if (suffix != null)
            {
                Sprite special = Atlas(baseName + "_" + suffix);
                if (special != null) return special;
            }
            return Atlas(baseName);
        }

        static bool Has(ItemStack[] gear, GearSlot slot, ItemId id)
        {
            int index = (int)slot;
            return gear != null && gear.Length > index && !gear[index].Empty && gear[index].id == id;
        }

        static string FrameName(int frame)
        {
            switch (((frame % 5) + 5) % 5)
            {
                case 1: return "survivor_walk_a";
                case 2: return "survivor_walk_b";
                case 3: return "survivor_jump";
                case 4: return "survivor_attack";
                default: return "survivor_idle";
            }
        }

        public static Sprite Watcher(int frame = 0)
        {
            return frame % 2 == 0 ? Atlas("watcher_idle") : Atlas("watcher_attack");
        }

        public static Sprite TraderSprite()
        {
            Sprite trader = Atlas("trader");
            return trader != null ? trader : Survivor(0);
        }

        // ------------------------------------------------------------------
        // environment: tiling materials
        // ------------------------------------------------------------------
        public static Sprite WallTile(int theme)
        {
            Sprite sprite = theme == 0 ? Tile("wall_level0") : Tile("wall_level1");
            if (sprite != null) return sprite;
            return Tile("wall_level1") ?? Tile("wall_level0") ?? Block(new Color(.35f, .33f, .25f));
        }

        public static Sprite FloorTile(int theme)
        {
            Sprite sprite = theme == 0 ? Tile("carpet_level0") : Tile("floor_level1");
            if (sprite != null) return sprite;
            return Tile("floor_level1") ?? Tile("carpet_level0") ?? Block(new Color(.3f, .28f, .22f));
        }

        public static Sprite CeilingTile(int theme = 3)
        {
            Sprite sprite = theme == 0 ? Tile("ceiling_level0") : Tile("ceiling_level1");
            if (sprite != null) return sprite;
            return Tile("ceiling_level1") ?? Tile("ceiling_level0") ?? Block(new Color(.25f, .25f, .21f));
        }

        public static Sprite ManilaWallTile()
        {
            return Tile("manila_wall") ?? Tile("wall_level0") ?? Block(new Color(.7f, .64f, .46f));
        }

        // ------------------------------------------------------------------
        // environment: objects
        // ------------------------------------------------------------------
        public static Sprite WallOutlet() => AtlasOr("outlet", Block(new Color(.55f, .5f, .38f)));
        public static Sprite WarningPlacard(int theme) => AtlasOr("placard", Block(new Color(.4f, .4f, .3f)));
        public static Sprite CeilingLamp() => Atlas("lamp");
        public static Sprite ServicePipe() => Atlas("pipe");
        public static Sprite GreenAccessCard() => AtlasOr("keycard", ItemSprite(ItemId.Keycard));
        public static Sprite BunkerDoor() => AtlasOr("door", Block(new Color(.3f, .25f, .2f)));

        public static Sprite ElevatorDoor(bool open)
        {
            Sprite sprite = Atlas(open ? "elevator_open" : "elevator_closed");
            return sprite != null ? sprite : BunkerDoor();
        }

        public static Sprite Crate2D(int tier)
        {
            string name = tier <= 1 ? "crate_1" : tier == 2 ? "crate_2" : "crate_3";
            return AtlasOr(name, Block(new Color(.45f, .35f, .22f)));
        }

        public static Sprite Crate() => Crate2D(1);

        public static Sprite Workbench2D(int tier)
        {
            string name = tier <= 1 ? "workbench_1" : tier == 2 ? "workbench_2" : "workbench_3";
            return AtlasOr(name, Block(new Color(.4f, .35f, .3f)));
        }

        public static Sprite ItemSprite(ItemId id)
        {
            string key = "world_item:" + id;
            if (cache.TryGetValue(key, out Sprite found) && found != null) return found;
            Texture2D texture = ItemIcon(id);
            if (texture == null) return Block(new Color(.6f, .55f, .4f));
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(.5f, 0f), 48f, 0, SpriteMeshType.FullRect, Vector4.zero, false);
            sprite.name = "item_" + id;
            cache[key] = sprite;
            return sprite;
        }

        public static Sprite Supply(SupplyPickup.Kind kind)
        {
            switch (kind)
            {
                case SupplyPickup.Kind.Water: return ItemSprite(ItemId.Water);
                case SupplyPickup.Kind.Food: return ItemSprite(ItemId.CannedFood);
                case SupplyPickup.Kind.Cloth: return ItemSprite(ItemId.Cloth);
                default: return ItemSprite(ItemId.Scrap);
            }
        }

        // ------------------------------------------------------------------
        // handheld props (drawn procedurally: they sit in front of the player)
        // ------------------------------------------------------------------
        public static Sprite HandFlashlight()
        {
            const string key = "hand_flashlight_px";
            if (cache.TryGetValue(key, out Sprite found) && found != null) return found;
            var texture = new Texture2D(28, 16, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "hand_flashlight_px"
            };
            var p = new Color32[28 * 16];
            for (int i = 0; i < p.Length; i++) p[i] = Clear;
            void R(int x, int y, int w, int h, Color32 c)
            {
                for (int yy = y; yy < y + h; yy++)
                    for (int xx = x; xx < x + w; xx++)
                        if (xx >= 0 && yy >= 0 && xx < 28 && yy < 16) p[yy * 28 + xx] = c;
            }
            var body = C(48, 57, 54);
            var light = C(78, 92, 78);
            var dark = C(26, 32, 30);
            R(9, 3, 9, 10, dark);
            R(10, 4, 7, 8, body);
            R(10, 10, 7, 1, light);
            R(4, 2, 6, 12, C(150, 138, 96));
            R(5, 3, 4, 10, C(196, 182, 128));
            R(2, 3, 3, 10, C(232, 214, 150));
            R(1, 4, 1, 8, C(255, 240, 190));
            R(18, 4, 4, 8, body);
            R(22, 5, 3, 6, dark);
            R(11, 1, 5, 2, C(30, 38, 34));
            R(9, 7, 3, 3, C(122, 96, 60));
            texture.SetPixels32(p);
            texture.Apply(false, false);
            found = Sprite.Create(texture, new Rect(0, 0, 28, 16), new Vector2(.05f, .5f), 40f, 0, SpriteMeshType.FullRect);
            found.name = key;
            cache[key] = found;
            return found;
        }

        public static Sprite FlashlightBeam()
        {
            const string key = "flashlight_beam_px";
            if (cache.TryGetValue(key, out Sprite found) && found != null) return found;
            const int width = 192, height = 88, center = height / 2;
            var p = new Color32[width * height];
            for (int i = 0; i < p.Length; i++) p[i] = Clear;
            for (int x = 0; x < width; x++)
            {
                float t = x / (float)(width - 1);
                int halfHeight = Mathf.Max(2, Mathf.RoundToInt(Mathf.Lerp(3f, 35f, t)));
                for (int y = center - halfHeight; y <= center + halfHeight; y++)
                {
                    if (y < 0 || y >= height) continue;
                    float edge = Mathf.Abs(y - center) / (float)halfHeight;
                    float falloff = (.34f + .66f * (1f - edge) * (1f - edge)) * (1f - .58f * t);
                    byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt(96f * falloff), 12, 96);
                    p[y * width + x] = edge < .28f ? new Color32(255, 244, 208, alpha) : new Color32(219, 209, 158, alpha);
                }
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = key
            };
            texture.SetPixels32(p);
            texture.Apply(false, false);
            found = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.025f, .5f), 32f, 0, SpriteMeshType.FullRect);
            found.name = key;
            cache[key] = found;
            return found;
        }

        // ------------------------------------------------------------------
        // menu backdrop: a pixel-art corridor painted at 320x180
        // ------------------------------------------------------------------
        public static Sprite MenuCorridor()
        {
            const string key = "pixel_menu_corridor";
            if (cache.TryGetValue(key, out Sprite found) && found != null) return found;
            const int w = 320, h = 180;
            var p = new Color32[w * h];
            var wallpaper = new[] { C(96, 78, 40), C(120, 99, 50), C(140, 116, 58), C(74, 60, 32) };
            var ceiling = new[] { C(58, 55, 42), C(74, 70, 54), C(38, 37, 30) };
            var floor = new[] { C(64, 52, 30), C(84, 68, 38), C(48, 39, 24) };
            void R(int x, int y, int rw, int rh, Color32 c)
            {
                for (int yy = y; yy < y + rh; yy++)
                    for (int xx = x; xx < x + rw; xx++)
                        if (xx >= 0 && yy >= 0 && xx < w && yy < h) p[yy * w + xx] = c;
            }
            var rng = new System.Random(7);
            // ceiling
            for (int y = 118; y < h; y++)
                for (int x = 0; x < w; x++) p[y * w + x] = ceiling[(x / 26 + (y > 160 ? 2 : 0)) % 3];
            // walls with a perspective squeeze toward the vanishing point at x=160
            for (int y = 40; y < 132; y++)
            {
                float t = (y - 40) / 92f;
                int half = Mathf.RoundToInt(Mathf.Lerp(26, 150, t * t * .9f + t * .1f));
                for (int x = 0; x < w; x++)
                {
                    int dist = Mathf.Abs(x - 160);
                    if (dist < half) continue;
                    int band = (y / 6) % 2;
                    int stripe = ((x + (band * 5)) / 9) % 4;
                    p[y * w + x] = wallpaper[(stripe + band) % 4];
                    if (dist % 52 < 2) p[y * w + x] = C(58, 47, 26);
                }
                if (((y - 40) % 13) == 0)
                    for (int x = 0; x < w; x++)
                        if (Mathf.Abs(x - 160) >= half) p[y * w + x] = C(70, 57, 30);
            }
            // floor
            for (int y = 0; y < 40; y++)
                for (int x = 0; x < w; x++)
                {
                    int shade = (y < 12) ? 2 : (y % 7 < 2 ? 1 : 0);
                    p[y * w + x] = floor[shade];
                    if (((x + y * 3) % 23) == 0) p[y * w + x] = C(40, 33, 20);
                }
            // corridor mouth
            R(126, 40, 68, 92, C(12, 14, 13));
            R(134, 44, 52, 84, C(20, 24, 21));
            for (int i = 0; i < 12; i++)
            {
                R(134, 44 + i * 7, 52 - i * 2, 2, C(72, 66, 44));
            }
            // ceiling lamps receding
            for (int i = 0; i < 5; i++)
            {
                int lampY = 158 - i * 15;
                int lampHalf = Mathf.Max(9, 44 - i * 7);
                R(160 - lampHalf, lampY, lampHalf * 2, 4, C(230, 214, 156));
                R(160 - lampHalf, lampY - 1, lampHalf * 2, 1, C(255, 240, 190));
                R(160 - lampHalf + 3, lampY + 4, lampHalf * 2 - 6, 2, C(150, 140, 100));
            }
            // wall pipes and a sign
            for (int x = 24; x < w; x += 47)
            {
                R(x, 52, 3, 76, C(52, 58, 50));
                R(x + 1, 54, 1, 72, C(96, 104, 84));
            }
            R(206, 84, 24, 16, C(74, 82, 60));
            R(208, 86, 20, 12, C(118, 126, 88));
            R(211, 90, 14, 2, C(30, 36, 30));
            R(211, 94, 10, 2, C(30, 36, 30));
            // vignette
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = Mathf.Abs(x - 160) / 160f, dy = Mathf.Abs(y - 90) / 90f;
                    float v = Mathf.Clamp01(1f - (dx * dx * .55f + dy * dy * .75f));
                    Color32 c = p[y * w + x];
                    p[y * w + x] = C((byte)(c.r * (.5f + .5f * v)), (byte)(c.g * (.5f + .5f * v)), (byte)(c.b * (.5f + .5f * v)));
                }
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "menu_corridor_px"
            };
            texture.SetPixels32(p);
            texture.Apply(false, false);
            found = Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(.5f, .5f), 1f, 0, SpriteMeshType.FullRect);
            found.name = key;
            cache[key] = found;
            return found;
        }

        public static void ClearCache()
        {
            cache.Clear();
            textures.Clear();
        }
    }
}
