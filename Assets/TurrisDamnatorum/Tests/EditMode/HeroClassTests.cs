using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>
    /// Bohaterowie z Hell Unleashed: specjalizacje, skill klasy, cechy klas, nowe efekty (zachwianie, ogłuszenie, pęknięcie
    /// pancerza), żywioł ziemi, przeciążenie, nieśmiertelność, aury i walka dwiema brońmi.
    /// </summary>
    public class HeroClassTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        ClassDefinition C(string id) => fx.Get<ClassDefinition>(id);
        SpellDefinition S(string id) => fx.Get<SpellDefinition>(id);
        BalanceConfig B => fx.Cfg.balance;

        PlayerCombat Hero(string classId, int loadout = 0, SpellDefinition opening = null)
        {
            var plan = fx.Plan(classId);
            plan.loadoutIndex = loadout;
            plan.startingSkill = opening;
            return fx.NewCombatant(RunFactory.Create(plan, fx.Cfg, 11));
        }

        [Test]
        public void Specializations_InheritFamily_AndOwnTheirKit()
        {
            var paladin = C("class_hu_paladin");
            Assert.AreSame(C("class_knight"), paladin.Family);
            Assert.AreEqual("Wojownik", C("class_knight").displayName);
            Assert.AreEqual(ClassTrait.Devotion, paladin.trait);
            Assert.AreEqual("hu_judgement", paladin.signatureSkill.id);
            var pc = Hero("class_hu_paladin");
            Assert.AreEqual("hu_flail", pc.Run.equipment.Get(EquipSlot.MainHand).definition.id);
            Assert.AreEqual("hu_shield", pc.Run.equipment.Get(EquipSlot.OffHand).definition.id);
            Assert.IsNotNull(pc.Skill(PlayerCombat.SignatureSlot), "Skill klasy pod osobnym przyciskiem");
            Assert.IsTrue(pc.Run.startingClass.classModifiers.Count > 0, "Mistrzostwo klasy");
        }

        [Test]
        public void Signature_IsCastFromItsOwnButton_AndDoesNotUseASlot()
        {
            var pc = Hero("class_knight");
            Assert.IsFalse(pc.Run.skillSlots.Any(s => s != null && s.definition == S("hu_warcry")), "Okrzyk nie zajmuje slotu");
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.AreEqual(ActionType.Cast, pc.Actions.Current);
            Fixture.Advance(pc, 0.6f);
            Assert.IsFalse(pc.Run.signatureSkill.Ready, "Odnowienie");
        }

        [Test]
        public void Stagger_ThirdStackStuns_ThenImmunity()
        {
            var s = new StatusEffects();
            Assert.IsFalse(s.Apply(StatusKind.Stagger, 10, 0, B));
            Assert.IsFalse(s.Apply(StatusKind.Stagger, 10, 0, B));
            Assert.Less(s.MoveMultiplier(B), 1f, "Zachwianie spowalnia");
            Assert.IsTrue(s.Apply(StatusKind.Stagger, 10, 0, B), "Trzecia warstwa ogłusza");
            Assert.IsTrue(s.Stunned && s.Incapacitated);
            Assert.AreEqual(0f, s.MoveMultiplier(B));
            s.Tick(B.stunDuration + 0.05f, false, B);
            Assert.IsFalse(s.Stunned);
            Assert.IsFalse(s.Apply(StatusKind.Stun, 0, 0, B), "Odporność po ogłuszeniu");
        }

        [Test]
        public void Sunder_IncreasesPhysicalDamageTaken()
        {
            var s = new StatusEffects();
            var h0 = Fixture.FrontHit(100);
            s.ModifyIncoming(ref h0, B);
            for (int i = 0; i < 7; i++) s.Apply(StatusKind.Sunder, 0, 0, B);
            Assert.AreEqual(B.sunderMaxStacks, s.SunderStacks);
            var h = Fixture.FrontHit(100);
            s.ModifyIncoming(ref h, B);
            Assert.AreEqual(100f * (1f + B.sunderMaxStacks * B.sunderPerStack), h.physical, 0.01f);
        }

        [Test]
        public void Earth_OnBurning_EmberBurst_OnChilled_Mud()
        {
            var s = new StatusEffects();
            s.Apply(StatusKind.Burn, 0, 50, B);
            var hit = new HitData { magic = 40, element = Element.Earth };
            float instant = s.OnLanded(hit, new HitResult { outcome = HitOutcome.Hit, healthDamage = 40 }, B, out var reaction, out _);
            Assert.AreEqual("WYBUCH ŻARU", reaction);
            Assert.AreEqual(40 * B.emberBurstShare, instant, 0.01f);

            var t = new StatusEffects();
            t.Apply(StatusKind.Chill, 0, 10, B);
            t.OnLanded(hit, new HitResult { outcome = HitOutcome.Hit, healthDamage = 40 }, B, out var mud, out bool stunned);
            Assert.AreEqual("BŁOTO", mud);
            Assert.IsTrue(stunned && t.Stunned);
        }

        [Test]
        public void Fury_FillsFromHitsTaken_RageGivesAttackSpeed()
        {
            var pc = Hero("class_knight");
            float aspd = pc.Build.attackSpeed;
            for (int i = 0; i < 12 && !pc.Raging; i++)
            {
                pc.ReceiveHit(Fixture.FrontHit(5, blockable: false, parryable: false, dodgeable: false));
                Fixture.Advance(pc, 0.6f);
            }
            Assert.IsTrue(pc.Raging, "Trafienia ładują furię aż do szału");
            Assert.Greater(pc.Build.attackSpeed, aspd, "Szał przyspiesza ataki");
        }

        [Test]
        public void ArcaneWard_AbsorbsBeforeHealth()
        {
            var pc = Hero("class_mage");
            Assert.Greater(pc.WardMax, 0f);
            Assert.AreEqual(pc.WardMax, pc.Ward, 0.01f, "Osłona pełna na start");
            float hp = pc.Health.Current;
            pc.ReceiveHit(Fixture.FrontHit(10, blockable: false, parryable: false, dodgeable: false));
            Assert.AreEqual(hp, pc.Health.Current, 0.01f, "Mały cios pochłonięty przez osłonę");
            Assert.Less(pc.Ward, pc.WardMax);
        }

        [Test]
        public void Undying_PreventsDeath()
        {
            var pc = Hero("class_hu_berserker");
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Fixture.Advance(pc, 0.5f);
            Assert.Greater(pc.ImmortalTime, 0f);
            pc.ReceiveHit(Fixture.FrontHit(99999, blockable: false, parryable: false, dodgeable: false));
            Assert.IsFalse(pc.IsDead, "Nieśmiertelność: żaden cios nie zabije");
            Assert.GreaterOrEqual(pc.Health.Current, 1f);
        }

        [Test]
        public void Overload_EmpowersNextSlotSkill_ThenOverheats()
        {
            var pc = Hero("class_mage");
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Fixture.Advance(pc, 0.8f);
            Assert.IsTrue(pc.OverloadReady);
            int slot = pc.Run.SlotOf(pc.Run.FindSpell(S("hu_nova_fire")));
            Assert.GreaterOrEqual(slot, 0);
            pc.RequestSkill(slot);
            Fixture.Advance(pc, 1.5f);
            Assert.IsFalse(pc.OverloadReady, "Zużyte");
            Assert.Greater(pc.OverheatTime, 0f, "Sloty przegrzane");
            pc.Run.knownSpells.ForEach(s => s.ResetCooldown());
            pc.RequestSkill(slot);
            Assert.AreNotEqual(ActionType.Cast, pc.Actions.Current, "W czasie przegrzania sloty nie działają");
        }

        [Test]
        public void Aura_TogglesOnAndOff_DefianceRaisesDefense()
        {
            var pc = Hero("class_hu_paladin", 0, S("hu_aura_defiance"));
            int slot = pc.Run.SlotOf(pc.Run.FindSpell(S("hu_aura_defiance")));
            float def = pc.Build.stats[StatType.PhysicalDefense];
            pc.RequestSkill(slot);
            Assert.AreEqual(1, pc.Auras.Count);
            Assert.Greater(pc.Build.stats[StatType.PhysicalDefense], def, "Aura obronna podnosi pancerz");
            Fixture.Advance(pc, 1.2f);
            pc.RequestSkill(slot);
            Assert.AreEqual(0, pc.Auras.Count, "Ten sam przycisk gasi aurę");
            Assert.AreEqual(def, pc.Build.stats[StatType.PhysicalDefense], 0.01f);
        }

        [Test]
        public void DualWield_AlternatesHands_TwinBladesFaster()
        {
            var dual = Hero("class_hu_dual");
            Assert.IsNotNull(dual.Build.offWeapon, "Druga broń w lewej ręce");
            var knight = Hero("class_knight");
            Assert.Greater(dual.Build.attackSpeed, knight.Build.attackSpeed, "Dwie klingi: szybciej");
        }

        [Test]
        public void Grimoire_GrantsNova_WhileEquipped()
        {
            var pc = Hero("class_mage", 2);
            Assert.IsNotNull(pc.Run.FindSpell(S("hu_nova_air")), "Grymuar powietrza – Nova powietrza");
            pc.Run.UnequipToInventory(EquipSlot.OffHand);
            pc.RefreshBuild();
            Assert.IsNull(pc.Run.FindSpell(S("hu_nova_air")), "Po zdjęciu grymuaru nova znika");
        }
    }
}
