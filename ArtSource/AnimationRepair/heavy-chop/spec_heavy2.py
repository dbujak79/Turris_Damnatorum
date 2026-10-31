"""Ciężki 2 (ChopFollow) – drugie rąbnięcie z kroku dostawnego, bez obrotu ciała (dawniej „great sword high spin attack”, obrót o 360°).
Bazuje na spec_chop.py (ten sam klip źródłowy, kalibracja broni i poza uderzenia HIT). Uruchamiać po exec(spec_chop.py).
Klatki Blendera 1–69 = Unity 0–68, znaczniki gry jak w ciężkim 1: koniec zamachu 24, koniec fazy aktywnej 29.
- 1–24: topór wyrwany z ziemi (seria po ciężkim 1) idzie w górę nad prawym barkiem za głowę (nagranie 11–25, bez cofania w dół);
  krok dostawny: przednia (lewa) stopa 35 cm w przód (kl. 6–14), tylna dostawiona (kl. 14–21), ciało przechodzi za nimi;
- 25–28: rąbnięcie w ziemię przed postacią (poza HIT), 28–34 zatrzymanie, potem powrót z nagrania.
"""
import math
from mathutils import Vector, Matrix

_TM2 = [(1, 11.0), (18, 20.0), (24, 24.5), (25, 25.0), (69, 69.0)]


def time_map(f):
    for (a, ta), (b, tb) in zip(_TM2, _TM2[1:]):
        if f <= b:
            return ta + (tb - ta) * (f - a) / (b - a)
    return 69.0


K_T = [(1, -5), (14, 12), (20, 12), (24, 4), (27, -5), (34, -5), (45, 0), (69, 0)]
K_P = [(1, -5), (14, 4), (20, 4), (25, -5), (34, -5), (45, 0), (69, 0)]
K_TP = [(1, 20), (10, 0), (20, -6), (23, -4), (28, 30), (34, 30), (44, 6), (52, 0), (69, 0)]
K_D = [(1, 14), (10, 7), (20, 7), (23, 7), (28, 18), (34, 18), (44, 4), (52, 0), (69, 0)]
STEP_CM = 35.0
K_ROOT = [(1, 0), (8, 0), (22, 1), (69, 1)]                     # ciało za krokiem (udział STEP_CM)


def root_offset(f):
    return Vector((0, 0, STEP_CM * _lin(K_ROOT, f)))


def _arc(f, a, b, lift):
    u = min(1.0, max(0.0, (f - a) / (b - a)))
    s = u * u * (3 - 2 * u)
    return s, (lift * math.sin(math.pi * u) if 0 < u < 1 else 0.0)


def foot_target(f, side, m0, off):
    a, b, step = (5, 12, STEP_CM) if side == "L" else (11, 19, STEP_CM + 5.0)   # przednia stopa, potem tylna (dalej – nie zostaje w tyle)
    s, h = _arc(f, a, b, 9.0)
    return Matrix.Translation(Vector((0, h, step * s))) @ m0


W_HIT2 = [(1, 0), (24, 0), (27.5, 1), (34, 1), (44, 0), (69, 0)]


def weapon(f, W0, hips):
    d = _lin(K_D, f)
    W = Matrix.Translation((0, -d, 0)) @ W0          # W0 już przesunięte o root_offset (bake)
    W = _blend(W, Matrix.Translation(root_offset(f)) @ HIT, _lin(W_HIT2, f))
    return W


SPEC = {"time_map": time_map, "root_offset": root_offset, "foot_target": foot_target,
        "pelvis_yaw": lambda f: _lin(K_P, f), "torso_yaw": lambda f: _lin(K_T, f),
        "torso_pitch": lambda f: _lin(K_TP, f), "drop": lambda f: _lin(K_D, f),
        "weapon": weapon, "grip_span": 30.0, "grip_reach": 0.95, "grip_slide": 10.0}
