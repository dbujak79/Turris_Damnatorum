using System.Collections.Generic;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Cztery cechy postaci i ich rozwój (plan rozwoju, etap G).</summary>
    public class AttributeTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        BalanceConfig B => fx.Cfg.balance;
        BuildSnapshot Snap(RunState run) => BuildCalculator.Compute(run, fx.Cfg);

        [Test]
        public void Classes_HaveStartingAttributes()
        {
            var knight = fx.NewRun("class_knight").attributes;
            var mage = fx.NewRun("class_mage").attributes;
            Assert.Greater(knight.strength, mage.strength);
            Assert.Greater(knight.toughness, mage.toughness);
            Assert.Greater(mage.intelligence, knight.intelligence);
        }

        [Test]
        public void Toughness_GivesHealthStaminaAndLoad_IntelligenceGivesMana()
        {
            var run = fx.NewRun("class_knight");
            var before = Snap(run);
            run.attributes.Add(AttributeType.Toughness, 2);
            var after = Snap(run);
            Assert.AreEqual(before.MaxHealth + 2 * B.healthPerToughness, after.MaxHealth, 0.01f);
            Assert.AreEqual(before.MaxStamina + 2 * B.staminaPerToughness, after.MaxStamina, 0.01f);
            Assert.AreEqual(before.stats[StatType.EquipLoad] + 2 * B.equipLoadPerToughness, after.stats[StatType.EquipLoad], 0.01f);

            run.attributes.Add(AttributeType.Intelligence, 3);
            Assert.AreEqual(after.MaxMana + 3 * B.manaPerIntelligence, Snap(run).MaxMana, 0.01f);
        }

        [Test]
        public void LegacyModifiers_CountAsToughnessAndIntelligence()
        {
            var attrs = new AttributeBlock { toughness = 10, strength = 10, dexterity = 10, intelligence = 10 };
            var sheet = StatCalculator.Compute(attrs, new List<StatModifier> { new StatModifier(StatType.LegacyEndurance, 3), new StatModifier(StatType.LegacyMind, 2) }, B);
            Assert.AreEqual(13f, sheet[StatType.Toughness], 1e-3f, "Stara Kondycja → Wytrzymałość");
            Assert.AreEqual(12f, sheet[StatType.Intelligence], 1e-3f, "Stary Umysł → Inteligencja");
        }

        [Test]
        public void Dexterity_Evasion_Dodge_AttackSpeed_WithinLimits()
        {
            var run = fx.NewRun("class_knight");
            run.attributes.dexterity = 12;
            var s12 = Snap(run);
            Assert.AreEqual(0f, s12.evasionChance, "Uchylenie dopiero powyżej progu");
            run.attributes.dexterity = 16;
            var s16 = Snap(run);
            Assert.AreEqual((16 - B.evasionFromDexterity) * B.evasionPerDexterity / 100f, s16.evasionChance, 1e-4f);
            Assert.Greater(s16.dodge.distance, s12.dodge.distance, "Dłuższy unik");
            Assert.Greater(s16.attackSpeed, s12.attackSpeed, "Szybsze ataki");

            run.attributes.dexterity = 80;
            var sMax = Snap(run);
            Assert.AreEqual(B.maxEvasion / 100f, sMax.evasionChance, 1e-4f, "Limit uchylenia");
            Assert.AreEqual(1f + B.maxAttackSpeedBonus, sMax.attackSpeed, 1e-4f, "Limit szybkości ataku");
            Assert.LessOrEqual(sMax.dodge.distance, B.dodgeDistance * (1f + B.maxDodgeDistanceBonus) + 1e-3f, "Limit długości uniku");
        }

        [Test]
        public void Evasion_MakesSomeDirectHitsMiss_NeverStatusTicks()
        {
            var run = fx.NewRun("class_knight");
            run.attributes.dexterity = 80; // limit: 25%
            var pc = fx.NewCombatant(run);
            int misses = 0;
            for (int i = 0; i < 400; i++)
            {
                pc.Health.SetCurrent(pc.Health.Max);
                var hit = Fixture.FrontHit(5); hit.attacker = new object();
                if (pc.ReceiveHit(hit).outcome == HitOutcome.Dodged) misses++;
                pc.Actions.Reset();
            }
            Assert.That(misses, Is.InRange(50, 150), "Około 25% trafień chybia");

            var tick = Fixture.FrontHit(5); tick.attacker = new object(); tick.isStatusTick = true;
            for (int i = 0; i < 50; i++) Assert.AreNotEqual(HitOutcome.Dodged, pc.ReceiveHit(tick).outcome, "Tyknięcia efektów zawsze trafiają");
        }

        [Test]
        public void Strength_UnmetWeaponRequirement_IsWeakerAndSlower()
        {
            var run = fx.NewRun("class_mage");
            var greataxe = new ItemInstance(fx.Get<ItemDefinition>("weapon_greataxe"));
            run.inventory.Add(greataxe);
            run.EquipFromInventory(greataxe, EquipSlot.MainHand);
            var snap = Snap(run);
            Assert.Less(snap.weaponEffectiveness, 1f);
            Assert.Less(snap.attackSpeed, 1f, "Za mało Siły – wolniejsze ataki");

            var pc = fx.NewCombatant(run);
            pc.Request(ActionType.LightAttack);
            Assert.Greater(pc.AttackWindup, pc.ScaleStartup(greataxe.definition.weapon.light.windup), "Zamach dłuższy niż bazowy");
        }

        [Test]
        public void Intelligence_GatesStrongerSpells()
        {
            var run = fx.NewRun("class_knight"); // Inteligencja 8
            var nova = run.LearnSpell(fx.Get<SpellDefinition>("spell_nova")); // wymaga 12
            var pc = fx.NewCombatant(run);
            pc.RequestSkill(run.SlotOf(nova));
            Assert.AreNotEqual(ActionType.Cast, pc.Actions.Current, "Bez Inteligencji czaru nie da się rzucić");
            Assert.IsTrue(nova.Ready, "Nieudana próba nie zużywa ładunku");

            run.attributes.intelligence = 12;
            pc.RefreshBuild();
            pc.RequestSkill(run.SlotOf(nova));
            Assert.AreEqual(ActionType.Cast, pc.Actions.Current, "Po rozwinięciu Inteligencji – działa");
        }

        [Test]
        public void AttributePoint_PerFloor_IsSpentOnce()
        {
            var run = fx.NewRun("class_knight");
            int str = run.attributes.strength;
            Assert.IsFalse(run.SpendAttributePoint(AttributeType.Strength), "Brak punktów");
            run.attributePoints = B.attributePointsPerFloor;
            Assert.IsTrue(run.SpendAttributePoint(AttributeType.Strength));
            Assert.AreEqual(str + 1, run.attributes.strength);
            Assert.AreEqual(0, run.attributePoints);
        }
    }
}
