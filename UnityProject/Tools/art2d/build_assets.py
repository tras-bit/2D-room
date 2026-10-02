#!/usr/bin/env python3
"""Build every SUBSISTENCE 2D art asset from the painted source art.

Authored source art lives in UnityProject/ArtSource/raw. Everything needed to
rebuild the in-game atlases is part of the repository. Outputs:

  Assets/Resources/Art/PixelArt/characters.png   character frames atlas
  Assets/Resources/Art/PixelArt/props.png        world objects atlas
  Assets/Resources/Art/PixelArt/icons/*.png      individual HUD icons
  Assets/Resources/Art/Tiles/*.png               seamless tiling materials
  Assets/Scripts/PixelArtAtlas.cs                generated lookup table

Run:  python3 build_assets.py            (all groups)
      python3 build_assets.py chars      (subset: chars, tiles, props, items)
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

import numpy as np
from PIL import Image

sys.path.insert(0, str(Path(__file__).resolve().parent))

import spritegen as sg  # noqa: E402
import fallback as fb  # noqa: E402

PROJECT = Path(__file__).resolve().parents[2]
RES = PROJECT / "Assets" / "Resources" / "Art"
PIX = RES / "PixelArt"
TILES = RES / "Tiles"
CS_OUT = PROJECT / "Assets" / "Scripts" / "PixelArtAtlas.cs"
META = PROJECT / "Tools" / "art2d" / "atlas_manifest.json"

PPU = 96.0
ALPHA_CUT = 140
MATERIAL_RAMPS = ["paper", "cloth", "yellow", "olive", "grey", "rust", "teal"]

# --------------------------------------------------------------------------
# manifests: source file -> atlas cell
# --------------------------------------------------------------------------
CHARACTERS = [
    # name, source, cell w, cell h, pivot
    ("survivor_idle", "px_survivor_idle", 224, 208, (0.5, 0.0)),
    ("survivor_walk_a", "px_survivor_walkA", 224, 208, (0.5, 0.0)),
    ("survivor_walk_b", "px_survivor_walkB", 224, 208, (0.5, 0.0)),
    ("survivor_jump", "px_survivor_jump", 224, 208, (0.5, 0.0)),
    ("survivor_attack", "px_survivor_attack", 224, 208, (0.5, 0.0)),
    ("watcher_idle", "px_watcher_idle", 224, 208, (0.5, 0.0)),
    ("watcher_attack", "px_watcher_attack", 224, 208, (0.5, 0.0)),
    ("trader", "px_trader", 224, 208, (0.5, 0.0)),
    ("survivor_pack", "px_survivor_idle", 224, 208, (0.5, 0.0)),
]

PROPS = [
    # name, source, cell w, cell h, pivot
    ("crate_1", "px_crate_1", 128, 112, (0.5, 0.0)),
    ("crate_2", "px_crate_2", 128, 112, (0.5, 0.0)),
    ("crate_3", "px_crate_3", 128, 112, (0.5, 0.0)),
    ("workbench_1", "px_bench_1", 160, 128, (0.5, 0.0)),
    ("workbench_2", "px_bench_2", 160, 128, (0.5, 0.0)),
    ("workbench_3", "px_bench_3", 160, 128, (0.5, 0.0)),
    ("door", "px_door", 176, 240, (0.5, 0.0)),
    ("elevator_closed", "px_elevator_closed", 176, 240, (0.5, 0.0)),
    ("elevator_open", "px_elevator_open", 176, 240, (0.5, 0.0)),
    ("outlet", "px_outlet", 48, 40, (0.5, 0.0)),
    ("placard", "px_placard", 80, 56, (0.5, 0.0)),
    ("lamp", "px_lamp", 160, 48, (0.5, 1.0)),
    ("pipe", "px_pipe", 192, 40, (0.5, 0.5)),
    ("keycard", "px_keycard", 48, 48, (0.5, 0.5)),
]

ITEMS = [
    # ItemId order used by PixelArtFactory (26 drawable entries)
    ("flashlight", "px_item_flashlight"),
    ("cloth", "px_item_cloth"),
    ("scrap", "px_item_scrap"),
    ("metal_fragments", "px_item_metal"),
    ("wood", "px_item_wood"),
    ("water", "px_item_water"),
    ("canned_food", "px_item_food"),
    ("bandage", "px_item_bandage"),
    ("pistol", "px_item_pistol"),
    ("pistol_ammo", "px_item_pistol_ammo"),
    ("pipe", "px_item_pipe"),
    ("blueprint", "px_item_blueprint"),
    ("workbench_1", "px_item_bench1"),
    ("workbench_2", "px_item_bench2"),
    ("field_jacket", "px_item_jacket"),
    ("field_pants", "px_item_pants"),
    ("work_boots", "px_item_boots"),
    ("backpack", "px_item_backpack"),
    ("helmet", "px_item_helmet"),
    ("armor_vest", "px_item_vest"),
    ("medkit", "px_item_medkit"),
    ("keycard", "px_item_keycard"),
    ("rifle", "px_item_rifle"),
    ("rifle_ammo", "px_item_rifle_ammo"),
    ("hazmat", "px_item_hazmat"),
    ("circuit_board", "px_item_board"),
]

TILE_JOBS = [
    ("wall_level0", "tile_wall_l0", (384, 384), True),
    ("carpet_level0", "tile_carpet_l0", (384, 96), True),
    ("ceiling_level0", "tile_ceiling_l0", (384, 48), True),
    ("wall_level1", "tile_wall_l1", (384, 384), True),
    ("floor_level1", "tile_floor_l1", (384, 96), True),
    ("ceiling_level1", "tile_ceiling_l1", (384, 48), True),
    ("manila_wall", "tile_manila", (384, 384), True),
]

CHAR_TARGET = {
    "px_survivor_idle": 144, "px_survivor_walkA": 146, "px_survivor_walkB": 144,
    "px_survivor_jump": 140, "px_survivor_attack": 150,
    "px_watcher_idle": 150, "px_watcher_attack": 152, "px_trader": 148,
}

entries: list[dict] = []


# --------------------------------------------------------------------------
# helpers
# --------------------------------------------------------------------------
def make_seamless(img: Image.Image, band=0.18) -> Image.Image:
    """Cross-fade against a half-rolled copy and seal all four pixel edges."""
    mode = "RGBA" if "A" in img.getbands() else "RGB"
    a = np.asarray(img.convert(mode)).astype(np.float32)
    h, w = a.shape[:2]
    rolled = np.roll(np.roll(a, h // 2, axis=0), w // 2, axis=1)
    yy = np.arange(h, dtype=np.float32)[:, None] / h
    xx = np.arange(w, dtype=np.float32)[None, :] / w
    # Weight is 1 at an outer edge and fades to 0 in the centre.
    fx = np.clip((band - np.minimum(xx, 1 - xx)) / band, 0, 1)
    fy = np.clip((band - np.minimum(yy, 1 - yy)) / band, 0, 1)
    fx = fx * fx * (3 - 2 * fx)
    fy = fy * fy * (3 - 2 * fy)
    weight = np.clip(fx + fy, 0, 1)[:, :, None]
    out = a * (1 - weight) + rolled * weight
    # Exact equality avoids a hairline seam after point-filtered tiling.
    edge = (out[:, 0, :] + out[:, -1, :]) * 0.5
    out[:, 0, :] = edge
    out[:, -1, :] = edge
    edge = (out[0, :, :] + out[-1, :, :]) * 0.5
    out[0, :, :] = edge
    out[-1, :, :] = edge
    return Image.fromarray(np.clip(out, 0, 255).astype(np.uint8), mode)


def seal_palette_edges(img: Image.Image, palette, weights) -> Image.Image:
    """Make opposing borders identical after quantisation/dithering."""
    arr = np.asarray(img.convert("RGBA")).copy()
    colors = np.asarray(palette, dtype=np.float32)
    rgb_weights = np.asarray(weights, dtype=np.float32)

    def nearest(values):
        flat = values.reshape(-1, 3).astype(np.float32)
        out = np.empty_like(flat)
        for start in range(0, len(flat), 65536):
            block = flat[start:start + 65536]
            d = ((block[:, None, :] - colors[None, :, :]) ** 2 * rgb_weights[None, None, :]).sum(axis=2)
            out[start:start + len(block)] = colors[d.argmin(axis=1)]
        return out.reshape(values.shape).astype(np.uint8)

    vertical = nearest((arr[:, 0, :3].astype(np.float32) + arr[:, -1, :3].astype(np.float32)) * 0.5)
    arr[:, 0, :3] = vertical
    arr[:, -1, :3] = vertical
    horizontal = nearest((arr[0, :, :3].astype(np.float32) + arr[-1, :, :3].astype(np.float32)) * 0.5)
    arr[0, :, :3] = horizontal
    arr[-1, :, :3] = horizontal
    arr[:, 0, 3] = arr[:, -1, 3] = 255
    arr[0, :, 3] = arr[-1, :, 3] = 255
    return Image.fromarray(arr, "RGBA")


def process_tile(source: Path, size, seamless=True):
    img = Image.open(source).convert("RGB")
    # Crop to the requested aspect ratio before pixel-art downscaling.
    tw, th = size
    ratio = tw / th
    src_ratio = img.width / img.height
    if src_ratio > ratio:
        new_w = int(img.height * ratio)
        left = (img.width - new_w) // 2
        img = img.crop((left, 0, left + new_w, img.height))
    else:
        new_h = int(img.width / ratio)
        top = (img.height - new_h) // 2
        img = img.crop((0, top, img.width, top + new_h))
    img = img.resize(size, Image.LANCZOS)
    if seamless:
        img = make_seamless(img)
    palette, weights = sg.build_palette(MATERIAL_RAMPS)
    img = sg.quantise(img, palette, weights, alpha_cut=0, dither=True)
    if seamless:
        img = seal_palette_edges(img, palette, weights)
    return img


def prep(source: Path, height: int, dither=True):
    return sg.process(source, target_height=height, dither=dither)


def pack_shelf(name: str, cells, path: Path, alpha_cut=ALPHA_CUT):
    """Shelf-pack variable sized cells into one atlas and register the entries."""
    path.parent.mkdir(parents=True, exist_ok=True)
    cells = [(n, img, pivot, ppu) for n, img, pivot, ppu in cells if img is not None]
    if not cells:
        print(f"  ! {name}: nothing to pack")
        return None
    order = sorted(range(len(cells)), key=lambda i: -cells[i][1].height)
    max_w = 1024
    rows, row, row_h, x = [], [], 0, 0
    for i in order:
        img = cells[i][1]
        if x + img.width > max_w and row:
            rows.append((row, row_h))
            row, row_h, x = [], 0, 0
        row.append(i)
        row_h = max(row_h, img.height)
        x += img.width + 2
    if row:
        rows.append((row, row_h))
    total_h = sum(h + 2 for _, h in rows)
    total_w = max((sum(cells[i][1].width + 2 for i in r) for r, _ in rows), default=1)
    total_w = min(max_w, max(total_w, 64))
    total_h = max(total_h, 64)
    sheet = Image.new("RGBA", (total_w, total_h), (0, 0, 0, 0))
    y = 0
    for r, row_h in rows:
        x = 0
        for i in r:
            n, img, pivot, ppu = cells[i]
            sheet.paste(img, (x, y), img)
            entries.append(dict(name=n, atlas=name, x=x, y=y, w=img.width, h=img.height,
                                pivotX=pivot[0], pivotY=pivot[1], ppu=ppu))
            x += img.width + 2
        y += row_h + 2
    sheet.save(path)
    print(f"  {name} atlas {sheet.size}, {len(cells)} sprites -> {path.name}")
    return sheet


# --------------------------------------------------------------------------
# groups
# --------------------------------------------------------------------------
def build_characters():
    print("characters:")
    cells = []
    for name, source, cw, ch, pivot in CHARACTERS:
        src = sg.RAW / f"{source}.png"
        if not src.exists():
            idle = sg.RAW / "px_survivor_idle.png"
            if idle.exists() and source != "px_survivor_idle":
                print(f"  {name} -> idle sprite (frame art not generated yet)")
                cells.append((name, prep(idle, 144), pivot, PPU))
                continue
            print(f"  ! missing {src.name}; skipping sprite rather than writing a grey block")
            continue
        height = CHAR_TARGET.get(source, 144)
        img = prep(src, height)
        if img.width > cw or img.height > ch:
            img = img.resize((min(cw, img.width), min(ch, img.height)), Image.LANCZOS)
        cells.append((name, img, pivot, PPU))
    pack_shelf("characters", cells, PIX / "characters.png")


def build_props():
    print("props:")
    cells = []
    for name, source, cw, ch, pivot in PROPS:
        src = sg.RAW / f"{source}.png"
        if not src.exists():
            maker = fb.PROP_FALLBACKS.get(name)
            if maker is not None:
                img = maker()
                print(f"  {name} (procedural fallback)")
                cells.append((name, img, pivot, PPU))
                continue
            print(f"  ! missing {src.name}; skipping sprite rather than writing a grey block")
            continue
        img = prep(src, ch)
        if img.width > cw or img.height > ch:
            scale = min(cw / img.width, ch / img.height)
            img = img.resize((int(img.width * scale), int(img.height * scale)), Image.LANCZOS)
        cells.append((name, img, pivot, PPU))
    pack_shelf("props", cells, PIX / "props.png")


def build_items():
    """Build individual 64x64 PNG icons for HUD use; otherwise the C# fallback draws them."""
    print("items:")
    src_dir = sg.RAW / "icons"
    out_dir = PIX / "icons"
    out_dir.mkdir(parents=True, exist_ok=True)
    built = 0
    for name, source in ITEMS:
        src = src_dir / f"{source}.png"
        if not src.exists():
            print(f"  ! missing {src.name} (using in-game pictogram fallback)")
            continue
        img = prep(src, 62, dither=True)
        if img.width > 62:
            scale = 62 / img.width
            img = img.resize((62, max(1, int(round(img.height * scale)))), Image.LANCZOS)
        canvas = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        canvas.paste(img, ((64 - img.width) // 2, (64 - img.height) // 2), img)
        canvas.save(out_dir / f"{name}.png")
        built += 1
    print(f"  {built}/{len(ITEMS)} painted icons -> Art/PixelArt/icons")


def build_tiles():
    print("tiles:")
    TILES.mkdir(parents=True, exist_ok=True)
    palette, weights = sg.build_palette(MATERIAL_RAMPS)
    for name, source, size, seamless in TILE_JOBS:
        src = sg.RAW / f"{source}.png"
        if not src.exists():
            maker = fb.TILE_FALLBACKS.get(name)
            if maker is None:
                print(f"  ! missing {src.name}")
                continue
            img = make_seamless(maker().convert("RGB"))
            img = sg.quantise(img, palette, weights, alpha_cut=0, dither=True)
            img = seal_palette_edges(img, palette, weights)
            img.save(TILES / f"{name}.png")
            print(f"  {name} (seamless procedural fallback)")
            continue
        img = process_tile(src, size, seamless)
        img.save(TILES / f"{name}.png")
        print(f"  {name} {img.size} -> {TILES.name}/{name}.png")


# --------------------------------------------------------------------------
# C# table
# --------------------------------------------------------------------------
CS_HEADER = """// <auto-generated>
//   Generated by UnityProject/Tools/art2d/build_assets.py - do not edit by hand.
//   Sprite rectangles inside Assets/Resources/Art/PixelArt/*.png.
// </auto-generated>
using System.Collections.Generic;
using UnityEngine;

namespace Subsistence
{
    /// <summary>Lookup table for the painted 2D pixel-art atlases.</summary>
    public static class PixelArtAtlas
    {
        public struct Entry
        {
            public string name;
            public string texture;
            public float x, y, w, h;
            public float pivotX, pivotY;
            public float pixelsPerUnit;
        }

        public static readonly Entry[] All =
        {
%s        };

        static readonly Dictionary<string, Entry> Index = BuildIndex();

        static Dictionary<string, Entry> BuildIndex()
        {
            var map = new Dictionary<string, Entry>(All.Length);
            for (int i = 0; i < All.Length; i++) map[All[i].name] = All[i];
            return map;
        }

        public static bool TryGet(string name, out Entry entry) => Index.TryGetValue(name, out entry);

        /// <summary>Creates (and caches) a sprite from an atlas entry.</summary>
        public static Sprite Load(string name)
        {
            if (!TryGet(name, out Entry e)) return null;
            if (Cache.TryGetValue(name, out Sprite cached) && cached != null) return cached;
            Texture2D texture = Resources.Load<Texture2D>("Art/PixelArt/" + e.texture);
            if (texture == null) return null;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            Rect rect = new Rect(e.x, texture.height - e.y - e.h, e.w, e.h);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(e.pivotX, e.pivotY),
                e.pixelsPerUnit, 0, SpriteMeshType.FullRect, Vector4.zero, false);
            sprite.name = name;
            Cache[name] = sprite;
            return sprite;
        }

        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
    }
}
"""


def write_cs():
    if not entries:
        print("no atlas entries collected - keeping the existing PixelArtAtlas.cs")
        return
    rows = []
    for e in entries:
        rows.append(
            "            new Entry {{ name = \"{name}\", texture = \"{atlas}\", "
            "x = {x}f, y = {y}f, w = {w}f, h = {h}f, "
            "pivotX = {px}f, pivotY = {py}f, pixelsPerUnit = {ppu}f }},\n".format(
                name=e["name"], atlas=e["atlas"], x=e["x"], y=e["y"], w=e["w"], h=e["h"],
                px=e["pivotX"], py=e["pivotY"], ppu=e["ppu"]))
    CS_OUT.write_text(CS_HEADER % "".join(rows), encoding="utf-8")
    META.write_text(json.dumps(entries, indent=1), encoding="utf-8")
    print(f"wrote {CS_OUT.relative_to(PROJECT)} ({len(entries)} entries)")


def load_existing_entries():
    """Load atlas rows for groups not rebuilt during this invocation."""
    try:
        data = json.loads(META.read_text(encoding="utf-8"))
        return [entry for entry in data if isinstance(entry, dict) and "name" in entry and "atlas" in entry]
    except (OSError, json.JSONDecodeError):
        return []


def main():
    groups = set(sys.argv[1:] or ["chars", "tiles", "props", "items"])
    valid = {"chars", "tiles", "props", "items"}
    unknown = groups - valid
    if unknown:
        raise SystemExit("unknown build group(s): " + ", ".join(sorted(unknown)))

    prior = load_existing_entries()
    if "chars" in groups:
        build_characters()
    else:
        entries.extend(entry for entry in prior if entry.get("atlas") == "characters")
    if "tiles" in groups:
        build_tiles()
    if "props" in groups:
        build_props()
    else:
        entries.extend(entry for entry in prior if entry.get("atlas") == "props")
    if "items" in groups:
        build_items()
    write_cs()


if __name__ == "__main__":
    main()
