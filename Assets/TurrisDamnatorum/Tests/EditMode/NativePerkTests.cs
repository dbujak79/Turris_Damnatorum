using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>
    /// Perki specjalizacyjne (docs/PROMPT_WDROZENIE_PERKOW_SPECJALIZACYJNYCH.md, rozdz. 11 – EditMode): katalog i rejestracja, sklep
    /// i ceny, kwalifikacja Pojedynkowicza, matematyka tempa, sfora, harmonogram stref i zapis.
    /// </summary>
    public class NativePerkTests
    {
        Fixture fx;
        [SetUp] public void SetUp() { fx = new Fixture(); NativePerkCatalog.ResetForTests(); }
        [TearDown] public void TearDown() { Servant.DismissAll(); foreach (var z in DamageZone.Active.ToArray()) if (z != null) Object.DestroyImmediate(z.gameObject); fx.Cleanup(); NativePerkCatalog.ResetForTests(); }

        ItemDefinition I(string id) => fx.Get<ItemDefinition>(id);
        BoonDefinition Perk(string id) => NativePerkCatalog.Get(fx.Cfg, id);

        RunState Run(string cls, string main, string off, params string[] skills)
        {
            var run = fx.NewRun(cls);
            run.equipment.Unequip(EquipSlot.MainHand); run.equipment.Unequip(EquipSlot.OffHand);
            if (main != null) { var it = new ItemInstance(I(main)); run.inventory.Add(it); Assert.IsTrue(run.EquipFromInventory(it, EquipSlot.MainHand), main); }
            if (off != null) { var it = new ItemInstance(I(off)); run.inventory.Add(it); Assert.IsTrue(run.EquipFromInventory(it, EquipSlot.OffHand), off); }
            for (int i = 0; i < RunState.SkillSlotCount; i++) run.ClearSlot(i);
            for (int i = 0; i < skills.Length; i++) { var d = fx.Get<SpellDefinition>(skills[i]); run.AssignSlot(run.FindSpell(d) ?? run.LearnSpell(d, true), i); }
            return run;
        }

        // E01
        [Test]
        public void Catalog_FourStableIds_OneLevel_Defaults_IdempotentPerConfig()
        {
            var a = NativePerkCatalog.Boons(fx.Cfg);
            Assert.AreEqual(4, a.Count);
            CollectionAssert.AreEquivalent(new[] { "perk.turris.duelist", "perk.turris.exploit_breach", "perk.turris.diverse_pack", "perk.turris.violent_manifestation" }, a.Select(b => b.id));
            Assert.IsTrue(a.All(b => b.maxStacks == 1));
            Assert.AreSame(a[0], NativePerkCatalog.Boons(fx.Cfg)[0], "idempotentnie");
            var other = new Fixture();
            try { Assert.AreNotSame(a[0], NativePerkCatalog.Boons(other.Cfg)[0], "osobno dla innej konfiguracji"); } finally { other.Cleanup(); }
            var t = NativePerkCatalog.Tuning(fx.Cfg);
            Assert.AreEqual(0.15f, t.duelistSpeed, 1e-5f); Assert.AreEqual(1.5f, t.breachMultiplier, 1e-5f); Assert.AreEqual(50f, t.breachMinPoise, 1e-5f); Assert.AreEqual(3f, t.breachSeconds, 1e-5f);
            Assert.AreEqual(0.15f, t.packPerKind, 1e-5f); Assert.AreEqual(0.5f, t.manifestationTime, 1e-5f);
            var bad = new NativePerkTuning { breachSeconds = float.NaN, manifestationTime = -1f, duelistSpeed = float.PositiveInfinity }.Valid();
            Assert.AreEqual(3f, bad.breachSeconds, 1e-5f); Assert.Greater(bad.manifestationTime, 0f); Assert.AreEqual(0.15f, bad.duelistSpeed, 1e-5f);
            Assert.IsFalse(fx.Cfg.baseBoonPool.Any(NativePerkCatalog.IsNative), "nie w starych pulach, talentach ani odblokowaniach");
        }

        // E02 + E03
        [Test]
        public void Shop_NativeAndHuCoexist_FourDistinct_Deterministic_Prices_Floors_FailedBuyFree()
        {
            var run = Run("class_knight", "hu_sword", null);
            run.floorsCleared = 5; run.souls = 100000;
            var a = FloorShop.Generate(run, fx.Cfg, new System.Random(3)).Where(o => o.kind == FloorOffer.Kind.Perk).ToList();
            var b = FloorShop.Generate(run, fx.Cfg, new System.Random(3)).Where(o => o.kind == FloorOffer.Kind.Perk).ToList();
            Assert.AreEqual(4, a.Count);
            Assert.AreEqual(4, a.Select(o => o.perk.id).Distinct().Count(), "bez duplikatów");
            CollectionAssert.AreEqual(a.Select(o => o.perk.id), b.Select(o => o.perk.id), "stały seed – ten sam wynik");
            int nat = 0, hu = 0;
            for (int s = 0; s < 300; s++)
                foreach (var o in FloorShop.Generate(run, fx.Cfg, new System.Random(s)).Where(o => o.kind == FloorOffer.Kind.Perk))
                { if (NativePerkCatalog.IsNative(o.perk)) nat++; else hu++; Assert.IsTrue(NativePerkCatalog.IsNative(o.perk) || o.perk.id.StartsWith("hu_perk_"), "bez zwykłych talentów"); }
            Assert.Greater(nat, 0, "natywne perki pojawiają się"); Assert.Greater(hu, nat, "nie wypierają puli HU");
            // Ceny przy inflacji 1 (pierwszy zakup): 70/75/80/75.
            var fresh = Run("class_knight", "hu_sword", null);
            var t = NativePerkCatalog.Tuning(fx.Cfg);
            Assert.AreEqual(7, t.packFloor, "Sfora dopiero z drugim rodzajem sług");
            float infl = SoulShop.Inflation(fresh, fx.Cfg.balance);
            Assert.AreEqual(Mathf.RoundToInt(70f * infl), NativePerkCatalog.Price(NativePerkCatalog.Duelist, t, fresh, fx.Cfg.balance));
            if (Mathf.Approximately(infl, 1f))
            {
                Assert.AreEqual(75, NativePerkCatalog.Price(NativePerkCatalog.Breach, t, fresh, fx.Cfg.balance));
                Assert.AreEqual(80, NativePerkCatalog.Price(NativePerkCatalog.Pack, t, fresh, fx.Cfg.balance));
                Assert.AreEqual(75, NativePerkCatalog.Price(NativePerkCatalog.Manifestation, t, fresh, fx.Cfg.balance));
            }
            // Wyłom od piętra 3 (numer oferty = floorsCleared + 1).
            run.floorsCleared = 1;
            for (int s = 0; s < 200; s++) Assert.IsFalse(FloorShop.Generate(run, fx.Cfg, new System.Random(s)).Any(o => o.perk != null && o.perk.id == NativePerkCatalog.Breach), "Wyłom przed piętrem 3");
            run.floorsCleared = 2;
            Assert.IsTrue(Enumerable.Range(0, 300).Any(s => FloorShop.Generate(run, fx.Cfg, new System.Random(s)).Any(o => o.perk != null && o.perk.id == NativePerkCatalog.Breach)), "Wyłom na piętrze 3");
            // Zakup 1/1; ponowny – odrzucony bez pobrania dusz; zmiana buildu przed zakupem – odmowa.
            var offer = new FloorOffer { kind = FloorOffer.Kind.Perk, perk = Perk(NativePerkCatalog.Duelist), price = 70 };
            int souls = run.souls;
            Assert.IsTrue(FloorShop.Buy(offer, run));
            Assert.AreEqual(souls - 70, run.souls);
            var again = new FloorOffer { kind = FloorOffer.Kind.Perk, perk = Perk(NativePerkCatalog.Duelist), price = 70 };
            Assert.IsFalse(FloorShop.Buy(again, run)); Assert.AreEqual(souls - 70, run.souls, "nieudany zakup nie pobiera dusz");
            var shield = new ItemInstance(I("hu_shield")); run.inventory.Add(shield); run.EquipFromInventory(shield, EquipSlot.OffHand);
            var pack = new FloorOffer { kind = FloorOffer.Kind.Perk, perk = Perk(NativePerkCatalog.Pack), price = 80 };
            Assert.IsFalse(FloorShop.Buy(pack, run), "brak przywołań – kwalifikacja przy zakupie"); Assert.AreEqual(souls - 70, run.souls);
        }

        // E04
        [Test]
        public void Duelist_Matrix_OneHandedMeleeEmptyOffhandOnly()
        {
            foreach (var id in new[] { "hu_sword", "hu_dagger", "hu_sabre", "hu_morningstar", "hu_flail" })
                Assert.IsTrue(NativePerkCatalog.DuelistHands(I(id), null), id);
            foreach (var id in new[] { "hu_greataxe", "hu_warhammer", "hu_bow", "hu_wand_fire", "weapon_fists", "weapon_knives" })
                Assert.IsFalse(NativePerkCatalog.DuelistHands(I(id), null), id);
            foreach (var off in new[] { "hu_shield", "hu_grimoire_fire", "hu_dagger", "hu_offdagger" })
                Assert.IsFalse(NativePerkCatalog.DuelistHands(I("hu_sword"), I(off)), "zajęta druga ręka: " + off);
        }

        // E06 (ostrzeżenie w analizie): tarcza albo broń dwuręczna usypia Pojedynkowicza, inna broń jednoręczna – nie.
        [Test]
        public void Duelist_LoadoutWarning_ShieldOrTwoHanded()
        {
            var run = Run("class_knight", "hu_sword", null);
            var duel = Perk(NativePerkCatalog.Duelist);
            run.AddBoon(duel);
            CollectionAssert.Contains(LoadoutAnalysis.NativePerksLosingEffect(run, I("hu_shield")), duel, "tarcza do drugiej ręki");
            CollectionAssert.Contains(LoadoutAnalysis.PerksLosingEffect(run, I("hu_greataxe")), duel, "broń dwuręczna");
            CollectionAssert.DoesNotContain(LoadoutAnalysis.PerksLosingEffect(run, I("hu_sabre")), duel, "inna jednoręczna");        }

        // E05
        [Test]
        public void Duelist_AddsToSpeedSum_NotMultiplier_OnlyQualifiedAttack_NoSkillScaleChange()
        {
            var run = Run("class_knight", "hu_sword", null, "hu_cleave");
            run.AddBoon(Perk(NativePerkCatalog.Duelist));
            var pc = fx.NewCombatant(run);
            var atk = pc.Build.weapon.light;
            Assert.AreEqual(0.15f, pc.DuelistBonus(ActionType.LightAttack, atk, false), 1e-5f);
            Assert.AreEqual(1.45f, 1.20f + 0.10f + pc.DuelistBonus(ActionType.HeavyAttack, pc.Build.weapon.heavy, false), 1e-5f, "1,20 + 0,10 + 0,15");
            Assert.AreEqual(0f, pc.DuelistBonus(ActionType.Cast, atk, false), "techniki/czary bez premii");
            Assert.AreEqual(0f, pc.DuelistBonus(ActionType.LightAttack, atk, true), "druga ręka bez premii");
            Assert.IsFalse(pc.Build.stats[StatType.AttackSpeed] > BuildCalculator.Compute(Run("class_knight", "hu_sword", null), fx.Cfg).stats[StatType.AttackSpeed], "bez wpisu do statystyki szybkości");
            float scale = pc.SkillDamage(pc.Skill(0));
            var plainPc = fx.NewCombatant(Run("class_knight", "hu_sword", null, "hu_cleave"));
            Assert.AreEqual(plainPc.SkillDamage(plainPc.Skill(0)), scale, 1e-3f, "obrażenia technik bez zmian");
            // Czas zwykłego ataku krótszy dokładnie o sumę tempa.
            pc.Request(ActionType.LightAttack);
            plainPc.Request(ActionType.LightAttack);
            float ratio = plainPc.AttackWindup / pc.AttackWindup;
            float baseSpeed = Mathf.Max(0.3f, plainPc.Build.attackSpeed);
            Assert.AreEqual((baseSpeed + 0.15f) / baseSpeed, ratio, 0.01f);
            // Tarcza wyłącza.
            var shield = new ItemInstance(I("hu_shield")); run.inventory.Add(shield); run.EquipFromInventory(shield, EquipSlot.OffHand); pc.RefreshBuild();
            Assert.AreEqual(0f, pc.DuelistBonus(ActionType.LightAttack, pc.Build.weapon.light, false), "tarcza wyłącza perk");
            Assert.IsNotNull(NativePerkCatalog.Dormant(NativePerkCatalog.Duelist, run), "uśpiony – opis przyczyny");
            Assert.IsTrue(run.BoonStacks(Perk(NativePerkCatalog.Duelist)) == 1, "uśpiony zostaje kupiony");
        }

        // E09 + E10
        [Test]
        public void Pack_KindsCount_ChimeraParts_OwnerOnly_LayerAtAttackTime()
        {
            var run = Run("class_hu_summoner", null, "hu_grimoire_summoning", "hu_summon_imps");
            run.AddBoon(Perk(NativePerkCatalog.Pack));
            var other = fx.NewCombatant(Run("class_hu_summoner", null, null)); // drugi właściciel (start postaci czyści sługi – przed przyzwaniami)
            var pc = fx.NewCombatant(run);
            Assert.IsTrue(pc.PerkActive(NativePerkCatalog.Pack), "skill przywołań z grymuaru");
            Assert.AreEqual(1f, pc.PackMultiplier, 1e-5f, "K=0");
            var hit = pc.FeatureHitForTest(10f);
            Servant.Spawn(pc, ServantKind.Imp, 20f, 1f, () => hit, Color.green);
            Servant.Spawn(pc, ServantKind.Imp, 20f, 1f, () => hit, Color.green);
            Assert.AreEqual(1f, pc.PackMultiplier, 1e-5f, "K=1 (duplikaty nie liczą się)");
            var hound = Servant.Spawn(pc, ServantKind.Hound, 20f, 1f, () => hit, Color.green);
            Assert.AreEqual(1.15f, pc.PackMultiplier, 1e-5f, "K=2");
            var golem = Servant.Spawn(pc, ServantKind.Golem, 20f, 1f, () => hit, Color.green);
            Assert.AreEqual(1.30f, pc.PackMultiplier, 1e-5f, "K=3");
            Assert.AreEqual(169f, 100f * 1.30f * pc.PackMultiplier, 0.01f, "przykład 169");
            golem.Dismiss();
            Assert.AreEqual(149.5f, 100f * 1.30f * pc.PackMultiplier, 0.01f, "przykład 149,5 po zniknięciu rodzaju");
            // Obcy właściciel się nie liczy.
            Servant.Spawn(other, ServantKind.Golem, 20f, 1f, () => hit, Color.green);
            Assert.AreEqual(2, pc.PackKinds);
            // Chimera: rodzaje działających wkładów.
            Servant.DismissAll();
            var parts = new System.Collections.Generic.List<Servant.Part>
            {
                new Servant.Part { kind = ServantKind.Golem, remaining = 5f, interval = 1f, makeHit = () => hit },
                new Servant.Part { kind = ServantKind.Imp, remaining = 5f, interval = 1f, makeHit = () => hit },
                new Servant.Part { kind = ServantKind.Imp, remaining = 5f, interval = 1f, makeHit = () => hit },
            };
            Servant.SpawnChimera(pc, parts, Vector3.zero, Color.green);
            Assert.AreEqual(2, pc.PackKinds, $"chimera: golem + chochlik (duplikat wkładu bez wpływu); aktywni={Servant.Active.Count} chimera={Servant.Active.Count(s => s != null && s.IsChimera)} pan={Servant.Active.FirstOrDefault()?.Master == pc}");
            // Migawka przyzwania nie zawiera premii (makeHit zwraca budżet bez warstwy sfory).
            Assert.AreEqual(hit.Total, parts[0].makeHit().Total, 1e-4f);
        }

        // E11 + E12
        [Test]
        public void Manifestation_ZoneQualification_And_ScheduleCompression_KeepsEveryTick()
        {
            var t = NativePerkCatalog.Tuning(fx.Cfg);
            Assert.IsTrue(NativePerkCatalog.HasQualifiedZone(new SpellInstance(fx.Get<SpellDefinition>("spell_burningground"))), "Płonąca ziemia");
            Assert.IsTrue(NativePerkCatalog.HasQualifiedZone(new SpellInstance(fx.Get<SpellDefinition>("hu_ember_ring"))), "Krąg żaru");
            foreach (var id in new[] { "hu_arrow_rain", "hu_snare", "hu_thorn_scatter", "hu_slag_spray", "hu_aura_holy_fire", "hu_summon_imps" })
                Assert.IsFalse(NativePerkCatalog.HasQualifiedZone(new SpellInstance(fx.Get<SpellDefinition>(id))), id + " poza zakresem");
            var nova = new SpellInstance(fx.Get<SpellDefinition>("hu_nova_fire"), 0);
            bool novaZoneL1 = NativePerkCatalog.HasQualifiedZone(nova);
            nova.level = 2;
            TestContext.WriteLine($"nova ognia: strefa na I={novaZoneL1}, na III={NativePerkCatalog.HasQualifiedZone(nova)}");
            Assert.IsTrue(NativePerkCatalog.HasQualifiedZone(nova), "nova ognia III ma strefę");

            int Count(float duration, float interval, float scale, float step)
            {
                var z = DamageZone.Spawn(Vector3.zero, 2f, duration, interval, new HitData { physical = 10f }, Faction.Player, null, Color.red, scale);
                int before = DamageZone.TotalTicks;
                for (float s = 0f; s < duration + 1f; s += step) { if (z == null) break; z.Step(step); }
                if (z != null) Object.DestroyImmediate(z.gameObject);
                return DamageZone.TotalTicks - before;
            }
            Assert.AreEqual(8, Count(4f, 0.5f, 1f, 0.01f));
            Assert.AreEqual(8, Count(4f, 0.5f, t.manifestationTime, 0.01f), "4/0,5 → 2/0,25: te same 8 trafień");
            Assert.AreEqual(8, Count(2.5f, 0.35f, 1f, 0.01f));
            Assert.AreEqual(8, Count(2.5f, 0.35f, t.manifestationTime, 0.01f), "2,5/0,35 → 1,25/0,175: 8 trafień");
            Assert.AreEqual(8, Count(2.5f, 0.35f, t.manifestationTime, 0.3f), "duży krok czasu – ta sama liczba");
            Assert.AreEqual(8, Count(0.8f, 0.1f, t.manifestationTime, 0.016f), "interwał 0,05 s nie obcięty minimum 0,1 s");
            var zz = DamageZone.Spawn(Vector3.zero, 2f, 4f, 0.5f, new HitData { physical = 10f }, Faction.Player, null, Color.red, 0.5f);
            Assert.AreEqual(2f, zz.Remaining, 1e-4f, "czas świata"); Assert.AreEqual(0.25f, zz.Interval, 1e-4f);
            Assert.AreEqual(80f, zz.RemainingBudget, 1e-3f, "budżet bez zmian");
            zz.Step(1f);
            Assert.AreEqual(1f, zz.Remaining, 1e-3f);
            Object.DestroyImmediate(zz.gameObject);
        }

        // E14
        [Test]
        public void Save_FourPerks_RoundTripBeforeShop_OldSnapshotUnchanged()
        {
            var run = Run("class_knight", "hu_sword", "hu_shield");
            foreach (var id in NativePerkCatalog.Ids) run.AddBoon(Perk(id));
            run.manaDebt = 0f;
            var json = JsonUtility.ToJson(RunSnapshot.Capture(run));
            NativePerkCatalog.ResetForTests(); // jak nowa sesja: rejestr pusty, sklep nieotwierany
            var back = JsonUtility.FromJson<RunSnapshot>(json).Restore(fx.Cfg);
            foreach (var id in NativePerkCatalog.Ids) Assert.AreEqual(1, back.boons.Count(b => b.definition.id == id && b.stacks == 1), id);
            Assert.IsNotNull(back.equipment.Get(EquipSlot.OffHand), "wyposażenie nienaruszone (Pojedynkowicz uśpiony, bez zdejmowania)");
            Assert.IsNotNull(NativePerkCatalog.Dormant(NativePerkCatalog.Duelist, back));
            var old = RunSnapshot.Capture(Run("class_knight", "hu_sword", null));
            old.boons.Clear();
            var o = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(old)).Restore(fx.Cfg);
            Assert.AreEqual(0, o.boons.Count, "stary zapis bez nowych perków");
        }
    }
}
