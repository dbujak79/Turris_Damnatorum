using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Reżyser ataków (Hell Unleashed): limit demonów naraz w zamachu, osobno wręcz, z dystansu i specjalnych.</summary>
    public class AttackDirectorTests
    {
        [TearDown]
        public void TearDown() { AttackDirector.Enabled = true; AttackDirector.Reset(1, false); }

        [Test]
        public void Limits_GrowWithActs_BossFloorKeepsServantsLow()
        {
            AttackDirector.Reset(1, false);
            Assert.AreEqual(3, AttackDirector.Limit(AggressionKind.Melee));
            Assert.AreEqual(1, AttackDirector.Limit(AggressionKind.Ranged));
            AttackDirector.Reset(11, false);
            Assert.AreEqual(4, AttackDirector.Limit(AggressionKind.Melee));
            Assert.AreEqual(2, AttackDirector.Limit(AggressionKind.Ranged));
            AttackDirector.Reset(30, false);
            Assert.AreEqual(5, AttackDirector.Limit(AggressionKind.Melee));
            AttackDirector.Reset(9, true);
            Assert.AreEqual(2, AttackDirector.Limit(AggressionKind.Melee), "sługi obok bossa");
            Assert.AreEqual(1, AttackDirector.Limit(AggressionKind.Special));
        }

        [Test]
        public void Acquire_RespectsLimit_ReleaseFreesSlot()
        {
            AttackDirector.Reset(1, false);
            for (int i = 0; i < 3; i++) Assert.IsTrue(AttackDirector.TryAcquire(AggressionKind.Melee));
            Assert.IsFalse(AttackDirector.TryAcquire(AggressionKind.Melee), "czwarty cios wręcz czeka");
            Assert.IsTrue(AttackDirector.TryAcquire(AggressionKind.Ranged), "dystans liczony osobno");
            AttackDirector.Release(AggressionKind.Melee);
            Assert.IsTrue(AttackDirector.TryAcquire(AggressionKind.Melee));
            AttackDirector.Reset(2, false);
            Assert.AreEqual(0, AttackDirector.Active(AggressionKind.Melee), "nowe piętro – liczniki od zera");
        }
    }
}
