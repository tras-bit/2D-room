#!/usr/bin/env python3
"""Dependency-free static sanity checks for the Unity project.

Checks C# delimiter balance, generated atlas metadata against the PNG files,
asset names referenced by PixelArtFactory, and ItemId icon mappings. It does not
replace compiling/importing the project in Unity Editor.
"""
from __future__ import annotations

import re
import struct
import sys
from pathlib import Path

PROJECT = Path(__file__).resolve().parents[2]
SCRIPTS = PROJECT / "Assets" / "Scripts"
FACTORY_PATH = SCRIPTS / "PixelArtFactory.cs"
ATLAS_PATH = SCRIPTS / "PixelArtAtlas.cs"
ART = PROJECT / "Assets" / "Resources" / "Art"
PIX = ART / "PixelArt"
TILES_DIR = ART / "Tiles"
ICONS_DIR = PIX / "icons"

FAIL = 0


def fail(message: str) -> None:
    global FAIL
    FAIL += 1
    print("  FAIL " + message)


def check_balance(path: Path) -> None:
    text = path.read_text(encoding="utf-8", errors="ignore")
    pattern = re.compile(r'"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|//[^\n]*|/\*.*?\*/', re.S)
    stripped = pattern.sub(lambda match: " " * len(match.group(0)), text)
    for opener, closer, label in (("{", "}", "braces"), ("(", ")", "parens"), ("[", "]", "brackets")):
        depth = stripped.count(opener) - stripped.count(closer)
        if depth != 0:
            fail(f"{path.relative_to(PROJECT)}: unbalanced {label} ({depth:+d})")


def png_size(path: Path):
    try:
        header = path.read_bytes()[:24]
        if len(header) < 24 or header[:8] != b"\x89PNG\r\n\x1a\n":
            return None
        return struct.unpack(">II", header[16:24])
    except OSError:
        return None


def extract_body(source: str, signature: str) -> str:
    start = source.find(signature)
    if start < 0:
        return ""
    brace = source.find("{", start)
    if brace < 0:
        return ""
    depth = 0
    for index in range(brace, len(source)):
        if source[index] == "{":
            depth += 1
        elif source[index] == "}":
            depth -= 1
            if depth == 0:
                return source[brace + 1:index]
    return ""


ENTRY_RE = re.compile(
    r'new\s+Entry\s*\{\s*name\s*=\s*"([^"]+)"\s*,\s*'
    r'texture\s*=\s*"([^"]+)"\s*,\s*'
    r'x\s*=\s*([\d.]+)f\s*,\s*y\s*=\s*([\d.]+)f\s*,\s*'
    r'w\s*=\s*([\d.]+)f\s*,\s*h\s*=\s*([\d.]+)f'
)


def atlas_entries(source: str):
    result = []
    for match in ENTRY_RE.finditer(source):
        name, texture = match.group(1), match.group(2)
        x, y, width, height = (int(float(value)) for value in match.groups()[2:])
        result.append((name, texture, x, y, width, height))
    return result


def main() -> int:
    global FAIL
    files = sorted(SCRIPTS.glob("*.cs"))
    print(f"checking {len(files)} C# files")
    for path in files:
        check_balance(path)

    if not FACTORY_PATH.exists() or not ATLAS_PATH.exists():
        fail("PixelArtFactory.cs or PixelArtAtlas.cs is missing")
        print("FAILURES:", FAIL)
        return 1

    factory = FACTORY_PATH.read_text(encoding="utf-8")
    atlas_source = ATLAS_PATH.read_text(encoding="utf-8")
    entries = atlas_entries(atlas_source)
    names = [entry[0] for entry in entries]
    known = set(names)
    if not entries:
        fail("PixelArtAtlas.cs has no readable generated entries")
    if len(names) != len(known):
        fail("PixelArtAtlas.cs contains duplicate sprite names")

    # Every lookup target must be present in the atlas and inside its texture.
    for name in set(re.findall(r'\bAtlas(?:Or)?\("([a-z0-9_]+)"', factory)):
        if name not in known:
            fail(f"atlas sprite referenced by PixelArtFactory is missing: {name}")

    texture_sizes = {}
    for name, texture, x, y, width, height in entries:
        path = PIX / f"{texture}.png"
        if not path.exists():
            fail(f"atlas texture missing: {path.relative_to(PROJECT)}")
            continue
        if texture not in texture_sizes:
            texture_sizes[texture] = png_size(path)
            if texture_sizes[texture] is None:
                fail(f"atlas texture is not a readable PNG: {path.relative_to(PROJECT)}")
        dimensions = texture_sizes[texture]
        if dimensions is None:
            continue
        tex_w, tex_h = dimensions
        if x < 0 or y < 0 or width <= 0 or height <= 0 or x + width > tex_w or y + height > tex_h:
            fail(f"atlas rect outside {path.name}: {name} ({x},{y},{width},{height}) vs {tex_w}x{tex_h}")

    tile_names = {path.stem for path in TILES_DIR.glob("*.png")} if TILES_DIR.exists() else set()
    for name in set(re.findall(r'\bTile\("([a-z0-9_]+)"', factory)):
        if name not in tile_names:
            fail(f"tile texture missing: Art/Tiles/{name}.png")

    # Rust-style run start: only abandoned loot caches may be seeded by the
    # bootstrap; player workbenches/base storage must be crafted and placed.
    bootstrap_path = SCRIPTS / "GameBootstrap.cs"
    if bootstrap_path.exists():
        bootstrap = bootstrap_path.read_text(encoding="utf-8")
        if "CreateWorkbenches(" in bootstrap or "MakeBench(" in bootstrap or "AddComponent<WorkbenchStation>" in bootstrap:
            fail("GameBootstrap must not spawn a pre-placed workbench")
        if "CreateHomeStorage(" in bootstrap or "AddComponent<HomeStorage" in bootstrap:
            fail("GameBootstrap must not spawn player home storage")
        if "AddComponent<ToolCupboard2D>" in bootstrap or "InitializeToolCupboard(" in bootstrap:
            fail("GameBootstrap must not spawn a pre-placed Tool Cupboard")

    building_path = SCRIPTS / "BuildingSystem2D.cs"
    building = building_path.read_text(encoding="utf-8") if building_path.exists() else ""
    clear_body = extract_body(building, "public static void ClearPlayerPlacedObjects")
    if not clear_body or "IsPlayerStorage" not in clear_body or "IsToolCupboard" not in clear_body:
        fail("new runs must clear both player storage and the Tool Cupboard")
    hud_path = SCRIPTS / "GameHUD.cs"
    hud = hud_path.read_text(encoding="utf-8") if hud_path.exists() else ""
    if "BuildingSystem2D.ClearPlayerPlacedObjects();" not in hud:
        fail("GameHUD.StartRun must clear player-placed building objects")

    world_builder_path = SCRIPTS / "WorldBuilder2D.cs"
    world_builder = world_builder_path.read_text(encoding="utf-8") if world_builder_path.exists() else ""
    if not re.search(r"\b\w+\.gameObject\.AddComponent<AmbientDust2D>\(\)", world_builder):
        fail("ambient dust component must be added through its GameObject")

    icon_factory_path = SCRIPTS / "ItemIconFactory.cs"
    icon_factory = icon_factory_path.read_text(encoding="utf-8") if icon_factory_path.exists() else ""
    procedural_body = extract_body(icon_factory, "public static Texture2D Procedural")
    if not re.search(r"\b(?:var|Texture2D)\s+texture\s*=\s*new\s+Texture2D", procedural_body):
        fail("ItemIconFactory.Procedural must declare its generated Texture2D")

    icon_body = extract_body(factory, "public static string IconName")
    icon_names = set(re.findall(r'return\s+"([a-z0-9_]+)"\s*;', icon_body))
    inv_path = SCRIPTS / "InventorySystem.cs"
    inventory = inv_path.read_text(encoding="utf-8") if inv_path.exists() else ""
    enum_body = extract_body(inventory, "public enum ItemId")
    item_ids = [part.strip() for part in enum_body.split(",") if part.strip()]
    for item in item_ids:
        if item == "None":
            continue
        if not re.search(rf"case\s+ItemId\.{re.escape(item)}\s*:", factory):
            fail(f"ItemId.{item} has no icon mapping")

    actual_icons = {path.stem for path in ICONS_DIR.glob("*.png")} if ICONS_DIR.exists() else set()
    missing_icons = sorted(icon for icon in icon_names if icon not in actual_icons)
    if missing_icons:
        fallback = (SCRIPTS / "ItemIconFactory.cs").read_text(encoding="utf-8")
        if "Procedural(ItemId id)" not in fallback:
            fail("painted icons are incomplete and no ItemIconFactory.Procedural fallback exists")
        else:
            print(f"  note: {len(missing_icons)} standalone icon PNGs absent; procedural pixel-art fallback is active")

    print(f"item ids: {len(item_ids)}, atlas sprites: {len(known)}, tiles: {len(tile_names)}, icons: {len(actual_icons)}")
    print("FAILURES:", FAIL)
    return 1 if FAIL else 0


if __name__ == "__main__":
    sys.exit(main())
