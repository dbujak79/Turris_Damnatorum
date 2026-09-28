using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>
    /// Kryteria ukończenia z dokumentu wymagań (sekcja 3), sprawdzane na prawdziwym komponencie PlayerCombat.
    /// </summary>
    public class CombatCriteriaTests
    {
        Fixture fx;

        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        [Test]
        public void Mage_CanEquipAxe_AndFightWithIt()
        {
            var run = fx.NewRun("class_mage");
            var axeDef = fx.Get<ItemDefinition>("weapon_axe");
            var axe = new ItemInstance(axeDef);
            run.inventory.Add(axe);

            Assert.IsTrue(run.EquipFromInventory(axe, EquipSlot.MainHand), "Mag powinien móc założyć topór");
            Assert.AreEqual(axe, run.equipment.Get(EquipSlot.MainHand));
            Assert.IsTrue(run.inventory.Exists(i => i.definition.id == "weapon_staff"), "Kostur wraca do plecaka");

            var pc = fx.NewCombatant(run);
            Assert.AreSame(axeDef.weapon, pc.Build.weapon, "Ataki pochodzą z topora");
            Assert.AreEqual(1f, pc.Build.weaponEffectiveness, "Mag (Siła 10) spełnia wymagania topora");
            Assert.IsTrue(pc.Build.CanBlock, "Topór umożliwia blok bronią");
            Assert.IsFalse(pc.Build.CanParry, "Topór nie paruje, a mag nie ma tarczy");

            float stamina = pc.Stamina.Current;
            pc.Request(ActionType.LightAttack);
            Assert.AreEqual(ActionType.LightAttack, pc.Actions.Current);
            Assert.AreEqual(stamina - axeDef.weapon.light.staminaCost, pc.Stamina.Current, 0.01f);
            Fixture.Advance(pc, axeDef.weapon.light.windup + 0.02f);
            Assert.AreEqual(ActionPhase.Active, pc.Actions.Phase);
            Assert.Greater(pc.Build.WeaponDamage(axeDef.weapon.light), 48f);
        }

        [Test]
        public void Knight_CanLearnSpell_AndCastingConsumesMana()
        {
            var run = fx.NewRun("class_knight");
            var pc = fx.NewCombatant(run);
            Assert.IsNull(pc.CurrentSpell, "Rycerz startuje bez czarów");

            var bolt = fx.Get<SpellDefinition>("spell_bolt");
            run.LearnSpell(bolt, fx.Cfg.balance.maxAttunedSpells);
            pc.RefreshBuild();
            Assert.IsNotNull(pc.CurrentSpell);
            Assert.IsTrue(pc.CurrentSpell.requirementsMet);

            float mana = pc.Mana.Current;
            pc.Request(ActionType.Cast);
            Assert.AreEqual(ActionType.Cast, pc.Actions.Current);
            Assert.AreEqual(mana - pc.CurrentSpell.manaCost, pc.Mana.Current, 0.01f, "Rzucenie zużywa manę");

            Fixture.Advance(pc, bolt.castTime + bolt.recovery + 0.1f);
            Assert.AreEqual(ActionType.None, pc.Actions.Current);

            pc.Mana.Drain(pc.Mana.Current);
            pc.Request(ActionType.Cast);
            Assert.AreNotEqual(ActionType.Cast, pc.Actions.Current, "Bez many czar nie zostaje rzucony");
        }

        [Test]
        public void ShieldBlock_ReducesDamage_AndConsumesStamina()
        {
            var pc = fx.NewCombatant(fx.NewRun("class_knight"));
            Assert.IsTrue(pc.Build.guardIsShield);
            pc.SetBlockHeld(true);
            Assert.AreEqual(ActionType.Block, pc.Actions.Current);

            float hp = pc.Health.Current, st = pc.Stamina.Current;
            var r = pc.ReceiveHit(Fixture.FrontHit(physical: 50, guardLoad: 20));
            Assert.AreEqual(HitOutcome.Blocked, r.outcome);
            Assert.AreEqual(hp, pc.Health.Current, 0.001f, "Tarcza herbowa zatrzymuje 100% zwykłych obrażeń fizycznych");
            Assert.AreEqual(st - 20f * pc.Build.guard.stabilityMultiplier, pc.Stamina.Current, 0.01f);

            var m = pc.ReceiveHit(Fixture.FrontHit(physical: 0, magic: 50, guardLoad: 20));
            Assert.AreEqual(HitOutcome.Blocked, m.outcome);
            Assert.Greater(m.healthDamage, 0f, "Obrażenia magiczne są redukowane tylko częściowo (osobny parametr)");
        }

        [Test]
        public void WeaponBlock_LetsPartOfDamageThrough()
        {
            var run = fx.NewRun("class_knight");
            run.UnequipToInventory(EquipSlot.OffHand);
            var pc = fx.NewCombatant(run);
            Assert.IsTrue(pc.Build.CanBlock);
            Assert.IsFalse(pc.Build.guardIsShield, "Bez tarczy blokuje miecz");

            pc.SetBlockHeld(true);
            float hp = pc.Health.Current, st = pc.Stamina.Current;
            var unblocked = DamageResolver.Resolve(Fixture.FrontHit(50), new DefenseState { forward = UnityEngine.Vector3.forward, physicalResist = pc.Build.PhysicalReduction(fx.Cfg.balance) }, fx.Cfg.balance);
            var r = pc.ReceiveHit(Fixture.FrontHit(physical: 50, guardLoad: 20));
            Assert.AreEqual(HitOutcome.Blocked, r.outcome);
            Assert.Greater(r.healthDamage, 0f, "Część obrażeń przechodzi");
            Assert.Less(r.healthDamage, unblocked.healthDamage, "…ale mniej niż bez bloku");
            Assert.AreEqual(hp - r.healthDamage, pc.Health.Current, 0.01f);
            Assert.Less(pc.Stamina.Current, st);
        }

        [Test]
        public void Parry_InActiveWindow_StopsParryableAttack()
        {
            var pc = fx.NewCombatant(fx.NewRun("class_knight"));
            pc.Request(ActionType.Parry);
            Fixture.Advance(pc, pc.Build.parry.startup + 0.02f);
            Assert.AreEqual(ActionPhase.Active, pc.Actions.Phase);

            float hp = pc.Health.Current;
            var r = pc.ReceiveHit(Fixture.FrontHit(80));
            Assert.AreEqual(HitOutcome.Parried, r.outcome);
            Assert.AreEqual(hp, pc.Health.Current);
            Assert.AreEqual(pc.Build.parry.poiseDamage, r.attackerPoiseDamage);

            // Atak oznaczony jako niemożliwy do sparowania trafia mimo aktywnego okna.
            var pc2 = fx.NewCombatant(fx.NewRun("class_knight"));
            pc2.Request(ActionType.Parry);
            Fixture.Advance(pc2, pc2.Build.parry.startup + 0.02f);
            Assert.AreEqual(HitOutcome.Hit, pc2.ReceiveHit(Fixture.FrontHit(80, parryable: false)).outcome);
        }

        [Test]
        public void Parry_MistimedLeavesPlayerExposed_AndDoesNotTurnIntoBlock()
        {
            var pc = fx.NewCombatant(fx.NewRun("class_knight"));
            pc.Request(ActionType.Parry);
            var p = pc.Build.parry;
            Fixture.Advance(pc, p.startup + p.activeWindow + 0.05f);
            Assert.AreEqual(ActionPhase.Recovery, pc.Actions.Phase);

            pc.SetBlockHeld(true);
            Assert.AreEqual(ActionType.Parry, pc.Actions.Current, "Parowanie nie przechodzi w bezpieczny blok");
            Assert.IsFalse(pc.Actions.CanStart(ActionType.Dodge), "W fazie zakończenia nie można uciec unikiem");

            float hp = pc.Health.Current;
            var r = pc.ReceiveHit(Fixture.FrontHit(80));
            Assert.AreEqual(HitOutcome.Hit, r.outcome);
            Assert.Less(pc.Health.Current, hp);
            Assert.AreEqual(ActionType.Flinch, pc.Actions.Current);
        }

        [Test]
        public void Dodge_ProtectsOnlyDuringInvulnerabilityWindow()
        {
            // Początek uniku (przed oknem) – trafienie przechodzi.
            var early = fx.NewCombatant(fx.NewRun("class_knight"));
            early.Request(ActionType.Dodge);
            Assert.AreEqual(ActionPhase.Startup, early.Actions.Phase);
            Assert.AreEqual(HitOutcome.Hit, early.ReceiveHit(Fixture.FrontHit()).outcome);

            // W oknie – unik.
            var pc = fx.NewCombatant(fx.NewRun("class_knight"));
            pc.Request(ActionType.Dodge);
            var d = pc.Build.dodge;
            Fixture.Advance(pc, d.invulnStart + 0.05f);
            Assert.AreEqual(ActionPhase.Active, pc.Actions.Phase);
            float hp = pc.Health.Current;
            Assert.AreEqual(HitOutcome.Dodged, pc.ReceiveHit(Fixture.FrontHit()).outcome);
            Assert.AreEqual(hp, pc.Health.Current);
            Assert.AreEqual(HitOutcome.Hit, pc.ReceiveHit(Fixture.FrontHit(10, dodgeable: false)).outcome, "Atak obszarowy bez 'dodgeable' trafia mimo niewrażliwości");

            // Po oknie (faza zakończenia) – trafienie przechodzi, a kolejny unik jest zablokowany do końca animacji.
            var late = fx.NewCombatant(fx.NewRun("class_knight"));
            late.Request(ActionType.Dodge);
            Fixture.Advance(late, d.invulnStart + d.invulnDuration + 0.05f);
            Assert.AreEqual(ActionPhase.Recovery, late.Actions.Phase);
            Assert.IsFalse(late.Actions.CanStart(ActionType.Dodge));
            Assert.AreEqual(HitOutcome.Hit, late.ReceiveHit(Fixture.FrontHit()).outcome);
        }

        [Test]
        public void StaminaExhaustion_WhileBlocking_BreaksGuard()
        {
            var pc = fx.NewCombatant(fx.NewRun("class_knight"));
            pc.SetBlockHeld(true);
            pc.Stamina.Drain(pc.Stamina.Current - 5f);
            var r = pc.ReceiveHit(Fixture.FrontHit(physical: 60, guardLoad: 40));
            Assert.AreEqual(HitOutcome.GuardBroken, r.outcome);
            Assert.AreEqual(0f, pc.Stamina.Current, 0.001f);
            Assert.AreEqual(ActionType.GuardBroken, pc.Actions.Current);
            Assert.Greater(r.healthDamage, 0f, "Reguła: przełamanie zadaje max(po bloku, bez bloku × współczynnik)");
        }

        [Test]
        public void Flasks_RestoreHealthAndMana_AndDrinkingIsInterruptible()
        {
            var run = fx.NewRun("class_mage");
            var pc = fx.NewCombatant(run);
            pc.Health.Drain(200);
            float hp = pc.Health.Current;
            int charges = run.healthFlasks;
            pc.RequestFlask(false);
            Assert.AreEqual(ActionType.Flask, pc.Actions.Current);
            Fixture.Advance(pc, fx.Cfg.balance.flaskDrinkTime + fx.Cfg.balance.flaskEffectDuration + 0.2f);
            Assert.AreEqual(charges - 1, run.healthFlasks);
            Assert.AreEqual(hp + pc.Health.Max * fx.Cfg.balance.healthFlaskFraction, pc.Health.Current, 1f);

            pc.Mana.Drain(pc.Mana.Current);
            int manaCharges = run.manaFlasks;
            pc.RequestFlask(true);
            Fixture.Advance(pc, fx.Cfg.balance.flaskDrinkTime + fx.Cfg.balance.flaskEffectDuration + 0.2f);
            Assert.AreEqual(manaCharges - 1, run.manaFlasks);
            Assert.GreaterOrEqual(pc.Mana.Current, pc.Mana.Max * fx.Cfg.balance.manaFlaskFraction);

            // Trafienie w trakcie picia przerywa – ładunek nie zostaje zużyty, ale gracz był narażony.
            int before = run.healthFlasks;
            pc.RequestFlask(false);
            Fixture.Advance(pc, 0.2f);
            pc.ReceiveHit(Fixture.FrontHit(20));
            Fixture.Advance(pc, 1f);
            Assert.AreEqual(before, run.healthFlasks);
        }

        [Test]
        public void Regeneration_ComesFromEquipment()
        {
            var run = fx.NewRun("class_knight");
            var pc = fx.NewCombatant(run);
            pc.Health.Drain(100);
            float hp = pc.Health.Current;
            Fixture.Advance(pc, 5f);
            Assert.AreEqual(hp, pc.Health.Current, 0.01f, "Rycerz nie ma startowej regeneracji życia");

            run.AddItem(new ItemInstance(fx.Get<ItemDefinition>("ring_blood")), true);
            pc.RefreshBuild();
            Fixture.Advance(pc, 5f);
            Assert.AreEqual(hp + 5f * 1.2f, pc.Health.Current, 0.2f);

            var mage = fx.NewCombatant(fx.NewRun("class_mage"));
            mage.Mana.Drain(50);
            float mana = mage.Mana.Current;
            Fixture.Advance(mage, 10f);
            Assert.Greater(mage.Mana.Current, mana + 10f, "Kostur i szata regenerują manę");
        }

        [Test]
        public void ActionsAreExclusive_NoFlaskOrParryDuringAttack()
        {
            var run = fx.NewRun("class_knight");
            var pc = fx.NewCombatant(run);
            int flasks = run.healthFlasks;
            pc.Request(ActionType.LightAttack);
            pc.RequestFlask(false);
            pc.Request(ActionType.Parry);
            pc.Request(ActionType.Dodge);
            pc.SetBlockHeld(true);
            Assert.AreEqual(ActionType.LightAttack, pc.Actions.Current);
            Assert.AreEqual(flasks, run.healthFlasks);

            // Po punkcie przerwania w fazie regeneracji dozwolony jest unik.
            var atk = pc.Build.weapon.light;
            Fixture.Advance(pc, atk.windup + atk.active + atk.cancelAfter + 0.02f);
            Assert.IsTrue(pc.Actions.CanStart(ActionType.Dodge));
            Assert.IsFalse(pc.Actions.CanStart(ActionType.Flask));
        }

        [Test]
        public void ClassDoesNotGateActions()
        {
            // Rycerz z kosturem i czarami maga ma te same możliwości co mag.
            var run = fx.NewRun("class_knight");
            var staff = new ItemInstance(fx.Get<ItemDefinition>("weapon_staff"));
            run.inventory.Add(staff);
            run.EquipFromInventory(staff, EquipSlot.MainHand);
            run.UnequipToInventory(EquipSlot.OffHand);
            run.LearnSpell(fx.Get<SpellDefinition>("spell_nova"), 3);
            var pc = fx.NewCombatant(run);
            Assert.IsFalse(pc.Build.CanBlock, "Kostur nie blokuje – niezależnie od klasy");
            Assert.IsFalse(pc.Build.CanParry);
            Assert.AreEqual(1, pc.Build.spells.Count);
            Assert.Greater(pc.Build.stats[StatType.SpellPower], 0f, "Premia kostura działa u rycerza");
        }
    }
}
