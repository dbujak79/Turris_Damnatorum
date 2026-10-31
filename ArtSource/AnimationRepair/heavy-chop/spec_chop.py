"""Specyfikacja ciężkiego ciosu topora/młota (Chop) z ładowaniem – na bazie „Standing Melee Attack Downward” (Mixamo Loose).
Klatki Blendera 1–69 = klatki Unity 0–68 (30 kl./s). Znaczniki gry bez zmian: koniec zamachu 24, koniec fazy aktywnej 29.
Gra zatrzymuje ładowanie w 80% przygotowania: klatka klipu ≈ 0,8 × (24 − 0,5) = 18,8 (Blender ≈ 19,8).

Życzenie użytkownika: „ładowanie polega na tym, że postać wycofuje topór do tyłu i na dół, w trakcie ładowania lekkie drganie
(warstwa ChargeTremble w kodzie), a po zakończeniu następuje potężne uderzenie”.
- Przygotowanie 1–19,5: wolne cofnięcie (nagranie 1–9,5 rozciągnięte), pogłębione: tułów zwinięty w prawo, biodra niżej,
  dłonie z tyłu nisko, trzonek w tył i w dół, ostrze zwrócone w kierunku dalszego ruchu.
- 19,5–25: szybki łuk górą za plecami (nagranie 9,5–25, ~2,8× szybciej).
- 25–28: kontakt – ostrze w ziemię na wprost, skłon i przysiad (dawniej dokładał to GreatSwingPose).
- 28–34: zatrzymanie w uderzeniu; 34–52: powrót z nagrania.
Oś trzonka i ostrza w dłoni Mixamo – kalibracja z gry dla tego klipu (TestResults/berserker-anim-review/raw-heavy).
"""
import math
from mathutils import Vector, Matrix, Quaternion

AXE_AXIS = Vector((0.744, 0.618, 0.255)).normalized()
AXE_BLADE = Vector((-0.619, 0.78, -0.097)).normalized()

# klatka wyniku → klatka nagrania
_TM = [(1, 1.0), (19.5, 9.5), (25, 25.0), (69, 69.0)]


def time_map(f):
    for (a, ta), (b, tb) in zip(_TM, _TM[1:]):
        if f <= b:
            return ta + (tb - ta) * (f - a) / (b - a)
    return _TM[-1][1]


def _lin(keys, f):
    if f <= keys[0][0]:
        return keys[0][1]
    for (f0, v0), (f1, v1) in zip(keys, keys[1:]):
        if f <= f1:
            t = (f - f0) / (f1 - f0); t = t * t * (3 - 2 * t)
            return v0 + (v1 - v0) * t
    return keys[-1][1]


K_T = [(1, 0), (17, 8), (20, 8), (23, 4), (27, -5), (34, -5), (45, 0), (69, 0)]      # skręt tułowia (+ w prawo)
K_P = [(1, 0), (17, 3), (20, 3), (25, -5), (34, -5), (45, 0), (69, 0)]                 # skręt miednicy
K_TP = [(1, 0), (17, 5), (20, 5), (23, -5), (28, 30), (34, 30), (44, 6), (52, 0), (69, 0)]  # skłon (+ w przód)
K_D = [(1, 0), (17, 6), (20, 6), (23, 4), (28, 18), (34, 18), (44, 4), (52, 0), (69, 0)]  # biodra w dół, cm
W_CHARGE = [(1, 0), (4, 0), (17, 1), (20, 1), (25, 0), (69, 0)]   # obrót ostrza z dołu do przodu rozłożony na łuk górą                         # udział pozy ładowania
W_HIT = [(1, 0), (24, 0), (27.5, 1), (34, 1), (44, 0), (69, 0)]                           # udział pozy uderzenia

# Przestrzeń szkieletu: prawo postaci = −X, góra = +Y, przód = +Z (cm). Wektor (prawo, góra, przód) → szkielet:
def C(r, u, f):
    return Vector((-r, u, f))


def _frame(grip, shaft, edge):
    z = shaft.normalized(); x = (edge - z * edge.dot(z)).normalized(); y = z.cross(x)
    m = Matrix((x, y, z)).transposed().to_4x4(); m.translation = grip
    return m


def _tangent(shaft):
    """Ostrze w kierunku ruchu łuku (obrót w płaszczyźnie przód–góra, kąt malejący: tył→góra→przód→dół)."""
    u, f = shaft.y, shaft.z
    th = math.atan2(u, f)
    return Vector((0.0, -math.cos(th), math.sin(th)))


_CH_SHAFT = C(0.2, -0.6, -0.77).normalized()
def _edge_down(shaft, out_deg=35.0):
    """Ostrze w dół (prostopadle do trzonka), odchylone o out_deg na zewnątrz (w prawo postaci) – uwaga użytkownika:
    „ostrze skierowane w dół, a nie w górę, raczej pod kątem, nie idealnie w dół”."""
    z = shaft.normalized()
    down = Vector((0, -1, 0)); down = (down - z * down.dot(z)).normalized()
    side = C(1, 0, 0); side = (side - z * side.dot(z) - down * side.dot(down)).normalized()
    a = math.radians(out_deg)
    return down * math.cos(a) + side * math.sin(a)


# Pozycję ładowania robią ręce (uwaga: „postać ma ręce, tułów nie musi się aż tak skręcać”): dłonie z tyłu przy prawym biodrze.
CHARGE = _frame(C(24, 98, -20), _CH_SHAFT, _edge_down(_CH_SHAFT))
_HIT_SHAFT = C(0.0, -0.72, 0.70).normalized()
HIT = _frame(C(6, 72, 62), _HIT_SHAFT, _tangent(_HIT_SHAFT))   # ostrze tuż nad podłożem


def _blend(A, B, w):
    if w <= 0: return A
    if w >= 1: return B
    q = A.to_quaternion().slerp(B.to_quaternion(), w)
    m = q.to_matrix().to_4x4(); m.translation = A.translation.lerp(B.translation, w)
    return m


AXE_LEN, TIP_CLEAR = 97.8, 25.0   # cm: grot topora od chwytu; najmniejszy prześwit czubka przy cofaniu


def weapon(f, W0, hips):
    d = _lin(K_D, f)
    W = Matrix.Translation((0, -d, 0)) @ W0
    W = _blend(W, CHARGE, _lin(W_CHARGE, f))
    W = _blend(W, HIT, _lin(W_HIT, f))
    if f < 24:
        # przy cofaniu czubek nie szura po podłodze – broń w górę o brakujący prześwit
        tip_y = (W @ Vector((0, 0, AXE_LEN))).y
        if tip_y < TIP_CLEAR:
            W = Matrix.Translation((0, TIP_CLEAR - tip_y, 0)) @ W
    return W


SPEC = {"time_map": time_map,
        "pelvis_yaw": lambda f: _lin(K_P, f), "torso_yaw": lambda f: _lin(K_T, f),
        "torso_pitch": lambda f: _lin(K_TP, f), "drop": lambda f: _lin(K_D, f),
        "weapon": weapon, "grip_span": 30.0, "grip_reach": 0.95, "grip_slide": 10.0}
