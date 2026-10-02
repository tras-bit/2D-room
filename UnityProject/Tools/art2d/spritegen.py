"""Turn painted/detailed source art into crisp, palette-locked pixel sprites.

Pipeline: chroma-key the magenta background -> trim -> downscale -> quantise to
the shared SUBSISTENCE ramps -> clean stray pixels -> write the atlas + meta.
"""
from __future__ import annotations

import json
import math
from pathlib import Path

import numpy as np
from PIL import Image

import palette as pal

PROJECT = Path(__file__).resolve().parents[2]
OUT = PROJECT / "Assets" / "Resources" / "Art"
# Keep source art inside the Unity project so a clean checkout can rebuild every
# asset without relying on a machine-specific /home/user/art-src directory.
RAW = PROJECT / "ArtSource" / "raw"
SOURCE_COPY = OUT / "processed"


# --------------------------------------------------------------------------
# background removal
# --------------------------------------------------------------------------
def chroma_key(img: Image.Image, tol=58.0, spill=2.6) -> Image.Image:
    """Remove magenta while preserving transparency in authored PNG sources.

    Several accepted frames are already PNGs with a transparent background;
    converting those to RGB first would turn every transparent pixel opaque.
    """
    rgba = np.asarray(img.convert("RGBA")).astype(np.float32)
    rgb = rgba[:, :, :3].copy()
    source_alpha = rgba[:, :, 3] / 255.0
    r, g, b = rgb[:, :, 0], rgb[:, :, 1], rgb[:, :, 2]
    # Magenta strength: high red+blue, low green. Non-magenta transparent pixels
    # retain alpha zero from the original image.
    mb = np.minimum(r, b) - g
    key_alpha = np.clip((tol - mb) / max(1.0, tol * 0.45), 0.0, 1.0)
    alpha = source_alpha * key_alpha
    # Despill only visible, partially keyed fringe pixels.
    fringe = (source_alpha > 0) & (key_alpha < 1.0) & (mb > 0)
    if fringe.any():
        f = np.clip(mb / (tol * spill), 0, 1)[fringe][:, None]
        target = np.array([[126.0, 126.0, 126.0]])
        rgb[fringe] = rgb[fringe] * (1 - f * 0.8) + target * (f * 0.8)
    out = np.dstack([np.clip(rgb, 0, 255).astype(np.uint8), (alpha * 255).astype(np.uint8)])
    return Image.fromarray(out, "RGBA")


def trim(img: Image.Image, pad=2) -> Image.Image:
    arr = np.asarray(img)
    alpha = arr[:, :, 3]
    ys, xs = np.nonzero(alpha > 8)
    if len(ys) == 0:
        return img
    y0, y1 = max(0, ys.min() - pad), min(arr.shape[0], ys.max() + 1 + pad)
    x0, x1 = max(0, xs.min() - pad), min(arr.shape[1], xs.max() + 1 + pad)
    return img.crop((x0, y0, x1, y1))


# --------------------------------------------------------------------------
# palette quantisation
# --------------------------------------------------------------------------
def build_palette(ramps=None):
    colors = []
    names = ramps or list(pal.RAMPS.keys())
    for name in names:
        colors.extend(pal.RAMPS[name])
    if ramps is None:
        colors.extend(list(pal.PALETTE.values()))
    else:
        # Material-specific palettes retain shared dark outlines but avoid
        # unrelated accents (skin, acid-green UI, blood red) in world textures.
        colors.extend(pal.PALETTE[name] for name in ("void", "ink", "deep", "shadow", "wire"))
    arr = np.array(colors, dtype=np.float32)
    # deduplicate so distance search stays quick and clean
    uniq = np.unique(arr.astype(np.uint8), axis=0).astype(np.float32)
    # perceptual weighting (green matters most, blue least)
    weights = np.array([0.30, 0.59, 0.11], dtype=np.float32)
    return uniq, weights


def quantise(img: Image.Image, palette, weights, alpha_cut=110, dither=False):
    arr = np.asarray(img).astype(np.float32)
    if arr.shape[2] == 4:
        rgb, alpha = arr[:, :, :3], arr[:, :, 3]
    else:
        rgb, alpha = arr[:, :, :3], np.full(arr.shape[:2], 255.0)
    h, w = alpha.shape
    flat = rgb.reshape(-1, 3)
    # nearest palette colour (weighted euclidean, computed in chunks)
    out = np.zeros_like(flat)
    chunk = 65536
    for start in range(0, flat.shape[0], chunk):
        block = flat[start:start + chunk]
        d = ((block[:, None, :] - palette[None, :, :]) ** 2 * weights[None, None, :]).sum(axis=2)
        idx = d.argmin(axis=1)
        out[start:start + chunk] = palette[idx]
    rgb_q = out.reshape(h, w, 3).astype(np.uint8)
    a = np.where(alpha >= alpha_cut, 255, 0).astype(np.uint8)
    if dither:
        rgb_q = _apply_dither(rgb_q, a, palette, weights)
    return Image.fromarray(np.dstack([rgb_q, a]), "RGBA")


def _apply_dither(rgb, alpha, palette, weights, strength=0.35):
    """Light ordered dithering between the two nearest ramp colours."""
    h, w, _ = rgb.shape
    flat = rgb.reshape(-1, 3).astype(np.float32)
    d = ((flat[:, None, :] - palette[None, :, :]) ** 2 * weights[None, None, :]).sum(axis=2)
    order = np.argsort(d, axis=1)[:, :2]
    d0 = np.take_along_axis(d, order[:, :1], axis=1)[:, 0]
    d1 = np.take_along_axis(d, order[:, 1:2], axis=1)[:, 0]
    ratio = np.clip(d0 / (d1 + 1e-3), 0, 1)
    pat = _bayer(h, w).reshape(-1) / 4.0
    swap = (ratio * strength) > pat
    out = palette[order[:, 0]].astype(np.uint8)
    out[swap] = palette[order[swap, 1]].astype(np.uint8)
    return out.reshape(h, w, 3)


def _bayer(h, w):
    tile = np.array([[0, 2], [3, 1]], dtype=np.float32)
    yy = (np.arange(h)[:, None] % 2)
    xx = (np.arange(w)[None, :] % 2)
    return tile[yy, xx]


# --------------------------------------------------------------------------
# cleanup
# --------------------------------------------------------------------------
def despeckle(img: Image.Image) -> Image.Image:
    """Remove single-pixel islands and close single-pixel holes."""
    arr = np.asarray(img).copy()
    alpha = arr[:, :, 3] > 0
    neighbors = np.zeros_like(alpha, dtype=np.int32)
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            if dx == 0 and dy == 0:
                continue
            shifted = np.roll(np.roll(alpha, dy, axis=0), dx, axis=1)
            neighbors += shifted.astype(np.int32)
    islands = alpha & (neighbors <= 1)
    arr[islands, 3] = 0
    holes = ~alpha & (neighbors >= 6)
    if holes.any():
        # fill with the average of the surrounding pixels
        ys, xs = np.nonzero(holes)
        for y, x in zip(ys, xs):
            acc = np.zeros(3, dtype=np.float32)
            count = 0
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    ny, nx = y + dy, x + dx
                    if 0 <= ny < arr.shape[0] and 0 <= nx < arr.shape[1] and arr[ny, nx, 3] > 0:
                        acc += arr[ny, nx, :3]
                        count += 1
            if count:
                arr[y, x, :3] = (acc / count).astype(np.uint8)
                arr[y, x, 3] = 255
    return Image.fromarray(arr, "RGBA")


def outline(img: Image.Image, color=(16, 19, 18), alpha_min=110) -> Image.Image:
    """Classic dark 1px outline around the silhouette."""
    arr = np.asarray(img).copy()
    alpha = arr[:, :, 3] > alpha_min
    grown = np.zeros_like(alpha)
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            grown |= np.roll(np.roll(alpha, dy, axis=0), dx, axis=1)
    edge = grown & ~alpha
    arr[edge] = (color[0], color[1], color[2], 255)
    return Image.fromarray(arr, "RGBA")


# --------------------------------------------------------------------------
# high level
# --------------------------------------------------------------------------
def process(path: Path, target_height: int, dither=False, do_outline=False,
            trim_pad=1, tol=58.0, alpha_cut=140):
    img = Image.open(path)
    img = chroma_key(img, tol=tol)
    img = trim(img, pad=trim_pad)
    scale = target_height / img.height
    new_w = max(1, int(round(img.width * scale)))
    small = img.resize((new_w, target_height), Image.LANCZOS)
    palette, weights = build_palette()
    small = quantise(small, palette, weights, alpha_cut=alpha_cut, dither=dither)
    small = despeckle(small)
    if do_outline:
        small = outline(small)
    return small


def save(img: Image.Image, name: str, subdir="px"):
    out_dir = OUT / subdir
    out_dir.mkdir(parents=True, exist_ok=True)
    copy_dir = SOURCE_COPY / subdir
    copy_dir.mkdir(parents=True, exist_ok=True)
    img.save(out_dir / f"{name}.png")
    img.resize((img.width * 4, img.height * 4), Image.NEAREST).save(copy_dir / f"{name}_x4.png")
    return out_dir / f"{name}.png"


def grey_check(img: Image.Image):
    """Report average colour to catch magenta leftovers."""
    arr = np.asarray(img).astype(np.float32)
    m = arr[:, :, 3] > 0
    if not m.any():
        return (0, 0, 0)
    return tuple(int(v) for v in arr[m][:, :3].mean(axis=0))
