# Item icon source PNGs

Optional painted 64×64 HUD source images belong here. Use a transparent background (or a clean magenta key) and the `source` filenames from the `ITEMS` list in `../../Tools/art2d/build_assets.py`, for example:

- `px_item_flashlight.png`
- `px_item_cloth.png`
- `px_item_metal.png`
- `px_item_food.png`
- `px_item_pistol.png`
- `px_item_jacket.png`

The builder fits every source inside a 62×62 area without cropping long objects and writes `Assets/Resources/Art/PixelArt/icons/<item-id>.png`. Until an image is present, the HUD uses its in-code icon fallback.
