using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>
    /// Zestaw ruchów topora i młota dwuręcznego: seria rąbnięć, ładowanie przytrzymanym ciężkim, ciosy z biegu i po uniku.
    /// Inne bronie zostają przy dotychczasowych ciosach.
    /// </summary>
    public class GreatWeaponMovesetEditTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        PlayerCombat Hero(string classId, int loadout = 0)
        {
            var plan = fx.Plan(classId);
            plan.loadoutIndex = loadout;
            return fx.NewCombatant(RunFactory.Create(plan, fx.Cfg, 11));
        }

        static float UntilPhase(PlayerCombat pc, ActionPhase phase, float max = 4f)
        {
            float t = 0f;
            while (pc.Actions.Phase != phase && t < max) { pc.Tick(1f / 60f); t += 1f / 60f; }
            return t;
        }

        [Test]
        public void Axe_AndHammer_UseTheMoveset_SwordDoesNot()
        {
            Assert.IsTrue(Hero("class_hu_berserker", 0).GreatWeaponMoveset, "topór dwuręczny");
            Assert.IsTrue(Hero("class_hu_berserker", 1).GreatWeaponMoveset, "młot bojowy");
            var knight = Hero("class_knight");
            Assert.IsFalse(knight.GreatWeaponMoveset, "miecz");
            knight.Sprinting = true;
            knight.Request(ActionType.LightAttack);
            Assert.AreEqual(GreatMove.Normal, knight.CurrentMove, "miecz bez ciosu z biegu");
            Assert.AreEqual(0f, knight.AttackCarry);
        }

        [Test]
        public void Heavy_ChopsFromAbove_ThenChainsIntoSecondChop()
        {
            var pc = Hero("class_hu_berserker");
            pc.Request(ActionType.HeavyAttack);
            Assert.AreEqual(ActionType.HeavyAttack, pc.Actions.Current);
            Assert.AreEqual(AttackAnim.Chop, pc.GetAnimState().attack);
            UntilPhase(pc, ActionPhase.Recovery);
            while (!pc.Actions.PastCancelPoint) pc.Tick(1f / 60f);
            pc.Request(ActionType.HeavyAttack);
            Assert.AreEqual(1, pc.Actions.ComboIndex, "drugi ciężki w serii");
            Assert.AreEqual(AttackAnim.ChopFollow, pc.GetAnimState().attack, "drugie rąbnięcie zza pleców");
        }

        [Test]
        public void HeldHeavy_StaysAboveHead_ThenHitsHarder()
        {
            var quick = Hero("class_hu_berserker");
            quick.Request(ActionType.HeavyAttack);
            float normal = UntilPhase(quick, ActionPhase.Active);
            Assert.AreEqual(GreatMove.Normal, quick.CurrentMove, "puszczony od razu – zwykły ciężki");

            var pc = Hero("class_hu_berserker");
            pc.HeavyHeld = true;
            pc.Request(ActionType.HeavyAttack);
            float charged = UntilPhase(pc, ActionPhase.Active);
            Assert.AreEqual(GreatMove.Charged, pc.CurrentMove);
            Assert.AreEqual(PlayerCombat.ChargeMax, pc.ChargeTime, 0.02f, "pełne ładowanie");
            Assert.AreEqual(normal + PlayerCombat.ChargeMax, charged, 0.05f, "zamach czeka o czas ładowania");
            Assert.AreEqual(AttackAnim.Chop, pc.GetAnimState().attack);
            pc.HeavyHeld = false;
            Fixture.Advance(pc, 2f); // fala uderzeniowa na początku powrotu – bez wyjątków
            Assert.AreEqual(ActionType.None, pc.Actions.Current, "cios naładowany się kończy");

            // Puszczenie w trakcie: ładowanie przerwane, cios leci od razu.
            var released = Hero("class_hu_berserker");
            released.HeavyHeld = true;
            released.Request(ActionType.HeavyAttack);
            float held = 0f;
            while (released.ChargeTime < 0.3f && held < 3f) { released.Tick(1f / 60f); held += 1f / 60f; }
            released.HeavyHeld = false;
            float rest = UntilPhase(released, ActionPhase.Active);
            Assert.AreEqual(normal + 0.3f, held + rest, 0.06f, "ładowanie kończy się z puszczeniem przycisku");
            Assert.AreEqual(GreatMove.Charged, released.CurrentMove, "0,3 s ładowania to już cios naładowany");
        }

        [Test]
        public void SprintAttacks_SweepLow_AndLeap()
        {
            var pc = Hero("class_hu_berserker");
            pc.Sprinting = true;
            pc.Tick(1f / 60f);
            pc.Sprinting = false;
            pc.Request(ActionType.LightAttack);
            Assert.AreEqual(GreatMove.Running, pc.CurrentMove);
            Assert.AreEqual(AttackAnim.RunningSweep, pc.GetAnimState().attack);
            Assert.AreEqual(PlayerCombat.RunningLightCarry, pc.AttackCarry, 1e-4f, "rozpęd z biegu");

            var heavy = Hero("class_hu_berserker", 1);
            heavy.Sprinting = true;
            heavy.Request(ActionType.HeavyAttack);
            Assert.AreEqual(AttackAnim.LeapChop, heavy.GetAnimState().attack, "ciężki z biegu – skok");
            heavy.HeavyHeld = true;
            UntilPhase(heavy, ActionPhase.Active);
            Assert.AreEqual(0f, heavy.ChargeTime, "skoku z biegu się nie ładuje");
            Fixture.Advance(heavy, 2f); // fala po skoku – bez wyjątków
            Assert.AreEqual(ActionType.None, heavy.Actions.Current);

            // Po chwili od końca biegu – zwykły cios.
            var late = Hero("class_hu_berserker");
            late.Sprinting = true; late.Tick(1f / 60f); late.Sprinting = false;
            Fixture.Advance(late, PlayerCombat.RunAttackWindow + 0.1f);
            late.Request(ActionType.LightAttack);
            Assert.AreEqual(GreatMove.Normal, late.CurrentMove);
            Assert.AreEqual(AttackAnim.Cleave, late.GetAnimState().attack, "szerokie cięcie topora");
        }

        [Test]
        public void LightRightAfterDodge_IsQuickChop()
        {
            var pc = Hero("class_hu_berserker");
            pc.Request(ActionType.Dodge);
            Assert.AreEqual(ActionType.Dodge, pc.Actions.Current);
            while (pc.Actions.Current == ActionType.Dodge) pc.Tick(1f / 60f);
            pc.Request(ActionType.LightAttack);
            Assert.AreEqual(GreatMove.AfterDodge, pc.CurrentMove);
            Assert.AreEqual(AttackAnim.DodgeChop, pc.GetAnimState().attack);
            var plain = Hero("class_hu_berserker");
            plain.Request(ActionType.LightAttack);
            Assert.AreEqual(plain.AttackWindup * PlayerCombat.DodgeChopWindup, pc.AttackWindup, 0.01f, "krótszy zamach");
        }
    }
}
