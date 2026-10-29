using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>Reszta Hell Unleashed: perki zmieniające mechanikę, limity obrony, trzy oferty skilli, dusze i cechy za dusze.</summary>
    public class HuPerkTests
    {
        Fixture fx;
        BalanceConfig B => fx.Cfg.balance;

        [SetUp]
        public void SetUp() => fx = new Fixture();

        [TearDown]
        public void TearDown() => fx.Cleanup();

        BoonDefinition Perk(string huId) => fx.Get<BoonDefinition>("hu_perk_" + huId.Replace("upgrade.", "").Replace('.', '_'));

        [Test]
        public void AllHuPerks_AreInContent_WithMechanics()
        {
            Assert.AreEqual(HellEconomy.Upgrades.Count, HellEconomy.Upgrades.Count(DefaultContent.PerkSupported), "wszystkie 46 perków HU ma odpowiednik");
            Assert.AreEqual(HellEconomy.Upgrades.Count, fx.Cfg.baseBoonPool.Count(b => b != null && b.id.StartsWith("hu_perk_")));
            Assert.AreEqual(PassiveEffectType.ProjectileRicochet, Perk("upgrade.ricochet").effects.Single().type);
            Assert.AreEqual(5f, Perk("upgrade.penetration.fire").effects.Single(e => e.type == PassiveEffectType.PenetrationFire).value, 1e-4f);
            Assert.AreEqual(0.5f, Perk("upgrade.warrior.emberedge").effects.Single(e => e.type == PassiveEffectType.ConvertPhysicalToFire).value, 1e-4f);
            Assert.IsTrue(Perk("upgrade.added.fire").effects.Any(e => e.type == PassiveEffectType.BurnOnHit), "Płonące ostrze podpala");
            var deep = Perk("upgrade.warrior.deepwounds");
            Assert.IsTrue(deep.effects.Any(e => e.type == PassiveEffectType.BleedOnHit) && deep.effects.Any(e => e.type == PassiveEffectType.BleedMaxStacksBonus));
            Assert.IsTrue(deep.modifiers.Any(m => m.stat == StatType.BleedDamage));
            CollectionAssert.AreEqual(new[] { 2, 3, 5, 8 }, Enumerable.Range(1, 4).Select(l => PerkMechanics.VolleyCount(l)), "Rozszczepienie: łącznie 2/3/5/8 pocisków");
            CollectionAssert.AreEqual(new[] { 0.3f, 0.45f, 0.6f }, Enumerable.Range(1, 3).Select(l => PerkMechanics.ExplosionShare(l)));
        }

        [Test]
        public void ShieldPerks_SleepWithoutShield_PassiveBlockAndCost()
        {
            var run = fx.NewRun("class_knight");
            var shield = fx.Cfg.baseItemPool.First(i => i != null && i.IsShield);
            run.equipment.Unequip(EquipSlot.OffHand);
            var block = Perk("upgrade.blockchance");
            run.AddBoon(block); run.AddBoon(block);
            run.AddBoon(Perk("upgrade.blockcost"));
            var bare = BuildCalculator.Compute(run, fx.Cfg);
            Assert.AreEqual(0f, bare.effects[PassiveEffectType.PassiveBlockChance], "bez tarczy perk śpi");
            Assert.AreEqual(0f, bare.passiveBlockChance);

            Assert.IsTrue(run.equipment.Equip(new ItemInstance(shield, 1), EquipSlot.OffHand, new System.Collections.Generic.List<ItemInstance>()));
            var snap = BuildCalculator.Compute(run, fx.Cfg);
            Assert.AreEqual(0.15f + 0.02f + 0.10f, snap.passiveBlockChance, 1e-4f, "15% + 2% za poziom II tarczy + 2×5% perka");
            Assert.AreEqual(shield.shield.guard.stabilityMultiplier * 0.85f, snap.guard.stabilityMultiplier, 1e-4f, "Lekka tarcza: −15% kosztu bloku");
            Assert.AreEqual(0.92f, PerkMechanics.ShieldBlockCooldown(snap.shieldLevel), 1e-4f);
        }

        [Test]
        public void Residue_NeedsUnstable_Evasion_Area()
        {
            var run = fx.NewRun("class_knight");
            var residue = Perk("upgrade.mage.residue");
            var row = FloorShop.PerkRow(residue);
            Assert.IsFalse(PerkMechanics.Active(row, run), "pozostałość bez broni dystansowej i bez Niestabilnych pocisków – uśpiona");
            run.AddBoon(Perk("upgrade.evasion"));
            run.AddBoon(Perk("upgrade.area"));
            var snap = BuildCalculator.Compute(run, fx.Cfg);
            Assert.Greater(snap.evasionChance, 0.039f, "Refleks: +4% uchylenia");
            Assert.LessOrEqual(snap.evasionChance, 0.5f);
            Assert.AreEqual(10f, snap.effects[PassiveEffectType.AreaSize], 1e-4f);
        }

        [Test]
        public void DefenseCaps_FollowHuBalance2()
        {
            Assert.AreEqual(21.6f, B.dodgeStaminaCost, 1e-4f, "unik droższy o 20%");
            Assert.AreEqual(30f, B.HeroResist(30f), 1e-4f);
            Assert.AreEqual(50f, B.HeroResist(60f), 1e-4f, "pełna do 40%, dalej połowa");
            Assert.AreEqual(65f, B.HeroResist(200f), 1e-4f, "limit 65%");
            Assert.AreEqual(-20f, B.HeroResist(-20f), 1e-4f, "słabość bez zmian");
            Assert.AreEqual(0.325f, B.HeroDamageReduction(0.4f), 1e-4f, "40% surowej redukcji = 32,5%");
            Assert.AreEqual(0.45f, B.HeroDamageReduction(2f), 1e-4f, "limit 45%");
            Assert.AreEqual(new Vector2Int(28, 35), HellTower.EliteEvery);
            Assert.AreEqual(2, HellTower.EliteFirstWave);
        }

        [Test]
        public void DirectHit_KeepsMinimum_PassiveHealingCapped()
        {
            var run = fx.NewRun("class_knight");
            var pc = fx.NewCombatant(run);
            pc.Run.boons.Clear();
            float before = pc.Health.Current;
            // Cios z boku bez gardy: ogromna redukcja nie zbija go poniżej 15% surowych obrażeń.
            var hit = Fixture.FrontHit(physical: 100f, blockable: false, dodgeable: false); hit.attacker = new object(); // bez uchylenia i bloku biernego
            pc.Run.tempModifiers.Add(new StatModifier(StatType.DamageReduction, 500f));
            pc.Run.tempModifiers.Add(new StatModifier(StatType.PhysicalDefense, 100000f));
            pc.RefreshBuild();
            pc.ReceiveHit(hit);
            Assert.GreaterOrEqual(before - pc.Health.Current, 14.9f, "minimum 15% surowego ciosu");

            pc.Health.Drain(pc.Health.Current * 0.5f);
            float hp = pc.Health.Current;
            float healed = pc.PassiveHeal(10000f) + pc.PassiveHeal(10000f);
            Assert.AreEqual(pc.Health.Max * B.passiveHealingPerSecond, healed, 0.01f, "8% maks. życia w sekundzie");
            Assert.AreEqual(hp + healed, pc.Health.Current, 0.01f);
        }

        [Test]
        public void SkillShop_OffersThreeDifferentSkills()
        {
            var run = fx.NewRun("class_knight");
            run.floorsCleared = 8;
            var snap = BuildCalculator.Compute(run, fx.Cfg);
            var pools = new RewardPools();
            pools.spells.AddRange(fx.Cfg.baseSpellPool);
            for (int seed = 0; seed < 10; seed++)
            {
                var offers = SoulShop.PickOffers(run, snap, pools, new System.Random(seed), B);
                Assert.AreEqual(SoulShop.SkillOfferCount, offers.Count);
                Assert.AreEqual(offers.Count, offers.Distinct().Count(), "bez powtórek");
                Assert.IsTrue(offers.All(o => run.FindSpell(o) == null));
            }
        }
    }
}
