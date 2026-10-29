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
        public void FloorTwo_FewerCultists_TwentyPercentFewerDemons()
        {
            var p = fx.Cfg.tower.floors[1].portals;
            // HU (budżet 44, waga kultysty 4,5): ok. 16,9 kultysty, 13,9 chochlika, 5,3 czerwia. Turris: budżet ×0,8 i waga kultysty 3,7.
            Assert.AreEqual(11.9f, Expected(p, "enemy_hu_cultist"), 0.3f, "kultyści");
            Assert.AreEqual(11.9f, Expected(p, "enemy_imp"), 0.3f, "chochliki");
            Assert.AreEqual(4.5f, Expected(p, "enemy_worm"), 0.2f, "czerwie");
        }

        /// <summary>Wieża to 30 fal Hell Unleashed: boss co trzecie piętro z kapliczką po nim, finał (Arcydemon) kończy podejście.</summary>
        [Test]
        public void Tower_Is30HuWaves_BossEveryThirdFloor()
        {
            var f = fx.Cfg.tower.floors;
            Assert.AreEqual(30, f.Count);
            for (int i = 0; i < f.Count; i++)
            {
                bool boss = (i + 1) % 3 == 0;
                Assert.AreEqual(boss, f[i].isDuel, $"piętro {i + 1}: pojedynek z bossem");
                Assert.AreEqual(boss, !f[i].portals.enabled, $"piętro {i + 1}: portale");
                Assert.AreEqual(boss && i < 29, f[i].restAfter, $"piętro {i + 1}: kapliczka po bossie");
                Assert.AreEqual(i == 29, f[i].isBoss, $"piętro {i + 1}: zwycięstwo tylko po finale");
                if (boss) Assert.AreEqual(1, f[i].enemies.Count(e => e != null && e.isBoss), $"piętro {i + 1}: boss");
                else Assert.IsTrue(f[i].portals.composition.Count > 0 && f[i].portals.composition.All(c => c.enemy != null), $"piętro {i + 1}: skład z demonów HU");
            }
            Assert.AreEqual("enemy_gatelord", f[2].enemies[0].id, "piętro III: Władca Bramy");
        }

        /// <summary>Zwykłe piętro = fala HU: czas, budżet, grupy, skład i mnożniki (życie z łagodzeniem późnych fal).</summary>
        [Test]
        public void Floors_CopyHuWaveNumbers()
        {
            var w = HellTower.Rows[6];
            var f = fx.Cfg.tower.floors[6];
            // Fala 7: o 20% mniej demonów i o jednego mniej – najsłabszego (korekta Turris po raporcie botów).
            Assert.AreEqual(w.SpawnBudget * HellTower.MonsterCountScale - HellTower.FewerEnemies[7] * HellTower.Weakest(f.portals).spawnCost, f.portals.budget, 1e-3f);
            var eight = fx.Cfg.tower.floors[7];
            Assert.AreEqual(HellTower.Rows[7].SpawnBudget * HellTower.MonsterCountScale, eight.portals.budget, 1e-3f, "bez korekty: o 20% mniej niż w HU");
            Assert.AreEqual(w.DurationSeconds / HellTower.SpawnTempoScale, f.portals.duration, 1e-3f, "portale otwarte dłużej");
            Assert.AreEqual(w.GroupIntervalSeconds / HellTower.SpawnTempoScale, f.portals.groupInterval, 1e-3f, "tempo wychodzenia o połowę wolniejsze");
            Assert.AreEqual(w.MaxGroupSize, f.portals.maxGroup);
            Assert.AreEqual(w.Composition.Length, f.portals.composition.Count, "skład jak w HU");
            Assert.AreEqual(w.DamageMultiplier, f.DamageScale, 1e-4f);
            Assert.AreEqual(w.HealthMultiplier, f.statScale, 1e-4f, "do fali 10 bez łagodzenia");
            var late = fx.Cfg.tower.floors[28];
            Assert.AreEqual(HellTower.Rows[28].HealthMultiplier * 0.75f, late.statScale, 1e-3f, "fala 29: życie ×0,75");
            Assert.Greater(late.DamageScale, f.DamageScale);
        }

        [Test]
        public void Cleave_KnocksDown_BalancedToTarget()
        {
            var data = HuCatalog.Skill("skill.warrior.cleave");
            Assert.IsNotNull(data);
            CollectionAssert.AreEqual(new[] { 32f, 36f, 36f }, data.levels.Select(l => l.GetF("damage.physical")).ToArray());
            CollectionAssert.AreEqual(new[] { 5f, 5f, 5f }, data.levels.Select(l => l.GetF("cooldown")).ToArray(), "odnowienie 5 s");
            CollectionAssert.AreEqual(new[] { 1f, 1f, 1f }, data.levels.Select(l => l.GetF("down_seconds")).ToArray(), "powalenie 1 s");
        }

        /// <summary>Seria 5 botów: skille wojownika w jednym przedziale obrażeń na odnowienie (poziom 1, bez form).</summary>
        [Test]
        public void WarriorSkills_DamagePerCooldown_InOneBand()
        {
            // Fala przełamania pominięta: trafia stożkiem i falą (dwa razy), więc jej parametry są celowo niższe.
            var ids = new[] { "cleave", "wind_slash", "executioners_fall", "infernal_charge", "quake" };
            var perCd = ids.ToDictionary(id => id, id =>
            {
                var l = HuCatalog.Skill("skill.warrior." + id).levels[0];
                return (float)l.TotalDamage / l.GetF("cooldown");
            });
            float min = perCd.Values.Min(), max = perCd.Values.Max();
            Assert.Less(max / min, 1.6f, string.Join(", ", perCd.Select(kv => $"{kv.Key} {kv.Value:0.0}")));
        }
    }
}
