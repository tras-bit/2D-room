# Changelog

## alpha-1.0.0 — 2026-09-30

- First Unity-only field-test slice for the requested 2D pixel-art survival horror.
- Added runtime-generated service-concourse level, sprite art, sprite-frame clips, player movement and jump.
- Added pickup resources, hunger/thirst/health, watcher patrol/chase/hit/stun, bandages, objective and powered exit.
- Added title menu, controls, audio/fullscreen settings, pause, victory/defeat and restart flow.
- Added original generated theme, ambience and one-shot WAV palette.
- Pinned the Unity editor project to 2022.3.62f2 and created an Editor bootstrap for a launch scene/build-settings entry.
- Removed the playable browser-game prototype. Kept only the separately requested 20-question brief website.

### Known limitations for this alpha

- Unity Editor is not installed in the coding environment, so Unity import, compilation, and a native player build could not be run here.
- Art is procedural placeholder pixel art that establishes the submitted style and animation pipeline; hand-polished sprite sheets, more biomes/enemies, saves, multiplayer, and final URP lighting remain future production work.
- The release asset is a Unity project source archive, not a Windows executable.
