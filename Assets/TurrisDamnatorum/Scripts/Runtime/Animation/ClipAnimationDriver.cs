using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Turris
{
    /// <summary>
    /// Sterownik animacji z klipów (Playables, bez Animator Controllera).
    /// Warstwa 0: ruch – mieszanie dwuwymiarowe (bezczynność, chód i bieg w przód, w tył i w boki) ze wspólną fazą kroków.
    /// Warstwa 1: akcja całego ciała, warstwa 2: akcja górnej połowy ciała (maska – tylko dla modeli Humanoid).
    /// Każda warstwa akcji ma dwa wejścia (bieżący i poprzedni klip), więc kolejny cios serii, unik czy drgnięcie
    /// przenikają się z poprzednią akcją zamiast przeskakiwać.
    /// Czas klipu akcji jest USTAWIANY z postępu faz walki, a nie odtwarzany, dzięki czemu moment trafienia w klipie
    /// pokrywa się z fazą aktywną ataku niezależnie od długości klipu.
    /// Po klipach nakładana jest warstwa ciała (<see cref="BodyDynamics"/>): odchylenie kręgosłupa i głowy od ciosu
    /// oraz pochylenie przy ruszaniu, hamowaniu i w zakrętach.
    /// </summary>
    public class ClipAnimationDriver : ICharacterAnimationDriver, IImpactReactive
    {
        // Sloty ruchu: 0 spoczynek, 1–4 chód (przód, tył, lewo, prawo), 5–8 bieg (przód, tył, lewo, prawo).
        const int Idle = 0, Walk = 1, Run = 5, LocoSlots = 9;

        readonly CharacterVisualDefinition def;
        readonly WeaponAnimSet set;
        PlayableGraph graph;
        AnimationLayerMixerPlayable layers;
        AnimationMixerPlayable loco;
        readonly AnimationClipPlayable[] locoClips = new AnimationClipPlayable[LocoSlots];
        readonly float[] locoSpeeds = new float[LocoSlots];   // prędkość nagrania klipu (m/s)
        readonly float[] locoPhase = new float[LocoSlots];    // przesunięcie fazy: chwila wykroku lewej nogi w kierunku ruchu
        readonly bool[] locoHas = new bool[LocoSlots];
        readonly float[] locoWeights = new float[LocoSlots];
        float stepPhase; // wspólna faza cyklu kroków (0–1) wszystkich klipów ruchu – stopy nie rozjeżdżają się przy mieszaniu
        readonly bool humanoid;
        readonly Transform character;
        readonly Animator animator;
        readonly HellOffhandPose offhandPose;
        static AnimationClip emptyClip;

        /// <summary>Warstwa akcji: mikser z bieżącym (wejście 0) i poprzednim, zamrożonym klipem (wejście 1).</summary>
        class ActionLayer
        {
            public AnimationMixerPlayable mixer;
            public AnimationClipPlayable cur, prev;
            public ActionClip curClip;
            public float weight;      // udział warstwy nad ruchem
            public float mix = 1f;    // udział bieżącego klipu względem poprzedniego
            public float fade = 0.1f; // czas przenikania (s)
            public float sampleSpeed, previousSpeed;
        }
        readonly ActionLayer[] actionLayers = new ActionLayer[3];
        ActionClip currentAction;
        int currentLayer = -1;
        AnimAction lastAction;
        float lastNorm;

        public ActionClip CurrentActionClip => currentAction;
        public float CurrentActionTime => currentAction != null && currentLayer > 0 ? (float)actionLayers[currentLayer].cur.GetTime() : 0f;
        public PlayableGraph Graph => graph;
        /// <summary>Wagi slotów ruchu (dla testów): 0 spoczynek, 1–4 chód P/T/L/P, 5–8 bieg.</summary>
        public float LocoWeight(int slot) => !loco.IsValid() ? 0f : slot == Idle ? loco.GetInputWeight(Idle) + loco.GetInputWeight(LocoSlots) : loco.GetInputWeight(slot);
        /// <summary>Udział warstwy akcji (1 – całe ciało, 2 – górna połowa).</summary>
        public float ActionLayerWeight(int layer) => actionLayers[layer] != null ? actionLayers[layer].weight : 0f;
        public BodyDynamics Body { get; }

        // Krok z nagrania: najdalsze dotąd wysunięcie ciała w bieżącym ciosie i przesunięcie czekające na kontroler.
        float rootPeak, pendingAdvance;

        /// <summary>Zestaw broni ma kroki ciosów z nagrań – kontroler przesuwa bohatera według klipu zamiast stałego wypadu.</summary>
        public bool StepsFromClips { get; private set; }

        /// <summary>
        /// Przesunięcie do przodu (m) od ostatniego odczytu: tyle, o ile ciało w klipie ciosu poszło naprzód. Tylko do przodu –
        /// gdy nagranie cofa ciało do pozycji wyjściowej, bohater zostaje, a nogi dosuwają się do niego.
        /// </summary>
        public float ConsumeAdvance() { float a = pendingAdvance; pendingAdvance = 0f; return a; }

        // Prędkość, którą sterownik sam nadaje bohaterowi (krok ciosu, dostawienie nogi) – wygładzona jak prędkość postaci
        // w CharacterVisual. Odejmowana od prędkości ruchu, żeby własny krok nie włączał klipu chodu.
        float selfSpeed, lastAdvance;

        float RootAt(ActionClip c, float norm) =>
            c != null && c.rootForward != null && c.rootForward.length > 1
                ? c.rootForward.Evaluate(norm) * (humanoid ? animator.humanScale : 1f) * animator.transform.lossyScale.y
                : 0f;

        public ClipAnimationDriver(Animator animator, CharacterVisualDefinition def, Transform character, WeaponAnimSet set = null)
        {
            this.def = def;
            this.set = set != null && set.idle != null ? set : null;
            this.character = character;
            this.animator = animator;
            humanoid = animator.isHuman;
            offhandPose = humanoid ? new HellOffhandPose(animator) : null;
            animator.applyRootMotion = false; // ruch prowadzi logika gry
            graph = PlayableGraph.Create("TurrisCharacter_" + animator.name);
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var output = AnimationPlayableOutput.Create(graph, "Animation", animator);

            layers = AnimationLayerMixerPlayable.Create(graph, 3);
            output.SetSourcePlayable(layers);

            // Ruch z zestawu broni (gdy jest), inaczej z modelu.
            var s = this.set;
            var clips = s != null
                ? new[] { s.useModelIdle && def.idle != null ? def.idle : s.idle, s.walk, s.walkBack, s.strafeLeft, s.strafeRight, s.run, s.runBack, s.runLeft, s.runRight }
                : new[] { def.idle, def.walk, def.walkBack, def.strafeLeft, def.strafeRight, def.run, null, null, null };
            float walkSpeed = s != null ? s.walkSpeed : def.walkSpeed;
            float runSpeed = s != null ? s.runSpeed : def.runSpeed;
            // Bez klipu biegu biegnie chód (szybciej), a nie bezczynność.
            if (clips[Run] == null && clips[Walk] != null) { clips[Run] = clips[Walk]; runSpeed = walkSpeed; }
            loco = AnimationMixerPlayable.Create(graph, LocoSlots + 1);
            for (int i = 0; i < LocoSlots; i++)
            {
                locoHas[i] = clips[i] != null;
                var clip = clips[i] != null ? clips[i] : (clips[0] != null ? clips[0] : Empty());
                locoClips[i] = AnimationClipPlayable.Create(graph, clip);
                if (i >= 1) locoClips[i].SetSpeed(0); // czas z fazy kroków
                graph.Connect(locoClips[i], 0, loco, i);
                loco.SetInputWeight(i, i == 0 ? 1f : 0f);
                // Prędkość nagrania: z ruchu korzenia klipu (dokładna dla każdego kierunku), inaczej z zestawu.
                float nominal = i >= Run ? runSpeed : walkSpeed;
                Vector3 avg = clips[i] != null ? clips[i].averageSpeed : Vector3.zero;
                avg.y = 0f;
                locoSpeeds[i] = i == Idle ? 0f : avg.magnitude > 0.3f ? avg.magnitude : nominal;
            }
            // Wejście dodatkowe: postawa bojowa (spoczynek po ciosie).
            combatIdle = AnimationClipPlayable.Create(graph, s != null && s.combatIdle != null ? s.combatIdle : (clips[0] != null ? clips[0] : Empty()));
            hasCombatIdle = s != null && s.combatIdle != null;
            graph.Connect(combatIdle, 0, loco, LocoSlots);
            loco.SetInputWeight(LocoSlots, 0f);
            graph.Connect(loco, 0, layers, 0);
            layers.SetInputWeight(0, 1f);

            for (int l = 1; l <= 2; l++)
            {
                var al = new ActionLayer { mixer = AnimationMixerPlayable.Create(graph, 2) };
                al.cur = AnimationClipPlayable.Create(graph, Empty());
                al.prev = AnimationClipPlayable.Create(graph, Empty());
                graph.Connect(al.cur, 0, al.mixer, 0);
                graph.Connect(al.prev, 0, al.mixer, 1);
                al.mixer.SetInputWeight(0, 1f);
                al.mixer.SetInputWeight(1, 0f);
                graph.Connect(al.mixer, 0, layers, l);
                layers.SetInputWeight(l, 0f);
                actionLayers[l] = al;
            }
            if (humanoid) layers.SetLayerMaskFromAvatarMask(2, UpperBodyMask());

            Body = humanoid ? new BodyDynamics(animator, character) : null;
            // Pomiary na klipach (faza kroków, kierunek stóp) dopiero w drugiej klatce: przy budowie ewaluacja grafu jeszcze
            // nie przesuwa kości (pomiar widział pozę spoczynkową – fazy wychodziły zerowe). Poza spoczynkowa – teraz.
            if (humanoid) { CaptureRestFeet(); measureIn = 2; }
            if (this.set != null)
                foreach (var a in this.set.actions)
                    if (a != null && a.action == AnimAction.Attack && a.rootForward != null && a.rootForward.length > 1) { StepsFromClips = true; break; }
        }

        static AnimationClip Empty()
        {
            if (emptyClip == null) emptyClip = new AnimationClip { name = "Empty" };
            return emptyClip;
        }

        static AvatarMask upperMask;

        static AvatarMask UpperBodyMask()
        {
            if (upperMask != null) return upperMask;
            var m = new AvatarMask();
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++) m.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            foreach (var p in new[] { AvatarMaskBodyPart.Body, AvatarMaskBodyPart.Head, AvatarMaskBodyPart.LeftArm, AvatarMaskBodyPart.RightArm,
                                      AvatarMaskBodyPart.LeftFingers, AvatarMaskBodyPart.RightFingers, AvatarMaskBodyPart.LeftHandIK, AvatarMaskBodyPart.RightHandIK })
                m.SetHumanoidBodyPartActive(p, true);
            return upperMask = m;
        }

        // ------------------------------------------------------------------ Faza kroków

        static readonly Dictionary<AnimationClip, float> phaseCache = new Dictionary<AnimationClip, float>();
        static readonly Vector3[] SlotDir =
        {
            Vector3.zero, Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
            Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
        };

        /// <summary>
        /// Klipy Mixamo zaczynają cykl od różnych kroków. Przy mieszaniu (np. bieg po skosie = przód + bok) nogi
        /// muszą być w tej samej fazie, inaczej się krzyżują. Dla każdego klipu szukamy chwili, w której lewa stopa
        /// najdalej wyprzedza prawą w kierunku ruchu – od niej liczy się wspólna faza.
        /// </summary>
        void MeasureStepPhases()
        {
            var lf = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            var rf = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            if (lf == null || rf == null) return;
            for (int i = 1; i < LocoSlots; i++)
            {
                if (!locoHas[i]) continue;
                var clip = locoClips[i].GetAnimationClip();
                if (phaseCache.TryGetValue(clip, out float cached)) { locoPhase[i] = cached; continue; }
                for (int k = 0; k < LocoSlots; k++) loco.SetInputWeight(k, k == i ? 1f : 0f);
                const int Samples = 24;
                float best = float.MinValue, bestT = 0f;
                for (int n = 0; n < Samples; n++)
                {
                    float t = n / (float)Samples;
                    locoClips[i].SetTime(t * clip.length);
                    graph.Evaluate(0f);
                    float lead = Vector3.Dot(character.InverseTransformPoint(lf.position) - character.InverseTransformPoint(rf.position), SlotDir[i]);
                    if (lead > best) { best = lead; bestT = t; }
                }
                locoPhase[i] = bestT;
                phaseCache[clip] = bestT;
            }
            for (int k = 0; k < LocoSlots; k++) loco.SetInputWeight(k, k == 0 ? 1f : 0f);
            locoClips[0].SetTime(0);
            graph.Evaluate(0f);
        }

        // ------------------------------------------------------------------ Kierunek stóp (retarget)

        static readonly Dictionary<(Avatar, AnimationClip), Vector2> footYawCache = new Dictionary<(Avatar, AnimationClip), Vector2>();
        readonly Vector2[] footYawFix = new Vector2[LocoSlots]; // poprawka obrotu stóp na klip ruchu (stopnie): x – lewa, y – prawa
        Vector2 footFix;
        Transform footL, footR, shinL, shinR, thighL, thighR;

        /// <summary>Bieżąca poprawka obrotu stóp (stopnie, + w prawo patrząc z góry) – dla testów.</summary>
        public Vector2 FootYawFix => footFix;

        /// <summary>
        /// Udział poprawki stopy w podporze (kolano prawie proste) i w wymachu (kolano mocno zgięte). Skręt stopy względem goleni
        /// w przeniesionym biegu nie jest stały: stała poprawka lewej stopy Wojownika (41°) zostawiała w podporze ~5° na zewnątrz,
        /// a w wymachu była o ~15° za mała – pięta podciągnięta do tyłu odchodziła w lewo.
        /// </summary>
        public static float FootFixStanceGain = 1.1f, FootFixSwingGain = 1.4f;

        /// <summary>
        /// Przeniesienie biegu Mixamo na modele Meshy wykręca stopy na zewnątrz (w nagraniu stoją prosto): u Wojownika lewa stopa
        /// w podporze miała −22…−48° zamiast −13° z pozy spoczynkowej, kolano przy tym prosto. Dla każdego klipu ruchu mierzymy,
        /// jak stoją stopy w podporze (stopa nisko i płasko), i porównujemy z pozą spoczynkową modelu; różnicę oddaje obrót
        /// stopy w kostce (wokół osi goleni) po klipach, ważony udziałem klipu w ruchu i zgięciem kolana
        /// (<see cref="FootFixStanceGain"/>, <see cref="FootFixSwingGain"/>).
        /// </summary>
        Transform toeL, toeR;
        Quaternion restToeL, restToeR;
        Vector3 restToePosL, restToePosR, restToeScaleL, restToeScaleR;
        float restYawL, restYawR;
        Quaternion restModelFootL, restModelFootR;
        float restFootHeightL, restFootHeightR;
        Quaternion restModelThighL, restModelThighR, restModelShinL, restModelShinR;
        Vector3 restShinPositionL, restShinPositionR, restFootPositionL, restFootPositionR;
        Vector3 restThighDirectionL, restThighDirectionR, restShinDirectionL, restShinDirectionR;

        bool FootFlat(Transform foot, Transform toe, out float yaw)
        {
            Vector3 d = animator.transform.InverseTransformDirection(toe.position - foot.position);
            yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            return d.sqrMagnitude > 1e-6f && new Vector2(d.x, d.z).magnitude / d.magnitude > 0.75f;
        }

        /// <summary>Kierunek stóp w pozie spoczynkowej modelu (przy budowie – kości jeszcze nieruszone).</summary>
        void CaptureRestFeet()
        {
            footL = animator.GetBoneTransform(HumanBodyBones.LeftFoot); footR = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            shinL = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg); shinR = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            thighL = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg); thighR = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            toeL = animator.GetBoneTransform(HumanBodyBones.LeftToes); toeR = animator.GetBoneTransform(HumanBodyBones.RightToes);
            if (footL == null || footR == null || shinL == null || shinR == null || toeL == null || toeR == null || thighL == null || thighR == null)
            { footL = null; return; }
            FootFlat(footL, toeL, out restYawL);
            FootFlat(footR, toeR, out restYawR);
            restToeL = toeL.localRotation;
            restToeR = toeR.localRotation;
            restToePosL = toeL.localPosition; restToePosR = toeR.localPosition;
            restToeScaleL = toeL.localScale; restToeScaleR = toeR.localScale;
            if (character != null && def.sagittalRunFeet)
            {
                restModelFootL = Quaternion.Inverse(character.rotation) * footL.rotation;
                restModelFootR = Quaternion.Inverse(character.rotation) * footR.rotation;
                restFootHeightL = footL.position.y - character.position.y;
                restFootHeightR = footR.position.y - character.position.y;
                restModelThighL = Quaternion.Inverse(character.rotation) * thighL.rotation;
                restModelThighR = Quaternion.Inverse(character.rotation) * thighR.rotation;
                restModelShinL = Quaternion.Inverse(character.rotation) * shinL.rotation;
                restModelShinR = Quaternion.Inverse(character.rotation) * shinR.rotation;
                restShinPositionL = shinL.localPosition; restShinPositionR = shinR.localPosition;
                restFootPositionL = footL.localPosition; restFootPositionR = footR.localPosition;
                restThighDirectionL = character.InverseTransformDirection(shinL.position - thighL.position).normalized;
                restThighDirectionR = character.InverseTransformDirection(shinR.position - thighR.position).normalized;
                restShinDirectionL = character.InverseTransformDirection(footL.position - shinL.position).normalized;
                restShinDirectionR = character.InverseTransformDirection(footR.position - shinR.position).normalized;
            }
        }

        /// <summary>Korekta wadliwych celów IK w przeniesionym biegu, włączana tylko dla wskazanego modelu.</summary>
        void AlignArmoredRunFeet()
        {
            if (!def.sagittalRunFeet || !humanoid || footL == null || character == null) return;
            float action = actionLayers[1] != null ? Smooth(actionLayers[1].weight) : 0f;
            float weight = (loco.GetInputWeight(Walk) + loco.GetInputWeight(Run)) * (1f - action);
            if (weight < 0.001f) return;
            // Biodra mogą kierować krok na skos; klipy cofania, ruchu bokiem i pełne akcje pozostają bez korekty.
            Quaternion frame = Quaternion.AngleAxis(legTwist, character.up) * character.rotation;
            Vector3 right = frame * Vector3.right, forward = frame * Vector3.forward;
            float scale = animator.transform.lossyScale.y;
            float runShare = loco.GetInputWeight(Run) / Mathf.Max(0.001f, loco.GetInputWeight(Walk) + loco.GetInputWeight(Run));
            float halfTrack = Mathf.Lerp(0.08f, 0.055f, runShare) * scale;
            void Align(Transform thigh, Transform shin, Transform foot, Quaternion restThigh, Quaternion restShin, Quaternion restFoot,
                Vector3 shinPosition, Vector3 footPosition, Vector3 thighDirection, Vector3 restShinDirection, float restHeight, float side)
            {
                Vector3 hip = thigh.position, ankle = foot.position;
                Vector3 animatedKnee = shin.position;
                // Humanoid rozciągał goleń o ponad 2 cm do błędnego celu IK. Pancerz zachowuje wymiary pozy wiązania.
                shin.localPosition = Vector3.Lerp(shin.localPosition, shinPosition, weight);
                foot.localPosition = Vector3.Lerp(foot.localPosition, footPosition, weight);
                float upperLength = Vector3.Distance(hip, shin.position);
                float lowerLength = Vector3.Distance(shin.position, foot.position);
                // W biegu stopa ląduje bliżej osi ruchu niż biodro. Tor pod samym biodrem dawał szeroki, sztywny rozkrok.
                float lateral = Vector3.Dot(ankle - character.position, right);
                float residual = Mathf.Clamp(lateral - side * halfTrack, -0.04f * scale, 0.04f * scale) * 0.15f;
                Vector3 target = ankle + right * (side * halfTrack + residual - lateral);
                Vector3 toAnkle = target - hip;
                float reach = Mathf.Clamp(toAnkle.magnitude, Mathf.Abs(upperLength - lowerLength) + 0.001f, (upperLength + lowerLength) * 0.999f);
                Vector3 axis = toAnkle.normalized;
                // Zachowujemy niewielką pracę kolana z nagrania; ograniczamy wadliwy retarget, zamiast zamrażać zawias.
                Vector3 hint = Vector3.ProjectOnPlane(forward, axis).normalized;
                if (hint.sqrMagnitude < 1e-6f) return;
                Vector3 animatedHint = Vector3.ProjectOnPlane(animatedKnee - hip, axis);
                if (animatedHint.sqrMagnitude > 1e-6f)
                {
                    float bendPlane = Mathf.Clamp(Vector3.SignedAngle(hint, animatedHint, axis), -12f, 12f) * 0.30f;
                    hint = Quaternion.AngleAxis(bendPlane, axis) * hint;
                }
                float along = (upperLength * upperLength + reach * reach - lowerLength * lowerLength) / (2f * reach);
                Vector3 knee = hip + axis * along + hint * Mathf.Sqrt(Mathf.Max(0f, upperLength * upperLength - along * along));
                Quaternion upper = Quaternion.FromToRotation(frame * thighDirection, knee - hip) * frame * restThigh;
                thigh.rotation = Quaternion.Slerp(thigh.rotation, upper, weight);
                Vector3 shinDirection = hip + axis * reach - shin.position;
                Quaternion lower = Quaternion.FromToRotation(frame * restShinDirection, shinDirection) * frame * restShin;
                shin.rotation = Quaternion.Slerp(shin.rotation, lower, weight);
                // W podporze płaska podeszwa; w wymachu but podąża za golenią z ograniczonym zgięciem kostki.
                Vector3 localShin = Quaternion.Inverse(frame) * shinDirection.normalized;
                float pitch = Mathf.Clamp(Vector3.SignedAngle(Vector3.down, localShin, Vector3.right) * 0.65f - 5f, -25f, 65f);
                float lift = Mathf.InverseLerp(restHeight + 0.035f, restHeight + 0.20f, foot.position.y - character.position.y);
                Vector3 heading = Vector3.ProjectOnPlane(hint, character.up);
                float yaw = Mathf.Clamp(Vector3.SignedAngle(forward, heading, character.up), -4f, 4f);
                var rotation = frame * Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch * Smooth(lift), Vector3.right) * restFoot;
                foot.rotation = Quaternion.Slerp(foot.rotation, rotation, weight);
            }
            Align(thighL, shinL, footL, restModelThighL, restModelShinL, restModelFootL,
                restShinPositionL, restFootPositionL, restThighDirectionL, restShinDirectionL, restFootHeightL, -1f);
            Align(thighR, shinR, footR, restModelThighR, restModelShinR, restModelFootR,
                restShinPositionR, restFootPositionR, restThighDirectionR, restShinDirectionR, restFootHeightR, 1f);
        }

        void MeasureFootYaw()
        {
            if (footL == null) return;
            var root = animator.transform;
            float restL = restYawL, restR = restYawR;
            bool Flat(Transform foot, Transform toe, out float yaw) => FootFlat(foot, toe, out yaw);
            var avatar = animator.avatar;
            // Tylko ruch do przodu (chód, bieg): w biegu bokiem i w tył stopy z natury stoją pod innym kątem niż w spoczynku.
            foreach (int i in new[] { Walk, Run })
            {
                if (!locoHas[i]) continue;
                var clip = locoClips[i].GetAnimationClip();
                if (avatar != null && footYawCache.TryGetValue((avatar, clip), out var cached)) { footYawFix[i] = cached; continue; }
                for (int k = 0; k < LocoSlots; k++) loco.SetInputWeight(k, k == i ? 1f : 0f);
                const int Samples = 32;
                var yl = new List<(float yaw, float h, bool flat)>(); var yr = new List<(float yaw, float h, bool flat)>();
                for (int n = 0; n < Samples; n++)
                {
                    locoClips[i].SetTime(n / (float)Samples * clip.length);
                    graph.Evaluate(0f);
                    bool fa = Flat(footL, toeL, out float a), fb = Flat(footR, toeR, out float b);
                    yl.Add((a, root.InverseTransformPoint(footL.position).y, fa));
                    yr.Add((b, root.InverseTransformPoint(footR.position).y, fb));
                }
                // Podpora: stopa przy ziemi (do 4 cm nad najniższym położeniem w cyklu), płasko i palcami mniej więcej
                // do przodu – stopa zadarta do tyłu w wymachu też bywa „płaska”.
                float Fix(List<(float yaw, float h, bool flat)> samples, float rest)
                {
                    float lo = samples.Min(x => x.h);
                    float sx = 0f, sz = 0f; int count = 0;
                    foreach (var x in samples)
                        if (x.flat && x.h <= lo + 0.04f && Mathf.Abs(Mathf.DeltaAngle(x.yaw, rest)) < 80f)
                        { sx += Mathf.Sin(x.yaw * Mathf.Deg2Rad); sz += Mathf.Cos(x.yaw * Mathf.Deg2Rad); count++; }
                    if (count < 2) return 0f;
                    float fix = Mathf.DeltaAngle(Mathf.Atan2(sx, sz) * Mathf.Rad2Deg, rest);
                    return Mathf.Abs(fix) > 6f ? Mathf.Clamp(fix, -50f, 50f) : 0f; // drobne różnice to styl nagrania
                }
                footYawFix[i] = new Vector2(Fix(yl, restL), Fix(yr, restR));
                if (avatar != null) footYawCache[(avatar, clip)] = footYawFix[i];
            }
        }

        /// <summary>Obrót stóp w kostce wokół osi goleni (po klipach) – tyle, ile wynosi poprawka klipów ruchu w bieżącym miksie.</summary>
        void ApplyFootYaw()
        {
            if (!humanoid || footL == null) return;
            Vector2 fix = Vector2.zero;
            for (int i = 1; i < LocoSlots; i++) fix += footYawFix[i] * loco.GetInputWeight(i);
            fix *= 1f - (actionLayers[1] != null ? Smooth(actionLayers[1].weight) : 0f); // cios, unik – klip akcji prowadzi stopy
            void Turn(Transform thigh, Transform shin, Transform foot, ref float deg)
            {
                if (Mathf.Abs(deg) < 0.01f) return;
                Vector3 axis = shin.position - foot.position; // w górę wzdłuż goleni – dodatni kąt skręca palce w prawo
                if (axis.sqrMagnitude < 1e-6f) return;
                float bend = Vector3.Angle(shin.position - thigh.position, foot.position - shin.position); // 0 – noga prosta
                deg *= Mathf.Lerp(FootFixStanceGain, FootFixSwingGain, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(45f, 90f, bend)));
                foot.rotation = Quaternion.AngleAxis(deg, axis.normalized) * foot.rotation;
            }
            Turn(thighL, shinL, footL, ref fix.x);
            Turn(thighR, shinR, footR, ref fix.y);
            footFix = fix;
        }

        /// <summary>
        /// Idle_Combat odwraca tylną nogę niemal bokiem (prawa stopa ~80°). Nie prostujemy samej kostki:
        /// obrót całej nogi wokół osi biodro–kostka zachowuje punkt podparcia i długości kości.
        /// Stopie oddajemy dotychczasowe pochylenie podeszwy, zmieniając tylko kierunek w poziomie.
        /// Udział spoczynku i wygasanie warstwy całego ciała chronią kroki, ciosy i uniki.
        /// </summary>
        void ApplyIdleLegPose()
        {
            if (!humanoid || footL == null || character == null) return;
            float weight = (loco.GetInputWeight(Idle) + loco.GetInputWeight(LocoSlots))
                * (1f - (actionLayers[1] != null ? Smooth(actionLayers[1].weight) : 0f));
            if (weight <= 0.001f) return;
            void Align(Transform thigh, Transform foot, Transform toe)
            {
                Vector3 forward = toe.position - foot.position;
                Vector3 local = character.InverseTransformDirection(forward);
                if (new Vector2(local.x, local.z).sqrMagnitude < 0.000001f) return;
                float yaw = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
                float delta = Mathf.DeltaAngle(yaw, Mathf.Clamp(yaw, -40f, 40f)) * weight;
                if (Mathf.Abs(delta) < 0.01f) return;
                Vector3 axis = (thigh.position - foot.position).normalized;
                Quaternion yawTurn = Quaternion.AngleAxis(delta, character.up);
                Quaternion footRotation = yawTurn * foot.rotation;
                float legTurn = Vector3.SignedAngle(Vector3.ProjectOnPlane(forward, axis), Vector3.ProjectOnPlane(yawTurn * forward, axis), axis);
                thigh.rotation = Quaternion.AngleAxis(legTurn, axis) * thigh.rotation;
                foot.rotation = footRotation;
            }
            Align(thighL, footL, toeL);
            Align(thighR, footR, toeR);
        }

        // ------------------------------------------------------------------ Fazy walki → czas klipu

        /// <summary>
        /// Ciosy w Turris trwają ok. połowę nagrania Mixamo. Gdyby każdą fazę ściskać osobno, samo cięcie (krótka faza
        /// aktywna) szłoby ~2,4× szybciej niż nagranie (dłoń ~35 m/s), a powrót do gardy byłby pośpieszny. Dlatego faza
        /// aktywna obejmuje w klipie tylko przejście ostrza przez cel: zaczyna się chwilę przed klatką ciosu
        /// (<see cref="StrikeEarly"/> rozpiętości), kończy po <see cref="StrikeShare"/> rozpiętości, a dalsze
        /// wytracanie rozpędu należy już do powrotu. Powrót sięga <see cref="RecoveryReach"/> reszty klipu – ostatnie
        /// dociągnięcie do gardy daje przenikanie do ruchu. Tempo całego ciosu jest wtedy prawie równe (~1,7×),
        /// a szczyt prędkości dłoni ~25 m/s.
        /// </summary>
        public static float StrikeEarly = 0.1f, StrikeShare = 0.6f, RecoveryReach = 0.75f;

        /// <summary>Czas znormalizowany klipu dla bieżącej fazy walki.</summary>
        public static float NormalizedTime(ActionClip c, in CharacterAnimState s, float clipLength)
        {
            switch (s.action)
            {
                case AnimAction.Attack:
                {
                    float q = Mathf.Clamp01(s.phaseProgress);
                    // Strzał z łuku i pocisk czaru wylatują dokładnie na granicy faz – bez wyprzedzenia.
                    bool melee = s.attack != AttackAnim.BowDraw && s.attack != AttackAnim.Cast;
                    float span = c.activeEnd - c.windupEnd;
                    float strike = melee ? Mathf.Max(0f, c.windupEnd - span * StrikeEarly) : c.windupEnd;
                    float share = c.strikeShare > 0f ? c.strikeShare : StrikeShare;
                    float follow = melee ? c.windupEnd + span * share : c.activeEnd;
                    // Krótki wycinek paladyna nie może zwalniać po założeniu wolniejszej broni (korbacza).
                    // Nadmiar zapowiedzi to przygotowanie; sam zamach i przejście przez cel co najmniej w tempie nagrania.
                    if (melee && c.strikeShare > 0f && clipLength < 0.75f && s.windupSeconds > 0f && s.activeSeconds > 0f)
                    {
                        follow = Mathf.Min(1f, Mathf.Max(follow, strike + s.activeSeconds / clipLength));
                        if (s.phase == ActionPhase.Startup)
                        {
                            float preparation = Mathf.Max(0f,strike-(1f-q)*Mathf.Max(strike,s.windupSeconds/clipLength));
                            if (c.preparationBlendShare > 0f && strike > 0f)
                            {
                                // Wyjście z przygotowania ma zerową prędkość na początku i tempo nagrania przy trafieniu.
                                float u=preparation/strike;
                                preparation=strike*u*u*(2f-u);
                            }
                            return preparation;
                        }
                        if (s.phase == ActionPhase.Active)
                            return Mathf.Min(1f,strike + q*Mathf.Max(follow-strike,s.activeSeconds/clipLength));
                        if (s.phase == ActionPhase.Recovery)
                        {
                            float seconds = Mathf.Max(0f, s.actionDuration - s.windupSeconds - s.activeSeconds);
                            return Mathf.Min(1f, follow + q * Mathf.Max(1f - follow, seconds / clipLength));
                        }
                    }
                    switch (s.phase)
                    {
                        case ActionPhase.Startup: return Mathf.Lerp(0f, strike, q);
                        case ActionPhase.Active: return Mathf.Lerp(strike, follow, q);
                        case ActionPhase.Recovery: return Mathf.Lerp(follow, follow + (1f - follow) * RecoveryReach, q);
                        default: return 1f;
                    }
                }
                case AnimAction.Parry:
                case AnimAction.Cast:
                    float p = Mathf.Clamp01(s.phaseProgress);
                    switch (s.phase)
                    {
                        case ActionPhase.Startup: return Mathf.Lerp(0f, c.windupEnd, p);
                        case ActionPhase.Active: return Mathf.Lerp(c.windupEnd, c.activeEnd, p);
                        case ActionPhase.Recovery: return Mathf.Lerp(c.activeEnd, 1f, p);
                        default: return 1f;
                    }
                case AnimAction.Block:
                case AnimAction.Kneel:
                case AnimAction.GuardBroken:
                    if (c.loop && clipLength > 0) return (s.actionTime % clipLength) / clipLength;
                    return Mathf.Clamp01(s.actionTime / Mathf.Max(0.01f, clipLength));
                case AnimAction.Death:
                    return Mathf.Clamp01(s.actionTime / Mathf.Max(0.01f, clipLength));
                default:
                    return Mathf.Clamp01(s.actionTime / Mathf.Max(0.05f, s.actionDuration));
            }
        }

        /// <summary>
        /// Czas przenikania przy wejściu w akcję. Krótki dla ciosów i uników (liczy się czas reakcji), dłuższy dla
        /// śmierci i spokojnych gestów. Kolejny cios serii przechodzi z wybrzmienia poprzedniego.
        /// </summary>
        public static float FadeInto(AnimAction to, AnimAction from)
        {
            switch (to)
            {
                case AnimAction.Attack: return from == AnimAction.Attack ? 0.11f : 0.08f;
                case AnimAction.Dodge:
                case AnimAction.Backstep: return 0.07f;
                case AnimAction.Parry:
                case AnimAction.Flinch: return 0.06f;
                case AnimAction.Death: return 0.16f;
                default: return 0.14f;
            }
        }

        /// <summary>Powrót z akcji do ruchu – dłuższy niż wejście: końcówki klipów (cios z wyskoku, znad głowy) bywają daleko od gardy.</summary>
        public static float FadeOut = 0.24f;

        // ------------------------------------------------------------------ Klatka

        int measureIn;

        public void Tick(in CharacterAnimState s, Vector3 worldVelocity, float dt)
        {
            if (!graph.IsValid()) return;
            if (measureIn > 0 && --measureIn == 0)
            {
                MeasureFootYaw();
                MeasureStepPhases();
            }
            // W gardzie (blok, picie) bez biegu: krótkie, szybsze kroki chodu zamiast długiego wykroku biegu w tył.
            selfSpeed = Mathf.Lerp(selfSpeed, dt > 0f ? lastAdvance / dt : 0f, 1f - Mathf.Exp(-12f * dt));
            if (s.action == AnimAction.Attack || s.action == AnimAction.Parry) combatTimer = CombatStanceTime; // blok – bez przestawiania stóp
            else combatTimer = Mathf.Max(0f, combatTimer - dt);
            relaxing = hasCombatIdle && combatTimer <= 0f && combatMix > 0f;
            combatMix = hasCombatIdle ? Mathf.MoveTowards(combatMix, combatTimer > 0f ? 1f : 0f, dt / (combatTimer > 0f ? 0.25f : 0.6f)) : 0f;
            lastAdvance = 0f;
            UpdateLocomotion(worldVelocity, dt, s.action == AnimAction.Block || s.action == AnimAction.Drink);

            var clip = s.action == AnimAction.None ? null : FindClip(s.action, s.attack);
            float norm = clip != null ? NormalizedTime(clip, s, clip.clip.length) : 0f;
            // Ten sam klip od nowa (np. dwa jednakowe ciosy z rzędu) – też przenikanie, nie skok na początek klipu.
            bool restart = clip != null && clip == currentAction && !clip.loop && norm < lastNorm - 0.25f;
            if (clip != currentAction || restart)
            {
                SwapAction(clip, s.action, norm,clip != null ? s.windupSeconds*clip.preparationBlendShare : 0f);
                rootPeak = RootAt(clip, norm);
            }
            lastAction = s.action;
            lastNorm = norm;
            float advanceBefore = pendingAdvance;
            if (clip != null && s.action == AnimAction.Attack)
            {
                float root = RootAt(clip, norm);
                if (root > rootPeak) { pendingAdvance += root - rootPeak; rootPeak = root; }
            }

            if (currentAction != null)
            {
                var al = actionLayers[currentLayer];
                float sample = norm*currentAction.clip.length;
                al.sampleSpeed = dt > 0 ? Mathf.Max(0,(sample-(float)al.cur.GetTime())/dt) : 0f;
                al.cur.SetTime(sample);
            }
            for (int l = 1; l <= 2; l++)
            {
                var al = actionLayers[l];
                // Przygotowanie wybranego ciosu zachowuje ruch wybrzmienia poprzednika zamiast go zamrażać.
                // Wygładzona waga ma wtedy ciągłą prędkość na granicy ciosów.
                if (al.previousSpeed > 0 && al.mix < 1f && al.prev.IsValid())
                {
                    al.previousSpeed *= Mathf.Exp(-18f*dt);
                    al.prev.SetTime(Mathf.Min(al.prev.GetAnimationClip().length,(float)al.prev.GetTime()+al.previousSpeed*dt));
                }
                bool active = currentAction != null && currentLayer == l;
                if (active && currentAction.recoveryFadeStart > 0f && s.phase == ActionPhase.Recovery && s.phaseProgress >= currentAction.recoveryFadeStart)
                    active = false;
                if (active && currentAction.fadeWhenFinished && norm >= 0.999f) active = false;
                float rate = dt / Mathf.Max(0.01f, active ? al.fade : FadeOut);
                al.weight = Mathf.MoveTowards(al.weight, active ? 1f : 0f, rate);
                al.mix = Mathf.MoveTowards(al.mix, 1f, dt / Mathf.Max(0.01f, al.fade));
                // Wygładzone krzywe – start i koniec przenikania bez szarpnięcia.
                float m = Smooth(al.mix);
                al.mixer.SetInputWeight(0, m);
                al.mixer.SetInputWeight(1, 1f - m);
                layers.SetInputWeight(l, Smooth(al.weight));
            }
            graph.Evaluate(dt);
            // Klipy poruszają palcami jak w miękkim obuwiu. Stalowy nosek ma pozostać sztywny względem stopy.
            if (def.rigidBootToes && footL != null)
            {
                toeL.SetLocalPositionAndRotation(restToePosL, restToeL);
                toeR.SetLocalPositionAndRotation(restToePosR, restToeR);
                toeL.localScale = restToeScaleL; toeR.localScale = restToeScaleR;
            }
            offhandPose?.Apply(s.offhandAttack && s.action == AnimAction.Attack);
            ApplyFootYaw();
            ApplyIdleLegPose();
            ApplyLegTwist();
            ApplyLegIK(dt);
            Body?.Apply(s, worldVelocity, dt);
            AlignArmoredRunFeet();
            AnchorFeet(s, pendingAdvance - advanceBefore);
            lastAdvance = pendingAdvance - advanceBefore;
        }

        static float Smooth(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

        public void AddImpact(Vector3 localDir, float strength) => Body?.AddImpact(localDir, strength);

        /// <summary>Klip akcji: najpierw z zestawu broni, potem z modelu.</summary>
        ActionClip FindClip(AnimAction action, AttackAnim attack)
        {
            var c = set != null ? set.Find(action, attack) : null;
            return c ?? def.Find(action, attack);
        }

        void SwapAction(ActionClip clip, AnimAction action, float norm,float preparationDuration)
        {
            currentAction = clip;
            if (clip == null) return; // warstwa wygasa z zamrożonym ostatnim klipem
            int layer = clip.upperBodyOnly && humanoid ? 2 : 1;
            var al = actionLayers[layer];
            al.fade = Mathf.Max(FadeInto(action, lastAction),preparationDuration);
            bool visible = al.weight > 0.01f && al.curClip != null;
            // Poprzedni klip zostaje (zamrożony) na drugim wejściu i wygasa. Przy bardzo szybkiej zmianie zostaje ten,
            // który był bardziej widoczny.
            var keep = !visible ? default : (al.mix >= 0.5f ? al.cur : al.prev);
            al.previousSpeed = preparationDuration > 0f && visible && al.mix >= 0.5f ? al.sampleSpeed : 0f;
            var drop = !visible ? al.prev : (al.mix >= 0.5f ? al.prev : al.cur);
            var dropOther = !visible ? al.cur : default;
            graph.Disconnect(al.mixer, 0);
            graph.Disconnect(al.mixer, 1);
            if (drop.IsValid()) drop.Destroy();
            if (dropOther.IsValid()) dropOther.Destroy();
            if (!keep.IsValid()) keep = AnimationClipPlayable.Create(graph, Empty());
            var next = AnimationClipPlayable.Create(graph, clip.clip);
            next.SetSpeed(0); // czas ustawiany ręcznie
            next.SetApplyFootIK(false);
            graph.Connect(next, 0, al.mixer, 0);
            graph.Connect(keep, 0, al.mixer, 1);
            al.cur = next;
            al.prev = keep;
            al.curClip = clip;
            // Z widocznej warstwy – przenikanie klipów; z ruchu – narastanie udziału warstwy.
            al.mix = visible ? 0f : 1f;
            currentLayer = layer;
            // Odległe pozy (np. koniec cięcia nad głową → pchnięcie od biodra) przechodzą dłużej – dłonie nie mogą
            // pokonać metra w kilka klatek. Akcje wymuszone (trafienie, unik, parowanie, śmierć) dopuszczają szybszy
            // ruch rąk (szarpnięcie), ale też nie przeskok.
            if (humanoid)
            {
                bool urgent = IsUrgent(action);
                float travel = HandTravel(next, layer, norm * clip.clip.length);
                al.fade = Mathf.Clamp(travel / (urgent ? UrgentBlendHandSpeed : MaxBlendHandSpeed), al.fade,
                    Mathf.Max(al.fade,urgent ? UrgentMaxFade : MaxFade));
            }
        }

        static bool IsUrgent(AnimAction a) =>
            a == AnimAction.Flinch || a == AnimAction.Dodge || a == AnimAction.Backstep || a == AnimAction.Parry || a == AnimAction.Death || a == AnimAction.GuardBroken;

        /// <summary>Średnia prędkość dłoni w przejściu (m/s) – granica dla szybkiego, ale ludzkiego ruchu ręki.</summary>
        public const float MaxBlendHandSpeed = 7f, UrgentBlendHandSpeed = 8f;
        /// <summary>Najdłuższe przejście między akcjami (s) – dłuższe opóźniałoby czytelność ciosu albo reakcję.</summary>
        public const float MaxFade = 0.26f, UrgentMaxFade = 0.18f;

        /// <summary>Jak daleko przejdą dłonie z bieżącej pozy do pozy nowego klipu (próbna ewaluacja grafu).</summary>
        float HandTravel(AnimationClipPlayable next, int layer, float time)
        {
            var rh = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var lh = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            if (rh == null || lh == null || character == null) return 0f;
            Vector3 r0 = character.InverseTransformPoint(rh.position), l0 = character.InverseTransformPoint(lh.position);
            float w1 = layers.GetInputWeight(1), w2 = layers.GetInputWeight(2);
            var al = actionLayers[layer];
            float m0 = al.mixer.GetInputWeight(0), m1 = al.mixer.GetInputWeight(1);
            next.SetTime(time);
            layers.SetInputWeight(layer, 1f);
            layers.SetInputWeight(3 - layer, 0f);
            al.mixer.SetInputWeight(0, 1f);
            al.mixer.SetInputWeight(1, 0f);
            graph.Evaluate(0f);
            float d = Mathf.Max((character.InverseTransformPoint(rh.position) - r0).magnitude, (character.InverseTransformPoint(lh.position) - l0).magnitude);
            layers.SetInputWeight(1, w1);
            layers.SetInputWeight(2, w2);
            al.mixer.SetInputWeight(0, m0);
            al.mixer.SetInputWeight(1, m1);
            return d;
        }

        // ------------------------------------------------------------------ Ruch

        /// <summary>Najszybsze odtwarzanie klipu chodu w gardzie (cofanie z tarczą ~2,9 m/s przy chodzie w tył nagranym 1,1 m/s).</summary>
        const float GuardMaxStepRate = 2.7f;

        /// <summary>
        /// Najszybsze odtwarzanie klipu biegu. Bieg nagrany przy ~3,2 m/s grany 1,6× dla 5,2 m/s dawał cykl 0,4 s
        /// (~300 kroków/min): noga w powietrzu przeskakiwała 18–24 cm na klatkę i wyglądała jak kilka klatek na cykl.
        /// Ponad ten limit krok się wydłuża (ApplyStrideWarp) zamiast przyspieszać.
        /// </summary>
        const float RunCadenceMax = 1.2f;
        /// <summary>Największe wydłużenie kroku przez IK nóg (dalej noga prostuje się i zaczyna ślizgać).</summary>
        const float MaxStrideScale = 1.4f;

        float strideScale = 1f, strideTarget = 1f, locoMoving;
        /// <summary>Blokada stopy: miejsce na świecie, w którym stopa stanęła, i udział blokady (0–1).</summary>
        struct FootLock { public bool locked; public Vector3 pos; public float weight; }
        FootLock lockL, lockR;
        /// <summary>Stopa (kostka) uznana za postawioną / oderwaną – wysokość nad podłożem postaci [m].</summary>
        const float PlantHeight = 0.13f, LiftHeight = 0.17f;
        /// <summary>Dalej od pozy z klipu blokada puszcza (noga nie rozciąga się w nieskończoność przy ostrym zwrocie).</summary>
        const float MaxLockReach = 0.35f;
        Vector3 strideDir = Vector3.forward; // kierunek ruchu w układzie postaci
        Transform upperLegL, lowerLegL, upperLegR, lowerLegR, strideFootL, strideFootR;

        void UpdateLocomotion(Vector3 worldVelocity, float dt, bool guard)
        {
            Vector3 local = character != null ? Quaternion.Inverse(character.rotation) * worldVelocity : worldVelocity;
            local.y = 0;
            if (selfSpeed > 0.01f) local.z = local.z > 0f ? Mathf.Max(0f, local.z - selfSpeed * 1.15f) : local.z;
            float speed = local.magnitude;
            float shorten = 0f; // gardzie: część kroku „zabrana” (krótsze kroki, szybsza kadencja)
            float[] w = locoWeights;
            System.Array.Clear(w, 0, w.Length);
            twistTarget = 0f;
            if (speed < 0.08f) w[Idle] = 1f;
            else
            {
                // Kierunek: strefy zamiast mieszania cosinusami. Bieg na skos z dwóch nagrań (w przód i bokiem) krzyżował kolana
                // i przysiadał – teraz na skos gra jedno nagranie (przód albo tył), a biodra z nogami obracają się w stronę
                // ruchu (LegTwist), tułów zostaje zwrócony do celu. Bokiem – tylko blisko 90°.
                Vector3 d = local / speed;
                float a = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, abs = Mathf.Abs(a);
                float side = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(55f, 80f, abs)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(100f, 125f, abs)));
                float back = abs > 90f ? 1f - side : 0f, front = abs <= 90f ? 1f - side : 0f;
                float[] dir = { front, back, a < 0f ? side : 0f, a > 0f ? side : 0f };
                float sideTwist = a - 90f * Mathf.Sign(a), backTwist = a - 180f * Mathf.Sign(a);
                twistTarget = front * Mathf.Clamp(a, -MaxLegTwist, MaxLegTwist) + side * sideTwist + back * Mathf.Clamp(backTwist, -MaxLegTwist, MaxLegTwist);
                // Prędkości nagrania w tym kierunku – stąd próg chodu i biegu.
                float walkV = 0f, runV = 0f;
                for (int k = 0; k < 4; k++)
                {
                    walkV += dir[k] * locoSpeeds[Has(Walk + k) ? Walk + k : Walk];
                    runV += dir[k] * locoSpeeds[RunSlot(k)];
                }
                float toWalk = Mathf.Clamp01(speed / Mathf.Max(0.1f, walkV));
                float toRun = guard ? 0f : Mathf.Clamp01((speed - walkV) / Mathf.Max(0.1f, runV - walkV));
                w[Idle] = 1f - toWalk;
                for (int k = 0; k < 4; k++)
                {
                    if (dir[k] <= 0f) continue;
                    w[Has(Walk + k) ? Walk + k : Walk] += dir[k] * toWalk * (1f - toRun);
                    w[RunSlot(k)] += dir[k] * toWalk * toRun;
                }
                if (guard && speed > walkV)
                {
                    // Szybciej niż nagrany chód: domieszka postawy w miejscu skraca zakres nóg, a kadencja rośnie o tyle samo –
                    // stopy pokrywają tę samą drogę krótszymi krokami (bez ślizgania).
                    shorten = 0.2f * Mathf.Clamp01((speed - walkV) / Mathf.Max(0.1f, walkV));
                    for (int i = 1; i < LocoSlots; i++) w[i] *= 1f - shorten;
                    w[Idle] += shorten;
                }
            }
            // Faza kroków: tempo cykli to średnia ważona klipów, każdy z tempem dopasowanym do prędkości postaci.
            float cycles = 0f, moving = 0f, stride = 0f;
            if (speed > 0.08f) strideDir = local / speed;
            for (int i = 0; i < LocoSlots; i++)
            {
                float prev = i == Idle ? loco.GetInputWeight(Idle) + loco.GetInputWeight(LocoSlots) : loco.GetInputWeight(i);
                float cur = Mathf.MoveTowards(prev, w[i], dt * 7f);
                loco.SetInputWeight(i, cur);
                if (i == Idle || cur <= 0f) continue;
                float len = locoClips[i].GetAnimationClip().length;
                if (len <= 0.01f) continue;
                float rate = speed > 0.08f ? Mathf.Clamp(speed / Mathf.Max(0.1f, locoSpeeds[i]), 0.5f, guard ? GuardMaxStepRate : 2f) / (1f - shorten) : 1f;
                // Bieg: kadencja do RunCadenceMax, resztę drogi pokrywa dłuższy krok.
                float extra = 1f;
                if (i >= Run && !guard && rate > RunCadenceMax) { extra = rate / RunCadenceMax; rate = RunCadenceMax; }
                cycles += cur * rate / len;
                stride += cur * extra;
                moving += cur;
            }
            if (moving > 0f) stepPhase = Mathf.Repeat(stepPhase + dt * cycles / moving, 1f);
            locoMoving = moving;
            // Wydłużenie z wagą ruchu (spoczynek w mieszance go osłabia), wygładzone jak przejścia klipów.
            float strideWanted = moving > 0f ? Mathf.Lerp(1f, Mathf.Min(MaxStrideScale, stride / moving), Mathf.Clamp01(moving)) : 1f;
            strideTarget = strideWanted;
            strideScale = Mathf.MoveTowards(strideScale, strideTarget, dt * 4f);
            // Spoczynek dzieli się na zwykły i bojowy (po ciosie).
            float idleNow = loco.GetInputWeight(Idle); // w pętli wyżej: łączny udział spoczynku
            loco.SetInputWeight(Idle, idleNow * (1f - combatMix));
            loco.SetInputWeight(LocoSlots, idleNow * combatMix);
            legTwist = Mathf.MoveTowardsAngle(legTwist, twistTarget, dt * 420f);
            for (int i = 1; i < LocoSlots; i++)
                locoClips[i].SetTime(Mathf.Repeat(stepPhase + locoPhase[i], 1f) * locoClips[i].GetAnimationClip().length);
        }

        bool Has(int slot) => locoHas[slot];

        bool relaxing;
        Transform leftFoot, rightFoot;
        readonly float[] footRel = new float[2];
        bool footRelValid;

        /// <summary>
        /// Stopa na ziemi nie jedzie do tyłu: przy wejściu w cios ze spoczynku (stopy razem → wykrok) i przy powrocie
        /// do spoczynku poza ciosu przenika się z inną postawą – stopa podporowa cofałaby się względem bohatera.
        /// Bohater przesuwa się wtedy o tyle do przodu (ponad krok z nagrania), więc stopa stoi, a druga dostawia się
        /// naprzód. Tylko w ciosie i w wygasaniu akcji – w ruchu prowadzi chód.
        /// </summary>
        void AnchorFeet(in CharacterAnimState s, float advancedThisFrame)
        {
            if (!humanoid || character == null) return;
            if (leftFoot == null)
            {
                leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                if (leftFoot == null || rightFoot == null) return;
            }
            float l = character.InverseTransformPoint(leftFoot.position).z, r = character.InverseTransformPoint(rightFoot.position).z;
            var al = actionLayers[1];
            bool active = s.action == AnimAction.Attack || (s.action == AnimAction.None && al != null && al.weight > 0.001f && currentAction == null)
                          || (s.action == AnimAction.None && relaxing && loco.GetInputWeight(Idle) + loco.GetInputWeight(LocoSlots) > 0.95f);
            if (active && footRelValid)
            {
                float scale = animator.transform.lossyScale.y;
                float back = 0f;
                if (leftFoot.position.y - character.position.y < 0.16f * scale) back = Mathf.Max(back, footRel[0] - l);
                if (rightFoot.position.y - character.position.y < 0.16f * scale) back = Mathf.Max(back, footRel[1] - r);
                float extra = back - advancedThisFrame;
                if (extra > 0f) pendingAdvance += Mathf.Min(extra, 0.08f);
            }
            footRel[0] = l; footRel[1] = r;
            footRelValid = true;
        }

        AnimationClipPlayable combatIdle;
        bool hasCombatIdle;
        float combatTimer, combatMix;
        /// <summary>Jak długo po ciosie lub parowaniu bohater stoi w postawie bojowej (s).</summary>
        public static float CombatStanceTime = 4f;
        /// <summary>Udział postawy bojowej w spoczynku (0–1) – dla testów.</summary>
        public float CombatStance => combatMix;

        /// <summary>Największy obrót bioder i nóg względem tułowia przy biegu na skos (stopnie).</summary>
        const float MaxLegTwist = 65f;
        float twistTarget, legTwist;
        Transform hipsBone, spineBone, chestBone;

        /// <summary>Obrót nóg względem tułowia w stronę ruchu (stopnie, + w prawo) – dla testów.</summary>
        public float LegTwist => legTwist;

        /// <summary>
        /// Bieg na skos: biodra (z nogami) obracają się w stronę ruchu, kręgosłup i klatka oddają obrót – barki, broń
        /// i tarcza zostają zwrócone przed siebie. Akcja całego ciała (cios, unik) wygasza obrót.
        /// </summary>
        void ApplyLegTwist()
        {
            if (!humanoid) return;
            float k = 1f - (actionLayers[1] != null ? Smooth(actionLayers[1].weight) : 0f);
            float twist = legTwist * k;
            if (Mathf.Abs(twist) < 0.01f) return;
            if (hipsBone == null)
            {
                hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
                spineBone = animator.GetBoneTransform(HumanBodyBones.Spine);
                chestBone = animator.GetBoneTransform(HumanBodyBones.Chest);
                if (hipsBone == null || spineBone == null) return;
            }
            Vector3 up = character != null ? character.up : Vector3.up;
            hipsBone.rotation = Quaternion.AngleAxis(twist, up) * hipsBone.rotation;
            if (chestBone != null)
            {
                spineBone.rotation = Quaternion.AngleAxis(-twist * 0.6f, up) * spineBone.rotation;
                chestBone.rotation = Quaternion.AngleAxis(-twist * 0.4f, up) * chestBone.rotation;
            }
            else spineBone.rotation = Quaternion.AngleAxis(-twist, up) * spineBone.rotation;
        }

        /// <summary>
        /// IK nóg w ruchu, dwa etapy na stopę:
        /// 1) wydłużenie kroku (stride warping) – stopa z nagrania przesuwa się wzdłuż kierunku ruchu o (skala − 1) swojej
        ///    odległości od biodra, wysokość zostaje z klipu (szybki bieg przy umiarkowanej kadencji);
        /// 2) blokada stopy – stopa, która stanęła na ziemi, zostaje w tym miejscu świata do oderwania (zwroty, hamowanie,
        ///    różnice prędkości), a przy oderwaniu płynnie wraca do pozy z klipu.
        /// Dwukościowe IK udo–goleń zachowuje płaszczyznę kolana; stopa na ziemi zostaje płasko, w powietrzu idzie za goleniem.
        /// </summary>
        void ApplyLegIK(float dt)
        {
            if (!humanoid || character == null) return;
            float action = actionLayers[1] != null ? Smooth(actionLayers[1].weight) : 0f;
            float k = 1f + (strideScale - 1f) * (1f - action);
            bool lockOn = locoMoving > 0.3f && action < 0.5f;
            if (k < 1.005f && !lockOn && lockL.weight <= 0f && lockR.weight <= 0f) { lockL.locked = lockR.locked = false; return; }
            if (upperLegL == null)
            {
                upperLegL = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg); lowerLegL = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                upperLegR = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg); lowerLegR = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                strideFootL = animator.GetBoneTransform(HumanBodyBones.LeftFoot); strideFootR = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                if (upperLegL == null || lowerLegL == null || upperLegR == null || lowerLegR == null || strideFootL == null || strideFootR == null) { upperLegL = null; return; }
            }
            Vector3 dir = character.TransformDirection(strideDir); dir.y = 0f;
            dir = dir.sqrMagnitude < 1e-4f ? Vector3.zero : dir.normalized;
            float ground = character.position.y;
            Leg(upperLegL, lowerLegL, strideFootL, ref lockL, dir, k, ground, lockOn, dt);
            Leg(upperLegR, lowerLegR, strideFootR, ref lockR, dir, k, ground, lockOn, dt);
        }

        static void Leg(Transform upper, Transform lower, Transform foot, ref FootLock fl, Vector3 dir, float k, float ground, bool lockOn, float dt)
        {
            Vector3 hip = upper.position, ankle = foot.position;
            Vector3 anim = ankle + dir * (Vector3.Dot(ankle - hip, dir) * (k - 1f));
            float h = anim.y - ground;
            if (!lockOn) fl.locked = false;
            else if (!fl.locked && h < PlantHeight) { fl.locked = true; fl.pos = anim; }
            else if (fl.locked)
            {
                Vector3 off = fl.pos - anim; off.y = 0f;
                if (h > LiftHeight || off.sqrMagnitude > MaxLockReach * MaxLockReach) fl.locked = false;
            }
            fl.weight = Mathf.MoveTowards(fl.weight, fl.locked ? 1f : 0f, dt * (fl.locked ? 24f : 10f));
            Vector3 target = Vector3.Lerp(anim, new Vector3(fl.pos.x, anim.y, fl.pos.z), Smooth(fl.weight));
            if ((target - ankle).sqrMagnitude < 1e-8f) return;
            float planted = Mathf.Max(1f - Mathf.InverseLerp(0.15f, 0.3f, h), fl.weight);
            Reach(upper, lower, foot, target, dir, planted);
        }

        /// <summary>Dwukościowe IK: kolano w płaszczyźnie z bieżącej pozy; planted – udział obrotu stopy na świecie (reszta lokalnie).</summary>
        static void Reach(Transform upper, Transform lower, Transform foot, Vector3 target, Vector3 dir, float planted)
        {
            Vector3 hip = upper.position, knee = lower.position, ankle = foot.position;
            float a = Vector3.Distance(hip, knee), b = Vector3.Distance(knee, ankle);
            Vector3 toTarget = target - hip;
            if (toTarget.sqrMagnitude < 1e-6f) return;
            float d = Mathf.Clamp(toTarget.magnitude, Mathf.Abs(a - b) + 1e-3f, (a + b) * 0.999f);
            Quaternion footWorld = foot.rotation, footLocal = foot.localRotation;
            Vector3 hint = knee - hip;
            Vector3 normal = Vector3.Cross(toTarget, hint);
            if (normal.sqrMagnitude < 1e-8f) normal = Vector3.Cross(toTarget, dir.sqrMagnitude > 0f ? dir : Vector3.forward);
            if (normal.sqrMagnitude < 1e-8f) return;
            float cosHip = Mathf.Clamp((a * a + d * d - b * b) / (2f * a * d), -1f, 1f);
            Vector3 thigh = Quaternion.AngleAxis(Mathf.Acos(cosHip) * Mathf.Rad2Deg, normal.normalized) * toTarget.normalized;
            upper.rotation = Quaternion.FromToRotation(knee - hip, thigh) * upper.rotation;
            knee = lower.position;
            lower.rotation = Quaternion.FromToRotation(foot.position - knee, target - knee) * lower.rotation;
            foot.localRotation = footLocal;
            foot.rotation = Quaternion.Slerp(foot.rotation, footWorld, planted);
        }

        /// <summary>Bieg w kierunku k (0 przód, 1 tył, 2 lewo, 3 prawo): własny klip, inaczej chód w tym kierunku, inaczej bieg w przód.</summary>
        int RunSlot(int k)
        {
            if (Has(Run + k)) return Run + k;
            if (k > 0 && Has(Walk + k)) return Walk + k;
            return Run;
        }

        public void Dispose()
        {
            offhandPose?.Dispose();
            if (graph.IsValid()) graph.Destroy();
        }
    }
}
