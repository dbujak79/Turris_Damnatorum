using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>
    /// Cechy klasowe (docs/PROMPT_WDROZENIE_30_CECH_KLASOWYCH.md, rozdz. 16.1–16.2): katalog i wyłączność, wybór i profil per klasa,
    /// RunFactory i zapis (z migracją starych zapisów), oraz matematyka zasobów cech działająca na prawdziwym PlayerCombat.
    /// </summary>
    public class ClassFeatureTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        ClassDefinition C(string id) => fx.Get<ClassDefinition>(id);
        SpellDefinition S(string id) => fx.Get<SpellDefinition>(id);
        ClassFeatureTuning T => fx.Cfg.classFeatures;

        PlayerCombat Hero(string classId, string featureId = null, int loadout = 0, SpellDefinition opening = null)
        {
            var plan = fx.Plan(classId);
            plan.loadoutIndex = loadout; plan.startingSkill = opening;
            plan.classFeatureId = featureId;
            return fx.NewCombatant(RunFactory.Create(plan, fx.Cfg, 7));
        }

        static HitData Unblockable(float dmg) => Fixture.FrontHit(dmg, blockable: false, parryable: false, dodgeable: false);

        // ================================================================== Dane, wyłączność, wybór

        [Test]
        public void Catalog_Has30Features_TwoPerClass_UniqueIds_OwnersExist()
        {
            Assert.AreEqual(15, fx.Cfg.classes.Count);
            Assert.AreEqual(30, ClassFeatureCatalog.All.Count);
            Assert.AreEqual(30, ClassFeatureCatalog.All.Select(d => d.id).Distinct().Count(), "unikalne id");
            foreach (var c in fx.Cfg.classes)
            {
                var own = ClassFeatureCatalog.For(c);
                Assert.AreEqual(2, own.Count, c.id);
                Assert.AreNotEqual(own[0].id, own[1].id);
                Assert.AreEqual(ClassFeatureVariant.A, own[0].variant);
                Assert.AreEqual(ClassFeatureVariant.B, own[1].variant);
                foreach (var d in own)
                {
                    Assert.IsNotNull(ClassFeatureFactory.Create(d), d.id + ": kontroler");
                    Assert.IsNotEmpty(d.Rule(T)); Assert.IsNotEmpty(d.Benefit(T)); Assert.IsNotEmpty(d.Cost(T)); Assert.IsNotEmpty(d.Usage(T));
                    Assert.IsNotEmpty(d.coreBuild); Assert.IsNotEmpty(d.developedBuild); Assert.IsNotEmpty(d.exampleBuild);
                    foreach (var sid in d.startingSkillIds) Assert.IsTrue(fx.Cfg.baseSpellPool.Any(s => s != null && s.id == sid), d.id + ": zwykły skill " + sid + " w puli sklepu");
                }
                // Specjalizacja nie dostaje wariantów rodzica i odwrotnie.
                if (c.parent != null) Assert.IsFalse(own.Any(d => d.classId == c.parent.id));
            }
            Assert.IsFalse(fx.Cfg.baseSpellPool.Any(s => s != null && s.id.StartsWith("class_feature")), "cechy nie trafiają do sklepu");
            Assert.IsFalse(fx.Cfg.unlocks.Any(u => u.target != null && u.target.id.StartsWith("class_feature")), "ani do odblokowań");
        }

        [Test]
        public void Resolve_ForeignUnknownAndEmptyId_FallBackToVariantA()
        {
            var berserker = C("class_hu_berserker");
            Assert.AreEqual("class_feature.berserker.redline", ClassFeatureCatalog.Resolve(berserker, null, out var w0).id);
            Assert.IsNull(w0);
            Assert.AreEqual("class_feature.berserker.redline", ClassFeatureCatalog.Resolve(berserker, "class_feature.mage.arcane_debt", out var w1).id);
            Assert.IsNotNull(w1, "obca cecha – diagnostyka");
            Assert.AreEqual("class_feature.berserker.redline", ClassFeatureCatalog.Resolve(berserker, "bzdura", out var w2).id);
            Assert.IsNotNull(w2);
            Assert.AreEqual("class_feature.berserker.fury_spend", ClassFeatureCatalog.Resolve(berserker, "class_feature.berserker.fury_spend", out _).id);
            // Obca cecha w stanie podejścia nie daje mocy.
            var run = fx.NewRun("class_hu_berserker");
            run.classFeatureId = "class_feature.mage.arcane_debt";
            Assert.IsNull(run.ClassFeature);
            var pc = fx.NewCombatant(run);
            Assert.IsNull(pc.Feature, "obca cecha nie instaluje kontrolera");
        }

        [Test]
        public void ChoiceIsRememberedPerClass_AndDoesNotLeakToOtherClass()
        {
            var storage = new MemoryProfileStorage();
            var meta = new MetaService(fx.Cfg, storage);
            var plan = meta.LastPlan();
            meta.SelectClass(plan, C("class_hu_berserker"));
            Assert.IsTrue(meta.SelectFeature(plan, "class_feature.berserker.fury_spend"));
            Assert.IsFalse(meta.SelectFeature(plan, "class_feature.mage.mana_cloak"), "obcej cechy nie da się wybrać");
            meta.RecordRunStart(plan);
            meta.SelectClass(plan, C("class_knight"));
            Assert.AreEqual("class_feature.warrior.guard", plan.classFeatureId, "inna klasa – jej wariant A");
            meta.SelectClass(plan, C("class_hu_berserker"));
            Assert.AreEqual("class_feature.berserker.fury_spend", plan.classFeatureId, "powrót – zapamiętany wybór klasy");
            var reloaded = new MetaService(fx.Cfg, storage).LastPlan();
            Assert.AreEqual("class_feature.berserker.fury_spend", reloaded.classFeatureId, "profil po ponownym wczytaniu");
            // Wygląd nie zmienia mechaniki.
            plan.appearance = C("class_hu_mage_fire");
            var run = RunFactory.Create(plan, fx.Cfg, 3);
            Assert.AreEqual("class_feature.berserker.fury_spend", run.classFeatureId);
        }

        [Test]
        public void OldProfileV3_WithoutFeatures_Loads()
        {
            var storage = new MemoryProfileStorage { Content = "{\"version\":3,\"ash\":12,\"lastClassId\":\"class_hu_berserker\"}" };
            var meta = new MetaService(fx.Cfg, storage);
            Assert.AreEqual(12, meta.Profile.ash);
            Assert.AreEqual(ProfileData.CurrentVersion, meta.Profile.version);
            Assert.AreEqual("class_feature.berserker.redline", meta.LastPlan().classFeatureId);
        }

        [Test]
        public void RunFactory_InstallsExactlyOneFeature_WithoutOldSignature()
        {
            foreach (var c in fx.Cfg.classes)
                foreach (var d in ClassFeatureCatalog.For(c))
                {
                    var pc = Hero(c.id, d.id);
                    Assert.AreEqual(d.id, pc.Run.classFeatureId);
                    Assert.IsNotNull(pc.Feature, d.id);
                    Assert.AreEqual(d.id, pc.Feature.Def.id);
                    Assert.IsNull(pc.Run.signatureSkill, d.id + ": bez starej sygnatury");
                    Assert.IsNull(pc.Skill(PlayerCombat.SignatureSlot));
                }
        }

        [Test]
        public void Snapshot_KeepsFeature_AndOldSnapshotMapsToVariantA_ReturningSignatureSeals()
        {
            var pc = Hero("class_hu_berserker", "class_feature.berserker.fury_spend");
            var json = JsonUtility.ToJson(RunSnapshot.Capture(pc.Run, "t"));
            var back = JsonUtility.FromJson<RunSnapshot>(json).Restore(fx.Cfg);
            Assert.AreEqual("class_feature.berserker.fury_spend", back.classFeatureId);

            // Stary zapis: brak pola cechy, dawna sygnatura hu_undying z pieczęcią formy.
            var old = RunSnapshot.Capture(pc.Run, "stary");
            old.classFeatureId = null;
            var sealId = HuCatalog.Seals.First().id;
            old.signature = new RunSnapshot.Skill { id = "hu_undying", form = sealId };
            int pouch = pc.Run.sealPouch.Count;
            var restored = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(old)).Restore(fx.Cfg);
            Assert.AreEqual("class_feature.berserker.redline", restored.classFeatureId, "stary Berserker → Czerwona linia");
            Assert.IsNull(restored.signatureSkill, "stara sygnatura nie przetrwa obok cechy");
            Assert.AreEqual(pouch + 1, restored.sealPouch.Count, "pieczęć sygnatury wraca do sakwy");
            Assert.AreEqual(pc.Run.equipment.Get(EquipSlot.MainHand).definition, restored.equipment.Get(EquipSlot.MainHand).definition, "wyposażenie zostaje");
        }

        // ================================================================== Berserker A – Czerwona linia

        [Test]
        public void RedLine_CurveTable_IsMonotonic()
        {
            float[] life = { 1f, 0.75f, 0.5f, 0.35f, 0.25f, 0.1f, 0f };
            float[] want = { 0f, 0.2f, 0.4f, 0.52f, 0.6f, 0.72f, 0.8f };
            for (int i = 0; i < life.Length; i++) Assert.AreEqual(want[i], RedLineFeature.Reduction(life[i], 0.8f), 1e-4f, "PŻ " + life[i]);
            float prev = -1f;
            for (float h = 1f; h >= 0f; h -= 0.01f) { float r = RedLineFeature.Reduction(h, 0.8f); Assert.GreaterOrEqual(r, prev); prev = r; }
            // Przykład z promptu: (1 − 0,52)(1 − 0,20)(1 − 0,50)(1 − 0,45) = 0,1056.
            Assert.AreEqual(0.1056f, (1 - 0.52f) * (1 - 0.2f) * (1 - 0.5f) * (1 - 0.45f), 1e-4f);
        }

        [Test]
        public void RedLine_ReservesToLine_CapsHealing_NoFreeHealth_StillKillable()
        {
            var pc = Hero("class_hu_berserker");
            var rl = (RedLineFeature)pc.Feature;
            float max = pc.Health.Max;
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.IsTrue(rl.Active);
            Assert.AreEqual(max * T.redlineCap, pc.Health.Current, 0.5f, "życie odcięte do linii");
            Assert.AreEqual(max, pc.Health.Max, "normalne maksimum bez zmian");
            Assert.AreEqual(T.redlineCap, pc.Health.Fraction, 0.01f, "35/100 to nie pełne życie");
            pc.Health.Restore(max);
            Assert.AreEqual(max * T.redlineCap, pc.Health.Current, 0.5f, "leczenie najwyżej do linii");
            Assert.AreEqual(1f - (0.8f * (1f - T.redlineCap)), 1f - rl.CurrentReduction, 0.01f);
            // Kolejny cios korzysta z życia sprzed rozstrzygnięcia: wyższa redukcja przy niższym życiu.
            pc.Health.SetCurrent(max * 0.1f);
            Assert.AreEqual(0.72f, rl.CurrentReduction, 0.01f);
            Fixture.Advance(pc, T.redlineSeconds + 0.2f);
            Assert.IsFalse(rl.Active);
            Assert.LessOrEqual(pc.Health.Current, max * 0.1f + pc.Build.stats[StatType.HealthRegen] * (T.redlineSeconds + 0.5f) + 12f * (T.redlineSeconds + 0.5f), "koniec transu nie oddaje życia");
            Assert.IsTrue(float.IsPositiveInfinity(pc.Health.Ceiling), "linia zdjęta");

            var low = Hero("class_hu_berserker");
            low.Health.SetCurrent(low.Health.Max * 0.2f);
            float before = low.Health.Current;
            low.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.LessOrEqual(low.Health.Current, before + 0.01f, "aktywacja nie leczy");
            low.ReceiveHit(Unblockable(99999));
            Assert.IsTrue(low.IsDead, "śmierć nadal możliwa");
        }

        [Test]
        public void RedLine_DamageMatchesCurveFromPreHitLife()
        {
            var plain = Hero("class_hu_berserker", "class_feature.berserker.fury_spend");
            plain.Health.SetCurrent(plain.Health.Max * 0.35f);
            float drPlain = plain.CurrentDamageReduction;
            float h0 = plain.Health.Current;
            plain.ReceiveHit(Unblockable(60));
            float full = h0 - plain.Health.Current;

            var pc = Hero("class_hu_berserker");
            pc.RequestSkill(PlayerCombat.SignatureSlot); // odcina do 35%, ryk daje furię (szał)
            float hp = pc.Health.Current;
            float r = RedLineFeature.Reduction(hp / pc.Health.Max, T.redlineMaxReduction);
            float expected = full * (1f - pc.CurrentDamageReduction) / (1f - drPlain) * (1f - r) * (1f - PlayerCombat.BerserkerFuryReduction * pc.FuryFraction);
            pc.ReceiveHit(Unblockable(60));
            Assert.AreEqual(expected, hp - pc.Health.Current, full * 0.05f + 0.6f, "jeden mnożnik krzywej");
        }

        // ================================================================== Berserker B – Spalenie furii

        [Test]
        public void FurySpend_TokensFromRemainingRage_PayOnce_AndRefundOnCancel()
        {
            var pc = Hero("class_hu_berserker", "class_feature.berserker.fury_spend");
            var f = (FurySpendFeature)pc.Feature;
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.AreEqual(0, f.Tokens, "poza szałem – odmowa");
            Assert.AreEqual(0f, f.Cooldown);
            pc.AddFury(100f);
            Assert.IsTrue(pc.Raging);
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.IsFalse(pc.Raging, "szał przerwany");
            Assert.AreEqual(Mathf.Min(T.furyMaxTokens, Mathf.FloorToInt(6f / T.furySecondsPerToken)), f.Tokens);
            int slot = pc.Run.SlotOf(pc.Run.FindSpell(S("hu_cleave")));
            float st = pc.Stamina.Current;
            int tokens = f.Tokens;
            pc.RequestSkill(slot);
            Assert.AreEqual(ActionType.Cast, pc.Actions.Current);
            Assert.AreEqual(tokens - 1, f.Tokens, "jeden żeton");
            Assert.AreEqual(st, pc.Stamina.Current, 0.01f, "wytrzymałość nie pobrana");
            pc.MoveIntent = Vector3.forward;
            pc.Request(ActionType.Dodge); // przerwanie przed wyzwoleniem
            Assert.AreEqual(tokens, f.Tokens, "żeton zwrócony");
            Fixture.Advance(pc, T.furyTokenLife + 1f);
            Assert.AreEqual(0, f.Tokens, "żetony gasną");
        }

        // ================================================================== Paladyn A – Rozgrzeszenie

        [Test]
        public void Absolution_OnlyOverhealAfterLimitsFeedsGrace_WaveDoesNotFeedIt()
        {
            var pc = Hero("class_hu_paladin");
            var a = (AbsolutionFeature)pc.Feature;
            pc.HealFrom(30f, HealSource.Regen);
            Assert.AreEqual(30f, a.Grace, 0.01f, "pełne życie: całe leczenie to nadwyżka");
            pc.Health.SetCurrent(pc.Health.Max - 20f);
            pc.HealFrom(50f, HealSource.Regen);
            Assert.AreEqual(60f, a.Grace, 0.01f, "najpierw 20 PŻ, nadwyżka 30");
            pc.HealFrom(40f, HealSource.Flask);
            Assert.AreEqual(60f + 40f * T.graceFromFlask, a.Grace, 0.01f, "flaszka – jawny współczynnik");
            pc.HealFrom(99999f, HealSource.Regen);
            Assert.AreEqual(a.Capacity, a.Grace, 0.01f, "pojemność");
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.AreEqual(0f, a.Grace, "wydane");
            Assert.Greater(a.Cooldown, 0f);
            Assert.AreEqual(0f, a.Grace, "fala nie wytwarza Łaski");
        }

        // ================================================================== Mag A – Dług arkanów

        [Test]
        public void ArcaneDebt_BorrowsHardLimit_BlocksAfterWindow_AllRestoresRepay_SurvivesSnapshot()
        {
            var pc = Hero("class_mage");
            var d = (ArcaneDebtFeature)pc.Feature;
            int slot = pc.Run.SlotOf(pc.Run.FindSpell(S("hu_nova_fire")));
            var sk = pc.Skill(slot);
            pc.Mana.SetCurrent(0f);
            pc.RequestSkill(slot);
            Assert.AreNotEqual(ActionType.Cast, pc.Actions.Current, "bez okna – brak many");
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.IsTrue(d.Active);
            pc.RequestSkill(slot);
            Assert.AreEqual(ActionType.Cast, pc.Actions.Current, "dług pokrył koszt");
            Assert.AreEqual(sk.manaCost, d.Debt, 0.01f);
            Fixture.Advance(pc, T.debtSeconds + 0.5f);
            Assert.IsFalse(d.Active);
            Assert.IsNotNull(d.BlockCast(sk, slot), "po oknie dług blokuje czary");
            float before = d.Debt;
            pc.Mana.Restore(5f);
            Assert.AreEqual(before - 5f, d.Debt, 0.6f, "odnowienie najpierw spłaca dług");
            pc.RefreshBuild();
            Assert.AreEqual(before - 5f, d.Debt, 0.6f, "przeliczenie buildu nie zeruje długu");
            var back = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(RunSnapshot.Capture(pc.Run))).Restore(fx.Cfg);
            Assert.AreEqual(pc.Run.manaDebt, back.manaDebt, 0.01f, "dług w zapisie");
            float debt = d.Debt;
            pc.RestoreAll();
            Assert.AreEqual(0f, d.Debt, 0.01f, "odnowienie piętra rozlicza dług");
            Assert.AreEqual(pc.Mana.Max, pc.Mana.Current, debt + 0.5f);
        }

        [Test]
        public void ArcaneDebt_OverLimit_IsRejectedWithoutPartialPayment()
        {
            var pc = Hero("class_mage");
            var d = (ArcaneDebtFeature)pc.Feature;
            int slot = pc.Run.SlotOf(pc.Run.FindSpell(S("hu_nova_fire")));
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            pc.Mana.SetCurrent(0f);
            pc.Run.manaDebt = d.Limit - 1f;
            pc.RequestSkill(slot);
            Assert.AreNotEqual(ActionType.Cast, pc.Actions.Current);
            Assert.AreEqual(d.Limit - 1f, d.Debt, 0.01f, "nic nie pobrano");
            Assert.IsTrue(pc.Skill(slot).instance.Ready, "ładunek nietknięty");
        }

        // ================================================================== Mag B – Płaszcz many

        [Test]
        public void ManaCloak_PartialManaCoversOnlyPaidPart()
        {
            var plain = Hero("class_mage", "class_feature.mage.mana_cloak");
            plain.ConsumeWardForTest();
            float h0 = plain.Health.Current;
            plain.ReceiveHit(Unblockable(80));
            float full = h0 - plain.Health.Current;

            var pc = Hero("class_mage", "class_feature.mage.mana_cloak");
            pc.ConsumeWardForTest();
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            float m0 = pc.Mana.Current, hp = pc.Health.Current;
            pc.ReceiveHit(Unblockable(80));
            float covered = full * T.cloakShare;
            Assert.AreEqual(full - covered, hp - pc.Health.Current, 0.6f);
            Assert.AreEqual(covered * T.cloakManaPerHealth, m0 - pc.Mana.Current, 1f);

            pc.Mana.SetCurrent(covered * T.cloakManaPerHealth * 0.5f);
            hp = pc.Health.Current;
            pc.ReceiveHit(Unblockable(80));
            Assert.AreEqual(full - covered * 0.5f, hp - pc.Health.Current, 0.8f, "połowa many – połowa ochrony");
            Assert.AreEqual(0f, pc.Mana.Current, 0.01f);
        }

        // ================================================================== Wojownik

        [Test]
        public void Guard_BlocksFrontDuringOwnAttack_BackHits_StaminaDoesNotRegen()
        {
            var pc = Hero("class_knight");
            pc.DebugNoPassiveBlock = true;
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            pc.SetBlockHeld(true);
            pc.Request(ActionType.LightAttack);
            Assert.AreEqual(ActionType.LightAttack, pc.Actions.Current);
            var r = pc.ReceiveHit(Fixture.FrontHit(40));
            Assert.AreEqual(HitOutcome.Blocked, r.outcome, "blok w trakcie ciosu");
            pc.Request(ActionType.LightAttack);
            var back = pc.ReceiveHit(Fixture.BackHit(40));
            Assert.AreNotEqual(HitOutcome.Blocked, back.outcome, "plecy odsłonięte");
            pc.SetBlockHeld(false);
            Fixture.Advance(pc, 1f);
            float st = pc.Stamina.Current;
            Fixture.Advance(pc, 1f);
            Assert.AreEqual(st, pc.Stamina.Current, 0.01f, "wytrzymałość nie odrasta w oknie");

            var noGuard = Hero("class_hu_berserker", "class_feature.berserker.redline");
            Assert.IsNotNull(noGuard);
        }

        [Test]
        public void Arsenal_SwapHundredTimes_NoDuplicates_NoHealOrCooldownReset()
        {
            var plan = fx.Plan("class_knight");
            plan.classFeatureId = "class_feature.warrior.arsenal";
            var run = RunFactory.Create(plan, fx.Cfg, 9);
            Assert.IsNotNull(run.arsenalMain, "drugi zestaw od startu");
            var pc = fx.NewCombatant(run);
            int slot = 0;
            var skill = pc.Skill(slot);
            pc.RequestSkill(slot);
            Fixture.Advance(pc, 1.5f);
            pc.Health.SetCurrent(pc.Health.Max * 0.5f);
            float hp = pc.Health.Current;
            var all = new HashSet<ItemInstance>(run.equipment.Items.Concat(run.inventory).Concat(run.ArsenalItems));
            int total = all.Count;
            bool readyBefore = skill.instance.Ready;
            for (int i = 0; i < 100; i++)
            {
                pc.RequestSkill(PlayerCombat.SignatureSlot);
                Fixture.Advance(pc, T.arsenalSwapInterval + 0.02f);
            }
            var after = run.equipment.Items.Concat(run.inventory).Concat(run.ArsenalItems).ToList();
            Assert.AreEqual(total, after.Count, "brak duplikatów");
            Assert.AreEqual(total, after.Distinct().Count());
            Assert.LessOrEqual(pc.Health.Current, hp + pc.Build.stats[StatType.HealthRegen] * 80f + 1f, "przełączanie nie leczy");
            Assert.AreEqual(100, ((ArsenalFeature)pc.Feature).Swaps);
            var back = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(RunSnapshot.Capture(run))).Restore(fx.Cfg);
            Assert.AreEqual(run.arsenalMain?.definition, back.arsenalMain?.definition, "zestaw II w zapisie");
        }

        // ================================================================== Mag bitewny A – Arkana oręża

        [Test]
        public void WeaponArcana_MakesWeaponAFocus_OnlyForOwnerAndOnlyInWindow()
        {
            var pc = Hero("class_hu_battlemage");
            pc.Run.UnequipToInventory(EquipSlot.OffHand); // bez grymuaru – nova zostaje w kolekcji tylko z przedmiotu, więc dokładamy kulę
            var orb = pc.Run.LearnSpell(S("hu_infernal_orb"));
            pc.Run.AssignSlot(orb, 0);
            pc.RefreshBuild();
            Assert.IsFalse(pc.Skill(0).equipmentMet, "bez fokusu");
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.IsTrue(pc.Skill(0).equipmentMet, "broń jako fokus");
            Assert.IsTrue(pc.Skill(0).byArcana);
            Assert.IsNotNull(pc.Build.arcanaAttribute);
            Fixture.Advance(pc, T.arcanaSeconds + 0.3f);
            Assert.IsFalse(pc.Skill(0).equipmentMet, "po oknie – znowu bez fokusu");

            var knight = Hero("class_knight");
            var orb2 = knight.Run.LearnSpell(S("hu_infernal_orb"));
            knight.Run.AssignSlot(orb2, 0);
            knight.Run.weaponArcanaActive = true;
            knight.RefreshBuild();
            Assert.IsFalse(knight.Skill(0).equipmentMet, "ta sama broń na innej klasie nie jest fokusem");
        }

        // ================================================================== Statusy: Wielki pożar, Szron, odpowiedzi

        [Test]
        public void GreatFire_DistinctSourcesStack_SameSourceStaysOneFamily_CollapseKeepsStrongest()
        {
            var b = fx.Cfg.balance;
            var s = new StatusEffects();
            var m = StatusModifiers.None; m.burnFamilies = 3;
            m.burnFamily = 1; s.Apply(StatusKind.Burn, 0, 100, b, m);
            m.burnFamily = 1; s.Apply(StatusKind.Burn, 0, 100, b, m);
            Assert.AreEqual(1, s.BurnFamilyCount, "trzy kopie jednego źródła – jedna rodzina");
            m.burnFamily = 2; s.Apply(StatusKind.Burn, 0, 300, b, m);
            m.burnFamily = 3; s.Apply(StatusKind.Burn, 0, 50, b, m);
            m.burnFamily = 4; s.Apply(StatusKind.Burn, 0, 10, b, m);
            Assert.AreEqual(3, s.BurnFamilyCount, "limit rodzin");
            float strongest = 300f * b.burnShare / b.burnDuration;
            s.CollapseBurnFamilies();
            Assert.AreEqual(1, s.BurnFamilyCount);
            Assert.AreEqual(strongest, s.BurnDpsTotal, 0.01f, "zostaje najsilniejsza, bez sumy");
            var plain = new StatusEffects();
            plain.Apply(StatusKind.Burn, 0, 100, b); plain.Apply(StatusKind.Burn, 0, 300, b);
            Assert.AreEqual(1, plain.BurnFamilyCount, "bez cechy – zwykłe podpalenie");
        }

        [Test]
        public void EnduringFrost_RepeatsThermalShock_WithoutConsumingChill_AndRateLimited_NoControl()
        {
            var b = fx.Cfg.balance;
            var s = new StatusEffects();
            s.MarkFrost(6f, 1f, 0.35f);
            HitData Fire() => new HitData { magic = 50, element = Element.Fire, mods = StatusModifiers.None };
            var h1 = Fire(); Assert.AreEqual("SZOK TERMICZNY", s.ModifyIncoming(ref h1, b), "Szron spełnia warunek chłodu");
            var h2 = Fire(); Assert.IsNull(s.ModifyIncoming(ref h2, b), "limit raz na sekundę");
            s.Tick(1.05f, false, b);
            var h3 = Fire(); Assert.AreEqual("SZOK TERMICZNY", s.ModifyIncoming(ref h3, b), "ponownie po odstępie");
            Assert.AreEqual(0, s.ChillStacks, "sam znacznik nie spowalnia");
            Assert.IsFalse(s.Incapacitated);
            var earth = new HitData { magic = 50, element = Element.Earth, mods = StatusModifiers.None };
            s.Tick(1.05f, false, b);
            float instant = s.OnLanded(earth, new HitResult { outcome = HitOutcome.Hit, healthDamage = 50 }, b, out var reaction, out bool froze);
            Assert.AreEqual("BŁOTO (SZRON)", reaction);
            Assert.IsFalse(froze, "bez ogłuszenia z wirtualnego chłodu");
            Assert.Greater(instant, 0f, "premia do obrażeń ziemi");
            s.ClearFrostMark();
            Assert.IsFalse(s.FrostMarked);
        }

        [Test]
        public void AnsweringArrow_DeterministicOrder()
        {
            var b = fx.Cfg.balance;
            var s = new StatusEffects();
            Assert.AreEqual(Element.None, AnsweringArrowFeature.Answer(s));
            s.Apply(StatusKind.Burn, 0, 100, b);
            Assert.AreEqual(Element.Earth, AnsweringArrowFeature.Answer(s));
            s.Apply(StatusKind.Chill, 10, 0, b);
            Assert.AreEqual(Element.Fire, AnsweringArrowFeature.Answer(s), "chłód przed podpaleniem");
            s.Apply(StatusKind.Bleed, 100, 0, b);
            Assert.AreEqual(Element.Lightning, AnsweringArrowFeature.Answer(s), "krwawienie pierwsze");
        }

        [Test]
        public void SpellArrow_OnlyOffensiveHuSpellsAreCompatible()
        {
            var pc = Hero("class_hu_arcane_archer");
            var tear = pc.Run.FindSpell(S("hu_glacial_tear"));
            Assert.IsNotNull(tear, "czar startowy wariantu w kolekcji");
            pc.Run.AssignSlot(tear, 1);
            var aura = pc.Run.LearnSpell(S("hu_aura_defiance"));
            pc.Run.AssignSlot(aura, 2);
            pc.RefreshBuild();
            Assert.IsTrue(SpellArrowFeature.Compatible(pc.Skill(1), out _));
            Assert.IsFalse(SpellArrowFeature.Compatible(pc.Skill(2), out var why));
            Assert.IsNotEmpty(why);
            Assert.IsFalse(pc.Skill(1).equipmentMet, "bez fokusu zwykły rzut niemożliwy");
            // Wybór z przycisku slotu nie rzuca czaru i nie pobiera many.
            float mana = pc.Mana.Current;
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            pc.RequestSkill(1);
            var f = (SpellArrowFeature)pc.Feature;
            Assert.AreSame(pc.Skill(1), f.Armed);
            Assert.AreNotEqual(ActionType.Cast, pc.Actions.Current);
            Assert.AreEqual(mana, pc.Mana.Current, 0.01f, "przygotowanie bez kosztu");
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.IsNull(f.Armed, "anulowanie bez straty");
        }

        // ================================================================== Odmowy bez kosztu

        [Test]
        public void FailedActivations_DoNotStartCooldown()
        {
            foreach (var (cls, id) in new[] { ("class_hu_assassin", "class_feature.assassin.open_artery"), ("class_hu_assassin", "class_feature.assassin.pinned_shadow"),
                     ("class_hu_summoner", "class_feature.summoner.incarnation"), ("class_hu_summoner", "class_feature.summoner.chimera"),
                     ("class_hu_paladin", "class_feature.paladin.absolution"), ("class_hu_paladin", "class_feature.paladin.judgement"),
                     ("class_hu_mage_water", "class_feature.water.deep_current"), ("class_hu_mage_earth", "class_feature.earth.shattering_shell"),
                     ("class_hu_berserker", "class_feature.berserker.fury_spend"), ("class_rogue", "class_feature.rogue.stolen_trick") })
            {
                var pc = Hero(cls, id);
                if (pc.Feature is ShatteringShellFeature) pc.ConsumeWardForTest();
                float mana = pc.Mana.Current, st = pc.Stamina.Current;
                pc.RequestSkill(PlayerCombat.SignatureSlot);
                Assert.AreEqual(0f, pc.Feature.Cooldown, id + ": odmowa bez odnowienia");
                Assert.AreEqual(mana, pc.Mana.Current, 0.01f, id + ": bez many");
                Assert.AreEqual(st, pc.Stamina.Current, 0.01f, id);
                Assert.Greater(pc.FeatureTelemetry.rejections.Count, 0, id + ": przyczyna w telemetrii");
            }
        }

        [Test]
        public void ShatteringShell_ConsumesWardInSameTransaction_SplitsBudget()
        {
            var pc = Hero("class_hu_mage_earth", "class_feature.earth.shattering_shell");
            var f = (ShatteringShellFeature)pc.Feature;
            float ward = pc.Ward;
            Assert.Greater(ward, 0f);
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            Assert.AreEqual(0f, pc.Ward, "ochrona zużyta");
            Assert.AreEqual(T.shellMaxPlates, f.Plates);
            Assert.AreEqual(ward, f.PlateBudget, 0.01f, "suma płyt = zużyta ochrona");
            Fixture.Advance(pc, 0.5f);
            Assert.AreEqual(0f, pc.Ward, 0.01f, "osłona nie odrasta od razu");
        }

        [Test]
        public void TwoBeats_LaneCompatibility()
        {
            var pc = Hero("class_hu_battlemage", "class_feature.battlemage.two_beats");
            var nova = pc.Run.FindSpell(S("hu_nova_fire"));
            Assert.IsNotNull(nova);
            int slot = pc.Run.SlotOf(nova);
            Assert.IsTrue(PlayerCombat.LaneCompatible(pc.Skill(slot)), "nova bez akcji ciała");
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            pc.Request(ActionType.LightAttack);
            Assert.AreEqual(ActionType.LightAttack, pc.Actions.Current);
            float mana = pc.Mana.Current;
            pc.RequestSkill(slot);
            Assert.AreEqual(ActionType.LightAttack, pc.Actions.Current, "atak trwa dalej");
            Assert.IsTrue(((TwoBeatsFeature)pc.Feature).LaneBusy, "czar w drugim torze");
            Assert.Less(pc.Mana.Current, mana, "czar zapłacony maną");
            Assert.IsFalse(pc.Skill(slot).instance.Ready || pc.Skill(slot).instance.MaxCharges > 1, "odnowienie czaru");
        }

        [Test]
        public void MismatchedPair_TempoAveragesBothWeapons()
        {
            var pc = Hero("class_hu_dual", "class_feature.dual.mismatched_pair", 1); // korbacz + miecz
            var f = pc.Feature;
            var main = pc.Build.weapon.light;
            Assert.AreEqual(1f, f.AttackTimeScale(ActionType.LightAttack, main), "poza oknem bez zmian");
            pc.RequestSkill(PlayerCombat.SignatureSlot);
            var off = pc.Build.offWeapon.definition.weapon.light;
            float expect = (main.windup + main.active + main.recovery + off.windup + off.active + off.recovery) * 0.5f / (main.windup + main.active + main.recovery);
            Assert.AreEqual(expect, f.AttackTimeScale(ActionType.LightAttack, main), 1e-4f);
            Assert.IsFalse(f.AllowsOffhandAlternation, "wspólne uderzenie zamiast naprzemiennych ciosów");
        }
    }
}
