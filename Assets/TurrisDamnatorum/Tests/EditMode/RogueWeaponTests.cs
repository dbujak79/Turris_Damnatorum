using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Klasa Łotrzyk, noże bliźniacze i łuk.</summary>
    public class RogueWeaponTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        [Test]
        public void Rogue_StartsNimble_WithKnivesAndTechniques()
        {
            var run = fx.NewRun("class_rogue");
            Assert.AreEqual(16, run.attributes.dexterity);
            var snap = BuildCalculator.Compute(run, fx.Cfg);
            Assert.Greater(snap.evasionChance, 0f, "Zręczność 16 – uchylenie");
            Assert.Greater(snap.attackSpeed, 1f, "…i szybsze ataki");
            Assert.AreEqual("weapon_knives", run.equipment.Get(EquipSlot.MainHand).definition.id);
            Assert.IsNull(run.equipment.Get(EquipSlot.OffHand), "Noże zajmują obie ręce");
            Assert.AreEqual(1f, snap.weaponEffectiveness, "Łotrzyk spełnia wymagania noży");
            CollectionAssert.AreEquivalent(new[] { "skill_lunge", "skill_knives" }, run.knownSpells.ConvertAll(s => s.definition.id));
            var look = RigLook.ForPlayer(run, fx.Cfg);
            Assert.AreEqual(WeaponModel.Knives, look.weapon);
            Assert.AreEqual(HeadGear.Hood, look.head);
        }

        [Test]
        public void Knives_AreFast_Bleed_ComboOfFour()
        {
            var knives = fx.Get<ItemDefinition>("weapon_knives").weapon;
            var sword = fx.Get<ItemDefinition>("weapon_longsword").weapon;
            Assert.Less(knives.light.windup, sword.light.windup);
            Assert.AreEqual(4, knives.lightComboLength);
            Assert.IsNotEmpty(knives.light.statuses);
            Assert.IsTrue(knives.canParry);
            Assert.IsFalse(knives.canBlock);

            var run = fx.NewRun("class_rogue");
            var pc = fx.NewCombatant(run);
            for (int i = 0; i < 4; i++)
            {
                pc.Request(ActionType.LightAttack);
                Fixture.Advance(pc, pc.AttackWindup + pc.AttackActive + 0.12f);
            }
            Assert.AreEqual(3, pc.Actions.ComboIndex, "Cztery cięcia w serii");
        }

        [Test]
        public void Floors_HaveWaves_BossFloorIsDuel()
        {
            var floors = fx.Cfg.tower.floors;
            for (int i = 0; i < floors.Count - 1; i++) Assert.GreaterOrEqual(floors[i].WaveCount, 3, floors[i].name + ": co najmniej 3 fale");
            Assert.AreEqual(1, floors[floors.Count - 1].WaveCount, "Boss – pojedynek");
            int total = 0;
            foreach (var f in floors) { total += f.enemies.Count; foreach (var w in f.extraWaves) total += w.enemies.Count; }
            Assert.GreaterOrEqual(total, 30, "Dużo więcej wrogów niż dawniej (8)");
            Assert.IsTrue(floors.Exists(f => f.extraWaves.Exists(w => w.enemies.Exists(e => e.id == "enemy_archer"))), "Łucznicy w falach");
        }

        [Test]
        public void Archer_IsRangedKiter_WithBowLook()
        {
            var a = fx.Get<EnemyDefinition>("enemy_archer");
            Assert.AreEqual(RigStyle.Archer, AnimResolve.ForEnemy(a));
            Assert.AreEqual(WeaponModel.Bow, AnimResolve.ForEnemyWeapon(a));
            Assert.Greater(a.retreatRange, 0f, "Ucieka przed zwarciem");
            Assert.IsTrue(a.attacks.TrueForAll(e => e.attack.delivery == AttackDelivery.Projectile));
            Assert.AreEqual(BodyGear.Leather, RigLook.ForEnemy(a).body);
        }

        [Test]
        public void ArrowTechniques_RequireBow()
        {
            var run = fx.NewRun("class_rogue");
            var fire = run.LearnSpell(fx.Get<SpellDefinition>("skill_firearrow"));
            run.AssignSlot(fire, 0);
            var pc = fx.NewCombatant(run);
            Assert.IsFalse(pc.Skill(0).equipmentMet, "Z nożami strzał specjalnych nie ma");
            var bow = new ItemInstance(fx.Get<ItemDefinition>("weapon_bow"));
            run.inventory.Add(bow);
            run.EquipFromInventory(bow, EquipSlot.MainHand);
            pc.RefreshBuild();
            Assert.IsTrue(pc.Skill(0).equipmentMet, "Z łukiem – działają");
            Assert.IsTrue(run.CurrentTags(pc.Build).HasFlag(BuildTag.Ranged));
            StringAssert.Contains("łuku", Describe.Spell(fx.Get<SpellDefinition>("skill_volley"), 0, null, fx.Cfg.balance));
        }

        [Test]
        public void Bow_ShootsProjectiles_NotMelee()
        {
            var bow = fx.Get<ItemDefinition>("weapon_bow");
            Assert.AreEqual(AttackDelivery.Projectile, bow.weapon.light.delivery);
            Assert.AreEqual(AttackDelivery.Projectile, bow.weapon.heavy.delivery);
            Assert.Greater(bow.weapon.heavy.baseDamage, bow.weapon.light.baseDamage);
            Assert.IsTrue(bow.twoHanded);
            Assert.AreEqual(WeaponModel.Bow, AnimResolve.ForItem(bow));
            Assert.AreEqual(AttackAnim.Cast, AnimResolve.ForAttack(bow.weapon.light, 0, WeaponModel.Bow));
            Assert.IsTrue(fx.Cfg.baseItemPool.Contains(bow) && fx.Cfg.unlocks.Exists(u => u.target == bow), "Łuk w nagrodach i jako dodatek startowy");
        }
    }
}
