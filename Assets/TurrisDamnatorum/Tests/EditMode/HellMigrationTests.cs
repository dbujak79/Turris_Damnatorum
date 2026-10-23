using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    public class HellMigrationTests
    {
        class DisposableFixture : Fixture, IDisposable { public void Dispose() => Cleanup(); }
        [Test]
        public void CocoonRoot_LeavesActionsAvailable_AndHasRepeatProtection()
        {
            using (var fx = new DisposableFixture())
            {
                var status = new StatusEffects();
                Assert.AreEqual(1.2f, status.TryRoot(1.2f));
                Assert.AreEqual(0, status.MoveMultiplier(fx.Cfg.balance));
                Assert.AreEqual(1, status.ActionSpeedMultiplier(fx.Cfg.balance));
                Assert.IsFalse(status.Incapacitated); Assert.AreEqual(0, status.TryRoot(3));
                status.Tick(1.3f, false, fx.Cfg.balance); Assert.AreEqual(0, status.TryRoot(1.2f));
                status.Tick(2, false, fx.Cfg.balance); Assert.AreEqual(0.6f, status.TryRoot(1.2f));
                status.Tick(7, false, fx.Cfg.balance); Assert.AreEqual(3, status.TryRoot(30));
                status.Clear(); status.TouchWeb(); Assert.AreEqual(0.5f, status.MoveMultiplier(fx.Cfg.balance));
                status.Tick(0.2f, false, fx.Cfg.balance); Assert.IsFalse(status.Webbed);
            }
        }
        [Serializable] class Row { public string id, targetId, sha256; }
        [Serializable] class Catalog { public Row[] skills, weapons, enemies; }
        [Test]
        public void Catalog_HasUniqueMappingsAndSourceHashes()
        {
            var catalog = JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("HellCatalog/catalog").text);
            Assert.AreEqual(61, catalog.skills.Length); Assert.AreEqual(53, catalog.weapons.Length); Assert.AreEqual(34, catalog.enemies.Length);
            foreach (var group in new[] { catalog.skills, catalog.weapons, catalog.enemies })
            {
                Assert.AreEqual(group.Length, group.Select(r => r.id).Distinct().Count());
                Assert.IsTrue(group.All(r => r.sha256.Length == 64));
                var mapped = group.Where(r => !string.IsNullOrEmpty(r.targetId)).ToArray();
                Assert.AreEqual(mapped.Length, mapped.Select(r => r.targetId).Distinct().Count());
                using (var fx = new DisposableFixture()) foreach (var r in mapped) Assert.IsTrue(fx.bundle.byId.ContainsKey(r.targetId), r.id + " → " + r.targetId);
            }
        }

        [Test]
        public void AllDemonAbilities_HaveDelivery_PhaseAndComboLinks()
        {
            string[] supported = { "Combo", "Swing", "Volley", "Zones", "InfernalCharge", "InfernalOrb", "TidalWave", "CycloneEye", "WindStep", "Whirlwind", "Quake", "StoneSpikes", "StoneWard", "Summon", "Flood", "Rockfall", "Latch", "ShieldBash", "BloodRite", "Cocoon", "EggSac", "Vomit", "Petrify", "Tether" };
            using (var fx = new DisposableFixture())
                foreach (var source in HellEnemyCatalog.Rows)
                {
                    var enemy = fx.Get<EnemyDefinition>(source.targetId);
                    Assert.IsNotNull(enemy); Assert.Greater(enemy.maxHealth, 0);
                    foreach (var text in source.BossAbilities.Concat(source.Specials)) Assert.Contains(HellEnemyAbility.Parse(text).name, supported, source.Id);
                    foreach (var a in enemy.attacks)
                    {
                        Assert.Greater(a.attack.TotalDuration, 0);
                        Assert.Less(a.followUp, enemy.attacks.Count, enemy.id);
                        if (a.huOnPhase > 0) Assert.AreEqual(0, a.weight, enemy.id + ": zdarzenie poza rotacją");
                    }
                    if (!string.IsNullOrEmpty(enemy.huSourceId) && source.Archetype == 3)
                        CollectionAssert.AreEqual(source.BossPhaseThresholds, enemy.phases.Select(p => p.healthThreshold));
                }
        }

        [Test]
        public void Appearance_DoesNotChangeClassAttributesOrSkills()
        {
            using (var fx = new DisposableFixture())
            {
                var knight = fx.Get<ClassDefinition>("class_knight"); var mage = fx.Get<ClassDefinition>("class_mage");
                var plan = new LoadoutPlan { classDef = knight, appearance = mage, difficulty = fx.Cfg.difficulties[0] };
                var run = RunFactory.Create(plan, fx.Cfg, 123);
                Assert.AreSame(knight, run.startingClass); Assert.AreSame(mage, run.appearance);
                Assert.AreEqual(knight.strength, run.attributes.strength);
                Assert.AreSame(knight.signatureSkill, run.startingClass.signatureSkill);
                Assert.AreEqual(plan.Weapon, run.equipment.Get(EquipSlot.MainHand).definition);
            }
        }

        [Test]
        public void IK_PreservesBoneLengths_AndReachesTarget()
        {
            var root = new GameObject("IK"); var lower = new GameObject("elbow"); var hand = new GameObject("hand");
            lower.transform.SetParent(root.transform); hand.transform.SetParent(lower.transform);
            lower.transform.localPosition = Vector3.right * 0.4f; hand.transform.localPosition = Vector3.right * 0.35f;
            var target = new Vector3(0.4f, 0.2f, 0.3f);
            try
            {
                HellEquipmentPose.Solve(root.transform, lower.transform, hand.transform, target, Vector3.down, 1);
                Assert.Less(Vector3.Distance(hand.transform.position, target), 0.002f);
                Assert.AreEqual(0.4f, Vector3.Distance(root.transform.position, lower.transform.position), 0.001f);
                Assert.AreEqual(0.35f, Vector3.Distance(lower.transform.position, hand.transform.position), 0.001f);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        [Test]
        public void SourceCombo_DefenseSignalsAndEnrageArePreserved()
        {
            using (var fx = new DisposableFixture())
            {
                var titan = fx.Get<EnemyDefinition>("enemy_hu_boss_titan");
                var thrust = titan.attacks.First(a => a.huAbility?.argument == "thrust");
                Assert.IsFalse(thrust.attack.parryable); Assert.IsTrue(thrust.attack.blockable);
                var slam = titan.attacks.First(a => a.huAbility?.argument == "slam");
                Assert.IsFalse(slam.attack.dodgeable); Assert.IsTrue(slam.attack.blockable);
                var spin = titan.attacks.First(a => a.huAbility?.argument == "spin"); Assert.IsFalse(spin.attack.blockable); Assert.IsTrue(spin.attack.dodgeable);
                Assert.IsTrue(titan.attacks.Any(a => a.huEnragedOnly));
                Assert.AreEqual(0.3f, titan.huEnrageHealth, 0.001f);
                var bow = fx.Get<ItemDefinition>("hu_bow"); Assert.AreEqual(AttackAnim.BowDraw, AnimResolve.ForAttack(bow.weapon.light, 0, WeaponModel.Bow));
                var staff = fx.Get<ItemDefinition>("hu_staff"); Assert.AreEqual(AttackDelivery.Projectile, staff.weapon.light.delivery); Assert.AreEqual(Element.Fire, staff.weapon.light.element);
                var flail = fx.Get<ItemDefinition>("hu_flail2h"); Assert.AreEqual(2, flail.weapon.light.statuses.Single(s => s.kind == StatusKind.Stagger).stacks);
            }
        }
    }
}
