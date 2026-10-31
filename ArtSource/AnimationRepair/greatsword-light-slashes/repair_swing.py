"""Naprawa ciosów Mixamo (Great Sword Pack) – pieczenie w Blenderze bez constraintów.

Uruchamiane w Blenderze (MCP): exec(open(<ten plik>).read()); potem bake_repair(...).
Dla każdej klatki: poza oryginału (akcja źródłowa nie jest zmieniana) → obrót miednicy i tułowia wokół pionu,
obniżenie bioder → tor broni (kontroler w przestrzeni szkieletu) → dwukościowe IK rąk do chwytów na broni
(prawa dłoń – stały chwyt, lewa – chwyt z oryginału względem broni) → IK nóg do stóp z oryginału (stopy stoją).
Wynik: nowa akcja z kluczem co klatkę (lokacja/rotacja/skala wszystkich kości).

Przestrzeń szkieletu Mixamo po imporcie FBX: centymetry, oś +Y w górę, postać patrzy w +Z
(świat Blendera: obiekt obrócony o 90° wokół X, skala 0,01 – przód = −Y świata).
"""
import bpy, math
from mathutils import Vector, Matrix, Quaternion
from bpy_extras import anim_utils

UP = Vector((0, 1, 0))          # pion w przestrzeni szkieletu
P = "mixamorig:"
# Oś trzonka w przestrzeni prawej dłoni (kalibracja z gry, BerserkerAnimeReviewTests + Unity → Blender) i gniazdo chwytu.
AXE_AXIS = Vector((0.8825, 0.299, 0.359)).normalized()
SLOT_ALONG_HAND = 7.0           # cm wzdłuż kości dłoni (środek dłoni)


def rot_about(pivot, axis, deg):
    """Obrót sztywny wokół osi przez punkt (macierz 4×4, przestrzeń szkieletu)."""
    return Matrix.Translation(pivot) @ Quaternion(axis, math.radians(deg)).to_matrix().to_4x4() @ Matrix.Translation(-pivot)


def children_map(arm):
    ch = {b.name: [c.name for c in b.children] for b in arm.data.bones}
    return ch


def subtree(ch, name):
    out = [name]
    for c in ch[name]:
        out += subtree(ch, c)
    return out


def apply_rigid(M, ch, name, T):
    for n in subtree(ch, name):
        M[n] = T @ M[n]


def axe_local(arm):
    """Macierz broni względem macierzy kości prawej dłoni (przestrzeń szkieletu): +Z – trzonek ku ostrzu."""
    bl = arm.data.bones[P + "RightHand"].length
    R = Vector((0, 0, 1)).rotation_difference(AXE_AXIS).to_matrix().to_4x4()
    # w macierzy kości oś Y biegnie od nadgarstka; gniazdo SLOT_ALONG_HAND cm od nadgarstka
    return Matrix.Translation((0, SLOT_ALONG_HAND, 0)) @ R


def two_bone(M, ch, a, b, c, target, pole_hint=None):
    """IK: kość a (ramię/udo), b (przedramię/podudzie), c (dłoń/stopa – tylko pozycja). Płaszczyzna zgięcia z bieżącej pozy."""
    A = M[a].translation.copy(); B = M[b].translation.copy(); C = M[c].translation.copy()
    l1 = (B - A).length; l2 = (C - B).length
    d = target - A
    dist = max(1e-4, min(d.length, (l1 + l2) * 0.9995, ))
    dist = max(dist, abs(l1 - l2) + 1e-3)
    u = d.normalized()
    pole = (B - A) - u * (B - A).dot(u)
    if pole_hint is not None and pole.length < 1e-3:
        pole = pole_hint
    if pole.length < 1e-6:
        pole = Vector((0, 0, 1)) - u * u.z
    pole.normalize()
    cosA = (l1 * l1 + dist * dist - l2 * l2) / (2 * l1 * dist)
    cosA = max(-1.0, min(1.0, cosA))
    sinA = math.sqrt(1 - cosA * cosA)
    Bn = A + u * (l1 * cosA) + pole * (l1 * sinA)
    q1 = (B - A).rotation_difference(Bn - A)
    apply_rigid(M, ch, a, Matrix.Translation(A) @ q1.to_matrix().to_4x4() @ Matrix.Translation(-A))
    B2 = M[b].translation.copy(); C2 = M[c].translation.copy()
    tgt = A + u * dist
    q2 = (C2 - B2).rotation_difference(tgt - B2)
    apply_rigid(M, ch, b, Matrix.Translation(B2) @ q2.to_matrix().to_4x4() @ Matrix.Translation(-B2))
    return (M[c].translation - target).length


def set_world_rot(M, ch, name, R3):
    """Ustaw obrót kości (przestrzeń szkieletu) zachowując jej położenie; dzieci idą za nią sztywno."""
    m = M[name]
    new = R3.to_4x4(); new.translation = m.translation
    apply_rigid(M, ch, name, new @ m.inverted())


def pose_matrices(arm):
    return {pb.name: pb.matrix.copy() for pb in arm.pose.bones}


def to_basis(arm, M):
    out = {}
    for b in arm.data.bones:
        if b.parent is None:
            basis = b.matrix_local.inverted() @ M[b.name]
        else:
            rel = b.parent.matrix_local.inverted() @ b.matrix_local
            basis = rel.inverted() @ M[b.parent.name].inverted() @ M[b.name]
        out[b.name] = basis
    return out


def interp(keys, f):
    """Łamana z wygładzeniem (smoothstep między kluczami) – keys: [(klatka, wartość)]."""
    if f <= keys[0][0]:
        return keys[0][1]
    for (f0, v0), (f1, v1) in zip(keys, keys[1:]):
        if f <= f1:
            t = (f - f0) / (f1 - f0)
            t = t * t * (3 - 2 * t)
            return v0 + (v1 - v0) * t
    return keys[-1][1]


def bake_repair(src_name, out_rig_name, out_action_name, spec, frames=None, sub=1):
    """spec: słownik funkcji klatki → wartości:
       pelvis_yaw(f), torso_yaw(f) [° , + = w prawo postaci], drop(f) [cm], weapon(f, W, hips) → nowa macierz broni (przestrzeń szkieletu).
       Zwraca listę błędów chwytu/stóp."""
    sc = bpy.context.scene
    src = bpy.data.objects[src_name]
    act = src.animation_data.action
    f0, f1 = int(act.frame_range[0]), int(act.frame_range[1])
    frames = frames or list(range(f0, f1 + 1))
    # obiekt wynikowy – kopia danych szkieletu (ta sama poza spoczynkowa)
    out = bpy.data.objects.get(out_rig_name)
    if out is None:
        out = src.copy(); out.data = src.data.copy(); out.name = out_rig_name
        sc.collection.objects.link(out)
    out.animation_data_clear(); out.animation_data_create()
    new = bpy.data.actions.get(out_action_name)
    if new:
        bpy.data.actions.remove(new)
    new = bpy.data.actions.new(out_action_name)
    out.animation_data.action = new
    slot = new.slots.new('OBJECT', out.name) if len(new.slots) == 0 else new.slots[0]
    out.animation_data.action_slot = slot
    ch = children_map(src)
    AX = axe_local(src)
    report = []
    keys = {}  # (bone, path, idx) -> [(frame, value)]
    prev_q = {}
    share = {P + "Spine": 0.3, P + "Spine1": 0.35, P + "Spine2": 0.35}

    def body(f):
        """Poza oryginału z obrotem miednicy/tułowia i obniżeniem; tor broni. Ręce i nogi – jeszcze z oryginału."""
        sc.frame_set(int(f), subframe=f - int(f))
        M = pose_matrices(src)
        hand0 = M[P + "RightHand"].copy(); lhand0 = M[P + "LeftHand"].copy()
        W0 = hand0 @ AX
        L_rel = W0.inverted() @ lhand0          # lewa dłoń względem broni (z oryginału)
        feet0 = (M[P + "LeftFoot"].copy(), M[P + "RightFoot"].copy())
        # przesunięcie całego ciała (np. przekrok zamiast cofnięcia nogi) – cm, przestrzeń szkieletu (+Z przód)
        off = spec.get("root_offset", lambda f: Vector((0, 0, 0)))(f)
        if off.length > 1e-6:
            apply_rigid(M, ch, P + "Hips", Matrix.Translation(off))
            W0 = Matrix.Translation(off) @ W0
        hips_p = M[P + "Hips"].translation.copy()
        py, ty, dr = spec["pelvis_yaw"](f), spec["torso_yaw"](f), spec["drop"](f)
        # miednica: obrót całego ciała wokół pionu przez biodra (+ = w prawo postaci → obrót wokół +Y o −kąt), obniżenie
        apply_rigid(M, ch, P + "Hips", Matrix.Translation((0, -dr, 0)) @ rot_about(hips_p, UP, -py))
        tp = spec.get("torso_pitch", lambda f: 0.0)(f)
        right = Quaternion(UP, math.radians(-ty)) @ Vector((-1, 0, 0))   # prawa strona postaci po skręcie
        for n, k in share.items():
            apply_rigid(M, ch, n, rot_about(M[n].translation.copy(), UP, -(ty - py) * k))
            if tp:   # + = skłon do przodu (głowa w przód i w dół)
                apply_rigid(M, ch, n, rot_about(M[n].translation.copy(), right, -tp * k))
        W = spec["weapon"](f, W0, hips_p)
        # cele stóp: domyślnie stopy z oryginału przesunięte razem z ciałem; spec["foot_target"] może je zastąpić
        ft = spec.get("foot_target")
        feet = []
        for side, m0 in zip(("L", "R"), feet0):
            t = ft(f, side, m0, off) if ft else None
            feet.append(t if t is not None else Matrix.Translation(off) @ m0)
        return M, W, L_rel, tuple(feet)

    def left_target(W, L_rel, span):
        if span is None:
            return W @ L_rel
        # lewa dłoń na osi trzonka, span cm od prawej ku głowicy; orientacja dłoni względem broni z oryginału
        Rl = W.to_3x3().normalized() @ L_rel.to_3x3().normalized()
        lslot = W @ Vector((0, 0, -span))
        t = Rl.to_4x4(); t.translation = lslot - Rl @ Vector((0, SLOT_ALONG_HAND, 0))
        return t

    # Rozstaw dłoni: żądany (grip_span). Lewa dłoń na osi trzonka ku głowicy; gdy nie sięga, prawa dłoń przesuwa się
    # po trzonku ku ostrzu (do grip_slide cm – chwyt szerszy w obie strony, jak przy toporze). Kryterium zasięgu:
    # odległość bark–nadgarstek ≤ grip_reach × długość ramienia (bez prostowania łokcia). Wartości wygładzone w czasie;
    # gdzie osi nie da się dosięgnąć wcale – płynny powrót do ułożenia lewej dłoni z oryginału.
    want = spec.get("grip_span")
    spans = {}; onaxis = {}; slides = {}
    if want is not None:
        want_f = want if callable(want) else (lambda f, w=want: w)
        REACH, SMIN, RMAX = spec.get("grip_reach", 0.93), spec.get("grip_min", 10.0), spec.get("grip_slide", 0.0)
        laL = src.data.bones[P + "LeftArm"].length + src.data.bones[P + "LeftForeArm"].length
        laR = src.data.bones[P + "RightArm"].length + src.data.bones[P + "RightForeArm"].length
        raw = []
        for f in frames:
            M, W, L_rel, _ = body(f)
            A = M[P + "LeftArm"].translation; AR = M[P + "RightArm"].translation
            best = (SMIN, 0.0, 0.0)
            S = want_f(f)
            while S >= SMIN and best[2] == 0.0:
                r = 0.0
                while r <= RMAX:
                    Wr = W @ Matrix.Translation((0, 0, r))
                    okR = ((Wr @ AX.inverted()).translation - AR).length <= REACH * laR
                    okL = (left_target(Wr, L_rel, S).translation - A).length <= REACH * laL
                    if okR and okL:
                        best = (S, r, 1.0); break
                    r += 1.0
                S -= 1.0
            raw.append(best)
        n = len(raw)
        def smooth(vals, k):
            return [sum(vals[max(0, i - k):i + k + 1]) / len(vals[max(0, i - k):i + k + 1]) for i in range(n)]
        lo = [min(r[0] for r in raw[max(0, i - 3):i + 4]) for i in range(n)]
        hi_r = [max(r[1] for r in raw[max(0, i - 3):i + 4]) for i in range(n)]
        okm = [min(r[2] for r in raw[max(0, i - 2):i + 3]) for i in range(n)]
        spans = dict(zip(frames, smooth(lo, 2))); slides = dict(zip(frames, smooth(hi_r, 2))); onaxis = dict(zip(frames, smooth(okm, 3)))

    for f in frames:
        M, W, L_rel, (footL0, footR0) = body(f)
        if slides.get(f):
            W = W @ Matrix.Translation((0, 0, slides[f]))   # prawa dłoń wyżej po trzonku – broń przesuwa się w dłoni
        hand_t = W @ AX.inverted()
        lhand_t = left_target(W, L_rel, spans.get(f) if want is not None else None)
        if want is not None and onaxis.get(f, 1.0) < 1.0:
            # poza zasięgiem osi (garda, szczyt zamachu) – płynny powrót do ułożenia lewej dłoni z oryginału
            w = onaxis[f]; a = W @ L_rel
            q = a.to_quaternion().slerp(lhand_t.to_quaternion(), w)
            t = a.translation.lerp(lhand_t.translation, w)
            lhand_t = q.to_matrix().to_4x4(); lhand_t.translation = t
        eR = two_bone(M, ch, P + "RightArm", P + "RightForeArm", P + "RightHand", hand_t.translation)
        set_world_rot(M, ch, P + "RightHand", hand_t.to_3x3().normalized())
        eL = two_bone(M, ch, P + "LeftArm", P + "LeftForeArm", P + "LeftHand", lhand_t.translation)
        set_world_rot(M, ch, P + "LeftHand", lhand_t.to_3x3().normalized())
        # nogi: stopy z oryginału
        efL = two_bone(M, ch, P + "LeftUpLeg", P + "LeftLeg", P + "LeftFoot", footL0.translation)
        set_world_rot(M, ch, P + "LeftFoot", footL0.to_3x3().normalized())
        efR = two_bone(M, ch, P + "RightUpLeg", P + "RightLeg", P + "RightFoot", footR0.translation)
        set_world_rot(M, ch, P + "RightFoot", footR0.to_3x3().normalized())
        report.append((f, eR, eL, efL, efR, spans.get(f), slides.get(f), onaxis.get(f)))
        B = to_basis(src, M)
        for name, m in B.items():
            loc, rot, scl = m.decompose()
            if name in prev_q and prev_q[name].dot(rot) < 0:
                rot.negate()
            prev_q[name] = rot
            for path, vals in (("location", loc), ("rotation_quaternion", rot), ("scale", scl)):
                for i, v in enumerate(vals):
                    keys.setdefault((name, path, i), []).append((f, v))
    cb = anim_utils.action_ensure_channelbag_for_slot(new, slot)
    for (name, path, i), kv in keys.items():
        fc = cb.fcurves.new(f'pose.bones["{name}"].{path}', index=i, group_name=name)
        fc.keyframe_points.add(len(kv))
        for k, (f, v) in zip(fc.keyframe_points, kv):
            k.co = (f, v); k.interpolation = 'LINEAR'
        fc.update()
    for pb in out.pose.bones:
        pb.rotation_mode = 'QUATERNION'
    sc.frame_set(f0)
    return report
