"""Build single fill / detail colour helpers for flat hand-painted pixel shapes.

Tiles, props and icons are drawn procedurally (like the original game) but with
the new dense-pixel look: 3-4 tone clusters, hard pixel steps and a shared
palette. This module keeps those helpers in one place.
"""
from __future__ import annotations

import numpy as np

import palette as pal


class Paint:
    """Draws into a numpy RGBA array using y-up coordinates."""

    def __init__(self, w, h, color=(0, 0, 0, 0)):
        self.w, self.h = w, h
        self.a = np.zeros((h, w, 4), dtype=np.uint8)
        if color[3]:
            self.rect(0, 0, w, h, color)

    # -- primitives --------------------------------------------------------
    def px(self, x, y, c, alpha=255):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.a[self.h - 1 - y, x] = (c[0], c[1], c[2], alpha)

    def rect(self, x, y, w, h, c, alpha=255):
        x0, x1 = max(0, x), min(self.w, x + w)
        yb = self.h - 1 - y
        y1, y0 = min(self.h - 1, yb), max(0, yb - h + 1)
        if x1 <= x0 or y0 > y1:
            return
        self.a[y0:y1 + 1, x0:x1] = (c[0], c[1], c[2], alpha)

    def step_rect(self, x, y, w, h, base, shadow, light, light_side=1):
        """Rect with a 1px light edge and a 1px shadow edge."""
        self.rect(x, y, w, h, base)
        self.rect(x, y, w, 1, light if light_side > 0 else shadow)
        self.rect(x, y + h - 1, w, 1, shadow if light_side > 0 else light)
        if light_side > 0:
            self.rect(x + w - 1, y + 1, 1, h - 2, light)
            self.rect(x, y + 1, 1, h - 2, shadow)
        else:
            self.rect(x, y + 1, 1, h - 2, light)
            self.rect(x + w - 1, y + 1, 1, h - 2, shadow)

    def line(self, x0, y0, x1, y1, c, thickness=1):
        dx, dy = abs(x1 - x0), abs(y1 - y0)
        sx = 1 if x0 < x1 else -1
        sy = 1 if y0 < y1 else -1
        err = dx - dy
        while True:
            for oy in range(thickness):
                for ox in range(thickness):
                    self.px(x0 + ox, y0 + oy, c)
            if x0 == x1 and y0 == y1:
                break
            e2 = 2 * err
            if e2 > -dy:
                err -= dy
                x0 += sx
            if e2 < dx:
                err += dx
                y0 += sy

    def disc(self, cx, cy, r, c):
        for y in range(cy - r, cy + r + 1):
            for x in range(cx - r, cx + r + 1):
                if (x - cx) ** 2 + (y - cy) ** 2 <= r * r + r:
                    self.px(x, y, c)

    def noise(self, x, y, w, h, c, density=0.12, seed=1, alpha=255):
        rng = np.random.default_rng(seed)
        for yy in range(y, y + h):
            for xx in range(x, x + w):
                if rng.random() < density:
                    self.px(xx, yy, c, alpha)

    def dither_rect(self, x, y, w, h, c0, c1, step=2, offset=0, alpha=255):
        for yy in range(y, y + h):
            for xx in range(x, x + w):
                self.px(xx, yy, c1 if (xx + yy + offset) % step == 0 else c0, alpha)

    def shade_edge(self, x, y, w, h, c, side="bottom", thickness=1, alpha=180):
        for t in range(thickness):
            if side == "bottom":
                self.rect(x, y + t, w, 1, c, alpha)
            elif side == "top":
                self.rect(x, y + h - 1 - t, w, 1, c, alpha)
            elif side == "left":
                self.rect(x + t, y, 1, h, c, alpha)
            else:
                self.rect(x + w - 1 - t, y, 1, h, c, alpha)

    def to_image(self):
        from PIL import Image
        return Image.fromarray(self.a, "RGBA")
