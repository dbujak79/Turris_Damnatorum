using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Skill zdobyty później przychodzi na wyższym poziomie – progi jak w Hell Unleashed (fala 7 → II, fala 16 → III).</summary>
    public class SkillTiersTests
    {
        Fixture fx;

        [SetUp]
        public void SetUp() => fx = new Fixture();

        [Test]
        public void StartLevel_FollowsHuWaveThresholds()
        {
            var hu = fx.Get<SpellDefinition>("hu_cross_cut"); // trzy poziomy (0–2)
            Assert.AreEqual(0, SkillTiers.StartLevel(hu, 0), "piętro 1");
            Assert.AreEqual(0, SkillTiers.StartLevel(hu, 5), "oferta przed piętrem 6");
            Assert.AreEqual(1, SkillTiers.StartLevel(hu, 6), "oferta przed piętrem 7 – poziom II");
            Assert.AreEqual(1, SkillTiers.StartLevel(hu, 14));
            Assert.AreEqual(2, SkillTiers.StartLevel(hu, 15), "oferta przed piętrem 16 – poziom III");
            var turris = fx.Get<SpellDefinition>("skill_rend"); // pięć poziomów (0–4)
            Assert.AreEqual(2, SkillTiers.StartLevel(turris, 6));
            Assert.AreEqual(turris.maxLevel, SkillTiers.StartLevel(turris, 20));
        }

        [Test]
        public void Shop_TeachesSkillAtTierLevel()
        {
            var run = fx.NewRun("legacy_knight");
            run.floorsCleared = 15;
            run.souls = 100000;
            var cross = fx.Get<SpellDefinition>("hu_cross_cut");
            Assert.IsTrue(SoulShop.TryBuyOffer(cross, run, fx.Cfg.balance));
            Assert.AreEqual(2, run.FindSpell(cross).level, "piętro 15: poziom II");

            var offer = fx.Get<SpellDefinition>("skill_rend");
            Assert.IsTrue(SoulShop.TryBuyOffer(offer, run, fx.Cfg.balance));
            Assert.AreEqual(offer.maxLevel, run.FindSpell(offer).level, "oferta sklepu też na poziomie progu");
        }
    }
}
