# SUBSISTENCE — 2D survival horror

Unity **2022.3.62f2** project with a side-view 2D pixel-art pipeline. The game keeps its 2D camera, physics and public gameplay APIs. No live online game servers are part of the project.

## Project brief website

A local, Russian-language **50-question** design brief is available from the repository root (`index.html`). It has per-question and per-section copy, a full brief copy dialog for manual copying, search and answer filters, one-question focus mode, browser-local autosave, and `.txt`, `.md` and `.json` export/import. Answers are not submitted to any service.

To run it locally:

```bash
python3 -m http.server 4173 --bind 0.0.0.0
```

The previous 20-question version is archived in [`brief-20/`](brief-20/).

## Unity project

Open `UnityProject/` in Unity Hub using Editor **2022.3.62f2**. The game uses URP 2D Renderer, 2D physics, parallax, 2D lights, a pixel-snapped camera, survival systems, inventory, crafting, a Trader NPC and the Watcher enemy.

- Level 0: yellow, patterned office corridors, damp carpet, ceiling panels, false passages and a rare Manila Room.
- Level 1: stained industrial service passages, pipes, doors and elevators.
- Controls, setup and project status: [`UnityProject/README.md`](UnityProject/README.md).
- Art source naming and rebuild instructions: [`UnityProject/ART_PIPELINE.md`](UnityProject/ART_PIPELINE.md).
- Side-view mockups for both levels are in [`docs/preview/`](docs/preview/).

The seven environment materials now use source pixel art. Three of 14 props are newly painted and integrated (crate tier 1, workbench tier 1 and the door); the other 11 use palette-matched fallbacks. Inventory icons still use the in-game pictogram fallback. The pipeline lets remaining art be added without changing gameplay code.

## Rebuild art and run static checks

```bash
python3 -m pip install pillow numpy
python3 UnityProject/Tools/art2d/build_assets.py
python3 UnityProject/Tools/art2d/check_cs.py
```

The static checks do not replace a Unity Editor import/compile. Unity Editor is not available in this environment, so Play Mode and a Windows build have not been verified here.
