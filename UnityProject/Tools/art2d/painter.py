"""Tiny numpy pixel-art painter used by all SUBSISTENCE 2D generators.

The painter works on explicit pixel masks so that garments, gear and details can
be re-shaded with one consistent light setup - that is what keeps procedurally
generated sprites looking hand authored instead of gradient filled.
"""
from __future__ import annotations

from collections import deque

import numpy as np

from palette import mix, pick, ramp as get_ramp, darken, lighten

INK = (17, 21, 20)


def hexc(value: str):
    value = value.lstrip("#")
    return tuple(int(value[i:i + 2], 16) for i in (0, 2, 4))


class Canvas:
    """RGBA pixel canvas with a y-up authoring space.

    Public helpers take y-up coordinates (0 = ground) and convert internally,
    which keeps character proportions readable in the source.
    """

    def __init__(self, w: int, h: int, color=(0, 0, 0, 0)):
        self.w, self.h = w, h
        self.arr = np.zeros((h, w, 4), dtype=np.uint8)
        if color[3] if len(color) > 3 else True:
            self.arr[:, :, 0] = color[0]
            self.arr[:, :, 1] = color[1]
            self.arr[:, :, 2] = color[2]
            self.arr[:, :, 3] = color[3] if len(color) > 3 else 255

    # ---------- coordinate helpers ----------
    def iy(self, y):
        return self.h - 1 - y

    def empty_mask(self):
        return np.zeros((self.h, self.w), dtype=bool)

    # ---------- primitives (y-up) ----------
    def px(self, x, y, color, alpha=255):
        if x < 0 or y < 0 or x >= self.w or y >= self.h:
            return
        self.arr[self.iy(y), x] = (color[0], color[1], color[2], alpha)

    def rect(self, x, y, w, h, color, alpha=255):
        x0, x1 = max(0, x), min(self.w, x + w)
        y1, y0 = self.iy(y), self.iy(y + h - 1)
        if x1 <= x0 or y0 > y1:
            return
        self.arr[y0:y1 + 1, x0:x1] = (color[0], color[1], color[2], alpha)

    def line(self, x0, y0, x1, y1, color, alpha=255, thickness=1):
        dx, dy = abs(x1 - x0), abs(y1 - y0)
        sx = 1 if x0 < x1 else -1
        sy = 1 if y0 < y1 else -1
        err = dx - dy
        while True:
            self.disc(x0, y0, thickness - 1, color, alpha)
            if x0 == x1 and y0 == y1:
                break
            e2 = 2 * err
            if e2 > -dy:
                err -= dy
                x0 += sx
            if e2 < dx:
                err += dx
                y0 += sy

    def disc(self, cx, cy, r, color, alpha=255):
        if r <= 0:
            self.px(cx, cy, color, alpha)
            return
        r2 = r * r + r
        for y in range(cy - r, cy + r + 1):
            for x in range(cx - r, cx + r + 1):
                if (x - cx) ** 2 + (y - cy) ** 2 <= r2:
                    self.px(x, y, color, alpha)

    def oval(self, cx, cy, rx, ry, color, alpha=255, filled=True):
        for y in range(cy - ry, cy + ry + 1):
            for x in range(cx - rx, cx + rx + 1):
                if rx <= 0 or ry <= 0:
                    continue
                d = ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2
                if d <= 1.05:
                    if filled or d > 0.45:
                        self.px(x, y, color, alpha)

    def polygon(self, points, color, alpha=255):
        """Scanline fill of a convex or simple concave polygon (y-up input)."""
        pts = [(float(x), float(y)) for x, y in points]
        if len(pts) < 3:
            return
        ys = [p[1] for p in pts]
        y_min, y_max = int(np.floor(min(ys))), int(np.ceil(max(ys)))
        for y in range(y_min, y_max + 1):
            nodes = []
            j = len(pts) - 1
            for i in range(len(pts)):
                yi, yj = pts[i][1], pts[j][1]
                if (yi < y <= yj) or (yj < y <= yi):
                    t = (y - yi) / (yj - yi) if yj != yi else 0
                    nodes.append(pts[i][0] + t * (pts[j][0] - pts[i][0]))
                j = i
            nodes.sort()
            for k in range(0, len(nodes) - 1, 2):
                x0, x1 = int(np.ceil(nodes[k])), int(np.floor(nodes[k + 1]))
                for x in range(x0, x1 + 1):
                    self.px(x, y, color, alpha)

    # ---------- masks ----------
    def mask_polygon(self, points):
        mask = self.empty_mask()
        pts = [(float(x), float(y)) for x, y in points]
        ys = [p[1] for p in pts]
        for y in range(int(np.floor(min(ys))), int(np.ceil(max(ys))) + 1):
            nodes = []
            j = len(pts) - 1
            for i in range(len(pts)):
                yi, yj = pts[i][1], pts[j][1]
                if (yi < y <= yj) or (yj < y <= yi):
                    t = (y - yi) / (yj - yi) if yj != yi else 0
                    nodes.append(pts[i][0] + t * (pts[j][0] - pts[i][0]))
                j = i
            nodes.sort()
            for k in range(0, len(nodes) - 1, 2):
                for x in range(int(np.ceil(nodes[k])), int(np.floor(nodes[k + 1])) + 1):
                    if 0 <= x < self.w and 0 <= y < self.h:
                        mask[self.iy(y), x] = True
        return mask

    def mask_band(self, points, width: float):
        """Thick stroke mask along a polyline - used for limbs."""
        mask = self.empty_mask()
        pts = list(points)
        for i in range(len(pts) - 1):
            (x0, y0), (x1, y1) = pts[i], pts[i + 1]
            steps = int(max(abs(x1 - x0), abs(y1 - y0))) * 2 + 2
            for s in range(steps + 1):
                t = s / steps
                cx, cy = x0 + (x1 - x0) * t, y0 + (y1 - y0) * t
                r = width / 2 if i == 0 and s == 0 else width / 2
                half = max(0, int(round(r)))
                for dy in range(-half - 1, half + 2):
                    for dx in range(-half - 1, half + 2):
                        if dx * dx + dy * dy <= (r + 0.25) ** 2:
                            x, y = int(round(cx)) + dx, int(round(cy)) + dy
                            if 0 <= x < self.w and 0 <= y < self.h:
                                mask[self.iy(y), x] = True
        return mask

    # ---------- shading ----------
    @staticmethod
    def edge_distance(mask: np.ndarray, max_dist: int = 24) -> np.ndarray:
        """Chebyshev-ish distance from every inside pixel to the nearest edge."""
        h, w = mask.shape
        dist = np.full((h, w), max_dist, dtype=np.float32)
        dist[mask] = max_dist
        q = deque()
        border = mask & ~_shift_and(mask, 1)
        ys, xs = np.nonzero(border)
        for y, x in zip(ys, xs):
            dist[y, x] = 0
            q.append((y, x))
        neigh = ((1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1))
        while q:
            y, x = q.popleft()
            d = dist[y, x]
            if d >= max_dist:
                continue
            for dy, dx in neigh:
                ny, nx = y + dy, x + dx
                if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and dist[ny, nx] > d + 1:
                    dist[ny, nx] = d + 1
                    q.append((ny, nx))
        return dist

    def shade(self, mask, ramp_name, light=(0.42, -0.90), depth=7.0, base=0.52,
              contrast=1.0, rim=0.55, rim_ramp=None, grain=0.06, grain_seed=7, dither=True,
              gradient=0.0, steps=None, bottom_darken=0.0):
        """Shade a masked region with one light setup and ramp quantisation."""
        if not mask.any():
            return
        dist = self.edge_distance(mask)
        ys, xs = np.nonzero(mask)
        lum = np.zeros((self.h, self.w), dtype=np.float32)

        gy, gx = np.gradient(dist)
        norm = np.sqrt(gx * gx + gy * gy) + 1e-6
        nx, ny = -gx / norm, gy / norm

        # curvature term: pixels far from an edge bulge toward the light
        volume = np.clip(dist / depth, 0.0, 1.0)
        lx, ly = light
        diffuse = (nx * lx + ny * (-ly))
        lum = base + (diffuse * 0.42 + (volume - 0.5) * 0.34) * contrast

        if gradient:
            ys_norm = (self.h - 1 - ys) / max(1, self.h - 1)
            lum[ys, xs] += (ys_norm - 0.5) * gradient

        if bottom_darken:
            ys_norm = (self.h - 1 - ys) / max(1, self.h - 1)
            lum[ys, xs] -= np.clip((0.35 - ys_norm) / 0.35, 0, 1) * bottom_darken

        colors = get_ramp(ramp_name)
        n = len(colors) - 1
        if rim > 0:
            # rim light along the light-facing edge
            edge = (dist <= 1.2)
            rim_mask = mask & edge & ((nx * lx + ny * (-ly)) > 0.15)
            lum[rim_mask] += rim * 0.55

        t = np.clip(lum, 0.0, 1.0)
        idx = t * n
        low = np.floor(idx).astype(int)
        frac = idx - low
        if dither:
            pattern = _bayer(self.h, self.w)
            rng = _grain(self.h, self.w, grain_seed)
            weighted = frac + np.where(rng < grain, 1.0, 0.0) * 0.25
            low = np.where((weighted > 0.5) & (pattern == 1), np.minimum(low + 1, n), low)
        low = np.clip(low, 0, n)

        shade_rgb = np.zeros((self.h, self.w, 3), dtype=np.uint8)
        rail = np.array(colors, dtype=np.uint8)
        for k in range(n + 1):
            shade_rgb[low == k] = rail[k]
        self._composite(mask, shade_rgb)

        if rim_ramp and rim > 0:
            rim_mask = mask & (dist <= 1.05) & ((nx * light[0] + ny * (-light[1])) > 0.30)
            self._composite(rim_mask, np.array(pick(ramp_ramp, 1.0), dtype=np.uint8) * np.ones((self.h, self.w, 1), dtype=np.uint8))

    def fill_mask(self, mask, color, alpha=255):
        self._composite(mask, np.array(color, dtype=np.uint8))

    def _composite(self, mask, color):
        color_arr = np.asarray(color, dtype=np.uint8)
        if color_arr.ndim == 1:
            self.arr[mask, 0:3] = color_arr[0:3]
        else:
            self.arr[mask, 0:3] = color_arr[mask, 0:3]
        self.arr[mask, 3] = 255

    # ---------- effects ----------
    def dither_mask(self, mask, ramp_name, t0, t1, pattern=0.5, seed=3):
        """Flat two-tone dithered fill inside a mask (classic pixel-art texture)."""
        colors = get_ramp(ramp_name)
        c0 = np.array(colors[int(t0 * (len(colors) - 1))], dtype=np.float32)
        c1 = np.array(colors[int(t1 * (len(colors) - 1))], dtype=np.float32)
        pat = _bayer(self.h, self.w)
        rng = _grain(self.h, self.w, seed)
        sel = mask & ((pat == 1) | (rng < (pattern - 0.5) * 2 + 0.5))
        out = np.zeros((self.h, self.w, 3), dtype=np.uint8)
        out[:] = c0.astype(np.uint8)
        out[sel] = c1.astype(np.uint8)
        self._composite(mask, out)

    def paint_part(self, mask, ramp_name, light=(1, -1), shadow_depth=3, light_depth=1,
                   base=0.55, shadow=0.28, high=0.82, dither_band=True, dither_seed=3,
                   bottom_shadow=0, noise=0, noise_seed=5, flat=False):
        """Classic pixel-art cluster shading: base fill, directional shadow ring,
        light ring, dithered transition band and an optional contact shadow.

        `light` is given in array coordinates (x right, y down), so (1, -1) means
        "light comes from the upper right"."""
        if not mask.any():
            return
        colors = get_ramp(ramp_name)
        idx = lambda t: np.array(colors[int(round(t * (len(colors) - 1)))], dtype=np.uint8)

        self._composite(mask, idx(base))
        if flat:
            return
        dark_side = (-int(round(light[0])), -int(round(light[1])))
        light_side = (int(round(light[0])), int(round(light[1])))
        dark_ring = ring(mask, dark_side[0], dark_side[1], shadow_depth) if shadow_depth > 0 else mask & False
        if shadow_depth > 0:
            self._composite(dark_ring, idx(shadow))
        if dither_band and shadow_depth > 0:
            band = ring(mask, dark_side[0], dark_side[1], shadow_depth + 2) & ~dark_ring
            pat = _bayer(self.h, self.w)
            rng = _grain(self.h, self.w, dither_seed)
            band = band & ((pat == 1) | (rng < 0.32))
            self._composite(band, idx((base + shadow) * 0.5))
        if light_depth > 0:
            self._composite(ring(mask, light_side[0], light_side[1], light_depth), idx(high))
        if bottom_shadow > 0:
            self._composite(ring(mask, 0, -1, bottom_shadow), idx(shadow))
        if noise > 0:
            self.noise_into(mask, ramp_name, amount=noise, seed=noise_seed)

    def seam(self, mask, color, dx, dy, thickness=1, inside=True):
        """Draw a thin line hugging one border of a mask (straps, seams, piping)."""
        r = ring(mask, dx, dy, thickness)
        self._composite(r if inside else r, np.array(color, dtype=np.uint8))

    def outline(self, color=INK, only_alpha=True):
        """1px outline around every opaque pixel."""
        alpha = self.arr[:, :, 3] > 0
        grown = _shift_or(alpha, 1) & ~alpha
        if only_alpha:
            self.arr[grown] = (color[0], color[1], color[2], 255)

    def shade_overlay(self, base_alpha, mask, color, strength=0.35):
        """Multiply an ambient-occlusion colour into an existing region."""
        c = np.array(color, dtype=np.float32)
        target = mask & base_alpha
        blended = (self.arr[target, 0:3].astype(np.float32) * (1 - strength) + c * strength).astype(np.uint8)
        self.arr[target, 0:3] = blended

    def to_image(self):
        from PIL import Image
        return Image.fromarray(self.arr, mode="RGBA")

    def save(self, path):
        self.to_image().save(path)

    def scaled(self, factor: int):
        from PIL import Image
        img = self.to_image()
        return img.resize((self.w * factor, self.h * factor), Image.NEAREST)

    def blit(self, other: "Canvas", ox=0, oy=0):
        """Blit another canvas with y-up offset."""
        for y in range(other.h):
            ty = self.iy(oy + (other.h - 1 - y))
            if ty < 0 or ty >= self.h:
                continue
            for x in range(other.w):
                tx = ox + x
                if tx < 0 or tx >= self.w:
                    continue
                src = other.arr[y, x]
                if src[3] == 0:
                    continue
                self.arr[ty, tx] = src


def _shift_and(mask, n):
    out = mask.copy()
    for _ in range(n):
        out = out & np.roll(out, 1, axis=0) & np.roll(out, -1, axis=0) & np.roll(out, 1, axis=1) & np.roll(out, -1, axis=1)
    return out


def _shift_or(mask, n):
    out = mask.copy()
    for _ in range(n):
        out = out | np.roll(out, 1, axis=0) | np.roll(out, -1, axis=0) | np.roll(out, 1, axis=1) | np.roll(out, -1, axis=1)
    return out


def _bayer(h, w):
    tile = np.array([[0, 2], [3, 1]], dtype=np.int32)
    yy = np.arange(h)[:, None] % 2
    xx = np.arange(w)[None, :] % 2
    return tile[yy, xx]


def _grain(h, w, seed):
    rng = np.random.default_rng(seed)
    return rng.random((h, w)).astype(np.float32)


# --------------------------------------------------------------------------
# mask morphology helpers used by paint_part / seam
# --------------------------------------------------------------------------
def _shift_arr(mask, dx, dy):
    """Return a mask whose content is shifted by (dx, dy), zero-filled."""
    out = np.zeros_like(mask)
    h, w = mask.shape
    ys, xs = np.nonzero(mask)
    ty, tx = ys + dy, xs + dx
    ok = (ty >= 0) & (ty < h) & (tx >= 0) & (tx < w)
    out[ty[ok], tx[ok]] = True
    return out


def erode_along(mask, dx, dy, t):
    """Erode `mask` t times in direction (dx, dy): keeps pixels that still have
    mask neighbours `t` steps toward (dx, dy)."""
    out = mask
    for _ in range(max(1, int(t))):
        out = out & _shift_arr(out, dx, dy)
    return out


def ring(mask, dx, dy, t):
    """Border band of `mask` on the side you reach by travelling (dx, dy)."""
    return mask & ~erode_along(mask, dx, dy, t)


def contour(canvas, mask, color=(17, 21, 20), thickness=1):
    """Dark inner contour around a mask - the classic pixel-art part separator."""
    inner = ring(mask, 1, 0, thickness) | ring(mask, -1, 0, thickness) | \
        ring(mask, 0, 1, thickness) | ring(mask, 0, -1, thickness)
    canvas.fill_mask(inner, color)
