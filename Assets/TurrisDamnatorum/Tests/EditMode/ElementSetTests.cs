using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Zestawy żywiołów i krwawienia (plan rozwoju, etapy A i B).</summary>
    public class ElementSetTests
    {
        static readonly BalanceConfig B = new BalanceConfig();
        Fixture fx;

        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        static HitData Elemental(float magic, Element e)
        {
            var h = Fixture.FrontHit(0, magic);
            h.element = e;
            h.mods = StatusModifiers.None;
            return h;
        }

        // ------------------------------------------------------------------ A1. Roztrzaskanie

        [Test]
        public void Shatter_LightningOnFrozen_ExplodesAndBreaksIce()
        {
            var s = new StatusEffects();
            s.Apply(StatusKind.Frozen, 0, 0, B);
            float instant = s.OnLanded(Elemental(40, Element.Lightning), new HitResult { outcome = HitOutcome.Hit, healthDamage = 40 }, B,
                out var reaction, out _, out float shatter);
            Assert.AreEqual("ROZTRZASKANIE", reaction);
            Assert.AreEqual(40f * B.shatterShare, shatter, 1e-3f);
            Assert.AreEqual(0f, instant);
            Assert.IsFalse(s.Frozen, "Lód pęka");

            var again = new StatusEffects();
            again.Apply(StatusKind.Frozen, 0, 0, B);
            var reactionHit = Elemental(40, Element.Lightning); reactionHit.isReaction = true;
            again.OnLanded(reactionHit, new HitResult { outcome = HitOutcome.Hit, healthDamage = 40 }, B, out var r2, out _, out float s2);
            Assert.IsNull(r2, "Obrażenia reakcji nie wywołują kolejnej reakcji");
            Assert.AreEqual(0f, s2);
        }

        // ------------------------------------------------------------------ A3. Modyfikatory efektów

        [Test]
        public void Modifiers_ExtendBleedStacks_FasterFreeze_LongerBurn()
        {
            var mods = StatusModifiers.None;
            mods.bleedStackBonus = 1; mods.freezeReduction = 1; mods.burnDurationBonus = 1f;
            var s = new StatusEffects();
            for (int i = 0; i < 8; i++) s.Apply(StatusKind.Bleed, 50, 0, B, mods);
            Assert.AreEqual(B.bleedMaxStacks + 1, s.BleedStacks, "Rękawice rzeźnika: +1 warstwa");

            var f = new StatusEffects();
            f.Apply(StatusKind.Chill, 0, 10, B, mods);
            Assert.IsTrue(f.Apply(StatusKind.Chill, 0, 10, B, mods), "Szron w żyłach: zamrożenie po dwóch warstwach");

            var normal = new StatusEffects(); normal.Apply(StatusKind.Burn, 0, 100, B);
            var longer = new StatusEffects(); longer.Apply(StatusKind.Burn, 0, 100, B, mods);
            float a = 0, b = 0;
            for (int i = 0; i < 300; i++) { a += normal.Tick(1f / 60f, false, B); b += longer.Tick(1f / 60f, false, B); }
            Assert.Greater(b, a * 1.3f, "Dłuższe podpalenie zadaje więcej");
        }

        [Test]
        public void Bonuses_VsFrozenAndBleeding_AndMaceBreaksIce()
        {
            var s = new StatusEffects();
            s.Apply(StatusKind.Frozen, 0, 0, B);
            var hit = Fixture.FrontHit(100); hit.bonusVsFrozen = 0.5f;
            s.ModifyIncoming(ref hit, B);
            Assert.AreEqual(150f, hit.physical, 1e-3f);
            s.OnLanded(hit, new HitResult { outcome = HitOutcome.Hit, healthDamage = 100 }, B, out _, out _);
            Assert.IsFalse(s.Frozen, "Obuch rozbija lód");
        }

        [Test]
        public void BurnImmunity_PreventsBurning()
        {
            var s = new StatusEffects { BurnImmune = true };
            s.Apply(StatusKind.Burn, 0, 100, B);
            Assert.IsFalse(s.Burning);
        }

        // ------------------------------------------------------------------ A2/A4. Odporności i broń z żywiołem

        [Test]
        public void PlayerResist_ReducesElementalPart()
        {
            var plain = fx.NewCombatant(fx.NewRun("class_knight"));
            var run = fx.NewRun("class_knight");
            run.AddItem(new ItemInstance(fx.Get<ItemDefinition>("head_fur")), true);
            var furred = fx.NewCombatant(run);
            Assert.AreEqual(30f, furred.Build.stats[StatType.FrostResist], 1e-3f);
            // Różnica tylko z hełmu: odejmujemy wpływ jego obrony fizycznej, porównując czysty mróz.
            float lostPlain = plain.ReceiveHit(Elemental(100, Element.Frost)).healthDamage;
            float lostFur = furred.ReceiveHit(Elemental(100, Element.Frost)).healthDamage;
            Assert.AreEqual(lostPlain * 0.7f, lostFur, lostPlain * 0.02f, "30% odporności na mróz");
        }

        [Test]
        public void ElementWeapon_SplitsDamage_AndDescribesIt()
        {
            var flame = fx.Get<ItemDefinition>("weapon_flamesword");
            var hit = HitData.FromAttack(flame.weapon.light, 100, UnityEngine.Vector3.zero, null);
            Assert.AreEqual(60f, hit.physical, 1e-3f);
            Assert.AreEqual(40f, hit.magic, 1e-3f);
            Assert.AreEqual(Element.Fire, hit.element);
            StringAssert.Contains("ogień", Describe.Item(new ItemInstance(flame), null, fx.Cfg.balance));
            Assert.IsEmpty(fx.Get<ItemDefinition>("weapon_longsword").weapon.light.statuses, "Kopia broni nie zmienia oryginału");
        }

        // ------------------------------------------------------------------ A5. Nowe umiejętności (bez świata)

        [Test]
        public void BloodPact_CostsHealth_ThenSpellsAreFree()
        {
            var run = fx.NewRun("class_mage");
            var pact = run.LearnSpell(fx.Get<SpellDefinition>("spell_bloodpact"));
            run.AssignSlot(pact, 2);
            var pc = fx.NewCombatant(run);
            float hp = pc.Health.Current;
            pc.RequestSkill(2);
            Fixture.Advance(pc, 1f);
            Assert.Less(pc.Health.Current, hp);
            Assert.AreEqual(3, pc.FreeCasts);
            float mana = pc.Mana.Current;
            pc.RequestSkill(0); // pocisk arkanów
            Assert.AreEqual(ActionType.Cast, pc.Actions.Current);
            Assert.AreEqual(mana, pc.Mana.Current, 0.01f, "Czar bez many");
            Assert.AreEqual(2, pc.FreeCasts);
        }

        [Test]
        public void Warcry_GrantsDamageBuff_AndStamina()
        {
            var run = fx.NewRun("class_knight");
            var cry = run.LearnSpell(fx.Get<SpellDefinition>("skill_warcry"));
            var pc = fx.NewCombatant(run);
            pc.Stamina.Drain(50);
            float st = pc.Stamina.Current;
            pc.RequestSkill(run.SlotOf(cry));
            Fixture.Advance(pc, 0.5f);
            Assert.Greater(pc.DamageBuffTime, 0f);
            Assert.AreEqual(25f, pc.DamageBuffPct, 1e-3f);
            Assert.Greater(pc.Stamina.Current, st + 20f);
        }

        [Test]
        public void StormBoots_ChargeAfterDodge()
        {
            var run = fx.NewRun("class_knight");
            run.AddItem(new ItemInstance(fx.Get<ItemDefinition>("feet_storm")), true);
            var pc = fx.NewCombatant(run);
            Assert.IsFalse(pc.ShockCharged);
            pc.Request(ActionType.Dodge);
            Assert.IsTrue(pc.ShockCharged, "Po uniku ładunek burzy");
        }

        // ------------------------------------------------------------------ C. Cechy poziomów

        [Test]
        public void LevelFeatures_UnlockWithLevel_AndAreDescribed()
        {
            var fireball = fx.Get<SpellDefinition>("spell_fireball");
            Assert.AreEqual(0f, fireball.Feature(LevelFeatureKind.LeaveZone, 3));
            Assert.AreEqual(3f, fireball.Feature(LevelFeatureKind.LeaveZone, 4), "Od +4 płonąca ziemia");
            var chain = fx.Get<SpellDefinition>("spell_chain");
            Assert.AreEqual(2f, chain.Feature(LevelFeatureKind.ExtraTargets, 4), "Łańcuch +4: dwa cele więcej (6)");

            StringAssert.Contains("od +4", Describe.Spell(fireball, 0, null, fx.Cfg.balance), "Opis zapowiada cechę");
            StringAssert.Contains("✓ +4", Describe.Spell(fireball, 4, null, fx.Cfg.balance), "Opis potwierdza odblokowaną cechę");

            var run = fx.NewRun("class_mage");
            var inst = run.LearnSpell(fireball);
            inst.level = 3;
            var upgrade = new RewardOption { kind = RewardKind.UpgradeSpell, spellToUpgrade = inst };
            StringAssert.Contains("nowa cecha", Describe.Reward(upgrade, run, null, fx.Cfg.balance), "Nagroda mówi, co realnie dochodzi");
        }

        [Test]
        public void LevelFeatures_AreDefinedForKeySkills()
        {
            foreach (var id in new[] { "spell_bolt", "spell_fireball", "spell_chain", "spell_frostcone", "spell_meteor", "spell_storm", "skill_whirlwind", "skill_rend", "skill_knives" })
                Assert.IsNotEmpty(fx.Get<SpellDefinition>(id).levelFeatures, id);
        }

        // ------------------------------------------------------------------ B. Treść

        [Test]
        public void Content_SetsAreComplete()
        {
            string[] weapons = { "weapon_flamesword", "weapon_frostaxe", "weapon_stormhammer", "weapon_serratedsword", "weapon_mace", "weapon_staff_fire", "weapon_staff_frost", "weapon_staff_storm" };
            string[] items = { "ring_embers", "ring_frostvein", "amulet_conductor", "amulet_salamander", "hands_butcher", "feet_storm", "body_ash", "head_fur", "body_grounded" };
            string[] skills = { "spell_spark", "spell_icelance", "spell_meteor", "spell_storm", "spell_bloodpact", "spell_frostarmor", "spell_chains",
                "skill_counter", "skill_leapslash", "skill_execute", "skill_warcry", "skill_rupture" };
            foreach (var id in weapons.Concat(items))
            {
                var it = fx.Get<ItemDefinition>(id);
                Assert.IsTrue(fx.Cfg.baseItemPool.Contains(it) || fx.Cfg.unlocks.Exists(u => u.target == it), id + " jest do zdobycia");
                Assert.IsFalse(string.IsNullOrEmpty(Describe.Item(new ItemInstance(it), null, fx.Cfg.balance)));
            }
            foreach (var id in skills) Assert.IsNotNull(fx.Get<SpellDefinition>(id), id);
        }

        [Test]
        public void EnemyVariants_AreElemental_AndDoNotChangeOriginals()
        {
            var ghoul = fx.Get<EnemyDefinition>("enemy_ghoul");
            var fire = fx.Get<EnemyDefinition>("enemy_ghoul_fire");
            Assert.Contains(fire, ghoul.variants);
            Assert.AreEqual(Element.Fire, fire.attacks[0].attack.element);
            Assert.AreEqual(StatusKind.Burn, fire.attacks[0].attack.statuses[0].kind);
            Assert.AreEqual(Element.None, ghoul.attacks[0].attack.element, "Oryginał bez zmian");
            Assert.AreEqual(StatusKind.Bleed, ghoul.attacks[0].attack.statuses[0].kind);
            Assert.Greater(fire.frostMultiplier, 1f);
            Assert.IsNotEmpty(fx.Get<EnemyDefinition>("enemy_warden").variants);
            Assert.IsNotEmpty(fx.Get<EnemyDefinition>("enemy_heretic").variants);
        }
    }
}
