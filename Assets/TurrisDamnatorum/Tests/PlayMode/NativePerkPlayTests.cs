using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>Perki specjalizacyjne w działającym silniku (rozdz. 11 – PlayMode): prawdziwy gracz, AI wroga, sługi i strefy.</summary>
    public class NativePerkPlayTests
    {
        readonly List<Object> cleanup = new List<Object>();
        DefaultContent.Bundle bundle;

        [SetUp] public void SetUp() { Time.timeScale = 1f; bundle = TestContent.Create(); EnemyBrain.DebugNoEvasion = true; NativePerkCatalog.ResetForTests(); }

        [TearDown]
        public void TearDown()
        {
            Servant.DismissAll(); HuWorld.Clear();
            foreach (var p in Projectile.Active.ToArray()) if (p != null) Object.Destroy(p.gameObject);
            foreach (var z in DamageZone.Active.ToArray()) if (z != null) Object.Destroy(z.gameObject);
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            EnemyBrain.DebugNoEvasion = false;
            GameRoot.ProfileFileOverride = null;
        }

        PlayerCombat Hero(string cls, string feature, string main, string off, string[] perks, params string[] skills)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0); floor.transform.localScale = new Vector3(80, 1, 80); cleanup.Add(floor);
            var camGo = new GameObject("TestCam"); camGo.AddComponent<Camera>(); var rig = camGo.AddComponent<CameraRig>(); cleanup.Add(camGo);
            var p = WorldBuilder.CreatePlayer(); cleanup.Add(p);
            var ctrl = p.GetComponent<PlayerController>(); ctrl.cameraRig = rig; ctrl.InputEnabled = false; rig.target = p.transform; rig.player = ctrl;
            var run = RunFactory.Create(new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(cls), difficulty = bundle.config.difficulties[0], classFeatureId = feature }, bundle.config, 7);
            run.attributes.intelligence = 30; run.attributes.strength = 20; run.attributes.dexterity = 20;
            run.equipment.Unequip(EquipSlot.MainHand); run.equipment.Unequip(EquipSlot.OffHand);
            void Eq(string id, EquipSlot s) { if (id == null) return; var it = new ItemInstance(bundle.Get<ItemDefinition>(id)); run.inventory.Add(it); Assert.IsTrue(run.EquipFromInventory(it, s), id); }
            Eq(main, EquipSlot.MainHand); Eq(off, EquipSlot.OffHand);
            run.SyncGrantedSkills();
            for (int i = 0; i < RunState.SkillSlotCount; i++) run.ClearSlot(i);
            for (int i = 0; i < skills.Length; i++) { var d = bundle.Get<SpellDefinition>(skills[i]); run.AssignSlot(run.FindSpell(d) ?? run.LearnSpell(d, true), i); }
            foreach (var id in perks) run.AddBoon(NativePerkCatalog.Get(bundle.config, id));
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(run, bundle.config);
            pc.DebugNoPassiveBlock = true; pc.Immortal = true;
            p.GetComponent<PlayerVisuals>().Refresh();
            return pc;
        }

        EnemyBrain Enemy(PlayerCombat pc, Vector3 pos, float poise = 100000f, bool attacks = false, float speed = 0f)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = "Manekin"; def.maxHealth = 50000f; def.maxPoise = poise; def.scale = 1f; def.moveSpeed = speed; def.strafeChance = 0f;
            def.preferredRange = 1.4f; def.attackInterval = new Vector2(0.3f, 0.5f);
            if (attacks) def.attacks.Add(new EnemyAttackEntry { attack = new AttackDefinition { name = "Pazur", baseDamage = 20f, reach = 2.2f, radius = 1f, windup = 0.35f, lunge = 0f }, maxRange = 3f });
            cleanup.Add(def);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(pc.transform.position - pos);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            return e;
        }

        static void Face(PlayerCombat pc, EnemyBrain e) { Vector3 d = e.transform.position - pc.transform.position; d.y = 0; pc.transform.rotation = Quaternion.LookRotation(d); pc.LockTarget = e; }
        static float P(PlayerCombat pc, string k) => pc.PerkTelemetry.TryGetValue(k, out var v) ? v : 0f;

        // P01 – istniejący GameConfig (asset), bez regeneracji
        [UnityTest]
        public IEnumerator RealConfig_ShopOffersAndSellsNativePerks_SaveRoundTrip()
        {
            GameRoot.ProfileFileOverride = $"turris_perk_{System.Guid.NewGuid():N}.json";
            SceneManager.LoadScene("Main");
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameRoot>();
            Assert.IsNotNull(root);
            Assert.AreEqual(8, NativePerkCatalog.Boons(root.config).Count);
            var plan = new LoadoutPlan { classDef = root.config.classes.First(c => c.id == "class_knight"), difficulty = root.config.difficulties[0] };
            var run = RunFactory.Create(plan, root.config, 3);
            run.equipment.Unequip(EquipSlot.OffHand);
            run.floorsCleared = 4; run.souls = 99999;
            var seen = new HashSet<string>();
            for (int s = 0; s < 400; s++) foreach (var o in FloorShop.Generate(run, root.config, new System.Random(s))) if (o.perk != null && NativePerkCatalog.IsNative(o.perk)) seen.Add(o.perk.id);
            TestContext.WriteLine("oferowane natywne: " + string.Join(", ", seen));
            Assert.IsTrue(seen.Contains(NativePerkCatalog.Duelist) && seen.Contains(NativePerkCatalog.Breach), "sklep oferuje perki zgodne z buildem");
            Assert.IsFalse(seen.Contains(NativePerkCatalog.Pack), "bez przywołań – bez Sfory");
            Assert.IsFalse(seen.Contains(NativePerkCatalog.LongBreath) || seen.Contains(NativePerkCatalog.LightStep), "perki Łotrzyka nie dla Wojownika");
            // Rodzina Łotrzyka w tym samym sklepie.
            var rogue = RunFactory.Create(new LoadoutPlan { classDef = root.config.classes.First(c => c.id == "class_rogue"), difficulty = root.config.difficulties[0] }, root.config, 3);
            rogue.floorsCleared = 4; rogue.souls = 99999;
            var rogueSeen = new HashSet<string>();
            for (int s = 0; s < 400; s++) foreach (var o in FloorShop.Generate(rogue, root.config, new System.Random(s))) if (o.perk != null && NativePerkCatalog.IsNative(o.perk)) rogueSeen.Add(o.perk.id);
            TestContext.WriteLine("oferowane Łotrzykowi: " + string.Join(", ", rogueSeen));
            Assert.IsTrue(rogueSeen.Contains(NativePerkCatalog.LongBreath), "Zasadzka – Długi oddech w ofercie");
            var offer = Enumerable.Range(0, 400).SelectMany(s => FloorShop.Generate(run, root.config, new System.Random(s))).First(o => o.perk != null && o.perk.id == NativePerkCatalog.Duelist);
            Assert.IsTrue(FloorShop.Buy(offer, run));
            var back = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(RunSnapshot.Capture(run))).Restore(root.config);
            Assert.AreEqual(1, back.BoonStacks(NativePerkCatalog.Get(root.config, NativePerkCatalog.Duelist)));
            string path = System.IO.Path.Combine(Application.persistentDataPath, GameRoot.ProfileFileOverride);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            // Scena Main nie może zostać na kolejne testy.
            var mainScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(SceneManager.CreateScene("PoTesciePerkow"));
            yield return SceneManager.UnloadSceneAsync(mainScene);
        }

        // P02 – Pojedynkowicz: czas serii, koszt, tarcza wyłącza, inne akcje bez zmian
        [UnityTest]
        public IEnumerator Duelist_FasterLightAttack_SameStaminaPerSwing_ShieldDisables()
        {
            float Measure(PlayerCombat pc, out float stamina)
            {
                float st = pc.Stamina.Current;
                pc.Request(ActionType.LightAttack);
                stamina = st - pc.Stamina.Current;
                return pc.AttackWindup + pc.AttackActive;
            }
            var with = Hero("class_knight", null, "hu_sword", null, new[] { NativePerkCatalog.Duelist });
            var without = Hero("class_knight", null, "hu_sword", null, new string[0]);
            yield return null;
            float tw = Measure(with, out float sw), to = Measure(without, out float so);
            TestContext.WriteLine($"zamach+aktywna: z perkiem {tw:0.000} s, bez {to:0.000} s; wytrzymałość {sw:0.0}/{so:0.0}");
            Assert.Less(tw, to * 0.95f, "szybszy kwalifikowany atak");
            Assert.AreEqual(so, sw, 0.01f, "ten sam koszt jednego ataku");
            Assert.GreaterOrEqual(P(with, "pojedynkowicz: ataki"), 1f);
            var shielded = Hero("class_knight", null, "hu_sword", "hu_shield", new[] { NativePerkCatalog.Duelist });
            yield return null;
            Assert.AreEqual(to, Measure(shielded, out _), 0.005f, "tarcza wyłącza premię");
        }

        // P04 – rzeczywiste przełamanie ciosem → premia w nowej akcji, raz
        [UnityTest]
        public IEnumerator Breach_RealPoiseBreak_NextNewActionGetsBonusOnce()
        {
            var pc = Hero("class_knight", null, "hu_sword", null, new[] { NativePerkCatalog.Breach });
            // Słaby wróg (postawa poniżej progu) łamie się, ale okna nie daje.
            var weak = Enemy(pc, new Vector3(0, 0.05f, 1.5f), poise: 20f);
            yield return new WaitForSeconds(1.2f);
            Face(pc, weak);
            float t = 0f;
            while (t < 4f && P(pc, "wyłom: pominięte (słaba postawa)") < 1f) { if (pc.Actions.IsIdle) pc.Request(ActionType.LightAttack); t += Time.deltaTime; yield return null; }
            Assert.GreaterOrEqual(P(pc, "wyłom: pominięte (słaba postawa)"), 1f, "słaby wróg przełamany");
            Assert.IsFalse(pc.HasBreach(weak), "postawa 20 < progu 50 – bez okna");
            while (!pc.Actions.IsIdle) yield return null;
            Object.Destroy(weak.gameObject);
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.5f), poise: 60f);
            yield return new WaitForSeconds(1.2f);
            Face(pc, e);
            t = 0f;
            while (t < 8f && !pc.HasBreach(e)) { if (pc.Actions.IsIdle) pc.Request(ActionType.LightAttack); t += Time.deltaTime; yield return null; }
            Assert.IsTrue(pc.HasBreach(e), "zwykłe przełamanie (Staggered) otworzyło okno");
            Assert.GreaterOrEqual(P(pc, "wyłom: okna z ciosu"), 1f);
            while (!pc.Actions.IsIdle) yield return null; // akcja otwierająca kończy się bez zużycia
            Assert.IsTrue(pc.HasBreach(e), "akcja otwierająca nie zużywa okna");
            pc.Request(ActionType.HeavyAttack);
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(1f, P(pc, "wyłom: okna zużyte"), 0.01f, "jedno wykorzystanie");
            Assert.IsFalse(pc.HasBreach(e));
        }

        // P05 – parowanie → riposta wykorzystuje okno raz
        [UnityTest]
        public IEnumerator Breach_ParryBreak_RiposteConsumesOnce()
        {
            var pc = Hero("class_knight", null, "hu_sword", "hu_shield", new[] { NativePerkCatalog.Breach });
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.8f), poise: 60f, attacks: true);
            yield return new WaitForSeconds(1.2f);
            Face(pc, e);
            float t = 0f; int parried = 0, struck = 0, tele = 0;
            System.Action<IHitReceiver, HitData, HitResult> onImpact = (tg, h, r) => { if (ReferenceEquals(tg, pc)) { if (r.outcome == HitOutcome.Parried) parried++; else struck++; } };
            CombatEvents.Impact += onImpact;
            float teleStart = -1f;
            while (t < 8f && !e.CanBeRiposted)
            {
                pc.RestoreAll();
                if (e.IsTelegraphing) { tele++; if (teleStart < 0f) teleStart = t; } else teleStart = -1f;
                // Parowanie pod koniec zamachu wroga (okno 0,2 s musi objąć cios).
                if (pc.Actions.IsIdle && teleStart >= 0f && t - teleStart >= e.AttackWindupDuration - 0.15f) pc.Request(ActionType.Parry);
                t += Time.deltaTime; yield return null;
            }
            CombatEvents.Impact -= onImpact;
            Assert.IsTrue(e.CanBeRiposted, $"sparowane={parried} trafienia={struck} klatki zapowiedzi={tele} parowanie={pc.Build.parry != null} stan={e.CurrentState}");
            Assert.GreaterOrEqual(P(pc, "wyłom: okna z parowania"), 1f, "parowanie gracza (jawne pochodzenie) otworzyło okno");
            while (!pc.Actions.IsIdle) yield return null;
            Face(pc, e);
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(0.8f);
            Assert.AreEqual(1f, P(pc, "wyłom: riposty"), 0.01f, "riposta wykorzystała okno");
            Assert.AreEqual(1f, P(pc, "wyłom: okna zużyte"), 0.01f);
        }

        // Otwarta rana: premia rośnie z warstwami do limitu, działa w prawdziwym ciosie
        [UnityTest]
        public IEnumerator OpenWound_ScalesWithBleedStacks_CappedOnRealHit()
        {
            var pc = Hero("class_hu_assassin", null, "hu_dagger_kris", null, new[] { NativePerkCatalog.OpenWound });
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.3f));
            yield return new WaitForSeconds(1.2f);
            Face(pc, e);
            Assert.AreEqual(1f, pc.OpenWoundMultiplier(e), 1e-4f, "bez krwawienia bez premii");
            e.ApplyStatus(StatusKind.Bleed, 100f, 0f);
            Assert.AreEqual(1f + 0.04f * e.Status.BleedStacks, pc.OpenWoundMultiplier(e), 1e-4f, "za warstwę");
            for (int i = 0; i < 6; i++) e.ApplyStatus(StatusKind.Bleed, 100f, 0f);
            TestContext.WriteLine($"warstwy {e.Status.BleedStacks}, mnożnik {pc.OpenWoundMultiplier(e):0.00}");
            Assert.LessOrEqual(pc.OpenWoundMultiplier(e), 1.12f + 1e-4f, "limit");
            pc.Request(ActionType.LightAttack);
            float t = 0f; while (t < 2f && P(pc, "otwarta rana: ciosy") < 1f) { t += Time.deltaTime; yield return null; }
            Assert.GreaterOrEqual(P(pc, "otwarta rana: ciosy"), 1f, "prawdziwy cios dostał premię");
        }

        // Długi oddech: dłuższe okno po uniku tylko z perkiem
        [UnityTest]
        public IEnumerator LongBreath_LongerAmbushWindowAfterDodge()
        {
            float Window(PlayerCombat pc) { pc.MoveIntent = Vector3.back; pc.Request(ActionType.Dodge); pc.MoveIntent = Vector3.zero; return pc.AmbushWindowLeft; }
            var plain = Hero("class_hu_assassin", null, "hu_dagger", null, new string[0]);
            yield return new WaitForSeconds(0.1f);
            float w0 = Window(plain);
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            yield return null;
            var perk = Hero("class_hu_assassin", null, "hu_dagger", null, new[] { NativePerkCatalog.LongBreath });
            yield return new WaitForSeconds(0.1f);
            float w1 = Window(perk);
            TestContext.WriteLine($"okno bez perka {w0:0.00} s, z perkiem {w1:0.00} s");
            Assert.AreEqual(1.5f, w0, 0.05f);
            Assert.AreEqual(2.5f, w1, 0.05f);
        }

        // Lekki krok: tylko pierwszy lekki cios po uniku, bez kumulacji
        [UnityTest]
        public IEnumerator LightStep_FirstLightAfterDodgeHalfCost_OnlyOnce()
        {
            var pc = Hero("class_hu_assassin", null, "hu_dagger", null, new[] { NativePerkCatalog.LightStep });
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.3f));
            yield return new WaitForSeconds(0.3f);
            Face(pc, e);
            float Cost(ActionType a) { pc.Stamina.SetCurrent(pc.Stamina.Max); float s = pc.Stamina.Current; pc.Request(a); return s - pc.Stamina.Current; }
            float full = pc.Build.weapon.light.staminaCost;
            Assert.AreEqual(full, Cost(ActionType.LightAttack), 0.01f, "bez uniku pełny koszt");
            while (!pc.Actions.IsIdle) yield return null;
            pc.MoveIntent = Vector3.back; pc.Request(ActionType.Dodge); pc.MoveIntent = Vector3.zero;
            while (!pc.Actions.IsIdle) yield return null;
            Assert.AreEqual(full * 0.5f, Cost(ActionType.LightAttack), 0.01f, "pierwszy po uniku za połowę");
            while (!pc.Actions.IsIdle) yield return null;
            Assert.AreEqual(full, Cost(ActionType.LightAttack), 0.01f, "drugi już pełny");
        }

        // Daleki strzał: te same strzały w trzech odległościach
        [UnityTest]
        public IEnumerator FarShot_BonusFarPenaltyNear_NeutralBetween()
        {
            var pc = Hero("class_rogue", null, "hu_bow", null, new[] { NativePerkCatalog.FarShot });
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(0.88f, pc.FarShotMultiplier(3f), 1e-4f);
            Assert.AreEqual(1f, pc.FarShotMultiplier(6f), 1e-4f);
            Assert.AreEqual(1.12f, pc.FarShotMultiplier(10f), 1e-4f);
            var e = Enemy(pc, new Vector3(0, 0.05f, 10f));
            yield return new WaitForSeconds(1.2f);
            Face(pc, e);
            pc.Request(ActionType.LightAttack);
            float t = 0f; while (t < 3f && P(pc, "daleki strzał: premia") < 1f) { t += Time.deltaTime; yield return null; }
            Assert.GreaterOrEqual(P(pc, "daleki strzał: premia"), 1f, "prawdziwy strzał z daleka");
            Assert.Greater(e.Health.Max - e.Health.Current, 0f);
        }

        // Riposta: premia ogólna i dodane żywioły (audyt, etap 3), przed nimi omijała cały potok.
        [UnityTest]
        public IEnumerator Riposte_GetsGlobalBonusAndAddedElements()
        {
            var pc = Hero("class_knight", null, "hu_sword", "hu_shield", new string[0]);
            var boon = ScriptableObject.CreateInstance<BoonDefinition>(); cleanup.Add(boon);
            boon.id = boon.name = "test_riposta"; boon.displayName = "Test"; boon.maxStacks = 1;
            boon.modifiers.Add(new StatModifier(StatType.GlobalDamage, 100f));
            boon.modifiers.Add(new StatModifier(StatType.AddedWeaponFire, 100f));
            boon.modifiers.Add(new StatModifier(StatType.FireDamage, 50f));
            pc.Run.AddBoon(boon);
            pc.RefreshBuild();
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.8f), poise: 30f, attacks: true);
            e.Def.fireMultiplier = 0.5f; // odporność celu działa na składnik ognia
            yield return new WaitForSeconds(1.2f);
            Face(pc, e);
            float t = 0f, tele = -1f;
            while (t < 10f && !e.CanBeRiposted)
            {
                pc.RestoreAll();
                if (e.IsTelegraphing) { if (tele < 0f) tele = t; } else tele = -1f;
                if (pc.Actions.IsIdle && tele >= 0f && t - tele >= e.AttackWindupDuration - 0.15f) pc.Request(ActionType.Parry);
                t += Time.deltaTime; yield return null;
            }
            Assert.IsTrue(e.CanBeRiposted, "parowanie przełamało postawę");
            while (!pc.Actions.IsIdle) yield return null;
            Face(pc, e);
            var b = pc.Build;
            float strike = b.weapon.riposteMultiplier * (1f + b.stats[StatType.RiposteDamage] / 100f) * 2f;
            float physical = b.WeaponDamage(b.weapon.light) * strike;
            float fire = b.WeaponBase(b.weapon.light) * strike * 1.0f * 1.5f * 0.5f; // +100% B, premia ognia ×1,5, odporność ×0,5
            float before = e.Health.Current;
            pc.Request(ActionType.LightAttack);
            t = 0f; while (t < 1f && e.Health.Current >= before) { t += Time.deltaTime; yield return null; }
            float lost = before - e.Health.Current;
            TestContext.WriteLine($"riposta: {lost:0.0} (rachunek: fizyczne {physical:0.0} + ogień {fire:0.0})");
            // Rachunek bierze statystyki sprzed ciosu; bezwzględna wartość ±10%, a stosunek składników jest ścisły: lost = fizyczne × (1 + 0,75).
            Assert.Greater(lost, (physical + fire) * 0.9f, "premia ogólna i dodany ogień");
            Assert.Less(lost, (physical + fire) * 1.1f, "bez innych premii (np. egzekucji)");
        }

        // P06 + P07 – sługi nie tworzą wyłomu; sfora rośnie z liczbą rodzajów (w chwili ataku)
        [UnityTest]
        public IEnumerator Pack_ServantHitsScaleWithKinds_ServantsNeverOpenBreach()
        {
            var pc = Hero("class_hu_summoner", null, null, "hu_grimoire_summoning", new[] { NativePerkCatalog.Pack, NativePerkCatalog.Breach }, "hu_summon_imps");
            var e = Enemy(pc, new Vector3(0, 0.05f, 3f), poise: 5f);
            var hits = new List<float>();
            System.Action<IHitReceiver, HitData, HitResult> onImpact = (tg, h, r) => { if (h.servantHit && ReferenceEquals(tg, e)) hits.Add(h.Total); };
            CombatEvents.Impact += onImpact;
            try
            {
                yield return new WaitForSeconds(1.1f);
                var snap = pc.FeatureHitForTest(10f);
                var imp = Servant.Spawn(pc, ServantKind.Imp, 30f, 0.6f, () => snap, Color.green);
                float t = 0f; while (t < 3f && hits.Count == 0) { t += Time.deltaTime; yield return null; }
                Assert.Greater(hits.Count, 0, "chochlik zaatakował");
                float one = hits[0];
                Servant.Spawn(pc, ServantKind.Hound, 30f, 0.6f, () => snap, Color.green).transform.position = e.transform.position + Vector3.left * 8f;
                Servant.Spawn(pc, ServantKind.Golem, 30f, 0.6f, () => snap, Color.green).transform.position = e.transform.position + Vector3.right * 8f;
                hits.Clear();
                t = 0f; while (t < 3f && hits.Count == 0) { t += Time.deltaTime; yield return null; }
                Assert.Greater(hits.Count, 0);
                TestContext.WriteLine($"atak przy 1 rodzaju {one:0.00}, przy 3 rodzajach {hits[0]:0.00}");
                Assert.AreEqual(1.3f, hits[0] / one, 0.02f, "K=3 → +30% w ataku sługi");
                Assert.AreEqual(0f, P(pc, "wyłom: okna z ciosu"), "przełamanie przez sługę nie tworzy wyłomu");
            }
            finally { CombatEvents.Impact -= onImpact; }
        }

        // P09 – strefy: Krąg żaru – ta sama liczba tyknięć w połowie czasu; technika bez zmian
        [UnityTest]
        public IEnumerator Manifestation_EmberRingSameTicksHalfTime_TechniqueZoneUnchanged()
        {
            IEnumerator Cast(bool perk, string skill, string main, System.Action<int, float> done)
            {
                var pc = Hero(skill == "hu_snare" ? "class_rogue" : "class_hu_mage_fire", null, main, null, perk ? new[] { NativePerkCatalog.Manifestation } : new string[0], skill);
                yield return new WaitForSeconds(0.1f);
                int before = DamageZone.TotalTicks; float start = Time.time;
                pc.RequestSkill(0);
                DamageZone zone = null;
                float t = 0f; while (t < 2f && zone == null) { zone = DamageZone.Active.FirstOrDefault(z => z != null && ReferenceEquals(z.Owner, pc)); t += Time.deltaTime; yield return null; }
                Assert.IsNotNull(zone, skill + ": strefa");
                float born = Time.time;
                while (zone != null) yield return null;
                done(DamageZone.TotalTicks - before, Time.time - born);
                foreach (var o in cleanup) if (o != null) Object.Destroy(o);
                cleanup.Clear();
                yield return null;
            }
            int tPlain = 0, tPerk = 0, tSnare = 0, tSnarePerk = 0; float dPlain = 0, dPerk = 0, dSnare = 0, dSnarePerk = 0;
            yield return Cast(false, "hu_ember_ring", "hu_wand_fire", (n, d) => { tPlain = n; dPlain = d; });
            yield return Cast(true, "hu_ember_ring", "hu_wand_fire", (n, d) => { tPerk = n; dPerk = d; });
            yield return Cast(false, "hu_snare", "hu_dagger", (n, d) => { tSnare = n; dSnare = d; });
            yield return Cast(true, "hu_snare", "hu_dagger", (n, d) => { tSnarePerk = n; dSnarePerk = d; });
            TestContext.WriteLine($"Krąg żaru: {tPlain} tyknięć w {dPlain:0.00} s → {tPerk} w {dPerk:0.00} s; Sidła: {tSnare}/{dSnare:0.00} → {tSnarePerk}/{dSnarePerk:0.00}");
            Assert.AreEqual(tPlain, tPerk, "te same tyknięcia");
            Assert.AreEqual(dPlain * 0.5f, dPerk, 0.15f, "połowa czasu");
            Assert.AreEqual(tSnare, tSnarePerk, "technika – bez zmian");
            Assert.AreEqual(dSnare, dSnarePerk, 0.15f, "technika – ten sam czas");
        }
    }
}
