using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Pełny zamach topora i młota dwuręcznego (ciosy z <see cref="For"/>: cięcia, rąbnięcia, ciosy z biegu i po uniku).
    /// Klipy Mixamo z paczki miecza mają krótki łuk: rąbnięcie kończy się z ostrzem przy biodrze, cięcie – przed ciałem,
    /// a postać od razu wraca do gardy, jakby bała się uderzyć. Wzorzec (ciężki topór souls-like) prowadzi ciało za bronią:
    /// głębszy zamach, tułów i biodra idą za ciosem (rąbnięcie – przysiad i ostrze w ziemi, cięcie – skręt na drugą stronę),
    /// a poza końca ciosu chwilę trwa, zanim postać się podniesie.
    /// Nakładane po klipie: zgięcie i skręt rozłożone na kręgi, biodra w dół, nogi dwukościowym IK do stóp z klipu
    /// (stopy nie zapadają się w podłogę). Wartości docelowe dochodzą przez krótkie wygładzenie – przejście
    /// do kolejnego ciosu serii nie skacze.
    /// </summary>
    public class GreatSwingPose
    {
        /// <summary>Profil ciosu: zamach (wind) i dociągnięcie (follow) – stopnie, metry; skręt + w prawo (widok z góry, zgodnie z zegarem).</summary>
        public struct Profile
        {
            public float windPitch, windYaw, followPitch, followYaw, followRoll, drop;
            /// <summary>Rąbnięcie: prawa dłoń w tym punkcie (przestrzeń postaci, m), trzonek w kierunku aim (ostrze przed postacią, w ziemi).</summary>
            public bool reach;
            public Vector3 hand, aim;
        }

        // Rąbnięcie: odchylenie w zamachu, potem głęboki skłon i przysiad – ostrze dochodzi do ziemi.
        static readonly Profile ChopProfile = new Profile { windPitch = -14f, followPitch = 34f, drop = 0.2f,
            reach = true, hand = new Vector3(0.06f, 0.62f, 0.62f), aim = new Vector3(0f, -0.72f, 0.7f) };
        static readonly Profile ChopFollowProfile = new Profile { windPitch = -10f, followPitch = 30f, drop = 0.18f,
            reach = true, hand = new Vector3(0.1f, 0.66f, 0.6f), aim = new Vector3(0.05f, -0.7f, 0.72f) };
        // Cięcie poziome: skręt od celu w zamachu i przez cel na drugą stronę po ciosie, lekki skłon i ugięcie nóg.
        // Pierwsze cięcie idzie z prawej na lewo i kończy się z toporem nisko po lewej, drugie – odwrotnie.
        static readonly Profile CleaveProfile = new Profile { windYaw = 22f, followYaw = -38f, followPitch = 12f, followRoll = -6f, drop = 0.1f,
            reach = true, hand = new Vector3(-0.3f, 0.92f, 0.42f), aim = new Vector3(-0.85f, -0.3f, 0.35f) };
        static readonly Profile CleaveBackProfile = new Profile { windYaw = -22f, followYaw = 38f, followPitch = 12f, followRoll = 6f, drop = 0.1f,
            reach = true, hand = new Vector3(0.42f, 0.92f, 0.36f), aim = new Vector3(0.85f, -0.3f, 0.3f) };
        static readonly Profile SweepProfile = new Profile { windYaw = 18f, followYaw = -36f, followPitch = 18f, followRoll = -6f, drop = 0.16f,
            reach = true, hand = new Vector3(-0.3f, 0.8f, 0.45f), aim = new Vector3(-0.8f, -0.4f, 0.4f) };
        // Skok z biegu: lądowanie w przysiadzie z ostrzem w ziemi (jak rąbnięcie).
        static readonly Profile LeapProfile = new Profile { windPitch = -12f, followPitch = 32f, drop = 0.22f,
            reach = true, hand = new Vector3(0.06f, 0.6f, 0.62f), aim = new Vector3(0f, -0.72f, 0.7f) };

        /// <summary>Udział powrotu, przez który poza końca ciosu trwa w pełni (potem łagodnie wraca do klipu).</summary>
        public const float FollowHold = 0.35f;
        const float SmoothTime = 0.045f;
        const float MaxPitch = 40f, MaxYaw = 45f;
        /// <summary>Ostrze kończy rąbnięcie tuż nad podłogą (m).</summary>
        const float GroundClearance = 0.02f;
        readonly System.Collections.Generic.List<Renderer> renderers = new System.Collections.Generic.List<Renderer>();

        readonly Transform character, hips;
        readonly Transform[] spine;
        readonly float[] share;
        readonly Transform upperL, lowerL, footL, upperR, lowerR, footR;
        readonly Transform armR, foreR, handR, clavL, clavR;
        /// <summary>Połowa szerokości barków (m) – rozstaw stóp w postawie topora.</summary>
        readonly float halfShoulders;
        readonly Transform head;
        /// <summary>Spoczynek z topora: największy skręt stopy od przodu postaci (°) i kolana od kierunku stopy.</summary>
        /// <summary>Największe opuszczenie uniesionej stopy (m).</summary>
        const float MaxLevel = 0.12f;
        /// <summary>Liczba punktów buta na stopę (próbka wierzchołków).</summary>
        const int SolePoints = 240;
        /// <summary>Wysunięcie lewej stopy przed / prawej za środek (m) i największe podniesienie bioder przy zwężaniu rozkroku.</summary>
        public const float StanceDepth = 0.14f, MaxStanceRaise = 0.15f;

        float pitch, yaw, roll, drop, pitchV, yawV, rollV, dropV, reachW, reachV;
        Vector3 hand, aim;

        /// <summary>
        /// Postawa topora i młota w spoczynku i w chodzie: prawa dłoń na trzonku przy prawej piersi, lewa niżej, przy pasie
        /// (rozstaw <see cref="HellEquipmentPose.HeavyGripSpan"/>), broń pionowo ostrzem w górę (jak ciężki topór souls-like), lekkie ugięcie kolan. Miecz dwuręczny trzyma broń po swojemu (klip).
        /// </summary>
        public bool Guard { get; set; }
        /// <summary>Chwyt postawy: prawa dłoń (przestrzeń postaci, m), kierunek trzonka ku ostrzu.</summary>
        /// Wzorzec użytkownika (2026-10-31, zrzut z Elden Ring): topór oburącz nisko – trzonek poziomo przed pasem, ramiona opuszczone,
        /// dłonie przy biodrach, głowica z prawej, ostrzem w dół. Ręce berserkera wydłużone do ludzkich proporcji (ramię + przedramię ≈ 0,50 m):
        /// trzonek na wysokości bioder ~0,3 m przed postacią (przód brzucha ≈ 0,24 m). Jak na wzorcu prawa dłoń trzyma wyżej
        /// na trzonku (broń przesunięta w dłoni o <see cref="GuardSlide"/>), lewa przy końcu – rozstaw
        /// <see cref="HellEquipmentPose.GuardGripSpan"/>, obie dłonie przy biodrach, nie przy środku pasa.
        /// Dawniej topór prawie pionowo przy ciele i dłonie, które nie sięgały trzonka przed tułowiem („ręce w tułowiu”).
        public static Vector3 GuardHand = new Vector3(0.30f, 0.95f, 0.32f), GuardAim = new Vector3(0.97f, -0.03f, 0.14f);
        /// <summary>Przesunięcie broni w prawej dłoni w gardzie (m, ku końcowi trzonka): dłoń ~0,68 m od końca zamiast 0,34 m.</summary>
        public const float GuardSlide = 0.32f;
        /// <summary>Udział niskiej gardy (0–1) dla chwytu: przesunięcie broni, rozstaw dłoni, obrót broni wokół trzonka.</summary>
        public float GuardGrip { get; private set; }
        /// <summary>Obrót broni wokół trzonka w dłoni (°) – ostrze w dół bez wykręcania nadgarstka.</summary>
        public float GuardSpin { get; private set; }
        Quaternion restR = Quaternion.identity;
        /// <summary>Kierunek ostrza (od trzonka ku krawędzi) w postawie – w dół, lekko w przód (przestrzeń postaci).</summary>
        public static Vector3 GuardBlade = new Vector3(0f, -0.9f, 0.4f);
        /// <summary>Wysunięcie barków (obrót obojczyków wokół pionu, °) w postawie – więcej zasięgu dla dłoni przed tułowiem.</summary>
        public const float GuardShoulderForward = 6f;
        /// <summary>Bieg: topór oburącz przed sobą jak w gardzie (uwaga użytkownika 2026-10-31, wzorzec z Elden Ring) – trochę wyżej
        /// i dalej w przód, bo tułów w biegu pochyla się; głowica po prawej, lekko w przód. Dawniej broń niesiona przy prawym boku
        /// skośnie w dół i do tyłu („ucieka na dół i w bok”).</summary>
        public static Vector3 CarryHand = new Vector3(0.28f, 1.02f, 0.38f), CarryAim = new Vector3(0.95f, 0.04f, 0.30f);
        public const float GuardDrop = 0.04f, GuardSmooth = 0.12f;
        /// <summary>Wyprostowanie pleców i uniesienie głowy w spoczynku (°, ujemne – do tyłu / w górę): klip garbi się i patrzy w ziemię.</summary>
        /// Dawne −16° wyginało postać do tyłu; −4° tylko łagodzi pochylenie klipu (zostaje ~3–4° w przód). Głowa lekko uniesiona.
        public const float GuardLean = -4f, GuardHeadLift = -10f;
        float guardW, guardV, carry, carryV;
        /// <summary>Udział postawy topora (0–1) – dla testów.</summary>
        public float GuardWeight => guardW;

        /// <summary>Gniazdo broni w prawej dłoni (+Y – wzdłuż trzonka ku ostrzu); bez niego dłoń sięga, ale broń się nie kieruje.</summary>
        public Transform Weapon { get; set; }

        /// <summary>Bieżące wychylenie (dla testów): x – skłon, y – skręt, z – obniżenie bioder [m].</summary>
        public Vector3 Current => new Vector3(pitch, yaw, drop);

        /// <param name="guard">postawa topora (<see cref="Guard"/>) – podeszwy kalibrowane na siatce przy budowie postaci.</param>
        public GreatSwingPose(Animator animator, Transform character, bool guard = false)
        {
            Guard = guard;
            this.character = character;
            hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            var s = animator.GetBoneTransform(HumanBodyBones.Spine);
            var c = animator.GetBoneTransform(HumanBodyBones.Chest);
            var u = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (u != null) { spine = new[] { s, c, u }; share = new[] { 0.3f, 0.35f, 0.35f }; }
            else { spine = new[] { s, c }; share = new[] { 0.4f, 0.6f }; }
            upperL = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg); lowerL = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            footL = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            upperR = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg); lowerR = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            footR = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            armR = animator.GetBoneTransform(HumanBodyBones.RightUpperArm); foreR = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            var armL = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            head = animator.GetBoneTransform(HumanBodyBones.Neck) ?? animator.GetBoneTransform(HumanBodyBones.Head);
            halfShoulders = armL != null && armR != null ? Mathf.Clamp(0.5f * Vector3.Distance(armL.position, armR.position), 0.12f, 0.3f) : 0.2f;
            handR = animator.GetBoneTransform(HumanBodyBones.RightHand);
            clavL = animator.GetBoneTransform(HumanBodyBones.LeftShoulder); clavR = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            if (foreR != null && handR != null) restR = HellEquipmentPose.BindRelative(animator, foreR, handR);
            if (guard && footL != null && footR != null) CalibrateSoles(animator);
        }

        /// <summary>Punkty buta (przestrzeń kości stopy, wierzchołki poruszają się sztywno ze stopą).</summary>
        sealed class Sole { public Vector3[] points; }
        Sole soleL, soleR;

        /// <summary>Punkty buta z BakeMesh przy budowie postaci, przeliczone do przestrzeni kości stopy (bez BakeMesh w pętli).</summary>
        void CalibrateSoles(Animator animator)
        {
            var baked = new Mesh();
            var feet = new[] { footL, footR };
            var lists = new[] { new System.Collections.Generic.List<Vector3>(), new System.Collections.Generic.List<Vector3>() };
            foreach (var skin in animator.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skin.BakeMesh(baked);
                foreach (var v in baked.vertices)
                {
                    Vector3 p = skin.transform.TransformPoint(v);
                    int k = (p - footL.position).sqrMagnitude < (p - footR.position).sqrMagnitude ? 0 : 1;
                    if ((p - feet[k].position).sqrMagnitude < 0.3f * 0.3f) lists[k].Add(feet[k].InverseTransformPoint(p));
                }
            }
            if (Application.isPlaying) Object.Destroy(baked); else Object.DestroyImmediate(baked);
            var sole = new Sole[2];
            for (int k = 0; k < 2; k++)
            {
                if (lists[k].Count < 20) return;
                int step = Mathf.Max(1, lists[k].Count / SolePoints);
                var pts = new Vector3[(lists[k].Count + step - 1) / step];
                for (int j = 0; j < pts.Length; j++) pts[j] = lists[k][j * step];
                sole[k] = new Sole { points = pts };
            }
            soleL = sole[0]; soleR = sole[1];
        }

        /// <summary>
        /// Oś stopy w poziomie: główna oś spodu buta (punkty poniżej kostki); zwrot bez znaczenia dla poziomowania podeszwy.
        /// Oś kości palców w tym rigu jest krzywa i nie nadaje się.
        /// </summary>
        Vector3 FootDir(Transform foot, Sole sole)
        {
            Vector3 up = character.up, right = character.right, fwd = character.forward, ankle = foot.position;
            float sx = 0f, sz = 0f; int n = 0;
            for (int i = 0; i < sole.points.Length; i++)
            {
                Vector3 d = foot.TransformPoint(sole.points[i]) - ankle;
                if (Vector3.Dot(d, up) > -0.02f) continue;
                sx += Vector3.Dot(d, right); sz += Vector3.Dot(d, fwd); n++;
            }
            if (n < 5) return fwd;
            float mx = sx / n, mz = sz / n, cxx = 0f, cxz = 0f, czz = 0f;
            for (int i = 0; i < sole.points.Length; i++)
            {
                Vector3 d = foot.TransformPoint(sole.points[i]) - ankle;
                if (Vector3.Dot(d, up) > -0.02f) continue;
                float x = Vector3.Dot(d, right) - mx, z = Vector3.Dot(d, fwd) - mz;
                cxx += x * x; cxz += x * z; czz += z * z;
            }
            float angle = 0.5f * Mathf.Atan2(2f * cxz, czz - cxx); // oś główna od przodu postaci
            return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, up) * fwd;
        }

        /// <summary>
        /// Podeszwa w poziomie: najniższy punkt pięty (za kostką) i przodu stopy (przed kostką) na tej samej wysokości
        /// – obrót stopy wokół osi poprzecznej. Kierunek z siatki buta, nie z osi kości (rig berserkera ma je nierówne).
        /// </summary>
        void LevelSole(Transform foot, Sole sole, Vector3 dir)
        {
            Vector3 up = character.up, lateral = Vector3.Cross(up, dir);
            for (int it = 0; it < 2; it++)
            {
                Vector3 ankle = foot.position;
                float hHeel = float.MaxValue, aHeel = 0f, hBall = float.MaxValue, aBall = 0f;
                foreach (var q in sole.points)
                {
                    Vector3 d = foot.TransformPoint(q) - ankle;
                    float h = Vector3.Dot(d, up), a = Vector3.Dot(d, dir);
                    if (h > -0.02f || Mathf.Abs(Vector3.Dot(d, lateral)) > 0.1f) continue;
                    if (a < -0.01f) { if (h < hHeel) { hHeel = h; aHeel = a; } }
                    else if (a > 0.06f && h < hBall) { hBall = h; aBall = a; }
                }
                if (hHeel == float.MaxValue || hBall == float.MaxValue || aBall - aHeel < 0.05f) return;
                float angle = Mathf.Atan2(hBall - hHeel, aBall - aHeel) * Mathf.Rad2Deg;
                foot.rotation = Quaternion.AngleAxis(Mathf.Clamp(angle, -40f, 40f), lateral) * foot.rotation;
            }
        }

        /// <summary>
        /// Stopa w spoczynku z toporem: klip po retargecie na krępy rig stawia ją na palcach – podeszwa kładzie się
        /// poziomo (<see cref="LevelSole"/>), skręt stopy zostaje z klipu. Wysokość wyrównuje potem <see cref="LevelFeet"/>.
        /// </summary>
        void PlantFoot(Transform foot, Sole sole, float w)
        {
            if (foot == null || sole == null || w < 0.002f) return;
            Quaternion footRot = foot.rotation;
            LevelSole(foot, sole, FootDir(foot, sole));
            foot.rotation = Quaternion.Slerp(footRot, foot.rotation, w);
        }

        /// <summary>
        /// Stopy na szerokość barków, lewa lekko z przodu (prawa ręka prowadzi topór), zamiast szermierczego wykroku
        /// z klipu (stopy niemal w jednej linii, 0,9 m jedna za drugą). Wysokość kostek zostaje z klipu; biodra idą w górę
        /// tak, by nogi zachowały wyprost z klipu (bez tego krótszy rozkrok kończy się przysiadem).
        /// </summary>
        void PlaceFeet(float w)
        {
            if (upperL == null || footL == null || upperR == null || footR == null) return;
            Vector3 up = character.up;
            Vector3 targetL = Stance(footL, -halfShoulders, StanceDepth, w), targetR = Stance(footR, halfShoulders, -StanceDepth, w);
            float raise = Mathf.Min(Raise(upperL, footL, targetL), Raise(upperR, footR, targetR));
            raise = Mathf.Clamp(raise, 0f, MaxStanceRaise);
            if (raise > 0.001f) hips.position += up * raise;
            ClipAnimationDriver.Reach(upperL, lowerL, footL, targetL, character.forward, 1f, KneeHint(-1f));
            ClipAnimationDriver.Reach(upperR, lowerR, footR, targetR, character.forward, 1f, KneeHint(1f));
        }

        /// <summary>Kąt (°), o który kolano w postawie odchyla się od przodu na zewnątrz – nad palcami lekko rozstawionych stóp.</summary>
        public const float KneeOut = 8f;

        /// <summary>Kierunek kolana dla IK nogi (side: −1 lewa, +1 prawa): przód postaci, lekko na zewnątrz.</summary>
        Vector3 KneeHint(float side) => Quaternion.AngleAxis(KneeOut * side, character.up) * character.forward;

        /// <summary>Kostka w postawie: x, z w przestrzeni postaci, wysokość z klipu; w – udział postawy.</summary>
        Vector3 Stance(Transform foot, float x, float z, float w)
        {
            Vector3 local = character.InverseTransformPoint(foot.position);
            return character.TransformPoint(Vector3.Lerp(local, new Vector3(x, local.y, z), w));
        }

        /// <summary>O ile podnieść biodro, by odległość biodro–kostka po przestawieniu stopy była taka jak w klipie.</summary>
        float Raise(Transform upper, Transform foot, Vector3 target)
        {
            Vector3 up = character.up, hip = upper.position;
            float d = Vector3.Distance(hip, foot.position);
            float r = Vector3.ProjectOnPlane(hip - target, up).magnitude;
            if (r >= d) return 0f;
            return Mathf.Sqrt(d * d - r * r) - Vector3.Dot(hip - target, up);
        }

        /// <summary>Spód buta nad poziomem postaci (m) – najniższy z punktów podeszwy.</summary>
        float Bottom(Transform foot, Sole sole)
        {
            float y = float.MaxValue;
            foreach (var q in sole.points) y = Mathf.Min(y, Vector3.Dot(foot.TransformPoint(q) - character.position, character.up));
            return y;
        }

        /// <summary>
        /// Stopa uniesiona na palcach w klipie schodzi (IK nogi) do poziomu niższej stopy – obie stoją na jednej podłodze.
        /// Poziom niższej stopy zostaje z klipu, żeby postać nie podskakiwała między spoczynkiem a chodem.
        /// </summary>
        void LevelFeet(float w)
        {
            if (soleL == null || soleR == null) return;
            float bl = Bottom(footL, soleL), br = Bottom(footR, soleR), floor = Mathf.Min(bl, br);
            float sl = Mathf.Min(bl - floor, MaxLevel) * w, sr = Mathf.Min(br - floor, MaxLevel) * w;
            if (sl > 0.001f) ClipAnimationDriver.Reach(upperL, lowerL, footL, footL.position - character.up * sl, character.forward, 1f, KneeHint(-1f));
            if (sr > 0.001f) ClipAnimationDriver.Reach(upperR, lowerR, footR, footR.position - character.up * sr, character.forward, 1f, KneeHint(1f));
        }

        public static bool For(AttackAnim a, out Profile p)
        {
            switch (a)
            {
                case AttackAnim.Chop: case AttackAnim.DodgeChop: p = ChopProfile; return true;
                case AttackAnim.ChopFollow: p = ChopFollowProfile; return true;
                case AttackAnim.Cleave: p = CleaveProfile; return true;
                case AttackAnim.CleaveBack: p = CleaveBackProfile; return true;
                case AttackAnim.RunningSweep: p = SweepProfile; return true;
                case AttackAnim.LeapChop: p = LeapProfile; return true;
                default: p = default; return false;
            }
        }

        /// <summary>
        /// Udział zamachu i dociągnięcia w danej chwili ciosu: zamach narasta do końca przygotowania (i trwa przy ładowaniu),
        /// w fazie aktywnej szybko przechodzi w dociągnięcie, które trwa przez <see cref="FollowHold"/> powrotu i gaśnie.
        /// </summary>
        public static void Envelope(in CharacterAnimState s, out float wind, out float follow)
        {
            float q = Mathf.Clamp01(s.phaseProgress);
            wind = follow = 0f;
            switch (s.phase)
            {
                case ActionPhase.Startup: wind = Smooth(Mathf.InverseLerp(0.2f, 1f, q)); break;
                case ActionPhase.Active:
                    float t = Mathf.Sqrt(q); // przejście ostrza – szybko na początku, jak uderzenie
                    wind = 1f - Smooth(t); follow = Smooth(t); break;
                case ActionPhase.Recovery: follow = 1f - Smooth(Mathf.InverseLerp(FollowHold, 1f, q)); break;
            }
        }

        /// <param name="rest">udział ruchu bez akcji całego ciała (0 – cios, unik…).</param>
        /// <param name="running">udział biegu w ruchu (0–1) – chwyt do biegu zamiast postawy.</param>
        /// <param name="still">udział stania w miejscu (0–1) – stopy płasko, kolana nad stopami.</param>
        public void Apply(in CharacterAnimState s, float dt, float rest = 0f, float running = 0f, float still = 0f)
        {
            GuardGrip = 0f; GuardSpin = 0f;
            ApplyPose(s, dt, rest, running, still);
            // Ciosy bez profilu (np. klipy autorskie – ActionClip.authoredSwing): broń nie wchodzi w podłogę.
            // Klip układany na szkielecie Mixamo, a postacie mają różne proporcje – dłoń idzie w górę o tyle, ile broń jest pod podłogą.
            if (FloorGuard && s.action == AnimAction.Attack && reachW <= 0.002f) KeepWeaponAboveFloor();
        }

        /// <summary>Ustawiane przez sterownik: bieżący cios to klip autorski – pilnuj, by broń nie weszła w podłogę.</summary>
        public bool FloorGuard { get; set; }

        void KeepWeaponAboveFloor()
        {
            if (Weapon == null || armR == null || foreR == null || handR == null || character == null) return;
            float under = character.position.y + GroundClearance - LowestWeaponPoint();
            if (under > 0f) HellEquipmentPose.Solve(armR, foreR, handR, handR.position + character.up * under, character.right - character.up, 1f);
        }

        void ApplyPose(in CharacterAnimState s, float dt, float rest, float running, float still)
        {
            if (character == null || hips == null || dt <= 0f) return;
            float tp = 0f, ty = 0f, tr = 0f, td = 0f, tw = 0f;
            if (s.action == AnimAction.Attack && For(s.attack, out var p))
            {
                Envelope(s, out float w, out float f);
                tp = p.windPitch * w + p.followPitch * f;
                ty = p.windYaw * w + p.followYaw * f;
                tr = p.followRoll * f;
                td = p.drop * f;
                if (p.reach) { tw = f; hand = p.hand; aim = p.aim.normalized; }
            }
            reachW = Mathf.SmoothDamp(reachW, tw, ref reachV, SmoothTime, Mathf.Infinity, dt);
            float tg = Guard && (s.action == AnimAction.None) ? Mathf.Clamp01(rest) : 0f;
            guardW = Mathf.SmoothDamp(guardW, tg, ref guardV, GuardSmooth, Mathf.Infinity, dt);
            carry = Mathf.SmoothDamp(carry, Mathf.Clamp01(running), ref carryV, GuardSmooth, Mathf.Infinity, dt);
            td = Mathf.Max(td, GuardDrop * guardW * (1f - carry));
            tp += GuardLean * guardW * Mathf.Clamp01(still); // klip spoczynku garbi plecy (chód ma je proste)
            pitch = Mathf.SmoothDamp(pitch, tp, ref pitchV, SmoothTime, Mathf.Infinity, dt);
            yaw = Mathf.SmoothDamp(yaw, ty, ref yawV, SmoothTime, Mathf.Infinity, dt);
            roll = Mathf.SmoothDamp(roll, tr, ref rollV, SmoothTime, Mathf.Infinity, dt);
            drop = Mathf.SmoothDamp(drop, td, ref dropV, SmoothTime * 1.5f, Mathf.Infinity, dt);
            if (Mathf.Abs(pitch) + Mathf.Abs(yaw) + Mathf.Abs(roll) < 0.05f && drop < 0.002f && reachW < 0.002f && guardW < 0.002f) return;

            Vector3 right = character.right, up = character.up, fwd = character.forward;
            if (drop > 0.002f && upperL != null && footL != null && upperR != null && footR != null)
            {
                // Stopy z klipu zostają, biodra schodzą niżej – kolana uginają się (przysiad).
                Vector3 ankleL = footL.position, ankleR = footR.position;
                hips.position -= up * drop;
                ClipAnimationDriver.Reach(upperL, lowerL, footL, ankleL, fwd, 1f, KneeHint(-1f));
                ClipAnimationDriver.Reach(upperR, lowerR, footR, ankleR, fwd, 1f, KneeHint(1f));
            }
            float plant = guardW * Mathf.Clamp01(still);
            if (plant > 0.002f)
            {
                PlaceFeet(plant);
                PlantFoot(footL, soleL, plant);
                PlantFoot(footR, soleR, plant);
                LevelFeet(plant);
            }
            float pt = Mathf.Clamp(pitch, -MaxPitch, MaxPitch), yw = Mathf.Clamp(yaw, -MaxYaw, MaxYaw);
            for (int i = 0; i < spine.Length; i++)
            {
                if (spine[i] == null) continue;
                float k = share[i];
                var q = Quaternion.AngleAxis(yw * k, up) * Quaternion.AngleAxis(pt * k, right) * Quaternion.AngleAxis(roll * k, fwd);
                spine[i].rotation = q * spine[i].rotation;
            }
            if (plant > 0.002f) Upright(plant);
            if (head != null && guardW > 0.002f) head.rotation = Quaternion.AngleAxis(GuardHeadLift * guardW * Mathf.Clamp01(still), right) * head.rotation;
            if (guardW > 0.002f && armR != null && foreR != null && handR != null)
            {
                float sf = GuardShoulderForward * guardW;
                if (clavL != null) clavL.rotation = Quaternion.AngleAxis(sf, up) * clavL.rotation;      // lewy bark w przód
                if (clavR != null) clavR.rotation = Quaternion.AngleAxis(-sf * 0.6f, up) * clavR.rotation; // prawy lekko w przód
                Vector3 local = Vector3.Lerp(GuardHand, CarryHand, carry);
                Vector3 dir = Vector3.Slerp(GuardAim.normalized, CarryAim.normalized, carry);
                Vector3 target = Vector3.Lerp(handR.position, character.TransformPoint(local), guardW);
                // łokieć w dół i lekko do tyłu (ręka opuszczona wzdłuż boku) – także w biegu
                Vector3 pole = character.TransformDirection(new Vector3(0.35f, -0.75f, -0.55f));
                HellEquipmentPose.Solve(armR, foreR, handR, target, pole, 1f);
                if (Weapon != null)
                {
                    Vector3 axis = Weapon.up;
                    handR.rotation = Quaternion.FromToRotation(axis, Vector3.Slerp(axis, character.TransformDirection(dir), guardW)) * handR.rotation;
                    // Ostrze w dół (oś ostrza w gnieździe: −X): dawniej obrotem dłoni wokół trzonka – nadgarstek skręcał się o ~120°
                    // i siatka dłoni zapadała się w płaski kształt („dłonie źle zaciśnięte”). Teraz dłoń bierze ułożenie najbliższe
                    // naturalnemu, a wokół trzonka obraca się broń (GuardSpin, stosuje CharacterVisual). W biegu – ten sam chwyt.
                    float bw = guardW;
                    GuardGrip = bw;
                    GuardSpin = 0f;
                    if (bw > 0.002f)
                    {
                        HellEquipmentPose.NaturalWrist(foreR, handR, restR, Weapon.up, bw);
                        Vector3 shaft = Weapon.up, blade = Vector3.ProjectOnPlane(-Weapon.right, shaft), want = Vector3.ProjectOnPlane(character.TransformDirection(GuardBlade), shaft);
                        if (blade.sqrMagnitude > 1e-6f && want.sqrMagnitude > 1e-6f)
                            GuardSpin = Vector3.SignedAngle(blade, want, shaft) * bw;
                    }
                }
            }
            else { GuardGrip = 0f; GuardSpin = 0f; }
            if (reachW > 0.002f && armR != null && foreR != null && handR != null)
            {
                // Lewa dłoń wraca na trzonek w HellEquipmentPose (IK do gniazda broni).
                Vector3 target = Vector3.Lerp(handR.position, character.TransformPoint(hand), reachW);
                HellEquipmentPose.Solve(armR, foreR, handR, target, right - up, 1f);
                if (Weapon == null) return;
                Vector3 axis = Weapon.up;
                Vector3 desired = Vector3.Slerp(axis, character.TransformDirection(aim), reachW);
                handR.rotation = Quaternion.FromToRotation(axis, desired) * handR.rotation;
                // Ostrze dotyka ziemi, ale nie wchodzi w nią – dłoń idzie w górę o tyle, ile broń jest pod podłogą
                // (Solve zachowuje obrót dłoni, więc kierunek broni zostaje).
                float under = character.position.y + GroundClearance - LowestWeaponPoint();
                if (under > 0f) HellEquipmentPose.Solve(armR, foreR, handR, handR.position + up * under, right - up, 1f);
            }
        }

        /// <summary>
        /// Postawa topora w staniu: klip spoczynku po retargecie przechyla tułów w bok (u berserkera ~11°, prawy bark niżej).
        /// Kręgosłup prostuje się o zmierzony przechył (szyja nad biodrami w płaszczyźnie czołowej), rozłożony na kręgi;
        /// skłon w przód/tył zostaje (GuardLean). w – udział postawy w staniu.
        /// </summary>
        void Upright(float w)
        {
            if (head == null) return;
            Vector3 up = character.up, fwd = character.forward, right = character.right;
            // dwa przejścia: kręgi obracają się wokół różnych punktów, drugie dociąga szyję nad biodra
            for (int pass = 0; pass < 2; pass++)
            {
                Vector3 s = head.position - hips.position;
                float side = Mathf.Atan2(Vector3.Dot(s, right), Vector3.Dot(s, up)) * Mathf.Rad2Deg;   // + = szyja na prawo od bioder
                if (Mathf.Abs(side) < 0.5f) return;
                for (int i = 0; i < spine.Length; i++)
                {
                    if (spine[i] == null) continue;
                    // obrót wokół osi przód–tył: dodatni kąt wokół +fwd przechyla górę w lewo (−right)
                    spine[i].rotation = Quaternion.AngleAxis(side * share[i] * w, fwd) * spine[i].rotation;
                }
            }
        }

        /// <summary>Najniższy punkt modelu broni (m, świat).</summary>
        float LowestWeaponPoint()
        {
            Weapon.GetComponentsInChildren(renderers);
            float y = float.MaxValue;
            foreach (var r in renderers) if (r.enabled) y = Mathf.Min(y, r.bounds.min.y);
            return renderers.Count > 0 ? y : float.MaxValue;
        }

        static float Smooth(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }
    }
}
