"""Contact sheets for quick visual review of generated pixel art."""
from __future__ import annotations

import sys
from pathlib import Path

from PIL import Image, ImageDraw

sys.path.insert(0, str(Path(__file__).resolve().parent))

import characters  # noqa: E402


def sheet(items, path, scale=3, bg=(22, 25, 22)):
    """items: list of (caption, PIL image)"""
    pad = 10
    cols = min(6, len(items))
    rows = (len(items) + cols - 1) // cols
    cell_w = max(im.width for _, im in items) * scale + pad * 2
    cell_h = max(im.height for _, im in items) * scale + pad * 2 + 16
    sheet_img = Image.new("RGB", (cols * cell_w, rows * cell_h), bg)
    draw = ImageDraw.Draw(sheet_img)
    for index, (caption, im) in enumerate(items):
        cx = (index % cols) * cell_w
        cy = (index // cols) * cell_h
        big = im.resize((im.width * scale, im.height * scale), Image.NEAREST)
        sheet_img.paste(big, (cx + pad, cy + pad), big)
        draw.text((cx + pad, cy + pad + big.height + 2), caption, fill=(180, 190, 170))
    sheet_img.save(path)
    print("wrote", path, sheet_img.size)


def main():
    out = Path(sys.argv[1] if len(sys.argv) > 1 else "/tmp/art-preview")
    out.mkdir(parents=True, exist_ok=True)
    items = []
    for frame in range(5):
        items.append((f"frame {frame}", characters.render_survivor(frame).to_image()))
    sheet(items, out / "survivor_frames.png")

    gear_items = []
    variants = [
        ("base", {}),
        ("jacket", {"chest": "jacket"}),
        ("vest", {"chest": "vest"}),
        ("hazmat", {"chest": "hazmat"}),
        ("helmet", {"head": "helmet"}),
        ("pack", {"back": "pack"}),
        ("pants+boots", {"legs": "pants", "feet": "boots"}),
        ("full kit", {"chest": "jacket", "legs": "pants", "feet": "boots", "head": "helmet", "back": "pack"}),
        ("hurt", {}),
    ]
    for name, gear in variants:
        gear_items.append((name, characters.render_survivor(0, gear, hurt=(name == "hurt")).to_image()))
    sheet(gear_items, out / "survivor_gear.png")


if __name__ == "__main__":
    main()
