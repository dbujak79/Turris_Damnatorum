using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>Plan migracji HU, rozdz. 7.1/7.2: statystyki bez odpowiednika, moce unikatów, sprzedaż i plecak, tarcza, żywioły, reakcje.</summary>
    public class HuPortTests
    {
        static readonly BalanceConfig B = new BalanceConfig();
        Fixture fx;

        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        static float Fx(ItemDefinition d, PassiveEffectType t) => d.effects.Where(e => e.type == t).Sum(e => e.value);

        [Test]
        public void ItemStatsWithoutTurrisStat_BecomeEffects()
        {
            Assert.AreEqual(3.5f, Fx(fx.Get<ItemDefinition>("hu_item_body_light_t1"), PassiveEffectType.Evasion), 1e-3f, "lekka zbroja daje uchylenie");
            Assert.Greater(Fx(fx.Get<ItemDefinition>("hu_item_ring_gold"), PassiveEffectType.SoulGain), 0f, "złoty pierścień – dusze");
            var hood = fx.Get<ItemDefinition>("hu_item_unique_nameless_hood");
            Assert.AreEqual(8f, Fx(hood, PassiveEffectType.Evasion), 1e-3f);
            Assert.AreEqual(5f, Fx(hood, PassiveEffectType.Accuracy), 1e-3f);
            Assert.AreEqual(0.02f, Fx(hood, PassiveEffectType.SoulHarvest), 1e-4f, "moc unikatu");
            Assert.Greater(Fx(fx.Get<ItemDefinition>("hu_item_unique_heart_of_hell"), PassiveEffectType.CheatDeath), 0f);
            Assert.Greater(Fx(fx.Get<ItemDefinition>("hu_item_unique_zawisza_helm"), PassiveEffectType.Thorns), 0f);
            int powered = fx.Cfg.baseItemPool.Count(d => d != null && d.id.StartsWith("hu_item_unique_") && d.effects.Any(e => IsPower(e.type)));
            Assert.AreEqual(17, powered, "17 unikatów zbroi i biżuterii z mocą");
        }

        static bool IsPower(PassiveEffectType t) => t == PassiveEffectType.CheatDeath || t == PassiveEffectType.CorpseExplosion || t == PassiveEffectType.EmberAura
            || t == PassiveEffectType.Thorns || t == PassiveEffectType.WeaponChainLightning || t == PassiveEffectType.FrostWard
            || t == PassiveEffectType.LowHealthDamage || t == PassiveEffectType.SoulHarvest;

        [Test]
        public void AllAffixes_AreSupported_AndEffectAffixesRoll()
        {
            Assert.IsTrue(HellEconomy.Affixes.All(a => a.Ranges.All(r => HellEconomy.Supported(r.Split('|')[0]))), "każdy affix HU ma odpowiednik");
            var rng = new System.Random(7);
            var body = fx.Get<ItemDefinition>("hu_item_body_light_t2");
            bool sawEffect = Enumerable.Range(0, 200).Any(_ => HellEconomy.Roll(body, LootSource.Shop, 20, rng, ItemRarity.Legendary).affixEffects.Count > 0);
            Assert.IsTrue(sawEffect, "affixy uchylenia/dusz/życia za zabójstwo wypadają");
        }

        [Test]
        public void Quality_ScalesPowers_ButNotCounts()
        {
            var it = new ItemInstance(fx.Get<ItemDefinition>("hu_item_unique_heart_of_hell")) { huQuality = 2 };
            float raw = Fx(it.definition, PassiveEffectType.CheatDeath);
            Assert.AreEqual(raw * 1.5f, it.Effects.Where(e => e.type == PassiveEffectType.CheatDeath).Sum(e => e.value), 1e-4f);
            Assert.IsFalse(PassiveEffects.ScalesWithQuality(PassiveEffectType.ProjectilePierce));
        }

        [Test]
        public void Sell_GivesThirtyFivePercent_AndBackpackHoldsEight()
        {
            var run = fx.NewRun("class_knight");
            run.inventory.Clear();
            var ring = new ItemInstance(fx.Get<ItemDefinition>("hu_item_ring_iron"));
            run.inventory.Add(ring);
            int souls = run.souls, price = RunState.SellPrice(ring);
            Assert.AreEqual(UnityEngine.Mathf.RoundToInt(HellEconomy.Value(ring) * 0.35f), price);
            Assert.IsTrue(run.SellItem(ring));
            Assert.AreEqual(souls + price, run.souls);
            Assert.IsFalse(run.inventory.Contains(ring));

            for (int i = 0; i < RunState.BackpackSize; i++) run.inventory.Add(new ItemInstance(fx.Get<ItemDefinition>("hu_item_ring_iron")));
            Assert.IsTrue(run.BackpackFull);
            var head = run.equipment.Get(EquipSlot.Head);
            if (head == null) run.equipment.Equip(new ItemInstance(fx.Get<ItemDefinition>("hu_item_head_light_t1")), EquipSlot.Head, null);
            Assert.IsFalse(run.CanReceive(new ItemInstance(fx.Get<ItemDefinition>("hu_item_head_light_t2"))), "slot zajęty i plecak pełny");
            Assert.IsFalse(run.UnequipToInventory(EquipSlot.Head), "zdjęcie zablokowane");
            Assert.IsFalse(run.CanSell(run.equipment.Get(EquipSlot.MainHand)), "broni w ręku się nie sprzedaje");
        }

        [Test]
        public void Shop_OffersShieldLevel_UpToThird()
        {
            var shield = fx.Cfg.baseItemPool.First(d => d != null && d.IsShield);
            Assert.AreEqual(2, HellEconomy.MaxLevelOf(shield));
            var run = fx.NewRun("class_knight");
            run.equipment.Equip(new ItemInstance(shield), EquipSlot.OffHand, null);
            run.floorsCleared = 3;
            var offers = FloorShop.Generate(run, fx.Cfg, new System.Random(1));
            Assert.IsTrue(offers.Any(o => o.kind == FloorOffer.Kind.WeaponLevel && o.item.definition.IsShield), "oferta poziomu tarczy");
        }

        [Test]
        public void MixedElements_EachMeetsItsOwnResistance()
        {
            var hit = Fixture.FrontHit(0, 100);
            hit.element = Element.Fire; hit.element2 = Element.Earth; hit.element2Share = 0.4f;
            Assert.AreEqual(0.6f, hit.ShareOf(Element.Fire), 1e-4f);
            Assert.IsTrue(hit.Has(Element.Earth));
            hit.ScaleMagicByElement(e => e == Element.Fire ? 0.5f : 1f); // odporny na ogień, nie na ziemię
            Assert.AreEqual(100f * (0.6f * 0.5f + 0.4f), hit.magic, 1e-3f);
            Assert.AreEqual(0.4f / 0.7f, hit.element2Share, 1e-3f, "udział ziemi rośnie po odporności na ogień");
        }

        [Test]
        public void Steam_OnEnemies_PutsFireOut()
        {
            var s = new StatusEffects { AreaReactions = true };
            s.Apply(StatusKind.Burn, 0f, 100f, B);
            var water = Fixture.FrontHit(0, 20);
            water.element = Element.Frost;
            s.OnLanded(water, new HitResult { outcome = HitOutcome.Hit, healthDamage = 20 }, B, out var reaction, out _);
            Assert.AreEqual("PARA", reaction);
            Assert.IsTrue(s.LastSteam);
            Assert.IsFalse(s.Burning, "ogień zgasł");

            var hero = new StatusEffects();
            hero.Apply(StatusKind.Burn, 0f, 100f, B);
            hero.OnLanded(water, new HitResult { outcome = HitOutcome.Hit, healthDamage = 20 }, B, out var none, out _);
            Assert.IsNull(none, "para tylko od ciosów bohatera");
        }

        [Test]
        public void EmberBurst_OnEnemies_GoesToArea()
        {
            var s = new StatusEffects { AreaReactions = true };
            s.Apply(StatusKind.Burn, 0f, 100f, B);
            var earth = Fixture.FrontHit(0, 40);
            earth.element = Element.Earth;
            float instant = s.OnLanded(earth, new HitResult { outcome = HitOutcome.Hit, healthDamage = 40 }, B, out var reaction, out _);
            Assert.AreEqual("WYBUCH ŻARU", reaction);
            Assert.AreEqual(0f, instant, "nie tylko w trafionego");
            Assert.AreEqual(40f * B.emberBurstShare, s.LastEmberBurstDamage, 1e-3f);
        }

        [Test]
        public void Sunder_OnEnemies_LowersPhysicalResistance()
        {
            var s = new StatusEffects { SunderAsResistance = true };
            for (int i = 0; i < 3; i++) s.Apply(StatusKind.Sunder, 0, 0, B);
            Assert.AreEqual(3 * B.sunderPerStack, s.SunderResistReduction(B), 1e-4f);
            var hit = Fixture.FrontHit(100);
            s.ModifyIncoming(ref hit, B);
            Assert.AreEqual(100f, hit.physical, 1e-3f, "bez mnożnika – działa przez odporność");
        }

        [Test]
        public void HuSkillCost_ComesFromLevelParameters()
        {
            var run = fx.NewRun("class_mage");
            var nova = fx.Get<SpellDefinition>("hu_nova_fire");
            var inst = run.LearnSpell(nova, false, 2);
            run.AssignSlot(inst, 0);
            var snap = BuildCalculator.Compute(run, fx.Cfg);
            var rt = snap.skills.First(r => r != null && r.instance == inst);
            var p = HuCatalog.ForTarget("hu_nova_fire").At(3);
            Assert.AreEqual((float)p.Get("mana", 8 + 2 * p.Get("cooldown")), rt.manaCost, 1e-3f, "HU: mana = 8 + 2 × odnowienie, bez zniżki za poziom");
        }

        [Test]
        public void Demons_HaveHuEvasion_AndHeroAccuracyComesFromDexterityAndItems()
        {
            var stalker = fx.Cfg.enemyCatalog.First(e => e != null && e.huSourceId == "enemy.stalker");
            Assert.AreEqual(0.18f, stalker.evasion, 1e-4f, "łowca HU unika w 18%");
            var run = fx.NewRun("class_knight");
            var bare = BuildCalculator.Compute(run, fx.Cfg);
            float dex = bare.stats[StatType.Dexterity];
            Assert.AreEqual(Mathf.Max(0f, dex - fx.Cfg.balance.evasionFromDexterity) * fx.Cfg.balance.accuracyPerDexterity / 100f, bare.accuracy, 1e-4f);
            run.equipment.Equip(new ItemInstance(fx.Get<ItemDefinition>("hu_item_unique_nameless_hood")), EquipSlot.Head, null);
            Assert.AreEqual(bare.accuracy + 0.05f, BuildCalculator.Compute(run, fx.Cfg).accuracy, 1e-4f, "Kaptur Bezimiennego: +5 pkt celności");
        }

        [Test]
        public void LoadoutAnalysis_ShowsDps_AndPerksLostWithNewWeapon()
        {
            var run = fx.NewRun("class_knight");
            var snap = BuildCalculator.Compute(run, fx.Cfg);
            var main = LoadoutAnalysis.MainAttack(snap);
            Assert.Greater(main.dps, 0f);
            Assert.AreEqual(main.perHit * main.perSecond, main.dps, 1e-3f);

            var heavy = fx.Cfg.baseBoonPool.First(b => b != null && b.id == "hu_perk_warrior_heavyblows");
            run.AddBoon(heavy);
            Assert.IsTrue(PerkMechanics.Active(heavy, run), "miecz – perk wręcz działa");
            var bow = fx.Cfg.baseItemPool.First(d => d != null && d.IsWeapon && d.huSourceId == "weapon.bow");
            CollectionAssert.Contains(LoadoutAnalysis.PerksLosingEffect(run, bow), heavy, "po założeniu łuku perk wręcz zaśnie");
            var sword = fx.Cfg.baseItemPool.First(d => d != null && d.IsWeapon && d.huSourceId == "weapon.sword.broad");
            CollectionAssert.DoesNotContain(LoadoutAnalysis.PerksLosingEffect(run, sword), heavy);
        }
    }
}
