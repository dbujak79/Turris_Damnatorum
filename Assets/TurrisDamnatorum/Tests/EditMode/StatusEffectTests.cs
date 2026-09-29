using System.Collections.Generic;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Żywioły i efekty: krwawienie, podpalenie, chłód/zamrożenie, porażenie oraz reakcje.</summary>
    public class StatusEffectTests
    {
        static readonly BalanceConfig B = new BalanceConfig();
        Fixture fx;

        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        static float Run(StatusEffects s, float seconds, bool moving)
        {
            float total = 0;
            const float dt = 1f / 60f;
            for (float t = 0; t < seconds; t += dt) total += s.Tick(dt, moving, B);
            return total;
        }

        [Test]
        public void Bleed_StacksIndependently_UpToLimit()
        {
            var s = new StatusEffects();
            for (int i = 0; i < 7; i++) s.Apply(StatusKind.Bleed, 100f, 0f, B);
            Assert.AreEqual(B.bleedMaxStacks, s.BleedStacks, "Limit warstw");
            float dmg = Run(s, B.bleedDuration + 0.2f, false);
            Assert.AreEqual(B.bleedMaxStacks * 100f * B.bleedShare, dmg, 1f, "Każda warstwa zadaje ułamek wylądowanych obrażeń fizycznych");
            Assert.AreEqual(0, s.BleedStacks, "Warstwy wygasają");
        }

        [Test]
        public void Bleed_HurtsMore_WhenTargetMoves_AndNeedsPhysicalDamage()
        {
            var still = new StatusEffects(); still.Apply(StatusKind.Bleed, 100f, 0f, B);
            var moving = new StatusEffects(); moving.Apply(StatusKind.Bleed, 100f, 0f, B);
            Assert.AreEqual(Run(still, 4f, false) * B.bleedMovingMultiplier, Run(moving, 4f, true), 1f);

            var magicOnly = new StatusEffects();
            magicOnly.Apply(StatusKind.Bleed, 0f, 80f, B);
            Assert.AreEqual(0, magicOnly.BleedStacks, "Krwawienie powstaje tylko z obrażeń fizycznych");

            var immune = new StatusEffects { BleedMultiplier = 0f };
            immune.Apply(StatusKind.Bleed, 100f, 0f, B);
            Assert.AreEqual(0, immune.BleedStacks, "Odporny na krwawienie");
        }

        [Test]
        public void Burn_RefreshesAndKeepsStronger()
        {
            var s = new StatusEffects();
            s.Apply(StatusKind.Burn, 0f, 100f, B);
            s.Apply(StatusKind.Burn, 0f, 10f, B);
            Assert.IsTrue(s.Burning);
            Assert.AreEqual(100f * B.burnShare, Run(s, B.burnDuration + 0.2f, false), 1f, "Słabsze podpalenie nie zastępuje silniejszego");
        }

        [Test]
        public void Chill_Slows_ThirdStackFreezes_ThenImmunity()
        {
            var s = new StatusEffects();
            Assert.IsFalse(s.Apply(StatusKind.Chill, 0, 10, B));
            Assert.AreEqual(1f - B.chillSlowPerStack, s.MoveMultiplier(B), 1e-4f, "Warstwa chłodu spowalnia");
            s.Apply(StatusKind.Chill, 0, 10, B);
            Assert.IsTrue(s.Apply(StatusKind.Chill, 0, 10, B), "Trzecia warstwa zamraża");
            Assert.IsTrue(s.Frozen);
            Assert.AreEqual(0f, s.MoveMultiplier(B));

            Run(s, B.freezeDuration + 0.1f, false);
            Assert.IsFalse(s.Frozen);
            for (int i = 0; i < 3; i++) Assert.IsFalse(s.Apply(StatusKind.Chill, 0, 10, B), "Po zamrożeniu – chwilowa odporność");
            Run(s, B.freezeImmunity + 0.1f, false);
            s.Clear();
            for (int i = 0; i < 2; i++) s.Apply(StatusKind.Chill, 0, 10, B);
            Assert.IsTrue(s.Apply(StatusKind.Chill, 0, 10, B), "Po odporności znów można zamrozić");
        }

        [Test]
        public void BossControl_IsShorter()
        {
            var boss = new StatusEffects { ControlMultiplier = B.bossControlMultiplier };
            boss.Apply(StatusKind.Frozen, 0, 0, B);
            Assert.AreEqual(B.freezeDuration * B.bossControlMultiplier, boss.FrozenRemaining, 1e-4f);
        }

        [Test]
        public void Shock_IncreasesIncomingDamage()
        {
            var s = new StatusEffects();
            s.Apply(StatusKind.Shock, 0, 0, B);
            var hit = Fixture.FrontHit(100, 50);
            s.ModifyIncoming(ref hit, B);
            Assert.AreEqual(100f * (1 + B.shockDamageBonus), hit.physical, 1e-3f);
            Assert.AreEqual(50f * (1 + B.shockDamageBonus), hit.magic, 1e-3f);
        }

        [Test]
        public void ThermalShock_FireOnChilledTarget()
        {
            var s = new StatusEffects();
            s.Apply(StatusKind.Chill, 0, 10, B);
            var hit = Fixture.FrontHit(0, 40);
            hit.element = Element.Fire;
            Assert.AreEqual("SZOK TERMICZNY", s.ModifyIncoming(ref hit, B));
            Assert.AreEqual(40f * (1 + B.thermalShockBonus), hit.magic, 1e-3f);
            Assert.AreEqual(0, s.ChillStacks, "Chłód znika");
        }

        [Test]
        public void Conduction_LightningOnBleedingTarget_DealsRestOfBleedAtOnce()
        {
            var s = new StatusEffects();
            s.Apply(StatusKind.Bleed, 100f, 0f, B);
            s.Apply(StatusKind.Bleed, 100f, 0f, B);
            float rest = s.BleedRemainingDamage;
            var hit = Fixture.FrontHit(0, 30);
            hit.element = Element.Lightning;
            float instant = s.OnLanded(hit, new HitResult { outcome = HitOutcome.Hit, healthDamage = 30 }, B, out var reaction, out _);
            Assert.AreEqual("PRZEWODZENIE", reaction);
            Assert.AreEqual(rest * B.conductionMultiplier, instant, 1e-3f);
            Assert.AreEqual(0, s.BleedStacks);
        }

        [Test]
        public void StatusTicks_DoNotTriggerReactions()
        {
            var s = new StatusEffects();
            s.Apply(StatusKind.Chill, 0, 10, B);
            var tick = Fixture.FrontHit(0, 10);
            tick.element = Element.Fire; tick.isStatusTick = true;
            Assert.IsNull(s.ModifyIncoming(ref tick, B));
            Assert.AreEqual(1, s.ChillStacks);
        }

        // ------------------------------------------------------------------ Na bohaterze

        static HitData WithStatus(HitData h, StatusKind k, int stacks = 1)
        {
            h.statuses = new List<StatusApplication> { new StatusApplication(k, stacks) };
            return h;
        }

        [Test]
        public void Player_BleedsFromLandedHit_ButNotFromBlockedOne()
        {
            var pc = fx.NewCombatant(fx.NewRun("class_knight"));
            pc.ReceiveHit(WithStatus(Fixture.FrontHit(40), StatusKind.Bleed));
            Assert.AreEqual(1, pc.Status.BleedStacks);
            Fixture.Advance(pc, 0.5f);
            float hp = pc.Health.Current;
            Fixture.Advance(pc, 1.5f);
            Assert.Less(pc.Health.Current, hp, "Krwawienie zadaje obrażenia w czasie");

            var blocker = fx.NewCombatant(fx.NewRun("class_knight"));
            blocker.SetBlockHeld(true);
            var r = blocker.ReceiveHit(WithStatus(Fixture.FrontHit(40), StatusKind.Bleed));
            Assert.AreEqual(HitOutcome.Blocked, r.outcome);
            Assert.AreEqual(0, blocker.Status.BleedStacks, "Zablokowany cios nie nakłada efektów");
        }

        [Test]
        public void Player_FrozenAfterThreeChills_CannotAct()
        {
            var pc = fx.NewCombatant(fx.NewRun("class_knight"));
            for (int i = 0; i < 3; i++) pc.ReceiveHit(WithStatus(Fixture.FrontHit(5), StatusKind.Chill));
            Assert.IsTrue(pc.Status.Frozen);
            Assert.AreEqual(ActionType.Flinch, pc.Actions.Current);
            pc.Request(ActionType.Dodge);
            Assert.AreNotEqual(ActionType.Dodge, pc.Actions.Current, "Zamrożony nie zrobi uniku");
            Fixture.Advance(pc, B.freezeDuration + 0.4f);
            Assert.IsTrue(pc.Actions.IsIdle || pc.Actions.Current == ActionType.Block);
        }

        [Test]
        public void Player_Barrier_AbsorbsStatusTicks()
        {
            var run = fx.NewRun("class_mage");
            var barrier = run.LearnSpell(fx.Get<SpellDefinition>("spell_barrier"));
            run.AssignSlot(barrier, 0);
            var pc = fx.NewCombatant(run);
            pc.RequestSkill(0);
            Fixture.Advance(pc, 1f);
            pc.Status.Apply(StatusKind.Bleed, 60f, 0f, B);
            float hp = pc.Health.Current, shield = pc.BarrierAmount;
            Fixture.Advance(pc, 2f);
            Assert.AreEqual(hp, pc.Health.Current, 0.01f);
            Assert.Less(pc.BarrierAmount, shield, "Osłona pochłania także tyknięcia");
        }

        // ------------------------------------------------------------------ Treść

        [Test]
        public void AllSkills_HaveValidDataAndDescriptions()
        {
            int spells = 0, techniques = 0;
            foreach (var so in fx.bundle.all)
            {
                if (!(so is SpellDefinition s)) continue;
                if (s.IsSpell) spells++; else techniques++;
                Assert.Greater(s.cooldown, 0f, s.id);
                Assert.IsFalse(string.IsNullOrEmpty(Describe.Spell(s, 0, null, fx.Cfg.balance)), s.id);
                if (s.attack.element != Element.None || s.attack.statuses.Count > 0)
                    StringAssert.Contains(s.attack.statuses.Count > 0 ? Names.Status(s.attack.statuses[0].kind) : Names.Element(s.attack.element),
                        Describe.Spell(s, 0, null, fx.Cfg.balance), s.id + ": opis pokazuje żywioł/efekt");
                Assert.IsTrue(fx.Cfg.baseSpellPool.Contains(s) || fx.Cfg.unlocks.Exists(u => u.target == s) || fx.Cfg.classes.Exists(c => c.startingSpells.Contains(s)),
                    s.id + " jest dostępna (pula, odblokowanie lub klasa)");
            }
            Assert.GreaterOrEqual(spells, 13);
            Assert.GreaterOrEqual(techniques, 10);
        }

        [Test]
        public void Weapons_AxeAndDaggerCanBleed_EnemiesHaveAffinities()
        {
            Assert.IsNotEmpty(fx.Get<ItemDefinition>("weapon_axe").weapon.heavy.statuses);
            Assert.IsNotEmpty(fx.Get<ItemDefinition>("weapon_dagger").weapon.light.statuses);
            StringAssert.Contains("krwawienie", Describe.Item(new ItemInstance(fx.Get<ItemDefinition>("weapon_axe")), null, fx.Cfg.balance));
            Assert.Greater(fx.Get<EnemyDefinition>("enemy_ghoul").fireMultiplier, 1f, "Ghul słaby na ogień");
            Assert.Less(fx.Get<EnemyDefinition>("enemy_castellan").fireMultiplier, 1f, "Kasztelan odporny na ogień");
        }
    }
}
