using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Natywne unikaty w działającym silniku (rozdz. 28.3): prawdziwy gracz z PlayerController, wrogowie z AI i fizyką, pociski, sługi
    /// i strefy. Każdy test sprawdza rozstrzygający przypadek mocy i jej ograniczenie (budżet raz, brak działania bez warunku).
    /// </summary>
    public class UniquePlayTests
    {
        readonly List<Object> cleanup = new List<Object>();
        DefaultContent.Bundle bundle;

        [SetUp] public void SetUp() { Time.timeScale = 1f; bundle = TestContent.Create(); EnemyBrain.DebugNoEvasion = true; UniqueRegistry.ResetForTests(); }

        [TearDown]
        public void TearDown()
        {
            Servant.DismissAll(); FeatureDecoy.ClearAll(); PinnedShadow.ClearAll(); EarthWall.ClearAll(); HuWorld.Clear();
            foreach (var p in Projectile.Active.ToArray()) if (p != null) Object.Destroy(p.gameObject);
            foreach (var z in DamageZone.Active.ToArray()) if (z != null) Object.Destroy(z.gameObject);
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
            EnemyBrain.DebugNoEvasion = false;
            EnemyBrain.BleedRateProvider = null;
            UniqueMines.HoldContact = null;
        }

        PlayerCombat Hero(string classId, string featureId, string[] uniques, params string[] skills)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(80, 1, 80);
            cleanup.Add(floor);
            var camGo = new GameObject("TestCam");
            camGo.AddComponent<Camera>();
            var rig = camGo.AddComponent<CameraRig>();
            cleanup.Add(camGo);
            var p = WorldBuilder.CreatePlayer();
            cleanup.Add(p);
            var ctrl = p.GetComponent<PlayerController>();
            ctrl.cameraRig = rig; ctrl.InputEnabled = false;
            rig.target = p.transform; rig.player = ctrl;
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(classId), difficulty = bundle.config.difficulties[0], classFeatureId = featureId };
            var run = RunFactory.Create(plan, bundle.config, 7);
            foreach (var u in uniques)
            {
                var it = HellEconomy.Roll(UniqueRegistry.Find(bundle.config, "item.unique.turris." + u), LootSource.Boss, 8, new System.Random(1));
                run.inventory.Add(it);
                Assert.IsTrue(run.EquipFromInventory(it, it.definition.DefaultSlot), u);
            }
            run.SyncGrantedSkills();
            for (int i = 0; i < skills.Length; i++)
            {
                var inst = run.FindSpell(bundle.Get<SpellDefinition>(skills[i])) ?? run.LearnSpell(bundle.Get<SpellDefinition>(skills[i]), true);
                run.AssignSlot(inst, i);
            }
            run.attributes.intelligence = 30; run.attributes.strength = 30; run.attributes.dexterity = 30;
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(run, bundle.config);
            pc.DebugNoPassiveBlock = true;
            pc.DebugInvulnerable = true;
            p.GetComponent<PlayerVisuals>().Refresh();
            return pc;
        }

        EnemyBrain Enemy(PlayerCombat pc, Vector3 pos, float hp = 5000f, float speed = 0f, bool attacks = false, float poise = 100000f)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = "Manekin"; def.maxHealth = hp; def.maxPoise = poise; def.scale = 1f; def.moveSpeed = speed; def.strafeChance = 0f;
            def.preferredRange = 1.4f; def.attackInterval = new Vector2(0.2f, 0.3f);
            if (attacks) def.attacks.Add(new EnemyAttackEntry { attack = new AttackDefinition { name = "Pazur", baseDamage = 20f, reach = 2f, radius = 0.9f, windup = 0.3f, lunge = 0f }, maxRange = 3f });
            cleanup.Add(def);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(pc.transform.position - pos);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            return e;
        }

        static void Face(PlayerCombat pc, EnemyBrain e) { Vector3 d = e.transform.position - pc.transform.position; d.y = 0; pc.transform.rotation = Quaternion.LookRotation(d); pc.LockTarget = e; }
        static float Lost(EnemyBrain e) => e.Health.Max - e.Health.Current;
        static string Diag(PlayerCombat pc, int slot)
        {
            var sk = pc.Skill(slot);
            return sk == null ? "slot pusty" : $"{sk.Def.id} hu={sk.instance.seals != null} eq={sk.equipmentMet} req={sk.requirementsMet} ready={sk.instance.Ready} usable={pc.SkillUsable(slot)} mana={pc.Mana.Current:0}/{sk.manaCost:0} st={pc.Stamina.Current:0} moc={string.Join(",", pc.Uniques.All.Select(u => u.Card.code + ":" + (u.Dormant() ?? "ok")))}";
        }
        IEnumerator Wait(float s) { yield return new WaitForSeconds(s); }

        // ================================================================== W05 Odwrót szarży

        [UnityTest]
        public IEnumerator EmptyFront_ChargeRetreatsAwayFromTarget_HitsChaserOnce_NoEndBlast()
        {
            var pc = Hero("class_knight", null, new[] { "warrior.empty_front" }, "hu_charge");
            var e = Enemy(pc, new Vector3(0, 0.05f, 2.2f));
            yield return Wait(0.1f);
            Face(pc, e);
            float z0 = pc.transform.position.z;
            pc.RequestSkill(0);
            yield return Wait(1.0f);
            Assert.Less(pc.transform.position.z, z0 - 1.5f, "szarża od celu (odwrót)");
            Assert.AreEqual(1f, pc.UniqueTelemetry.Get("W05: odwroty"), 0.01f);
            Assert.Greater(Lost(e), 0f, "ścigający na trasie dostał taran");
            float after = Lost(e);
            yield return Wait(0.5f);
            Assert.AreEqual(after, Lost(e), 0.01f, "brak końcowego wybuchu i drugiego trafienia");
        }

        // ================================================================== M03 Nova jako mina

        [UnityTest]
        public IEnumerator SleepingCircles_NovaBecomesArmedMine_DetonatesOnEnemyOnce()
        {
            var pc = Hero("class_mage", null, new[] { "mage.sleeping_circles" }, "hu_nova_fire");
            var e = Enemy(pc, new Vector3(0, 0.05f, 4f));
            yield return Wait(0.1f);
            Face(pc, e);
            pc.RequestSkill(0);
            yield return Wait(0.35f);
            Assert.AreEqual(1f, pc.UniqueTelemetry.Get("M03: miny"), 0.01f, "nova położona jako mina");
            Assert.AreEqual(0f, Lost(e), 0.01f, "bez natychmiastowego wybuchu");
            yield return Wait(1.5f);
            Assert.Greater(Lost(e), 0f, "uzbrojona mina wybuchła przy wrogu");
            Assert.AreEqual(1f, pc.UniqueTelemetry.Get("M03: miny odpalone"), 0.01f);
            yield return Wait(1.2f);
            Assert.AreEqual(1f, pc.UniqueTelemetry.Get("M03: miny odpalone"), 0.01f, "jeden wybuch – mina zużyta (dalsze obrażenia to podpalenie novy)");
            Assert.AreEqual(0, pc.Uniques.Get<SleepingCirclesPower>().Mines);
        }

        // ================================================================== H03 Kokony na sługach

        [UnityTest]
        public IEnumerator SilentGlacier_NovaSplitsIntoCocoons_ReleasedByServantAttack()
        {
            var pc = Hero("class_hu_summoner", null, new[] { "water.silent_glacier" }, "hu_nova_water");
            var e = Enemy(pc, new Vector3(0, 0.05f, 3f));
            var hit = pc.FeatureHitForTest(5f);
            var s1 = Servant.Spawn(pc, ServantKind.Hound, 20f, 1f, () => hit, Color.green);
            var s2 = Servant.Spawn(pc, ServantKind.Hound, 20f, 1f, () => hit, Color.green);
            yield return Wait(0.1f);
            Face(pc, e);
            string diag = Diag(pc, 0);
            pc.RequestSkill(0);
            yield return Wait(0.8f);
            Assert.AreEqual(2f, pc.UniqueTelemetry.Get("H03: kokony"), 0.01f, "budżet na dwa kokony; " + diag);
            yield return Wait(2.5f);
            Assert.GreaterOrEqual(pc.UniqueTelemetry.Get("H03: kokony uwolnione"), 1f, "atak sługi uwolnił porcję");
            Assert.LessOrEqual(pc.UniqueTelemetry.Get("H03: kokony uwolnione"), 2f, "nie więcej niż kokonów");
        }

        // ================================================================== S03 Komendy sług

        [UnityTest]
        public IEnumerator BoneConductor_ServantsWaitForOwnersHits()
        {
            var pc = Hero("class_hu_summoner", null, new[] { "summoner.bone_conductor" });
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.6f));
            var hit = pc.FeatureHitForTest(5f);
            var s = Servant.Spawn(pc, ServantKind.Hound, 20f, 0.6f, () => hit, Color.green);
            s.transform.position = e.transform.position + Vector3.right * 1.0f;
            yield return Wait(2.0f);
            Assert.AreEqual(0, s.Attacks, "bez trafień właściciela sługa nie atakuje");
            Face(pc, e);
            pc.Request(ActionType.LightAttack);
            yield return Wait(1.0f);
            Assert.GreaterOrEqual(s.Attacks, 1, "trafienie właściciela wydało komendę");
            Assert.LessOrEqual(s.Attacks, 2, "jedna akcja – ograniczona kolejka");
        }

        // ================================================================== F04 Odroczone podpalenie

        [UnityTest]
        public IEnumerator DeferredWildfire_BurnsWaitThenHeavyCompressesIntoPillar()
        {
            var pc = Hero("class_hu_mage_fire", null, new[] { "fire.deferred_wildfire" });
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.6f));
            yield return Wait(0.1f);
            // Zwykłe trafienie ogniem z podpaleniem przez pełny potok wyjścia gracza (moc Szaty działa na końcu potoku).
            var real = new HitData { magic = 40f, element = Element.Fire, blockable = true, dodgeable = true, sourcePosition = pc.transform.position, attacker = pc,
                attackName = "Test", mods = StatusModifiers.None, statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Burn) } };
            pc.PrepareOutgoingForTest(ref real);
            Assert.IsTrue(real.mods.deferBurn, "podpalenie odroczone w potoku");
            HitQuery.Apply(pc, e, real, false);
            float afterHit = Lost(e);
            Assert.IsFalse(e.Status.Burning, "podpalenie nie tyka");
            Assert.Greater(e.Status.DeferredBurn, 0f, "budżet czeka");
            yield return Wait(1.0f);
            Assert.AreEqual(afterHit, Lost(e), 0.5f, "brak tyknięć");
            Face(pc, e);
            pc.Request(ActionType.HeavyAttack);
            yield return Wait(1.5f);
            Assert.GreaterOrEqual(pc.UniqueTelemetry.Get("F04: słupy"), 1f, "mocny atak ścisnął budżet");
            Assert.AreEqual(0f, e.Status.DeferredBurn, 0.01f, "budżet wydany raz");
        }

        // ================================================================== E03 Kolce pod ziemią

        [UnityTest]
        public IEnumerator UndugGrave_SpikesWaitUnderground_HammerHeavyReleasesOnce()
        {
            var pc = Hero("class_hu_mage_earth", null, new[] { "earth.undug_grave" }, "hu_stone_spikes");
            var e = Enemy(pc, new Vector3(0, 0.05f, 4f));
            yield return Wait(0.1f);
            Face(pc, e);
            pc.RequestSkill(0);
            yield return Wait(1.2f);
            Assert.AreEqual(1f, pc.UniqueTelemetry.Get("E03: znaki"), 0.01f);
            Assert.AreEqual(0f, Lost(e), 0.01f, "kolce nie uderzyły od razu");
            pc.Request(ActionType.HeavyAttack);
            yield return Wait(1.6f);
            Assert.Greater(Lost(e), 0f, "mocny atak młotem wyzwolił linię");
            Assert.AreEqual(0, pc.Uniques.Get<UndugGravePower>().Marks, "znak zużyty");
        }

        // ================================================================== L03 Przyklejona bomba

        [UnityTest]
        public IEnumerator Duds_BombSticks_NoBlast_SnareOnCarrierDetonates()
        {
            var pc = Hero("class_rogue", null, new[] { "rogue.duds" }, "hu_sulfur_bomb", "hu_snare");
            var dagger = new ItemInstance(bundle.Get<ItemDefinition>("hu_dagger")); // Bomba i Sidła wymagają sztyletu
            pc.Run.inventory.Add(dagger); pc.Run.EquipFromInventory(dagger, EquipSlot.MainHand); pc.RefreshBuild();
            var e = Enemy(pc, new Vector3(0, 0.05f, 4f));
            yield return Wait(0.1f);
            Face(pc, e);
            string diag = Diag(pc, 0);
            pc.RequestSkill(0);
            yield return Wait(1.2f);
            var duds = pc.Uniques.Get<DudsPower>();
            Assert.AreEqual(1, duds.Bombs, "bomba przyklejona; " + diag);
            float before = Lost(e);
            pc.RequestSkill(1);
            yield return Wait(1.0f);
            Assert.AreEqual(0, duds.Bombs, "Sidła odpaliły ładunek");
            Assert.Greater(Lost(e), before, "wybuch zranił nosiciela");
            Assert.GreaterOrEqual(pc.UniqueTelemetry.Get("L03: detonacje: Sidła"), 1f);
        }

        // ================================================================== A03 Krok wichru steruje pociskami

        [UnityTest]
        public IEnumerator WindlessNight_WindStepRedirectsOwnProjectiles_PlayerStays()
        {
            var pc = Hero("class_hu_mage_air", null, new[] { "air.windless_night" }, "hu_wind_step");
            var e = Enemy(pc, new Vector3(4f, 0.05f, 0f));
            yield return Wait(0.1f);
            var h = pc.FeatureHitForTest(0f, 20f, Element.Lightning);
            var p = Projectile.Spawn(pc.transform.position + Vector3.up + Vector3.forward, Vector3.forward, 6f, h, Faction.Player, pc, Color.white, 0.2f, 3f);
            Face(pc, e);
            Vector3 start = pc.transform.position;
            pc.RequestSkill(0);
            yield return Wait(0.4f);
            Assert.Less(Vector3.Distance(start, pc.transform.position), 0.6f, "postać nie doskoczyła");
            Assert.GreaterOrEqual(pc.UniqueTelemetry.Get("A03: pokierowane pociski"), 1f);
            yield return Wait(1.0f);
            Assert.Greater(Lost(e), 0f, "pokierowany pocisk trafił cel z boku");
        }

        // ================================================================== B03 Rany płyną z ruchem właściciela

        [UnityTest]
        public IEnumerator WalkingSlaughter_BleedFasterWhenOwnerMovesNearVictim()
        {
            var pc = Hero("class_hu_berserker", null, new[] { "berserker.walking_slaughter" });
            var still = Enemy(pc, new Vector3(0, 0.05f, 2f));
            yield return Wait(0.1f);
            still.Status.Apply(StatusKind.Bleed, 200f, 0f, bundle.config.balance);
            float budget = still.Status.BleedRemainingDamage;
            yield return Wait(1.0f);
            float standing = budget - still.Status.BleedRemainingDamage;
            still.Status.Clear();
            still.Status.Apply(StatusKind.Bleed, 200f, 0f, bundle.config.balance);
            float budget2 = still.Status.BleedRemainingDamage;
            var cc = pc.GetComponent<CharacterController>();
            for (float t = 0; t < 1.0f; t += Time.deltaTime)
            {
                cc.Move(new Vector3(Mathf.Sin(t * 6f), 0, Mathf.Cos(t * 6f)) * 3f * Time.deltaTime); // krążenie wokół ofiary
                yield return null;
            }
            float moving = budget2 - still.Status.BleedRemainingDamage;
            TestContext.WriteLine($"stojąc {standing:0.0}, w ruchu {moving:0.0}");
            Assert.Greater(moving, standing * 2f, "ruch właściciela przy ofierze przyspiesza ranę");
            Assert.LessOrEqual(moving, budget2 + 0.01f, "suma rany skończona");
        }

        // ================================================================== T05 Lekki atak różdżki jako ostrze

        [UnityTest]
        public IEnumerator BrokenWand_LightAttackIsMeleeBlade_NoProjectile()
        {
            var pc = Hero("class_hu_battlemage", null, new[] { "battlemage.broken_wand" });
            var wand = new ItemInstance(bundle.Get<ItemDefinition>("hu_wand_fire"));
            pc.Run.inventory.Add(wand); pc.Run.EquipFromInventory(wand, EquipSlot.MainHand);
            pc.RefreshBuild();
            Assert.IsNull(pc.Uniques.Get<BrokenWandPower>().Dormant(), "różdżka w głównej, grymuar w drugiej");
            Assert.IsNotNull(pc.Uniques.Get<BrokenWandPower>(), "grymuar założony");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.6f));
            yield return Wait(0.1f);
            Face(pc, e);
            int shots = Projectile.Active.Count;
            pc.Request(ActionType.LightAttack);
            yield return Wait(0.8f);
            Assert.AreEqual(shots, Projectile.Active.Count, "bez niewidzialnego pocisku");
            Assert.Greater(Lost(e), 0f, "cięcie ostrzem trafiło w zwarciu");
            Assert.GreaterOrEqual(pc.UniqueTelemetry.Get("T05: cięcia ostrzem"), 1f);
        }

        // ================================================================== Z03 Rana w pułapce

        [UnityTest]
        public IEnumerator GraveNeedle_HeavyPullsBleedIntoTrap_TransfersToOtherEnemyAtomically()
        {
            var pc = Hero("class_hu_assassin", null, new[] { "assassin.grave_needle" });
            var first = Enemy(pc, new Vector3(0, 0.05f, 1.6f));
            yield return Wait(0.1f);
            first.Status.Apply(StatusKind.Bleed, 300f, 0f, bundle.config.balance);
            Face(pc, first);
            pc.Request(ActionType.HeavyAttack);
            yield return Wait(1.0f);
            var needle = pc.Uniques.Get<GraveNeedlePower>();
            Assert.AreEqual(1, needle.Traps, "pułapka z rany");
            float moved = pc.UniqueTelemetry.Get("Z03: budżet wyciągnięty");
            Assert.Greater(moved, 0f);
            var second = Enemy(pc, first.transform.position + Vector3.right * 0.3f);
            yield return Wait(0.3f);
            Assert.AreEqual(0, needle.Traps, "kontakt innego wroga zabrał ranę");
            Assert.Greater(second.Status.BleedRemainingDamage, 0f, "rana na nowym celu");
            Assert.LessOrEqual(second.Status.BleedRemainingDamage, moved + 0.5f, "bez klonu i wzrostu");
        }

        // ================================================================== K04 Rząd strzał

        [UnityTest]
        public IEnumerator UnfinishedVolley_PlantsRow_HeavyShotFiresItOnce()
        {
            var pc = Hero("class_hu_archer", null, new[] { "archer.unfinished_volley" }, "hu_volley");
            var e = Enemy(pc, new Vector3(0, 0.05f, 9f));
            yield return Wait(0.1f);
            Face(pc, e);
            pc.RequestSkill(0);
            yield return Wait(0.8f);
            var v = pc.Uniques.Get<UnfinishedVolleyPower>();
            Assert.Greater(v.Planted, 0, "rząd w ziemi");
            Assert.AreEqual(0f, Lost(e), 0.01f, "rząd nie strzela sam");
            int planted = v.Planted;
            pc.Request(ActionType.HeavyAttack);
            yield return Wait(1.6f);
            Assert.AreEqual(0, v.Planted, "rząd wystrzelony raz");
            Assert.AreEqual(planted, pc.UniqueTelemetry.Get("K04: strzały rzędu"), 0.01f);
            Assert.Greater(Lost(e), 0f, "strzały rzędu trafiły");
        }
    }
}
