"""Ciężki z biegu (LeapChop) – wyskok z rozpędu z toporem nad głową i lądowanie z rąbnięciem w ziemię, bez obrotu
(dawniej „great sword jump attack” jako styl bazowy Leap – skok z obrotem). Uruchamiać po exec(spec_chop.py).
Gra: ciężki cios z biegu – rozpęd 4,5 m/s wygasa do końca fazy aktywnej, fala uderzeniowa w miejscu uderzenia.
Klatki 1–69 = Unity 0–68, znaczniki jak w ciężkim: koniec zamachu 24, koniec fazy aktywnej 29.
- 1–4: przysiad na odbicie, topór w górę (nagranie 11→); 4–22: lot (biodra do +30 cm, nogi podkurczone, 50 cm w przód),
  topór za głową; 22–25: lądowanie; 25–28: rąbnięcie w ziemię (poza HIT), przysiad; 28–34 zatrzymanie; powrót z nagrania.
"""
import math
from mathutils import Vector, Matrix

_TML = [(1, 11.0), (16, 20.0), (24, 24.5), (25, 25.0), (69, 69.0)]


def time_map(f):
    for (a, ta), (b, tb) in zip(_TML, _TML[1:]):
        if f <= b:
            return ta + (tb - ta) * (f - a) / (b - a)
    return 69.0


AIR0, AIR1, PEAK, FWD = 4.0, 22.0, 30.0, 50.0


def _air(f):
    """Udział lotu 0–1 (sinus) i postęp w przód 0–1."""
    if f <= AIR0: return 0.0, 0.0
    if f >= AIR1: return 0.0, 1.0
    u = (f - AIR0) / (AIR1 - AIR0)
    return math.sin(math.pi * u), u * u * (3 - 2 * u)


def root_offset(f):
    h, p = _air(f)
    return Vector((0, PEAK * h, FWD * p))


def foot_target(f, side, m0, off):
    h, p = _air(f)
    tuck = 12.0 * h if side == "R" else 8.0 * h                 # nogi podkurczone w locie
    return Matrix.Translation(Vector((0, PEAK * h + tuck, FWD * p))) @ m0


K_T = [(1, 0), (10, 8), (20, 8), (25, -3), (34, -3), (45, 0), (69, 0)]
K_P = [(1, 0), (10, 4), (20, 4), (25, -4), (34, -4), (45, 0), (69, 0)]
K_TP = [(1, 6), (8, -8), (20, -5), (23, 10), (28, 32), (34, 32), (44, 6), (52, 0), (69, 0)]
K_D = [(1, 0), (3, 10), (5, 2), (21, 2), (24, 10), (28, 20), (34, 20), (44, 4), (52, 0), (69, 0)]
W_HITL = [(1, 0), (24, 0), (27.5, 1), (34, 1), (44, 0), (69, 0)]


def weapon(f, W0, hips):
    W = Matrix.Translation((0, -_lin(K_D, f), 0)) @ W0          # W0 już przesunięte o root_offset (bake)
    return _blend(W, Matrix.Translation(Vector((0, 0, FWD))) @ HIT, _lin(W_HITL, f))


SPEC = {"time_map": time_map, "root_offset": root_offset, "foot_target": foot_target,
        "pelvis_yaw": lambda f: _lin(K_P, f), "torso_yaw": lambda f: _lin(K_T, f),
        "torso_pitch": lambda f: _lin(K_TP, f), "drop": lambda f: _lin(K_D, f),
        "weapon": weapon, "grip_span": 30.0, "grip_reach": 0.95, "grip_slide": 10.0}
