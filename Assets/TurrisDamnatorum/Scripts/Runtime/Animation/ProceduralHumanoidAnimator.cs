using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    public interface ICharacterAnimationDriver
    {
        void Tick(in CharacterAnimState state, Vector3 worldVelocity, float dt);
        void Dispose();
    }

    /// <summary>
    /// Animacja proceduralna humanoida: chód/bieg (FK nóg), IK obu rąk do celów z pozy,
    /// orientacja broni i tarczy w przestrzeni postaci, obrót całego ciała dla przewrotu i śmierci.
    /// Ataki i parowanie są próbkowane wprost z postępu faz walki (bez wygładzania), więc są zsynchronizowane z logiką.
    /// </summary>
    public class ProceduralHumanoidAnimator : ICharacterAnimationDriver
    {
        readonly HumanoidRig rig;
        readonly Transform character;
        Pose current;
        bool hasCurrent;
        float gaitPhase;
        float idleTime;
        float moveBlend;
        // Krótkie przenikanie przy zmianie akcji (np. unik przerywający zamach), by poza nie przeskakiwała.
        const float TransitionTime = 0.09f;
        AnimAction lastAction;
        AttackAnim lastAttack;
        float sinceActionChange = TransitionTime;
        readonly Dictionary<AttackAnim, AttackKeys> attackCache = new Dictionary<AttackAnim, AttackKeys>();
        Pose ready, block, parryWind, parryActive, drink, tucked, dead;
        RigLook cachedLook;

        public Pose Current => current;

        public ProceduralHumanoidAnimator(HumanoidRig rig, Transform character)
        {
            this.rig = rig;
            this.character = character;
            Refresh();
        }

        /// <summary>Przelicza pozy po zmianie broni/tarczy.</summary>
        public void Refresh()
        {
            var look = rig.Look;
            cachedLook = look;
            attackCache.Clear();
            ready = PoseLibrary.Ready(look);
            block = PoseLibrary.Block(look);
            parryWind = PoseLibrary.ParryWind(look);
            parryActive = PoseLibrary.ParryActive(look);
            drink = PoseLibrary.Drink(look);
            tucked = PoseLibrary.Tucked(look);
            dead = PoseLibrary.Dead(look);
        }

        AttackKeys Keys(AttackAnim a)
        {
            if (!attackCache.TryGetValue(a, out var k)) { k = PoseLibrary.Attack(a, rig.Look); attackCache[a] = k; }
            return k;
        }

        public void Dispose() { }

        public void Tick(in CharacterAnimState s, Vector3 worldVelocity, float dt)
        {
            if (rig == null) return;
            if (cachedLook != rig.Look) Refresh();
            idleTime += dt;
            if (s.action != lastAction || (s.action == AnimAction.Attack && s.attack != lastAttack))
            {
                // Nowa akcja z bezczynności startuje z pozy gotowości – przenikanie potrzebne tylko przy przerwaniu innej akcji.
                sinceActionChange = lastAction == AnimAction.None ? TransitionTime : 0f;
                lastAction = s.action;
                lastAttack = s.attack;
            }
            else sinceActionChange += dt;

            // --- Ruch
            Vector3 localVel = Quaternion.Inverse(character.rotation) * worldVelocity;
            localVel.y = 0;
            float speed = localVel.magnitude;
            float scale = Mathf.Max(0.01f, rig.VisualRoot.lossyScale.x);
            float speedN = Mathf.Clamp(speed / (4.6f * scale), 0f, 1.7f);
            bool locomotionAllowed = s.action == AnimAction.None || s.action == AnimAction.Block || s.action == AnimAction.Drink || s.action == AnimAction.Cast;
            moveBlend = Mathf.MoveTowards(moveBlend, locomotionAllowed ? Mathf.Clamp01(speedN * 2f) : 0f, dt * 6f);
            float stride = 1.35f * scale * Mathf.Lerp(1f, 1.4f, Mathf.Clamp01(speedN - 1f));
            gaitPhase += dt * speed / stride * Mathf.PI * 2f;
            Vector3 moveDir = speed > 0.05f ? localVel / speed : Vector3.forward;

            // --- Poza docelowa
            bool exact = false;
            Pose target = ready;
            Quaternion pivotRot = Quaternion.identity;
            Vector3 pivotOffset = Vector3.zero;
            bool flask = false;
            float breathe = Mathf.Sin(idleTime * 2.1f);

            switch (s.action)
            {
                case AnimAction.None:
                    target = ready;
                    target.chestPitch += breathe * 1.5f;
                    target.handR.y += breathe * 0.01f;
                    break;
                case AnimAction.Attack:
                    target = PoseLibrary.SampleAttack(Keys(s.attack), ready, s.phase, s.phaseProgress);
                    exact = true;
                    if (s.attack == AttackAnim.Leap && s.phase == ActionPhase.Active)
                        pivotOffset.y = Mathf.Sin(Mathf.Clamp01(s.phaseProgress) * Mathf.PI) * 0.35f;
                    if (s.spinAngle != 0f) pivotRot = Quaternion.AngleAxis(s.spinAngle, Vector3.up);
                    break;
                case AnimAction.Block:
                    target = block;
                    break;
                case AnimAction.Parry:
                    exact = true;
                    if (s.phase == ActionPhase.Startup) target = Pose.Lerp(ready, parryWind, Mathf.Clamp01(s.phaseProgress));
                    else if (s.phase == ActionPhase.Active) target = Pose.Lerp(parryWind, parryActive, 1f - Mathf.Pow(1f - Mathf.Clamp01(s.phaseProgress * 2.5f), 3f));
                    else target = Pose.Lerp(parryActive, ready, Mathf.SmoothStep(0, 1, s.phaseProgress));
                    break;
                case AnimAction.Dodge:
                {
                    exact = true;
                    float rollDur = Mathf.Max(0.2f, s.rollDuration > 0f ? s.rollDuration : s.actionDuration * 0.62f);
                    float t = Mathf.Clamp01(s.actionTime / rollDur);
                    float tuck = Mathf.Sin(t * Mathf.PI);
                    target = Pose.Lerp(ready, tucked, Mathf.Clamp01(tuck * 1.6f));
                    Vector3 localDir = Quaternion.Inverse(character.rotation) * s.dodgeDirection;
                    localDir.y = 0;
                    if (localDir.sqrMagnitude < 0.01f) localDir = Vector3.back;
                    Vector3 axis = Vector3.Cross(Vector3.up, localDir.normalized);
                    // Najpierw zgięcie, potem obrót, na końcu chwila na wstanie – obrót nie wypełnia całego czasu.
                    float spin = Mathf.Clamp01((t - 0.08f) / 0.8f);
                    pivotRot = Quaternion.AngleAxis(360f * Mathf.SmoothStep(0, 1, spin), axis);
                    break;
                }
                case AnimAction.Backstep:
                {
                    float t = Mathf.Clamp01(s.actionTime / Mathf.Max(0.1f, s.actionDuration));
                    target = Pose.Lerp(ready, tucked, Mathf.Sin(t * Mathf.PI) * 0.4f);
                    target.chestPitch = -12f * Mathf.Sin(t * Mathf.PI);
                    pivotOffset.y = Mathf.Sin(t * Mathf.PI) * 0.12f;
                    exact = true;
                    break;
                }
                case AnimAction.Cast:
                    exact = true;
                    if (s.phase == ActionPhase.Startup) target = Pose.Lerp(ready, PoseLibrary.Cast(rig.Look, 0f), Mathf.SmoothStep(0, 1, s.phaseProgress));
                    else if (s.phase == ActionPhase.Active) target = PoseLibrary.Cast(rig.Look, 1f);
                    else target = Pose.Lerp(PoseLibrary.Cast(rig.Look, 1f), ready, Mathf.SmoothStep(0, 1, s.phaseProgress));
                    break;
                case AnimAction.Drink:
                    target = drink;
                    flask = true;
                    break;
                case AnimAction.Flinch:
                    target = PoseLibrary.Flinch(rig.Look, s.actionTime / Mathf.Max(0.05f, s.actionDuration));
                    exact = true;
                    break;
                case AnimAction.GuardBroken:
                    target = PoseLibrary.GuardBroken(rig.Look, s.actionTime);
                    break;
                case AnimAction.Kneel:
                    target = PoseLibrary.Kneel(rig.Look, s.actionTime);
                    break;
                case AnimAction.Death:
                {
                    float t = Mathf.Clamp01(s.actionTime / 0.75f);
                    float e = t * t;
                    target = Pose.Lerp(ready, dead, t);
                    pivotRot = Quaternion.AngleAxis(-86f * e, Vector3.right);
                    pivotOffset.y = -(rig.HipsHeight - 0.18f) * e;
                    exact = true;
                    break;
                }
            }

            if (!hasCurrent) { current = target; hasCurrent = true; }
            else if (exact && sinceActionChange >= TransitionTime) current = target;
            else if (exact) current = Pose.Lerp(current, target, 1f - Mathf.Exp(-45f * dt));
            else current = Pose.Lerp(current, target, 1f - Mathf.Exp(-18f * dt));

            // Kołysanie rąk przy chodzie (tylko tam, gdzie poza nie kontroluje rąk w pełni).
            Pose applied = current;
            float swing = Mathf.Sin(gaitPhase) * 0.14f * moveBlend * Mathf.Clamp01(moveDir.z * 1.5f + 0.3f) * (1f - applied.armsOverride);
            applied.handR.z -= swing;
            applied.handL.z += swing;
            // Broń dwuręczna: lewa dłoń na drzewcu (poza piciem i tarczą).
            if (rig.Look.TwoHanded && rig.Look.shield == ShieldModel.None && s.action != AnimAction.Drink && s.action != AnimAction.Death && s.action != AnimAction.GuardBroken)
                applied.handL = applied.handR + applied.weaponDir.normalized * 0.28f;

            // Naciąg cięciwy: rośnie w czasie zamachu ataku łukiem, w chwili strzału puszcza.
            bowDrawNow = s.action == AnimAction.Attack && s.attack == AttackAnim.BowDraw && s.phase == ActionPhase.Startup
                ? Mathf.SmoothStep(0f, 1f, s.phaseProgress) : 0f;
            Apply(applied, moveDir, pivotRot, pivotOffset, s.action == AnimAction.Death);
            rig.ShowFlask(flask);
        }

        void Apply(in Pose p, Vector3 moveDir, Quaternion pivotRot, Vector3 pivotOffset, bool dead)
        {
            float bob = -Mathf.Abs(Mathf.Sin(gaitPhase)) * 0.035f * moveBlend;
            rig.Pivot.localPosition = new Vector3(0, rig.HipsHeight - p.crouch + bob, 0) + pivotOffset;
            rig.Pivot.localRotation = pivotRot;

            float hunch = rig.RestHunch;
            rig[Bone.Hips].localRotation = Quaternion.Euler(0, -p.chestYaw * 0.15f, 0);
            rig[Bone.Spine].localRotation = Quaternion.Euler(p.spinePitch + hunch * 0.55f + p.chestPitch * 0.35f, p.chestYaw * 0.35f, 0);
            rig[Bone.Chest].localRotation = Quaternion.Euler(p.chestPitch * 0.65f + hunch * 0.45f, p.chestYaw * 0.65f, 0);
            rig[Bone.Neck].localRotation = Quaternion.Euler(-(hunch + p.chestPitch + p.spinePitch) * 0.45f, -p.chestYaw * 0.25f, 0);
            rig[Bone.Head].localRotation = Quaternion.Euler(p.headPitch - (hunch + p.chestPitch) * 0.35f, -p.chestYaw * 0.35f, 0);

            // Nogi: przysiad + chód + wypad.
            float legLen = rig.LegLength;
            float crouchAngle = Mathf.Acos(Mathf.Clamp((legLen - Mathf.Max(0, p.crouch - (dead ? 0 : 0))) / legLen, 0.2f, 1f)) * Mathf.Rad2Deg;
            float amp = Mathf.Lerp(0f, 32f, moveBlend) * Mathf.Lerp(1f, 1.35f, Mathf.Clamp01(moveBlend));
            float fwd = moveDir.z, side = moveDir.x;
            for (int leg = 0; leg < 2; leg++)
            {
                bool left = leg == 0;
                float ph = gaitPhase + (left ? 0f : Mathf.PI);
                float sw = Mathf.Sin(ph);
                float thighX = -crouchAngle - sw * amp * fwd;
                float thighZ = sw * amp * 0.45f * side;
                float knee = crouchAngle * 2f + Mathf.Max(0f, Mathf.Cos(ph)) * amp * 1.2f;
                float foot = -crouchAngle;
                // Wypad: lewa noga z przodu, prawa z tyłu.
                float st = p.stance * (1f - moveBlend);
                if (left) { thighX += -20f * st; knee += 10f * st; foot += 8f * st; }
                else { thighX += 14f * st; knee += 12f * st; foot -= 8f * st; }
                var thigh = rig[left ? Bone.ThighL : Bone.ThighR];
                var shin = rig[left ? Bone.ShinL : Bone.ShinR];
                var ft = rig[left ? Bone.FootL : Bone.FootR];
                thigh.localRotation = Quaternion.Euler(thighX, 0, thighZ + (left ? -3f : 3f));
                shin.localRotation = Quaternion.Euler(knee, 0, 0);
                ft.localRotation = Quaternion.Euler(foot, 0, 0);
            }

            // Peleryna: wisi pionowo (kompensuje pochylenie tułowia) i odchyla się do tyłu w ruchu.
            if (rig.CapePivot != null)
            {
                float lean = hunch + p.chestPitch + p.spinePitch;
                float flow = 6f + moveBlend * 22f + Mathf.Sin(gaitPhase * 2f) * 3f * moveBlend;
                rig.CapePivot.localRotation = Quaternion.Euler(flow - lean, 0, 0);
                if (rig.CapeLower != null)
                    rig.CapeLower.localRotation = Quaternion.Euler(moveBlend * 12f + Mathf.Sin(gaitPhase * 2f + 0.8f) * 4f * moveBlend + 2f, 0, 0);
            }

            // Ręce: IK do celów w przestrzeni postaci.
            var root = rig.VisualRoot;
            float s = root.lossyScale.x;
            SolveTwoBone(rig[Bone.UpperArmR], rig[Bone.ForearmR], rig[Bone.HandR], root.TransformPoint(p.handR), root.TransformDirection(p.hintR),
                rig.UpperArmLength * s, rig.ForearmLength * s, root.forward);
            SolveTwoBone(rig[Bone.UpperArmL], rig[Bone.ForearmL], rig[Bone.HandL], root.TransformPoint(p.handL), root.TransformDirection(p.hintL),
                rig.UpperArmLength * s, rig.ForearmLength * s, root.forward);

            // Broń w dłoni, orientacja w przestrzeni postaci.
            var hand = rig[Bone.HandR];
            rig.WeaponSocket.position = hand.position + hand.rotation * new Vector3(0, -0.07f * s, 0);
            rig.WeaponSocket.rotation = root.rotation * SafeLook(p.weaponDir, p.weaponUp);

            // Żywa cięciwa: w czasie naciągania (zamach ataku łukiem) środek podąża za lewą dłonią, po strzale wraca prosto.
            if (rig.BowStringUpper != null)
            {
                Vector3 up = rig.WeaponSocket.TransformPoint(GearBuilder.BowTipUp);
                Vector3 down = rig.WeaponSocket.TransformPoint(GearBuilder.BowTipDown);
                Vector3 stringMid = (up + down) * 0.5f;
                float draw = bowDrawNow;
                Vector3 nock = Vector3.Lerp(stringMid, rig[Bone.HandL].position, draw);
                PlaceString(rig.BowStringUpper, up, nock);
                PlaceString(rig.BowStringLower, down, nock);
                lastBowDraw = draw;
                // Strzała na cięciwie: od nasadki (dłoń) przez łuk do przodu – tylko w czasie naciągu.
                if (rig.NockedArrow != null)
                {
                    bool show = draw > 0.05f;
                    if (rig.NockedArrow.gameObject.activeSelf != show) rig.NockedArrow.gameObject.SetActive(show);
                    if (show)
                    {
                        Vector3 grip = rig.WeaponSocket.position;
                        Vector3 aimDir = (grip - nock).sqrMagnitude > 1e-4f ? (grip - nock).normalized : rig.VisualRoot.forward;
                        rig.NockedArrow.SetPositionAndRotation(nock, Quaternion.LookRotation(aimDir, Vector3.up));
                    }
                }
            }

            // Druga broń (noże) w lewej dłoni – kierunek lustrzany do prawej.
            if (rig.OffhandSocket != null)
            {
                var hl = rig[Bone.HandL];
                Vector3 mirrored = new Vector3(-p.weaponDir.x, p.weaponDir.y, p.weaponDir.z);
                rig.OffhandSocket.position = hl.position + hl.rotation * new Vector3(0, -0.07f * s, 0);
                rig.OffhandSocket.rotation = root.rotation * SafeLook(mirrored, p.weaponUp);
            }

            // Tarcza na lewym przedramieniu.
            var fa = rig[Bone.ForearmL];
            var handL = rig[Bone.HandL];
            Vector3 mid = Vector3.Lerp(fa.position, handL.position, 0.55f);
            Quaternion shieldRot = root.rotation * SafeLook(p.shieldNormal, p.shieldUp);
            rig.ShieldSocket.SetPositionAndRotation(mid + shieldRot * Vector3.forward * (0.06f * s), shieldRot);
        }

        /// <summary>Stopień naciągnięcia cięciwy w ostatniej klatce (0–1) – dla testów i efektów.</summary>
        public float BowDraw => lastBowDraw;
        float lastBowDraw;

        float bowDrawNow;

        void PlaceString(Transform seg, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 1e-4f) return;
            seg.position = (a + b) * 0.5f;
            seg.rotation = Quaternion.FromToRotation(Vector3.up, d / len);
            float ps = seg.parent != null ? Mathf.Max(0.001f, seg.parent.lossyScale.x) : 1f;
            seg.localScale = new Vector3(0.007f / ps, len / ps, 0.007f / ps);
        }

        static Quaternion SafeLook(Vector3 dir, Vector3 up)
        {
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;
            dir.Normalize();
            if (up.sqrMagnitude < 1e-6f || Mathf.Abs(Vector3.Dot(dir, up.normalized)) > 0.97f)
                up = Mathf.Abs(dir.y) > 0.9f ? Vector3.forward : Vector3.up;
            return Quaternion.LookRotation(dir, up);
        }

        /// <summary>Analityczne IK dwóch kości (ramię–przedramię). Kości wskazują lokalnym −Y na dziecko.</summary>
        public static void SolveTwoBone(Transform upper, Transform lower, Transform end, Vector3 target, Vector3 hint, float l1, float l2, Vector3 twistRef)
        {
            Vector3 root = upper.position;
            Vector3 toT = target - root;
            float d = toT.magnitude;
            if (d < 1e-4f) return;
            Vector3 dir = toT / d;
            d = Mathf.Clamp(d, Mathf.Abs(l1 - l2) + 1e-3f, (l1 + l2) * 0.999f);

            Vector3 bend = Vector3.ProjectOnPlane(hint, dir);
            if (bend.sqrMagnitude < 1e-6f) bend = Vector3.ProjectOnPlane(Vector3.down, dir);
            if (bend.sqrMagnitude < 1e-6f) bend = Vector3.ProjectOnPlane(Vector3.forward, dir);
            bend.Normalize();

            float cosA = Mathf.Clamp((l1 * l1 + d * d - l2 * l2) / (2f * l1 * d), -1f, 1f);
            float sinA = Mathf.Sqrt(1f - cosA * cosA);
            Vector3 elbow = root + dir * (l1 * cosA) + bend * (l1 * sinA);
            Vector3 endPos = root + dir * d;

            SetBone(upper, elbow - root, twistRef);
            SetBone(lower, endPos - elbow, twistRef);
            end.rotation = lower.rotation;
        }

        static void SetBone(Transform t, Vector3 axis, Vector3 fwdRef)
        {
            axis.Normalize();
            Vector3 fwd = Vector3.ProjectOnPlane(fwdRef, axis);
            if (fwd.sqrMagnitude < 1e-6f) fwd = Vector3.ProjectOnPlane(Vector3.up, axis);
            t.rotation = Quaternion.LookRotation(fwd.normalized, -axis);
        }
    }
}
