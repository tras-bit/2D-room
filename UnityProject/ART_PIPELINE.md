# SUBSISTENCE — 2D pixel-art pipeline

The game stays a **2D side-view** project. Art is authored as detailed pixel art, then processed into a shared limited palette and packed into runtime atlases. Source files live inside the repo but outside `Resources`, so Unity can rebuild them without shipping the original working images as runtime assets.

## Asset locations

| Path | Contents |
| --- | --- |
| `ArtSource/raw/` | Authored source sprites and environment materials |
| `ArtSource/raw/icons/` | Individual inventory-icon sources (optional until painted) |
| `Tools/art2d/` | Palette, pixel processing, procedural fallbacks, atlas build and static checks |
| `Assets/Resources/Art/PixelArt/characters.png` | Survivor, Watcher and Trader sprites |
| `Assets/Resources/Art/PixelArt/props.png` | Doors, elevators, workbenches, crates, lamp, pipe, outlet, placard and access card |
| `Assets/Resources/Art/PixelArt/icons/` | Painted 64×64 HUD icons; missing images use `ItemIconFactory.Procedural` |
| `Assets/Resources/Art/Tiles/` | Seven repeating Level 0, Level 1 and Manila Room materials |
| `Assets/Scripts/PixelArtAtlas.cs` | Generated atlas rectangles and pivots |
| `Tools/art2d/atlas_manifest.json` | Generated JSON copy of atlas metadata |

## Build and checks

The builder uses Python 3, Pillow and NumPy:

```bash
python3 -m pip install pillow numpy
python3 UnityProject/Tools/art2d/build_assets.py
python3 UnityProject/Tools/art2d/check_cs.py
```

You can rebuild only selected groups:

```bash
python3 UnityProject/Tools/art2d/build_assets.py chars
python3 UnityProject/Tools/art2d/build_assets.py props
python3 UnityProject/Tools/art2d/build_assets.py tiles
python3 UnityProject/Tools/art2d/build_assets.py items
```

`check_cs.py` has no third-party dependencies. It checks C# delimiter balance, lookup names, PNG atlas bounds, tile files and item-icon fallbacks. It is not a substitute for importing and compiling the project in Unity Editor.

## Source naming

The names below are the builder's file contract. A source can have a transparent background or a flat magenta `#FF00FF` background for sprites; textures should be seamless or will be cross-faded at the edges.

- Characters in `ArtSource/raw/`: `px_survivor_idle.png`, `px_survivor_walkA.png`, `px_survivor_walkB.png`, `px_survivor_jump.png`, `px_survivor_attack.png`, `px_watcher_idle.png`, `px_watcher_attack.png`, `px_trader.png`.
- World props in `ArtSource/raw/`: `px_crate_1.png`–`px_crate_3.png`, `px_bench_1.png`–`px_bench_3.png`, `px_door.png`, `px_elevator_closed.png`, `px_elevator_open.png`, `px_outlet.png`, `px_placard.png`, `px_lamp.png`, `px_pipe.png`, `px_keycard.png`.
- Tiles in `ArtSource/raw/`: `tile_wall_l0.png`, `tile_carpet_l0.png`, `tile_ceiling_l0.png`, `tile_wall_l1.png`, `tile_floor_l1.png`, `tile_ceiling_l1.png`, `tile_manila.png`.
- Item pictures in `ArtSource/raw/icons/`: filenames from the `ITEMS` manifest in `Tools/art2d/build_assets.py` (for example `px_item_flashlight.png`, `px_item_cloth.png`).

When a prop or material source is not ready, the build uses a palette-locked procedural fallback instead of a blank/grey sprite. For icons, the game draws an in-code pictogram until a painted PNG exists; no blank placeholder file is generated.

## Pixel scale and rendering

- **96 pixels per world unit** across the game.
- Survivor sprite height is about 144 px; pivot is centered at the feet.
- Wall/manila textures are 384×384 px (4×4 world units); floor and carpet are 384×96 px; ceiling strips are 384×48 px. Pattern repeats are sized for 2 world units.
- Atlas textures use point filtering. World tile renderers repeat at 96 PPU; the camera is orthographic and snaps to the pixel grid.

## Current state / next art pass

- Character art is packed from the source PNGs; the five survivor motion poses, two Watcher poses and Trader are present.
- The Level 0 wallpaper tile is painted pixel art. The remaining six environment textures and all 14 props currently have same-palette fallback art and can be replaced just by adding their source PNGs under `ArtSource/raw/`.
- Inventory icons currently use the in-game procedural pictogram fallback. The source folder is ready for painted 64×64 item icons.
- Gear-specific survivor variants are not yet authored; the game safely uses the base character until those frames are added.
