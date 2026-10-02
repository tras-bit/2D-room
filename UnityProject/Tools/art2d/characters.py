"""Hand-placed side-view pixel-art characters for SUBSISTENCE.

Drawn with explicit pixel clusters at 96 px per world unit: the survivor is
96x144 (1.5 world units tall) with the feet on y=0 so Unity can use a
bottom-centre pivot and flip X for facing.
"""
from __future__ import annotations

import math

import numpy as np

from painter import Canvas, INK, ring, contour, _shift_arr
from palette import darken, lighten, mix, pick

PPU = 96
SURVIVOR_W, SURVIVOR_H = 96, 144
WATCHER_W, WATCHER_H = 104, 160

LIGHT = (1, -1)                 # array space: key light from the upper right
LINE = (20, 24, 22)             # part separator colour
SKIN_D = (74, 49, 38)


def capsule(canvas, x0, y0, x1, y1, r0, r1):
    mask = canvas.empty_mask()
    steps = int(max(abs(x1 - x0), abs(y1 - y0)) * 2) + 3
    for s in range(steps + 1):
        t = s / steps
        cx, cy = x0 + (x1 - x0) * t, y0 + (y1 - y0) * t
        r = r0 + (r1 - r0) * t
        half = int(math.floor(r)) + 1
        for dy in range(-half, half + 1):
            for dx in range(-half, half + 1):
                if dx * dx + dy * dy <= (r + 0.25) ** 2:
                    x, y = int(round(cx)) + dx, int(round(cy)) + dy
                    if 0 <= x < canvas.w and 0 <= y < canvas.h:
                        mask[canvas.iy(y), x] = True
    return mask


def poly(canvas, points):
    return canvas.mask_polygon(points)


def shift(mask, dx, dy):
    return _shift_arr(mask, dx, dy)


# --------------------------------------------------------------------------
# pose skeletons (y-up, ground at y=0)
# --------------------------------------------------------------------------
def survivor_pose(frame=0, hurt=False):
    p = {
        "hip": (46, 67), "chest": (46, 88), "neck": (47, 101), "head": (48, 120),
        "sh_back": (39, 97), "elb_back": (35, 80), "wr_back": (37, 60),
        "sh_front": (54, 97), "elb_front": (59, 80), "wr_front": (57, 61),
        "knee_back": (42, 38), "ankle_back": (38, 12), "foot_back": (32, 5),
        "knee_front": (51, 38), "ankle_front": (55, 12), "foot_front": (62, 5),
    }
    if frame == 1:        # walk contact
        p.update(hip=(46, 68), chest=(46, 90), neck=(47, 103), head=(48, 122),
                 knee_front=(55, 40), ankle_front=(64, 13), foot_front=(71, 6),
                 knee_back=(38, 38), ankle_back=(29, 12), foot_back=(24, 5),
                 elb_front=(52, 80), wr_front=(45, 62),
                 elb_back=(42, 80), wr_back=(48, 62))
    elif frame == 2:      # walk pass
        p.update(hip=(46, 69), chest=(46, 91), neck=(47, 104), head=(48, 123),
                 knee_front=(52, 41), ankle_front=(50, 12), foot_front=(56, 5),
                 knee_back=(42, 39), ankle_back=(46, 13), foot_back=(42, 6),
                 elb_front=(58, 81), wr_front=(56, 64),
                 elb_back=(37, 81), wr_back=(39, 63))
    elif frame == 3:      # airborne tuck
        p.update(hip=(46, 71), chest=(46, 93), neck=(47, 106), head=(47, 125),
                 knee_front=(59, 52), ankle_front=(53, 27), foot_front=(59, 22),
                 knee_back=(37, 49), ankle_back=(31, 24), foot_back=(25, 20),
                 sh_front=(54, 98), elb_front=(62, 86), wr_front=(64, 72),
                 elb_back=(32, 82), wr_back=(30, 68))
    elif frame == 4:      # overhead swing
        p.update(hip=(45, 67), chest=(45, 89), neck=(46, 102), head=(47, 121),
                 knee_front=(55, 40), ankle_front=(62, 12), foot_front=(68, 5),
                 knee_back=(38, 38), ankle_back=(32, 12), foot_back=(27, 5),
                 sh_front=(54, 96), elb_front=(65, 109), wr_front=(57, 121),
                 elb_back=(35, 84), wr_back=(27, 72))
    if hurt:
        p["head"] = (p["head"][0] - 3, p["head"][1] - 2)
        p["chest"] = (p["chest"][0] - 2, p["chest"][1] - 3)
        p["neck"] = (p["neck"][0] - 2, p["neck"][1] - 3)
    return p


# --------------------------------------------------------------------------
# limbs
# --------------------------------------------------------------------------
def leg(canvas, hip, knee, ankle, ramp, far=False):
    thigh = capsule(canvas, hip[0], hip[1], knee[0], knee[1], 6.6, 5.2)
    calf = capsule(canvas, knee[0], knee[1], ankle[0], ankle[1], 5.2, 3.8)
    mask = thigh | calf
    base, shadow, high = (0.34, 0.16, 0.5) if far else (0.56, 0.32, 0.82)
    canvas.paint_part(mask, ramp, base=base, shadow=shadow, high=high,
                      shadow_depth=3, light_depth=1, dither_seed=3)
    # knee reinforcement + trouser folds
    canvas.seam(capsule(canvas, knee[0] - 4, knee[1] - 3, knee[0] + 4, knee[1] - 4, 0.6, 0.6) & mask,
                darken(pick(ramp, shadow), 0.15), 0, 1, 1)
    canvas.seam(capsule(canvas, hip[0] - 4, hip[1] - 8, hip[0] + 5, hip[1] - 10, 0.6, 0.6) & mask,
                darken(pick(ramp, shadow), 0.2), 0, 1, 1)
    contour(canvas, mask, LINE)
    return mask


def boot(canvas, ankle, foot, ramp="rust", far=False, tall=False):
    height = 12 if tall else 9
    shoe = capsule(canvas, ankle[0] - 1, ankle[1] + height - 3, foot[0], foot[1] + 2, 4.4, 3.8)
    sole = capsule(canvas, ankle[0] - 2, foot[1], foot[0] + 1, foot[1] - 1, 4.4, 4.2)
    mask = shoe | sole
    base, shadow, high = (0.3, 0.16, 0.44) if far else (0.5, 0.3, 0.72)
    canvas.paint_part(mask, ramp, base=base, shadow=shadow, high=high, shadow_depth=2, light_depth=1)
    canvas.seam(capsule(canvas, ankle[0] - 5, foot[1] + 2, foot[0] + 2, foot[1] + 1, 0.6, 0.6) & mask, (22, 18, 15), 0, -1, 2)
    for i in range(3):
        canvas.seam(capsule(canvas, ankle[0] - 3 + i, ankle[1] + height - 4 + i,
                            ankle[0] + 3 + i, ankle[1] + height - 4 + i, 0.5, 0.5) & mask,
                    pick("cloth", 0.98), 0, -1, 1)
    contour(canvas, mask, LINE)
    return mask


def arm(canvas, shoulder, elbow, wrist, ramp, far=False, glove=True):
    upper = capsule(canvas, shoulder[0], shoulder[1], elbow[0], elbow[1], 5.0, 4.2)
    fore = capsule(canvas, elbow[0], elbow[1], wrist[0], wrist[1], 4.2, 3.4)
    mask = upper | fore
    base, shadow, high = (0.32, 0.16, 0.46) if far else (0.54, 0.30, 0.80)
    canvas.paint_part(mask, ramp, base=base, shadow=shadow, high=high, shadow_depth=2, light_depth=1, dither_seed=9)
    hand_color = "grey" if glove else "skin"
    hand = capsule(canvas, wrist[0] - 1, wrist[1] + 1, wrist[0] + 2, wrist[1] - 5, 3.8, 3.0)
    hbase = 0.3 if far else 0.44
    canvas.paint_part(hand, hand_color, base=hbase, shadow=hbase - 0.14, high=hbase + 0.2,
                      shadow_depth=2, light_depth=1)
    for i in range(3):
        canvas.seam(capsule(canvas, wrist[0] - 2 + i, wrist[1] - 4, wrist[0] - 1 + i, wrist[1] - 7, 0.4, 0.4) & hand,
                    (16, 18, 17), 0, 1, 1)
    mask = mask | hand
    canvas.seam(capsule(canvas, wrist[0] - 4, wrist[1] + 6, wrist[0] + 4, wrist[1] + 5, 0.8, 0.8) & mask,
                darken(pick(ramp, shadow), 0.3), 0, -1, 1)
    contour(canvas, mask, LINE)
    return mask


# --------------------------------------------------------------------------
# body
# --------------------------------------------------------------------------
def torso(canvas, p, ramp, vest=False, jacket=True, hazmat=False, far=False):
    hip, chest, neck = p["hip"], p["chest"], p["neck"]
    pts = [
        (chest[0] - 10, hip[1] + 3),
        (chest[0] - 12, chest[1] - 4),
        (chest[0] - 11, neck[1] - 4),
        (chest[0] - 3, neck[1] - 2),
        (chest[0] + 8, neck[1] - 5),
        (chest[0] + 12, chest[1] - 5),
        (chest[0] + 13, hip[1] + 3),
        (chest[0] + 11, hip[1] - 7),
        (hip[0] - 7, hip[1] - 8),
    ]
    mask = poly(canvas, pts)
    mask |= capsule(canvas, chest[0] - 9, chest[1] + 8, chest[0] + 10, chest[1] + 8, 6.2, 6.0)
    base, shadow, high = (0.34, 0.18, 0.5) if far else (0.58, 0.34, 0.84)
    canvas.paint_part(mask, ramp, base=base, shadow=shadow, high=high,
                      shadow_depth=4, light_depth=1, dither_seed=13)
    if jacket:
        collar = capsule(canvas, neck[0] - 10, neck[1] - 3, neck[0] + 8, neck[1] - 4, 3.2, 3.0)
        canvas.paint_part(collar & mask, "cloth", base=0.5, shadow=0.3, high=0.74, shadow_depth=2, light_depth=1)
        # zip line and two chest pockets
        canvas.seam(capsule(canvas, chest[0] + 5, neck[1] - 4, chest[0] + 7, hip[1], 0.6, 0.6) & mask,
                    (218, 210, 172), 1, 0, 1)
        for py in (chest[1] - 2, chest[1] - 12):
            pocket = poly(canvas, [
                (chest[0] + 1, py + 2), (chest[0] + 11, py + 1),
                (chest[0] + 11, py - 5), (chest[0] + 1, py - 4),
            ]) & mask
            canvas.seam(pocket, darken(pick(ramp, shadow), 0.1), 1, 0, 1)
            canvas.seam(pocket, lighten(pick(ramp, high), 0.1), -1, 0, 1)
        canvas.seam(ring(mask, 0, -1, 3), darken(pick(ramp, shadow), 0.25), 0, -1, 1)
    if vest:
        plate = poly(canvas, [
            (chest[0] - 11, neck[1] - 8), (chest[0] + 12, neck[1] - 10),
            (chest[0] + 12, hip[1] + 12), (chest[0] - 10, hip[1] + 10),
        ]) & mask
        canvas.paint_part(plate, "grey", base=0.4, shadow=0.24, high=0.62, shadow_depth=3, light_depth=1, dither_seed=43)
        canvas.seam(capsule(canvas, chest[0] - 6, neck[1] - 9, chest[0] - 8, hip[1] + 11, 1.0, 1.0) & plate, (16, 18, 17), 1, 0, 1)
        pouch = poly(canvas, [
            (chest[0] + 2, hip[1] + 20), (chest[0] + 10, hip[1] + 19),
            (chest[0] + 10, hip[1] + 11), (chest[0] + 2, hip[1] + 12),
        ]) & mask
        canvas.paint_part(pouch, "cloth", base=0.52, shadow=0.34, high=0.74, shadow_depth=2, light_depth=1)
        canvas.seam(pouch, (26, 26, 22), 0, -1, 1)
    if hazmat:
        canvas.paint_part(mask, "hazmat", base=0.6, shadow=0.4, high=0.82, shadow_depth=4, light_depth=1, dither_seed=47)
        canvas.seam(capsule(canvas, chest[0] + 6, neck[1] - 4, chest[0] + 8, hip[1], 0.6, 0.6) & mask, (246, 214, 128), 1, 0, 1)
        for py in (chest[1] - 2, chest[1] - 12):
            canvas.seam(capsule(canvas, chest[0] + 1, py, chest[0] + 11, py - 1, 0.6, 0.6) & mask, darken((186, 136, 30), 0.35), 0, -1, 1)
    if not far:
        contour(canvas, mask, LINE)
    return mask


def head(canvas, center, hair_style="short", helmet=False, hazmat=False, hurt=False, far=False):
    cx, cy = center
    # neck stub so the head does not float
    neck = capsule(canvas, cx - 2, cy - 10, cx - 1, cy - 16, 4.4, 4.8)
    canvas.paint_part(neck, "skin", base=0.42, shadow=0.26, high=0.6, shadow_depth=2, light_depth=1)

    skull = capsule(canvas, cx + 1, cy + 4, cx - 1, cy - 3, 8.0, 7.4)
    jaw = poly(canvas, [
        (cx - 7, cy - 2), (cx + 3, cy - 3), (cx + 9, cy - 5),
        (cx + 9, cy - 10), (cx + 2, cy - 13), (cx - 7, cy - 12), (cx - 8, cy - 6),
    ])
    face = skull | jaw
    canvas.paint_part(face, "skin", base=0.6, shadow=0.38, high=0.84, shadow_depth=3, light_depth=1, dither_seed=21)

    if hazmat:
        hood = capsule(canvas, cx - 1, cy + 2, cx - 1, cy + 1, 10.4, 10.0)
        canvas.paint_part(hood, "hazmat", base=0.62, shadow=0.42, high=0.84, shadow_depth=3, light_depth=1)
        visor = poly(canvas, [(cx - 5, cy + 3), (cx + 7, cy + 2), (cx + 7, cy - 8), (cx - 5, cy - 7)])
        canvas.paint_part(visor, "teal", base=0.6, shadow=0.4, high=0.88, shadow_depth=2, light_depth=1)
        canvas.seam(capsule(canvas, cx + 6, cy + 2, cx + 7, cy - 7, 0.6, 0.6), (222, 238, 226), 1, 0, 1)
        canister = capsule(canvas, cx + 9, cy - 2, cx + 12, cy - 7, 2.6, 2.4)
        canvas.paint_part(canister, "grey", base=0.44, shadow=0.28, high=0.64, shadow_depth=2, light_depth=1)
        out = face | hood | visor | canister
        contour(canvas, out, LINE)
        return out

    # ear, brow, eye, nose, mouth (readable 2-4 px features)
    ear = capsule(canvas, cx - 6, cy + 1, cx - 7, cy - 4, 2.6, 2.2)
    canvas.paint_part(ear, "skin", base=0.52, shadow=0.36, high=0.72, shadow_depth=2, light_depth=1)
    canvas.seam(ear, darken(SKIN_D, 0.25), -1, 0, 1)
    canvas.paint_part(capsule(canvas, cx + 1, cy + 4, cx + 8, cy + 4, 0.6, 0.6) & face, "skin", base=0.3, shadow=0.2, high=0.4, shadow_depth=1, light_depth=0)
    eye = capsule(canvas, cx + 4, cy + 2, cx + 7, cy + 2, 1.0, 1.0)
    canvas.fill_mask(eye & face, (232, 226, 205))
    canvas.fill_mask(capsule(canvas, cx + 6, cy + 2, cx + 7, cy + 2, 0.5, 0.5) & face, (34, 32, 28))
    nose = poly(canvas, [(cx + 8, cy + 2), (cx + 11, cy - 1), (cx + 11, cy - 4), (cx + 7, cy - 3)])
    canvas.paint_part(nose & face, "skin", base=0.68, shadow=0.5, high=0.9, shadow_depth=2, light_depth=1)
    canvas.fill_mask(capsule(canvas, cx + 10, cy - 2, cx + 11, cy - 4, 0.5, 0.5) & face, darken(SKIN_D, 0.3))
    canvas.fill_mask(capsule(canvas, cx + 5, cy - 8, cx + 9, cy - 9, 0.6, 0.6) & face, darken((124, 70, 58), 0.4))
    if hurt:
        canvas.fill_mask(capsule(canvas, cx + 2, cy + 1, cx + 6, cy + 1, 0.5, 0.5) & face, (134, 44, 34))

    out = face | ear
    if not helmet:
        ramp_hair = "rust" if not far else "rust"
        if hair_style == "short":
            hair = poly(canvas, [
                (cx - 9, cy + 2), (cx - 8, cy + 8), (cx - 1, cy + 11), (cx + 6, cy + 10),
                (cx + 9, cy + 6), (cx + 4, cy + 6), (cx - 3, cy + 4),
            ]) | capsule(canvas, cx - 8, cy + 1, cx - 8, cy - 3, 2.6, 2.2)
        else:
            hair = poly(canvas, [
                (cx - 10, cy + 2), (cx - 9, cy + 9), (cx + 1, cy + 12), (cx + 8, cy + 10),
                (cx + 10, cy + 5), (cx + 3, cy + 6), (cx - 3, cy + 4),
            ]) | capsule(canvas, cx - 8, cy + 2, cx - 9, cy - 8, 3.2, 2.6)
        canvas.paint_part(hair, ramp_hair, base=0.28, shadow=0.14, high=0.46, shadow_depth=2, light_depth=1, dither_seed=31)
        canvas.seam(capsule(canvas, cx - 6, cy + 9, cx + 5, cy + 8, 0.5, 0.5) & hair, lighten(pick(ramp_hair, 0.6), 0.11), 0, -1, 1)
        out = out | hair
    else:
        cap = poly(canvas, [
            (cx - 9, cy + 3), (cx - 7, cy + 11), (cx + 1, cy + 13), (cx + 9, cy + 10),
            (cx + 11, cy + 5), (cx + 10, cy + 3), (cx - 8, cy + 1),
        ])
        canvas.paint_part(cap, "olive", base=0.46, shadow=0.28, high=0.66, shadow_depth=3, light_depth=1)
        brim = capsule(canvas, cx + 3, cy + 4, cx + 13, cy + 3, 1.9, 1.7)
        canvas.paint_part(brim, "olive", base=0.36, shadow=0.22, high=0.5, shadow_depth=1, light_depth=1)
        lamp = capsule(canvas, cx + 5, cy + 9, cx + 8, cy + 9, 2.3, 2.1)
        canvas.paint_part(lamp, "grey", base=0.46, shadow=0.3, high=0.66, shadow_depth=1, light_depth=1)
        canvas.fill_mask(capsule(canvas, cx + 10, cy + 9, cx + 11, cy + 9, 0.6, 0.6) & lamp, (238, 216, 130))
        out = out | cap | brim | lamp
    contour(canvas, out, LINE)
    return out


def backpack(canvas, p, ramp="cloth"):
    hip, chest = p["hip"], p["chest"]
    body = poly(canvas, [
        (chest[0] - 28, chest[1] + 5), (chest[0] - 13, chest[1] + 7),
        (chest[0] - 12, hip[1] - 4), (chest[0] - 26, hip[1] + 1),
    ])
    canvas.paint_part(body, ramp, base=0.4, shadow=0.24, high=0.58, shadow_depth=4, light_depth=1, dither_seed=55)
    roll = capsule(canvas, chest[0] - 27, chest[1] + 8, chest[0] - 14, chest[1] + 8, 3.6, 3.4)
    canvas.paint_part(roll, ramp, base=0.46, shadow=0.3, high=0.64, shadow_depth=2, light_depth=1)
    canvas.seam(capsule(canvas, chest[0] - 21, chest[1] + 5, chest[0] - 19, hip[1] + 2, 1.0, 1.0) & body, (24, 24, 20), 1, 0, 1)
    canvas.seam(capsule(canvas, chest[0] - 24, chest[1] - 2, chest[0] - 14, chest[1] - 3, 0.6, 0.6) & body, darken(pick(ramp, 0.4), 0.2), 0, -1, 1)
    out = body | roll
    contour(canvas, out, (14, 16, 15))
    return out


# --------------------------------------------------------------------------
# assembly
# --------------------------------------------------------------------------
def render_survivor(frame=0, gear=None, hostile=False, hurt=False):
    gear = gear or {}
    jacket = gear.get("chest") == "jacket"
    vest = gear.get("chest") == "vest"
    hazmat = gear.get("chest") == "hazmat"
    pants = gear.get("legs") == "pants"
    boots = gear.get("feet") == "boots"
    helmet = gear.get("head") == "helmet"
    pack = gear.get("back") == "pack"

    c = Canvas(SURVIVOR_W, SURVIVOR_H)
    p = survivor_pose(frame, hurt)

    leg_ramp = "hazmat" if hazmat else ("cloth" if pants else "denim")
    boot_ramp = "grey" if hazmat else ("cloth" if (pants and not boots) else "rust")

    leg(c, p["hip"], p["knee_back"], p["ankle_back"], leg_ramp, far=True)
    boot(c, p["ankle_back"], p["foot_back"], boot_ramp, far=True, tall=boots or hazmat)
    arm(c, p["sh_back"], p["elb_back"], p["wr_back"], leg_ramp, far=True, glove=True)
    if pack:
        backpack(c, p)
    torso(c, p, leg_ramp, vest=vest, jacket=jacket and not hazmat, hazmat=hazmat)
    head(c, p["head"], hair_style="short", helmet=helmet, hazmat=hazmat, hurt=hurt)

    leg(c, p["hip"], p["knee_front"], p["ankle_front"], leg_ramp)
    boot(c, p["ankle_front"], p["foot_front"], boot_ramp, tall=boots or hazmat)
    front_arm = arm(c, p["sh_front"], p["elb_front"], p["wr_front"], leg_ramp, glove=True)
    c.seam(ring(front_arm, -1, 0, 1), (12, 14, 13), 0, 0, 1)

    c.outline(INK)
    return c


def canvas_to_atlas(images, columns, cell_w, cell_h, path):
    """Pack equally sized RGBA sprites into a sheet for the Unity runtime atlas."""
    from PIL import Image
    rows = (len(images) + columns - 1) // columns
    sheet = Image.new("RGBA", (columns * cell_w, rows * cell_h), (0, 0, 0, 0))
    for index, img in enumerate(images):
        assert img.width <= cell_w and img.height <= cell_h, "sprite does not fit the cell"
        cx = (index % columns) * cell_w + (cell_w - img.width) // 2
        cy = (index // columns) * cell_h + (cell_h - img.height)
        sheet.paste(img, (cx, cy), img)
    sheet.save(path)
    return sheet
