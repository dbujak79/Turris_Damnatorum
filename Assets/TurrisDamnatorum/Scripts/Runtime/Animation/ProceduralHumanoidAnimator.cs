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
    public class ProceduralHumanoidAnimator : ICharacterAnimationDriver, IImpactReactive
    {
        readonly HumanoidRig rig;
        readonly Transform character;
        Pose current;
        bool hasCurrent;
        float gaitPhase;
        float idleTime;
        float moveBlend;
        // Przenikanie przy zmianie akcji (np. unik przerywający zamach, kolejny cios, trafienie w czasie ataku):
        // od zapamiętanej pozy do nowej po gładkiej krzywej, w czasie zależnym od rodzaju akcji.
        const float TransitionTime = 0.09f;
        AnimAction lastAction;
        AttackAnim lastAttack;
        float sinceActionChange = TransitionTime;
        float transitionTime = TransitionTime;
        Pose transitionFrom;
        readonly Dictionary<AttackAnim, AttackKeys> attackCache = new Dictionary<AttackAnim, AttackKeys>();
        Pose ready, block, parryWind, parryActive, drink, tucked, dead;
        RigLook cachedLook;

        public Pose Current => current;

        // Reakcja na trafienie: sprężyna tułowia (lekko niedotłumiona – odchylenie, powrót z małym przestrzałem)
        // i wolniejsza sprężyna głowy, która „dogania” tułów z opóźnieniem.
        const float ReactK = 170f, ReactC = 15f, HeadK = 95f, HeadC = 9f;
        Vector3 react, reactVel, headReact, headVel;

        /// <summary>Bieżące wychylenie reakcji na trafienie (przestrzeń postaci) – dla testów.</summary>
        public Vector3 ReactOffset => react;

        public void AddImpact(Vector3 localDir, float strength)
        {
            localDir.y = 0f;
            if (localDir.sqrMagnitude < 1e-4f) localDir = Vector3.back;
            // Prędkość początkowa tak dobrana, by szczyt wychylenia ≈ strength.
            reactVel += localDir.normalized * (strength * 24f);
            // Tkaniny zostają w tyle za ciałem: pchnięcie do tyłu podrzuca pelerynę i płaty do przodu (i odwrotnie).
            float kick = localDir.normalized.z * strength;
            if (capeVel != null)
                for (int i = 0; i < capeVel.Length; i++) capeVel[i] += new Vector2(kick * 160f * (0.5f + (i + 1f) / capeVel.Length), -localDir.normalized.x * strength * 80f);
            if (flapVel != null)
                for (int i = 0; i < flapVel.Length; i++) flapVel[i] += kick * 140f;
        }

        // Ruch wtórny: pochylenie przy ruszaniu/hamowaniu i przechył w zakrętach (sprężyna – z lekkim przestrzałem po zatrzymaniu).
        const float LeanK = 90f, LeanC = 13f;
        Vector3 lean, leanVel, prevLocalVel;
        float prevYaw;
        bool hasPrevMotion;
        Pose attackFrom;
        float gaitAmp = 30f, gaitStride = 2f, gaitSpeedN;
        float idleLook;

        /// <summary>Bieżące pochylenie ruchu: x – przechył w zakręcie, z – pochylenie przód/tył (dla testów).</summary>
        public Vector3 Lean => lean;

        void TickLean(AnimAction action, Vector3 localVel, float speedN, bool locomotion, float dt)
        {
            float yaw = character.eulerAngles.y;
            Vector3 target = Vector3.zero;
            if (hasPrevMotion && dt > 1e-4f && action != AnimAction.Dodge && action != AnimAction.Death)
            {
                Vector3 acc = (localVel - prevLocalVel) / dt;
                float yawRate = Mathf.DeltaAngle(prevYaw, yaw) / dt;
                float k = locomotion ? 1f : 0.35f;
                target.z = Mathf.Clamp(acc.z * 0.03f, -1f, 1f) * k;
                target.x = Mathf.Clamp(yawRate / 400f * Mathf.Clamp01(speedN), -1f, 1f) * k;
            }
            prevLocalVel = localVel;
            prevYaw = yaw;
            hasPrevMotion = true;
            int steps = Mathf.Clamp(Mathf.CeilToInt(dt / 0.01f), 1, 6);
            float h = dt / steps;
            for (int i = 0; i < steps; i++)
            {
                leanVel += (LeanK * (target - lean) - LeanC * leanVel) * h;
                lean += leanVel * h;
            }
        }

        void TickReaction(float dt)
        {
            // Kilka mniejszych kroków – stabilnie także przy niskim FPS.
            int steps = Mathf.Clamp(Mathf.CeilToInt(dt / 0.008f), 1, 8);
            float h = dt / steps;
            for (int i = 0; i < steps; i++)
            {
                reactVel += (-ReactK * react - ReactC * reactVel) * h;
                react += reactVel * h;
                headVel += (HeadK * (react - headReact) - HeadC * headVel) * h;
                headReact += headVel * h;
            }
            if (react.sqrMagnitude < 1e-6f && reactVel.sqrMagnitude < 1e-5f && headReact.sqrMagnitude < 1e-6f && headVel.sqrMagnitude < 1e-5f)
                react = reactVel = headReact = headVel = Vector3.zero;
        }

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
                transitionTime = ClipAnimationDriver.FadeInto(s.action, lastAction) + 0.02f;
                sinceActionChange = lastAction == AnimAction.None ? transitionTime : 0f;
                if (hasCurrent) transitionFrom = current;
                // Kolejny cios w serii (lub cios po uniku/bloku) zaczyna zamach z bieżącej pozy, nie z gardy.
                attackFrom = s.action == AnimAction.Attack && lastAction != AnimAction.None && hasCurrent ? current : ready;
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
            // Przy zachwianiu nogi kroczą razem z odrzutem (krok w tył dla złapania równowagi), zamiast sunąć po ziemi.
            bool staggering = s.action == AnimAction.Flinch;
            bool locomotionAllowed = s.action == AnimAction.None || s.action == AnimAction.Block || s.action == AnimAction.Drink || s.action == AnimAction.Cast || staggering;
            moveBlend = Mathf.MoveTowards(moveBlend, locomotionAllowed ? Mathf.Clamp01(speedN * 2f) : 0f, dt * (staggering ? 16f : 6f));
            // Długość cyklu (dwa kroki) rośnie z prędkością, a zamach nóg wynika z niej – stopa w podporze przesuwa się
            // względem ciała tyle, ile ciało przejeżdża nad nią (bez ślizgania).
            float stride = scale * Mathf.Lerp(1.15f, 1.75f, Mathf.Clamp01(speedN)) * Mathf.Lerp(1f, 1.25f, Mathf.Clamp01(speedN - 1f));
            gaitAmp = Mathf.Asin(Mathf.Clamp(stride * 0.25f / (rig.LegLength * scale), 0f, 0.75f)) * Mathf.Rad2Deg;
            gaitStride = stride / scale; // w jednostkach szkieletu (pod VisualRoot)
            gaitSpeedN = speedN;
            gaitPhase += dt * speed / stride * Mathf.PI * 2f;
            Vector3 moveDir = speed > 0.05f ? localVel / speed : Vector3.forward;
            TickLean(s.action, localVel, speedN, locomotionAllowed, dt);
            clothVel = localVel;
            frameDt = dt;

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
                {
                    target = ready;
                    target.chestPitch += breathe * 1.5f;
                    target.handR.y += breathe * 0.01f;
                    // Stanie w miejscu: powolne przenoszenie ciężaru z nogi na nogę.
                    float still = 1f - moveBlend;
                    pivotOffset.x += Mathf.Sin(idleTime * 0.55f) * 0.018f * scale * still;
                    target.crouch += (0.5f + 0.5f * Mathf.Sin(idleTime * 1.1f + 1f)) * 0.01f * still;
                    break;
                }
                case AnimAction.Attack:
                    target = PoseLibrary.SampleAttack(Keys(s.attack), ready, s.phase == ActionPhase.Startup ? attackFrom : ready, s.phase, s.phaseProgress);
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
                {
                    // Zachwianie w stronę pchnięcia, tym głębsze, im cięższy cios; całe ciało lekko odchyla się na stopach.
                    Vector3 push = s.hitDirection.sqrMagnitude > 1e-4f ? Quaternion.Inverse(character.rotation) * s.hitDirection : Vector3.back;
                    push.y = 0f;
                    push = push.sqrMagnitude > 1e-4f ? push.normalized : Vector3.back;
                    float w = s.hitWeight > 0f ? s.hitWeight : 0.5f;
                    float t = s.actionTime / Mathf.Max(0.05f, s.actionDuration);
                    target = PoseLibrary.Stagger(rig.Look, t, push, w);
                    float tilt = PoseLibrary.StaggerEnvelope(t) * Mathf.Lerp(3f, 6f, Mathf.InverseLerp(0.3f, 1.2f, w));
                    pivotRot = Quaternion.AngleAxis(push.z * tilt, Vector3.right) * Quaternion.AngleAxis(-push.x * tilt, Vector3.forward);
                    exact = true;
                    break;
                }
                case AnimAction.GuardBroken:
                    target = PoseLibrary.GuardBroken(rig.Look, s.actionTime);
                    break;
                case AnimAction.Kneel:
                    target = PoseLibrary.Kneel(rig.Look, s.actionTime);
                    break;
                case AnimAction.Death:
                {
                    // Upadek w stronę, w którą pchnął ostatni cios (domyślnie na plecy); ciężki cios odrzuca ciało dalej.
                    float t = Mathf.Clamp01(s.actionTime / 0.75f);
                    float e = t * t;
                    target = Pose.Lerp(ready, dead, t);
                    Vector3 fall = s.hitDirection.sqrMagnitude > 1e-4f ? Quaternion.Inverse(character.rotation) * s.hitDirection : Vector3.back;
                    fall.y = 0f;
                    fall = fall.sqrMagnitude > 1e-4f ? fall.normalized : Vector3.back;
                    // Upadek na twarz wygląda lepiej, gdy postać najpierw przygina się w pasie.
                    if (fall.z > 0.3f) target.chestPitch += 25f * Mathf.Sin(t * Mathf.PI * 0.5f) * fall.z;
                    pivotRot = Quaternion.AngleAxis(86f * e, Vector3.Cross(Vector3.up, fall));
                    // Krótkie odbicie przy uderzeniu o ziemię.
                    float land = Mathf.Clamp01((s.actionTime - 0.75f) / 0.25f);
                    float bounce = land > 0f && land < 1f ? Mathf.Sin(land * Mathf.PI) * 0.05f : 0f;
                    pivotOffset.y = -(rig.HipsHeight - 0.18f) * e + bounce;
                    float push = Mathf.Lerp(0.2f, 0.9f, Mathf.Clamp01(s.hitWeight)) * rig.VisualRoot.lossyScale.x;
                    pivotOffset += fall * (push * (1f - Mathf.Pow(1f - t, 2f)));
                    exact = true;
                    break;
                }
            }

            if (!hasCurrent) { current = target; hasCurrent = true; }
            else if (sinceActionChange < transitionTime)
            {
                // Gładkie przejście z pozy, w której zastała postać zmiana akcji (bez skoku na początku i na końcu).
                float u = Mathf.Clamp01(sinceActionChange / transitionTime);
                Pose blended = Pose.Lerp(transitionFrom, target, u * u * (3f - 2f * u));
                current = exact ? blended : Pose.Lerp(current, blended, 1f - Mathf.Exp(-30f * dt));
            }
            else if (exact) current = target;
            else current = Pose.Lerp(current, target, 1f - Mathf.Exp(-18f * dt));

            // Kołysanie rąk przy chodzie (tylko tam, gdzie poza nie kontroluje rąk w pełni).
            Pose applied = current;

            // Reakcja na trafienie – dokładana na każdą pozę (także w trakcie ataku, bez przerywania go).
            TickReaction(dt);
            if (react != Vector3.zero || headReact != Vector3.zero)
            {
                float sc = rig.VisualRoot.lossyScale.x;
                applied.chestPitch += react.z * 32f;
                applied.spinePitch += react.z * 14f;
                applied.chestYaw += react.x * -32f;
                applied.headPitch += headReact.z * 32f;
                applied.crouch += Mathf.Min(react.magnitude, 1.2f) * 0.07f;
                Vector3 fling = new Vector3(react.x, -Mathf.Abs(react.z) * 0.3f, react.z) * 0.14f;
                applied.handR += fling;
                applied.handL += fling;
                pivotRot = Quaternion.AngleAxis(react.z * 10f, Vector3.right) * Quaternion.AngleAxis(-react.x * 11f, Vector3.forward) * pivotRot;
                pivotOffset += new Vector3(react.x, 0f, react.z) * (0.1f * sc);
            }
            // Pochylenie ruchu: przy ruszaniu do przodu, przy hamowaniu do tyłu, w zakręcie do środka łuku.
            if (lean != Vector3.zero && s.action != AnimAction.Death)
            {
                applied.chestPitch += lean.z * 9f;
                applied.spinePitch += lean.z * 4f;
                pivotRot = Quaternion.AngleAxis(lean.z * 3f, Vector3.right) * Quaternion.AngleAxis(-lean.x * 7f, Vector3.forward) * pivotRot;
            }
            // Stanie: czasem rozejrzenie się (głowa), wygaszane w ruchu i w walce.
            idleLook = s.action == AnimAction.None ? Mathf.Sin(idleTime * 0.37f) * Mathf.Sin(idleTime * 0.13f + 0.7f) * 10f * (1f - moveBlend) : 0f;
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
            // Nogi z IK (stopa w podporze stoi na podłodze), chyba że całe ciało się obraca (przewrót, młynek, upadek) lub jest w powietrzu.
            bool legIK = !dead && pivotOffset.y < 0.05f && Quaternion.Angle(pivotRot, Quaternion.identity) < 20f;
            float phase01 = Mathf.Repeat(gaitPhase / (Mathf.PI * 2f), 1f);
            float duty = Mathf.Lerp(0.6f, 0.34f, Mathf.Clamp01(gaitSpeedN));
            float bob;
            if (legIK)
            {
                // Najniżej w połowie podpory każdej nogi; biodra obniżają się tyle, by noga sięgnęła końców kroku.
                bob = -0.035f * moveBlend * (0.5f + 0.5f * Mathf.Cos((phase01 - duty * 0.5f) * Mathf.PI * 4f));
                float half = gaitStride * duty * 0.5f;
                float reach = Mathf.Sqrt(Mathf.Max(0.01f, rig.LegLength * rig.LegLength - half * half)) * 0.985f;
                float hipJoint = rig.HipsHeight - p.crouch + bob + pivotOffset.y - 0.05f;
                bob -= Mathf.Max(0f, hipJoint - AnkleHeight - reach) * moveBlend;
            }
            else bob = -Mathf.Abs(Mathf.Sin(gaitPhase)) * 0.035f * moveBlend;
            rig.Pivot.localPosition = new Vector3(0, rig.HipsHeight - p.crouch + bob, 0) + pivotOffset;
            rig.Pivot.localRotation = pivotRot;

            float hunch = rig.RestHunch;
            // Skręt tułowia zaczyna się w biodrach (cios „z bioder”), nogi kompensują go, by stopy zostały w miejscu.
            // W chodzie biodra opadają na stronę nogi w wykroku, a klatka równoważy to przeciwnym przechyłem.
            float hipsYaw = p.chestYaw * 0.3f;
            float hipRoll = Mathf.Sin(gaitPhase) * 3.5f * moveBlend;
            rig[Bone.Hips].localRotation = Quaternion.Euler(0, hipsYaw, hipRoll);
            rig[Bone.Spine].localRotation = Quaternion.Euler(p.spinePitch + hunch * 0.55f + p.chestPitch * 0.35f, p.chestYaw * 0.25f, -hipRoll * 0.6f);
            rig[Bone.Chest].localRotation = Quaternion.Euler(p.chestPitch * 0.65f + hunch * 0.45f, p.chestYaw * 0.35f, -hipRoll * 0.3f);
            rig[Bone.Neck].localRotation = Quaternion.Euler(-(hunch + p.chestPitch + p.spinePitch) * 0.45f, -p.chestYaw * 0.25f + idleLook * 0.4f, 0);
            rig[Bone.Head].localRotation = Quaternion.Euler(p.headPitch - (hunch + p.chestPitch) * 0.35f, -p.chestYaw * 0.35f + idleLook * 0.6f, 0);

            if (legIK) SolveLegs(p, moveDir, phase01, duty);
            else FkLegs(p, moveDir, hipsYaw, hipRoll, dead);

            TickCloth(p, hunch);
            ApplyUpper(p);
        }

        // ------------------------------------------------------------------ Tkaniny (ruch wtórny)

        Vector2[] capeAng, capeVel;
        float[] flapAng, flapVel;
        Vector3 clothVel;
        float frameDt = 1f / 60f;

        /// <summary>Kąty segmentów peleryny (x – odchylenie do tyłu, y – na bok) – dla testów.</summary>
        public Vector2[] CapeAngles => capeAng;
        /// <summary>Kąty płatów na biodrach (dodatni – dół płata do tyłu) – dla testów.</summary>
        public float[] FlapAngles => flapAng;

        /// <summary>
        /// Peleryna: łańcuch sprężyn (niższe segmenty luźniejsze – fala biegnie w dół), odchyla się w biegu, przy ruszaniu
        /// i w zakrętach, łopocze w ruchu i wisi pionowo mimo pochylenia tułowia. Płaty na biodrach: kołyszą się,
        /// a kolano w wykroku odpycha je (twarde ograniczenie – tkanina nie przenika przez uda).
        /// </summary>
        void TickCloth(in Pose p, float hunch)
        {
            float dt = Mathf.Clamp(frameDt, 1e-4f, 0.05f);
            int steps = Mathf.Clamp(Mathf.CeilToInt(dt / 0.01f), 1, 5);
            float h = dt / steps;
            float scale = Mathf.Max(0.01f, rig.VisualRoot.lossyScale.x);
            float fwd = Mathf.Clamp(clothVel.z / (4.6f * scale), -1f, 1.7f);
            float side = Mathf.Clamp(clothVel.x / (4.6f * scale), -1f, 1f);

            var segs = rig.CapeSegments;
            if (segs != null && segs.Length > 0)
            {
                int n = segs.Length;
                if (capeAng == null || capeAng.Length != n) { capeAng = new Vector2[n]; capeVel = new Vector2[n]; }
                float torsoLean = hunch + p.chestPitch + p.spinePitch;
                for (int i = 0; i < n; i++)
                {
                    float k = (i + 1f) / n;
                    var target = new Vector2(
                        (i == 0 ? 6f - torsoLean : 3f) + Mathf.Max(0f, fwd) * (i == 0 ? 24f : 8f) + Mathf.Min(0f, fwd) * 3f + lean.z * (i == 0 ? 10f : 5f)
                            + Mathf.Sin(idleTime * 9f - i * 1.3f) * 2.5f * moveBlend * (0.4f + k),
                        -lean.x * (i == 0 ? 8f : 5f) - side * (i == 0 ? 12f : 4f));
                    float K = Mathf.Lerp(160f, 70f, k), C = Mathf.Lerp(16f, 8f, k);
                    for (int st = 0; st < steps; st++)
                    {
                        capeVel[i] += (K * (target - capeAng[i]) - C * capeVel[i]) * h;
                        capeAng[i] += capeVel[i] * h;
                    }
                    // Peleryna nie wchodzi w plecy ani w nogi.
                    float minX = i == 0 ? -torsoLean - 2f : -12f;
                    if (capeAng[i].x < minX) { capeAng[i].x = minX; if (capeVel[i].x < 0f) capeVel[i].x = 0f; }
                    segs[i].localRotation = Quaternion.Euler(capeAng[i].x, 0f, capeAng[i].y);
                }
            }

            var flaps = rig.HipFlaps;
            if (flaps.Count > 0)
            {
                if (flapAng == null || flapAng.Length != flaps.Count) { flapAng = new float[flaps.Count]; flapVel = new float[flaps.Count]; }
                var hips = rig[Bone.Hips];
                float kneeFront = float.MinValue, kneeBack = float.MaxValue;
                foreach (var b in new[] { Bone.ShinL, Bone.ShinR })
                {
                    float z = hips.InverseTransformPoint(rig[b].position).z; // już w jednostkach szkieletu
                    kneeFront = Mathf.Max(kneeFront, z);
                    kneeBack = Mathf.Min(kneeBack, z);
                }
                for (int i = 0; i < flaps.Count; i++)
                {
                    var f = flaps[i];
                    bool front = f.localPosition.z > 0f;
                    float length = Mathf.Max(0.15f, 0.47f + f.localPosition.y);
                    float need = front ? kneeFront + 0.085f - f.localPosition.z : -kneeBack + 0.085f + f.localPosition.z;
                    float clear = Mathf.Asin(Mathf.Clamp(need / length, 0f, 0.95f)) * Mathf.Rad2Deg;
                    float target = Mathf.Max(0f, fwd) * (front ? 4f : 12f) + lean.z * 6f + (front ? -clear : clear);
                    for (int st = 0; st < steps; st++)
                    {
                        flapVel[i] += (150f * (target - flapAng[i]) - 13f * flapVel[i]) * h;
                        flapAng[i] += flapVel[i] * h;
                    }
                    if (front && flapAng[i] > -clear) { flapAng[i] = -clear; flapVel[i] = Mathf.Min(flapVel[i], 0f); }
                    if (!front && flapAng[i] < clear) { flapAng[i] = clear; flapVel[i] = Mathf.Max(flapVel[i], 0f); }
                    f.localRotation = Quaternion.Euler(flapAng[i], 0f, 0f);
                }
            }
        }

        /// <summary>Wysokość kostki nad podłożem w pozycji spoczynkowej (biodra 0,95 − 0,05 − nogi 0,85).</summary>
        const float AnkleHeight = 0.05f;

        /// <summary>
        /// Chód na IK: stopa w podporze przesuwa się względem ciała dokładnie z prędkością ruchu (stoi na podłodze),
        /// w wymachu przenosi się łukiem do przodu. Udział podpory maleje z prędkością (bieg ma fazę lotu).
        /// Bez ruchu stopy stoją w pozycji wypadu z pozy (stance), a przysiad uginają kolana.
        /// </summary>
        void SolveLegs(in Pose p, Vector3 moveDir, float phase01, float duty)
        {
            var root = rig.VisualRoot;
            float s = root.lossyScale.x;
            float S = gaitStride * duty;
            float lift = Mathf.Lerp(0.07f, 0.2f, Mathf.Clamp01(gaitSpeedN));
            Vector3 dir = new Vector3(moveDir.x, 0f, moveDir.z);
            dir = dir.sqrMagnitude > 1e-4f ? dir.normalized : Vector3.forward;
            float w = rig.Look != null ? rig.Look.bulk : 1f;
            for (int leg = 0; leg < 2; leg++)
            {
                bool left = leg == 0;
                float x0 = (left ? -0.11f : 0.11f) * w;
                Vector3 rest = new Vector3(x0 + (left ? -0.03f : 0.03f) * p.stance, AnkleHeight, left ? 0.3f * p.stance : -0.25f * p.stance);
                float ph = Mathf.Repeat(phase01 + (left ? 0f : 0.5f), 1f);
                float along, h = 0f, pitch = 0f;
                if (ph < duty) along = S * 0.5f - ph / duty * S;
                else
                {
                    float u = (ph - duty) / (1f - duty);
                    along = -S * 0.5f + u * u * (3f - 2f * u) * S;
                    h = lift * Mathf.Sin(u * Mathf.PI);
                    pitch = Mathf.Sin(u * Mathf.PI) * Mathf.Lerp(25f, -15f, u); // odbicie palcami, lądowanie na pięcie
                }
                Vector3 gait = new Vector3(x0, AnkleHeight + h, 0f) + dir * along;
                Vector3 target = Vector3.Lerp(rest, gait, moveBlend);
                var thigh = rig[left ? Bone.ThighL : Bone.ThighR];
                var shin = rig[left ? Bone.ShinL : Bone.ShinR];
                var ft = rig[left ? Bone.FootL : Bone.FootR];
                Vector3 hint = root.forward + root.right * (left ? -0.15f : 0.15f);
                SolveTwoBone(thigh, shin, ft, root.TransformPoint(target), hint,
                    shin.localPosition.magnitude * s, ft.localPosition.magnitude * s, root.forward);
                ft.rotation = root.rotation * Quaternion.Euler(pitch * moveBlend, 0f, 0f);
            }
        }

        void FkLegs(in Pose p, Vector3 moveDir, float hipsYaw, float hipRoll, bool dead)
        {
            // Nogi: przysiad + chód + wypad.
            float legLen = rig.LegLength;
            float crouchAngle = Mathf.Acos(Mathf.Clamp((legLen - Mathf.Max(0, p.crouch - (dead ? 0 : 0))) / legLen, 0.2f, 1f)) * Mathf.Rad2Deg;
            float amp = gaitAmp * moveBlend;
            float fwd = moveDir.z, side = moveDir.x;
            for (int leg = 0; leg < 2; leg++)
            {
                bool left = leg == 0;
                float ph = gaitPhase + (left ? 0f : Mathf.PI);
                float sw = Mathf.Sin(ph);
                float thighX = -crouchAngle - sw * amp * fwd;
                // Krok w bok: odwodzenie w biodrze (mniejsze niż wykrok, ale bez ślizgania przy namierzaniu).
                float thighZ = sw * amp * 0.8f * side;
                float knee = crouchAngle * 2f + Mathf.Max(0f, Mathf.Cos(ph)) * amp * 1.2f;
                float foot = -crouchAngle;
                // Wypad: lewa noga z przodu, prawa z tyłu.
                float st = p.stance * (1f - moveBlend);
                if (left) { thighX += -20f * st; knee += 10f * st; foot += 8f * st; }
                else { thighX += 14f * st; knee += 12f * st; foot -= 8f * st; }
                var thigh = rig[left ? Bone.ThighL : Bone.ThighR];
                var shin = rig[left ? Bone.ShinL : Bone.ShinR];
                var ft = rig[left ? Bone.FootL : Bone.FootR];
                thigh.localRotation = Quaternion.Euler(thighX, -hipsYaw, thighZ + (left ? -3f : 3f) - hipRoll);
                shin.localRotation = Quaternion.Euler(knee, 0, 0);
                ft.localRotation = Quaternion.Euler(foot, 0, 0);
            }
        }

        void ApplyUpper(in Pose p)
        {
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
