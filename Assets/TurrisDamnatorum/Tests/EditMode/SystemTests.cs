using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Turris.Tests
{
    public class DamageResolverTests
    {
        static readonly BalanceConfig Rules = new BalanceConfig();

        static DefenseState Def() => new DefenseState { position = Vector3.zero, forward = Vector3.forward, stamina = 100 };

        [Test]
        public void GuardOnlyWorksWithinAngle()
        {
            var d = Def();
            d.guarding = true;
            d.guard = new GuardData { physicalReduction = 1f, blockAngle = 120f, stabilityMultiplier = 1f };
            Assert.AreEqual(HitOutcome.Blocked, DamageResolver.Resolve(Fixture.FrontHit(), d, Rules).outcome);
            Assert.AreEqual(HitOutcome.Hit, DamageResolver.Resolve(Fixture.BackHit(), d, Rules).outcome);
        }

        [Test]
        public void UnblockableButParryable_IsIndependent()
        {
            var d = Def();
            d.guarding = true;
            d.guard = new GuardData { physicalReduction = 1f, blockAngle = 180f };
            var hit = Fixture.FrontHit(blockable: false, parryable: true);
            Assert.AreEqual(HitOutcome.Hit, DamageResolver.Resolve(hit, d, Rules).outcome);

            var p = Def();
            p.parryActive = true;
            p.parry = new ParryData();
            Assert.AreEqual(HitOutcome.Parried, DamageResolver.Resolve(hit, p, Rules).outcome);
        }

        [Test]
        public void UnparryableButBlockable_IsBlocked()
        {
            var d = Def();
            d.guarding = true;
            d.guard = new GuardData { physicalReduction = 1f, blockAngle = 180f };
            Assert.AreEqual(HitOutcome.Blocked, DamageResolver.Resolve(Fixture.FrontHit(parryable: false), d, Rules).outcome);
        }

        [Test]
        public void HeavyAttacks_CostMoreStaminaToBlock()
        {
            var d = Def();
            d.guarding = true;
            d.guard = new GuardData { physicalReduction = 1f, blockAngle = 180f, stabilityMultiplier = 0.5f };
            var light = DamageResolver.Resolve(Fixture.FrontHit(guardLoad: 20), d, Rules);
            var heavy = DamageResolver.Resolve(Fixture.FrontHit(guardLoad: 70), d, Rules);
            Assert.AreEqual(10f, light.staminaDamage, 0.001f);
            Assert.AreEqual(35f, heavy.staminaDamage, 0.001f);
        }

        [Test]
        public void GuardBreakRule_IsMaxOfLeakedAndFactor()
        {
            var d = Def();
            d.guarding = true;
            d.stamina = 1f;
            d.guard = new GuardData { physicalReduction = 0.6f, blockAngle = 180f, stabilityMultiplier = 1f };
            var r = DamageResolver.Resolve(Fixture.FrontHit(100, guardLoad: 30), d, Rules);
            Assert.AreEqual(HitOutcome.GuardBroken, r.outcome);
            Assert.AreEqual(Mathf.Max(40f, 100f * Rules.guardBreakDamageFactor), r.healthDamage, 0.001f);
            Assert.AreEqual(1f, r.staminaDamage, 0.001f);
        }

        [Test]
        public void ArmorReducesEachDamageTypeSeparately()
        {
            var d = Def();
            d.physicalResist = 0.5f;
            d.magicResist = 0f;
            var r = DamageResolver.Resolve(Fixture.FrontHit(100, 100), d, Rules);
            Assert.AreEqual(150f, r.healthDamage, 0.001f);
        }

        [Test]
        public void DeadTarget_IsIgnored()
        {
            var d = Def();
            d.dead = true;
            Assert.AreEqual(HitOutcome.Ignored, DamageResolver.Resolve(Fixture.FrontHit(), d, Rules).outcome);
        }
    }

    public class ActionRulesTests
    {
        [Test]
        public void TransitionTable()
        {
            Assert.IsTrue(ActionRules.CanTransition(ActionType.None, ActionType.Flask, false));
            Assert.IsFalse(ActionRules.CanTransition(ActionType.Block, ActionType.Flask, true), "Z gardy nie pije się flaszki");
            Assert.IsTrue(ActionRules.CanTransition(ActionType.Block, ActionType.Parry, false));
            Assert.IsTrue(ActionRules.CanTransition(ActionType.LightAttack, ActionType.Dodge, false), "Unik przerywa zamach w każdej fazie");
            Assert.IsTrue(ActionRules.CanTransition(ActionType.HeavyAttack, ActionType.Dodge, false));
            Assert.IsTrue(ActionRules.CanTransition(ActionType.Cast, ActionType.Dodge, false), "Unik przerywa inkantację");
            foreach (var to in new[] { ActionType.LightAttack, ActionType.HeavyAttack, ActionType.Parry, ActionType.Block, ActionType.Cast, ActionType.Flask })
                Assert.IsFalse(ActionRules.CanTransition(ActionType.LightAttack, to, false), $"Zamach bez przerwania: {to}");
            Assert.IsTrue(ActionRules.CanTransition(ActionType.LightAttack, ActionType.Parry, true));
            Assert.IsFalse(ActionRules.CanTransition(ActionType.LightAttack, ActionType.Flask, true));
            // Unik: w całości do punktu przerwania, potem dowolna akcja poza flaszką.
            foreach (var to in new[] { ActionType.LightAttack, ActionType.Block, ActionType.Dodge, ActionType.Parry, ActionType.Flask, ActionType.Cast })
                Assert.IsFalse(ActionRules.CanTransition(ActionType.Dodge, to, false), $"Dodge → {to}");
            Assert.IsTrue(ActionRules.CanTransition(ActionType.Dodge, ActionType.LightAttack, true));
            Assert.IsTrue(ActionRules.CanTransition(ActionType.Dodge, ActionType.Dodge, true));
            Assert.IsFalse(ActionRules.CanTransition(ActionType.Dodge, ActionType.Flask, true));
            foreach (var locked in new[] { ActionType.Parry, ActionType.Flask, ActionType.Flinch, ActionType.GuardBroken, ActionType.Riposte, ActionType.Dead })
                foreach (var to in new[] { ActionType.LightAttack, ActionType.Block, ActionType.Dodge, ActionType.Parry, ActionType.Flask, ActionType.Cast })
                    Assert.IsFalse(ActionRules.CanTransition(locked, to, true), $"{locked} → {to}");
        }

        [Test]
        public void PhasesFireInOrder_EvenWithLargeTimeStep()
        {
            var c = new ActionController();
            var log = new List<ActionPhase>();
            c.PhaseChanged += (t, p) => log.Add(p);
            c.TryStart(ActionType.LightAttack, 0.2f, 0.1f, 0.3f);
            c.Tick(5f);
            CollectionAssert.AreEqual(new[] { ActionPhase.Startup, ActionPhase.Active, ActionPhase.Recovery, ActionPhase.Finished }, log);
            Assert.IsTrue(c.IsIdle);
        }

        [Test]
        public void HitTracker_RegistersTargetOnce()
        {
            var t = new HitTracker();
            var go = new GameObject("dummy");
            var target = go.AddComponent<PlayerCombat>();
            Assert.IsTrue(t.TryRegister(target));
            Assert.IsFalse(t.TryRegister(target));
            t.Reset();
            Assert.IsTrue(t.TryRegister(target));
            Object.DestroyImmediate(go);
        }
    }

    public class EquipmentTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        [Test]
        public void EquipUnequip_DoesNotAccumulateBonuses()
        {
            var run = fx.NewRun("legacy_knight");
            float baseHp = BuildCalculator.Compute(run, fx.Cfg).MaxHealth;
            var ring = new ItemInstance(fx.Get<ItemDefinition>("ring_blood"));
            run.inventory.Add(ring);
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(run.EquipFromInventory(ring, EquipSlot.Ring1));
                Assert.AreEqual(baseHp + 2 * fx.Cfg.balance.healthPerToughness, BuildCalculator.Compute(run, fx.Cfg).MaxHealth, 0.01f);
                Assert.IsTrue(run.EquipFromInventory(ring, EquipSlot.Ring2), "Przełożenie do drugiego slotu");
                Assert.IsNull(run.equipment.Get(EquipSlot.Ring1));
                Assert.AreEqual(baseHp + 2 * fx.Cfg.balance.healthPerToughness, BuildCalculator.Compute(run, fx.Cfg).MaxHealth, 0.01f);
                Assert.IsTrue(run.UnequipToInventory(EquipSlot.Ring2));
                Assert.AreEqual(baseHp, BuildCalculator.Compute(run, fx.Cfg).MaxHealth, 0.01f);
            }
            Assert.AreEqual(1, run.inventory.Count(i => i == ring), "Przedmiot nie dubluje się w plecaku");
        }

        [Test]
        public void PercentBonuses_FromDifferentSourcesStack()
        {
            var run = fx.NewRun("legacy_mage"); // kostur: +15% mocy czarów
            run.AddItem(new ItemInstance(fx.Get<ItemDefinition>("head_hood")), true); // +8%
            run.AddBoon(fx.Get<BoonDefinition>("boon_arcane")); // +12%
            Assert.AreEqual(35f, BuildCalculator.Compute(run, fx.Cfg).stats[StatType.SpellPower], 0.01f);
        }

        [Test]
        public void SlotCompatibility_IsEnforced()
        {
            var run = fx.NewRun("legacy_knight");
            var ring = new ItemInstance(fx.Get<ItemDefinition>("ring_blood"));
            run.inventory.Add(ring);
            Assert.IsFalse(run.EquipFromInventory(ring, EquipSlot.Head));
            Assert.IsTrue(run.inventory.Contains(ring));
            var shield = new ItemInstance(fx.Get<ItemDefinition>("shield_buckler"));
            Assert.IsFalse(run.equipment.CanEquip(shield, EquipSlot.MainHand, out _));
        }

        [Test]
        public void TwoHandedWeapon_FreesOffHand()
        {
            var run = fx.NewRun("legacy_knight");
            var ga = new ItemInstance(fx.Get<ItemDefinition>("weapon_greataxe"));
            run.inventory.Add(ga);
            Assert.IsTrue(run.EquipFromInventory(ga, EquipSlot.MainHand));
            Assert.IsNull(run.equipment.Get(EquipSlot.OffHand));
            Assert.IsTrue(run.inventory.Any(i => i.definition.id == "shield_heater"));
            Assert.IsTrue(run.inventory.Any(i => i.definition.id == "weapon_longsword"));

            var snap = BuildCalculator.Compute(run, fx.Cfg);
            Assert.IsTrue(snap.CanBlock, "Broń dwuręczna może blokować");
            Assert.Less(snap.weaponEffectiveness, 1f, "Siła 14 < 16: niespełnione wymagania obniżają skuteczność zamiast blokować użycie");

            var shield = run.inventory.First(i => i.definition.id == "shield_heater");
            Assert.IsTrue(run.EquipFromInventory(shield, EquipSlot.OffHand));
            Assert.IsNull(run.equipment.Get(EquipSlot.MainHand), "Tarcza wypiera broń dwuręczną");
            Assert.AreEqual(fx.Cfg.unarmed, BuildCalculator.Compute(run, fx.Cfg).mainHand.definition, "Pusta ręka = pięści");
        }

        [Test]
        public void HeavyLoad_WorsensDodge()
        {
            var run = fx.NewRun("legacy_mage");
            var light = BuildCalculator.Compute(run, fx.Cfg).dodge;
            foreach (var id in new[] { "body_plate", "weapon_axe", "shield_great", "feet_iron", "head_helm" })
            {
                var it = new ItemInstance(fx.Get<ItemDefinition>(id));
                run.inventory.Add(it);
                run.EquipFromInventory(it, it.definition.DefaultSlot);
            }
            var heavy = BuildCalculator.Compute(run, fx.Cfg);
            Assert.Greater(heavy.loadRatio, fx.Cfg.balance.heavyLoadThreshold);
            Assert.Less(heavy.dodge.distance, light.distance);
            Assert.Less(heavy.dodge.invulnDuration, light.invulnDuration);
        }
    }

    public class RewardTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        RewardPools Pools()
        {
            var p = new RewardPools();
            p.items.AddRange(fx.Cfg.baseItemPool);
            p.boons.AddRange(fx.Cfg.baseBoonPool);
            p.spells.AddRange(fx.Cfg.baseSpellPool);
            return p;
        }

        [Test]
        public void OffersItemBoonAndSpell()
        {
            var run = fx.NewRun("legacy_knight");
            var opts = RewardGenerator.Generate(run, BuildCalculator.Compute(run, fx.Cfg), Pools(), fx.Cfg, new System.Random(1));
            Assert.AreEqual(3, opts.Count);
            Assert.AreEqual(RewardKind.Item, opts[0].kind);
            Assert.AreEqual(RewardKind.Boon, opts[1].kind);
            Assert.IsTrue(opts[2].kind == RewardKind.LearnSpell || opts[2].kind == RewardKind.UpgradeSpell, "Trzecia nagroda to umiejętność (nowa lub ulepszenie)");
        }

        [Test]
        public void MageIsOfferedMeleeGear_NoClassFilter()
        {
            var seen = new HashSet<string>();
            for (int seed = 0; seed < 300; seed++)
            {
                var run = fx.NewRun("legacy_mage");
                var opts = RewardGenerator.Generate(run, BuildCalculator.Compute(run, fx.Cfg), Pools(), fx.Cfg, new System.Random(seed));
                seen.Add(opts[0].item.definition.id);
            }
            Assert.Contains("weapon_axe", seen.ToList());
            Assert.Contains("shield_heater", seen.ToList());
            Assert.Contains("body_plate", seen.ToList());
        }

        [Test]
        public void SpellUpgrade_IncreasesPowerAndApplies()
        {
            var run = fx.NewRun("legacy_mage");
            var spell = run.knownSpells[0];
            float before = BuildCalculator.ComputeSpell(spell, BuildCalculator.Compute(run, fx.Cfg).stats, fx.Cfg.balance).power;
            new RewardOption { kind = RewardKind.UpgradeSpell, spellToUpgrade = spell }.Apply(run);
            float after = BuildCalculator.ComputeSpell(spell, BuildCalculator.Compute(run, fx.Cfg).stats, fx.Cfg.balance).power;
            Assert.AreEqual(1, spell.level);
            Assert.Greater(after, before);
        }

        [Test]
        public void BuildTags_ComeFromGearNotClass()
        {
            var run = fx.NewRun("legacy_mage");
            var tagsBefore = run.CurrentTags(BuildCalculator.Compute(run, fx.Cfg));
            Assert.IsFalse(tagsBefore.HasFlag(BuildTag.Shield));
            var sh = new ItemInstance(fx.Get<ItemDefinition>("shield_heater"));
            run.AddItem(sh, true);
            var tags = run.CurrentTags(BuildCalculator.Compute(run, fx.Cfg));
            Assert.IsTrue(tags.HasFlag(BuildTag.Shield));
            Assert.IsTrue(tags.HasFlag(BuildTag.Parry));
        }
    }

    public class ProfileTests
    {
        [Test]
        public void SaveLoad_RoundTrip()
        {
            var storage = new MemoryProfileStorage();
            var p = new ProfileData { ash = 42, bestFloor = 3 };
            p.unlocked.Add("unlock_dagger");
            ProfileSerializer.Save(storage, p);
            var loaded = ProfileSerializer.Load(storage, out var warn, out var allow);
            Assert.IsNull(warn);
            Assert.IsTrue(allow);
            Assert.AreEqual(42, loaded.ash);
            Assert.AreEqual(3, loaded.bestFloor);
            Assert.IsTrue(loaded.IsUnlocked("unlock_dagger"));
            StringAssert.Contains("\"version\": " + ProfileData.CurrentVersion, storage.Content);
        }

        [Test]
        public void MigratesVersion2_WithDefaultVolumes()
        {
            var storage = new MemoryProfileStorage { Content = "{\"version\":2,\"ash\":7,\"masterVolume\":0,\"musicVolume\":0,\"sfxVolume\":0}" };
            var p = ProfileSerializer.Load(storage, out _, out _);
            Assert.AreEqual(ProfileData.CurrentVersion, p.version);
            Assert.AreEqual(7, p.ash);
            Assert.Greater(p.masterVolume, 0.5f, "Stary profil dostaje domyślną głośność");
            Assert.Greater(p.musicVolume, 0.5f);
            Assert.Greater(p.sfxVolume, 0.5f);
            // Ustawienie użytkownika (także cisza) przetrwa zapis w nowej wersji.
            p.musicVolume = 0f;
            ProfileSerializer.Save(storage, p);
            Assert.AreEqual(0f, ProfileSerializer.Load(storage, out _, out _).musicVolume);
        }

        [Test]
        public void MigratesVersion1()
        {
            var storage = new MemoryProfileStorage { Content = "{\"version\":1,\"ash\":50,\"unlocked\":[\"unlock_heal\"],\"bestFloor\":5,\"totalVictories\":1}" };
            var p = ProfileSerializer.Load(storage, out _, out _);
            Assert.AreEqual(ProfileData.CurrentVersion, p.version);
            Assert.AreEqual(50, p.ash);
            Assert.Contains(0, p.victoryTiers);
            Assert.IsNotNull(p.lastLoadout);
        }

        [Test]
        public void CorruptedSave_StartsFreshAndKeepsBackup()
        {
            var storage = new MemoryProfileStorage { Content = "{to nie jest json" };
            var p = ProfileSerializer.Load(storage, out var warn, out var allow);
            Assert.IsNotNull(warn);
            Assert.AreEqual(0, p.ash);
            Assert.AreEqual("{to nie jest json", storage.Backup);
            Assert.IsTrue(allow);
        }

        [Test]
        public void NewerVersion_IsNotOverwritten()
        {
            var storage = new MemoryProfileStorage { Content = "{\"version\":99,\"ash\":7}" };
            ProfileSerializer.Load(storage, out var warn, out var allow);
            Assert.IsFalse(allow);
            Assert.IsNotNull(warn);
        }
    }

    public class MetaTests
    {
        Fixture fx;
        [SetUp] public void SetUp() => fx = new Fixture();
        [TearDown] public void TearDown() => fx.Cleanup();

        [Test]
        public void UnlockPersists_AcrossRestart()
        {
            var storage = new MemoryProfileStorage();
            var meta = new MetaService(fx.Cfg, storage);
            meta.DebugAddAsh(100);
            var dagger = fx.Get<UnlockDefinition>("unlock_dagger");
            Assert.IsTrue(meta.Purchase(dagger));
            Assert.AreEqual(100 - dagger.ashCost, meta.Profile.ash);

            var restarted = new MetaService(fx.Cfg, storage);
            Assert.IsTrue(restarted.IsUnlocked(dagger));
            Assert.AreEqual(100 - dagger.ashCost, restarted.Profile.ash);
            Assert.IsTrue(restarted.BuildRewardPools().items.Contains(fx.Get<ItemDefinition>("weapon_dagger")));
        }

        [Test]
        public void AshReward_ScalesWithFloorAndDifficulty_FirstClearBonusOnce()
        {
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            var run0 = fx.NewRun("legacy_knight", 0);
            int first = meta.AwardFloorClear(run0, 0);
            int again = meta.AwardFloorClear(run0, 0);
            Assert.Greater(first, again);
            int floor3 = meta.AwardFloorClear(run0, 2);
            Assert.Greater(floor3, again, "Wyższe piętro = więcej popiołu");

            var run1 = fx.NewRun("legacy_knight", 1);
            meta.AwardFloorClear(run1, 2); // zużyj premię pierwszego ukończenia
            int t1 = meta.AwardFloorClear(run1, 2);
            int t0 = meta.AwardFloorClear(run0, 2);
            Assert.Greater(t1, t0, "Wyższa trudność = więcej popiołu");

            var floors = fx.Cfg.tower.floors;
            for (int i = 1; i < floors.Count; i++) Assert.Greater(floors[i].ashReward, floors[i - 1].ashReward);
            int fullRun = floors.Sum(f => f.ashReward) + fx.Cfg.tower.victoryAshBonus;
            Assert.Greater(fullRun, floors[0].ashReward * 10, "Farmienie 1. piętra się nie opłaca");
        }

        [Test]
        public void LoadoutBudget_IsEnforced()
        {
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            meta.DebugAddAsh(100);
            Assert.IsTrue(meta.Purchase(fx.Get<UnlockDefinition>("unlock_ring_riposte")));
            var plan = fx.Plan("class_knight");
            plan.extras.Add(fx.Get<UnlockDefinition>("unlock_axe"));
            plan.extras.Add(fx.Get<UnlockDefinition>("unlock_ring_riposte"));
            Assert.IsTrue(meta.ValidateLoadout(plan, out var ok), ok);
            plan.extras.Add(fx.Get<UnlockDefinition>("unlock_vigilance"));
            Assert.IsFalse(meta.ValidateLoadout(plan, out var reason), reason);
            StringAssert.Contains("budżet", reason);

            var locked = fx.Plan("class_knight");
            locked.extras.Add(fx.Get<UnlockDefinition>("unlock_heal"));
            Assert.IsFalse(meta.ValidateLoadout(locked, out _), "Nieodblokowany czar nie może być wybrany");
        }

        [Test]
        public void DifficultyUnlock_RequiresProgressAndAsh()
        {
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            var d1 = fx.Get<UnlockDefinition>("unlock_diff1");
            var diff1 = fx.Cfg.difficulties[1];
            Assert.IsFalse(meta.IsDifficultyAvailable(diff1));
            meta.DebugAddAsh(100);
            Assert.IsFalse(meta.CanPurchase(d1, out _), "Wymaga dotarcia na piętro 3");
            meta.RecordReachedFloor(2);
            Assert.IsTrue(meta.Purchase(d1));
            Assert.IsTrue(meta.IsDifficultyAvailable(diff1));
        }

        [Test]
        public void KnightStartsWithOwnSkills_MageWithDefaultSpell()
        {
            // Pocisk arkanów jest odblokowany domyślnie, ale tylko dla maga – wojownik zaczyna z własnymi technikami.
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            var plan = meta.LastPlan();
            meta.SelectClass(plan, fx.Get<ClassDefinition>("class_knight"));
            Assert.AreEqual(0, plan.Cost);
            Assert.IsFalse(meta.LoadoutOptions.Any(u => u.kind == UnlockKind.Spell), "Umiejętności nie zajmują budżetu");
            var run = RunFactory.Create(plan, fx.Cfg, 1);
            Assert.IsNull(run.FindSpell(fx.Get<SpellDefinition>("spell_bolt")), "Wojownik nie zaczyna z czarem maga");
            CollectionAssert.AreEquivalent(new[] { "hu_cleave", "skill_stomp" }, run.knownSpells.Select(s => s.definition.id).ToArray(),
                "Skill startowy (Rozpłatanie) i Tąpnięcie");
            Assert.AreEqual("hu_warcry", run.signatureSkill.definition.id, "Skill klasy pod osobnym przyciskiem");

            meta.SelectClass(plan, fx.Get<ClassDefinition>("class_mage"));
            var mageRun = RunFactory.Create(plan, fx.Cfg, 2);
            Assert.IsNotNull(mageRun.FindSpell(fx.Get<SpellDefinition>("hu_nova_fire")), "Grymuar ognia daje Novę ognia");
            var bolt = mageRun.FindSpell(fx.Get<SpellDefinition>("spell_bolt"));
            Assert.IsNotNull(bolt);
            Assert.IsTrue(bolt.permanent);
        }

        [Test]
        public void HarderDifficulty_ReducesFlasks()
        {
            var easy = fx.NewRun("legacy_knight", 0);
            var hard = fx.NewRun("legacy_knight", 2);
            Assert.AreEqual(easy.MaxHealthFlasks - 1, hard.MaxHealthFlasks);
        }
    }
}

namespace Turris.Tests
{
    public class HitQueryTests
    {
        class Dummy : MonoBehaviour, IHitReceiver
        {
            public Faction Faction => Faction.Enemy;
            public bool IsDead => false;
            public Transform Transform => transform;
            public Vector3 AimPoint => transform.position;
            public HitResult ReceiveHit(HitData hit) => default;
            public void OnAttackParried(float poiseDamage) { }
            public void OnHitResolved(IHitReceiver target, in HitData hit, in HitResult result) { }
        }

        /// <summary>Trafienie może wywołać kolejne zapytanie (wybuch, efekt „przy trafieniu”) – zewnętrzne nie gubi celów ani nie rzuca wyjątku.</summary>
        [Test]
        public void NestedQuery_InsideOnHit_KeepsOuterTargets()
        {
            var made = new List<GameObject>();
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.transform.position = new Vector3(1000f + 2f * i, 0f, 1000f);
                    go.AddComponent<Dummy>();
                    made.Add(go);
                }
                Physics.SyncTransforms();
                var outer = new List<IHitReceiver>();
                int inner = 0;
                HitQuery.Sphere(new Vector3(1002f, 0f, 1000f), 3f, Faction.Player, new HitTracker(), r =>
                {
                    outer.Add(r);
                    HitQuery.Sphere(r.Transform.position, 0.6f, Faction.Player, new HitTracker(), _ => inner++);
                });
                Assert.AreEqual(3, outer.Distinct().Count(), "każdy cel zewnętrznego zapytania trafiony raz");
                Assert.AreEqual(3, inner, "zagnieżdżone zapytania działają");
            }
            finally { foreach (var go in made) Object.DestroyImmediate(go); }
        }
    }
}
