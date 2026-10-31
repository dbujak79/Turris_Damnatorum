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

        PlayerCombat Hero(string classId, int loadout = 0, SpellDefinition opening = null, bool legacySignature = false)
        {
            var plan = fx.Plan(classId);
            plan.loadoutIndex = loadout;
            plan.startingSkill = opening;
            var run = RunFactory.Create(plan, fx.Cfg, 11);
            if (legacySignature) Fixture.Legacy(run);
            return fx.NewCombatant(run);
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
            Assert.IsInstanceOf<AbsolutionFeature>(pc.Feature, "Cecha klasowa (wariant A) pod przyciskiem klasy");
            Assert.IsNull(pc.Run.signatureSkill, "Dawna sygnatura nie zostaje obok cechy");
            Assert.IsTrue(pc.Run.startingClass.classModifiers.Count > 0, "Mistrzostwo klasy");
        }

        [Test]
        public void ClassButton_RunsFeature_WithoutSlot()
        {
            var feat = Hero("class_knight");
            Assert.IsFalse(feat.Run.skillSlots.Any(s => s != null && s.definition == S("hu_warcry")), "Okrzyk nie zajmuje slotu");
            feat.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.IsTrue(((GuardFeature)feat.Feature).Active, "Przycisk klasy uruchamia cechę");
            Assert.AreNotEqual(ActionType.Cast, feat.Actions.Current, "Cecha nie jest zwykłym rzutem ze slotu");

            var pc = Hero("class_knight", legacySignature: true);
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
        public void BerserkerFury_ReducesDamage_AndRegenerates()
        {
            var calm = Hero("class_hu_berserker");
            var furious = Hero("class_hu_berserker");
            furious.AddFury(50f);
            float c00 = calm.Health.Current, f00 = furious.Health.Current;
            calm.ReceiveHit(Fixture.FrontHit(120, blockable: false, parryable: false, dodgeable: false));
            furious.ReceiveHit(Fixture.FrontHit(120, blockable: false, parryable: false, dodgeable: false));
            Assert.AreEqual((c00 - calm.Health.Current) * 0.9f, f00 - furious.Health.Current, 0.5f, "połowa furii – 10% mniej obrażeń, poza limitem");
            calm.AddFury(-calm.Fury);            // trafienie dokłada furii – spokojny z powrotem do zera,
            furious.AddFury(50f - furious.Fury); // wściekły do połowy
            float c0 = calm.Health.Current, f0 = furious.Health.Current;
            Fixture.Advance(calm, 1f);
            Fixture.Advance(furious, 1f);
            float extra = (furious.Health.Current - f0) - (calm.Health.Current - c0);
            Assert.AreEqual(PlayerCombat.BerserkerFuryRegen * 0.5f, extra, 1.5f, "połowa furii – ok. 5 życia na sekundę");

            var knight = Hero("class_knight");
            var knightFurious = Hero("class_knight");
            knightFurious.AddFury(50f);
            float k0 = knight.Health.Current, kf0 = knightFurious.Health.Current;
            knight.ReceiveHit(Fixture.FrontHit(120, blockable: false, parryable: false, dodgeable: false));
            knightFurious.ReceiveHit(Fixture.FrontHit(120, blockable: false, parryable: false, dodgeable: false));
            Assert.AreEqual(k0 - knight.Health.Current, kf0 - knightFurious.Health.Current, 0.01f, "furia rycerza nie daje redukcji");
        }

        /// <summary>Broń dwuręczna: +10% szybkości ataku i pancerz w ataku – w zamachu połowa obrażeń i bez drgnięcia.</summary>
        [Test]
        public void TwoHanded_AttacksFaster_AndHasHyperArmorWhileSwinging()
        {
            var pc = Hero("class_hu_berserker");
            Assert.IsTrue(pc.Build.twoHandedMelee, "topór dwuręczny");
            Assert.IsFalse(Hero("class_hu_paladin").Build.twoHandedMelee, "korbacz z tarczą");
            Assert.IsFalse(Hero("class_hu_dual").Build.twoHandedMelee, "dwie jednoręczne");

            var idle = Hero("class_hu_berserker");
            float hp0 = idle.Health.Current;
            idle.ReceiveHit(Fixture.FrontHit(100, blockable: false, parryable: false, dodgeable: false));
            float full = hp0 - idle.Health.Current;

            pc.Request(ActionType.LightAttack);
            Fixture.Advance(pc, 1f / 60f);
            Assert.AreEqual(ActionType.LightAttack, pc.Actions.Current);
            Assert.IsTrue(pc.HyperArmor, "zamach bronią dwuręczną");
            float hp = pc.Health.Current;
            pc.ReceiveHit(Fixture.FrontHit(100, blockable: false, parryable: false, dodgeable: false));
            Assert.AreEqual(full * (1f - PlayerCombat.HyperArmorReduction), hp - pc.Health.Current, full * 0.05f + 0.5f, "połowa obrażeń");
            Assert.AreEqual(ActionType.LightAttack, pc.Actions.Current, "cios nie wybija z ataku");
        }

        [Test]
        public void IronSkin_CutsDamageBy80Percent_ButDoesNotPreventDeath()
        {
            var plain = Hero("class_hu_berserker");
            float hp0 = plain.Health.Current, drPlain = plain.CurrentDamageReduction;
            plain.ReceiveHit(Fixture.FrontHit(100, blockable: false, parryable: false, dodgeable: false));
            float full = hp0 - plain.Health.Current;

            var pc = Hero("class_hu_berserker", legacySignature: true);
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Fixture.Advance(pc, 0.5f);
            Assert.Greater(pc.IronSkinTime, 0f);
            float hp = pc.Health.Current;
            // Ryk daje pełną furię (szał) – jej redukcja dochodzi osobno, więc porównanie po zwykłej redukcji obu.
            float expected = full * (1f - pc.CurrentDamageReduction) / (1f - drPlain) * (1f - PlayerCombat.IronSkinReduction)
                             * (1f - PlayerCombat.BerserkerFuryReduction * pc.FuryFraction);
            pc.ReceiveHit(Fixture.FrontHit(100, blockable: false, parryable: false, dodgeable: false));
            Assert.AreEqual(expected, hp - pc.Health.Current, full * 0.05f + 0.5f, "Żelazna skóra: 80% mniej obrażeń");
            pc.ReceiveHit(Fixture.FrontHit(99999, blockable: false, parryable: false, dodgeable: false));
            Assert.IsTrue(pc.IsDead, "Żelazna skóra nie chroni przed śmiercią");
        }

        [Test]
        public void Overload_EmpowersNextSlotSkill_ThenOverheats()
        {
            var pc = Hero("class_mage", legacySignature: true);
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
