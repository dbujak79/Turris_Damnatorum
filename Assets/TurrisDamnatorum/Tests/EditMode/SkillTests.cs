using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>
    /// Trzy sloty umiejętności (czary i techniki), odnowienia i ładunki,
    /// odblokowania na stałe oraz umiejętności zdobywane tylko na jedno podejście.
    /// </summary>
    public class SkillTests
    {
        Fixture fx;

        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        SpellDefinition Def(string id) => fx.Get<SpellDefinition>(id);

        [Test]
        public void Slots_AssignSwapAndClear()
        {
            var run = fx.NewRun("class_knight");
            var cleave = run.FindSpell(Def("skill_cleave"));
            var bash = run.FindSpell(Def("skill_shieldbash"));
            CollectionAssert.AreEqual(new[] { cleave, bash, null }, run.skillSlots, "Umiejętności klasy w kolejnych slotach");

            var bolt = run.LearnSpell(Def("spell_bolt"));
            Assert.AreEqual(2, run.SlotOf(bolt), "Nowa umiejętność trafia do wolnego slotu");

            run.AssignSlot(bolt, 0);
            CollectionAssert.AreEqual(new[] { bolt, bash, cleave }, run.skillSlots, "Przełożenie zamienia sloty miejscami");
            run.AssignSlot(bolt, 0);
            Assert.AreSame(bolt, run.skillSlots[0], "Ponowne przypisanie do tego samego slotu niczego nie zmienia");
            run.ClearSlot(0);
            Assert.IsNull(run.skillSlots[0]);
            Assert.IsTrue(run.knownSpells.Contains(bolt), "…a umiejętność zostaje w kolekcji");

            var charge = run.LearnSpell(Def("skill_charge"));
            Assert.AreEqual(0, run.SlotOf(charge));
            var nova = run.LearnSpell(Def("spell_nova"));
            Assert.AreEqual(-1, run.SlotOf(nova), "Przy zajętych slotach nowa umiejętność czeka w kolekcji");
        }

        [Test]
        public void EachSlotUsesItsOwnSkill_WithCooldown()
        {
            var run = fx.NewRun("class_knight");
            var pc = fx.NewCombatant(run);
            var cleave = pc.Skill(0);
            Assert.AreEqual(SpellKind.Cleave, cleave.Def.kind);

            float stamina = pc.Stamina.Current;
            pc.RequestSkill(0);
            Assert.AreEqual(ActionType.Cast, pc.Actions.Current);
            Assert.AreSame(cleave, pc.ActiveSkill);
            Assert.AreEqual(stamina - cleave.staminaCost, pc.Stamina.Current, 0.01f, "Technika kosztuje wytrzymałość…");
            Assert.AreEqual(pc.Mana.Max * run.manaFraction, pc.Mana.Current, 0.01f, "…a nie manę");
            Assert.IsFalse(cleave.instance.Ready);

            Fixture.Advance(pc, 1.2f);
            Assert.IsTrue(pc.Actions.IsIdle);
            pc.RequestSkill(0);
            Assert.AreNotEqual(ActionType.Cast, pc.Actions.Current, "W trakcie odnowienia umiejętność nie działa");

            // Drugi slot ma własne odnowienie.
            pc.RequestSkill(1);
            Assert.AreEqual(SpellKind.ShieldBash, pc.ActiveSkill.Def.kind);

            Fixture.Advance(pc, cleave.instance.Cooldown);
            Assert.IsTrue(cleave.instance.Ready, "Po odnowieniu umiejętność znów jest gotowa");
            pc.RequestSkill(0);
            Assert.AreSame(cleave, pc.ActiveSkill);
        }

        [Test]
        public void Cooldown_StaysWithSkill_WhenMovedToAnotherSlot()
        {
            var run = fx.NewRun("class_knight");
            var pc = fx.NewCombatant(run);
            var cleave = run.skillSlots[0];
            pc.RequestSkill(0);
            Fixture.Advance(pc, 1f);
            run.AssignSlot(cleave, 2);
            pc.RefreshBuild();
            Assert.IsFalse(pc.Skill(2).instance.Ready, "Przełożenie do innego slotu nie zeruje odnowienia");
            pc.OnFloorStart();
            Assert.IsTrue(cleave.Ready, "Nowe piętro – pełne ładunki");
        }

        [Test]
        public void ExtraCharge_FromUpgrade()
        {
            var run = fx.NewRun("class_knight");
            var charge = run.LearnSpell(Def("skill_charge"));
            Assert.AreEqual(1, charge.MaxCharges);
            charge.level = charge.definition.extraChargeAtLevel;
            Assert.AreEqual(2, charge.MaxCharges);
            Assert.IsTrue(charge.TryUse());
            Assert.IsTrue(charge.TryUse(), "Dwa ładunki – dwa użycia pod rząd");
            Assert.IsFalse(charge.TryUse());
            charge.Tick(charge.Cooldown + 0.01f);
            Assert.AreEqual(1, charge.Charges, "Ładunki wracają po jednym na odnowienie");
            charge.Tick(charge.Cooldown + 0.01f);
            Assert.AreEqual(2, charge.Charges);
        }

        [Test]
        public void ShieldBash_RequiresShield()
        {
            var run = fx.NewRun("class_knight");
            var pc = fx.NewCombatant(run);
            Assert.IsTrue(pc.Skill(1).equipmentMet);
            run.UnequipToInventory(EquipSlot.OffHand);
            pc.RefreshBuild();
            Assert.IsFalse(pc.Skill(1).equipmentMet, "Bez tarczy uderzenie tarczą jest niedostępne");
            pc.RequestSkill(1);
            Assert.IsTrue(pc.Actions.IsIdle);
            Assert.IsTrue(pc.Skill(1).instance.Ready, "Nieudana próba nie zużywa ładunku");
        }

        [Test]
        public void DodgeInterruptsTechnique_BeforeRelease_RefundsCharge()
        {
            var run = fx.NewRun("class_knight");
            var pc = fx.NewCombatant(run);
            pc.RequestSkill(0);
            Assert.AreEqual(ActionPhase.Startup, pc.Actions.Phase);
            pc.Request(ActionType.Dodge);
            Assert.AreEqual(ActionType.Dodge, pc.Actions.Current);
            Assert.IsTrue(pc.Skill(0).instance.Ready);
        }

        [Test]
        public void Barrier_AbsorbsDamage_AndFullyAbsorbedHitDoesNotFlinch()
        {
            var run = fx.NewRun("class_mage");
            var barrier = run.LearnSpell(Def("spell_barrier"));
            run.AssignSlot(barrier, 0);
            var pc = fx.NewCombatant(run);
            pc.RequestSkill(0);
            Fixture.Advance(pc, pc.ScaleStartup(barrier.definition.castTime) + 0.1f);
            float capacity = pc.BarrierAmount;
            Assert.Greater(capacity, barrier.definition.amount, "Osłona skaluje z Inteligencją");
            Fixture.Advance(pc, 1f);

            float hp = pc.Health.Current;
            var r = pc.ReceiveHit(Fixture.FrontHit(20));
            Assert.AreEqual(HitOutcome.Hit, r.outcome);
            Assert.AreEqual(hp, pc.Health.Current, 0.01f, "Osłona pochłonęła cios");
            Assert.AreNotEqual(ActionType.Flinch, pc.Actions.Current, "W pełni pochłonięty cios nie przerywa akcji");
            Assert.Less(pc.BarrierAmount, capacity);

            pc.ReceiveHit(Fixture.FrontHit(capacity * 3f));
            Assert.AreEqual(0f, pc.BarrierAmount, "Silny cios rozbija osłonę…");
            Assert.Less(pc.Health.Current, hp, "…a nadmiar trafia w życie");
        }

        [Test]
        public void PermanentUnlock_IsInEveryRun_RewardSkillOnlyInThisRun()
        {
            var storage = new MemoryProfileStorage();
            var meta = new MetaService(fx.Cfg, storage);
            meta.DebugAddAsh(100);
            Assert.IsTrue(meta.Purchase(fx.Get<UnlockDefinition>("unlock_charge")));

            var run1 = RunFactory.Create(meta.LastPlan(), fx.Cfg, 1);
            var charge = run1.FindSpell(Def("skill_charge"));
            Assert.IsNotNull(charge, "Odblokowana umiejętność jest w kolekcji od startu");
            Assert.IsTrue(charge.permanent);

            new RewardOption { kind = RewardKind.LearnSpell, spell = Def("spell_nova") }.Apply(run1);
            Assert.IsFalse(run1.FindSpell(Def("spell_nova")).permanent, "Nagroda między piętrami – tylko na to podejście");

            // Kolejne podejście (także po ponownym wczytaniu profilu): stała umiejętność jest, tymczasowej nie ma.
            var run2 = RunFactory.Create(new MetaService(fx.Cfg, storage).LastPlan(), fx.Cfg, 2);
            Assert.IsNotNull(run2.FindSpell(Def("skill_charge")));
            Assert.IsNull(run2.FindSpell(Def("spell_nova")));
        }

        [Test]
        public void SkillSlotLayout_IsRememberedBetweenRuns()
        {
            var storage = new MemoryProfileStorage();
            var meta = new MetaService(fx.Cfg, storage);
            var plan = meta.LastPlan();
            var bolt = Def("spell_bolt");
            plan.AssignSlot(bolt, 0);
            meta.RememberSkillSlots(plan.skillSlots);

            var plan2 = new MetaService(fx.Cfg, storage).LastPlan();
            Assert.AreSame(bolt, plan2.skillSlots[0]);
            var run = RunFactory.Create(plan2, fx.Cfg, 3);
            Assert.AreEqual(bolt, run.skillSlots[0].definition, "Wybrany układ obowiązuje od startu");
            Assert.IsTrue(run.skillSlots.All(s => s != null), "Pozostałe sloty wypełniły się automatycznie");
        }

        [Test]
        public void RewardPool_OffersLockedSkillsTemporarily_AndRespectsEquipment()
        {
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            var whirl = Def("skill_whirlwind");
            Assert.IsFalse(meta.BuildRewardPools().spells.Contains(whirl), "Przed dotarciem na piętro 2 młynka nie ma w puli");
            meta.RecordReachedFloor(1);
            Assert.IsTrue(meta.BuildRewardPools().spells.Contains(whirl), "Nieodblokowany młynek można zdobyć na jedno podejście");

            var pools = meta.BuildRewardPools();
            var offered = new HashSet<string>();
            for (int seed = 0; seed < 300; seed++)
            {
                var run = fx.NewRun("class_mage");
                foreach (var o in RewardGenerator.Generate(run, BuildCalculator.Compute(run, fx.Cfg), pools, fx.Cfg, new System.Random(seed)))
                    if (o.kind == RewardKind.LearnSpell) offered.Add(o.spell.id);
            }
            Assert.IsFalse(offered.Contains("skill_shieldbash"), "Mag bez tarczy nie dostaje uderzenia tarczą");
            Assert.IsTrue(offered.Contains("skill_cleave"), "Techniki bronią są proponowane także magowi");
        }

        [Test]
        public void TechniqueDamage_ScalesWithWeapon()
        {
            var run = fx.NewRun("class_knight");
            var cleave = run.skillSlots[0];
            var snap = BuildCalculator.Compute(run, fx.Cfg);
            float before = snap.WeaponDamage(snap.weapon.light) * snap.skills[0].power;
            run.equipment.Get(EquipSlot.MainHand).level = 3;
            snap = BuildCalculator.Compute(run, fx.Cfg);
            float after = snap.WeaponDamage(snap.weapon.light) * snap.skills[0].power;
            Assert.Greater(after, before, "Ulepszenie broni wzmacnia techniki");
            Assert.AreEqual(0f, snap.skills[0].manaCost);
            Assert.AreEqual(SkillCategory.Technique, cleave.definition.category);
        }
    }
}
