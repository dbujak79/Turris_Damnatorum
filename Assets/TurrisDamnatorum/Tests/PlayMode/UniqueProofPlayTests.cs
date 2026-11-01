using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Turris.Tests
{
    /// <summary>
    /// Dowody działania 24 mocy unikatów, które nie zadziałały w ogólnym teście dymnym (docs/AUDYT_BUILDCRAFT.md, etap 2).
    /// Każdy scenariusz tworzy warunek danej mocy prawdziwą ścieżką gry (AI wroga, parowanie, przycisk cechy, rzut skilla)
    /// i sprawdza skutek: zdarzenie mocy w telemetrii oraz obrażenia albo stan w świecie. To dowód działania, nie siły.
    /// </summary>
    public class UniqueProofPlayTests
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

        PlayerCombat Hero(string classId, string featureId, string unique, string main, string off, string[] perks, params string[] skills)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0); floor.transform.localScale = new Vector3(80, 1, 80); cleanup.Add(floor);
            var camGo = new GameObject("TestCam"); camGo.AddComponent<Camera>(); var rig = camGo.AddComponent<CameraRig>(); cleanup.Add(camGo);
            var p = WorldBuilder.CreatePlayer(); cleanup.Add(p);
            var ctrl = p.GetComponent<PlayerController>(); ctrl.cameraRig = rig; ctrl.InputEnabled = false; rig.target = p.transform; rig.player = ctrl;
            var plan = new LoadoutPlan { classDef = bundle.Get<ClassDefinition>(classId), difficulty = bundle.config.difficulties[0], classFeatureId = featureId };
            var run = RunFactory.Create(plan, bundle.config, 7);
            run.attributes.intelligence = 30; run.attributes.strength = 30; run.attributes.dexterity = 30;
            run.equipment.Unequip(EquipSlot.MainHand); run.equipment.Unequip(EquipSlot.OffHand);
            void Eq(string id, EquipSlot s) { if (id == null) return; var it = new ItemInstance(bundle.Get<ItemDefinition>(id)); run.inventory.Add(it); Assert.IsTrue(run.EquipFromInventory(it, s), id); }
            Eq(main, EquipSlot.MainHand); Eq(off, EquipSlot.OffHand);
            var item = HellEconomy.Roll(UniqueRegistry.Find(bundle.config, "item.unique.turris." + unique), LootSource.Boss, 8, new System.Random(1));
            run.inventory.Add(item); Assert.IsTrue(run.EquipFromInventory(item, item.definition.DefaultSlot), unique);
            if (perks != null) foreach (var perk in perks) run.AddBoon(bundle.Get<BoonDefinition>(perk));
            run.SyncGrantedSkills();
            for (int i = 0; i < RunState.SkillSlotCount; i++) run.ClearSlot(i);
            for (int i = 0; i < skills.Length; i++) { var d = bundle.Get<SpellDefinition>(skills[i]); run.AssignSlot(run.FindSpell(d) ?? run.LearnSpell(d, true), i); }
            var pc = p.GetComponent<PlayerCombat>();
            pc.Init(run, bundle.config);
            pc.DebugNoPassiveBlock = true; pc.Immortal = true;
            p.GetComponent<PlayerVisuals>().Refresh();
            return pc;
        }

        EnemyBrain Enemy(PlayerCombat pc, Vector3 pos, bool attacks = false, float poise = 100000f, float speed = 0f, float hp = 6000f, float turn = 360f)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.displayName = "Manekin"; def.maxHealth = hp; def.maxPoise = poise; def.scale = 1f; def.moveSpeed = speed; def.strafeChance = 0f; def.turnSpeed = turn;
            def.preferredRange = 1.4f; def.attackInterval = new Vector2(0.3f, 0.5f);
            if (attacks) def.attacks.Add(new EnemyAttackEntry { attack = new AttackDefinition { name = "Pazur", baseDamage = 25f, reach = 2.2f, radius = 1f, windup = 0.35f, lunge = 0f }, maxRange = 3f });
            cleanup.Add(def);
            var e = WorldBuilder.CreateEnemy(def, pos, null);
            e.transform.rotation = Quaternion.LookRotation(pc.transform.position - pos);
            e.Setup(def, pc, 1f, null, false, bundle.config.balance);
            cleanup.Add(e.gameObject);
            return e;
        }

        const float Wake = 1.2f; // koniec uśpienia wrogów po pojawieniu się

        static void Face(PlayerCombat pc, EnemyBrain e) { Vector3 d = e.transform.position - pc.transform.position; d.y = 0; pc.transform.rotation = Quaternion.LookRotation(d); pc.LockTarget = e; }
        static float Lost(EnemyBrain e) => e.Health.Max - e.Health.Current;
        static float U(PlayerCombat pc, string key) => pc.UniqueTelemetry.Get(key);
        static string Dump(PlayerCombat pc) => string.Join("; ", pc.UniqueTelemetry.values.Select(kv => $"{kv.Key}={kv.Value:0.#}").Concat(pc.UniqueTelemetry.rejections.Select(kv => $"ODMOWA {kv.Key}×{kv.Value}")));
        static void Press(PlayerCombat pc) => pc.RequestSkill(PlayerCombat.SignatureSlot);

        static void Teleport(PlayerCombat pc, Vector3 at)
        {
            var cc = pc.GetComponent<CharacterController>();
            cc.enabled = false; pc.transform.position = at; cc.enabled = true;
        }

        /// <summary>Czeka na warunek, wykonując co klatkę działanie (np. atak, gdy bezczynny).</summary>
        static IEnumerator Until(Func<bool> done, float seconds, Action each = null)
        {
            float t = 0f;
            while (t < seconds && !done()) { each?.Invoke(); t += Time.deltaTime; yield return null; }
        }

        static IEnumerator Idle(PlayerCombat pc, float seconds = 4f) => Until(() => pc.Actions.IsIdle, seconds);

        /// <summary>Ciosy wroga w bohatera prawdziwym potokiem trafienia co 0,2 s, aż warunek się spełni (powtarzalnie, niezależnie od uników i regeneracji osłony).</summary>
        static IEnumerator StrikeUntil(PlayerCombat pc, EnemyBrain e, Func<bool> done, float seconds = 4f, float damage = 30f)
        {
            var claw = new AttackDefinition { name = "Pazur", baseDamage = damage, reach = 2.2f, radius = 1f };
            float t = 0f, next = 0f;
            while (t < seconds && !done())
            {
                if (t >= next) { next = t + 0.2f; HitQuery.Apply(e, pc, HitData.FromAttack(claw, damage, e.transform.position, e), true); }
                t += Time.deltaTime; yield return null;
            }
        }

        /// <summary>Prawdziwe parowanie: naciśnięte pod koniec zamachu wroga, aż warunek się spełni.</summary>
        static IEnumerator ParryUntil(PlayerCombat pc, EnemyBrain e, Func<bool> done, float seconds = 10f)
        {
            float t = 0f, tele = -1f;
            while (t < seconds && !done())
            {
                pc.RestoreAll();
                if (e.IsTelegraphing) { if (tele < 0f) tele = t; } else tele = -1f;
                if (pc.Actions.IsIdle && tele >= 0f && t - tele >= e.AttackWindupDuration - 0.15f) pc.Request(ActionType.Parry);
                t += Time.deltaTime; yield return null;
            }
        }

        // ================================================================== Warunek cechy

        [UnityTest]
        public IEnumerator B02_FurySpendInRage_SpiritsStrike()
        {
            var pc = Hero("class_hu_berserker", UniqueCatalog.FurySpend, "berserker.angry_spirits", "hu_greataxe", null, null);
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.6f));
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            pc.AddFury(100f);
            yield return Until(() => pc.Raging, 1f);
            Assert.IsTrue(pc.Raging, "pełna furia daje szał");
            Press(pc);
            Assert.IsFalse(pc.Raging, "szał oddany");
            float hp = Lost(e);
            yield return Until(() => U(pc, "B02: ciosy widm") >= 1f, 3f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "B02: widma"), 1f, Dump(pc));
            Assert.GreaterOrEqual(U(pc, "B02: ciosy widm"), 1f, Dump(pc));
            Assert.Greater(Lost(e), hp, "widma zadają obrażenia");
        }

        [UnityTest]
        public IEnumerator L01_WitnessWithSecondEnemy_DeceivedHitTheKing()
        {
            var pc = Hero("class_rogue", UniqueCatalog.Witness, "rogue.false_king", "hu_dagger", null, null);
            var king = Enemy(pc, new Vector3(0, 0.05f, 2.2f));
            var other = Enemy(pc, new Vector3(1.6f, 0.05f, 2.4f), attacks: true, speed: 3f);
            yield return new WaitForSeconds(Wake);
            Face(pc, king);
            yield return Until(() => other.CurrentState != EnemyBrain.State.Attacking, 3f);
            Press(pc);
            Assert.IsTrue(other.Deceived, "wróg poza atakiem daje się oszukać");
            float hp = Lost(king);
            TestContext.WriteLine($"L01 po wciśnięciu: oszukany={other.Deceived}, stan {other.CurrentState}");
            Vector3 kingAt = king.transform.position;
            yield return Until(() => U(pc, "L01: ciosy w króla") >= 1f, 5f);
            TestContext.WriteLine(Dump(pc) + $"; oszukany={other.Deceived}, stan {other.CurrentState}, cecha: " + string.Join("; ", pc.FeatureTelemetry.values.Select(kv => $"{kv.Key}={kv.Value:0.#}")));
            Assert.GreaterOrEqual(U(pc, "L01: ciosy w króla"), 1f, Dump(pc));
            Assert.Greater(Lost(king), hp, "oszukany naprawdę rani króla");
            Assert.Less(Vector3.Distance(king.transform.position, kingAt), 0.5f, "wabik nie wypycha króla z miejsca");
        }

        [UnityTest]
        public IEnumerator L02_SpentTrickBecomesTrap_ContactExecutesIt()
        {
            var pc = Hero("class_rogue", UniqueCatalog.Trick, "rogue.professional_cheat", "hu_dagger", null, null);
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.8f), attacks: true);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            Press(pc);
            var f = (StolenTrickFeature)pc.Feature;
            yield return Until(() => f.Trick != StolenTrickFeature.TrickKind.None, 5f);
            Assert.AreNotEqual(StolenTrickFeature.TrickKind.None, f.Trick, "sztuczka skradziona");
            float hp = Lost(e);
            Press(pc);
            Assert.IsTrue(pc.UniqueTelemetry.values.Keys.Any(k => k.StartsWith("L02: pułapki")), "wydanie kładzie pułapkę: " + Dump(pc));
            Assert.AreEqual(hp, Lost(e), 0.01f, "bez natychmiastowego ciosu");
            yield return Until(() => Lost(e) > hp, 3f);
            TestContext.WriteLine(Dump(pc));
            Assert.Greater(Lost(e), hp, "kontakt po uzbrojeniu wykonuje profil");
        }

        [UnityTest]
        public IEnumerator K01_RipWithTwoAnchors_LinesDamage()
        {
            var pc = Hero("class_hu_archer", UniqueCatalog.Nails, "archer.bowstring_weaver", "hu_bow", null, null);
            var a = Enemy(pc, new Vector3(-1.5f, 0.05f, 6f));
            var b = Enemy(pc, new Vector3(1.5f, 0.05f, 6f));
            var mid = Enemy(pc, new Vector3(0f, 0.05f, 6.05f));
            yield return new WaitForSeconds(Wake);
            Press(pc);
            var f = (HuntersNailsFeature)pc.Feature;
            foreach (var target in new[] { a, b })
            {
                Face(pc, target);
                yield return Until(() => f.NailsOn(target) >= 1, 4f, () => { Face(pc, target); if (pc.Actions.IsIdle) pc.Request(ActionType.LightAttack); });
                Assert.GreaterOrEqual(f.NailsOn(target), 1, "grot osadzony");
                yield return Idle(pc);
            }
            float hp = Lost(mid);
            Press(pc);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "K01: sieci"), 1f, Dump(pc));
            Assert.Greater(Lost(mid), hp, "wróg przecinający odcinek dostaje cięciwę");
        }

        [UnityTest]
        public IEnumerator Q02_ReactionStored_NextShotDeliversIt()
        {
            var pc = Hero("class_hu_arcane_archer", UniqueCatalog.Answer, "arcane_archer.stolen_elements", "hu_bow", null, null);
            var e = Enemy(pc, new Vector3(0, 0.05f, 6f), hp: 100000f);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            // Krwawienie → strzała odpowiedzi to błyskawica; przemoczenie + błyskawica to prawdziwa reakcja.
            e.ApplyStatus(StatusKind.Bleed, 200f, 0f);
            e.ApplyStatus(StatusKind.Soaked, 1f, 8f);
            Press(pc);
            yield return Until(() => U(pc, "Q02: odłożone reakcje") > 0f, 4f, () => { if (pc.Actions.IsIdle) pc.Request(ActionType.LightAttack); });
            Assert.Greater(U(pc, "Q02: odłożone reakcje"), 0f, "reakcja odłożona: " + Dump(pc));
            yield return Until(() => U(pc, "Q02: przeniesione reakcje") > 0f, 4f, () => { if (pc.Actions.IsIdle) pc.Request(ActionType.LightAttack); });
            TestContext.WriteLine(Dump(pc));
            Assert.Greater(U(pc, "Q02: przeniesione reakcje"), 0f, "następny strzał niesie ładunek: " + Dump(pc));
        }

        // ================================================================== Obrona zgrana z ciosem

        [UnityTest]
        public IEnumerator W04_BlockSavesWatchPoint_TechniqueBornThere()
        {
            var pc = Hero("class_knight", null, "warrior.left_watch", "hu_sword", "hu_shield", null, "hu_cleave");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.8f), attacks: true);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            pc.SetBlockHeld(true);
            yield return Until(() => U(pc, "W04: warty") >= 1f, 6f, () => pc.RestoreAll());
            pc.SetBlockHeld(false);
            Assert.GreaterOrEqual(U(pc, "W04: warty"), 1f, "udany blok zapisał punkt: " + Dump(pc));
            yield return Idle(pc);
            pc.RestoreAll();
            Face(pc, e);
            pc.RequestSkill(0);
            yield return Until(() => U(pc, "W04: techniki z warty") >= 1f, 2f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "W04: techniki z warty"), 1f, Dump(pc));
        }

        [UnityTest]
        public IEnumerator P04_AuraStoresPulses_ParryReleasesWave()
        {
            var pc = Hero("class_hu_paladin", null, "paladin.penitent_bells", "hu_morningstar", "hu_shield", null, "hu_aura_holy_fire");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.8f), attacks: true);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            pc.RequestSkill(0);
            var power = pc.Uniques.Get<PenitentBellsPower>();
            yield return Until(() => power.Pulses >= 2, 6f, () => pc.RestoreAll());
            Assert.GreaterOrEqual(power.Pulses, 2, "aura gromadzi impulsy zamiast tykać");
            float hp = Lost(e);
            yield return ParryUntil(pc, e, () => U(pc, "P04: uwolnione impulsy") >= 1f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "P04: uwolnione impulsy"), 2f, Dump(pc));
            Assert.Greater(Lost(e), hp, "fala rani");
        }

        [UnityTest]
        public IEnumerator T04_ParryThenRiposteCastsSlottedSpell()
        {
            var pc = Hero("class_hu_battlemage", null, "battlemage.parrying_rune", "hu_sword", "hu_grimoire_water", null, "hu_frost_shard");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.8f), attacks: true, poise: 30f);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            yield return ParryUntil(pc, e, () => e.CanBeRiposted);
            Assert.IsTrue(e.CanBeRiposted, "parowanie przełamało postawę");
            yield return Idle(pc);
            Face(pc, e);
            float mana = pc.Mana.Current;
            pc.Request(ActionType.LightAttack);
            yield return Until(() => U(pc, "T04: riposty-czary") >= 1f, 2f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "T04: riposty-czary"), 1f, Dump(pc));
            Assert.Less(pc.Mana.Current, mana + 0.01f + pc.Mana.Max * 0.2f, "czar kosztuje normalną manę");
        }

        // ================================================================== Prawdziwe przyjęcie ciosu

        [UnityTest]
        public IEnumerator M02_CloakAbsorbsHit_RuneStrengthensNextNova()
        {
            var pc = Hero("class_mage", UniqueCatalog.Cloak, "mage.sealed_source", null, "hu_grimoire_fire", null, "hu_nova_fire");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.6f));
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            Press(pc);
            var power = pc.Uniques.Get<SealedSourcePower>();
            yield return StrikeUntil(pc, e, () => power.Runes >= 1);
            Assert.GreaterOrEqual(power.Runes, 1, "mana wydana płaszczem zostawia runę: " + Dump(pc));
            int runes = power.Runes;
            yield return Idle(pc);
            pc.RequestSkill(0);
            yield return Until(() => U(pc, "M02: runy wydane") >= 1f, 2f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "M02: runy wydane"), runes, Dump(pc));
            Assert.AreEqual(0, power.Runes, "jedna konsumpcja");
        }

        [UnityTest]
        public IEnumerator E04_WardPlateBroken_SpikesPickShards()
        {
            var pc = Hero("class_hu_mage_earth", null, "earth.living_quarry", "hu_wand_earth", null, null, "hu_stone_ward", "hu_stone_spikes");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.8f));
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            pc.RequestSkill(0);
            yield return Until(() => pc.BarrierAmount > 0f, 2f);
            Assert.Greater(pc.BarrierAmount, 0f, "osłona działa");
            var power = pc.Uniques.Get<LivingQuarryPower>();
            yield return StrikeUntil(pc, e, () => power.Shards >= 1); // pierwsze ciosy zużywają Arkanową osłonę maga, potem barierę
            Assert.GreaterOrEqual(power.Shards, 1, "płyta odłamana ciosem zostawia odłamek: " + Dump(pc));
            yield return Idle(pc);
            pc.RestoreAll();
            Face(pc, e);
            pc.RequestSkill(1);
            yield return Until(() => U(pc, "E04: zebrane odłamki") >= 1f, 2f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "E04: zebrane odłamki"), 1f, Dump(pc));
        }

        // ================================================================== Śmierć podpalonego

        [UnityTest]
        public IEnumerator F02_BurningVictimDies_PuppetCarriesWoundsToNextEnemy()
        {
            var pc = Hero("class_hu_mage_fire", UniqueCatalog.GreatFire, "fire.ash_puppets", "hu_wand_fire", null, null);
            var victim = Enemy(pc, new Vector3(0, 0.05f, 4f));
            var next = Enemy(pc, new Vector3(2.5f, 0.05f, 4f));
            yield return new WaitForSeconds(Wake);
            victim.Status.AddBurnFamily(1, 60f, 6f, 3);
            victim.Health.SetCurrent(1f);
            yield return Until(() => victim.IsDead, 3f);
            Assert.IsTrue(victim.IsDead, "ofiara ginie od podpalenia");
            yield return Until(() => U(pc, "F02: rany przeniesione") >= 1f, 4f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "F02: marionetki"), 1f, Dump(pc));
            Assert.GreaterOrEqual(U(pc, "F02: rany przeniesione"), 1f, Dump(pc));
            Assert.IsTrue(next.Status.Burning, "pozostałe rany płoną na nowym celu");
        }

        // ================================================================== Kolejność i ustawienie

        [UnityTest]
        public IEnumerator F01_OrbEatsOwnFireField_NoFieldLeft()
        {
            var pc = Hero("class_hu_mage_fire", null, "fire.pyre_devourer", "hu_wand_fire", null, null, "hu_infernal_orb", "hu_ember_ring");
            var e = Enemy(pc, new Vector3(0, 0.05f, 5f), hp: 100000f);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            pc.RequestSkill(1);
            yield return Until(() => DamageZone.Active.Any(z => z != null && ReferenceEquals(z.Owner, pc)), 2f);
            Assert.IsTrue(DamageZone.Active.Any(z => z != null && ReferenceEquals(z.Owner, pc)), "własne pole ognia");
            yield return Idle(pc);
            pc.RestoreAll();
            Face(pc, e);
            pc.RequestSkill(0);
            yield return Until(() => U(pc, "F01: pożarte pola") >= 1f, 3f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "F01: pożarte pola"), 1f, Dump(pc));
            Assert.Greater(U(pc, "F01: budżet pól"), 0f);
        }

        [UnityTest]
        public IEnumerator A02_KnockedEnemyHitsOwnSlowShot_Burst()
        {
            var pc = Hero("class_hu_mage_air", UniqueCatalog.Fronts, "air.storm_smith", "hu_wand_air", null, null, "hu_infernal_orb");
            var e = Enemy(pc, new Vector3(0, 0.05f, 6f), hp: 100000f);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            pc.RequestSkill(0);
            HuShot shot = null;
            yield return Until(() => (shot = HuShot.Live.FirstOrDefault(s => s != null && s.IsActive && s.SourceCast != null && s.SourceCast.caster == pc)) != null, 2f);
            Assert.IsNotNull(shot, "własny wolny pocisk: " + Dump(pc));
            TestContext.WriteLine($"prędkość pocisku {shot.Velocity.magnitude:0.0}");
            // Prawdziwy odrzut: uderzenie od strony dalszej niż pocisk pcha wroga na pocisk.
            var hit = pc.FeatureHitForTest(1f);
            hit.knockback = 8f;
            hit.sourcePosition = e.transform.position + (e.transform.position - shot.transform.position).normalized;
            HitQuery.Apply(pc, e, hit, false);
            yield return Until(() => U(pc, "A02: zderzenia") >= 1f, 2f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "A02: zderzenia"), 1f, Dump(pc));
        }

        [UnityTest]
        public IEnumerator A04_ChainUsesOwnShotAsNode()
        {
            var pc = Hero("class_hu_mage_air", null, "air.empty_nodes", null, null, null, "hu_damned_chain");
            // Skok łańcucha 4,5 m: drugi wróg poza zasięgiem z ciała, ale w zasięgu z pocisku.
            var e1 = Enemy(pc, new Vector3(0, 0.05f, 4f), hp: 100000f);
            var e2 = Enemy(pc, new Vector3(7.5f, 0.05f, 4f), hp: 100000f);
            yield return new WaitForSeconds(Wake);
            Face(pc, e1);
            var node = Projectile.Spawn(new Vector3(3.5f, 1f, 4f), Vector3.back, 0.05f, pc.FeatureHitForTest(1f), Faction.Player, pc, Color.white, 0.1f, 8f, null, 0f, false, default);
            Assert.IsNotNull(node, "własny wolny pocisk w świecie");
            pc.RequestSkill(0);
            yield return Until(() => U(pc, "A04: węzły") >= 1f, 2f);
            yield return new WaitForSeconds(0.3f);
            TestContext.WriteLine(Dump(pc) + $"; drugi wróg stracił {Lost(e2):0}");
            Assert.GreaterOrEqual(U(pc, "A04: węzły"), 1f, Dump(pc));
            Assert.Greater(Lost(e2), 0f, "łańcuch doszedł przez węzeł do dalszego wroga");
        }

        [UnityTest]
        public IEnumerator L05_KnifePassingMarkerTriggersTrap_EnemyContactDoesNot()
        {
            var pc = Hero("class_rogue", null, "rogue.abyss_signal", "hu_dagger", "hu_offdagger", null, "hu_knife_throw", "hu_sulfur_bomb", "hu_snare");
            var e = Enemy(pc, new Vector3(0, 0.05f, 4f), hp: 100000f);
            // Pułapkę daje pieczęć formy Wnyki – sama strefa Sideł nie jest miną.
            var snare = pc.Run.FindSpell(bundle.Get<SpellDefinition>("hu_snare"));
            var wnyki = HuCatalog.Seal("seal.form.snare.trap");
            pc.Run.AddSeal(wnyki, toPouch: true);
            Assert.IsTrue(pc.Run.InsertSeal(snare, wnyki), "pieczęć Wnyki na Sidłach");
            pc.RefreshBuild();
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            pc.RequestSkill(2);
            yield return Until(() => UniqueMines.Live(pc).Count > 0, 2f);
            Assert.Greater(UniqueMines.Live(pc).Count, 0, "własna pułapka ze znacznikiem");
            yield return Idle(pc);
            pc.RestoreAll();
            yield return new WaitForSeconds(1f); // uzbrojenie; wróg na znaczniku nie odpala
            Assert.Greater(UniqueMines.Live(pc).Count, 0, "wejście wroga nie odpala pułapki");
            Face(pc, e);
            pc.RequestSkill(0);
            yield return Until(() => U(pc, "L05: sygnały") >= 1f, 2f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "L05: sygnały"), 1f, Dump(pc));
        }

        [UnityTest]
        public IEnumerator Z04_BackstabChargeDeliveredByKnives()
        {
            var pc = Hero("class_hu_assassin", null, "assassin.silent_witness", "hu_dagger", "hu_offdagger", null, "hu_knife_throw");
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.2f), hp: 100000f, turn: 0f);
            e.transform.rotation = Quaternion.LookRotation(Vector3.forward); // plecami do bohatera
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            var power = pc.Uniques.Get<SilentWitnessPower>();
            yield return Until(() => power.Charge > 0f, 4f, () => { pc.RestoreAll(); if (pc.Actions.IsIdle) pc.Request(ActionType.LightAttack); });
            Assert.Greater(power.Charge, 0f, "cios w plecy odkłada ładunek: " + Dump(pc));
            yield return Idle(pc);
            pc.RestoreAll();
            pc.RequestSkill(0);
            yield return Until(() => U(pc, "Z04: dostarczony ładunek") > 0f, 3f);
            TestContext.WriteLine(Dump(pc));
            Assert.Greater(U(pc, "Z04: dostarczony ładunek"), 0f, Dump(pc));
        }

        [UnityTest]
        public IEnumerator Z05_CrossCutKnots_HitOneMovesBleedToOther()
        {
            var pc = Hero("class_hu_assassin", null, "assassin.crimson_knot", "hu_dagger", null, null, "hu_cross_cut");
            var a = Enemy(pc, new Vector3(-0.5f, 0.05f, 1.3f), hp: 100000f);
            var b = Enemy(pc, new Vector3(0.5f, 0.05f, 1.3f), hp: 100000f);
            yield return new WaitForSeconds(Wake);
            Face(pc, a);
            pc.RequestSkill(0);
            yield return Until(() => U(pc, "Z05: węzły") >= 1f, 2f);
            Assert.GreaterOrEqual(U(pc, "Z05: węzły"), 1f, "dwie ofiary jednego cięcia: " + Dump(pc));
            yield return Idle(pc);
            a.ApplyStatus(StatusKind.Bleed, 400f, 0f);
            pc.RestoreAll();
            yield return Until(() => U(pc, "Z05: odpowiedzi węzła") >= 1f, 4f, () => { Face(pc, a); if (pc.Actions.IsIdle) pc.Request(ActionType.LightAttack); });
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "Z05: odpowiedzi węzła"), 1f, Dump(pc));
            Assert.Greater(U(pc, "Z05: budżet węzła"), 0f);
        }

        [UnityTest]
        public IEnumerator R02_DodgeCarryingSwingLeavesTrace_HeavyReleases()
        {
            var pc = Hero("class_hu_dual", UniqueCatalog.Dance, "dual.unfinished_step", "hu_sword", "hu_dagger", null);
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.5f), hp: 100000f);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            Press(pc);
            pc.Request(ActionType.LightAttack);
            yield return null;
            pc.MoveIntent = Vector3.right * 0.2f + Vector3.forward;
            pc.Request(ActionType.Dodge);
            pc.MoveIntent = Vector3.zero;
            var power = pc.Uniques.Get<UnfinishedStepPower>();
            yield return Until(() => power.Traces >= 1, 1f);
            Assert.GreaterOrEqual(power.Traces, 1, "unik z ciosem odkłada ślad: " + Dump(pc));
            yield return Idle(pc);
            pc.RestoreAll();
            Face(pc, e);
            pc.Request(ActionType.HeavyAttack);
            yield return Until(() => U(pc, "R02: uwolnienia") >= 1f, 2f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "R02: uwolnienia"), 1f, Dump(pc));
            Assert.AreEqual(0, power.Traces);
        }

        [UnityTest]
        public IEnumerator S04_ShardLeavesServantsMaw_CostsItsLife()
        {
            var pc = Hero("class_hu_summoner", null, "summoner.open_maws", null, null, null, "hu_frost_shard");
            var e = Enemy(pc, new Vector3(0, 0.05f, 6f), hp: 100000f);
            var hound = Servant.Spawn(pc, ServantKind.Hound, 30f, 1f, () => pc.FeatureHitForTest(5f), Color.green);
            hound.transform.position = pc.transform.position + Vector3.right * 3f;
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            yield return Idle(pc);
            float life = hound.Remaining;
            pc.RequestSkill(0);
            yield return Until(() => U(pc, "S04: czary z pysków") >= 1f, 2f);
            TestContext.WriteLine(Dump(pc) + $"; umiejętność używalna: {pc.SkillUsable(0)}; czas sługi {life:0.0} → {hound.Remaining:0.0}");
            Assert.GreaterOrEqual(U(pc, "S04: czary z pysków"), 1f, Dump(pc));
            Assert.Less(hound.Remaining, life - 1f, "sługa zapłacił czasem życia");
        }

        // ================================================================== Czas

        [UnityTest]
        public IEnumerator S05_NaturalEndLeavesTrace_NextOtherKindInherits()
        {
            var pc = Hero("class_hu_summoner", null, "summoner.inherited_beast", null, "hu_grimoire_summoning", null, "hu_summon_hounds");
            var e = Enemy(pc, new Vector3(0, 0.05f, 4f), hp: 100000f);
            yield return new WaitForSeconds(Wake);
            Servant.Spawn(pc, ServantKind.Imp, 0.5f, 1f, () => pc.FeatureHitForTest(5f), Color.green);
            yield return Until(() => U(pc, "S05: ślady dziedziczenia") >= 1f, 2f);
            Assert.GreaterOrEqual(U(pc, "S05: ślady dziedziczenia"), 1f, "naturalny koniec: " + Dump(pc));
            yield return Idle(pc);
            pc.RestoreAll();
            pc.RequestSkill(0);
            yield return Until(() => U(pc, "S05: dziedziczenia") >= 1f, 2f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "S05: dziedziczenia"), 1f, Dump(pc));
            Assert.IsTrue(Servant.Active.Any(s => s != null && s.Master == pc && s.Inherited == StatusKind.Burn), "ogar dziedziczy podpalenie po chochliku");
        }

        // ================================================================== Pełny build

        [UnityTest]
        public IEnumerator Q03_ExtraArrowsTakeSlotElements()
        {
            var pc = Hero("class_hu_arcane_archer", null, "arcane_archer.heretic_choir", null, null, new[] { "hu_perk_mage_splinter" }, "hu_frost_shard", "hu_nova_fire", "hu_nova_air");
            var e = Enemy(pc, new Vector3(0, 0.05f, 6f), hp: 100000f);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            pc.Request(ActionType.LightAttack);
            yield return Until(() => U(pc, "Q03: strzały chóru") >= 1f, 2f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "Q03: strzały chóru"), 1f, Dump(pc));
        }

        [UnityTest]
        public IEnumerator Q05_ExtraArrowsStored_HeavyShotReleases()
        {
            var pc = Hero("class_hu_arcane_archer", null, "arcane_archer.between_worlds", "hu_bow", null, new[] { "hu_perk_mage_splinter" });
            var e = Enemy(pc, new Vector3(0, 0.05f, 6f), hp: 100000f);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            var power = pc.Uniques.Get<BetweenWorldsPower>();
            pc.Request(ActionType.LightAttack);
            yield return Until(() => power.Stored >= 1, 2f);
            Assert.GreaterOrEqual(power.Stored, 1, "dodatkowa strzała w kołczanie: " + Dump(pc));
            yield return Idle(pc);
            pc.Request(ActionType.HeavyAttack);
            yield return Until(() => U(pc, "Q05: uwolnione strzały") >= 1f, 3f);
            TestContext.WriteLine(Dump(pc));
            Assert.GreaterOrEqual(U(pc, "Q05: uwolnione strzały"), 1f, Dump(pc));
            Assert.AreEqual(0, power.Stored);
        }

        // ================================================================== Moce bierne: mierzony skutek

        [UnityTest]
        public IEnumerator P03_AuraReachesServants_OwnerGetsLess()
        {
            var pc = Hero("class_hu_paladin", null, "paladin.empty_monastery", "hu_morningstar", "hu_grimoire_summoning", null, "hu_aura_might");
            var e = Enemy(pc, new Vector3(0, 0.05f, 2.5f), hp: 100000f);
            var hits = new List<float>();
            Action<IHitReceiver, HitData, HitResult> onImpact = (tg, h, r) => { if (h.servantHit && ReferenceEquals(tg, e)) hits.Add(h.Total); };
            CombatEvents.Impact += onImpact;
            try
            {
                yield return new WaitForSeconds(Wake);
                var snap = pc.FeatureHitForTest(10f);
                Servant.Spawn(pc, ServantKind.Imp, 40f, 0.6f, () => snap, Color.green);
                yield return Until(() => hits.Count >= 1, 4f);
                Assert.Greater(hits.Count, 0, "sługa atakuje");
                float before = hits[0];
                var power = pc.Uniques.Get<EmptyMonasteryPower>();
                Assert.AreEqual(1f, power.ServantDamageMult, 1e-4f, "bez aury bez premii");
                pc.RequestSkill(0);
                yield return Until(() => power.ServantDamageMult > 1f, 2f);
                Assert.Greater(power.ServantDamageMult, 1f, "aura męstwa obejmuje sługi");
                Assert.Less(power.AuraOwnerScale, 1f, "właściciel dostaje mniej");
                hits.Clear();
                yield return Until(() => hits.Count >= 1, 4f);
                TestContext.WriteLine($"cios sługi {before:0.00} → {hits[0]:0.00}, mnożnik {power.ServantDamageMult:0.00}, właściciel ×{power.AuraOwnerScale:0.00}");
                Assert.AreEqual(power.ServantDamageMult, hits[0] / before, 0.02f, "prawdziwe trafienie sługi rośnie");
            }
            finally { CombatEvents.Impact -= onImpact; }
        }

        [UnityTest]
        public IEnumerator T03_SunderCutsElementResistInsteadOfPhysical()
        {
            var pc = Hero("class_hu_battlemage", null, "battlemage.rune_eater", "hu_warhammer", null, null);
            var e = Enemy(pc, new Vector3(0, 0.05f, 1.4f), hp: 100000f);
            yield return new WaitForSeconds(Wake);
            Face(pc, e);
            yield return Until(() => e.Status.SunderStacks > 0, 6f, () => { pc.RestoreAll(); if (pc.Actions.IsIdle) pc.Request(ActionType.HeavyAttack); });
            TestContext.WriteLine(Dump(pc) + $"; warstwy {e.Status.SunderStacks}, odporność żywiołów −{e.Status.SunderElementCut:0.00}, fizyczna −{e.Status.SunderResistReduction(bundle.config.balance):0.00}");
            Assert.Greater(e.Status.SunderStacks, 0, "pęknięcie z broni: " + Dump(pc));
            Assert.Greater(e.Status.SunderElementCut, 0f, "odporność żywiołów niżej");
            Assert.AreEqual(0f, e.Status.SunderResistReduction(bundle.config.balance), 1e-4f, "fizyczna bez zmian");
        }
    }
}
