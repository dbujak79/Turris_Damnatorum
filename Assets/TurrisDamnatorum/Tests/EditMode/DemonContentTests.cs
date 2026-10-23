using System.Linq;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Demony z pierwszych trzech aren Hell Unleashed w treści gry i na piętrach I–III.</summary>
    public class DemonContentTests
    {
        Fixture fx;

        [SetUp]
        public void SetUp() => fx = new Fixture();

        [Test]
        public void FloorsOneToThree_AreHellUnleashedArenas()
        {
            var f = fx.Cfg.tower.floors;
            Assert.IsTrue(f[0].portals.enabled && f[1].portals.enabled, "piętra I–II: portale");
            CollectionAssert.AreEquivalent(new[] { "enemy_imp" }, f[0].portals.composition.Select(c => c.enemy.id));
            CollectionAssert.AreEquivalent(new[] { "enemy_imp", "enemy_hu_cultist", "enemy_worm" }, f[1].portals.composition.Select(c => c.enemy.id));
            Assert.AreEqual(75f, f[0].portals.duration); Assert.AreEqual(30f, f[0].portals.budget);
            Assert.AreEqual(78f, f[1].portals.duration); Assert.AreEqual(44f, f[1].portals.budget);
            Assert.AreEqual("enemy_gatelord", f[2].enemies.Single().id, "piętro III: Władca Bramy");
            Assert.IsFalse(f[2].isBoss, "Władca Bramy nie kończy podejścia");
            Assert.IsTrue(f[f.Count - 1].isBoss, "ostatnie piętro – boss Turris");
        }

        [Test]
        public void Imp_AlwaysComesInOneOfFourElements()
        {
            var imp = fx.Get<EnemyDefinition>("enemy_imp");
            Assert.AreEqual(Element.Fire, imp.element);
            CollectionAssert.AreEquivalent(new[] { Element.Frost, Element.Lightning, Element.Earth }, imp.variants.Select(v => v.element));
            Assert.AreEqual(0.75f, imp.variantChance, 1e-4f, "każdy żywioł po 25%");
            foreach (var v in imp.variants.Append(imp))
            {
                Assert.LessOrEqual(v.ElementMultiplier(v.element), 0.75f, v.id + ": odporny na własny żywioł");
                Assert.IsTrue(v.attacks.All(a => a.attack.element == v.element && a.attack.elementShare > 0.5f), v.id + ": tnie swoim żywiołem");
            }
        }

        [Test]
        public void Worm_PackOfFour_EliteLatches_BossSummonsBats()
        {
            var worm = fx.Get<EnemyDefinition>("enemy_worm");
            Assert.AreEqual(4, worm.packSize);
            Assert.AreEqual(2.5f, worm.spawnCost);
            var latch = worm.attacks.Single(a => a.special == EnemySpecial.Latch);
            Assert.IsTrue(latch.eliteOnly, "na piętrach I–III specjał znają tylko elity (HU: od fali 4)");
            Assert.AreEqual(7f, worm.eliteHealthMultiplier);

            var lord = fx.Get<EnemyDefinition>("enemy_gatelord");
            Assert.IsTrue(lord.isBoss);
            var specials = lord.attacks.Select(a => a.special).ToList();
            foreach (var s in new[] { EnemySpecial.Charge, EnemySpecial.FireZones, EnemySpecial.Orbs, EnemySpecial.Leap, EnemySpecial.Summon })
                Assert.Contains(s, specials);
            Assert.AreEqual(2, lord.phases.Count);
            var open = lord.attacks[lord.phases[0].openingAttack];
            Assert.AreEqual(EnemySpecial.Summon, open.special, "połowa życia – wezwanie nietoperzy");
            Assert.IsTrue(open.summon.isMinion);
            Assert.AreEqual(1.35f, lord.phases[1].speedMultiplier, "szał poniżej 30%");
        }
    }
}
