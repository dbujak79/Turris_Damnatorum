"""Lekki z biegu (RunningSweep) – poziome cięcie nisko przez przód z rozpędu, bez obrotu ciała.
Baza: „great sword slash (2)” kl. 1–45 (Unity 0–44; znaczniki gry 19/27): krok lewą w przód zostaje (pasuje do rozpędu z gry),
zamach wysoko za głową i obrót miednicy o −121° z przekrokiem prawą nogą usunięte.
Uruchamiać po exec(spec_chop.py) (pomocnicze C, _frame, _lin, _blend; kalibracja broni z klipu ciężkiego – sprawdzana w grze).
Topór: tor zaprojektowany – z niesienia w biegu (nisko po prawej, ostrze w tył i w dół) zamach w tył-prawo poziomo,
cięcie poziome na wysokości bioder przez przód (kontakt ~kl. 23, w oknie trafienia 20–28), dociągnięcie nisko w lewo; ostrze prowadzi ruch.
Miednica: zwinięcie do 40° w prawo, po cięciu najwyżej 40° w lewo; prawa stopa robi zwykły krok (~75 cm) zamiast przekroku.
"""
import bpy, math
from mathutils import Vector, Matrix

_src = bpy.data.objects["Rig_sweep"]
_sc = bpy.context.scene


def _sample(f):
    _sc.frame_set(f)
    return _src.pose.bones[P + "Hips"].matrix.translation.z, _src.pose.bones[P + "RightFoot"].matrix.copy()


_HZ = {}
for _f in range(1, 46):
    _HZ[_f] = _sample(_f)[0]
_RF25 = _sample(25)[1]

_FACE_ORIG = [(1, 16), (10, 36), (16, 58), (19, 56), (22, 33), (25, 5), (28, -18), (31, -48), (34, -83), (37, -110), (40, -121), (45, -121)]
_FACE_WANT = [(1, 16), (10, 30), (16, 40), (19, 38), (22, 20), (25, 0), (28, -20), (31, -35), (34, -40), (45, -40)]


def _plin(keys, f):
    for (a, va), (b, vb) in zip(keys, keys[1:]):
        if f <= b:
            return va + (vb - va) * (f - a) / (b - a)
    return keys[-1][1]


def pelvis_yaw(f): return _plin(_FACE_WANT, f) - _plin(_FACE_ORIG, f)


_CHEST = [(1, 0), (16, 10), (21, 0), (26, -10), (34, -5), (45, 0)]


def torso_yaw(f): return pelvis_yaw(f) + _lin(_CHEST, f)


# ciało nie wybiega za daleko przed stopy (prawa robi krok ~75 cm, nie 1,5 m)
_HIPS_WANT = [(25, None), (36, 70.0), (45, 72.0)]


def root_offset(f):
    if f <= 25: return Vector((0, 0, 0))
    h25 = _HZ[25]
    want = h25 + (70.0 - h25) * min(1.0, (f - 25) / 11.0) if f <= 36 else 70.0 + 2.0 * (f - 36) / 9.0
    fi = min(45, max(1, int(round(f))))
    return Vector((0, 0, want - _HZ[fi]))


def foot_target(f, side, m0, off):
    if side == "L":
        return m0                                     # krok lewą z nagrania
    if f <= 25:
        return m0
    u = min(1.0, (f - 25) / 10.0); s = u * u * (3 - 2 * u)
    lift = 10.0 * math.sin(math.pi * u) if u < 1 else 0.0
    return Matrix.Translation(Vector((0, lift, 75.0 * s))) @ _RF25


# klucze broni: klatka, chwyt (prawo, góra, przód) względem bioder w poziomie i podłogi w pionie [cm], trzonek (prawo, góra, przód)
KEYS = [
    (1, (25, 100, 5), (0.30, -0.35, -0.90)),
    (8, (32, 105, -12), (0.55, -0.05, -0.83)),
    (16, (38, 108, -10), (0.85, 0.05, -0.50)),
    (20, (30, 102, 20), (0.98, 0.0, 0.15)),
    (23, (8, 98, 42), (0.15, -0.10, 0.99)),
    (26, (-22, 96, 32), (-0.90, -0.10, 0.40)),
    (31, (-32, 95, 10), (-0.80, -0.30, -0.50)),
    (36, (-32, 95, 10), (-0.80, -0.30, -0.50)),
]


def _key_at(f):
    if f <= KEYS[0][0]: k0 = k1 = KEYS[0]; t = 0.0
    elif f >= KEYS[-1][0]: k0 = k1 = KEYS[-1]; t = 0.0
    else:
        for k0, k1 in zip(KEYS, KEYS[1:]):
            if f <= k1[0]: break
        t = (f - k0[0]) / (k1[0] - k0[0]); t = t * t * (3 - 2 * t)
    g = Vector(k0[1]).lerp(Vector(k1[1]), t)
    s0, s1 = C(*k0[2]).normalized(), C(*k1[2]).normalized()
    s = s0.slerp(s1, t) if s0.dot(s1) > -0.999 else s1
    return Vector((-g.x, g.y, g.z)), s.normalized()   # (prawo, góra, przód) → szkielet


def weapon(f, W0, hips):
    g, s = _key_at(f)
    _, s_a = _key_at(f - 0.5); _, s_b = _key_at(f + 0.5)
    edge = (s_b - s_a)
    if edge.length < 1e-5:
        edge = C(0, -1, 0)                            # w zatrzymaniu – ostrze w dół
    grip = Vector((hips.x + g.x, g.y, hips.z + g.z))
    Wk = _frame(grip, s, edge)
    return Wk   # do końca klipu tor zaprojektowany (w nagraniu ciało obrócone o −121° – powrót do gardy daje przejście w grze)


SPEC = {"root_offset": root_offset, "foot_target": foot_target,
        "pelvis_yaw": pelvis_yaw, "torso_yaw": torso_yaw,
        "torso_pitch": lambda f: _lin([(1, 4), (16, 0), (23, 10), (31, 10), (45, 4)], f),
        "drop": lambda f: _lin([(1, 0), (20, 2), (24, 8), (31, 8), (45, 3)], f),
        "feet_follow": lambda f: 0.0,
        "weapon": weapon, "grip_span": 30.0, "grip_reach": 0.95, "grip_slide": 10.0}
