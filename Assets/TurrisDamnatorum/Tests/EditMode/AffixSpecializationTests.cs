using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>
    /// Affiksy specjalizacyjne (docs/PROMPT_WDROZENIE_AFFIKSOW_POE2.md, rozdz. 10.1): katalog, kwalifikacja, losowanie, jakość,
    /// rzeczywiste dodane żywioły, przebicie per żywioł, blok tarczą, postawa, szybkość rzucania, zapis i zakładanie.
    /// </summary>
    public class AffixSpecializationTests
    {
        Fixture fx;

        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        static readonly string[] NewGroups = { "fire_pen", "water_pen", "air_pen", "earth_pen", "poise_damage", "minion_damage", "shield_block_cost", "cast_speed" };

        ItemDefinition D(string id) => fx.Get<ItemDefinition>(id);

        /// <summary>Pierścień z ręcznie ustawionymi affiksami (stan „po losowaniu”, nowa wersja znaczenia).</summary>
        ItemInstance Ring(params StatModifier[] mods)
        {
            var it = new ItemInstance(D("hu_item_ring_iron")) { rarity = ItemRarity.Rare, affixVersion = HellEconomy.AffixVersion };
            it.affixes.AddRange(mods);
            return it;
        }

        static StatModifier M(StatType s, float v) => new StatModifier(s, v);

        // ------------------------------------------------------------------ E01 katalog

        [Test]
        public void E01_Catalog_EightNewGroups_ThreeTiersEach_UniqueIds_Supported()
        {
            var all = HellEconomy.Affixes;
            Assert.AreEqual(all.Count, all.Select(a => a.id).Distinct().Count(), "id unikalne");
            foreach (var g in NewGroups)
            {
                var tiers = all.Where(a => a.Group == g).OrderBy(a => a.Tier).ToList();
                Assert.AreEqual(3, tiers.Count, g);
                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, tiers.Select(t => t.Tier).ToArray(), g);
                Assert.IsTrue(tiers.All(t => t.id == $"affix.turris.{g}.t{t.Tier}"), g + ": trwałe id");
                Assert.IsTrue(tiers.All(t => !string.IsNullOrEmpty(t.name) && t.name != t.id), g + ": polska nazwa");
                Assert.IsTrue(tiers.All(t => t.Ranges.All(r => HellEconomy.AffixSupported(r.Split('|')[0]))), g + ": statystyka obsługiwana");
                Assert.IsTrue(tiers.All(t => t.Decimals == 3 && !string.IsNullOrEmpty(t.Filter)), g + ": precyzja i filtr");
                Assert.IsTrue(tiers.Zip(tiers.Skip(1), (a, b) => a.MinWave < b.MinWave).All(x => x), g + ": rosnące progi");
            }
            Assert.AreEqual(0.6f, NewGroups.Take(4).Sum(g => all.Where(a => a.Group == g).Max(a => a.Weight)), 1e-4f, "przebicie razem 0,6");
            Assert.AreEqual(155, all.Count, "131 HU + 24 Turris");
            Assert.AreEqual(54, all.Select(a => a.Group).Distinct().Count());
            Assert.AreEqual(92, all.Count(a => a.Kind == 0));
            Assert.AreEqual(63, all.Count(a => a.Kind == 1));
            Assert.IsTrue(all.All(a => a.Ranges.All(r => HellEconomy.AffixSupported(r.Split('|')[0]))), "każdy affiks ma odpowiednik");
        }

        // ------------------------------------------------------------------ E02 kwalifikacja przedmiotu

        [Test]
        public void E02_QualificationMatrix_DescribesItemNotClass()
        {
            bool Q(string group, string item)
            {
                var d = D(item);
                var a = HellEconomy.Affixes.First(x => x.Group == group);
                return HellEconomy.Qualifies(a, d, HellEconomy.Category(d), HellEconomy.TagsOf(d));
            }
            // Postawa: tylko ciężka broń wręcz.
            Assert.IsTrue(Q("poise_damage", "hu_greataxe"));
            Assert.IsFalse(Q("poise_damage", "hu_bow"), "łuk");
            Assert.IsFalse(Q("poise_damage", "hu_staff"), "kostur");
            Assert.IsFalse(Q("poise_damage", "hu_sword_fire"), "miecz żywiołu");
            // Rzucanie: fokus albo grymuar.
            Assert.IsTrue(Q("cast_speed", "hu_staff"));
            Assert.IsTrue(Q("cast_speed", "hu_wand_fire"));
            Assert.IsTrue(Q("cast_speed", "hu_grimoire_fire"));
            Assert.IsFalse(Q("cast_speed", "hu_sword_fire"), "miecz żywiołu");
            Assert.IsFalse(Q("cast_speed", "hu_shield"), "tarcza");
            Assert.IsFalse(Q("cast_speed", "hu_offdagger"), "nóż do rzucania");
            // Blok: wyłącznie tarcza.
            Assert.IsTrue(Q("shield_block_cost", "hu_shield"));
            Assert.IsFalse(Q("shield_block_cost", "hu_grimoire_fire"));
            // Sługi: hełm albo grymuar przywołań.
            Assert.IsTrue(Q("minion_damage", "hu_item_head_heavy_t1"), "hełm – dowolna klasa");
            Assert.IsTrue(Q("minion_damage", "hu_grimoire_summoning"));
            Assert.IsFalse(Q("minion_damage", "hu_grimoire_fire"), "inny grymuar");
            Assert.IsFalse(Q("minion_damage", "hu_item_ring_iron"));
            // Przebicie: broń (także fizyczna), grymuar, amulet; nie tarcza, nóż, pierścień, zbroja.
            Assert.IsTrue(Q("fire_pen", "hu_sword"), "fizyczny miecz");
            Assert.IsTrue(Q("water_pen", "hu_grimoire_fire"));
            Assert.IsTrue(Q("air_pen", "hu_item_amulet_bone"));
            Assert.IsFalse(Q("earth_pen", "hu_shield"));
            Assert.IsFalse(Q("earth_pen", "hu_offdagger"));
            Assert.IsFalse(Q("fire_pen", "hu_item_ring_iron"));
            Assert.IsFalse(Q("fire_pen", "hu_item_head_heavy_t1"));
            Assert.IsFalse(HellEconomy.PassesFilter("nieznany", D("hu_sword")), "nieznany filtr nie rozdaje premii");
        }

        // ------------------------------------------------------------------ E03/E04 losowanie

        [Test]
        public void E03_E04_Roll_ThresholdsUniqueGroupsAndDeterminism()
        {
            var axe = D("hu_greataxe");
            var seen = new HashSet<string>();
            for (int seed = 0; seed < 400; seed++)
            {
                var it = HellEconomy.Roll(axe, LootSource.Shop, 20, new System.Random(seed), ItemRarity.Legendary);
                var groups = it.affixIds.Select(id => HellEconomy.Affixes.First(a => a.id == id).Group).ToList();
                Assert.AreEqual(groups.Count, groups.Distinct().Count(), "grupa raz na przedmiot");
                Assert.AreEqual(it.affixIds.Count, it.affixNames.Count);
                Assert.AreEqual(HellEconomy.AffixVersion, it.affixVersion);
                foreach (var id in it.affixIds) seen.Add(id);
            }
            Assert.IsTrue(seen.Any(id => id.StartsWith("affix.turris.poise_damage")), "postawa wypada na ciężkiej broni");
            Assert.IsTrue(seen.Any(id => id.StartsWith("affix.turris.") && id.Contains("_pen.")), "przebicie wypada na broni");
            Assert.IsFalse(seen.Any(id => id.Contains("cast_speed") || id.Contains("minion") || id.Contains("shield_block")), "nie na ciężkiej broni");

            // Progi: przebicie od piętra 3 (T1), 10 (T2), 18 (T3); postawa T3 od 16.
            for (int seed = 0; seed < 300; seed++)
            {
                var early = HellEconomy.Roll(axe, LootSource.Shop, 2, new System.Random(seed), ItemRarity.Legendary);
                Assert.IsFalse(early.affixIds.Any(id => id.Contains("_pen.")), "przebicie dopiero od piętra 3");
                var mid = HellEconomy.Roll(axe, LootSource.Shop, 17, new System.Random(seed), ItemRarity.Legendary);
                Assert.IsFalse(mid.affixIds.Any(id => id.Contains("_pen.t3")), "T3 przebicia od piętra 18");
                Assert.IsFalse(HellEconomy.Roll(axe, LootSource.Shop, 15, new System.Random(seed), ItemRarity.Legendary).affixIds.Any(id => id == "affix.turris.poise_damage.t3"));
            }

            // Ten sam seed i dane – identyczne losowanie.
            var a1 = HellEconomy.Roll(axe, LootSource.Elite, 12, new System.Random(42));
            var a2 = HellEconomy.Roll(axe, LootSource.Elite, 12, new System.Random(42));
            CollectionAssert.AreEqual(a1.affixIds, a2.affixIds);
            CollectionAssert.AreEqual(a1.affixes.Select(m => m.value).ToArray(), a2.affixes.Select(m => m.value).ToArray());
        }

        [Test]
        public void E03_AddedElementAffixes_RollRealElementStats()
        {
            var sword = D("hu_sword");
            var stats = new HashSet<StatType>();
            for (int seed = 0; seed < 300; seed++)
            {
                var it = HellEconomy.Roll(sword, LootSource.Shop, 20, new System.Random(seed), ItemRarity.Legendary);
                foreach (var m in it.affixes) stats.Add(m.stat);
            }
            Assert.IsTrue(stats.Contains(StatType.AddedWeaponFire) && stats.Contains(StatType.AddedWeaponFrost)
                          && stats.Contains(StatType.AddedWeaponLightning) && stats.Contains(StatType.AddedWeaponEarth), "cztery dodane żywioły");
            var m2 = HellEconomy.MapAffix("FireDamage", "Flat", 4f).Value;
            Assert.AreEqual(StatType.AddedWeaponFire, m2.stat);
            Assert.AreEqual(14f, m2.value, 1e-4f, "3,5% bazy broni za punkt HU (nominalnie +7–49%)");
            Assert.AreEqual(StatType.FireDamage, HellEconomy.MapAffix("FireDamage", "PercentAdditive", 0.1f).Value.stat, "procentowa premia ognia bez zmian");
            Assert.AreEqual(StatType.PhysicalDamage, HellEconomy.Map("FireDamage|Flat|4").Value.stat, "przedmioty bazowe i perki – dawna mapa bez zmian");
        }

        // ------------------------------------------------------------------ E05 jakość

        [Test]
        public void E05_Quality_ScalesNewBonusesOnce_WeaponLevelDoesNot()
        {
            var it = new ItemInstance(D("hu_staff"), 3) { huQuality = 2, affixVersion = HellEconomy.AffixVersion };
            it.affixes.Add(M(StatType.CastSpeed, 12f));
            it.affixes.Add(M(StatType.HeavyPoiseDamage, 25f));
            it.affixes.Add(M(StatType.MinionDamage, 30f));
            it.affixes.Add(M(StatType.ShieldBlockCost, 20f));
            it.affixEffects.Add(new PassiveEffect(PassiveEffectType.PenetrationFire, 10f));
            var mods = it.Modifiers.Where(m => m.stat >= StatType.AddedWeaponFire).ToDictionary(m => m.stat, m => m.value);
            Assert.AreEqual(18f, mods[StatType.CastSpeed], 1e-4f);
            Assert.AreEqual(37.5f, mods[StatType.HeavyPoiseDamage], 1e-4f);
            Assert.AreEqual(45f, mods[StatType.MinionDamage], 1e-4f);
            Assert.AreEqual(30f, mods[StatType.ShieldBlockCost], 1e-4f);
            Assert.AreEqual(15f, it.Effects.Single(e => e.type == PassiveEffectType.PenetrationFire).value, 1e-4f, "przebicie 10 p.p. → 15 p.p.");
            Assert.AreEqual(1.6f, it.WeaponLevelFactor, 1e-4f, "poziom broni działa na bazę broni, nie na procent affiksu");

            var q0 = new ItemInstance(D("hu_staff")) { affixVersion = HellEconomy.AffixVersion };
            q0.affixes.Add(M(StatType.CastSpeed, 12f));
            Assert.AreEqual(12f, q0.Modifiers.Single(m => m.stat == StatType.CastSpeed).value, 1e-4f);
            var q1 = new ItemInstance(D("hu_staff")) { huQuality = 1, affixVersion = HellEconomy.AffixVersion };
            q1.affixes.Add(M(StatType.AddedWeaponFire, 20f));
            Assert.AreEqual(25f, q1.Modifiers.Single(m => m.stat == StatType.AddedWeaponFire).value, 1e-4f);
        }

        // ------------------------------------------------------------------ E06/E07 dodane żywioły i konwersja

        [Test]
        public void E06_ReferenceExample_B100_PhysicalAndFireSeparate()
        {
            // B = 100, broń fizyczna, premia fizyczna +20% (zawarta w ciosie), dodany ogień +20%, premia ognia +50%.
            var hit = new HitData { physical = 100f * 1.2f };
            hit.AddMagic(Element.Fire, AffixMath.AddedElement(100f, 20f));
            hit.ScaleMagicByElement(e => e == Element.Fire ? 1.5f : 1f);
            Assert.AreEqual(120f, hit.physical, 1e-3f);
            Assert.AreEqual(30f, hit.MagicOf(Element.Fire), 1e-3f, "ogień 30, nie 36");
            Assert.AreEqual(150f, hit.Total, 1e-3f);
            // Jakość 2: nominalne +20% → efektywne +30%, bez drugiego skalowania.
            Assert.AreEqual(30f, AffixMath.AddedElement(100f, 20f * 1.5f), 1e-4f);
            // Zero dodatku – trafienie bez zmian (dawny zapis).
            var plain = new HitData { physical = 50f, magic = 10f, element = Element.Frost };
            plain.AddMagic(Element.Fire, AffixMath.AddedElement(100f, 0f));
            Assert.IsFalse(plain.split.active);
            Assert.AreEqual(10f, plain.MagicOf(Element.Frost), 1e-4f);
            Assert.IsNull(plain.statuses, "sam dodatek nie nakłada efektów");
        }

        [Test]
        public void E06_FourAddedElements_PlusUnelementalMagic_NothingLost()
        {
            var hit = new HitData { physical = 40f, magic = 10f, element = Element.None }; // magia bez żywiołu (różdżka)
            hit.AddMagic(Element.Fire, 5f); hit.AddMagic(Element.Frost, 6f); hit.AddMagic(Element.Lightning, 7f); hit.AddMagic(Element.Earth, 8f);
            Assert.IsTrue(hit.split.active);
            Assert.AreEqual(10f, hit.MagicOf(Element.None), 1e-4f);
            Assert.AreEqual(5f, hit.MagicOf(Element.Fire), 1e-4f);
            Assert.AreEqual(6f, hit.MagicOf(Element.Frost), 1e-4f);
            Assert.AreEqual(7f, hit.MagicOf(Element.Lightning), 1e-4f);
            Assert.AreEqual(8f, hit.MagicOf(Element.Earth), 1e-4f);
            Assert.AreEqual(76f, hit.Total, 1e-4f);
            Assert.AreEqual(Element.Earth, hit.element, "opis: dominujący żywioł");
            Assert.IsTrue(hit.Has(Element.Fire) && hit.Has(Element.Frost) && hit.Has(Element.Lightning) && hit.Has(Element.Earth));
            // Premie każdego żywiołu raz, magia bez żywiołu bez premii żywiołu.
            hit.ScaleMagicByElement(e => e == Element.Fire ? 2f : e == Element.Earth ? 0.5f : 1f);
            Assert.AreEqual(10f, hit.MagicOf(Element.None), 1e-3f);
            Assert.AreEqual(10f, hit.MagicOf(Element.Fire), 1e-3f);
            Assert.AreEqual(4f, hit.MagicOf(Element.Earth), 1e-3f);
            Assert.AreEqual(6f, hit.MagicOf(Element.Frost), 1e-3f);
            // Kopia trafienia ma własny podział (struktura wartościowa).
            var copy = hit; copy.AddMagic(Element.Fire, 100f);
            Assert.AreEqual(10f, hit.MagicOf(Element.Fire), 1e-3f, "kopia nie zmienia oryginału");
        }

        [Test]
        public void E07_ConvertFireToFrost_KeepsSum_FrostBonusOnce()
        {
            var hit = new HitData { physical = 50f, magic = 20f, element = Element.Lightning };
            hit.AddMagic(Element.Fire, 30f);
            float before = hit.Total;
            hit.ConvertElement(Element.Fire, Element.Frost);
            Assert.AreEqual(before, hit.Total, 1e-4f, "suma przed premiami");
            Assert.AreEqual(0f, hit.MagicOf(Element.Fire), 1e-4f, "bez kopii ognia");
            Assert.AreEqual(30f, hit.MagicOf(Element.Frost), 1e-4f);
            hit.ScaleMagicByElement(e => e == Element.Frost ? 1.5f : e == Element.Fire ? 3f : 1f);
            Assert.AreEqual(45f, hit.MagicOf(Element.Frost), 1e-3f, "mróz dostaje premię mrozu raz, nie premię ognia");
            Assert.AreEqual(20f, hit.MagicOf(Element.Lightning), 1e-3f);
        }

        [Test]
        public void E06_Player_AddedFire_UsesBaseWithoutPhysicalBonus_ElementBonusOnce()
        {
            var run = fx.NewRun("class_knight");
            var sword = new ItemInstance(D("hu_sword")); // czysto fizyczny miecz
            run.inventory.Add(sword); run.EquipFromInventory(sword, EquipSlot.MainHand);
            Assert.IsTrue(run.equipment.Equip(Ring(M(StatType.AddedWeaponFire, 20f), M(StatType.FireDamage, 50f), M(StatType.PhysicalDamage, 20f)), EquipSlot.Ring1, null));
            var pc = fx.NewCombatant(run);
            pc.RefreshBuild();
            var b = pc.Build;
            var atk = b.weapon.light;
            float dmg = b.WeaponDamage(atk);
            float baseB = b.WeaponBase(atk);
            Assert.AreEqual(dmg / (1f + b.stats[StatType.PhysicalDamage] / 100f), baseB, 1e-3f);
            var hit = HitData.FromAttack(atk, dmg, Vector3.zero, pc);
            pc.AddWeaponElements(ref hit, baseB);
            Assert.AreEqual(baseB * 0.2f, hit.MagicOf(Element.Fire), 1e-3f, "dodatek = B × 20%");
            Assert.AreEqual(baseB * 0.2f, b.AddedElement(atk, Element.Fire), 1e-3f, "ta sama reguła w analizie buildu");
            float physBefore = hit.physical, fireBefore = hit.MagicOf(Element.Fire);
            pc.PrepareOutgoingForTest(ref hit);
            // Wspólne mnożniki (globalne, klasa) działają tak samo na oba składniki; ogień dostaje dodatkowo premię ognia, fizyczne nic.
            float common = hit.physical / physBefore;
            Assert.AreEqual(fireBefore * common * (1f + b.stats[StatType.FireDamage] / 100f), hit.MagicOf(Element.Fire), 1e-2f);
        }

        // ------------------------------------------------------------------ E08 przebicie

        [Test]
        public void E08_Penetration_PerElement_ReferenceResult36_VulnerabilityKept()
        {
            var hit = new HitData();
            hit.AddMagic(Element.Fire, 20f); hit.AddMagic(Element.Frost, 30f);
            hit.penFire = 0.10f;
            float Base(Element e) => e == Element.Fire ? 0.5f : e == Element.Frost ? 0.8f : 1f;
            DamageResolver.ApplyElementResistances(ref hit, Base, 0f);
            Assert.AreEqual(12f, hit.MagicOf(Element.Fire), 1e-3f, "50% − 10 p.p. = 40%");
            Assert.AreEqual(24f, hit.MagicOf(Element.Frost), 1e-3f, "mróz bez przebicia ognia");
            Assert.AreEqual(36f, hit.magic, 1e-3f);

            var weak = new HitData(); weak.AddMagic(Element.Fire, 20f); weak.penFire = 0.3f;
            DamageResolver.ApplyElementResistances(ref weak, e => 1.25f, 0f);
            Assert.AreEqual(25f, weak.magic, 1e-3f, "podatność bez dodatkowego mnożnika");

            var floor = new HitData(); floor.AddMagic(Element.Earth, 10f); floor.penEarth = 0.5f;
            DamageResolver.ApplyElementResistances(ref floor, e => 0.8f, 0f);
            Assert.AreEqual(10f, floor.magic, 1e-3f, "odporność nie schodzi poniżej 0");

            var cut = new HitData(); cut.AddMagic(Element.Lightning, 10f); cut.penLightning = 0.1f;
            DamageResolver.ApplyElementResistances(ref cut, e => 0.6f, 0.1f);
            Assert.AreEqual(8f, cut.magic, 1e-3f, "osłabienie odporności i przebicie sumują się do 0");
        }

        // ------------------------------------------------------------------ E09 blok tarczą

        [Test]
        public void E09_ShieldBlock_AppliedOnceOnLegacy_CapAndShieldOnly()
        {
            Assert.AreEqual(25.6f, 40f * 0.8f * AffixMath.ShieldBlockFactor(20f), 1e-4f, "40 → perk 32 → affiks 25,6");
            Assert.AreEqual(0.5f, AffixMath.ShieldBlockFactor(70f), 1e-4f, "limit nowej części 50%");
            Assert.IsTrue(AffixMath.ShieldBlockCapped(70f));

            var run = fx.NewRun("class_knight");
            var shield = new ItemInstance(D("hu_shield"));
            Assert.IsTrue(run.equipment.Equip(shield, EquipSlot.OffHand, null), "tarcza założona");
            var bare = BuildCalculator.Compute(run, fx.Cfg);
            Assert.IsTrue(bare.guardIsShield);
            float s0 = bare.guard.stabilityMultiplier;
            shield.affixVersion = HellEconomy.AffixVersion; shield.affixes.Add(M(StatType.ShieldBlockCost, 20f));
            var with = BuildCalculator.Compute(run, fx.Cfg);
            Assert.AreEqual(s0 * 0.8f, with.guard.stabilityMultiplier, 1e-4f);
            Assert.AreEqual(with.BlockCost(40f), 40f * with.guard.stabilityMultiplier, 1e-4f, "UI i rozstrzygnięcie – ta sama reguła");

            // Próg przełamania: koszt 25,6 przy 26 wytrzymałości – blok; bez affiksu (32) – przełamanie.
            var guard = new GuardData { stabilityMultiplier = 0.8f * AffixMath.ShieldBlockFactor(20f), physicalReduction = 1f, blockAngle = 180f };
            var def = new DefenseState { guarding = true, guard = guard, stamina = 26f, forward = Vector3.forward };
            var h = Fixture.FrontHit(50, 0, 40);
            Assert.AreEqual(HitOutcome.Blocked, DamageResolver.Resolve(h, def, fx.Cfg.balance).outcome);
            def.guard = new GuardData { stabilityMultiplier = 0.8f, physicalReduction = 1f, blockAngle = 180f };
            Assert.AreEqual(HitOutcome.GuardBroken, DamageResolver.Resolve(h, def, fx.Cfg.balance).outcome);

            // Bez tarczy (blok bronią) premia nie działa, nawet gdyby statystyka pochodziła z innego przedmiotu.
            var run2 = fx.NewRun("class_knight");
            run2.equipment.Unequip(EquipSlot.OffHand);
            var weaponGuard = BuildCalculator.Compute(run2, fx.Cfg);
            if (weaponGuard.guard != null)
            {
                float w0 = weaponGuard.guard.stabilityMultiplier;
                run2.equipment.Equip(Ring(M(StatType.ShieldBlockCost, 20f)), EquipSlot.Ring1, null);
                var w1 = BuildCalculator.Compute(run2, fx.Cfg);
                Assert.IsFalse(w1.guardIsShield);
                Assert.AreEqual(w0, w1.guard.stabilityMultiplier, 1e-5f, "blok bronią bez premii tarczy");
            }
        }

        // ------------------------------------------------------------------ E10 postawa

        [Test]
        public void E10_HeavyPoise_MultipliesPoiseOnly_HeavyMeleeOnly()
        {
            var run = fx.NewRun("class_knight");
            var axe = new ItemInstance(D("hu_greataxe"));
            run.inventory.Add(axe); run.EquipFromInventory(axe, EquipSlot.MainHand);
            run.equipment.Equip(Ring(M(StatType.HeavyPoiseDamage, 25f)), EquipSlot.Ring1, null);
            var pc = fx.NewCombatant(run);
            pc.RefreshBuild();
            Assert.IsTrue(pc.HoldsHeavyMelee);
            var hit = new HitData { physical = 100f, poiseDamage = 40f * 1.5f }; // ładowanie ×1,5
            pc.ApplyHeavyPoise(ref hit, false);
            Assert.AreEqual(75f, hit.poiseDamage, 1e-3f, "40 × 1,5 × 1,25");
            Assert.AreEqual(100f, hit.physical, 1e-4f, "zdrowie bez zmian");

            var zero = new HitData { poiseDamage = 0f }; pc.ApplyHeavyPoise(ref zero, false);
            Assert.AreEqual(0f, zero.poiseDamage, "źródło bez postawy");
            var rip = new HitData { poiseDamage = 40f, isRiposte = true }; pc.ApplyHeavyPoise(ref rip, false);
            Assert.AreEqual(40f, rip.poiseDamage, "riposta");
            var fxHit = new HitData { poiseDamage = 40f, featureEffect = true }; pc.ApplyHeavyPoise(ref fxHit, false);
            Assert.AreEqual(40f, fxHit.poiseDamage, "efekt wtórny cechy");
            var uq = new HitData { poiseDamage = 40f, uniqueEffect = true }; pc.ApplyHeavyPoise(ref uq, false);
            Assert.AreEqual(40f, uq.poiseDamage, "efekt wtórny unikatu");

            var sword = new ItemInstance(D("hu_sword"));
            run.inventory.Add(sword); run.EquipFromInventory(sword, EquipSlot.MainHand);
            pc.RefreshBuild();
            var light = new HitData { poiseDamage = 40f }; pc.ApplyHeavyPoise(ref light, false);
            Assert.AreEqual(40f, light.poiseDamage, "miecz nie jest ciężką bronią");
        }

        // ------------------------------------------------------------------ E11 szybkość rzucania

        [Test]
        public void E11_CastSpeed_DividesStartup_SpellsOnly()
        {
            Assert.AreEqual(0.6f / 1.1f, AffixMath.CastTime(0.6f, 10f), 1e-6f);
            Assert.AreEqual(0.54545f, AffixMath.CastTime(0.6f, 10f), 1e-5f);
            Assert.AreEqual(0f, AffixMath.CastTime(0f, 50f), "czas zero zostaje zerem");

            var run = fx.NewRun("class_knight");
            run.equipment.Equip(Ring(M(StatType.CastSpeed, 6f)), EquipSlot.Ring1, null);
            run.equipment.Equip(Ring(M(StatType.CastSpeed, 4f)), EquipSlot.Ring2, null);
            var pc = fx.NewCombatant(run);
            pc.RefreshBuild();
            Assert.AreEqual(10f, pc.Build.stats[StatType.CastSpeed], 1e-4f, "suma kilku źródeł");
            var spell = fx.bundle.all.OfType<SpellDefinition>().First(s => s != null && s.IsSpell && s.castTime > 0.1f);
            Assert.AreEqual(pc.ScaleStartup(spell.castTime) / 1.1f, pc.EffectiveCastTime(spell), 1e-5f);
            var tech = fx.bundle.all.OfType<SpellDefinition>().First(s => s != null && !s.IsSpell && s.castTime > 0.05f);
            Assert.AreEqual(pc.ScaleStartup(tech.castTime), pc.EffectiveCastTime(tech), 1e-6f, "technika bez skrócenia");
        }

        // ------------------------------------------------------------------ E12 zapis

        [Test]
        public void E12_Snapshot_NewItemKeepsRoll_OldItemKeepsBonuses_Idempotent()
        {
            var it = HellEconomy.Roll(D("hu_greataxe"), LootSource.Boss, 20, new System.Random(9), ItemRarity.Legendary);
            it.level = 2; it.powerSlot = 1;
            var json = JsonUtility.ToJson(RunSnapshot.Item.Of(it, 3));
            ItemInstance.BeginRestore();
            var back = JsonUtility.FromJson<RunSnapshot.Item>(json).Make(it.definition);
            CollectionAssert.AreEqual(it.affixIds, back.affixIds);
            CollectionAssert.AreEqual(it.affixes.Select(m => (m.stat, m.value)).ToArray(), back.affixes.Select(m => (m.stat, m.value)).ToArray());
            Assert.AreEqual(it.affixVersion, back.affixVersion);
            Assert.AreEqual(it.uid, back.uid); Assert.AreEqual(2, back.level); Assert.AreEqual(1, back.powerSlot);
            Assert.AreEqual(it.huQuality, back.huQuality); Assert.AreEqual(it.rarity, back.rarity);
            // Ponowny zapis – identyczny wynik (bez ponownego losowania).
            Assert.AreEqual(json, JsonUtility.ToJson(RunSnapshot.Item.Of(back, 3)));

            // Stary zapis: brak nowych pól, dawny „dodany ogień” jako premia fizyczna – zostaje, bez przeliczenia.
            const string old = "{\"id\":\"hu_sword\",\"level\":0,\"slot\":-1,\"uid\":0,\"powerSlot\":0,\"rarity\":1,\"quality\":0," +
                               "\"affixes\":[{\"stat\":14,\"mode\":0,\"value\":15.0}],\"affixNames\":[\"affix.fire_add.t1\"],\"affixEffects\":[]}";
            ItemInstance.BeginRestore();
            var legacy = JsonUtility.FromJson<RunSnapshot.Item>(old).Make(D("hu_sword"));
            Assert.AreEqual(0, legacy.affixVersion);
            Assert.AreEqual(0, legacy.affixIds.Count);
            Assert.AreEqual(StatType.PhysicalDamage, legacy.affixes.Single().stat);
            Assert.AreEqual(15f, legacy.affixes.Single().value, 1e-4f);
            Assert.IsTrue(GameUI.LegacyAddedElements(legacy), "opis: premia obrażeń broni ze starego zapisu");
            StringAssert.Contains("ze starego zapisu", GameUI.ItemLines(legacy));
            // Ponowne zapisanie nie udaje przeliczenia; dwukrotna migracja – ten sam wynik.
            var again = JsonUtility.FromJson<RunSnapshot.Item>(JsonUtility.ToJson(RunSnapshot.Item.Of(legacy))).Make(D("hu_sword"));
            Assert.AreEqual(0, again.affixVersion);
            Assert.AreEqual(15f, again.affixes.Single().value, 1e-4f);
            Assert.IsFalse(GameUI.LegacyAddedElements(it), "nowy egzemplarz bez dopisku starego zapisu");
        }

        [Test]
        public void E12_RunSnapshot_RoundTripsBackpackAndEquipment()
        {
            var run = fx.NewRun("class_knight");
            var rolled = HellEconomy.Roll(D("hu_item_head_heavy_t1"), LootSource.Elite, 16, new System.Random(4), ItemRarity.Legendary);
            run.inventory.Add(rolled);
            var snap = RunSnapshot.Capture(run);
            var json = JsonUtility.ToJson(snap);
            var restored = JsonUtility.FromJson<RunSnapshot>(json).Restore(fx.Cfg);
            var back = restored.inventory.Single(i => i.definition == rolled.definition);
            CollectionAssert.AreEqual(rolled.affixIds, back.affixIds);
            Assert.AreEqual(rolled.affixVersion, back.affixVersion);
        }

        // ------------------------------------------------------------------ E13 zakładanie

        [Test]
        public void E13_BackpackItemGivesNothing_EquipAndRemove_PerHandBase()
        {
            var run = fx.NewRun("class_knight");
            var ring = Ring(M(StatType.AddedWeaponFrost, 30f));
            run.inventory.Add(ring);
            Assert.AreEqual(0f, BuildCalculator.Compute(run, fx.Cfg).stats[StatType.AddedWeaponFrost], "plecak nie daje premii");
            run.EquipFromInventory(ring, EquipSlot.Ring1);
            Assert.AreEqual(30f, BuildCalculator.Compute(run, fx.Cfg).stats[StatType.AddedWeaponFrost], 1e-4f);
            run.equipment.Unequip(EquipSlot.Ring1);
            Assert.AreEqual(0f, BuildCalculator.Compute(run, fx.Cfg).stats[StatType.AddedWeaponFrost], "zdjęcie usuwa premię");

            // Dwie bronie: każda ręka ma własną bazę B (dodatek drugiej ręki nie z bazy pierwszej).
            var run2 = fx.NewRun("class_knight");
            run2.equipment.Unequip(EquipSlot.OffHand);
            var main = new ItemInstance(D("hu_sword_broad")); var off = new ItemInstance(D("hu_sword"));
            run2.inventory.Add(main); run2.EquipFromInventory(main, EquipSlot.MainHand);
            run2.inventory.Add(off); run2.EquipFromInventory(off, EquipSlot.OffHand);
            run2.equipment.Equip(Ring(M(StatType.AddedWeaponFire, 20f)), EquipSlot.Ring1, null);
            var b = BuildCalculator.Compute(run2, fx.Cfg);
            if (b.offWeapon != null)
            {
                var atk = b.weapon.light;
                Assert.AreEqual(b.WeaponBase(atk, true) * 0.2f, b.AddedElement(atk, Element.Fire, true), 1e-3f);
                Assert.AreEqual(b.WeaponBase(atk) * 0.2f, b.AddedElement(atk, Element.Fire), 1e-3f);
            }
        }

        // ------------------------------------------------------------------ E14 jedno trafienie

        [Test]
        public void E14_MixedHit_OneResolution_GuardReducesComponentsTogether()
        {
            var hit = Fixture.FrontHit(60f, 0f, 20f);
            hit.AddMagic(Element.Fire, 10f); hit.AddMagic(Element.Frost, 10f); hit.AddMagic(Element.Earth, 10f);
            var def = new DefenseState { forward = Vector3.forward, magicResist = 0.5f };
            var r = DamageResolver.Resolve(hit, def, fx.Cfg.balance);
            Assert.AreEqual(HitOutcome.Hit, r.outcome);
            Assert.AreEqual(60f + 30f * 0.5f, r.healthDamage, 1e-3f, "ogólna obrona magiczna na całą część magiczną, jeden wynik");
            Assert.AreEqual(hit.poiseDamage, r.poiseDamage, "jedna postawa");
            def.guarding = true; def.guard = new GuardData { stabilityMultiplier = 1f, physicalReduction = 0.5f, magicReduction = 0.2f, blockAngle = 180f }; def.stamina = 100f;
            var g = DamageResolver.Resolve(hit, def, fx.Cfg.balance);
            Assert.AreEqual(HitOutcome.Blocked, g.outcome);
            Assert.AreEqual(20f, g.staminaDamage, 1e-4f, "jeden koszt bloku za cios mieszany");
        }
    }
}
