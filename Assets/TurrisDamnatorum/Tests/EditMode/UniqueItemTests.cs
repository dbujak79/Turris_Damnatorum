using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>
    /// Natywne unikaty Turris (docs/PROMPT_WDROZENIE_75_UNIKATOW_KLASOWYCH.md, rozdz. 28.1–28.3): katalog i rejestr, rzadkość,
    /// ekonomia, łup z preferencją, zapis, sloty i duplikaty, uśpienie bez cechy oraz kontrakty budżetów mocy na prawdziwym PlayerCombat.
    /// </summary>
    public class UniqueItemTests
    {
        Fixture fx;
        [SetUp] public void SetUp() { fx = new Fixture(); UniqueRegistry.ResetForTests(); }
        [TearDown] public void TearDown() { fx.Cleanup(); UniqueRegistry.ResetForTests(); }

        ItemDefinition U(string shortId) => UniqueRegistry.Find(fx.Cfg, "item.unique.turris." + shortId);
        ItemInstance Make(string shortId, int quality = 0) { var it = HellEconomy.Roll(U(shortId), LootSource.Boss, 8, new System.Random(3)); it.huQuality = quality; return it; }

        PlayerCombat Hero(string classId, string featureId = null, params string[] uniques)
        {
            var plan = fx.Plan(classId);
            plan.classFeatureId = featureId;
            var run = RunFactory.Create(plan, fx.Cfg, 7);
            foreach (var u in uniques)
            {
                var it = Make(u);
                run.inventory.Add(it);
                Assert.IsTrue(run.EquipFromInventory(it, it.definition.DefaultSlot), u + ": założenie");
            }
            return fx.NewCombatant(run);
        }

        // ================================================================== Dane

        [Test]
        public void Catalog_75Cards_FivePerClass_30New_UniqueIds_ClassesExist()
        {
            var all = UniqueCatalog.All;
            Assert.AreEqual(75, all.Count);
            Assert.AreEqual(75, all.Select(c => c.id).Distinct().Count(), "unikalne id");
            Assert.AreEqual(75, all.Select(c => c.code).Distinct().Count(), "unikalne kody");
            Assert.AreEqual(30, all.Count(c => c.isNew), "30 nowych kart (pozycje 04–05)");
            var groups = all.GroupBy(c => c.classId).ToList();
            Assert.AreEqual(15, groups.Count);
            foreach (var g in groups)
            {
                Assert.AreEqual(5, g.Count(), g.Key);
                Assert.IsNotNull(fx.Cfg.classes.FirstOrDefault(c => c != null && c.id == g.Key), "klasa istnieje: " + g.Key);
            }
            foreach (var c in all)
            {
                Assert.IsTrue(c.id.StartsWith("item.unique.turris."), c.id);
                Assert.IsNotEmpty(c.name); Assert.IsNotEmpty(c.promise); Assert.IsNotEmpty(c.Rule); Assert.IsNotEmpty(c.Cost); Assert.IsNotEmpty(c.condition);
                Assert.IsNotEmpty(c.coreBuild); Assert.IsNotEmpty(c.developedBuild); Assert.IsNotEmpty(c.tradeoff);
                Assert.IsFalse(c.Rule.Contains("{") || c.Cost.Contains("{"), c.code + ": szablon bez brakującego parametru");
                Assert.Greater(c.stats.Count, 0, c.code + ": kierunek statystyk");
                if (c.requiredFeature != null) Assert.IsNotNull(ClassFeatureCatalog.Get(c.requiredFeature), c.code + ": cecha istnieje");
                if (c.requiredFeature != null) Assert.AreEqual(c.classId, ClassFeatureCatalog.Get(c.requiredFeature).classId, c.code + ": cecha należy do klasy karty");
                foreach (var s in c.skills) Assert.IsTrue(fx.Cfg.baseSpellPool.Any(x => x != null && x.id == s) || fx.bundle.byId.ContainsKey(s), c.code + ": skill " + s);
                Assert.IsNotNull(UniquePowerFactory.Create(c), c.code + ": kontroler mocy");
            }
        }

        [Test]
        public void Registry_Builds75NativeUniques_RightSlots_NoFakeHuSource_NotInShopPool()
        {
            var items = UniqueRegistry.Items(fx.Cfg);
            Assert.AreEqual(75, items.Count, "każda karta ma przedmiot (bazy istnieją)");
            foreach (var d in items)
            {
                var c = UniqueRegistry.CardOf(d);
                Assert.IsNotNull(c);
                Assert.IsTrue(d.IsNativeUnique && HellEconomy.IsUnique(d) && !HellEconomy.IsHuUnique(d), d.id);
                Assert.IsTrue(string.IsNullOrEmpty(d.huSourceId), d.id + ": bez fałszywego huSourceId");
                Assert.AreEqual(c.kind, d.kind, d.id);
                Assert.IsTrue(d.FitsSlot(d.DefaultSlot), d.id);
                if (c.baseItemId != null) Assert.IsNotNull(d.familyBase, d.id + ": broń/fokus z bazą");
                Assert.IsFalse(fx.Cfg.baseItemPool.Contains(d), d.id + ": poza zwykłą pulą (sklep, nagrody)");
                Assert.IsNotNull(RunSnapshot.Index(fx.Cfg)[d.id], "indeks zapisu");
            }
            // Bronie zachowują rodzaj bazy (wymagania skilli): łuk to Ranged, młot jednoręczny nie jest bronią ciężką.
            Assert.AreEqual(WeaponFamilies.Of(fx.Get<ItemDefinition>("hu_bow")), WeaponFamilies.Of(U("archer.gravediggers_bow")));
            Assert.AreEqual(0, (int)(WeaponFamilies.Of(U("fire.borrowed_sun")) & BuildTag.HeavyWeapon), "młot jednoręczny ≠ broń ciężka");
            Assert.IsTrue(U("archer.gravediggers_bow").twoHanded && U("berserker.walking_slaughter").twoHanded);
            Assert.AreEqual(ItemKind.OffHand, U("mage.sleeping_circles").kind);
            Assert.IsNotNull(U("mage.sleeping_circles").grantedSkill, "grymuar daje swoją novę");
        }

        [Test]
        public void Roll_KeepsUnique_ValueAndUpgrade_QualityScalesOnlyStats()
        {
            var d = U("warrior.bellringer");
            var rng = new System.Random(1);
            foreach (var src in new[] { LootSource.Normal, LootSource.Elite, LootSource.Boss })
            {
                var it = HellEconomy.Roll(d, src, 1, rng);
                Assert.AreEqual(ItemRarity.Unique, it.rarity, "rodzimy unikat nie spada do Legendary/Rare");
                Assert.AreEqual(0, it.affixes.Count);
            }
            var q0 = Make("warrior.bellringer", 0); var q2 = Make("warrior.bellringer", 2);
            Assert.Greater(HellEconomy.Value(q0), 300, "wartość unikatu");
            Assert.Greater(q2.Modifiers.First().value, q0.Modifiers.First().value, "jakość skaluje statystyki");
            Assert.AreEqual(UniqueCatalog.Get(d.id).P("wave_share"), 0.55f, 1e-4f, "parametr mocy strukturalny (niezależny od egzemplarza)");
            // Broń unikatowa ma drogę ulepszeń (poziom, droższy) – unikat HU nie.
            var axe = Make("berserker.walking_slaughter");
            Assert.Greater(HellEconomy.MaxLevelOf(axe.definition), 0);
            Assert.Greater(HellEconomy.WeaponLevelPrice(axe), HellEconomy.WeaponLevelPrice(new ItemInstance(fx.Get<ItemDefinition>("hu_greataxe"))));
            var run = fx.NewRun("class_hu_berserker");
            run.inventory.Add(axe); run.EquipFromInventory(axe, EquipSlot.MainHand);
            var offers = FloorShop.Generate(run, fx.Cfg, new System.Random(5));
            Assert.IsTrue(offers.Any(o => o.kind == FloorOffer.Kind.WeaponLevel && o.item == axe), "oferta poziomu unikatowej broni");
            Assert.IsFalse(offers.Any(o => o.item != null && o.item.definition.IsNativeUnique && o.kind != FloorOffer.Kind.WeaponLevel), "sklep nie sprzedaje unikatów");
        }

        [Test]
        public void Snapshot_RoundTrip_KeepsUidQualityPowerSlot_NoCollisions_AllSlots()
        {
            var run = fx.NewRun("class_knight");
            var ring1 = Make("berserker.held_rage", 1); var ring2 = Make("berserker.held_rage", 2);
            var amulet = Make("warrior.empty_front"); var shield = Make("warrior.last_pavise");
            var spare = Make("battlemage.parrying_rune"); spare.powerSlot = 2;
            run.inventory.AddRange(new[] { ring1, ring2, amulet, shield });
            run.EquipFromInventory(ring1, EquipSlot.Ring1); run.EquipFromInventory(ring2, EquipSlot.Ring2);
            run.EquipFromInventory(amulet, EquipSlot.Amulet); run.EquipFromInventory(shield, EquipSlot.OffHand);
            run.inventory.Add(spare);
            var snap = RunSnapshot.Capture(run);
            var json = JsonUtility.ToJson(snap);
            var back = JsonUtility.FromJson<RunSnapshot>(json).Restore(fx.Cfg);
            Assert.IsNotNull(back);
            Assert.AreEqual(ring1.uid, back.equipment.Get(EquipSlot.Ring1).uid);
            Assert.AreEqual(ring2.uid, back.equipment.Get(EquipSlot.Ring2).uid);
            Assert.AreEqual(2, back.equipment.Get(EquipSlot.Ring2).huQuality);
            Assert.AreEqual(ItemRarity.Unique, back.equipment.Get(EquipSlot.Amulet).rarity);
            Assert.AreEqual(shield.definition.id, back.equipment.Get(EquipSlot.OffHand).definition.id);
            var sp = back.inventory.First(i => i.definition.id == spare.definition.id);
            Assert.AreEqual(2, sp.powerSlot, "ustawienie mocy w zapisie");
            var uids = back.equipment.Items.Select(i => i.uid).Concat(back.inventory.Select(i => i.uid)).ToList();
            Assert.AreEqual(uids.Count, uids.Distinct().Count(), "bez kolizji uid");
            // Ten sam zapis wczytany drugi raz – nadal bez kolizji wewnątrz podejścia.
            var again = JsonUtility.FromJson<RunSnapshot>(json).Restore(fx.Cfg);
            var u2 = again.equipment.Items.Select(i => i.uid).Concat(again.inventory.Select(i => i.uid)).ToList();
            Assert.AreEqual(u2.Count, u2.Distinct().Count());
            // Stary zapis bez pola uid – nowe id, przedmiot działa.
            snap.equipment.ForEach(e => e.uid = 0);
            var old = snap.Restore(fx.Cfg);
            Assert.IsNotNull(old.equipment.Get(EquipSlot.Ring1));
        }

        // ================================================================== Łup

        [Test]
        public void Loot_PoolRespectsFloorAndSource_OwnClassPreferred_SoughtModerate()
        {
            var run = fx.NewRun("class_knight");
            run.floorsCleared = 0;
            var early = UniqueLoot.Pool(fx.Cfg, run, LootSource.Elite, null);
            Assert.IsFalse(early.Any(p => UniqueRegistry.CardOf(p.Key)?.minFloor > 0), "próg piętra");
            run.floorsCleared = 10;
            var elite = UniqueLoot.Pool(fx.Cfg, run, LootSource.Elite, null);
            Assert.IsFalse(elite.Any(p => p.Key.IsNativeUnique && p.Key.IsWeapon), "bronie – tylko z bossa");
            var boss = UniqueLoot.Pool(fx.Cfg, run, LootSource.Boss, null);
            Assert.IsTrue(boss.Any(p => p.Key.IsNativeUnique && p.Key.IsWeapon));
            Assert.AreEqual(75, boss.Count(p => p.Key.IsNativeUnique), "wszystkie 75 osiągalne z bossa na wysokim piętrze");
            // Symulacja: udział własnej klasy i poszukiwanego (deterministyczny seed).
            var rng = new System.Random(11);
            string soughtId = "item.unique.turris.warrior.bellringer";
            int own = 0, sought = 0, plainSought = 0; const int N = 4000;
            for (int i = 0; i < N; i++)
            {
                var d = UniqueLoot.Pick(fx.Cfg, run, LootSource.Boss, soughtId, rng);
                if (UniqueRegistry.CardOf(d)?.classId == "class_knight") own++;
                if (d.id == soughtId) sought++;
                if (UniqueLoot.Pick(fx.Cfg, run, LootSource.Boss, null, rng).id == soughtId) plainSought++;
            }
            float soughtShare = sought / (float)N, plainShare = plainSought / (float)N, ownShare = own / (float)N;
            TestContext.WriteLine($"własna klasa {ownShare:P1}, poszukiwany {soughtShare:P1}, bez wskazania {plainShare:P1}");
            Assert.Greater(soughtShare, plainShare * 2.5f, "wskazanie realnie pomaga");
            Assert.That(soughtShare, Is.InRange(0.08f, 0.35f), "umiarkowana preferencja, nie gwarancja");
            Assert.Greater(ownShare, 5f / 75f * 1.5f, "własna klasa częściej niż równy podział");
            Assert.Less(ownShare, 0.6f, "nadal znaleziska spoza klasy");
        }

        // ================================================================== Wyposażenie, duplikaty, uśpienie

        [Test]
        public void TwoSameRings_OnePower_BackpackItemHasNoPower_UnequipDisposes()
        {
            var pc = Hero("class_hu_berserker", null, "berserker.held_rage");
            var second = Make("berserker.held_rage");
            pc.Run.inventory.Add(second); pc.Run.EquipFromInventory(second, EquipSlot.Ring2); pc.RefreshBuild();
            Assert.AreEqual(1, pc.Uniques.All.Count, "dwa pierścienie tej samej definicji – jedna legenda");
            var bag = Make("warrior.bellringer");
            pc.Run.inventory.Add(bag); pc.RefreshBuild();
            Assert.IsNull(pc.Uniques.Get(bag.definition.id), "przedmiot w plecaku nie działa");
            pc.Run.UnequipToInventory(EquipSlot.Ring1); pc.Run.UnequipToInventory(EquipSlot.Ring2); pc.RefreshBuild();
            Assert.AreEqual(0, pc.Uniques.All.Count, "zdjęcie zamyka moc");
        }

        [Test]
        public void FeaturePower_DormantWithoutOwnersFeature_AndWithForeignClass()
        {
            var right = Hero("class_hu_berserker", "class_feature.berserker.redline", "berserker.hungry_scars");
            Assert.IsNull(right.Uniques.All[0].Dormant(), "właściwa cecha");
            var wrongVariant = Hero("class_hu_berserker", "class_feature.berserker.fury_spend", "berserker.hungry_scars");
            StringAssert.Contains("uśpiona", wrongVariant.Uniques.All[0].Dormant());
            var foreign = Hero("class_knight", null, "berserker.hungry_scars");
            StringAssert.Contains("uśpiona", foreign.Uniques.All[0].Dormant(), "cudza klasa nie dostaje cechy z przedmiotu");
            Assert.IsNull(foreign.Feature as RedLineFeature);
            // Ogólna moc działa na każdej klasie.
            var anyone = Hero("class_mage", null, "warrior.bellringer");
            Assert.IsNull(anyone.Uniques.All[0].Dormant());
        }

        [Test]
        public void DoubleSecret_WandOffhandOnlyWithGloves_RevertsToBackpack()
        {
            var run = fx.NewRun("class_hu_dual");
            var wand = new ItemInstance(fx.Get<ItemDefinition>("hu_wand_fire"));
            run.inventory.Add(wand);
            Assert.IsFalse(run.EquipFromInventory(wand, EquipSlot.OffHand), "bez rękawic różdżka nie wchodzi do lewej");
            var gloves = Make("dual.double_secret");
            run.inventory.Add(gloves); run.EquipFromInventory(gloves, EquipSlot.Hands);
            Assert.IsTrue(run.EquipFromInventory(wand, EquipSlot.OffHand), "z rękawicami – wyjątek lokalny");
            var snap = BuildCalculator.Compute(run, fx.Cfg);
            Assert.IsNull(snap.offWeapon, "różdżka nie jest drugim ostrzem");
            Assert.IsNotNull(snap.offWand);
            run.UnequipToInventory(EquipSlot.Hands);
            Assert.IsNull(run.equipment.Get(EquipSlot.OffHand), "zdjęcie rękawic odsyła różdżkę");
            Assert.IsTrue(run.inventory.Contains(wand));
            Assert.IsFalse(fx.Get<ItemDefinition>("hu_wand_fire").FitsSlot(EquipSlot.OffHand), "FitsSlot bez globalnej zmiany");
        }

        // ================================================================== Kontrakty budżetów (prawdziwy PlayerCombat)

        [Test]
        public void LastPavise_GuardWindowBlock_PaysManaFirst_PartialRestFromStamina()
        {
            var pc = Hero("class_knight", "class_feature.warrior.guard", "warrior.last_pavise");
            pc.DebugNoPassiveBlock = true;
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.IsTrue(((GuardFeature)pc.Feature).Active);
            pc.SetBlockHeld(true);
            float mana = pc.Mana.Current, st = pc.Stamina.Current;
            var r = pc.ReceiveHit(Fixture.FrontHit(40));
            Assert.AreEqual(HitOutcome.Blocked, r.outcome);
            Assert.Less(pc.Mana.Current, mana, "blok płaci maną");
            Assert.AreEqual(st, pc.Stamina.Current, 0.01f, "wytrzymałość nietknięta przy pełnej manie");
            pc.Mana.Drain(pc.Mana.Current - 1f);
            float st2 = pc.Stamina.Current;
            pc.ReceiveHit(Fixture.FrontHit(40));
            Assert.AreEqual(0f, pc.Mana.Current, 0.05f, "częściowa mana zużyta");
            Assert.Less(pc.Stamina.Current, st2, "reszta z wytrzymałości");
            // Bez okna cechy – zwykły koszt.
            var plain = Hero("class_knight", "class_feature.warrior.arsenal", "warrior.last_pavise");
            plain.DebugNoPassiveBlock = true; plain.SetBlockHeld(true);
            float m3 = plain.Mana.Current;
            plain.ReceiveHit(Fixture.FrontHit(40));
            Assert.AreEqual(m3, plain.Mana.Current, 0.01f, "inna cecha – uśpiona");
        }

        [Test]
        public void MoneylendersNote_RegenDoesNotRepayDebt()
        {
            var pc = Hero("class_mage", "class_feature.mage.arcane_debt", "mage.moneylenders_note");
            pc.Run.manaDebt = 30f;
            pc.Mana.Drain(pc.Mana.Current);
            Fixture.Advance(pc, 2f);
            Assert.AreEqual(30f, pc.Run.manaDebt, 0.01f, "regeneracja nie spłaca długu");
            Assert.AreEqual(0f, pc.Mana.Current, 0.01f, "ani nie tworzy osobnego zapasu");
            var plain = Hero("class_mage", "class_feature.mage.arcane_debt");
            plain.Run.manaDebt = 30f; plain.Mana.Drain(plain.Mana.Current);
            Fixture.Advance(plain, 2f);
            Assert.Less(plain.Run.manaDebt, 30f, "bez weksla regeneracja spłaca (zasada cechy)");
        }

        [Test]
        public void HeldRage_NoRageSpeed_CutsConsumeRageSeconds()
        {
            var pc = Hero("class_hu_berserker", "class_feature.berserker.fury_spend", "berserker.held_rage");
            pc.AddFury(100f);
            Assert.IsTrue(pc.Raging);
            Assert.IsFalse(pc.Run.tempModifiers.Any(m => m.stat == StatType.AttackSpeed && m.value > 0), "szał bez premii tempa");
            var p = pc.Uniques.Get<HeldRagePower>();
            float rage = pc.RageTime;
            p.ChargeTick(1.0f, 0.016f);
            Assert.AreEqual(2, p.Cuts, "1,0 s ładowania / 0,45 s = 2 cięcia");
            Assert.AreEqual(rage - 2 * UniqueCatalog.Get(p.Card.id).P("rage_per_cut"), pc.RageTime, 0.01f, "sekundy szału zużyte – Spalenie ich nie wyda");
            p.ForcedInterrupt();
            Assert.AreEqual(0, p.Cuts, "przerwanie przepala cięcia");
        }

        [Test]
        public void StatusTransfer_BleedTakeAddConservesBudget_DeferredBurnDoesNotTick_MagicSunder()
        {
            var b = fx.Cfg.balance;
            var s = new StatusEffects { SunderAsResistance = true };
            s.Apply(StatusKind.Bleed, 100f, 0f, b);
            float before = s.BleedRemainingDamage;
            float taken = s.TakeBleed(0.6f, out float rem);
            Assert.AreEqual(before * 0.6f, taken, 0.01f);
            Assert.AreEqual(before * 0.4f, s.BleedRemainingDamage, 0.01f, "rana naprawdę znika w tej części");
            var t = new StatusEffects();
            t.AddBleed(taken, rem, b);
            Assert.AreEqual(taken, t.BleedRemainingDamage, 0.01f, "przeniesiona bez klonu i bez wzrostu");
            // Odroczone podpalenie: nie tyka, budżet czeka; po terminie znika.
            var mods = StatusModifiers.None; mods.deferBurn = true; mods.deferCapSeconds = 6f;
            var f = new StatusEffects();
            f.Apply(StatusKind.Burn, 0f, 50f, b, mods);
            Assert.IsFalse(f.Burning);
            Assert.Greater(f.DeferredBurn, 0f);
            float dmg = 0f; for (int i = 0; i < 60; i++) dmg += f.Tick(0.05f, false, b);
            Assert.AreEqual(0f, dmg, 1e-4f, "brak tyknięć");
            float d1 = f.DeferredBurn;
            f.Apply(StatusKind.Burn, 0f, 50f, b, mods);
            Assert.LessOrEqual(f.DeferredBurn, d1 * 2.01f, "odświeżenie nie rośnie bez limitu");
            // Magiczne pęknięcie: odporność fizyczna bez zmian, żywioły niżej – nigdy obie wersje.
            var sm = StatusModifiers.None; sm.sunderMagicPerStack = 0.05f;
            var e = new StatusEffects { SunderAsResistance = true };
            e.Apply(StatusKind.Sunder, 0f, 0f, b, sm); e.Apply(StatusKind.Sunder, 0f, 0f, b, sm);
            Assert.AreEqual(0f, e.SunderResistReduction(b), 1e-4f);
            Assert.AreEqual(0.1f, e.SunderElementCut, 1e-4f);
        }

        [Test]
        public void TakeBurns_MovesFamiliesOnce_EmptyAfter()
        {
            var b = fx.Cfg.balance;
            var s = new StatusEffects();
            s.Apply(StatusKind.Burn, 0f, 40f, b);
            var burns = s.TakeBurns();
            Assert.AreEqual(1, burns.Count);
            Assert.IsFalse(s.Burning, "rodzina opuściła martwą ofiarę");
            Assert.AreEqual(0, s.TakeBurns().Count, "druga próba nie klonuje");
            var t = new StatusEffects();
            t.AddBurnFamily(burns[0].key, burns[0].dps, burns[0].remaining - 0.5f, 1);
            Assert.IsTrue(t.Burning);
            Assert.AreEqual(burns[0].dps, t.BurnDpsTotal, 1e-3f);
        }

        [Test]
        public void Texts_FillParametersFromCard_SameNumbersAsRuntime()
        {
            var c = UniqueCatalog.Get("item.unique.turris.mage.sleeping_circles");
            StringAssert.Contains("0,8 s", c.Rule);
            StringAssert.Contains("1,7 m", c.Rule);
            var it = Make("mage.sleeping_circles");
            var text = UniqueText.Lines(it);
            StringAssert.Contains("UNIKAT M03", text);
            StringAssert.Contains("Cena:", text);
            StringAssert.Contains("Warunek:", text);
        }
    }
}
