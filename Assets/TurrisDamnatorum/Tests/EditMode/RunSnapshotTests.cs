using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>Zapis stanu podejścia (ponowna próba piętra przez bota, wyzwanie dla gracza) odtwarza ten sam build.</summary>
    public class RunSnapshotTests
    {
        Fixture fx;

        [SetUp]
        public void SetUp() => fx = new Fixture();

        [Test]
        public void Capture_Restore_KeepsBuildAndFloor()
        {
            var run = fx.NewRun("class_knight");
            run.floorIndex = 6; run.floorsCleared = 6; run.souls = 321; run.attributePoints = 2;
            run.attributes.Add(AttributeType.Strength);
            var rend = run.LearnSpell(fx.Get<SpellDefinition>("skill_rend"), level: 2);
            run.AssignSlot(rend, 0);
            run.AddBoon(fx.Cfg.baseBoonPool.First(b => b != null));
            var ring = fx.Cfg.baseItemPool.First(i => i != null && i.kind == ItemKind.Ring);
            run.inventory.Add(new ItemInstance(ring, 1));

            var json = JsonUtility.ToJson(RunSnapshot.Capture(run, "test"));
            var back = JsonUtility.FromJson<RunSnapshot>(json).Restore(fx.Cfg);

            Assert.IsNotNull(back);
            Assert.AreEqual(run.startingClass, back.startingClass);
            Assert.AreEqual(6, back.floorIndex);
            Assert.AreEqual(321, back.souls);
            Assert.AreEqual(run.attributes.strength, back.attributes.strength);
            Assert.AreEqual(2, back.FindSpell(rend.definition).level);
            Assert.AreEqual(0, back.SlotOf(back.FindSpell(rend.definition)), "slot jak w zapisie");
            Assert.AreEqual(run.equipment.Get(EquipSlot.MainHand).definition, back.equipment.Get(EquipSlot.MainHand).definition);
            Assert.IsTrue(back.inventory.Any(i => i.definition == ring && i.level == 1));
            Assert.AreEqual(run.boons.Count, back.boons.Count);
            Assert.AreEqual(run.classFeatureId, back.classFeatureId, "cecha klasowa w zapisie");
            Assert.IsNull(back.signatureSkill);
        }
    }
}
