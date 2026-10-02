"""Shared limited palette for SUBSISTENCE 2D pixel art.

Everything is authored against these ramps so that every sprite in the game
shares one coherent, desaturated Backrooms-industrial palette.
"""
from __future__ import annotations

# --- ramps: dark -> light -------------------------------------------------
SKIN = [(74, 49, 38), (108, 71, 52), (146, 99, 70), (183, 133, 96), (211, 164, 121), (231, 195, 155)]
OLIVE = [(26, 33, 27), (40, 50, 38), (58, 70, 52), (78, 92, 68), (100, 115, 86), (126, 140, 106)]
YELLOW = [(60, 45, 22), (95, 71, 32), (135, 101, 44), (176, 135, 60), (208, 169, 85), (231, 199, 124)]
GREY = [(28, 33, 33), (45, 52, 51), (66, 74, 72), (90, 99, 96), (117, 126, 121), (148, 156, 148)]
RUST = [(52, 26, 20), (84, 40, 27), (122, 60, 35), (160, 85, 47), (196, 117, 68), (222, 155, 101)]
TEAL = [(18, 27, 28), (27, 40, 41), (39, 56, 56), (55, 76, 74), (77, 101, 96), (104, 130, 121)]
AMBER = [(84, 49, 22), (129, 77, 30), (176, 111, 42), (214, 150, 63), (240, 189, 106), (255, 224, 166)]
CLOTH = [(62, 57, 44), (88, 82, 62), (117, 110, 84), (146, 139, 108), (176, 169, 136), (203, 197, 166)]
HAZMAT = [(78, 52, 12), (122, 83, 17), (166, 117, 25), (203, 152, 40), (231, 186, 73), (250, 219, 132)]
PAPER = [(26, 24, 18), (52, 45, 28), (82, 69, 40), (114, 95, 54), (146, 123, 70), (178, 152, 92)]
DENIM = [(20, 26, 30), (31, 39, 44), (45, 55, 60), (62, 74, 79), (84, 98, 101), (110, 124, 124)]
FLESH_PALE = [(58, 56, 50), (80, 82, 70), (104, 110, 90), (129, 139, 109), (154, 168, 130), (180, 196, 152)]

PALETTE = {
    "void": (10, 12, 12),
    "ink": (17, 21, 20),
    "deep": (24, 31, 30),
    "shadow": (33, 42, 39),
    "acid": (198, 240, 104),
    "blood": (118, 40, 28),
    "glass": (96, 128, 122),
    "paper": (203, 197, 166),
    "wire": (66, 74, 72),
}

RAMPS = {
    "skin": SKIN,
    "olive": OLIVE,
    "yellow": YELLOW,
    "grey": GREY,
    "rust": RUST,
    "teal": TEAL,
    "amber": AMBER,
    "cloth": CLOTH,
    "hazmat": HAZMAT,
    "paper": PAPER,
    "denim": DENIM,
    "flesh": FLESH_PALE,
}


def ramp(name: str):
    return RAMPS[name]


def pick(name: str, t: float):
    """Pick a colour from a ramp. t=0 darkest, t=1 lightest."""
    colors = RAMPS[name]
    t = 0.0 if t < 0 else 1.0 if t > 1 else t
    return colors[int(round(t * (len(colors) - 1)))]


def mix(a, b, t: float):
    t = 0.0 if t < 0 else 1.0 if t > 1 else t
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(3))


def darken(c, t: float):
    return mix(c, (12, 14, 14), t)


def lighten(c, t: float):
    return mix(c, (255, 246, 214), t)
