using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>Plan przygotowania podejścia: klasa, trudność i dodatkowe odblokowania w ramach budżetu.</summary>
    public class LoadoutPlan
    {
        public ClassDefinition classDef;
        public DifficultyDefinition difficulty;
        public readonly List<UnlockDefinition> extras = new List<UnlockDefinition>();
        public int Cost => extras.Sum(e => e.loadoutCost);
    }

    /// <summary>Trwały postęp: odblokowania, waluta (popiół), nagrody za piętra, trudności.</summary>
    public class MetaService
    {
        readonly GameConfig cfg;
        readonly IProfileStorage storage;
        readonly bool allowSave;
        public ProfileData Profile { get; private set; }
        public string Warning { get; private set; }

        public MetaService(GameConfig cfg, IProfileStorage storage)
        {
            this.cfg = cfg;
            this.storage = storage;
            Profile = ProfileSerializer.Load(storage, out var warning, out allowSave);
            Warning = warning;
        }

        public void Save()
        {
            if (!allowSave) return;
            try { ProfileSerializer.Save(storage, Profile); }
            catch (System.Exception e) { Warning = "Błąd zapisu profilu: " + e.Message; Debug.LogError(e); }
        }

        // ------------------------------------------------------------------ Odblokowania

        public bool IsUnlocked(UnlockDefinition u) => u != null && (u.unlockedByDefault || Profile.IsUnlocked(u.id));

        public bool CanPurchase(UnlockDefinition u, out string reason)
        {
            reason = null;
            if (IsUnlocked(u)) { reason = "Odblokowane"; return false; }
            if (u.requiredBestFloor > 0 && Profile.bestFloor < u.requiredBestFloor) { reason = $"Wymaga dotarcia na piętro {u.requiredBestFloor}"; return false; }
            if (u.requiredVictoryTier >= 0 && !Profile.victoryTiers.Any(t => t >= u.requiredVictoryTier)) { reason = $"Wymaga zwycięstwa na trudności {u.requiredVictoryTier}+"; return false; }
            if (Profile.ash < u.ashCost) { reason = $"Brak popiołu ({Profile.ash}/{u.ashCost})"; return false; }
            return true;
        }

        public bool Purchase(UnlockDefinition u)
        {
            if (!CanPurchase(u, out _)) return false;
            Profile.ash -= u.ashCost;
            Profile.unlocked.Add(u.id);
            Save();
            return true;
        }

        public bool IsDifficultyAvailable(DifficultyDefinition d)
        {
            if (d.tier == 0) return true;
            var u = cfg.unlocks.FirstOrDefault(x => x.kind == UnlockKind.Difficulty && x.target == d);
            return u == null || IsUnlocked(u);
        }

        public IEnumerable<UnlockDefinition> LoadoutOptions =>
            cfg.unlocks.Where(u => u.kind != UnlockKind.Difficulty && IsUnlocked(u));

        public RewardPools BuildRewardPools()
        {
            var pools = new RewardPools();
            pools.items.AddRange(cfg.baseItemPool);
            pools.boons.AddRange(cfg.baseBoonPool);
            pools.spells.AddRange(cfg.baseSpellPool);
            foreach (var u in cfg.unlocks.Where(IsUnlocked))
            {
                switch (u.target)
                {
                    case ItemDefinition i when !pools.items.Contains(i): pools.items.Add(i); break;
                    case BoonDefinition b when !pools.boons.Contains(b): pools.boons.Add(b); break;
                    case SpellDefinition s when !pools.spells.Contains(s): pools.spells.Add(s); break;
                }
            }
            return pools;
        }

        public bool ValidateLoadout(LoadoutPlan plan, out string reason)
        {
            reason = null;
            if (plan.classDef == null) { reason = "Wybierz klasę"; return false; }
            if (plan.difficulty == null || !IsDifficultyAvailable(plan.difficulty)) { reason = "Niedostępna trudność"; return false; }
            if (plan.Cost > cfg.balance.loadoutBudget) { reason = $"Przekroczony budżet ({plan.Cost}/{cfg.balance.loadoutBudget})"; return false; }
            if (plan.extras.Any(e => !IsUnlocked(e))) { reason = "Wybrano zablokowany element"; return false; }
            return true;
        }

        // ------------------------------------------------------------------ Nagrody trwałe

        public static string FloorKey(int tier, int floorIndex) => $"t{tier}_f{floorIndex}";

        /// <summary>
        /// Popiół za ukończenie piętra: nagroda rośnie z numerem piętra i mnożnikiem trudności,
        /// a pierwsze ukończenie danego piętra na danej trudności daje jednorazową premię +50%.
        /// Powtarzanie tylko pierwszego piętra daje więc najmniej popiołu na minutę.
        /// </summary>
        public int AwardFloorClear(RunState run, int floorIndex)
        {
            var floor = cfg.tower.floors[floorIndex];
            float amount = floor.ashReward * run.difficulty.ashMultiplier;
            string key = FloorKey(run.difficulty.tier, floorIndex);
            if (!Profile.clearedFloorKeys.Contains(key))
            {
                amount *= 1.5f;
                Profile.clearedFloorKeys.Add(key);
            }
            int ash = Mathf.RoundToInt(amount);
            Profile.ash += ash;
            run.ashEarned += ash;
            Profile.bestFloor = Mathf.Max(Profile.bestFloor, floorIndex + 1);
            Save();
            return ash;
        }

        public int AwardVictory(RunState run)
        {
            bool first = !Profile.victoryTiers.Contains(run.difficulty.tier);
            int ash = Mathf.RoundToInt(cfg.tower.victoryAshBonus * run.difficulty.ashMultiplier * (first ? 2f : 1f));
            Profile.ash += ash;
            run.ashEarned += ash;
            if (first) Profile.victoryTiers.Add(run.difficulty.tier);
            Profile.totalVictories++;
            Save();
            return ash;
        }

        public void RecordReachedFloor(int floorIndex)
        {
            if (floorIndex + 1 > Profile.bestFloor) { Profile.bestFloor = floorIndex + 1; Save(); }
        }

        public void RecordRunStart(LoadoutPlan plan)
        {
            Profile.totalRuns++;
            Profile.lastClassId = plan.classDef.id;
            Profile.lastDifficultyTier = plan.difficulty.tier;
            Profile.lastLoadout = plan.extras.Select(e => e.id).ToList();
            Save();
        }

        public void RecordDeath()
        {
            Profile.totalDeaths++;
            Save();
        }

        public LoadoutPlan LastPlan()
        {
            var plan = new LoadoutPlan
            {
                classDef = cfg.classes.FirstOrDefault(c => c.id == Profile.lastClassId) ?? cfg.classes.FirstOrDefault(),
                difficulty = cfg.difficulties.FirstOrDefault(d => d.tier == Profile.lastDifficultyTier && IsDifficultyAvailable(d)) ?? cfg.difficulties.FirstOrDefault(),
            };
            foreach (var id in Profile.lastLoadout)
            {
                var u = cfg.unlocks.FirstOrDefault(x => x.id == id);
                if (u != null && IsUnlocked(u) && plan.Cost + u.loadoutCost <= cfg.balance.loadoutBudget) plan.extras.Add(u);
            }
            return plan;
        }

        /// <summary>Narzędzie deweloperskie (F9 w menu).</summary>
        public void DebugAddAsh(int amount) { Profile.ash += amount; Save(); }
    }

    public static class RunFactory
    {
        public static RunState Create(LoadoutPlan plan, GameConfig cfg, int seed)
        {
            var run = new RunState
            {
                startingClass = plan.classDef,
                difficulty = plan.difficulty,
                attributes = AttributeBlock.From(plan.classDef),
                seed = seed,
            };
            int maxAttuned = cfg.balance.maxAttunedSpells;
            foreach (var i in plan.classDef.startingItems) if (i != null) run.AddItem(new ItemInstance(i), true);
            foreach (var s in plan.classDef.startingSpells) if (s != null) run.LearnSpell(s, maxAttuned);
            foreach (var t in plan.classDef.startingTalents) if (t != null) run.AddBoon(t);
            foreach (var u in plan.extras)
            {
                switch (u.target)
                {
                    case ItemDefinition i: run.AddItem(new ItemInstance(i), true); break;
                    case SpellDefinition s: run.LearnSpell(s, maxAttuned); break;
                    case BoonDefinition b: run.AddBoon(b); break;
                }
            }
            run.RefillFlasks();
            return run;
        }
    }
}
