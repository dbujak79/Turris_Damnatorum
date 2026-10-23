using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    public class StubAnimSource : ICharacterAnimSource
    {
        public CharacterAnimState state;
        public CharacterAnimState GetAnimState() => state;
    }

    /// <summary>Animacja humanoida: synchronizacja z fazami walki, poprawność IK, sterownik klipów, galeria póz.</summary>
    public class AnimationPlayTests
    {
        readonly List<Object> cleanup = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
        }

        (CharacterVisual visual, StubAnimSource src) Dummy(RigLook look, Vector3 pos, float yaw = 180f, float scale = 1f)
        {
            var go = new GameObject("Dummy");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, yaw, 0);
            var v = go.AddComponent<CharacterVisual>();
            v.BuildProcedural(look, scale);
            var src = new StubAnimSource();
            v.Init(src);
            cleanup.Add(go);
            return (v, src);
        }

        static CharacterAnimState Attack(AttackAnim a, ActionPhase phase, float p) =>
            new CharacterAnimState { action = AnimAction.Attack, attack = a, phase = phase, phaseProgress = p };

        static RigLook Knight() => new RigLook { body = BodyGear.Chain, head = HeadGear.GreatHelm, cape = true, weapon = WeaponModel.Sword, shield = ShieldModel.Heater };

        [UnityTest]
        public IEnumerator SlashRight_BladeTravelsFromBehindRightToFrontLeft()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            Transform ch = v.transform;

            src.state = Attack(AttackAnim.SlashRight, ActionPhase.Startup, 1f);
            v.Tick(1f / 60f);
            Vector3 windTip = ch.InverseTransformPoint(v.WeaponTip);

            src.state = Attack(AttackAnim.SlashRight, ActionPhase.Active, 0.5f);
            v.Tick(1f / 60f);
            Vector3 hitTip = ch.InverseTransformPoint(v.WeaponTip);

            src.state = Attack(AttackAnim.SlashRight, ActionPhase.Active, 1f);
            v.Tick(1f / 60f);
            Vector3 followTip = ch.InverseTransformPoint(v.WeaponTip);
            yield return null;

            Assert.Greater(windTip.x, 0.2f, "Zamach: ostrze po prawej stronie");
            Assert.Less(windTip.z, hitTip.z, "Zamach: ostrze cofnięte względem trafienia");
            Assert.Greater(hitTip.z, 0.9f, "Trafienie: ostrze wyraźnie przed postacią");
            Assert.Less(followTip.x, -0.2f, "Wybrzmienie: ostrze po lewej stronie");
        }

        [UnityTest]
        public IEnumerator Overhead_BladeGoesFromAboveHeadToLowFront()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            src.state = Attack(AttackAnim.Overhead, ActionPhase.Startup, 1f);
            v.Tick(1f / 60f);
            Vector3 wind = v.transform.InverseTransformPoint(v.WeaponTip);
            src.state = Attack(AttackAnim.Overhead, ActionPhase.Active, 1f);
            v.Tick(1f / 60f);
            Vector3 follow = v.transform.InverseTransformPoint(v.WeaponTip);
            yield return null;
            Assert.Greater(wind.y, 1.6f, "Szczyt zamachu nad głową");
            Assert.Less(follow.y, 0.9f, "Cios kończy się nisko");
            Assert.Greater(follow.z, 0.5f, "…i przed postacią");
        }

        [UnityTest]
        public IEnumerator IK_HandReachesTarget()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            src.state = new CharacterAnimState { action = AnimAction.Block };
            for (int i = 0; i < 60; i++) v.Tick(1f / 60f);
            yield return null;
            var rig = v.Rig;
            var pose = ((ProceduralHumanoidAnimator)v.Driver).Current;
            Vector3 want = rig.VisualRoot.TransformPoint(pose.handL);
            float reach = (rig.UpperArmLength + rig.ForearmLength);
            float dist = Vector3.Distance(rig[Bone.UpperArmL].position, want);
            if (dist < reach * 0.98f)
                Assert.Less(Vector3.Distance(rig[Bone.HandL].position, want), 0.02f, "IK doprowadza dłoń do celu");
            foreach (var b in rig.bones) Assert.IsFalse(float.IsNaN(b.position.x) || float.IsNaN(b.rotation.x), $"NaN w kości {b.name}");
        }

        [UnityTest]
        public IEnumerator Dodge_RollsWholeBody_AndDeathEndsOnGround()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            src.state = new CharacterAnimState { action = AnimAction.Dodge, actionTime = 0.23f, actionDuration = 0.75f, dodgeDirection = Vector3.forward };
            v.Tick(1f / 60f);
            float midRollHeadY = v.Rig[Bone.Head].position.y;
            src.state = new CharacterAnimState { action = AnimAction.Death, actionTime = 2f };
            v.Tick(1f / 60f);
            float deadHeadY = v.Rig[Bone.Head].position.y;
            yield return null;
            Assert.Less(midRollHeadY, 1.0f, "W połowie przewrotu głowa jest nisko");
            Assert.Less(deadHeadY, 0.5f, "Po śmierci ciało leży na ziemi");
        }

        [UnityTest]
        public IEnumerator HitImpact_BodyGivesWayToBlow_HoldsInHitstop_ThenRecovers()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            src.state = new CharacterAnimState();
            for (int k = 0; k < 30; k++) v.Tick(1f / 60f);
            var anim = (ProceduralHumanoidAnimator)v.Driver;
            Vector3 chest0 = v.Rig[Bone.Chest].position;

            // Cios z przodu (od +Z w stronę postaci): pchnięcie do tyłu. Najpierw hitstop – poza stoi (tylko drży).
            v.Impact(Vector3.back, 1f);
            v.Hold(0.1f);
            Vector3 head0 = v.Rig[Bone.Head].position;
            for (int k = 0; k < 3; k++) v.Tick(1f / 60f);
            Assert.Less(Vector3.Distance(v.Rig[Bone.Head].position, head0), 0.06f, "W hitstopie poza zastyga");
            for (int k = 0; k < 12; k++) v.Tick(1f / 60f);
            Assert.Less(anim.ReactOffset.z, -0.4f, "Tułów ustępuje w kierunku ciosu");
            Assert.Less(v.Rig[Bone.Chest].position.z, chest0.z - 0.05f, "Klatka odchyla się do tyłu");
            yield return null;

            for (int k = 0; k < 90; k++) v.Tick(1f / 60f);
            Assert.Less(anim.ReactOffset.magnitude, 0.05f, "Po chwili postać wraca do gardy");
            Assert.Less(Vector3.Distance(v.Rig[Bone.Chest].position, chest0), 0.03f);
        }

        /// <summary>
        /// Zachwianie (drgnięcie wroga) zależy od ciosu: tułów odchyla się od napastnika, cios z boku skręca go w bok,
        /// cięższy cios zgina mocniej i ugina kolana; szczyt przychodzi szybko, powrót jest gładki; nogi kroczą z odrzutem.
        /// </summary>
        [UnityTest]
        public IEnumerator Stagger_FollowsBlowDirectionAndWeight_LegsStepWithKnockback()
        {
            var cases = new[]
            {
                (dir: Vector3.back, w: 0.35f), (dir: Vector3.back, w: 1.1f), (dir: Vector3.right, w: 0.8f),
            };
            var head = new Vector3[cases.Length];
            var hips = new float[cases.Length];
            Vector3 headRest = Vector3.zero; float hipsRest = 0f;
            for (int c = 0; c < cases.Length; c++)
            {
                var (v, src) = Dummy(Knight(), new Vector3(c * 4f, 0, 0), 0f);
                src.state = new CharacterAnimState();
                for (int k = 0; k < 20; k++) v.Tick(1f / 60f);
                headRest = v.transform.InverseTransformPoint(v.Rig[Bone.Head].position);
                hipsRest = v.Rig[Bone.Hips].position.y;
                // Szczyt zachwiania: ~0,14 czasu reakcji (0,75 s).
                for (int k = 0; k <= 7; k++)
                {
                    src.state = new CharacterAnimState { action = AnimAction.Flinch, actionTime = k / 60f, actionDuration = 0.75f, hitDirection = cases[c].dir, hitWeight = cases[c].w };
                    v.Tick(1f / 60f);
                }
                head[c] = v.transform.InverseTransformPoint(v.Rig[Bone.Head].position) - headRest;
                hips[c] = v.Rig[Bone.Hips].position.y - hipsRest;
            }
            yield return null;
            Debug.Log($"Głowa przy zachwianiu: lekki {head[0]}, ciężki {head[1]}, z boku {head[2]}; biodra {hips[0]:F3} / {hips[1]:F3}");
            Assert.Less(head[0].z, -0.05f, "Cios z przodu – głowa i tułów do tyłu");
            Assert.Less(head[1].z, head[0].z - 0.05f, "Cięższy cios odchyla wyraźnie mocniej");
            Assert.Less(hips[1], hips[0] - 0.01f, "Cięższy cios ugina kolana głębiej");
            Assert.Greater(head[2].x, 0.03f, "Cios z boku – tułów ugina się w stronę pchnięcia");

            // Odrzut w czasie zachwiania: stopy kroczą (zmieniają się podparcia), zamiast sunąć razem z ciałem.
            var (s, ss) = Dummy(Knight(), new Vector3(0, 0, 10f), 0f);
            ss.state = new CharacterAnimState();
            for (int k = 0; k < 20; k++) s.Tick(1f / 60f);
            Vector3 footL0 = s.Rig[Bone.FootL].position, footR0 = s.Rig[Bone.FootR].position;
            float pushed = 0f;
            int planted = 0;
            Vector3 pl = footL0, pr = footR0;
            for (int k = 0; k < 24; k++)
            {
                ss.state = new CharacterAnimState { action = AnimAction.Flinch, actionTime = k / 60f, actionDuration = 0.75f, hitDirection = Vector3.back, hitWeight = 1f };
                float step = k < 11 ? 0.6f / 11f : 0f; // 0,6 m w 0,18 s (3,3 m/s)
                s.transform.position += Vector3.back * step;
                pushed += step;
                s.Tick(1f / 60f);
                Vector3 l = s.Rig[Bone.FootL].position, r = s.Rig[Bone.FootR].position;
                // Jedna stopa stoi (podpora), druga wędruje – krok, a nie sunięcie obu nóg razem z ciałem.
                if (k >= 3 && k < 11 && Mathf.Min((l - pl).magnitude, (r - pr).magnitude) * 60f < 1.2f) planted++;
                pl = l; pr = r;
            }
            float movedL = Vector3.Distance(s.Rig[Bone.FootL].position, footL0), movedR = Vector3.Distance(s.Rig[Bone.FootR].position, footR0);
            Assert.Greater(Mathf.Max(movedL, movedR), pushed * 0.5f, "Stopy idą za ciałem");
            Assert.GreaterOrEqual(planted, 3, $"Krok: w czasie odrzutu jedna stopa stoi w podporze (klatek {planted})");
        }

        [UnityTest]
        public IEnumerator Death_FallsInDirectionOfBlow()
        {
            var (fwd, s1) = Dummy(Knight(), new Vector3(-3f, 0, 0), 0f);
            var (back, s2) = Dummy(Knight(), new Vector3(3f, 0, 0), 0f);
            s1.state = new CharacterAnimState { action = AnimAction.Death, actionTime = 2f, hitDirection = Vector3.forward, hitWeight = 1f };
            s2.state = new CharacterAnimState { action = AnimAction.Death, actionTime = 2f, hitDirection = Vector3.back };
            for (int k = 0; k < 5; k++) { fwd.Tick(1f / 60f); back.Tick(1f / 60f); }
            yield return null;
            Assert.Greater(fwd.Rig[Bone.Head].position.z, 0.9f, "Cios w plecy – pada na twarz, do przodu");
            Assert.Less(back.Rig[Bone.Head].position.z, -0.6f, "Cios z przodu – pada na plecy");
            Assert.Less(fwd.Rig[Bone.Head].position.y, 0.5f);
            Assert.Less(back.Rig[Bone.Head].position.y, 0.5f);
        }

        [UnityTest]
        public IEnumerator Run_StanceFootBarelySlides_AndBodyLeansWhenStartingAndStopping()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            src.state = new CharacterAnimState();
            var anim = (ProceduralHumanoidAnimator)v.Driver;
            const float dt = 1f / 60f, speed = 5.2f;
            for (int k = 0; k < 20; k++) v.Tick(dt);
            float maxLean = 0f;
            var feet = new List<(float y, float v)>();
            Vector3 prevL = v.Rig[Bone.FootL].position, prevR = v.Rig[Bone.FootR].position;
            for (int k = 0; k < 150; k++)
            {
                v.transform.position += Vector3.forward * speed * dt;
                v.Tick(dt);
                if (k < 20) maxLean = Mathf.Max(maxLean, anim.Lean.z);
                Vector3 l = v.Rig[Bone.FootL].position, r = v.Rig[Bone.FootR].position;
                if (k > 60) { feet.Add((l.y, Mathf.Abs(l.z - prevL.z) / dt)); feet.Add((r.y, Mathf.Abs(r.z - prevR.z) / dt)); }
                prevL = l; prevR = r;
            }
            // Stopa w podporze = na wysokości podłoża (w fazie lotu biegu obie są w górze); jej przesuw względem ziemi ma być mały.
            float ground = float.MaxValue;
            foreach (var f in feet) ground = Mathf.Min(ground, f.y);
            float slide = 0f; int samples = 0;
            foreach (var f in feet) if (f.y < ground + 0.003f) { slide += f.v; samples++; }
            Assert.Greater(samples, 20, "Są klatki z podporą");
            float avgSlide = slide / samples;
            Debug.Log($"[Turris] Średni poślizg stopy w podporze: {avgSlide:0.00} m/s przy {speed} m/s, pochylenie przy starcie {maxLean:0.00}");
            Assert.Greater(maxLean, 0.2f, "Przy ruszaniu ciało pochyla się do przodu");
            Assert.Less(avgSlide, 0.6f, "Stopa w podporze stoi na podłodze (IK)");

            float minLean = 0f;
            for (int k = 0; k < 30; k++) { v.Tick(dt); minLean = Mathf.Min(minLean, anim.Lean.z); }
            yield return null;
            Assert.Less(minLean, -0.1f, "Przy hamowaniu ciało odchyla się do tyłu");
        }

        [UnityTest]
        public IEnumerator Cloth_CapeFlowsInRun_TabardFlapsGiveWayToKnees()
        {
            var (v, src) = Dummy(Knight(), Vector3.zero, 0f);
            src.state = new CharacterAnimState();
            var anim = (ProceduralHumanoidAnimator)v.Driver;
            Assert.AreEqual(4, v.Rig.CapeSegments.Length, "Peleryna z czterech segmentów");
            Assert.AreEqual(2, v.Rig.HipFlaps.Count, "Tabard: płat przedni i tylny");
            for (int k = 0; k < 60; k++) v.Tick(1f / 60f);
            int fi = v.Rig.HipFlaps.FindIndex(f => f.name == "TabardFront"), bi = v.Rig.HipFlaps.FindIndex(f => f.name == "TabardBack");
            float idleCape = anim.CapeAngles[0].x;
            float minFront = 0f, maxBack = 0f, maxCape = idleCape;
            for (int k = 0; k < 120; k++)
            {
                v.transform.position += Vector3.forward * 5.2f / 60f;
                v.Tick(1f / 60f);
                maxCape = Mathf.Max(maxCape, anim.CapeAngles[0].x);
                minFront = Mathf.Min(minFront, anim.FlapAngles[fi]);
                maxBack = Mathf.Max(maxBack, anim.FlapAngles[bi]);
                foreach (var a in anim.CapeAngles) Assert.IsFalse(float.IsNaN(a.x) || float.IsNaN(a.y));
            }
            yield return null;
            Assert.Greater(maxCape - idleCape, 12f, "W biegu peleryna odchyla się do tyłu");
            Assert.Less(minFront, -8f, "Kolano w wykroku odpycha przedni płat tabardu");
            Assert.Greater(maxBack, 8f, "Tylny płat odchyla się (noga z tyłu, pęd)");
        }

        [UnityTest]
        public IEnumerator EnemyVariety_SameArchetype_DifferentLooks()
        {
            var def = TestContent.Create().Get<EnemyDefinition>("enemy_ghoul");
            var a = RigLook.Vary(RigLook.ForEnemy(def), 1);
            var b = RigLook.Vary(RigLook.ForEnemy(def), 2);
            var a2 = RigLook.Vary(RigLook.ForEnemy(def), 1);
            Assert.AreEqual(a.skin, a2.skin, "Ten sam los – ten sam wygląd");
            Assert.AreNotEqual(a.skin, b.skin, "Różne egzemplarze różnią się odcieniem");
            Assert.AreEqual(a.style, b.style);
            Assert.AreEqual(a.weapon, b.weapon, "Broń (i czytelność ataków) bez zmian");
            var boss = TestContent.Create().Get<EnemyDefinition>("enemy_castellan");
            Assert.AreEqual(RigLook.ForEnemy(boss).cloth, RigLook.Vary(RigLook.ForEnemy(boss), 5).cloth, "Boss bez odmian");
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerCombat_DrivesRigInSyncWithActionPhases()
        {
            var bundle = TestContent.Create();
            var p = WorldBuilder.CreatePlayer();
            cleanup.Add(p);
            p.GetComponent<PlayerController>().enabled = false;
            var pc = p.GetComponent<PlayerCombat>();
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>("legacy_knight"), difficulty = bundle.config.difficulties[0] };
            pc.Init(RunFactory.Create(plan, bundle.config, 1), bundle.config);
            p.GetComponent<PlayerVisuals>().Refresh();
            var v = p.GetComponent<CharacterVisual>();
            Assert.IsTrue(v.UsesModel, "Gracz to bohater z modelem FBX");
            Assert.IsInstanceOf<ClipAnimationDriver>(v.Driver);
            Assert.IsNotNull(v.ModelSet, "Miecz – zestaw animacji miecza z tarczą");
            var socket = Util.FindDeep(v.Model.transform, "WeaponSocket");
            Assert.IsNotNull(socket);
            var sword = socket.childCount > 0 ? socket.GetChild(0).gameObject : null;
            Assert.IsNotNull(sword, "Miecz w dłoni");

            pc.Request(ActionType.LightAttack);
            var st = pc.GetAnimState();
            Assert.AreEqual(AnimAction.Attack, st.action);
            Assert.AreEqual(AttackAnim.SlashRight, st.attack);
            yield return null;

            // Zmiana sprzętu przebudowuje wygląd: topór zamiast miecza.
            var run = pc.Run;
            var axe = new ItemInstance(bundle.Get<ItemDefinition>("weapon_axe"));
            run.inventory.Add(axe);
            run.EquipFromInventory(axe, EquipSlot.MainHand);
            pc.RefreshBuild();
            yield return null;
            socket = Util.FindDeep(v.Model.transform, "WeaponSocket");
            Assert.IsTrue(socket.childCount > 0 && socket.GetChild(0).gameObject != sword, "Topór zamiast miecza – broń w dłoni przebudowana");
        }

        [UnityTest]
        public IEnumerator Enemies_UseStyledHumanoids()
        {
            var bundle = TestContent.Create();
            var ids = new[] { "enemy_ghoul", "enemy_heretic", "enemy_warden", "enemy_castellan" };
            var expected = new[] { RigStyle.Ghoul, RigStyle.Heretic, RigStyle.Warden, RigStyle.Castellan };
            for (int i = 0; i < ids.Length; i++)
            {
                var def = bundle.Get<EnemyDefinition>(ids[i]);
                var e = WorldBuilder.CreateEnemy(def, new Vector3(i * 3, 0, 10), null);
                cleanup.Add(e.gameObject);
                var v = e.GetComponent<CharacterVisual>();
                Assert.IsNotNull(v.Rig);
                Assert.AreEqual(expected[i], v.Rig.Look.style);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator DetailLevel_ManyParts_FewRenderers()
        {
            var bundle = TestContent.Create();
            var looks = new (string name, RigLook look, float scale)[]
            {
                ("rycerz", new RigLook { body = BodyGear.Chain, head = HeadGear.Coif, cape = true, weapon = WeaponModel.Sword, shield = ShieldModel.Heater, cloth = new Color(0.45f, 0.08f, 0.08f) }, 1f),
                ("mag", new RigLook { body = BodyGear.Robe, head = HeadGear.Hood, style = RigStyle.Mage, weapon = WeaponModel.Staff, cloth = new Color(0.2f, 0.25f, 0.6f) }, 1f),
                ("ghul", RigLook.ForEnemy(bundle.Get<EnemyDefinition>("enemy_ghoul")), 0.95f),
                ("heretyk", RigLook.ForEnemy(bundle.Get<EnemyDefinition>("enemy_heretic")), 1f),
                ("straznik", RigLook.ForEnemy(bundle.Get<EnemyDefinition>("enemy_warden")), 1.25f),
                ("kasztelan", RigLook.ForEnemy(bundle.Get<EnemyDefinition>("enemy_castellan")), 1.6f),
            };
            foreach (var (name, look, scale) in looks)
            {
                var (v, _) = Dummy(look, Vector3.zero);
                int gearParts = 0;
                var renderers = v.GetComponentsInChildren<Renderer>(true);
                int submeshes = 0, verts = 0;
                foreach (var r in renderers)
                {
                    submeshes += r.sharedMaterials.Length;
                    var mf = r.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null) verts += mf.sharedMesh.vertexCount;
                }
                Debug.Log($"DETAIL {name}: części ciała {v.Rig.PartCount}, renderery {renderers.Length}, podsiatki {submeshes}, wierzchołki {verts}");
                Assert.GreaterOrEqual(v.Rig.PartCount, 60, name);
                Assert.LessOrEqual(renderers.Length, 30, $"{name}: scalanie części ogranicza liczbę rendererów");
                Object.Destroy(v.gameObject);
                gearParts++;
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClipDriver_ScrubsActionClipByCombatPhase()
        {
            // Model „Generic” z jedną kością i klipami wygenerowanymi w kodzie – sprawdza mapowanie faz na czas klipu.
            var model = new GameObject("GenericModel");
            cleanup.Add(model);
            var arm = new GameObject("Arm").transform;
            arm.SetParent(model.transform, false);
            model.AddComponent<Animator>();

            AnimationClip Clip(string name, float from, float to, float len)
            {
                var c = new AnimationClip { name = name };
                c.SetCurve("Arm", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, from, len, to));
                c.SetCurve("Arm", typeof(Transform), "localPosition.y", AnimationCurve.Constant(0, len, 0));
                c.SetCurve("Arm", typeof(Transform), "localPosition.z", AnimationCurve.Constant(0, len, 0));
                return c;
            }
            var def = ScriptableObject.CreateInstance<CharacterVisualDefinition>();
            def.idle = Clip("idle", 0, 0, 1f);
            def.actions.Add(new ActionClip { action = AnimAction.Attack, clip = Clip("attack", 0f, 10f, 2f), windupEnd = 0.4f, activeEnd = 0.6f });

            var driver = new ClipAnimationDriver(model.GetComponent<Animator>(), def, model.transform);
            try
            {
                // Cios wręcz: faza aktywna obejmuje w klipie samo przejście ostrza (od chwili przed windupEnd), reszta – powrót.
                float strike = 0.4f - 0.2f * ClipAnimationDriver.StrikeEarly;
                float follow = 0.4f + 0.2f * ClipAnimationDriver.StrikeShare;
                // Kilka klatek w tej samej fazie – warstwa akcji narasta przez przenikanie z ruchu.
                for (int i = 0; i < 20; i++) driver.Tick(Attack(AttackAnim.SlashRight, ActionPhase.Startup, 0.5f), Vector3.zero, 0.016f);
                Assert.AreEqual(2f * strike * 0.5f, driver.CurrentActionTime, 1e-3f, "Połowa zamachu = połowa drogi do początku cięcia");
                Assert.AreEqual(10f * strike * 0.5f, arm.localPosition.x, 0.05f);

                driver.Tick(Attack(AttackAnim.SlashRight, ActionPhase.Active, 1f), Vector3.zero, 0.016f);
                Assert.AreEqual(2f * follow, driver.CurrentActionTime, 1e-3f, "Koniec fazy aktywnej = koniec przejścia ostrza");
                Assert.AreEqual(10f * follow, arm.localPosition.x, 0.05f);

                // Strzał z łuku – bez wyprzedzenia: zwolnienie cięciwy dokładnie na granicy faz.
                driver.Tick(Attack(AttackAnim.BowDraw, ActionPhase.Active, 1f), Vector3.zero, 0.016f);
                Assert.AreEqual(2f * 0.6f, driver.CurrentActionTime, 1e-3f, "Łuk: koniec fazy aktywnej = activeEnd");
            }
            finally { driver.Dispose(); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllWeaponModels_Build_WithTipAwayFromHand()
        {
            foreach (WeaponModel m in System.Enum.GetValues(typeof(WeaponModel)))
            {
                if (m == WeaponModel.Auto || m == WeaponModel.None || m == WeaponModel.Claws) continue;
                var look = Knight(); look.weapon = m;
                if (look.TwoHanded) look.shield = ShieldModel.None;
                var (v, _) = Dummy(look, new Vector3(0, 0, 0));
                v.Tick(1f / 60f);
                Assert.IsNotNull(v.Rig.WeaponSocket.GetComponentInChildren<MeshRenderer>(), $"{m}: model broni");
                Assert.Greater(Vector3.Distance(v.WeaponTip, v.Rig[Bone.HandR].position), 0.3f, $"{m}: czubek broni daleko od dłoni");
                Object.Destroy(v.gameObject);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator Bow_StringDrawsToHand_AndSnapsBackOnRelease()
        {
            var look = new RigLook { body = BodyGear.Leather, head = HeadGear.Hood, weapon = WeaponModel.Bow, shield = ShieldModel.None };
            var (v, src) = Dummy(look, Vector3.zero);
            var anim = (ProceduralHumanoidAnimator)v.Driver;
            Assert.IsNotNull(v.Rig.BowStringUpper, "Łuk ma żywą cięciwę");

            src.state = Attack(AttackAnim.BowDraw, ActionPhase.Startup, 1f);
            for (int i = 0; i < 20; i++) v.Tick(1f / 60f);
            Assert.Greater(anim.BowDraw, 0.95f, "Pełny naciąg na końcu zamachu");
            Vector3 handL = v.Rig[Bone.HandL].position;
            Vector3 nockEnd = v.Rig.BowStringUpper.position + v.Rig.BowStringUpper.up * v.Rig.BowStringUpper.lossyScale.y * 0.5f;
            Assert.Less(Vector3.Distance(nockEnd, handL), 0.08f, "Cięciwa sięga dłoni naciągającej");
            Vector3 wrist = v.Rig.WeaponSocket.position;
            Assert.Greater(Vector3.Distance(handL, wrist), 0.35f, "Dłoń odciągnięta od łuku");
            Assert.IsTrue(v.Rig.NockedArrow.gameObject.activeSelf, "Strzała na cięciwie w czasie naciągu");

            src.state = Attack(AttackAnim.BowDraw, ActionPhase.Active, 0.2f);
            v.Tick(1f / 60f);
            Assert.AreEqual(0f, anim.BowDraw, "Po strzale cięciwa wraca");
            Assert.IsFalse(v.Rig.NockedArrow.gameObject.activeSelf, "Strzała wypuszczona – znika z cięciwy");
            yield return null;
        }

        // ------------------------------------------------------------------ Galeria (zrzuty do przeglądu)

        [UnityTest]
        public IEnumerator PoseGallery_WhenRequested()
        {
            string dir = System.Environment.GetEnvironmentVariable("TURRIS_SHOT_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SHOT_DIR, aby zapisać galerię póz."); yield break; }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0, -0.5f, 0); ground.transform.localScale = new Vector3(60, 1, 60);
            ground.GetComponent<Renderer>().material.color = new Color(0.35f, 0.34f, 0.33f);
            cleanup.Add(ground);
            var lightGo = new GameObject("L"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(45, 150, 0); cleanup.Add(lightGo);
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.42f);
            var camGo = new GameObject("Cam"); var cam = camGo.AddComponent<Camera>(); cleanup.Add(camGo);
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f); cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 40;

            var knight = Knight();
            var states = new List<(string, CharacterAnimState)>
            {
                ("ready", new CharacterAnimState()),
                ("slashR-wind", Attack(AttackAnim.SlashRight, ActionPhase.Startup, 1f)),
                ("slashR-hit", Attack(AttackAnim.SlashRight, ActionPhase.Active, 0.5f)),
                ("slashR-follow", Attack(AttackAnim.SlashRight, ActionPhase.Active, 1f)),
                ("overhead-wind", Attack(AttackAnim.Overhead, ActionPhase.Startup, 1f)),
                ("overhead-follow", Attack(AttackAnim.Overhead, ActionPhase.Active, 1f)),
                ("thrust-hit", Attack(AttackAnim.Thrust, ActionPhase.Active, 0.5f)),
                ("block", new CharacterAnimState { action = AnimAction.Block }),
                ("parry", new CharacterAnimState { action = AnimAction.Parry, phase = ActionPhase.Active, phaseProgress = 0.6f }),
            };
            var row2 = new List<(RigLook, float, CharacterAnimState)>
            {
                (new RigLook { body = BodyGear.Robe, head = HeadGear.Hood, style = RigStyle.Mage, weapon = WeaponModel.Staff, cloth = new Color(0.2f,0.25f,0.6f) }, 1f, new CharacterAnimState()),
                (new RigLook { body = BodyGear.Robe, head = HeadGear.Hood, style = RigStyle.Mage, weapon = WeaponModel.Staff, cloth = new Color(0.2f,0.25f,0.6f) }, 1f, Attack(AttackAnim.Cast, ActionPhase.Active, 1f)),
                (RigLook.ForEnemy(TestContent.Create().Get<EnemyDefinition>("enemy_ghoul")), 0.95f, new CharacterAnimState()),
                (RigLook.ForEnemy(TestContent.Create().Get<EnemyDefinition>("enemy_ghoul")), 0.95f, Attack(AttackAnim.Claw, ActionPhase.Startup, 1f)),
                (RigLook.ForEnemy(TestContent.Create().Get<EnemyDefinition>("enemy_heretic")), 1f, Attack(AttackAnim.Cast, ActionPhase.Startup, 1f)),
                (RigLook.ForEnemy(TestContent.Create().Get<EnemyDefinition>("enemy_warden")), 1.25f, new CharacterAnimState { action = AnimAction.Block }),
                (RigLook.ForEnemy(TestContent.Create().Get<EnemyDefinition>("enemy_warden")), 1.25f, Attack(AttackAnim.Overhead, ActionPhase.Startup, 1f)),
                (RigLook.ForEnemy(TestContent.Create().Get<EnemyDefinition>("enemy_castellan")), 1.6f, Attack(AttackAnim.SlashRight, ActionPhase.Startup, 1f)),
                (knight, 1f, new CharacterAnimState { action = AnimAction.Kneel, actionTime = 1f }),
            };

            float spacing = 2.2f;
            var all = new List<(CharacterVisual, StubAnimSource, CharacterAnimState)>();
            for (int i = 0; i < states.Count; i++)
            {
                var (v, s) = Dummy(knight, new Vector3((i - 4) * spacing, 0, 0));
                all.Add((v, s, states[i].Item2));
            }
            for (int i = 0; i < row2.Count; i++)
            {
                var (look, sc, st) = row2[i];
                var (v, s) = Dummy(look, new Vector3((i - 4) * spacing * 1.1f, 0, 5f), 180f, sc);
                all.Add((v, s, st));
            }
            foreach (var (v, s, st) in all) { s.state = st; for (int k = 0; k < 90; k++) v.Tick(1f / 60f); }
            yield return null;

            void Shot(string name, Vector3 pos, Vector3 look)
            {
                cam.transform.position = pos;
                cam.transform.LookAt(look);
                var rt = new RenderTexture(1920, 1080, 24);
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
                var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); tex.Apply();
                cam.targetTexture = null; RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(dir, name), tex.EncodeToPNG());
                Object.Destroy(rt); Object.Destroy(tex);
            }
            // Postacie patrzą w −Z (yaw 180), kamera przed nimi.
            Shot("pose_row1_front.png", new Vector3(0, 1.6f, -14f), new Vector3(0, 1.0f, 0));
            Shot("pose_row1_side.png", new Vector3(-16f, 1.5f, -2f), new Vector3(0, 1.0f, 0));
            Shot("pose_close_slash.png", new Vector3(-8.2f, 1.8f, -3.6f), new Vector3(-6.2f, 1.1f, 0));
            Shot("pose_close_block.png", new Vector3(5.5f, 1.8f, -3.6f), new Vector3(6.6f, 1.1f, 0));
            for (int i = 0; i < states.Count; i++) all[i].Item1.gameObject.SetActive(false);
            Shot("pose_row2_front.png", new Vector3(0, 2.2f, -7f), new Vector3(0, 1.2f, 5f));
            Shot("pose_close_mage.png", new Vector3(-8.2f, 1.7f, 2.6f), new Vector3(-9.2f, 1.25f, 5f));
            Shot("pose_close_ghoul.png", new Vector3(-4.2f, 1.5f, 3.2f), new Vector3(-4.84f, 1.0f, 5f));
            Shot("pose_close_boss.png", new Vector3(5.2f, 2.3f, 0.8f), new Vector3(7.26f, 1.7f, 5f));
            Shot("pose_close_warden.png", new Vector3(3.2f, 1.9f, 1.6f), new Vector3(2.42f, 1.3f, 5f));

            // Sekwencja przewrotu w przód (klatki co 0,1 s), widok z boku.
            foreach (var (v, _, _) in all) v.gameObject.SetActive(false);
            float roll = TestContent.Create().config.balance.dodgeRollDuration;
            const int frames = 7;
            for (int i = 0; i < frames; i++)
            {
                var (v, s) = Dummy(knight, new Vector3(0, 0, 20f - i * 2.0f));
                s.state = new CharacterAnimState { action = AnimAction.Dodge, actionTime = roll * i / (frames - 1), actionDuration = 0.82f, rollDuration = roll, dodgeDirection = Vector3.back };
                for (int k = 0; k < 10; k++) v.Tick(1f / 60f);
            }
            yield return null;
            Shot("pose_roll_side.png", new Vector3(-15f, 1.2f, 14f), new Vector3(0, 0.8f, 14f));

            // Wszystkie modele broni w dłoni (nowe: buława, młot, włócznia, kosa).
            WeaponModel[] models = { WeaponModel.Sword, WeaponModel.Axe, WeaponModel.Mace, WeaponModel.Knives, WeaponModel.Bow, WeaponModel.GreatAxe, WeaponModel.Hammer, WeaponModel.Spear, WeaponModel.Scythe, WeaponModel.Staff };
            for (int i = 0; i < models.Length; i++)
            {
                var look = Knight(); look.weapon = models[i];
                if (look.TwoHanded || look.weapon == WeaponModel.Knives || look.weapon == WeaponModel.Bow) look.shield = ShieldModel.None; // jak w grze: bronie dwuręczne
                var (v, s) = Dummy(look, new Vector3((i - 4.5f) * 1.9f, 0, 40f));
                s.state = new CharacterAnimState();
                for (int k = 0; k < 10; k++) v.Tick(1f / 60f);
            }
            yield return null;
            Shot("pose_weapons.png", new Vector3(0, 1.6f, 27f), new Vector3(0, 1.1f, 40f));

            // Zbliżenie: Łotrzyk z nożami i z łukiem (skórznia, kaptur).
            var rogueLook = new RigLook { body = BodyGear.Leather, head = HeadGear.Hood, style = RigStyle.Knight, weapon = WeaponModel.Knives, shield = ShieldModel.None, cape = true, cloth = new Color(0.2f, 0.34f, 0.24f) };
            var (rk, rks) = Dummy(rogueLook, new Vector3(-0.8f, 0, 60f));
            rks.state = new CharacterAnimState();
            var bowLook = new RigLook { body = BodyGear.Leather, head = HeadGear.Hood, style = RigStyle.Knight, weapon = WeaponModel.Bow, shield = ShieldModel.None, cape = true, cloth = new Color(0.2f, 0.34f, 0.24f) };
            var (rb, rbs) = Dummy(bowLook, new Vector3(0.8f, 0, 60f));
            rbs.state = Attack(AttackAnim.BowDraw, ActionPhase.Startup, 1f); // pełny naciąg
            for (int k = 0; k < 10; k++) { rk.Tick(1f / 60f); rb.Tick(1f / 60f); }
            yield return null;
            Shot("pose_close_rogue.png", new Vector3(0.6f, 1.5f, 56.4f), new Vector3(0, 1.1f, 60f));
            Shot("pose_bow_side.png", new Vector3(3.6f, 1.5f, 59.6f), new Vector3(0.8f, 1.3f, 60f)); // cięciwa w naciągu z boku
            rk.gameObject.SetActive(false); rb.gameObject.SetActive(false);

            // Reakcja na cios z przodu: klatki 0 (hitstop) / 0,06 / 0,1 / 0,16 / 0,3 / 0,6 s po trafieniu, widok z boku.
            float[] reactTimes = { 0f, 0.06f, 0.1f, 0.16f, 0.3f, 0.6f };
            for (int i = 0; i < reactTimes.Length; i++)
            {
                var (v, s) = Dummy(RigLook.ForEnemy(TestContent.Create().Get<EnemyDefinition>("enemy_ghoul")), new Vector3(-4f + i * 1.6f, 0, 80f), 90f, 0.95f);
                s.state = new CharacterAnimState();
                for (int k = 0; k < 20; k++) v.Tick(1f / 60f);
                v.Impact(Vector3.left, 1f); // cios od strony +X (postacie patrzą w +X)
                int n = Mathf.RoundToInt(reactTimes[i] * 60f);
                for (int k = 0; k < n; k++) v.Tick(1f / 60f);
            }
            yield return null;
            Shot("pose_hitreact.png", new Vector3(0, 1.2f, 71.5f), new Vector3(0, 1.0f, 80f));

            // Upadek w kierunku ciosu: cios z przodu, w plecy, z lewej, z prawej (postacie patrzą w −Z).
            Vector3[] blows = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
            for (int i = 0; i < blows.Length; i++)
            {
                var (v, s) = Dummy(knight, new Vector3((i - 1.5f) * 4.5f, 0, 100f));
                s.state = new CharacterAnimState { action = AnimAction.Death, actionTime = 2f, hitDirection = blows[i], hitWeight = 0.8f };
                for (int k = 0; k < 5; k++) v.Tick(1f / 60f);
            }
            yield return null;
            Shot("pose_death_dirs.png", new Vector3(0, 8f, 92f), new Vector3(0, 0.2f, 100f));

            // Seria: drugie cięcie (w lewo) zaczyna się z wybrzmienia pierwszego – klatki zamachu co 25%, na końcu przestrzał.
            var comboStates = new List<CharacterAnimState>
            {
                Attack(AttackAnim.SlashRight, ActionPhase.Active, 1f),
                Attack(AttackAnim.SlashLeft, ActionPhase.Startup, 0.25f),
                Attack(AttackAnim.SlashLeft, ActionPhase.Startup, 0.5f),
                Attack(AttackAnim.SlashLeft, ActionPhase.Startup, 0.8f),
                Attack(AttackAnim.SlashLeft, ActionPhase.Active, 0.5f),
                Attack(AttackAnim.SlashLeft, ActionPhase.Active, 0.8f),
            };
            for (int i = 0; i < comboStates.Count; i++)
            {
                var (v, s) = Dummy(knight, new Vector3((i - 2.5f) * 2.2f, 0, 120f));
                // Najpierw wybrzmienie pierwszego ciosu, potem przejście w drugi – jak w grze.
                s.state = Attack(AttackAnim.SlashRight, ActionPhase.Active, 1f);
                for (int k = 0; k < 10; k++) v.Tick(1f / 60f);
                s.state = comboStates[i];
                for (int k = 0; k < 10; k++) v.Tick(1f / 60f);
            }
            yield return null;
            Shot("pose_combo.png", new Vector3(0, 1.8f, 110f), new Vector3(0, 1.1f, 120f));

            // Bieg (5,2 m/s) – 8 faz jednego cyklu kroku, widok z boku; postacie biegną, a do zdjęcia są ustawione w rzędzie.
            for (int i = 0; i < 8; i++)
            {
                var (v, s) = Dummy(knight, new Vector3(0, 0, 200f + i * 20f), 90f);
                s.state = new CharacterAnimState();
                int n = 40 + i * 3;
                for (int k = 0; k < n; k++) { v.transform.position += Vector3.right * 5.2f / 60f; v.Tick(1f / 60f); }
                v.transform.position = new Vector3((i - 3.5f) * 1.5f, 0, 140f);
            }
            yield return null;
            Shot("pose_run_side.png", new Vector3(0, 1.0f, 130.5f), new Vector3(0, 0.8f, 140f));

            // Tkaniny w biegu: rycerz z peleryną (widok z tyłu, 3/4) – fala peleryny i płaty tabardu przy udach.
            {
                var (v, s) = Dummy(knight, new Vector3(0, 0, 400f), 0f);
                s.state = new CharacterAnimState();
                for (int k = 0; k < 47; k++) { v.transform.position += Vector3.forward * 5.2f / 60f; v.Tick(1f / 60f); }
                v.transform.position = new Vector3(0, 0, 160f);
            }
            yield return null;
            Shot("pose_cloth_run.png", new Vector3(-2.2f, 1.6f, 157.2f), new Vector3(0, 0.9f, 160f));

            // Odmiany wrogów: po sześć egzemplarzy łucznika, ghula i strażnika (różne losy) + halabarda we krwi.
            var bundle = TestContent.Create();
            string[] kinds = { "enemy_archer", "enemy_ghoul", "enemy_warden" };
            for (int row = 0; row < kinds.Length; row++)
            {
                var def = bundle.Get<EnemyDefinition>(kinds[row]);
                for (int i = 0; i < 6; i++)
                {
                    var (v, s) = Dummy(RigLook.Vary(RigLook.ForEnemy(def), 100 + i * 17 + row), new Vector3((i - 2.5f) * 1.7f, 0, 180f + row * 3f), 180f, def.scale);
                    s.state = new CharacterAnimState();
                    for (int k = 0; k < 10; k++) v.Tick(1f / 60f);
                    if (row == 2 && i < 2) v.AddWeaponBlood(1f);
                }
            }
            yield return null;
            Shot("pose_variety.png", new Vector3(0, 3.2f, 169f), new Vector3(0, 1.0f, 182.5f));
        }
    }
}
