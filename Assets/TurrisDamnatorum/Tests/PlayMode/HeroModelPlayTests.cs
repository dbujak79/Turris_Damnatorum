#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Bohaterowie z Hell Unleashed (modele FBX, zestawy animacji broni, modele broni w dłoniach).</summary>
    public class HeroModelPlayTests
    {
        const string Visuals = "Assets/TurrisDamnatorum/Content/Visuals/";
        const string Weapons = "Assets/TurrisDamnatorum/Models/Weapons/";
        readonly List<Object> cleanup = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
        }

        static readonly string[] Heroes =
        {
            "paladin", "hero_warrior", "hero_warrior_berserker", "hero_warrior_dual",
            "mage_witch", "hero_mage_fire", "hero_mage_water", "hero_mage_air", "hero_mage_earth", "hero_mage_summoner", "hero_mage_battle",
            "rogue_hooded_meshy", "hero_rogue_archer", "hero_rogue_assassin", "hero_rogue_arcane_archer",
        };

        static CharacterVisualDefinition Def(string hero) => AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Visuals + "visual_" + hero + ".asset");

        ItemDefinition Item(string model, ItemKind kind = ItemKind.Weapon, float scale = 1f, float grip = 0f)
        {
            var it = ScriptableObject.CreateInstance<ItemDefinition>();
            it.id = model;
            it.kind = kind;
            it.model = AssetDatabase.LoadAssetAtPath<GameObject>(Weapons + model + "/" + model + ".fbx");
            it.modelScale = scale;
            it.modelGripShift = grip;
            cleanup.Add(it);
            return it;
        }

        (CharacterVisual, StubAnimSource) Hero(string hero, RigLook look, Vector3 pos, float yaw = 180f)
        {
            var go = new GameObject("Hero_" + hero);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            var v = go.AddComponent<CharacterVisual>();
            var src = new StubAnimSource();
            v.BuildModel(Def(hero), 1f, look);
            v.Init(src);
            cleanup.Add(go);
            return (v, src);
        }

        RigLook Sword() => new RigLook { weapon = WeaponModel.Sword, weaponItem = Item("knight_sword"), shield = ShieldModel.Heater, offhandItem = Item("knight_shield", ItemKind.Shield) };
        RigLook Greatsword() => new RigLook { weapon = WeaponModel.GreatSword, weaponItem = Item("knight_sword", scale: 1.3f) };
        RigLook Greataxe() => new RigLook { weapon = WeaponModel.GreatAxe, weaponItem = Item("greataxe") };
        RigLook Hammer() => new RigLook { weapon = WeaponModel.Hammer, weaponItem = Item("warhammer", scale: 0.8f, grip: 0.24f) };
        RigLook Bow() => new RigLook { weapon = WeaponModel.Bow, weaponItem = Item("rogue_bow_rigged") };
        RigLook Staff(string e) => new RigLook { weapon = WeaponModel.Staff, weaponItem = Item("mage_staff_" + e), offhandItem = Item("mage_grimoire", ItemKind.Shield) };
        RigLook Daggers() => new RigLook { weapon = WeaponModel.Dagger, weaponItem = Item("rogue_dagger"), offhandItem = Item("rogue_dagger") };

        static CharacterAnimState Attack(AttackAnim a, ActionPhase phase, float p) =>
            new CharacterAnimState { action = AnimAction.Attack, attack = a, phase = phase, phaseProgress = p };

        [UnityTest]
        public IEnumerator AllHeroes_Build_RightHeight_WeaponInHand()
        {
            foreach (var h in Heroes)
            {
                var def = Def(h);
                Assert.IsNotNull(def, "brak definicji wyglądu " + h);
                Assert.IsNotNull(def.modelPrefab, h);
                Assert.IsNotNull(def.idle, h + ": brak spoczynku");
                var (v, src) = Hero(h, Sword(), Vector3.zero);
                for (int i = 0; i < 30; i++) v.Tick(1f / 60f);
                var anim = v.Model.GetComponentInChildren<Animator>();
                float foot = Mathf.Min(anim.GetBoneTransform(HumanBodyBones.LeftFoot).position.y, anim.GetBoneTransform(HumanBodyBones.RightFoot).position.y);
                Assert.That(foot, Is.InRange(0.0f, 0.25f), h + ": stopy na ziemi");
                var head = anim.GetBoneTransform(HumanBodyBones.Head).position.y;
                Assert.That(head, Is.InRange(1.2f, 1.85f), h + ": głowa na wysokości " + head);
                var hand = v.RightHand;
                Assert.IsNotNull(hand, h + ": brak prawej dłoni (Humanoid)");
                Assert.Greater(Vector3.Distance(v.WeaponTip, hand.position), 0.4f, h + ": czubek broni przy dłoni");
                Assert.IsNotNull(v.ModelSet, h + ": brak zestawu animacji miecza");
                Object.Destroy(v.gameObject);
                yield return null;
            }
        }

        /// <summary>
        /// Bieg z prędkością gry: ciało zostaje nad postacią (klipy Mixamo idą do przodu – ruch korzenia ma być odrzucony,
        /// inaczej ciało odjeżdża i co pętlę wraca skokiem), a nogi kroczą także u maga (bez zestawu broni).
        /// </summary>
        [UnityTest]
        public IEnumerator Run_BodyStaysOverCharacter_LegsStride()
        {
            var cases = new (string hero, RigLook look)[]
            {
                ("hero_warrior", Sword()), ("hero_warrior_berserker", Greataxe()), ("hero_rogue_archer", Bow()),
                ("mage_witch", Staff("fire")), ("hero_mage_fire", Staff("fire")),
            };
            var report = new System.Text.StringBuilder();
            bool ok = true;
            foreach (var (h, look) in cases)
            {
                var (v, src) = Hero(h, look, Vector3.zero, yaw: 0f);
                var anim = v.Model.GetComponentInChildren<Animator>();
                var hips = anim.GetBoneTransform(HumanBodyBones.Hips);
                var foot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                const float dt = 1f / 60f, speed = 5.2f;
                float maxDrift = 0f, maxJump = 0f, footMin = float.MaxValue, footMax = float.MinValue;
                Vector3 prev = Vector3.zero;
                for (int i = 0; i < 180; i++)
                {
                    v.transform.position += Vector3.forward * speed * dt;
                    v.Tick(dt);
                    Vector3 local = v.transform.InverseTransformPoint(hips.position); local.y = 0f;
                    if (i >= 40) // po rozpędzeniu
                    {
                        maxDrift = Mathf.Max(maxDrift, local.magnitude);
                        maxJump = Mathf.Max(maxJump, (local - prev).magnitude);
                        float fz = v.transform.InverseTransformPoint(foot.position).z;
                        footMin = Mathf.Min(footMin, fz); footMax = Mathf.Max(footMax, fz);
                    }
                    prev = local;
                }
                bool good = maxDrift < 0.3f && maxJump < 0.06f && footMax - footMin > 0.35f;
                ok &= good;
                report.AppendLine($"{h}: odjazd bioder {maxDrift:F2} m, skok {maxJump:F3} m/kl., zakres stopy {footMax - footMin:F2} m{(good ? "" : "  <-- ŹLE")}");
                Object.Destroy(v.gameObject);
                yield return null;
            }
            Debug.Log(report.ToString());
            Assert.IsTrue(ok, report.ToString());
        }

        /// <summary>Seria ciosów w czasach gry (miecz: zamach 0,29 s, cięcie 0,13 s, powrót 0,31 s; kolejny cios w 40% powrotu).</summary>
        static IEnumerable<CharacterAnimState> Combo(AttackAnim[] attacks, float windup = 0.29f, float active = 0.127f, float recovery = 0.305f, float chainAt = 0.4f)
        {
            const float dt = 1f / 60f;
            float[] dur = { windup, active, recovery };
            for (int a = 0; a < attacks.Length; a++)
                for (int ph = 0; ph < 3; ph++)
                {
                    float cut = ph == 2 && a < attacks.Length - 1 ? chainAt : 1f;
                    for (float t = dt * 0.5f; t < dur[ph] * cut; t += dt)
                        yield return Attack(attacks[a], (ActionPhase)ph, t / dur[ph]);
                }
        }

        /// <summary>Prędkości dłoni (m na klatkę) w czasie zamachu ciosu <paramref name="second"/> – po <paramref name="first"/> (seria) albo z gardy.</summary>
        List<float> WindupHandSteps(AttackAnim? first, AttackAnim second, Vector3 pos)
        {
            var (v, src) = Hero("paladin", Sword(), pos, yaw: 0f);
            var hand = v.Model.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
            const float dt = 1f / 60f;
            src.state = new CharacterAnimState();
            for (int i = 0; i < 20; i++) v.Tick(dt);
            var steps = new List<float>();
            var states = new List<CharacterAnimState>(Combo(first.HasValue ? new[] { first.Value, second } : new[] { second }));
            Vector3 prev = v.transform.InverseTransformPoint(hand.position);
            bool inSecond = !first.HasValue;
            for (int i = 0; i < states.Count; i++)
            {
                if (i > 0 && states[i].phase == ActionPhase.Startup && states[i - 1].phase == ActionPhase.Recovery) inSecond = true;
                src.state = states[i];
                v.Tick(dt);
                Vector3 h = v.transform.InverseTransformPoint(hand.position);
                if (inSecond && states[i].phase == ActionPhase.Startup) steps.Add((h - prev).magnitude);
                prev = h;
            }
            Object.Destroy(v.gameObject);
            return steps;
        }

        /// <summary>
        /// Przejścia: kolejny cios serii (także ten sam klip od nowa) przenika się z poprzednią pozą. Ruch dłoni w zamachu
        /// jest gładki (bez pojedynczego skoku) i niewiele szybszy niż w tym samym ciosie zaczętym z gardy.
        /// Twarde przełączenie dałoby skok kilkunastu centymetrów w jednej klatce.
        /// </summary>
        [UnityTest]
        public IEnumerator Transitions_Crossfade_NoPoseJump()
        {
            var report = new System.Text.StringBuilder();
            bool ok = true;
            foreach (var (first, second) in new[] { (AttackAnim.SlashRight, AttackAnim.SlashLeft), (AttackAnim.SlashLeft, AttackAnim.SlashLeft), (AttackAnim.SlashLeft, AttackAnim.Thrust) })
            {
                var chain = WindupHandSteps(first, second, Vector3.zero);
                var fresh = WindupHandSteps(null, second, new Vector3(3, 0, 0));
                yield return null;
                float peakChain = chain.Max(), peakFresh = fresh.Max();
                // Skok: klatka wyraźnie szybsza od średniej sąsiednich (gładki szczyt przejścia daje ~0,05 m, przeskok – ponad 0,1 m).
                float spike = 0f;
                for (int i = 1; i < chain.Count - 1; i++) spike = Mathf.Max(spike, chain[i] - (chain[i - 1] + chain[i + 1]) * 0.5f);
                // Przejście z odległej pozy może być szybsze niż zamach z gardy, ale nie szybsze od samego cięcia (~25 m/s).
                bool good = peakChain * 60f < Mathf.Max(22f, peakFresh * 60f * 1.3f) && spike < 0.08f;
                ok &= good;
                report.AppendLine($"{first} → {second}: szczyt {peakChain * 60f:F1} m/s (z gardy {peakFresh * 60f:F1}), skok {spike:F3} m{(good ? "" : "  <-- ŹLE")}");
            }
            Debug.Log(report.ToString());
            Assert.IsTrue(ok, report.ToString());

            // Drgnięcie i powrót do ruchu po ciosie – też bez skoku.
            var (v, src) = Hero("paladin", Sword(), Vector3.zero, yaw: 0f);
            var hand = v.Model.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
            const float dt = 1f / 60f;
            var states = new List<CharacterAnimState>(Combo(new[] { AttackAnim.SlashRight }, chainAt: 1f));
            int hitAt = states.Count - 10;
            states.RemoveRange(hitAt, 10);
            for (int i = 0; i < 8; i++) states.Add(new CharacterAnimState { action = AnimAction.Flinch, actionTime = i * dt, actionDuration = 0.4f });
            for (int i = 0; i < 30; i++) states.Add(new CharacterAnimState());
            Vector3 prev = Vector3.zero;
            var after = new List<float>();
            for (int i = 0; i < states.Count; i++)
            {
                src.state = states[i];
                v.Tick(dt);
                Vector3 h = v.transform.InverseTransformPoint(hand.position);
                if (i >= hitAt) after.Add((h - prev).magnitude);
                prev = h;
            }
            float flinchSpike = 0f;
            for (int i = 1; i < after.Count - 1; i++) flinchSpike = Mathf.Max(flinchSpike, after[i] - (after[i - 1] + after[i + 1]) * 0.5f);
            // Trafienie szarpie ręką szybko (wolniej niż cięcie), ale gładko – dawniej 0,53 m w jednej klatce.
            Assert.Less(after.Max() * 60f, 22f, "Drgnięcie po ciosie: dłoń nie szybsza od cięcia");
            Assert.Less(flinchSpike, 0.08f, "Drgnięcie i powrót do gardy bez skoku dłoni");

            // Odniesienie: twarde przełączenie – koniec pierwszego ciosu i pierwsza klatka drugiego są daleko od siebie.
            var (cut, cs) = Hero("paladin", Sword(), new Vector3(3, 0, 0), yaw: 0f);
            var cutHand = cut.Model.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
            cs.state = Attack(AttackAnim.SlashRight, ActionPhase.Recovery, 0.4f);
            for (int i = 0; i < 30; i++) cut.Tick(dt);
            Vector3 endFirst = cut.transform.InverseTransformPoint(cutHand.position);
            cs.state = Attack(AttackAnim.SlashLeft, ActionPhase.Startup, 0.02f);
            for (int i = 0; i < 30; i++) cut.Tick(dt);
            Vector3 startSecond = cut.transform.InverseTransformPoint(cutHand.position);
            Assert.Greater((startSecond - endFirst).magnitude, 0.12f, "Odniesienie: bez przenikania dłoń przeskoczyłaby wyraźnie");
            yield return null;
        }

        /// <summary>
        /// Tempo ciosu jak u człowieka: klip Mixamo jest dłuższy niż cios w grze, ale czas jest rozłożony tak, by cięcie
        /// nie było kilkukrotnie szybsze od nagrania, a powrót do gardy – wolniejszy od cięcia.
        /// </summary>
        [UnityTest]
        public IEnumerator Strike_HandSpeed_HumanlikeProfile()
        {
            var report = new System.Text.StringBuilder();
            bool ok = true;
            foreach (var a in new[] { AttackAnim.SlashRight, AttackAnim.SlashLeft, AttackAnim.Overhead })
            {
                var (v, src) = Hero("paladin", Sword(), Vector3.zero, yaw: 0f);
                var hand = v.Model.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
                const float dt = 1f / 60f;
                src.state = new CharacterAnimState();
                for (int i = 0; i < 20; i++) v.Tick(dt);
                Vector3 prev = v.transform.InverseTransformPoint(hand.position);
                float strike = 0f, back = 0f;
                foreach (var st in Combo(new[] { a }))
                {
                    src.state = st;
                    v.Tick(dt);
                    Vector3 h = v.transform.InverseTransformPoint(hand.position);
                    float speed = (h - prev).magnitude / dt;
                    if (st.phase == ActionPhase.Recovery) back = Mathf.Max(back, speed); else strike = Mathf.Max(strike, speed);
                    prev = h;
                }
                bool good = strike < 30f && back < strike * 0.7f;
                ok &= good;
                report.AppendLine($"{a}: szczyt cięcia {strike:F1} m/s, szczyt powrotu {back:F1} m/s{(good ? "" : "  <-- ŹLE")}");
                Object.Destroy(v.gameObject);
                yield return null;
            }
            Debug.Log(report.ToString());
            Assert.IsTrue(ok, report.ToString());
        }

        /// <summary>
        /// Bieg bokiem i w tył (namierzony cel) używa klipów biegu w tym kierunku. Po skosie – jeden klip (przód) i nogi
        /// obrócone w stronę ruchu: mieszanina biegu w przód i bokiem krzyżowała kolana (uwaga użytkownika: „krzywo nogę”).
        /// </summary>
        [UnityTest]
        public IEnumerator Locomotion_DirectionalRunClips()
        {
            var (v, src) = Hero("paladin", Sword(), Vector3.zero, yaw: 0f);
            var d = (ClipAnimationDriver)v.Driver;
            const float dt = 1f / 60f;
            void Move(Vector3 dir)
            {
                for (int i = 0; i < 60; i++) { v.transform.position += dir.normalized * 5.2f * dt; v.Tick(dt); }
            }
            Move(Vector3.right);
            Assert.Greater(d.LocoWeight(8), 0.7f, "Bieg w prawo – klip biegu bokiem");
            Move(Vector3.back);
            Assert.Greater(d.LocoWeight(6), 0.7f, "Bieg w tył – klip biegu w tył");
            Move(new Vector3(-1f, 0f, 1f));
            Assert.Greater(d.LocoWeight(5), 0.9f, "Po skosie: sam bieg w przód…");
            Assert.Less(d.LocoWeight(7), 0.05f, "…bez domieszki biegu bokiem");
            Assert.AreEqual(-45f, d.LegTwist, 3f, "…a nogi obrócone w stronę ruchu");
            Move(Vector3.forward);
            Assert.AreEqual(0f, d.LegTwist, 1f, "Prosto – bez obrotu nóg");
            yield return null;
        }

        /// <summary>Trafiony bohater FBX: tułów i głowa ustępują ciosowi (kręgosłup), po chwili wracają; nogi zostają z klipu.</summary>
        [UnityTest]
        public IEnumerator HeroBody_GivesWayToBlow_ThenRecovers()
        {
            var (hit, hs) = Hero("hero_warrior", Greatsword(), Vector3.zero, yaw: 0f);
            var (calm, cs) = Hero("hero_warrior", Greatsword(), new Vector3(3, 0, 0), yaw: 0f);
            Transform BoneOf(CharacterVisual v, HumanBodyBones b) => v.Model.GetComponentInChildren<Animator>().GetBoneTransform(b);
            Vector3 Local(CharacterVisual v, HumanBodyBones b) => v.transform.InverseTransformPoint(BoneOf(v, b).position);
            const float dt = 1f / 60f;
            for (int i = 0; i < 20; i++) { hit.Tick(dt); calm.Tick(dt); }
            hit.Impact(Vector3.back, 1f); // cios z przodu – pchnięcie do tyłu
            float maxBack = 0f, footShift = 0f;
            for (int i = 0; i < 15; i++)
            {
                hit.Tick(dt); calm.Tick(dt);
                maxBack = Mathf.Max(maxBack, Local(calm, HumanBodyBones.Head).z - Local(hit, HumanBodyBones.Head).z);
                footShift = Mathf.Max(footShift, (Local(hit, HumanBodyBones.LeftFoot) - Local(calm, HumanBodyBones.LeftFoot)).magnitude);
            }
            Assert.Greater(maxBack, 0.08f, "Głowa odchyla się do tyłu od ciosu");
            Assert.Less(footShift, 0.01f, "Stopy zostają na miejscu");
            for (int i = 0; i < 90; i++) { hit.Tick(dt); calm.Tick(dt); }
            Assert.Less((Local(hit, HumanBodyBones.Head) - Local(calm, HumanBodyBones.Head)).magnitude, 0.01f, "Po chwili postać wraca do pozy z klipu");
            yield return null;
        }

        /// <summary>
        /// Tarcza przypięta do przedramienia: przedramię leży w płaszczyźnie tarczy, a gdy jest równoległe do podłoża,
        /// tarcza stoi w pionie (niezależnie od zgięcia nadgarstka w klipie). Sprawdzane w spoczynku, chodzie, biegu, bloku i ciosach.
        /// </summary>
        [UnityTest]
        public IEnumerator Shield_StrappedToForearm_UprightWhenForearmLevel()
        {
            var (v, src) = Hero("paladin", Sword(), Vector3.zero, yaw: 0f);
            var anim = v.Model.GetComponentInChildren<Animator>();
            var hand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
            var elbow = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            var grip = Util.FindDeep(v.Model.transform, "ShieldGrip");
            Assert.IsNotNull(grip, "Tarcza w węźle chwytu na przedramieniu");
            const float dt = 1f / 60f;
            var states = new List<(CharacterAnimState s, Vector3 vel)>();
            for (int i = 0; i < 60; i++) states.Add((new CharacterAnimState(), Vector3.zero));
            for (int i = 0; i < 60; i++) states.Add((new CharacterAnimState(), Vector3.forward * 1.4f));
            for (int i = 0; i < 60; i++) states.Add((new CharacterAnimState(), Vector3.forward * 5.2f));
            for (int i = 0; i < 60; i++) states.Add((new CharacterAnimState { action = AnimAction.Block, actionTime = i * dt }, Vector3.zero));
            foreach (var st in Combo(new[] { AttackAnim.SlashRight, AttackAnim.SlashLeft, AttackAnim.Thrust })) states.Add((st, Vector3.zero));
            int level = 0;
            float worstTilt = 0f, worstPlane = 0f;
            foreach (var (s, vel) in states)
            {
                src.state = s;
                v.transform.position += vel * dt;
                v.Tick(dt);
                Vector3 f = (hand.position - elbow.position).normalized;
                worstPlane = Mathf.Max(worstPlane, Mathf.Abs(Vector3.Dot(grip.up, f))); // +Y tarczy – lico
                if (Mathf.Abs(Vector3.Dot(f, Vector3.up)) < 0.1f)
                {
                    level++;
                    worstTilt = Mathf.Max(worstTilt, Vector3.Angle(grip.forward, Vector3.up)); // +Z tarczy – góra
                }
            }
            yield return null;
            Assert.Greater(level, 5, "W sekwencji są klatki z poziomym przedramieniem");
            Assert.Less(worstTilt, 7f, "Przedramię poziomo – tarcza w pionie");
            Assert.Less(worstPlane, 0.02f, "Przedramię leży na tarczy (pasy)");
        }

        /// <summary>
        /// Blok w miejscu to klip górnej połowy ciała: stopy zostają tam, gdzie stawia je spoczynek, a ręka z tarczą
        /// unosi się do bloku.
        /// </summary>
        [UnityTest]
        public IEnumerator Block_UpperBody_FeetStayWhereLocomotionPutsThem()
        {
            var report = new System.Text.StringBuilder();
            bool ok = true;
            foreach (var (hero, look) in new[] { ("paladin", Sword()), ("hero_warrior", Greatsword()), ("hero_rogue_archer", Bow()) })
            {
                var (v, src) = Hero(hero, look, Vector3.zero, yaw: 0f);
                var anim = v.Model.GetComponentInChildren<Animator>();
                Vector3 Local(HumanBodyBones b) => v.transform.InverseTransformPoint(anim.GetBoneTransform(b).position);
                const float dt = 1f / 60f;
                src.state = new CharacterAnimState();
                for (int i = 0; i < 40; i++) v.Tick(dt);
                Vector3 l0 = Local(HumanBodyBones.LeftFoot), r0 = Local(HumanBodyBones.RightFoot), hand0 = Local(HumanBodyBones.LeftHand);
                float worst = 0f;
                for (int i = 0; i < 40; i++)
                {
                    src.state = new CharacterAnimState { action = AnimAction.Block, actionTime = i * dt };
                    v.Tick(dt);
                    if (i >= 20) worst = Mathf.Max(worst, (Local(HumanBodyBones.LeftFoot) - l0).magnitude, (Local(HumanBodyBones.RightFoot) - r0).magnitude);
                }
                float raise = (Local(HumanBodyBones.LeftHand) - hand0).magnitude;
                bool good = worst < 0.04f && raise > 0.1f;
                ok &= good;
                report.AppendLine($"{hero}: przesunięcie stopy {worst:F3} m, ruch ręki do bloku {raise:F2} m{(good ? "" : "  <-- ŹLE")}");
                Object.Destroy(v.gameObject);
                yield return null;
            }
            Debug.Log(report.ToString());
            Assert.IsTrue(ok, report.ToString());
        }

        /// <summary>
        /// Cofanie w bloku (55% prędkości ≈ 2,9 m/s): krótkie kroki chodu w tył, a nie wykrok biegu w tył
        /// (stopa lądowała ponad pół metra za biodrami).
        /// </summary>
        /// <summary>
        /// Cios mieczem to mały krok naprzód: bohater przesuwa się tyle, ile ciało w nagraniu, a żadna stopa nie cofa się
        /// w świecie (dawniej klip grał w miejscu i stopy jechały względem ciała do tyłu, a drugie cięcie cofało wykroczną nogę).
        /// Cały cios – zapowiedź, cięcie, powrót i przejście do spoczynku.
        /// </summary>
        [UnityTest]
        public IEnumerator SwordAttack_StepsForward_NoFootRetreats()
        {
            var cases = new (RigLook look, AttackAnim a, bool step)[]
            {
                (Sword(), AttackAnim.SlashRight, true), (Sword(), AttackAnim.SlashLeft, true), (Sword(), AttackAnim.Overhead, false), (Sword(), AttackAnim.Thrust, false),
                (Greatsword(), AttackAnim.SlashRight, true), (Greatsword(), AttackAnim.SlashLeft, true), (Greatsword(), AttackAnim.Thrust, false),
            };
            var report = new System.Text.StringBuilder();
            bool ok = true;
            foreach (var (look, a, step) in cases)
            {
                var (v, src) = Hero("hero_warrior", look, Vector3.zero, yaw: 0f);
                var drv = (ClipAnimationDriver)v.Driver;
                Assert.IsTrue(drv.StepsFromClips, "zestaw broni ma kroki z nagrań");
                var anim = v.Model.GetComponentInChildren<Animator>();
                var feet = new[] { anim.GetBoneTransform(HumanBodyBones.LeftFoot), anim.GetBoneTransform(HumanBodyBones.RightFoot) };
                const float dt = 1f / 60f;
                for (int i = 0; i < 30; i++) v.Tick(dt);
                drv.ConsumeAdvance();
                var best = feet.Select(f => f.position.z).ToArray();
                var retreat = new float[2];
                float start = v.transform.position.z;
                void Step(CharacterAnimState st)
                {
                    src.state = st;
                    v.Tick(dt);
                    v.transform.position += v.transform.forward * drv.ConsumeAdvance();
                    for (int k = 0; k < 2; k++)
                    {
                        float z = feet[k].position.z;
                        best[k] = Mathf.Max(best[k], z);
                        retreat[k] = Mathf.Max(retreat[k], best[k] - z);
                    }
                }
                void Blow()
                {
                    foreach (var (phase, time) in new[] { (ActionPhase.Startup, 0.35f), (ActionPhase.Active, 0.15f), (ActionPhase.Recovery, 0.45f) })
                    {
                        int n = Mathf.RoundToInt(time / dt);
                        for (int i = 1; i <= n; i++) Step(Attack(a, phase, i / (float)n));
                    }
                    for (int i = 0; i < 30; i++) Step(new CharacterAnimState());
                }
                Blow(); // ze spoczynku: wykrok w postawę ciosu
                float first = v.transform.position.z - start;
                float mid = v.transform.position.z;
                Blow(); // w postawie bojowej – kolejny cios
                float second = v.transform.position.z - mid;
                // Po chwili spokoju – powrót do swobodnego stania (dostawienie nogi, nie cofnięcie).
                for (int i = 0; i < Mathf.RoundToInt((ClipAnimationDriver.CombatStanceTime + 1.2f) / dt); i++) Step(new CharacterAnimState());
                report.AppendLine($"{look.weapon} {a}: krok ze spoczynku {first:0.00} m, w walce {second:0.00} m, cofnięcie stóp L {retreat[0]:0.00} P {retreat[1]:0.00}");
                if (retreat[0] > 0.15f || retreat[1] > 0.15f) ok = false;
                if (step && second < 0.25f) ok = false;
                // Drugie cięcie dwuręczne („slash (4)”) to długi wykrok z nagrania (~1,3 m) – reszta ciosów w walce do ~0,7 m.
                if (second > (look.weapon == WeaponModel.GreatSword && a == AttackAnim.SlashLeft ? 1.45f : 0.8f) || first < -0.001f || second < -0.001f) ok = false;
                Object.Destroy(v.gameObject);
                yield return null;
            }
            Debug.Log(report.ToString());
            Assert.IsTrue(ok, report.ToString());
        }

        [UnityTest]
        public IEnumerator Block_Backpedal_ShortSteps()
        {
            var (v, src) = Hero("paladin", Sword(), Vector3.zero, yaw: 0f);
            var anim = v.Model.GetComponentInChildren<Animator>();
            var d = (ClipAnimationDriver)v.Driver;
            const float dt = 1f / 60f;
            float behind = 0f;
            for (int i = 0; i < 90; i++)
            {
                src.state = new CharacterAnimState { action = AnimAction.Block, actionTime = i * dt };
                v.transform.position += Vector3.back * 2.86f * dt;
                v.Tick(dt);
                if (i < 30) continue;
                float hips = v.transform.InverseTransformPoint(anim.GetBoneTransform(HumanBodyBones.Hips).position).z;
                foreach (var b in new[] { HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot })
                    behind = Mathf.Max(behind, hips - v.transform.InverseTransformPoint(anim.GetBoneTransform(b).position).z);
            }
            yield return null;
            Assert.Less(d.LocoWeight(6), 0.01f, "W gardzie bez klipu biegu w tył");
            Assert.Greater(d.LocoWeight(2), 0.75f, "Chód w tył (z domieszką postawy – krótsze kroki)");
            // Postawa bojowa Idle_Combat (jak w HU) jest szersza od dawnego swobodnego stania – stąd 0,45 m (przed poprawką cofania: 0,53 m).
            Assert.Less(behind, 0.45f, $"Stopa za biodrami {behind:F2} m");
        }

        [Test]
        public void WeaponModels_BladeAlongPlusY()
        {
            var report = new System.Text.StringBuilder();
            bool ok = true;
            foreach (var w in new[] { "knight_sword", "greataxe", "warhammer", "mage_staff_fire", "rogue_dagger", "rogue_bow_rigged", "knight_shield", "mage_grimoire", "flail" })
            {
                var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Weapons + w + "/" + w + ".fbx"));
                cleanup.Add(go);
                var b = new Bounds(Vector3.zero, Vector3.zero);
                foreach (var r in go.GetComponentsInChildren<Renderer>())
                {
                    var mb = r is SkinnedMeshRenderer sk ? sk.sharedMesh.bounds : r.GetComponent<MeshFilter>().sharedMesh.bounds;
                    foreach (var sx in new[] { -1, 1 }) foreach (var sy in new[] { -1, 1 }) foreach (var sz in new[] { -1, 1 })
                        b.Encapsulate(go.transform.InverseTransformPoint(r.transform.TransformPoint(mb.center + Vector3.Scale(mb.extents, new Vector3(sx, sy, sz)))));
                }
                report.AppendLine($"{w}: min {b.min} max {b.max}");
                if (w != "knight_shield" && w != "mage_grimoire" && b.max.y < b.max.x) ok = false;
            }
            Assert.IsTrue(ok, report.ToString());
        }

        [UnityTest]
        public IEnumerator Staff_StandsUpright_AtRest()
        {
            var (v, src) = Hero("hero_mage_fire", Staff("fire"), Vector3.zero);
            for (int i = 0; i < 30; i++) v.Tick(1f / 60f);
            Vector3 hand = v.RightHand.position;
            Assert.Greater(v.WeaponTip.y - hand.y, 0.6f, $"kostur nie stoi pionowo: dłoń {hand}, czubek {v.WeaponTip}");
            yield return null;
            yield return null;
            var staff = v.GetComponentsInChildren<MeshRenderer>().First(r => r.name.Contains("staff") || r.transform.parent.name.Contains("staff"));
            var top = staff.transform.TransformPoint(new Vector3(0, 1.1f, 0));
            Assert.Greater(top.y - v.RightHand.position.y, 0.6f, "siatka kostura nie stoi pionowo po klatce");
        }

        /// <summary>
        /// Bieg do przodu: stopy w podporze stoją jak w pozie spoczynkowej modelu (uwaga użytkownika: lewa stopa wykręcona
        /// na zewnątrz – przeniesienie biegu Mixamo na model Meshy dawało −48…−57° zamiast −13°). W wymachu (kolano zgięte)
        /// lewa stopa jest skręcona względem zawiasu kolana tak jak lustrzana prawa (druga uwaga: pięta „uciekała” w lewo).
        /// </summary>
        [UnityTest]
        public IEnumerator Run_FeetInStancePointLikeRestPose()
        {
            var report = new System.Text.StringBuilder();
            bool ok = true;
            foreach (var h in new[] { "hero_warrior", "paladin", "mage_witch", "hero_rogue_archer" })
            {
                var (v, src) = Hero(h, Sword(), Vector3.zero, yaw: 0f);
                var anim = v.Model.GetComponentInChildren<Animator>();
                float Yaw(bool left, out bool stance)
                {
                    var f = anim.GetBoneTransform(left ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot).position;
                    var t = anim.GetBoneTransform(left ? HumanBodyBones.LeftToes : HumanBodyBones.RightToes).position;
                    var d = v.transform.InverseTransformDirection(t - f);
                    stance = new Vector2(d.x, d.z).magnitude / d.magnitude > 0.75f && f.y - v.transform.position.y < 0.17f;
                    return Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
                }
                // Skręt stopy wokół goleni względem zawiasu kolana (prawa noga w odbiciu lustrzanym); tylko przy kolanie zgiętym > 60°.
                float SwingTwist(bool left, out bool swing)
                {
                    var hip = anim.GetBoneTransform(left ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg).position;
                    var knee = anim.GetBoneTransform(left ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg).position;
                    var ankle = anim.GetBoneTransform(left ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot).position;
                    var toe = anim.GetBoneTransform(left ? HumanBodyBones.LeftToes : HumanBodyBones.RightToes).position;
                    Vector3 M(Vector3 w) { var q = v.transform.InverseTransformDirection(w); if (!left) q.x = -q.x; return q; }
                    swing = Vector3.Angle(knee - hip, ankle - knee) > 60f;
                    Vector3 shin = M(ankle - knee).normalized, hinge = Vector3.Cross(M(knee - hip).normalized, shin);
                    var fwd = Vector3.ProjectOnPlane(Vector3.Cross(shin, hinge.normalized), shin);
                    return Vector3.SignedAngle(fwd, Vector3.ProjectOnPlane(M(toe - ankle), shin), shin);
                }
                float restL = Yaw(true, out _), restR = Yaw(false, out _);
                float worstL = 0f, worstR = 0f, swingL = 0f, swingR = 0f; int nL = 0, nR = 0;
                for (int i = 0; i < 160; i++)
                {
                    v.transform.position += Vector3.forward * 5.2f / 60f;
                    v.Tick(1f / 60f);
                    if (i < 100) continue;
                    float l = Yaw(true, out bool sl), r = Yaw(false, out bool sr);
                    if (sl) worstL = Mathf.Max(worstL, Mathf.Abs(Mathf.DeltaAngle(l, restL)));
                    if (sr) worstR = Mathf.Max(worstR, Mathf.Abs(Mathf.DeltaAngle(r, restR)));
                    float tl = SwingTwist(true, out bool wl), tr = SwingTwist(false, out bool wr);
                    if (wl) { swingL += tl; nL++; }
                    if (wr) { swingR += tr; nR++; }
                }
                float swingDiff = nL > 0 && nR > 0 ? Mathf.Abs(swingL / nL - swingR / nR) : 0f;
                report.AppendLine($"{h}: odchylenie stóp w podporze od spoczynku L {worstL:0}° P {worstR:0}°, różnica skrętu w wymachu {swingDiff:0}° ({swingL / Mathf.Max(1, nL):0} / {swingR / Mathf.Max(1, nR):0})");
                // Łotrzyk: w wymachu nietypowo skręcona jest prawa stopa (~56°), lewa ma skręt jak u pozostałych – tylko raport.
                if (worstL > 15f || worstR > 15f || (h != "hero_rogue_archer" && swingDiff > 12f)) ok = false;
                Object.Destroy(v.gameObject);
                yield return null;
            }
            Debug.Log(report.ToString());
            Assert.IsTrue(ok, report.ToString());
        }

        /// <summary>Spoczynek Wojownika z mieczem i tarczą – z przodu (jak podgląd w HU) i zza pleców (kamera gry).</summary>
        [UnityTest]
        public IEnumerator WarriorIdleGallery_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR."); yield break; }
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(40, 1, 40);
            cleanup.Add(ground);
            var lightGo = new GameObject("L"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(35, 200, 0); cleanup.Add(lightGo);
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.52f);
            var camGo = new GameObject("Cam"); var cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.backgroundColor = new Color(0.12f, 0.08f, 0.08f); cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 30;
            Hero("hero_warrior", Sword(), Vector3.zero, yaw: 0f);
            Time.captureFramerate = 60;
            for (int i = 0; i < 60; i++) yield return null;
            Time.captureFramerate = 0;
            const int W = 420, H = 560;
            var sheet = new Texture2D(W * 3, H, TextureFormat.RGB24, false);
            var rt = new RenderTexture(W, H, 24);
            var views = new[] { new Vector3(0.8f, 1.2f, 3.6f), new Vector3(-2.6f, 1.2f, 2.4f), new Vector3(0f, 2.6f, -3.6f) };
            for (int k = 0; k < views.Length; k++)
            {
                cam.transform.position = views[k];
                cam.transform.LookAt(Vector3.up * 0.95f);
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                sheet.ReadPixels(new Rect(0, 0, W, H), k * W, 0);
                RenderTexture.active = null; cam.targetTexture = null;
            }
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(dir, "warrior_idle.png"), sheet.EncodeToPNG());
            Object.Destroy(rt); Object.Destroy(sheet);
        }

        /// <summary>Galeria postaw spoczynku miecza z tarczą (kandydaci na postawę bojową po ciosie).</summary>
        [UnityTest]
        public IEnumerator CombatIdleGallery_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR."); yield break; }
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(80, 1, 80);
            cleanup.Add(ground);
            var lightGo = new GameObject("L"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(40, 200, 0); cleanup.Add(lightGo);
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.52f);
            var camGo = new GameObject("Cam"); var cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.backgroundColor = new Color(0.15f, 0.15f, 0.18f); cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 30;
            string ss = "Assets/TurrisDamnatorum/Models/Animations/Mixamo/Pro Sword and Shield Pack/sword and shield ";
            var names = new[] { "idle", "idle (2)", "idle (3)", "idle (4)", "block idle" };
            var baseSet = AssetDatabase.LoadAssetAtPath<WeaponAnimSet>(Visuals + "anim_sword_shield.asset");
            const int W = 360, H = 480;
            var sheet = new Texture2D(W * names.Length, H * 2, TextureFormat.RGB24, false);
            var rt = new RenderTexture(W, H, 24);
            for (int n = 0; n < names.Length; n++)
            {
                var clip = AssetDatabase.LoadAllAssetsAtPath(ss + names[n] + ".fbx").OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__"));
                var set = Object.Instantiate(baseSet); set.idle = clip; cleanup.Add(set);
                var def = Object.Instantiate(Def("hero_warrior")); def.oneHandSet = set; cleanup.Add(def);
                var go = new GameObject("H"); cleanup.Add(go);
                var v = go.AddComponent<CharacterVisual>();
                var src = new StubAnimSource();
                v.BuildModel(def, 1f, Sword()); v.Init(src);
                for (int i = 0; i < 40; i++) yield return null;
                for (int k = 0; k < 2; k++)
                {
                    cam.transform.position = k == 0 ? new Vector3(0.6f, 1.1f, 3.6f) : new Vector3(3.6f, 1.1f, 0.3f);
                    cam.transform.LookAt(Vector3.up * 0.9f);
                    cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                    sheet.ReadPixels(new Rect(0, 0, W, H), n * W, (1 - k) * H);
                    RenderTexture.active = null; cam.targetTexture = null;
                }
                go.SetActive(false);
            }
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(dir, "hero_combat_idles.png"), sheet.EncodeToPNG());
            Object.Destroy(rt); Object.Destroy(sheet);
        }

        /// <summary>Galeria biegu Wojownika z mieczem i tarczą: klatki cyklu z boku, z przodu i z góry (kamera gry).</summary>
        [UnityTest]
        public IEnumerator RunGallery_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię biegu."); yield break; }
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(400, 1, 400);
            cleanup.Add(ground);
            var lightGo = new GameObject("L"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(40, 200, 0); cleanup.Add(lightGo);
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.52f);
            var camGo = new GameObject("Cam"); var cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.backgroundColor = new Color(0.15f, 0.15f, 0.18f); cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 30;
            var (v, src) = Hero("hero_warrior", Sword(), Vector3.zero, yaw: 0f);
            const int W = 300, H = 420;
            // Bok, przód i z tyłu z góry (jak kamera gry).
            var views = new[] { new Vector3(3.2f, 0.9f, 0f), new Vector3(0.2f, 0.9f, 3.4f), new Vector3(0f, 2.2f, -3.2f) };
            var sheet = new Texture2D(W * 8, H * 3, TextureFormat.RGB24, false);
            var rt = new RenderTexture(W, H, 24);
            int shot = 0;
            // Jak w grze: postać przesuwa się w Update, animację liczy LateUpdate wizualizacji (stałe 60 kl./s).
            Time.captureFramerate = 60;
            for (int i = 0; i < 160 && shot < 8; i++)
            {
                v.transform.position += Vector3.forward * 5.2f / 60f;
                yield return null;
                if (i < 100 || (i - 100) % 5 != 0) continue;
                Vector3 p = v.transform.position;
                for (int k = 0; k < views.Length; k++)
                {
                    cam.transform.position = p + views[k];
                    cam.transform.LookAt(p + Vector3.up * 0.8f);
                    cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                    sheet.ReadPixels(new Rect(0, 0, W, H), shot * W, (2 - k) * H);
                    RenderTexture.active = null; cam.targetTexture = null;
                }
                shot++;
            }
            v.gameObject.SetActive(false);
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(dir, "hero_run_sword.png"), sheet.EncodeToPNG());

            // Bieg w różnych kierunkach przy stałym obrocie (namierzony cel): przód-prawo, prawo, tył-prawo, tył, przód-lewo, lewo.
            {
                var dirs = new[] { 45f, 90f, 135f, 180f, -45f, -90f };
                var sheet3 = new Texture2D(W * 6, H * 2, TextureFormat.RGB24, false);
                for (int d = 0; d < dirs.Length; d++)
                {
                    var (v3, _) = Hero("hero_warrior", Sword(), Vector3.zero, yaw: 0f);
                    Vector3 mv = Quaternion.Euler(0, dirs[d], 0) * Vector3.forward;
                    for (int i = 0; i < 110; i++) { v3.transform.position += mv * 5.2f / 60f; yield return null; }
                    for (int k = 0; k < 2; k++)
                    {
                        // 0: z kierunku ruchu (ruch na kamerę), 1: z przodu postaci
                        Vector3 p3 = v3.transform.position;
                        Vector3 off = k == 0 ? mv * 3.4f + Vector3.up * 0.9f : views[1];
                        cam.transform.position = p3 + off;
                        cam.transform.LookAt(p3 + Vector3.up * 0.8f);
                        cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                        sheet3.ReadPixels(new Rect(0, 0, W, H), d * W, (1 - k) * H);
                        RenderTexture.active = null; cam.targetTexture = null;
                    }
                    v3.gameObject.SetActive(false);
                }
                sheet3.Apply();
                File.WriteAllBytes(Path.Combine(dir, "hero_run_dirs.png"), sheet3.EncodeToPNG());
                Object.Destroy(sheet3);
            }

            // Porównanie: samo nagranie biegu (bez sterownika) na tym samym modelu, z przodu.
            v.gameObject.SetActive(false);
            var raw = Object.Instantiate(Def("hero_warrior").modelPrefab);
            cleanup.Add(raw);
            raw.transform.localScale = Vector3.one * Def("hero_warrior").scale;
            var clip = AssetDatabase.LoadAssetAtPath<WeaponAnimSet>(Visuals + "anim_sword_shield.asset").run;
            var sheet2 = new Texture2D(W * 8, H * 2, TextureFormat.RGB24, false);
            for (int s = 0; s < 8; s++)
            {
                clip.SampleAnimation(raw, clip.length * s / 8f);
                raw.transform.position = Vector3.zero;
                yield return null;
                for (int k = 0; k < 2; k++)
                {
                    cam.transform.position = views[k];
                    cam.transform.LookAt(Vector3.up * 0.8f);
                    cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                    sheet2.ReadPixels(new Rect(0, 0, W, H), s * W, (1 - k) * H);
                    RenderTexture.active = null; cam.targetTexture = null;
                }
            }
            sheet2.Apply();
            File.WriteAllBytes(Path.Combine(dir, "hero_run_sword_raw.png"), sheet2.EncodeToPNG());
            Time.captureFramerate = 0;
            Object.Destroy(rt); Object.Destroy(sheet); Object.Destroy(sheet2);
        }

        [UnityTest]
        public IEnumerator HeroGallery_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię bohaterów."); yield break; }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(80, 1, 80);
            ground.GetComponent<Renderer>().material.color = new Color(0.35f, 0.34f, 0.33f);
            cleanup.Add(ground);
            var lightGo = new GameObject("L"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.shadows = LightShadows.Soft; l.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(40, 160, 0); cleanup.Add(lightGo);
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.48f);
            var camGo = new GameObject("Cam"); var cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f); cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 35;

            // Rząd 1: wszyscy bohaterowie w spoczynku z bronią startową swojej specjalizacji.
            var looks = new Dictionary<string, RigLook>
            {
                ["paladin"] = Sword(), ["hero_warrior"] = Greatsword(), ["hero_warrior_berserker"] = Greataxe(), ["hero_warrior_dual"] = Sword(),
                ["mage_witch"] = Staff("fire"), ["hero_mage_fire"] = Staff("fire"), ["hero_mage_water"] = Staff("water"), ["hero_mage_air"] = Staff("air"),
                ["hero_mage_earth"] = Staff("earth"), ["hero_mage_summoner"] = Staff("fire"), ["hero_mage_battle"] = Sword(),
                ["rogue_hooded_meshy"] = Bow(), ["hero_rogue_archer"] = Bow(), ["hero_rogue_assassin"] = Daggers(), ["hero_rogue_arcane_archer"] = Bow(),
            };
            var all = new List<(CharacterVisual, StubAnimSource, CharacterAnimState)>();
            float spacing = 1.6f;
            for (int i = 0; i < Heroes.Length; i++)
            {
                var (v, s) = Hero(Heroes[i], looks[Heroes[i]], new Vector3((i - 7) * spacing, 0, 0));
                all.Add((v, s, new CharacterAnimState()));
            }
            // Rząd 2: ciosy i gesty w kluczowych fazach.
            var row2 = new List<(string, RigLook, CharacterAnimState)>
            {
                ("paladin", Sword(), Attack(AttackAnim.SlashRight, ActionPhase.Startup, 1f)),
                ("paladin", Sword(), Attack(AttackAnim.SlashRight, ActionPhase.Active, 1f)),
                ("paladin", Sword(), new CharacterAnimState { action = AnimAction.Block }),
                ("hero_warrior", Greatsword(), Attack(AttackAnim.SlashRight, ActionPhase.Startup, 1f)),
                ("hero_warrior", Greatsword(), Attack(AttackAnim.Overhead, ActionPhase.Active, 0.5f)),
                ("hero_warrior_berserker", Hammer(), Attack(AttackAnim.SlashLeft, ActionPhase.Startup, 1f)),
                ("hero_rogue_archer", Bow(), Attack(AttackAnim.BowDraw, ActionPhase.Startup, 1f)),
                ("hero_rogue_archer", Bow(), Attack(AttackAnim.BowDraw, ActionPhase.Active, 1f)),
                ("hero_mage_fire", Staff("fire"), new CharacterAnimState { action = AnimAction.Cast, phase = ActionPhase.Active, phaseProgress = 0.5f }),
                ("hero_mage_water", Staff("water"), Attack(AttackAnim.Cast, ActionPhase.Active, 0.5f)),
                ("hero_rogue_assassin", Daggers(), Attack(AttackAnim.Thrust, ActionPhase.Active, 0.5f)),
                ("mage_witch", Staff("fire"), new CharacterAnimState { action = AnimAction.Death, actionTime = 3f }),
            };
            for (int i = 0; i < row2.Count; i++)
            {
                var (h, look, st) = row2[i];
                var (v, s) = Hero(h, look, new Vector3((i - 5.5f) * spacing * 1.2f, 0, 4f));
                all.Add((v, s, st));
            }
            foreach (var (v, s, st) in all) { s.state = st; for (int k = 0; k < 60; k++) v.Tick(1f / 60f); }
            yield return null;

            void Shot(string name, Vector3 pos, Vector3 look, int w = 2400, int hgt = 1000)
            {
                cam.transform.position = pos;
                cam.transform.LookAt(look);
                var rt = new RenderTexture(w, hgt, 24);
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                var tex = new Texture2D(w, hgt, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, w, hgt), 0, 0); tex.Apply();
                cam.targetTexture = null; RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(dir, name), tex.EncodeToPNG());
                Object.Destroy(rt); Object.Destroy(tex);
            }
            Shot("hero_row1.png", new Vector3(0, 1.3f, -15f), new Vector3(0, 0.95f, 0));
            Shot("hero_row1_left.png", new Vector3(-6.4f, 1.2f, -5.5f), new Vector3(-6.4f, 0.95f, 0), 1800, 1000);
            Shot("hero_row1_mid.png", new Vector3(0f, 1.2f, -5.5f), new Vector3(0f, 0.95f, 0), 1800, 1000);
            Shot("hero_row1_right.png", new Vector3(6.4f, 1.2f, -5.5f), new Vector3(6.4f, 0.95f, 0), 1800, 1000);
            // Rząd 2 zza rzędu 1 nie jest widoczny z przodu – kamera od tyłu rzędu 2 (postacie patrzą w −Z).
            foreach (var (v, _, _) in all.Take(Heroes.Length)) v.gameObject.SetActive(false);
            Shot("hero_row2.png", new Vector3(0, 1.3f, -12f), new Vector3(0, 0.95f, 4f));
            Shot("hero_row2_left.png", new Vector3(-6f, 1.3f, -1.5f), new Vector3(-6f, 0.95f, 4f), 1800, 1000);
            Shot("hero_row2_right.png", new Vector3(6f, 1.3f, -1.5f), new Vector3(6f, 0.95f, 4f), 1800, 1000);
            // Widok z góry pod kątem jak w grze.
            Shot("hero_row2_game.png", new Vector3(0, 9f, -6f), new Vector3(0, 0.5f, 4f));
        }
    }
}
#endif
