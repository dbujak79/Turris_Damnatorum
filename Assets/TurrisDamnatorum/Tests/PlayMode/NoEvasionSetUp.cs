using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Unik demonów (HU) jest losowy – w testach trafienia mają lądować deterministycznie. Serie botów włączają go z powrotem.</summary>
    [SetUpFixture]
    public class NoEvasionSetUp
    {
        [OneTimeSetUp] public void Off() => EnemyBrain.DebugNoEvasion = true;
        [OneTimeTearDown] public void On() => EnemyBrain.DebugNoEvasion = false;
    }
}
