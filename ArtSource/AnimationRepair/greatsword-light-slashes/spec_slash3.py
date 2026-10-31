"""Specyfikacja naprawy „great sword slash (3)” → lekki 2 (CleaveBack) i cięcie z lewej miecza dwuręcznego (SlashLeft).
Klatki Blendera 1–56 (= Unity 0–55). Nagranie: skręt w lewo z krokiem prawą nogą (≈0,83 m), ostrze nisko z lewej
przez przód w górę na prawo (kontakt ok. 25–31), potem prawa noga cofa się o metr do gardy – w grze postać odjeżdżała do tyłu.
Naprawa zakończenia: prawa stopa zostaje tam, gdzie stanęła (klatka 33), ciało przechodzi nad nią, lewa stopa robi krok
po łuku; garda kończy się o długość kroku dalej (jak dawny „slash (4)”, ale bez obrotu o 360°).
Rozstaw dłoni na trzonku jak w lekkim 1 (uwaga użytkownika: „nieco większy”).
"""
import bpy
from mathutils import Vector, Matrix

_src = bpy.data.objects["Rig_slash3"]
_sc = bpy.context.scene


def _foot(f, side):
    _sc.frame_set(f)
    return _src.pose.bones[P + side + "Foot"].matrix.copy()


PLANT_F = 22                                     # prawa stopa stoi od postawienia (w nagraniu 21–33 bez ruchu)
_RF_PLANT = _foot(PLANT_F, "Right")
_RF_END = _foot(56, "Right")
STEP = (_foot(33, "Right").translation - _RF_END.translation)
STEP.y = 0.0                                     # tylko w poziomie
STEP_F0, STEP_F1 = 34, 48                        # przejście ciała nad prawą stopą
LIFT_F0, LIFT_F1, LIFT_CM = 35, 46, 12.0         # krok lewą: oderwanie i postawienie
_LF_LAND = _foot(LIFT_F1, "Left")


def _hips_fwd(f):
    _sc.frame_set(int(f), subframe=f - int(f))
    return _src.pose.bones[P + "Hips"].matrix.translation.z


HOLD_F = 28                                       # po kontakcie biodra już się nie cofają
_H_HOLD = _hips_fwd(HOLD_F)
_H_END = _hips_fwd(56) + STEP.z
_H_ORIG = {f: _hips_fwd(f) for f in range(1, 57)}   # wcześniej – bez zmiany klatki sceny w trakcie pieczenia


def root_offset(f):
    """Przesunięcie (wzdłuż kroku) takie, by biodra od klatki 28 szły tylko do przodu: lekko trzymają, potem nad prawą stopą."""
    if f <= HOLD_F:
        return Vector((0, 0, 0))
    want = interp([(HOLD_F, _H_HOLD), (37, _H_HOLD + 4.0), (STEP_F1, _H_END), (56, _H_END)], f)
    return STEP * ((want - _H_ORIG[f]) / STEP.z)


def foot_target(f, side, m0, off):
    if side == "R" and f >= PLANT_F:
        # stopa podporowa: stoi w miejscu z klatki 33, obrót z oryginału przechodzi płynnie do obrotu z końca
        t = _RF_PLANT.copy()
        return t
    if side == "L":
        # lewa stopa stoi do oderwania, w powietrzu przechodzi o długość kroku, potem stoi (bez ślizgu po podłodze)
        import math
        u = min(1.0, max(0.0, (f - LIFT_F0) / (LIFT_F1 - LIFT_F0)))
        s = u * u * (3 - 2 * u)
        if f >= LIFT_F1:
            m0 = _LF_LAND                                   # po postawieniu stopa stoi (w nagraniu dalej się przesuwała)
        m = Matrix.Translation(STEP * s) @ m0
        if 0.0 < u < 1.0:
            m = Matrix.Translation(UP * (LIFT_CM * math.sin(math.pi * u))) @ m
        return m
    return None


K_D = [(1, 0), (18, 0), (24, -10), (32, -10), (38, 0), (56, 0)]   # wykrok nagrania opuszcza biodra o ~18 cm – płycej o 10 cm (lekka seria bez głębokiego przysiadu, PaladinAnimePlayTests)


def weapon(f, W0, hips):
    return Matrix.Translation((0, -interp(K_D, f), 0)) @ W0   # broń schodzi razem z biodrami


zero = lambda f: 0.0
SPEC = {"pelvis_yaw": zero, "torso_yaw": zero, "drop": lambda f: interp(K_D, f), "weapon": weapon,
        "root_offset": root_offset, "foot_target": foot_target,
        "grip_span": 30.0, "grip_reach": 0.95, "grip_slide": 10.0, "grip_fallback": "original"}
