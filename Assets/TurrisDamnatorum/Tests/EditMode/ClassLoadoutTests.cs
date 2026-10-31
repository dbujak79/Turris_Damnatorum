using System.Linq;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Start podejścia: dodatki i stałe umiejętności ogólne albo klasowe, broń startowa do wyboru, umiejętność obszarowa każdej klasy.</summary>
    public class ClassLoadoutTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        ClassDefinition Class(string id) => fx.Get<ClassDefinition>(id);
        UnlockDefinition U(string id) => fx.Get<UnlockDefinition>(id);
        SpellDefinition S(string id) => fx.Get<SpellDefinition>(id);

        [Test]
        public void BoughtSpell_IsForEveryClass_ButNeedsFocus_DefaultKitStaysWithClass()
        {
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            meta.DebugAddAsh(200);
            Assert.IsTrue(meta.Purchase(U("unlock_fireball")));
            Assert.IsTrue(meta.Purchase(U("unlock_heal")));

            var knight = fx.Plan("class_knight");
            meta.FillSkills(knight);
            var run = RunFactory.Create(knight, fx.Cfg, 1);
            var fireball = run.FindSpell(S("spell_fireball"));
            Assert.IsNotNull(fireball, "Kupiona kula ognia trafia do kolekcji każdej klasy (skille należą do broni, nie do klas)");
            Assert.Less(run.SlotOf(fireball), 0, "…ale rycerz z mieczem nie dostaje jej do slotu – czar wymaga fokusu");
            Assert.IsNull(run.FindSpell(S("spell_bolt")), "Pocisk arkanów to domyślny zestaw maga – zostaje przy klasie");
            Assert.IsNotNull(run.FindSpell(S("spell_heal")), "Leczenie jest ogólne");

            var mage = fx.Plan("class_mage");
            meta.FillSkills(mage);
            var mageRun = RunFactory.Create(mage, fx.Cfg, 2);
            Assert.IsNotNull(mageRun.FindSpell(S("spell_fireball")));
            Assert.IsNotNull(mageRun.FindSpell(S("spell_heal")));
        }

        [Test]
        public void LoadoutOptions_AreGeneralOrForSelectedClass()
        {
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            meta.DebugAddAsh(300);
            foreach (var id in new[] { "unlock_staff_storm", "unlock_ring_riposte", "unlock_bow", "unlock_dagger", "unlock_buckler" })
                if (!meta.IsUnlocked(U(id))) { meta.RecordReachedFloor(29); Assert.IsTrue(meta.Purchase(U(id)), id); }

            var knight = meta.LoadoutOptionsFor(Class("class_knight")).ToList();
            CollectionAssert.Contains(knight, U("unlock_axe"));
            CollectionAssert.Contains(knight, U("unlock_buckler"));
            CollectionAssert.Contains(knight, U("unlock_ring_riposte"), "Pierścień – ogólny");
            CollectionAssert.Contains(knight, U("unlock_vigilance"), "Talent ogólny");
            CollectionAssert.Contains(knight, U("unlock_staff_storm"), "Kupiona broń – dla każdej klasy (wymagania ma broń)");
            CollectionAssert.Contains(knight, U("unlock_dagger"));
            CollectionAssert.DoesNotContain(knight, U("unlock_ashrobe"), "Szata maga – dodatek klasowy");

            var rogue = meta.LoadoutOptionsFor(Class("class_rogue")).ToList();
            CollectionAssert.Contains(rogue, U("unlock_dagger"));
            CollectionAssert.DoesNotContain(rogue, U("unlock_axe"), "Topór to domyślny zestaw rycerza");

            // Specjalizacja dostaje dodatki swojego typu bohatera (paladyn – jak wojownik).
            var paladin = meta.LoadoutOptionsFor(Class("class_hu_paladin")).ToList();
            CollectionAssert.Contains(paladin, U("unlock_axe"));
            CollectionAssert.DoesNotContain(paladin, U("unlock_ashrobe"));

            // Każde odblokowanie (poza trudnością) jest albo ogólne, albo dostępne dla co najmniej jednej klasy.
            foreach (var u in fx.Cfg.unlocks.Where(x => x.kind != UnlockKind.Difficulty))
                Assert.IsTrue(u.IsGeneral || u.classes.All(c => fx.Cfg.classes.Contains(c)), u.id);
        }

        [Test]
        public void ChangingClass_DropsForeignExtras_AndValidationRejectsThem()
        {
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            var plan = fx.Plan("class_knight");
            plan.extras.Add(U("unlock_axe"));
            plan.extras.Add(U("unlock_vigilance"));
            Assert.IsTrue(meta.ValidateLoadout(plan, out var r), r);

            meta.SelectClass(plan, Class("class_mage"));
            CollectionAssert.DoesNotContain(plan.extras, U("unlock_axe"), "Topór rycerza wypada po zmianie klasy");
            CollectionAssert.Contains(plan.extras, U("unlock_vigilance"), "Ogólny talent zostaje");

            plan.extras.Add(U("unlock_axe"));
            Assert.IsFalse(meta.ValidateLoadout(plan, out var reason));
            StringAssert.Contains("niedostępne dla klasy", reason);
        }

        [Test]
        public void Rogue_CanStartWithBowOrDaggers_ChoiceIsRemembered()
        {
            var storage = new MemoryProfileStorage();
            var meta = new MetaService(fx.Cfg, storage);
            var rogue = Class("class_rogue");
            Assert.AreEqual(2, rogue.loadouts.Count, "Łuk albo sztylety");
            var bow = fx.Get<ItemDefinition>("hu_bow");
            var dagger = fx.Get<ItemDefinition>("hu_dagger");
            var offDagger = fx.Get<ItemDefinition>("hu_offdagger");

            var plan = fx.Plan("class_rogue");
            meta.SelectClass(plan, rogue);
            Assert.AreEqual(bow, plan.Weapon, "Domyślnie łuk");
            Assert.IsNull(plan.GearSkill, "Łuk nie daje skilla – wybór z dwóch");
            Assert.AreEqual(Class("class_rogue").startingSkillChoices[0], plan.OpeningSkill);

            meta.SelectLoadout(plan, 1);
            Assert.IsTrue(meta.ValidateLoadout(plan, out var r), r);
            Assert.AreEqual(0, plan.Cost, "Broń startowa nie kosztuje budżetu");
            Assert.AreEqual(fx.Get<SpellDefinition>("hu_knife_throw"), plan.OpeningSkill, "Sztylet do rzucania daje Rzut nożami");
            var run = RunFactory.Create(plan, fx.Cfg, 3);
            Assert.AreEqual(dagger, run.equipment.Get(EquipSlot.MainHand).definition, "Sztylet w dłoni");
            Assert.AreEqual(offDagger, run.equipment.Get(EquipSlot.OffHand).definition, "Nóż do rzucania w lewej");
            Assert.IsNotNull(run.FindSpell(fx.Get<SpellDefinition>("hu_knife_throw")));
            Assert.IsFalse(run.inventory.Any(i => i.definition == bow), "Łuk nie trafia do plecaka");

            meta.RecordRunStart(plan);
            var again = new MetaService(fx.Cfg, storage).LastPlan();
            Assert.AreEqual(dagger, again.Weapon, "Wybór zestawu zapamiętany");

            // Zdjęcie noża do rzucania zabiera skill, który dawał.
            run.UnequipToInventory(EquipSlot.OffHand);
            run.SyncGrantedSkills();
            Assert.IsNull(run.FindSpell(fx.Get<SpellDefinition>("hu_knife_throw")), "Skill z przedmiotu znika razem z nim");
        }

        [Test]
        public void ExtraWeaponAndShield_AreEquippedAtStart_ClassItemsGoToBackpack()
        {
            var knight = Class("class_knight");
            var plan = fx.Plan("class_knight");
            plan.extras.Add(U("unlock_axe"));
            plan.extras.Add(U("unlock_buckler"));
            var run = RunFactory.Create(plan, fx.Cfg, 7);
            Assert.AreEqual(U("unlock_axe").target, run.equipment.Get(EquipSlot.MainHand).definition, "Wybrany topór w dłoni");
            Assert.AreEqual(U("unlock_buckler").target, run.equipment.Get(EquipSlot.OffHand).definition, "Wybrany puklerz na ręce");
            Assert.IsTrue(run.inventory.Any(i => i.definition == plan.Weapon), "Miecz klasy w plecaku");

            // Broń dwuręczna jako dodatek wypycha też tarczę.
            var plan2 = fx.Plan("class_knight");
            plan2.extras.Add(U("unlock_greataxe"));
            var run2 = RunFactory.Create(plan2, fx.Cfg, 8);
            Assert.AreEqual(U("unlock_greataxe").target, run2.equipment.Get(EquipSlot.MainHand).definition);
            Assert.IsNull(run2.equipment.Get(EquipSlot.OffHand), "Dwuręczna – druga ręka wolna");
        }

        [Test]
        public void EveryClass_HasFeature_OpeningSkill_AndNoOldSignature()
        {
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            Assert.AreEqual(3, fx.Cfg.classes.Count(c => c.parent == null), "Trzy typy bohatera");
            Assert.AreEqual(12, fx.Cfg.classes.Count(c => c.parent != null), "Dwanaście specjalizacji");
            foreach (var c in fx.Cfg.classes)
            {
                var plan = meta.LastPlan();
                meta.SelectClass(plan, c);
                var run = RunFactory.Create(plan, fx.Cfg, 5);
                Assert.IsNotNull(run.ClassFeature, c.id + ": cecha klasowa");
                Assert.AreEqual(ClassFeatureCatalog.For(c)[0].id, run.classFeatureId, c.id + ": domyślnie wariant A");
                Assert.IsNull(run.signatureSkill, c.id + ": bez dawnej sygnatury obok cechy");
                Assert.IsTrue(c.loadouts.Count > 0, c.id + ": zestaw broni na start");
                var opening = plan.OpeningSkill;
                Assert.IsNotNull(opening, c.id + ": skill startowy");
                Assert.GreaterOrEqual(run.SlotOf(run.FindSpell(opening)), 0, c.id + ": skill startowy w slocie");

            }
        }
    }
}
