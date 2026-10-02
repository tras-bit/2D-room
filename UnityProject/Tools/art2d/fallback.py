#!/usr/bin/env python3
"""Procedural floor-plan art used when a painted source is not available yet.

Everything here is seamless by construction (periodic noise) and palette locked,
so the game always renders a coherent corridor instead of grey placeholder
blocks even before the painted atlases are generated.
"""
from __future__ import annotations

import numpy as np
from PIL import Image

import palette as pal
from draw import Paint

LINE = (22, 26, 23)


def _periodic(size: int, freq: int, seed: int, phase: float = 0.0) -> np.ndarray:
    x = np.arange(size, dtype=np.float32) / size
    rng = np.random.default_rng(seed)
    out = np.zeros(size, dtype=np.float32)
    for k in range(1, min(freq, 14) + 1):
        amp = 1.0 / (k ** 1.35)
        out += amp * np.sin(2 * np.pi * k * x + rng.random() * 6.283 + phase)
    out -= out.min()
    return (out / (out.max() + 1e-6)).astype(np.float32)


def _periodic_2d(w: int, h: int, freq: int, seed: int) -> np.ndarray:
    a = _periodic(w, freq, seed)[None, :]
    b = _periodic(h, freq, seed + 77)[:, None]
    return np.clip(a * 0.55 + b * 0.45, 0, 1)


def _ramp_array(name: str) -> np.ndarray:
    return np.array(pal.RAMPS[name], dtype=np.float32)


def _to_img(arr: np.ndarray) -> Image.Image:
    rgb = np.clip(arr, 0, 255).astype(np.uint8)
    alpha = np.full(rgb.shape[:2] + (1,), 255, dtype=np.uint8)
    return Image.fromarray(np.dstack([rgb, alpha]), "RGBA")


def _pick(ramp: np.ndarray, t: float):
    colors = ramp
    t = min(max(t, 0.0), 1.0)
    return colors[int(round(t * (len(colors) - 1)))]


def quantise_rgb(arr: np.ndarray, ramp_names=None) -> np.ndarray:
    names = ramp_names or list(pal.RAMPS.keys())
    colors = []
    for n in names:
        colors.extend(pal.RAMPS[n])
    if ramp_names is None:
        colors.extend(list(pal.PALETTE.values()))
    else:
        # Keep silhouette/structure shadows, but never quantise a wallpaper or
        # concrete tile into an unrelated skin, acid or blood accent.
        colors.extend(pal.PALETTE[n] for n in ("void", "ink", "deep", "shadow", "wire"))
    table = np.unique(np.array(colors, dtype=np.uint8), axis=0).astype(np.float32)
    h, w, _ = arr.shape
    flat = arr.reshape(-1, 3)
    weights = np.array([0.30, 0.59, 0.11], dtype=np.float32)
    out = np.zeros_like(flat)
    for start in range(0, flat.shape[0], 65536):
        block = flat[start:start + 65536]
        d = ((block[:, None, :] - table[None, :, :]) ** 2 * weights[None, None, :]).sum(axis=2)
        out[start:start + block.shape[0]] = table[d.argmin(axis=1)]
    return out.reshape(h, w, 3)


def _dither(a: np.ndarray, b: np.ndarray, ratio: np.ndarray, seed=3) -> np.ndarray:
    """Blend two colour arrays with a 2x2 ordered dither."""
    h, w, _ = a.shape
    tile = np.array([[0, 2], [3, 1]], dtype=np.float32)
    pat = (tile[np.arange(h)[:, None] % 2, np.arange(w)[None, :] % 2] / 4.0)[:, :, None]
    return np.where((1 - ratio)[:, :, None] > pat, a, b)


# --------------------------------------------------------------------------
# tiles
# --------------------------------------------------------------------------
def wall_level0(w=384, h=384) -> Image.Image:
    """Aged yellow damask wallpaper: one motif per 2 world units, heavy grime."""
    ramp = _ramp_array("paper")
    xs = np.arange(w)[None, :]
    ys = np.arange(h)[:, None]
    period = 192.0                       # 2 world units at 96 ppu
    mx = ((xs + period * .5) % period) - period * .5
    my = ((ys + period * .35) % period) - period * .5
    r = np.sqrt((mx / (period * .30)) ** 2 + (my / (period * .40)) ** 2)
    motif = np.clip(1.0 - np.abs(r - 0.85) * 2.6, 0, 1) * 0.85
    core = np.clip(1.0 - r * 2.2, 0, 1) * 0.55
    vines = np.clip(1.0 - np.abs(np.sin(mx / period * 6.283) * 0.5 +
                                 np.cos(my / period * 6.283) * 0.5 - 0.2) * 5.0, 0, 1) * 0.28
    seam = (((xs + 4) % period) < 3).astype(np.float32)
    grime = _periodic_2d(w, h, 3, 11)
    streak = np.clip(_periodic_2d(w, h, 9, 41) - 0.55, 0, 1) * 1.6
    vign = (ys / h) * 0.10
    lum = 0.26 + motif * 0.20 + core * 0.09 + vines * 0.05 - grime * 0.26 - seam * 0.16 - streak * 0.16 - vign
    low = np.floor(lum * (len(ramp) - 1)).astype(int).clip(0, len(ramp) - 1)
    frac = lum * (len(ramp) - 1) - low
    arr = _dither(ramp[low], ramp[np.clip(low + 1, 0, len(ramp) - 1)], 1 - frac)
    # damp bloom in the lower half of the tile
    damp = np.clip(_periodic_2d(w, h, 2, 71) - 0.45, 0, 1) * (1 - ys / h) * 1.4
    arr = arr * (1 - np.clip(damp, 0, .6)[:, :, None]) + \
        np.array([34, 31, 21], dtype=np.float32) * np.clip(damp, 0, .6)[:, :, None]
    return _to_img(quantise_rgb(arr, ["paper", "olive"]))


def carpet_level0(w=384, h=96) -> Image.Image:
    """Damp short-pile office carpet, one dark stain pattern per 2 units."""
    ramp = _ramp_array("paper")
    xs = np.arange(w)[None, :]
    ys = np.arange(h)[:, None]
    weave = ((xs + (ys % 4) * 2) % 5 < 2).astype(np.float32)
    thread = ((ys % 3) == 0).astype(np.float32)
    pile = _periodic_2d(w, h, 12, 21)
    damp = _periodic_2d(w, h, 3, 33)
    row = ys / max(1, h - 1)
    lum = 0.20 + row * 0.26 + pile * 0.10 + weave * 0.035 + thread * 0.02 - damp * 0.20
    low = np.floor(lum * (len(ramp) - 1)).astype(int).clip(0, len(ramp) - 1)
    frac = lum * (len(ramp) - 1) - low
    arr = _dither(ramp[low], ramp[np.clip(low + 1, 0, len(ramp) - 1)], 1 - frac)
    # faint large checker pattern, only in the upper part of the strip
    pattern = ((((xs // 96) + (ys // 48)) % 2) == 0) & (ys < h * 0.55)
    arr[pattern] = arr[pattern] * 0.88 + 0.12 * np.array([52, 42, 24], dtype=np.float32)
    return _to_img(quantise_rgb(arr, ["paper", "olive", "cloth"]))


def ceiling_level0(w=384, h=48) -> Image.Image:
    """Yellowed suspended ceiling: panels split by dark rails every 2 units."""
    ramp = _ramp_array("cloth")
    xs = np.arange(w)[None, :]
    ys = np.arange(h)[:, None]
    pins = (np.random.default_rng(9).random((h, w)) < 0.10).astype(np.float32)
    rail = (((xs + 2) % 192) < 5).astype(np.float32)
    panel = (((xs + 2) % 192) > 186).astype(np.float32)
    stain = _periodic_2d(w, h, 4, 41)
    lum = 0.34 + (ys / h) * 0.16 - pins * 0.13 - rail * 0.34 - stain * 0.18 - panel * 0.12
    low = np.floor(lum * (len(ramp) - 1)).astype(int).clip(0, len(ramp) - 1)
    frac = lum * (len(ramp) - 1) - low
    arr = _dither(ramp[low], ramp[np.clip(low + 1, 0, len(ramp) - 1)], 1 - frac)
    return _to_img(quantise_rgb(arr, ["cloth", "yellow", "grey"]))


def wall_level1(w=384, h=384) -> Image.Image:
    """Stained concrete service wall, panel seams every 2 units."""
    ramp = _ramp_array("grey")
    xs = np.arange(w)[None, :]
    ys = np.arange(h)[:, None]
    panel = ((((xs + 3) % 192) < 3) | (((ys + 5) % 192) < 3)).astype(np.float32)
    rough = _periodic_2d(w, h, 10, 7)
    stain = _periodic_2d(w, h, 3, 17)
    rust = np.clip(_periodic_2d(w, h, 5, 29) - 0.60, 0, 1) * 2.4
    drip = np.clip(_periodic_2d(w, h, 11, 61) - 0.55, 0, 1) * (ys / h) * 1.6
    lum = 0.28 + rough * 0.18 - panel * 0.20 - stain * 0.16 - drip * 0.10
    low = np.floor(lum * (len(ramp) - 1)).astype(int).clip(0, len(ramp) - 1)
    frac = lum * (len(ramp) - 1) - low
    arr = _dither(ramp[low], ramp[np.clip(low + 1, 0, len(ramp) - 1)], 1 - frac)
    rust_ramp = _ramp_array("rust")
    m = rust > 0.30
    k = np.clip(rust[m], 0, 1)[:, None]
    idx = np.clip((k * (len(rust_ramp) - 1)).astype(int), 0, len(rust_ramp) - 1)[:, 0]
    arr[m] = arr[m] * (1 - k * 0.5) + rust_ramp[idx] * k * 0.5
    return _to_img(quantise_rgb(arr, ["grey", "rust", "olive", "teal"]))


def floor_level1(w=384, h=96) -> Image.Image:
    ramp = _ramp_array("grey")
    xs = np.arange(w)[None, :]
    ys = np.arange(h)[:, None]
    slab = ((xs + 1) % 128 < 3).astype(np.float32)
    grit = (np.random.default_rng(13).random((h, w)) < 0.13).astype(np.float32)
    row = ys / max(1, h - 1)
    lum = 0.24 + row * 0.24 + _periodic_2d(w, h, 8, 23) * 0.12 - slab * 0.22 - grit * 0.10
    low = np.floor(lum * (len(ramp) - 1)).astype(int).clip(0, len(ramp) - 1)
    frac = lum * (len(ramp) - 1) - low
    arr = _dither(ramp[low], ramp[np.clip(low + 1, 0, len(ramp) - 1)], 1 - frac)
    return _to_img(quantise_rgb(arr, ["grey", "rust", "teal"]))


def ceiling_level1(w=384, h=48) -> Image.Image:
    ramp = _ramp_array("grey")
    xs = np.arange(w)[None, :]
    ys = np.arange(h)[:, None]
    rivet = (((xs % 64) < 3) & ((ys % 28) < 3)).astype(np.float32)
    beam = (((xs + 8) % 192) < 14).astype(np.float32)
    lum = 0.26 + (ys / h) * 0.08 - rivet * 0.18 - beam * 0.14 + _periodic_2d(w, h, 7, 31) * 0.10
    low = np.floor(lum * (len(ramp) - 1)).astype(int).clip(0, len(ramp) - 1)
    frac = lum * (len(ramp) - 1) - low
    arr = _dither(ramp[low], ramp[np.clip(low + 1, 0, len(ramp) - 1)], 1 - frac)
    return _to_img(quantise_rgb(arr, ["grey", "rust", "teal"]))


def manila_wall(w=384, h=384) -> Image.Image:
    ramp = _ramp_array("cloth")
    xs = np.arange(w)[None, :]
    ys = np.arange(h)[:, None]
    stripe = (((xs % 64) < 22) & ((ys % 192) < 88)).astype(np.float32)
    fleur = (np.abs(((xs + 6) % 192) - 96) < 3).astype(np.float32)
    stain = _periodic_2d(w, h, 3, 51)
    lum = 0.42 + stripe * 0.10 - fleur * 0.16 - stain * 0.22 + _periodic_2d(w, h, 7, 61) * 0.06 - (ys / h) * 0.06
    low = np.floor(lum * (len(ramp) - 1)).astype(int).clip(0, len(ramp) - 1)
    frac = lum * (len(ramp) - 1) - low
    arr = _dither(ramp[low], ramp[np.clip(low + 1, 0, len(ramp) - 1)], 1 - frac)
    return _to_img(quantise_rgb(arr, ["cloth", "yellow", "grey"]))


# --------------------------------------------------------------------------
# props (drawn with Paint, y-up, ground on y=0)
# --------------------------------------------------------------------------
def crate(tier=1, w=104, h=96) -> Image.Image:
    p = Paint(w, h)
    face = {1: (146, 108, 64), 2: (118, 112, 82), 3: (100, 108, 86)}[tier]
    edge = {1: (176, 138, 82), 2: (146, 136, 94), 3: (132, 140, 106)}[tier]
    dark = {1: (78, 55, 34), 2: (66, 62, 46), 3: (56, 62, 50)}[tier]
    tape = (198, 168, 104) if tier != 3 else (154, 124, 62)
    p.rect(3, 0, w - 6, h - 10, dark)
    p.rect(5, 2, w - 12, h - 16, face)
    # top flaps
    p.rect(6, h - 20, w // 2 - 8, 14, edge)
    p.rect(w // 2 + 2, h - 20, w // 2 - 9, 14, face)
    p.rect(6, h - 8, w - 14, 5, edge)
    # packing tape
    p.rect(w // 2 - 5, 4, 10, h - 20, tape)
    p.rect(w // 2 - 3, 6, 3, h - 24, (230, 199, 130))
    # shipping label
    p.rect(10, 24, 32, 22, (206, 190, 152))
    p.rect(11, 25, 30, 1, (238, 226, 186))
    for i, y in enumerate((28, 33, 38)):
        p.rect(14, y, 22 - i * 5, 2, (96, 78, 54))
    # scuffs and damp bloom
    p.rect(8, 6, 20, 4, dark)
    p.rect(w - 26, 10, 16, 5, dark)
    p.noise(6, 2, w - 14, h - 20, dark, 0.05, seed=tier * 3)
    p.rect(3, 0, w - 6, 2, LINE)
    p.rect(3, h - 2, w - 6, 2, LINE)
    p.rect(3, 0, 2, h, LINE)
    p.rect(w - 5, 0, 2, h, LINE)
    return _to_img(quantise_rgb(np.asarray(p.to_image())[:, :, :3].astype(np.float32),
                                ["yellow", "cloth", "olive", "grey"]))


def workbench(tier=1, w=128, h=112) -> Image.Image:
    p = Paint(w, h)
    wood = (112, 80, 46) if tier == 1 else (74, 84, 72)
    metal = (62, 72, 68)
    top = (150, 122, 72) if tier == 1 else (96, 106, 88)
    p.rect(6, 62, w - 12, 12, wood)
    p.rect(6, 72, w - 12, 3, top)
    p.rect(10, 8, 8, 56, metal)
    p.rect(w - 18, 8, 8, 56, metal)
    p.rect(10, 46, w - 20, 4, metal)
    p.rect(24, 76, 26, 16, (92, 96, 78))
    p.rect(60, 76, 30, 20, metal)
    p.rect(64, 80, 22, 12, (176, 110, 52) if tier > 1 else (128, 134, 96))
    p.rect(20, 30, 34, 14, metal)
    p.rect(24, 33, 26, 8, (52, 60, 54))
    p.rect(74, 28, 26, 18, (86, 74, 52))
    p.noise(8, 8, w - 16, 62, (40, 44, 38), 0.06, seed=tier)
    for x in range(8, w - 8, 18):
        p.rect(x, 40, 16, 1, (44, 48, 42))
    p.rect(4, 0, w - 8, 3, LINE)
    p.rect(4, 0, 2, h - 8, LINE)
    p.rect(w - 6, 0, 2, h - 8, LINE)
    p.rect(4, h - 8, w - 8, 2, LINE)
    return _to_img(quantise_rgb(np.asarray(p.to_image())[:, :, :3].astype(np.float32),
                                ["olive", "grey", "rust", "yellow"]))


def door(w=150, h=224) -> Image.Image:
    p = Paint(w, h)
    frame = (58, 62, 54)
    leaf = (96, 58, 38)
    p.rect(4, 0, w - 8, h - 4, frame)
    p.rect(14, 8, w - 28, h - 26, leaf)
    p.rect(18, 12, w - 36, h - 34, (74, 44, 30))
    p.rect(24, 22, w - 48, h - 52, (58, 40, 30))
    for i in range(6):
        p.rect(24, 30 + i * 26, w - 48, 3, (46, 32, 24))
    p.rect(w - 40, h // 2 - 10, 8, 22, (196, 132, 57))
    p.rect(w - 38, h // 2 - 8, 4, 18, (234, 184, 95))
    p.rect(20, h - 30, w - 40, 6, (150, 96, 46))
    for y in (34, 74, 120, 170):
        p.rect(12, y, 3, 3, (188, 176, 120))
        p.rect(w - 15, y, 3, 3, (188, 176, 120))
    p.noise(16, 10, w - 32, h - 30, (40, 30, 24), 0.07, seed=7)
    p.rect(2, 0, w - 4, 3, LINE)
    p.rect(2, 0, 3, h - 2, LINE)
    p.rect(w - 5, 0, 3, h - 2, LINE)
    p.rect(2, h - 3, w - 4, 3, LINE)
    return _to_img(quantise_rgb(np.asarray(p.to_image())[:, :, :3].astype(np.float32),
                                ["rust", "grey", "yellow", "olive"]))


def elevator(open_door=False, w=150, h=224) -> Image.Image:
    p = Paint(w, h)
    frame = (48, 54, 50)
    steel = (96, 106, 92)
    p.rect(2, 0, w - 4, h - 4, frame)
    p.rect(8, 6, w - 16, h - 20, (60, 70, 62))
    if open_door:
        p.rect(30, 10, w - 60, h - 30, (14, 18, 17))
        p.rect(36, 14, w - 72, h - 40, (26, 34, 30))
        for i in range(5):
            p.rect(40, 20 + i * 28, w - 80, 6, (58, 66, 54))
        p.rect(14, 12, 14, h - 32, steel)
        p.rect(w - 28, 12, 14, h - 32, steel)
    else:
        p.rect(18, 10, w // 2 - 22, h - 30, steel)
        p.rect(w // 2 + 4, 10, w // 2 - 22, h - 30, (86, 96, 84))
        p.rect(w // 2 - 3, 10, 6, h - 30, (34, 40, 36))
        for i in range(4):
            p.rect(24, 26 + i * 42, w // 2 - 34, 4, (72, 82, 70))
            p.rect(w // 2 + 10, 26 + i * 42, w // 2 - 34, 4, (64, 74, 64))
    p.rect(w - 30, h // 2 - 6, 6, 12, (188, 138, 60))
    p.rect(w - 28, h // 2 - 4, 2, 8, (238, 206, 120))
    p.rect(4, h - 14, w - 8, 8, (66, 74, 64))
    p.rect(4, h - 10, w - 8, 3, (128, 138, 112))
    p.rect(2, 0, w - 4, 3, LINE)
    p.rect(2, 0, 3, h - 2, LINE)
    p.rect(w - 5, 0, 3, h - 2, LINE)
    p.rect(2, h - 3, w - 4, 3, LINE)
    return _to_img(quantise_rgb(np.asarray(p.to_image())[:, :, :3].astype(np.float32),
                                ["grey", "teal", "rust", "yellow"]))


def lamp(w=150, h=44) -> Image.Image:
    p = Paint(w, h)
    p.rect(4, 26, w - 8, 12, (52, 56, 48))
    p.rect(4, 34, w - 8, 3, (86, 92, 78))
    p.rect(14, 18, w - 28, 9, (176, 168, 120))
    p.rect(18, 20, w - 36, 4, (232, 224, 168))
    p.rect(10, 14, w - 20, 4, (60, 64, 54))
    for x in range(10, w - 10, 24):
        p.rect(x, 12, 16, 2, (44, 48, 42))
    p.rect(4, 8, w - 8, 2, LINE)
    return _to_img(quantise_rgb(np.asarray(p.to_image())[:, :, :3].astype(np.float32),
                                ["yellow", "grey", "cloth"]))


def pipe(w=192, h=28) -> Image.Image:
    p = Paint(w, h)
    p.rect(0, 8, w, 12, (58, 62, 54))
    p.rect(0, 16, w, 3, (96, 102, 84))
    p.rect(0, 6, w, 2, (34, 38, 34))
    for x in range(8, w, 48):
        p.rect(x, 4, 5, 20, (70, 74, 64))
        p.rect(x + 1, 6, 2, 16, (108, 112, 92))
    return _to_img(quantise_rgb(np.asarray(p.to_image())[:, :, :3].astype(np.float32),
                                ["grey", "rust", "olive"]))


def outlet(w=40, h=34) -> Image.Image:
    p = Paint(w, h)
    p.rect(4, 2, w - 8, h - 4, (188, 172, 122))
    p.rect(6, 4, w - 12, h - 8, (214, 198, 146))
    p.rect(12, 12, 5, 10, (46, 48, 42))
    p.rect(w - 17, 12, 5, 10, (46, 48, 42))
    p.rect(12, 12, 5, 2, (92, 96, 84))
    p.rect(4, h - 4, w - 8, 2, LINE)
    return _to_img(quantise_rgb(np.asarray(p.to_image())[:, :, :3].astype(np.float32),
                                ["cloth", "yellow", "grey"]))


def placard(w=72, h=48) -> Image.Image:
    p = Paint(w, h)
    p.rect(2, 2, w - 4, h - 6, (28, 34, 31))
    p.rect(5, 5, w - 10, h - 12, (52, 74, 60))
    p.rect(8, h - 16, w - 16, 4, (196, 172, 96))
    p.rect(8, h - 24, w - 24, 3, (150, 150, 108))
    p.rect(8, h - 31, w - 30, 3, (112, 134, 100))
    p.rect(w - 18, 10, 9, 9, (196, 88, 44))
    p.rect(w - 16, 12, 5, 5, (28, 34, 31))
    return _to_img(quantise_rgb(np.asarray(p.to_image())[:, :, :3].astype(np.float32),
                                ["olive", "yellow", "rust", "teal"]))


def keycard(w=44, h=44) -> Image.Image:
    p = Paint(w, h)
    p.rect(8, 8, w - 16, h - 16, (34, 92, 58))
    p.rect(10, 10, w - 20, h - 20, (56, 132, 78))
    p.rect(13, h - 22, w - 26, 4, (124, 178, 96))
    p.rect(13, 16, w - 30, 3, (28, 74, 48))
    p.rect(13, 23, w - 34, 3, (28, 74, 48))
    p.rect(w - 22, 12, 4, 7, (206, 194, 126))
    p.rect(6, 6, w - 12, 2, LINE)
    p.rect(6, 6, 2, h - 12, LINE)
    p.rect(w - 8, 6, 2, h - 12, LINE)
    p.rect(6, h - 8, w - 12, 2, LINE)
    return _to_img(quantise_rgb(np.asarray(p.to_image())[:, :, :3].astype(np.float32),
                                ["teal", "olive", "cloth"]))


TILE_FALLBACKS = {
    "wall_level0": lambda: wall_level0(384, 384),
    "carpet_level0": lambda: carpet_level0(384, 96),
    "ceiling_level0": lambda: ceiling_level0(384, 48),
    "wall_level1": lambda: wall_level1(384, 384),
    "floor_level1": lambda: floor_level1(384, 96),
    "ceiling_level1": lambda: ceiling_level1(384, 48),
    "manila_wall": lambda: manila_wall(384, 384),
}

PROP_FALLBACKS = {
    "crate_1": lambda: crate(1),
    "crate_2": lambda: crate(2),
    "crate_3": lambda: crate(3),
    "workbench_1": lambda: workbench(1),
    "workbench_2": lambda: workbench(2),
    "workbench_3": lambda: workbench(3),
    "door": door,
    "elevator_closed": lambda: elevator(False),
    "elevator_open": lambda: elevator(True),
    "lamp": lamp,
    "pipe": pipe,
    "outlet": outlet,
    "placard": placard,
    "keycard": keycard,
}
