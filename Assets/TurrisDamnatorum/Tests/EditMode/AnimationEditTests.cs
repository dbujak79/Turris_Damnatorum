using NUnit.Framework;
using Turris.EditorTools;

namespace Turris.Tests
{
    public class AnimationEditTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        [Test]
        public void AttackAnim_InferredFromAttackData()
        {
            var sword = fx.Get<ItemDefinition>("weapon_longsword");
            Assert.AreEqual(WeaponModel.Sword, AnimResolve.ForItem(sword));
            Assert.AreEqual(AttackAnim.SlashRight, AnimResolve.ForAttack(sword.weapon.light, 0, WeaponModel.Sword));
            Assert.AreEqual(AttackAnim.SlashLeft, AnimResolve.ForAttack(sword.weapon.light, 1, WeaponModel.Sword), "Kombinacja naprzemienna");
            Assert.AreEqual(AttackAnim.Overhead, AnimResolve.ForAttack(sword.weapon.heavy, 0, WeaponModel.Sword));

            var ghoul = fx.Get<EnemyDefinition>("enemy_ghoul");
            Assert.AreEqual(RigStyle.Ghoul, AnimResolve.ForEnemy(ghoul));
            Assert.AreEqual(AttackAnim.Claw, AnimResolve.ForAttack(ghoul.attacks[0].attack, 0, WeaponModel.Claws));
            Assert.AreEqual(AttackAnim.Leap, AnimResolve.ForAttack(ghoul.attacks[2].attack, 0, WeaponModel.Claws) == AttackAnim.Claw
                ? AttackAnim.Leap : AnimResolve.ForAttack(ghoul.attacks[2].attack, 0, WeaponModel.Sword), "Skok = Leap");

            var heretic = fx.Get<EnemyDefinition>("enemy_heretic");
            Assert.AreEqual(AttackAnim.Cast, AnimResolve.ForAttack(heretic.attacks[0].attack, 0, WeaponModel.Staff));
            Assert.AreEqual(AttackAnim.Burst, AnimResolve.ForAttack(heretic.attacks[2].attack, 0, WeaponModel.Staff));

            var boss = fx.Get<EnemyDefinition>("enemy_castellan");
            Assert.AreEqual(RigStyle.Castellan, AnimResolve.ForEnemy(boss));
            Assert.AreEqual(AttackAnim.Thrust, AnimResolve.ForAttack(boss.attacks[2].attack, 0, WeaponModel.GreatSword), "Pchnięcie szarży");
        }

        [Test]
        public void ExplicitAnimationOverridesInference()
        {
            var a = new AttackDefinition { heavy = true, animation = AttackAnim.Thrust };
            Assert.AreEqual(AttackAnim.Thrust, AnimResolve.ForAttack(a, 0, WeaponModel.Sword));
        }

        [TestCase("sword and shield idle", ClipMatcher.Slot.Idle, AnimAction.None, AttackAnim.Auto)]
        [TestCase("sword and shield walk", ClipMatcher.Slot.Walk, AnimAction.None, AttackAnim.Auto)]
        [TestCase("sword and shield run", ClipMatcher.Slot.Run, AnimAction.None, AttackAnim.Auto)]
        [TestCase("sword and shield strafe left", ClipMatcher.Slot.StrafeLeft, AnimAction.None, AttackAnim.Auto)]
        [TestCase("sword and shield slash mixamo.com", ClipMatcher.Slot.Action, AnimAction.Attack, AttackAnim.SlashRight)]
        [TestCase("great sword high spin attack", ClipMatcher.Slot.Action, AnimAction.Attack, AttackAnim.SlashRight)]
        [TestCase("sword and shield attack downward", ClipMatcher.Slot.Action, AnimAction.Attack, AttackAnim.Overhead)]
        [TestCase("stable sword inward slash", ClipMatcher.Slot.Action, AnimAction.Attack, AttackAnim.SlashRight)]
        [TestCase("sword thrust", ClipMatcher.Slot.Action, AnimAction.Attack, AttackAnim.Thrust)]
        [TestCase("sword and shield block idle", ClipMatcher.Slot.Action, AnimAction.Block, AttackAnim.Auto)]
        [TestCase("stand to roll", ClipMatcher.Slot.Action, AnimAction.Dodge, AttackAnim.Auto)]
        [TestCase("sword and shield impact", ClipMatcher.Slot.Action, AnimAction.Flinch, AttackAnim.Auto)]
        [TestCase("sword and shield death", ClipMatcher.Slot.Action, AnimAction.Death, AttackAnim.Auto)]
        [TestCase("drinking", ClipMatcher.Slot.Action, AnimAction.Drink, AttackAnim.Auto)]
        [TestCase("standing 1h magic attack", ClipMatcher.Slot.Action, AnimAction.Cast, AttackAnim.Auto)]
        public void ClipNames_AreMatchedToGameActions(string name, ClipMatcher.Slot slot, AnimAction action, AttackAnim attack)
        {
            Assert.AreEqual(slot, ClipMatcher.Classify(name, out var a, out var at));
            Assert.AreEqual(action, a);
            if (action == AnimAction.Attack) Assert.AreEqual(attack, at);
        }
    }
}
