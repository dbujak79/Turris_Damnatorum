"""Szarża barkiem, wersja 2 (uwaga użytkownika: „szarża to raczej skręt tułowia tak, żeby bark był z przodu, i prawie normalny bieg,
a nie bieg bokiem”). Baza: biegi z zestawów (miecz dwuręczny / miecz z tarczą): przód – bieg, boki – bieg bokiem (strafe), tył – bieg w tył.
Gra: klip w 0,82 s; PlayerController przesuwa postać o 3,7 m w 0,6 s (profil prędkości 1−u²), potem stoi.
Czas nagrania prowadzony drogą: przebyta droga w nagraniu = droga w grze (stopy podporowe nie jadą po podłodze) –
szybki sprint na starcie, wyhamowanie do zatrzymania w 0,6 s; nagranie zapętlone (wrap).
- przód: tułów skręcony w prawo – lewy bark z przodu, pochylenie w ruch, głowa kontruje skręt (wzrok przed siebie);
- boki: bark po stronie ruchu prowadzi w samym biegu bokiem, przechył w ruch;
- tył: bieg w tył, ręce wypychają broń / tarczę przed siebie (poza bloku).
"""
import bpy, math
from mathutils import Vector

DIR = globals().get("CHARGE_DIR", "fwd")
ARMS = globals().get("CHARGE_ARMS", "twohand")
KEY = ("gs_" if ARMS == "twohand" else "ss_") + DIR
SRC = "Run_" + KEY
_src = bpy.data.objects[SRC]
_act = _src.animation_data.action
A0, A1 = int(_act.frame_range[0]), int(_act.frame_range[1])
_sc = bpy.context.scene
_sc.frame_set(A0); _h0 = _src.pose.bones[P + "Hips"].matrix.translation.copy()
_sc.frame_set(A1); _h1 = _src.pose.bones[P + "Hips"].matrix.translation.copy()
DISP = Vector((_h1.x - _h0.x, 0.0, _h1.z - _h0.z))        # droga cyklu (cm, przestrzeń szkieletu)
SPEED = DISP.length / (A1 - A0)                             # cm na klatkę nagrania

N = 25                                                      # klatek wyniku: 0,82 s przy 30 kl./s
TOTAL, MOVE, DIST = 0.82, 0.6, 4.62 * 0.805 * 100.0        # s, s, cm (BuildSnapshot: szarża ×0,805 dystansu uniku)


def _game_dist(t):
    if t >= MOVE: return DIST
    u = t / MOVE
    return DIST * 1.5 * (u - u ** 3 / 3.0)


def time_map(f):
    t = (f - 1) / (N - 1) * TOTAL
    return A0 + _game_dist(t) / SPEED


def _env(f, a, b, c, d):
    if f <= a or f >= d: return 0.0
    if f < b: t = (f - a) / (b - a)
    elif f <= c: return 1.0
    else: t = (d - f) / (d - c)
    return t * t * (3 - 2 * t)


def env(f): return _env(f, 0.0, 2.5, 18.0, 25.5)


PRESET = {
    "fwd":   dict(py=5, ty=40, tp=14, tr=0, drop=4),
    "left":  dict(py=0, ty=-10, tp=8, tr=-10, drop=3),
    "right": dict(py=0, ty=10, tp=8, tr=10, drop=3),
    "back":  dict(py=0, ty=0, tp=6, tr=0, drop=3),
}[DIR]

SPEC = {
    "time_map": time_map, "wrap": (float(A0), float(A1), DISP),
    "pelvis_yaw": lambda f: PRESET["py"] * env(f), "torso_yaw": lambda f: PRESET["ty"] * env(f),
    "torso_pitch": lambda f: PRESET["tp"] * env(f), "torso_roll": lambda f: PRESET["tr"] * env(f),
    "drop": lambda f: PRESET["drop"] * env(f), "feet_follow": lambda f: 1.0, "head_counter": 0.6,
    "weapon": lambda f, W0, hips: W0, "hands_ik": False,
}
if DIR == "back":
    # wypchnięcie broni / tarczy przed siebie w biegu w tył
    SPEC["arms_from"] = {"twohand": ("Rig_gsblock", 15.0), "shield": ("Rig_ssblock", 8.0)}[ARMS]
    SPEC["arms_weight"] = lambda f: _env(f, 0.0, 3.0, 20.0, 26.0)
