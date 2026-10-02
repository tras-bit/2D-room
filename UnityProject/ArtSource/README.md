# Source art

Put editable source PNGs here, **outside `Assets/Resources`**. The game ships processed atlases and tiles; the original working art remains available for rebuilding but is not loaded as a runtime resource.

- Characters, props and tiles: `raw/`
- Individual inventory icons: `raw/icons/`

Sprites may be transparent RGBA or use a clean flat magenta `#FF00FF` background. The pipeline preserves source alpha, removes magenta spill when present, trims, downsamples, palette-locks and packs the images. Texture tiles are resized to the target size and cross-faded at their edges.

Filenames and the full build contract are documented in [`../ART_PIPELINE.md`](../ART_PIPELINE.md). Run `python3 Tools/art2d/build_assets.py` from the repository root after adding or updating source art.
