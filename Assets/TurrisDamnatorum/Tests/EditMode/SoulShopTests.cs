using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Sklep dusz między piętrami (plan rozwoju, etap D).</summary>
    public class SoulShopTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        BalanceConfig B => fx.Cfg.balance;

        [Test]
        public void Prices_GrowWithFloors_AndSkillLevel()
        {
            var run = fx.NewRun("class_knight");
            var skill = run.skillSlots[0];
            run.floorsCleared = 1;
            int first = SoulShop.UpgradeCost(skill, run, B);
            Assert.AreEqual(B.skillUpgradeSoulCost, first);
            run.floorsCleared = 3;
            Assert.AreEqual(UnityEngine.Mathf.RoundToInt(B.skillUpgradeSoulCost * (1 + 2 * B.shopInflationPerFloor)), SoulShop.UpgradeCost(skill, run, B), "Inflacja po kolejnych piętrach");
            skill.level = 2;
            Assert.Greater(SoulShop.UpgradeCost(skill, run, B), first * 3 - 1, "Wyższy poziom kosztuje więcej");
        }

        [Test]
        public void Upgrade_IsAtomic()
        {
            var run = fx.NewRun("class_knight");
            run.floorsCleared = 1;
            var skill = run.skillSlots[0];
            run.souls = SoulShop.UpgradeCost(skill, run, B) - 1;
            Assert.IsFalse(SoulShop.TryUpgrade(skill, run, B));
            Assert.AreEqual(0, skill.level, "Bez dusz – nic się nie zmienia");
            run.souls += 1;
            Assert.IsTrue(SoulShop.TryUpgrade(skill, run, B));
            Assert.AreEqual(1, skill.level);
            Assert.AreEqual(0, run.souls);

            skill.level = skill.definition.maxLevel;
            run.souls = 9999;
            Assert.IsFalse(SoulShop.TryUpgrade(skill, run, B), "Maksymalny poziom");
        }

        [Test]
        public void Offer_NeverSellsSpellThatCannotBeCast()
        {
            var run = fx.NewRun("class_knight"); // Inteligencja 8
            var pools = new MetaService(fx.Cfg, new MemoryProfileStorage()).BuildRewardPools();
            var snap = BuildCalculator.Compute(run, fx.Cfg);
            for (int seed = 0; seed < 80; seed++)
            {
                var o = SoulShop.PickOffer(run, snap, pools, new System.Random(seed), B);
                if (o != null && o.IsSpell) Assert.IsTrue(StatCalculator.RequirementsMet(o.requirements, snap.stats), o.id);
            }
        }

        [Test]
        public void Offer_IsUnknownAndUsable_PurchaseIsTemporary()
        {
            var run = fx.NewRun("class_mage");
            var pools = new MetaService(fx.Cfg, new MemoryProfileStorage()).BuildRewardPools();
            var snap = BuildCalculator.Compute(run, fx.Cfg);
            for (int seed = 0; seed < 50; seed++)
            {
                var o = SoulShop.PickOffer(run, snap, pools, new System.Random(seed));
                Assert.IsNotNull(o);
                Assert.IsNull(run.FindSpell(o), "Oferta to nowa umiejętność");
                Assert.AreNotEqual("skill_shieldbash", o.id, "Mag bez tarczy nie dostaje uderzenia tarczą");
            }
            var offer = SoulShop.PickOffer(run, snap, pools, new System.Random(1));
            run.floorsCleared = 1;
            run.souls = SoulShop.OfferCost(run, B);
            Assert.IsTrue(SoulShop.TryBuyOffer(offer, run, B));
            Assert.IsFalse(run.FindSpell(offer).permanent, "Kupiona w sklepie – tylko na to podejście");
            Assert.IsFalse(SoulShop.TryBuyOffer(offer, run, B), "Nie da się kupić drugi raz");
        }
    }
}
