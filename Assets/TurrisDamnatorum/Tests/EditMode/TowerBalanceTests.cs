using System.Linq;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Korekty po raporcie botów Wojownika: piętro II, wrogowie charakterystyczni pięter IV–V, Rozpłatanie.</summary>
    public class TowerBalanceTests
    {
        Fixture fx;

        [SetUp]
        public void SetUp() => fx = new Fixture();

        /// <summary>Oczekiwana liczba wrogów danego typu z portali: budżet / średni koszt losowania × udział wagi.</summary>
        static float Expected(PortalSpawning s, string id)
        {
            var comp = s.composition.Where(c => c.enemy != null && c.weight > 0).ToList();
            float total = comp.Sum(c => c.weight);
            float avgCost = comp.Sum(c => c.weight * c.enemy.spawnCost) / total;
            return s.budget / avgCost * comp.Where(c => c.enemy.id == id).Sum(c => c.weight) / total;
        }

        [Test]
        public void FloorTwo_HasThreeFewerCultists_SameImpsAndWorms()
        {
            var p = fx.Cfg.tower.floors[1].portals;
            // Przed korektą (budżet 44, waga kultysty 4,5): ok. 16,9 kultysty, 13,9 chochlika, 5,3 czerwia.
            Assert.AreEqual(13.9f, Expected(p, "enemy_hu_cultist"), 0.3f, "kultyści");
            Assert.AreEqual(13.9f, Expected(p, "enemy_imp"), 0.3f, "chochliki bez zmian");
            Assert.AreEqual(5.3f, Expected(p, "enemy_worm"), 0.2f, "czerwie bez zmian");
        }

        [Test]
        public void FloorsFourAndFive_GetOneSignatureEnemy_BossFloorsUnchanged()
        {
            var f = fx.Cfg.tower.floors;
            Assert.AreEqual(1, f[3].extraWaves[1].enemies.Count(e => e.id == "enemy_hu_charger"), "Zbrojownia: Taranowiec");
            Assert.AreEqual(1, f[4].extraWaves[2].enemies.Count(e => e.id == "enemy_hu_gargoyle"), "Brama Strażnika: Gargulec");
            Assert.IsFalse(f[3].isBoss || f[4].isBoss);
            Assert.IsTrue(f[5].isBoss);
            Assert.AreEqual(1, f[5].enemies.Count, "piętro z bossem bez dodatkowego wroga");
            Assert.AreEqual(0, f[5].extraWaves.Count);

            // Ponowne zastosowanie (migracja edytora na gotowym assecie) niczego nie dubluje.
            int before = f[3].extraWaves.Sum(w => w.enemies.Count) + f[4].extraWaves.Sum(w => w.enemies.Count);
            HellEncounters.ExtendTower(fx.Cfg, fx.bundle.byId);
            Assert.AreEqual(before, f[3].extraWaves.Sum(w => w.enemies.Count) + f[4].extraWaves.Sum(w => w.enemies.Count));
        }

        [Test]
        public void Cleave_IsStronger_KnocksDown_FourSecondCooldown()
        {
            var data = HuCatalog.Skill("skill.warrior.cleave");
            Assert.IsNotNull(data);
            CollectionAssert.AreEqual(new[] { 39f, 44f, 44f }, data.levels.Select(l => l.GetF("damage.physical")).ToArray());
            CollectionAssert.AreEqual(new[] { 4f, 4f, 4f }, data.levels.Select(l => l.GetF("cooldown")).ToArray(), "odnowienie 4 s");
            CollectionAssert.AreEqual(new[] { 1f, 1f, 1f }, data.levels.Select(l => l.GetF("down_seconds")).ToArray(), "powalenie 1 s");
        }
    }
}
