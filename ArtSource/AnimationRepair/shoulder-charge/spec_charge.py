"""Szarża barkiem (unik klas z DodgeStyle.ShoulderCharge: berserker, wojownik, paladyn HU) – 4 kierunki × 2 układy rąk.
Baza: „Pro Longbow Pack / standing dodge forward|backward|left|right” (Mixamo). Gra: cały klip w 0,82 s (actionDuration),
przemieszczenie daje PlayerController (0,6 s, profil 1−u², 3,7 m); w fazie uniku kula przed postacią rani i zachwiewa.
- przód: lewy bark prowadzi (ciało bokiem), niskie wejście z pochyleniem zamiast podskoku;
- lewo / prawo: ciało przodem do kamery – prowadzi bark po stronie ruchu, przechył w ruch;
- tył: ciężki odskok, ręce wypychają broń/tarczę do przodu.
Ręce: poza z klipu bloku (broń dwuręczna – „great sword blocking (2)”, tarcza – „sword and shield block”),
lokalnie względem klatki piersiowej. Kąty: + = w prawo postaci; skłon + = w przód; obniżenie bioder w cm.
"""
import bpy, math
from mathutils import Vector, Matrix

DIR = globals().get("CHARGE_DIR", "fwd")
ARMS = globals().get("CHARGE_ARMS", "twohand")
_src = bpy.data.objects["Rig_" + DIR]
_act = _src.animation_data.action
F0, F1 = int(_act.frame_range[0]), int(_act.frame_range[1])
_sc = bpy.context.scene


def _hips_y(f):
    _sc.frame_set(f)
    return _src.pose.bones[P + "Hips"].matrix.translation.y


_HY = {f: _hips_y(f) for f in range(F0, F1 + 1)}
_BASE = _HY[F0]


def _env(f, a, b, c, d):
    """0 → 1 (a–b), 1 (b–c), 1 → 0 (c–d), wygładzone."""
    if f <= a or f >= d: return 0.0
    if f < b: t = (f - a) / (b - a)
    elif f <= c: return 1.0
    else: t = (d - f) / (d - c)
    return t * t * (3 - 2 * t)


# kierunek: (skręt miednicy, skręt tułowia, skłon, przechył, obniżenie cm, koniec napędu – klatka)
PRESET = {
    "fwd":   dict(py=20, ty=30, tp=18, tr=0, drop=6, drive=(1, 4, 20, 30)),
    "left":  dict(py=-58, ty=-62, tp=10, tr=-12, drop=6, drive=(1, 4, 19, 29)),
    "right": dict(py=-58, ty=-62, tp=10, tr=12, drop=6, drive=(1, 4, 19, 29)),
    "back":  dict(py=-35, ty=-40, tp=8, tr=0, drop=4, drive=(1, 4, 26, 40)),
}[DIR]
_D = PRESET["drive"]


def env(f): return _env(f, *_D)


def drop(f):
    # podskok z nagrania znoszony (biodra nie wyżej niż na początku) + niskie wejście
    hop = max(0.0, _HY.get(int(round(time_map(f))), _BASE) - _BASE)   # wysokość z klatki nagrania (po przemapowaniu czasu)
    return hop * env(f) + PRESET["drop"] * env(f)


# Ręce: broń dwuręczna niesiona nisko przy prawej piersi (bieg z mieczem dwuręcznym) – lewy bark prowadzi, broń z tyłu;
# tarcza – wysunięta przed bark (blok tarczą).
ARMS_FROM = {"twohand": ("Rig_gsrun", 7.0), "shield": ("Rig_ssblock", 8.0)}[ARMS]

# Przemapowanie czasu pod ruch z gry (PlayerController: 3,7 m w 0,6 s, profil 1−u²; klip w 0,82 s):
# odbicie ≈ 0,06 s gry (~12% drogi), lot do ≈ 0,42 s (~80% drogi bez kontaktu stóp), lądowanie przy wyhamowaniu.
# Dane nagrania (stopy nad podłogą): przód – oderwanie 14, lądowanie 21; tył 13/21; lewo 11/20; prawo 11/19.
_EV = {"fwd": (6, 14, 21), "back": (6, 13, 21), "left": (5, 11, 20), "right": (5, 11, 19)}[DIR]
_N = {"fwd": 31, "back": 36, "left": 30, "right": 30}[DIR]          # klatki odtwarzane w grze (tył – przycięty do 36)
_TOFF, _TLAND, _TOTAL = 0.06, 0.42, 0.82


def _out(t): return 1 + t / _TOTAL * (_N - 1)


_TM = [(1, float(_EV[0])), (_out(_TOFF), float(_EV[1])), (_out(_TLAND), float(_EV[2])), (_N, float(_N))]


def time_map(f):
    if f >= _N: return float(min(f, F1))
    for (a, ta), (b, tb) in zip(_TM, _TM[1:]):
        if f <= b: return ta + (tb - ta) * (f - a) / (b - a)
    return float(_N)

SPEC = {
    "time_map": time_map,
    "pelvis_yaw": lambda f: PRESET["py"] * env(f), "torso_yaw": lambda f: PRESET["ty"] * env(f),
    "torso_pitch": lambda f: PRESET["tp"] * env(f), "torso_roll": lambda f: PRESET["tr"] * env(f),
    "drop": drop, "feet_follow": lambda f: 1.0,
    "weapon": lambda f, W0, hips: W0, "hands_ik": False,
    "arms_from": ARMS_FROM, "arms_weight": lambda f: _env(f, F0, F0 + 3, _D[2] + 4, F1 + 6),
}
