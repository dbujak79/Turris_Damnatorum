"""Specyfikacja naprawy „great sword slash” (lekki 1: Cleave / SlashRight). Klatki Blendera 1–39 (= klatki Unity 0–38).
Znaczniki gry bez zmian: koniec zamachu 14, koniec fazy aktywnej 23; w grze klip kończy się ok. klatki 34 (przenikanie do gardy).
Fazy: przygotowanie 1–16 (zwinięcie w prawo), przyspieszenie 17–20, kontakt 20–22 (ostrze przez przód, ukośnie w dół),
dociągnięcie 22–27 (ostrze nisko w lewo, tułów za ciosem, krótkie zatrzymanie), powrót 27–34 do gardy klipu.
Stopy: z oryginału (prawa z tyłu, lewa wykroczna). Kąty: + = w prawo postaci; skłon + = do przodu; obniżenie w cm.
"""
from mathutils import Vector

K_T = [(1, 0), (9, 12), (16, 22), (19, 8), (21, -18), (24, -35), (27, -35), (33, -8), (36, 0), (39, 0)]   # skręt tułowia
K_P = [(1, 0), (10, 6), (16, 10), (20, 0), (23, -14), (27, -14), (33, -3), (36, 0), (39, 0)]             # skręt miednicy
K_D = [(1, 0), (17, 0), (22, 0), (27, 0), (33, 0), (36, 0), (39, 0)]                                       # biodra w dół (płytko – lekka seria bez głębokiego przysiadu)
K_TP = [(1, 0), (19, 0), (23, 6), (27, 6), (33, 1), (36, 0), (39, 0)]                                    # skłon tułowia
K_EY = [(1, 0), (21, 0), (24, -10), (27, -10), (33, 0), (39, 0)]                                           # broń: dodatkowo w lewo
K_EP = [(1, 0), (21, 0), (23, -20), (27, -24), (33, 0), (39, 0)]                                           # broń: ostrze w dół


def weapon(f, W0, hips):
    T = interp(K_T, f); d = interp(K_D, f); ey = interp(K_EY, f); ep = interp(K_EP, f)
    W = Matrix.Translation((0, -d, 0)) @ rot_about(hips, UP, -T) @ W0
    W = rot_about(W.translation.copy(), UP, -ey) @ W   # obrót ostrza wokół chwytu – ręce nie są wypychane
    shaft = (W.to_3x3() @ Vector((0, 0, 1))).normalized()
    axis = shaft.cross(UP)
    if axis.length > 1e-4:
        W = rot_about(W.translation.copy(), axis.normalized(), ep) @ W
    return W


SPEC = {"pelvis_yaw": lambda f: interp(K_P, f), "torso_yaw": lambda f: interp(K_T, f), "drop": lambda f: interp(K_D, f),
        "torso_pitch": lambda f: interp(K_TP, f), "weapon": weapon,
        # rozstaw dłoni na trzonku (cm, między środkami dłoni); oryginał miecza ~12 cm i lewa dłoń obok osi (uwaga użytkownika: „nieco większy”)
        "grip_span": 30.0, "grip_reach": 0.95, "grip_slide": 10.0, "grip_fallback": "original"}
