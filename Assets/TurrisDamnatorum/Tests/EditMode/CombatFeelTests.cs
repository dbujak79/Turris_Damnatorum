using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>Odczucie ciosu: ciężar trafienia, wibracje pada, płynne łączenie ciosów w serię.</summary>
    public class CombatFeelTests
    {
        [TearDown] public void TearDown() { Rumble.Stop(); Rumble.Strength = 1f; }

        [Test]
        public void Weight_GrowsWithPoiseHeavyAndRiposte()
        {
            float light = ImpactDirector.Weight(new HitData { physical = 20, poiseDamage = 15 });
            float heavy = ImpactDirector.Weight(new HitData { physical = 40, poiseDamage = 40, heavy = true });
            float riposte = ImpactDirector.Weight(new HitData { physical = 90, isRiposte = true, heavy = true });
            float zone = ImpactDirector.Weight(new HitData { magic = 5 });
            Assert.Less(light, heavy);
            Assert.Less(heavy, riposte);
            Assert.Less(zone, light, "Strefa/czar bez postawy – najsłabsza reakcja");
            Assert.That(light, Is.InRange(0.3f, 0.5f));
        }

        [Test]
        public void Rumble_DecaysLinearly_StopsInPause_AndRespectsStrength()
        {
            Rumble.Stop();
            Rumble.Strength = 1f;
            Rumble.Play(1f, 0.5f, 0.2f);
            Rumble.Tick(0.1f, false);
            Assert.AreEqual(0.5f, Rumble.CurrentLow, 0.02f, "Połowa czasu – połowa siły");
            Assert.AreEqual(0.25f, Rumble.CurrentHigh, 0.02f);
            Rumble.Tick(0.01f, true);
            Assert.AreEqual(0f, Rumble.CurrentLow, "Pauza – silniki stoją");
            Rumble.Tick(0.2f, false);
            Assert.AreEqual(0f, Rumble.CurrentLow, "Impuls wygasł");

            Rumble.Strength = 0.5f;
            Rumble.Play(1f, 1f, 1f);
            Rumble.Tick(0.001f, false);
            Assert.AreEqual(0.5f, Rumble.CurrentLow, 0.01f, "Siła z ustawień");
            Rumble.Strength = 0f;
            Rumble.Stop();
            Rumble.Play(1f, 1f, 1f);
            Rumble.Tick(0.01f, false);
            Assert.AreEqual(0f, Rumble.CurrentLow, "0% = wyłączone");
        }

        [Test]
        public void ComboSwing_StartsFromPreviousPose_AndOvershootsFollowThrough()
        {
            var look = new RigLook { weapon = WeaponModel.Sword, shield = ShieldModel.Heater };
            var ready = PoseLibrary.Ready(look);
            var a = PoseLibrary.Attack(AttackAnim.SlashRight, look);
            var b = PoseLibrary.Attack(AttackAnim.SlashLeft, look);
            // Drugi cios serii zaczyna się tam, gdzie skończył pierwszy (wybrzmienie), a nie w gardzie.
            var start = PoseLibrary.SampleAttack(b, ready, a.follow, ActionPhase.Startup, 0f);
            Assert.Less(Vector3.Distance(start.handR, a.follow.handR), 1e-4f);
            var peak = PoseLibrary.SampleAttack(b, ready, a.follow, ActionPhase.Startup, 0.75f);
            Assert.Less(Vector3.Distance(peak.handR, b.wind.handR), 1e-3f, "Szczyt zamachu – poza zamachu");
            // Wybrzmienie: w połowie drugiej części fazy aktywnej dłoń jest dalej niż poza końcowa (przestrzał).
            Vector3 hitToFollow = a.follow.handR - a.hit.handR;
            var over = PoseLibrary.SampleAttack(a, ready, ActionPhase.Active, 0.85f);
            float along = Vector3.Dot(over.handR - a.hit.handR, hitToFollow.normalized);
            Assert.Greater(along, hitToFollow.magnitude, "Przestrzał za pozą wybrzmienia");
            var end = PoseLibrary.SampleAttack(a, ready, ActionPhase.Active, 1f);
            Assert.Less(Vector3.Distance(end.handR, a.follow.handR), 1e-3f, "Koniec fazy – dokładnie wybrzmienie");
        }
    }
}
