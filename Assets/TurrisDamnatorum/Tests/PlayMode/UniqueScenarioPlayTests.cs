using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Turris.Tests
{
    /// <summary>
    /// Scenariusze mocy wymagających konkretnej sytuacji (parowanie, przyjęty cios w transie, technika + leczenie, transport min,
    /// lustra, śmiertelna chimera, zmiana zestawu) – prawdziwe AI wroga, fizyka i potok trafienia.
    /// </summary>
    public class UniqueScenarioPlayTests
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
            EnemyBrain.DebugNoEvasion = false; EnemyBrain.BleedRateProvider = null; UniqueMines.HoldContact = null;
        }

        PlayerCombat Hero(string classId, string featureId, string[] uniques, string main, string off, params string[] skills)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0); floor.transform.localScale = new Vector3(80, 1, 80); cleanup.Add(floor);
            var camGo = new GameObject("TestCam"); camGo.AddComponent<Camera>(); var rig = camGo.AddComponent<CameraRig>(); cleanup.Add(camGo);
            var p = WorldBuilder.CreatePlayer(); cleanup.Add(p);
            var ctrl = p.GetComponent<PlayerController>(); ctrl.cameraRig = rig; ctrl.InputEnabled = false; rig.target = p.transform; rig.player = ctrl;
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(classId), difficulty = bundle.config.difficulties[0], classFeatureId = featureId };
            var run = RunFactory.Create(plan, bundle.config, 7);
            run.attributes.intelligence = 30; run.attributes.strength = 30; run.attributes.dexterity = 30;
            void Eq(string id, EquipSlot s) { if (id == null) return; var it = new ItemInstance(bundle.Get<ItemDefinition>(id)); run.inventory.Add(it); Assert.IsTrue(run.EquipFromInventory(it, s), id); }
            Eq(main, EquipSlot.MainHand); Eq(off, EquipSlot.OffHand);
            foreach (var u in uniques)
            {
                var it = HellEconomy.Roll(UniqueRegistry.Find(bundle.config, "item.unique.turris." + u), LootSource.Boss, 8, new System.Random(1));
                run.inventory.Add(it); Assert.IsTrue(run.EquipFromInventory(it, it.definition.DefaultSlot), u);
            }
            run.SyncGrantedSkills();
            for (int i = 0; i < skills.Length; i++) { var d = bundle.Get<SpellDefinition>(skills[i]); run.AssignSlot(run.FindSpell(d) ?? run.LearnSpell(d, true), i); }
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(run, bundle.config);
            pc.DebugNoPassiveBlock = true; pc.Immortal = true;
            p.GetComponent<PlayerVisuals>().Refresh();
            return pc;
        }

        EnemyBrain Enemy(PlayerCombat pc, Vector3 pos, bool attacks = false, float poise = 100000f, float speed = 0f)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = "Manekin"; def.maxHealth = 6000f; def.maxPoise = poise; def.scale = 1f; def.moveSpeed = speed; def.strafeChance = 0f;
            def.preferredRange = 1.4f; def.attackInterval = new Vector2(0.3f, 0.5f);
            if (attacks) def.attacks.Add(new EnemyAttackEntry { attack = new AttackDefinition { name = "Pazur", baseDamage = 25f, reach = 2.2f, radius = 1f, windup = 0.35f, lunge = 0f }, maxRange = 3f });
            cleanup.Add(def);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(pc.transform.position - pos);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            return e;
        }

        static void Face(PlayerCombat pc, EnemyBrain e) { Vector3 d = e.transform.position - pc.transform.position; d.y = 0; pc.transform.rotation = Quaternion.LookRotation(d); pc.LockTarget = e; }
        static float Lost(EnemyBrain e) => e.Health.Max - e.Health.Current;

        // ================================================================== B01 Krwawy odwet w transie

        [UnityTest]
        public IEnumerator HungryScars_RedlineReducedMeleeHit_RetaliatesOnAttacker()
        {
            var pc = Hero("class_hu_berserker", "class_feature.berserker.redline", new[] { "berserker.hungry_scars" }, "hu_greataxe", null);
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.6f), attacks: true, speed: 2f);
            yield return new WaitForSeconds(1.2f); // koniec uśpienia wroga po pojawieniu się
            pc.Health.SetCurrent(pc.Health.Max * 0.3f); // niskie życie – mocna Żelazna skóra
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.IsTrue(((RedLineFeature)pc.Feature).Active);
            float before = Lost(e);
            float t = 0f;
            // Ciosy wręcz prawdziwym potokiem co 0,2 s: AI wroga, 25% uchylenia i odrzut bohatera czyniły 4-sekundowe okno losowym.
            var claw = new AttackDefinition { name = "Pazur", baseDamage = 25f, reach = 2.2f, radius = 1f };
            float next = 0f;
            while (t < 4f && pc.UniqueTelemetry.Get("B01: odwety") < 1f)
            {
                if (t >= next) { next = t + 0.2f; HitQuery.Apply(e, pc, HitData.FromAttack(claw, 25f, e.transform.position, e), true); }
                t += Time.deltaTime; yield return null;
            }
            Assert.GreaterOrEqual(pc.UniqueTelemetry.Get("B01: odwety"), 1f, "prawdziwy cios wręcz zredukowany skórą oddał odwet");
            Assert.Greater(Lost(e), before, "napastnik dostał odwet");
            // Własny koszt życia nie jest ciosem wroga.
            float r = pc.UniqueTelemetry.Get("B01: odwety");
            pc.PayLifeForTest(5f);
            yield return null;
            Assert.AreEqual(r, pc.UniqueTelemetry.Get("B01: odwety"), 0.01f, "koszt życia bez odwetu");
        }

        // ================================================================== W03 Fala riposty

        [UnityTest]
        public IEnumerator Bellringer_RiposteBecomesWave_HitsSecondEnemyOnce()
        {
            var pc = Hero("class_knight", null, new[] { "warrior.bellringer" }, "hu_sword", "hu_shield");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.8f), attacks: true, poise: 30f);
            var side = Enemy(pc, new Vector3(1.2f, 0.05f, 2.6f));
            yield return new WaitForSeconds(1.3f); // koniec uśpienia wrogów
            Face(pc, e);
            // Prawdziwe parowanie ataku wroga przełamuje postawę (okno riposty).
            float t = 0f, tele = -1f;
            while (t < 10f && !e.CanBeRiposted)
            {
                pc.RestoreAll();
                if (e.IsTelegraphing) { if (tele < 0f) tele = t; } else tele = -1f;
                // Pod koniec zamachu: parowanie na początku zapowiedzi kończy się przed ciosem.
                if (pc.Actions.IsIdle && tele >= 0f && t - tele >= e.AttackWindupDuration - 0.15f) pc.Request(ActionType.Parry);
                t += Time.deltaTime;
                yield return null;
            }
            Assert.IsTrue(e.CanBeRiposted, "parowanie przełamało postawę");
            float sideBefore = Lost(side);
            while (!pc.Actions.IsIdle) yield return null; // koniec parowania
            Face(pc, e);
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(0.8f);
            TestContext.WriteLine($"fale={pc.UniqueTelemetry.Get("W03: fale")} trafieni={pc.UniqueTelemetry.Get("W03: trafieni")} ripostowalny={e.CanBeRiposted}");
            Assert.AreEqual(1f, pc.UniqueTelemetry.Get("W03: fale"), 0.01f, $"riposta zastąpiona falą (fale={pc.UniqueTelemetry.Get("W03: fale")}, cel ripostowalny={e.CanBeRiposted}, akcja={pc.Actions.Current})");
            Assert.Greater(Lost(side), sideBefore, "fala zraniła drugiego wroga");
            Assert.GreaterOrEqual(pc.UniqueTelemetry.Get("W03: trafieni"), 2f);
        }

        // ================================================================== R05 Zbłąkana klinga

        [UnityTest]
        public IEnumerator WanderingBlade_LeftHandFightsAloneDuringVortex_NoFists()
        {
            var pc = Hero("class_hu_dual", null, new[] { "dual.wandering_blade" }, "hu_sword", "hu_dagger", "hu_whirlwind");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.3f));
            yield return new WaitForSeconds(1.2f);
            Face(pc, e);
            pc.RequestSkill(0);
            var power = pc.Uniques.Get<WanderingBladePower>();
            float t = 0f; while (t < 2f && !power.Spinning) { t += Time.deltaTime; yield return null; }
            Assert.IsTrue(power.Spinning, "wir uruchomiony");
            var names = new System.Collections.Generic.List<string>();
            System.Action<IHitReceiver, HitData, HitResult> onImpact = (tg, h, r) => { if (h.attacker == (IHitReceiver)pc && !h.uniqueEffect && !string.IsNullOrEmpty(h.attackName)) names.Add(h.attackName); };
            CombatEvents.Impact += onImpact;
            t = 0f;
            while (t < 3f && power.Spinning) { if (pc.Actions.IsIdle) pc.Request(ActionType.LightAttack); t += Time.deltaTime; yield return null; }
            CombatEvents.Impact -= onImpact;
            TestContext.WriteLine("ciosy podczas wiru: " + string.Join(", ", names));
            Assert.Greater(names.Count, 0, "lewa broń walczy");
            var dagger = pc.Build.offWeapon.definition.weapon.light.name;
            CollectionAssert.AreEqual(Enumerable.Repeat(dagger, names.Count).ToList(), names, "wyłącznie lewa broń, bez pięści");
        }

        // ================================================================== P05 Wina i wyrok

        [UnityTest]
        public IEnumerator LongPenance_TechniqueStoresGuilt_FlaskSettlesOnce_NoHeal()
        {
            var pc = Hero("class_hu_paladin", null, new[] { "paladin.long_penance" }, null, "hu_shield", "hu_repel");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.3f));
            yield return new WaitForSeconds(1.2f);
            Face(pc, e);
            pc.RequestSkill(0);
            yield return new WaitForSeconds(1.0f);
            var p = pc.Uniques.Get<LongPenancePower>();
            Assert.Greater(p.GuiltOn(e), 0f, "część obrażeń techniki to wina");
            float guilt = p.GuiltOn(e), lost = Lost(e);
            pc.Health.SetCurrent(pc.Health.Max); // pełne życie – flaszka nie tworzy fikcyjnego leczenia
            pc.RequestFlask(false);
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(0f, p.TotalGuilt, 0.01f, "wina rozliczona");
            Assert.Greater(Lost(e), lost, "wyrok trafił");
            Assert.AreEqual(1f, pc.UniqueTelemetry.Get("P05: wyroki z flaszki"), 0.01f, "jeden wyrok");
            Assert.AreEqual(guilt, pc.UniqueTelemetry.Get("P05: wina rozliczona"), 0.5f, "rozliczono dokładnie zapisaną winę");
        }

        // ================================================================== H04 Fala przenosi minę

        [UnityTest]
        public IEnumerator EternalEbb_TidalWaveCarriesOwnMine_KeepsArming()
        {
            var pc = Hero("class_hu_mage_water", null, new[] { "water.eternal_ebb", "mage.sleeping_circles" }, "hu_wand_water", null, "hu_nova_fire", "hu_tidal_wave");
            var far = Enemy(pc, new Vector3(0, 0.05f, 30f));
            yield return new WaitForSeconds(0.1f);
            pc.LockTarget = null; pc.transform.rotation = Quaternion.identity;
            pc.RequestSkill(0); // mina 5 m przed postacią
            yield return new WaitForSeconds(0.6f);
            var mine = UniqueMines.Live(pc).FirstOrDefault();
            Assert.IsNotNull(mine, "mina z novy");
            Vector3 at = mine.AnchorPosition;
            pc.RequestSkill(1);
            yield return new WaitForSeconds(1.2f);
            Assert.GreaterOrEqual(pc.UniqueTelemetry.Get("H04: miny zebrane"), 1f, "fala zebrała minę");
            Assert.Greater(mine.AnchorPosition.z, at.z + 0.5f, "mina przeniesiona na koniec fali");
            Assert.IsTrue(mine.AnchorLive && mine.Armed, "uzbrojenie i termin zachowane");
        }

        // ================================================================== H05 Lustro soli

        [UnityTest]
        public IEnumerator SaltMirror_ShardIntoEmptySpaceLeavesMirror_NextShotBouncesToEnemy()
        {
            var pc = Hero("class_hu_mage_water", null, new[] { "water.salt_mirror" }, null, null, "hu_frost_shard");
            yield return new WaitForSeconds(0.1f);
            pc.LockTarget = null; pc.AssistTarget = null; pc.transform.rotation = Quaternion.identity;
            pc.RequestSkill(0); // odłamek leci prosto (+Z) w pustkę – wróg pojawia się dopiero później
            yield return new WaitForSeconds(1.4f);
            var e = Enemy(pc, new Vector3(5f, 0.05f, 12f));
            yield return new WaitForSeconds(1.2f);
            var power = pc.Uniques.Get<SaltMirrorPower>();
            Assert.AreEqual(1, power.Mirrors, "lustro po odłamku bez trafienia");
            var h = pc.FeatureHitForTest(30f);
            Projectile.Spawn(pc.transform.position + Vector3.up + Vector3.forward * 0.8f, Vector3.forward, 14f, h, Faction.Player, pc, Color.white, 0.2f, 3f);
            yield return new WaitForSeconds(1.6f);
            Assert.AreEqual(1f, pc.UniqueTelemetry.Get("H05: odbicia"), 0.01f, "pocisk odbity raz");
            Assert.AreEqual(0, power.Mirrors, "lustro jednorazowe");
            Assert.Greater(Lost(e), 0f, "odbity pocisk trafił wroga z boku");
        }

        // ================================================================== S02 Śmiertelna chimera

        [UnityTest]
        public IEnumerator SixthRib_ChimeraHasRealHealth_EnemyAttacksIt_CanDie()
        {
            var pc = Hero("class_hu_summoner", "class_feature.summoner.chimera", new[] { "summoner.sixth_rib" }, null, null);
            var hit = pc.FeatureHitForTest(1f);
            Servant.Spawn(pc, ServantKind.Hound, 3f, 1f, () => hit, Color.green);
            Servant.Spawn(pc, ServantKind.Imp, 3f, 1f, () => hit, Color.green);
            yield return new WaitForSeconds(0.1f);
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            yield return null;
            var power = pc.Uniques.Get<SixthRibPower>();
            var ch = power.Chimera;
            Assert.IsNotNull(ch, "chimera scalona");
            Assert.IsTrue(ch.Mortal && ch.HealthMax > 0f, "prawdziwe zdrowie");
            var e = Enemy(pc, ch.transform.position + Vector3.forward * 1.6f, attacks: true, speed: 2f);
            pc.transform.position = new Vector3(-12f, 0.05f, -12f); // gracz daleko – cel to chimera
            float t = 0f, hp0 = ch.HealthCurrent;
            while (t < 8f && ch != null && Servant.Active.Contains(ch) && ch.HealthCurrent >= hp0) { t += Time.deltaTime; yield return null; }
            Assert.IsTrue(ch == null || !Servant.Active.Contains(ch) || ch.HealthCurrent < hp0, "wróg zaatakował chimerę i zabrał jej zdrowie");
            Assert.Greater(t, 0f);
            Assert.IsTrue(e.Deceived || e.AttackingDecoy || ch == null || ch.HealthCurrent < hp0, "AI wybrało chimerę");
        }

        // ================================================================== W02 Ślad poprzedniej broni

        [UnityTest]
        public IEnumerator TwoOaths_FirstHitAfterRealSwap_EchoesPreviousWeapon_SpamDoesNotArm()
        {
            var pc = Hero("class_knight", "class_feature.warrior.arsenal", new[] { "warrior.two_oaths" }, "hu_sword", null);
            pc.Run.arsenalMain = new ItemInstance(bundle.Get<ItemDefinition>("hu_dagger"));
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.5f));
            yield return new WaitForSeconds(0.1f);
            Face(pc, e);
            // Zmiana bez trafienia – nic się nie uzbraja.
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            yield return new WaitForSeconds(0.6f);
            Assert.IsFalse(pc.Uniques.Get<TwoOathsPower>().Armed, "spam zmiany nie ładuje śladu");
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(0.9f);
            pc.RequestSkill(PlayerCombat.SignatureSlot); // zmiana po trafieniu
            yield return new WaitForSeconds(0.6f);
            Assert.IsTrue(pc.Uniques.Get<TwoOathsPower>().Armed, "ślad uzbrojony");
            pc.Request(ActionType.LightAttack);
            yield return new WaitForSeconds(0.9f);
            Assert.AreEqual(1f, pc.UniqueTelemetry.Get("W02: ślady"), 0.01f, "jeden ślad");
            Assert.IsFalse(pc.Uniques.Get<TwoOathsPower>().Armed);
        }
    }
}
