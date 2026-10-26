using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Plan przygotowania podejścia: klasa, trudność, dodatki w ramach budżetu
    /// oraz umiejętności odblokowane na stałe i ich układ w slotach.
    /// </summary>
    public class LoadoutPlan
    {
        public ClassDefinition classDef;
        /// <summary>Wyłącznie sylwetka; nie wpływa na atrybuty, cechę, broń ani skille.</summary>
        public ClassDefinition appearance;
        public EnemyDefinition trialEnemy;
        public DifficultyDefinition difficulty;
        /// <summary>Wybrana broń startowa (null = domyślna klasy). Liczy się tylko, jeśli klasa ją oferuje.</summary>
        public ItemDefinition startingWeapon;

        /// <summary>Wybrany zestaw broni na start (klasy z Hell Unleashed): indeks w <see cref="ClassDefinition.loadouts"/>.</summary>
        public int loadoutIndex;
        /// <summary>Wybrany skill startowy (jeden z <see cref="ClassDefinition.startingSkillChoices"/>).</summary>
        public SpellDefinition startingSkill;

        /// <summary>Zestaw broni na start (null, gdy klasa ich nie definiuje).</summary>
        public StartingLoadout Loadout => classDef == null || classDef.loadouts.Count == 0 ? null
            : classDef.loadouts[Mathf.Clamp(loadoutIndex, 0, classDef.loadouts.Count - 1)];

        /// <summary>Broń, z którą postać zacznie: z zestawu, wybrana (jeśli klasa ją oferuje) albo domyślna.</summary>
        public ItemDefinition Weapon => classDef == null ? null
            : Loadout != null ? Loadout.weapon
            : startingWeapon != null && classDef.WeaponChoices.Contains(startingWeapon) ? startingWeapon : classDef.DefaultWeapon;

        /// <summary>Druga ręka z zestawu (tarcza, grymuar, sztylet do rzucania, druga broń).</summary>
        public ItemDefinition OffHand => Loadout?.offHand;

        /// <summary>Skill z broni startowej (grymuar → nova, sztylet do rzucania → rzut nożami) – wtedy wyboru nie ma.</summary>
        public SpellDefinition GearSkill => OffHand != null && OffHand.grantedSkill != null ? OffHand.grantedSkill
            : Weapon != null ? Weapon.grantedSkill : null;

        /// <summary>Skill startowy: z broni, a gdy broń go nie daje – wybrany z listy klasy.</summary>
        public SpellDefinition OpeningSkill
        {
            get
            {
                if (classDef == null) return null;
                if (GearSkill != null) return GearSkill;
                if (startingSkill != null && classDef.startingSkillChoices.Contains(startingSkill)) return startingSkill;
                return classDef.startingSkillChoices.Count > 0 ? classDef.startingSkillChoices[0] : null;
            }
        }
        public readonly List<UnlockDefinition> extras = new List<UnlockDefinition>();
        /// <summary>Umiejętności odblokowane na stałe – trafiają do kolekcji każdego podejścia (poza budżetem).</summary>
        public readonly List<SpellDefinition> permanentSkills = new List<SpellDefinition>();
        /// <summary>Wybrany układ slotów (null = automatycznie: umiejętności klasy, potem odblokowane).</summary>
        public readonly SpellDefinition[] skillSlots = new SpellDefinition[RunState.SkillSlotCount];
        public int Cost => extras.Sum(e => e.loadoutCost);

        /// <summary>Umiejętności dostępne na starcie: klasowe i odblokowane na stałe (bez powtórzeń).</summary>
        public IEnumerable<SpellDefinition> StartingSkills =>
            (classDef != null ? classDef.startingSpells : new List<SpellDefinition>()).Concat(new[] { OpeningSkill }).Concat(permanentSkills).Where(s => s != null).Distinct();

        /// <summary>Wkłada umiejętność do slotu (jeśli była w innym – zamiana miejscami).</summary>
        public void AssignSlot(SpellDefinition skill, int slot)
        {
            if (slot < 0 || slot >= skillSlots.Length) return;
            int from = System.Array.IndexOf(skillSlots, skill);
            if (from == slot) return;
            if (from >= 0) skillSlots[from] = skillSlots[slot];
            skillSlots[slot] = skill;
        }

        public void ClearSlot(int slot)
        {
            if (slot >= 0 && slot < skillSlots.Length) skillSlots[slot] = null;
        }
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

        /// <summary>Dodatki wybierane w budżecie (wszystkie odblokowane, bez względu na klasę). Umiejętności się tu nie pojawiają.</summary>
        public IEnumerable<UnlockDefinition> LoadoutOptions =>
            cfg.unlocks.Where(u => u.kind != UnlockKind.Difficulty && u.kind != UnlockKind.Spell && IsUnlocked(u));

        /// <summary>
        /// Dodatki dla danej klasy: ogólne i przypisane do niej. Bez przedmiotów, które klasa i tak ma (np. łuk jako broń do wyboru).
        /// </summary>
        public IEnumerable<UnlockDefinition> LoadoutOptionsFor(ClassDefinition c) =>
            LoadoutOptions.Where(u => u.AvailableFor(c) && !(c != null && u.target is ItemDefinition i && (c.startingItems.Contains(i) || c.alternativeWeapons.Contains(i)
                                                                                                         || c.loadouts.Any(l => l.weapon == i || l.offHand == i))));

        /// <summary>Umiejętności odblokowane na stałe (za popiół lub domyślnie), które ta klasa dostaje na start.</summary>
        public IEnumerable<SpellDefinition> PermanentSkillsFor(ClassDefinition c) =>
            cfg.unlocks.Where(u => u.kind == UnlockKind.Spell && IsUnlocked(u) && u.AvailableFor(c)).Select(u => u.target as SpellDefinition).Where(s => s != null);

        /// <summary>Zmiana klasy na ekranie przygotowania: dodatki innej klasy wypadają, umiejętności i broń – od nowa.</summary>
        public void SelectClass(LoadoutPlan plan, ClassDefinition c)
        {
            plan.classDef = c;
            plan.extras.RemoveAll(u => !LoadoutOptionsFor(c).Contains(u));
            if (plan.startingWeapon != null && (c == null || !c.WeaponChoices.Contains(plan.startingWeapon))) plan.startingWeapon = null;
            plan.loadoutIndex = 0;
            if (c != null && plan.startingSkill != null && !c.startingSkillChoices.Contains(plan.startingSkill)) plan.startingSkill = null;
            FillSkills(plan);
        }

        /// <summary>Zmiana zestawu broni na start (np. łuk albo sztylety); skill z broni zastępuje wybór skilla.</summary>
        public void SelectLoadout(LoadoutPlan plan, int index)
        {
            if (plan.classDef == null || plan.classDef.loadouts.Count == 0) return;
            plan.loadoutIndex = Mathf.Clamp(index, 0, plan.classDef.loadouts.Count - 1);
            FillSkills(plan);
        }

        /// <summary>Wybór skilla startowego (gdy broń żadnego nie daje).</summary>
        public void SelectStartingSkill(LoadoutPlan plan, SpellDefinition skill)
        {
            if (plan.classDef == null || !plan.classDef.startingSkillChoices.Contains(skill)) return;
            plan.startingSkill = skill;
            FillSkills(plan);
        }

        /// <summary>Uzupełnia plan o trwałe umiejętności klasy i odtwarza zapamiętany układ slotów.</summary>
        public void FillSkills(LoadoutPlan plan)
        {
            plan.permanentSkills.Clear();
            plan.permanentSkills.AddRange(PermanentSkillsFor(plan.classDef));
            var available = plan.StartingSkills.ToList();
            for (int i = 0; i < plan.skillSlots.Length; i++)
            {
                string id = i < Profile.skillSlots.Count ? Profile.skillSlots[i] : null;
                var s = string.IsNullOrEmpty(id) ? null : available.FirstOrDefault(x => x.id == id);
                plan.skillSlots[i] = s != null && System.Array.IndexOf(plan.skillSlots, s) < 0 ? s : null;
            }
        }

        /// <summary>Zapamiętuje układ slotów (ekran przygotowania i ekwipunek między piętrami).</summary>
        public void RememberSkillSlots(IList<SpellDefinition> slots)
        {
            Profile.skillSlots = slots.Select(s => s != null ? s.id : "").ToList();
            Save();
        }

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
            // Umiejętności jeszcze nieodblokowane można zdobyć tymczasowo (na jedno podejście), gdy gracz dotarł
            // wystarczająco wysoko – to okazja, by wypróbować je przed odblokowaniem za popiół.
            foreach (var u in cfg.unlocks)
                if (u.kind == UnlockKind.Spell && u.target is SpellDefinition s && !pools.spells.Contains(s) && Profile.bestFloor >= u.requiredBestFloor)
                    pools.spells.Add(s);
            return pools;
        }

        public bool ValidateLoadout(LoadoutPlan plan, out string reason)
        {
            reason = null;
            if (plan.classDef == null) { reason = "Wybierz klasę"; return false; }
            if (plan.difficulty == null || !IsDifficultyAvailable(plan.difficulty)) { reason = "Niedostępna trudność"; return false; }
            if (plan.Cost > cfg.balance.loadoutBudget) { reason = $"Przekroczony budżet ({plan.Cost}/{cfg.balance.loadoutBudget})"; return false; }
            if (plan.extras.Any(e => !IsUnlocked(e))) { reason = "Wybrano zablokowany element"; return false; }
            var foreign = plan.extras.FirstOrDefault(e => !e.AvailableFor(plan.classDef));
            if (foreign != null) { reason = $"{foreign.displayName}: niedostępne dla klasy {plan.classDef.displayName}"; return false; }
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
            RecordDiscoveries(run);
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

        /// <summary>Kodeks pieczęci: dopisuje zespolenia i więzi obudzone w podejściu.</summary>
        public void RecordDiscoveries(RunState run)
        {
            if (Profile.discoveredSeals == null) Profile.discoveredSeals = new List<string>();
            foreach (var id in run.WokenSeals) if (!Profile.discoveredSeals.Contains(id)) Profile.discoveredSeals.Add(id);
        }

        public void RecordReachedFloor(int floorIndex)
        {
            if (floorIndex + 1 > Profile.bestFloor) { Profile.bestFloor = floorIndex + 1; Save(); }
        }

        public void RecordRunStart(LoadoutPlan plan)
        {
            Profile.totalRuns++;
            Profile.lastClassId = plan.classDef.id;
            Profile.lastAppearanceId = plan.appearance != null ? plan.appearance.id : "";
            Profile.lastDifficultyTier = plan.difficulty.tier;
            Profile.lastLoadout = plan.extras.Select(e => e.id).ToList();
            Profile.lastWeaponId = plan.Weapon != null && plan.Weapon != plan.classDef.DefaultWeapon ? plan.Weapon.id : "";
            Profile.lastLoadoutIndex = plan.loadoutIndex;
            Profile.lastStartSkillId = plan.startingSkill != null ? plan.startingSkill.id : "";
            Save();
        }

        // ------------------------------------------------------------------ Bestiariusz

        BestiaryRecord Record(string id, bool create)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var r = Profile.bestiary.FirstOrDefault(b => b.id == id);
            if (r == null && create) { r = new BestiaryRecord { id = id }; Profile.bestiary.Add(r); }
            return r;
        }

        /// <summary>Wróg pojawił się na arenie – trafia do bestiariusza (zapis od razu, bo to rzadkie).</summary>
        public void RecordSeen(EnemyDefinition def)
        {
            string id = Bestiary.BaseId(cfg, def);
            if (Record(id, false) != null) return;
            Record(id, true);
            Save();
        }

        /// <summary>Zabity wróg (zapis przy najbliższym zapisie profilu – koniec piętra, śmierć).</summary>
        public void RecordKill(EnemyDefinition def) { var r = Record(Bestiary.BaseId(cfg, def), true); if (r != null) r.kills++; }

        public bool Seen(EnemyDefinition def) => Record(Bestiary.BaseId(cfg, def), false) != null;
        public int Kills(EnemyDefinition def) => Record(Bestiary.BaseId(cfg, def), false)?.kills ?? 0;

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
                appearance = cfg.classes.FirstOrDefault(c => c.id == Profile.lastAppearanceId),
                difficulty = cfg.difficulties.FirstOrDefault(d => d.tier == Profile.lastDifficultyTier && IsDifficultyAvailable(d)) ?? cfg.difficulties.FirstOrDefault(),
            };
            foreach (var id in Profile.lastLoadout)
            {
                var u = cfg.unlocks.FirstOrDefault(x => x.id == id);
                if (u != null && u.kind != UnlockKind.Spell && IsUnlocked(u) && u.AvailableFor(plan.classDef) && plan.Cost + u.loadoutCost <= cfg.balance.loadoutBudget) plan.extras.Add(u);
            }
            if (plan.classDef != null && !string.IsNullOrEmpty(Profile.lastWeaponId))
                plan.startingWeapon = plan.classDef.WeaponChoices.FirstOrDefault(w => w.id == Profile.lastWeaponId);
            if (plan.classDef != null)
            {
                plan.loadoutIndex = Mathf.Clamp(Profile.lastLoadoutIndex, 0, Mathf.Max(0, plan.classDef.loadouts.Count - 1));
                plan.startingSkill = plan.classDef.startingSkillChoices.FirstOrDefault(s => s.id == Profile.lastStartSkillId);
            }
            FillSkills(plan);
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
                appearance = cfg.classes.Contains(plan.appearance) ? plan.appearance : null,
                difficulty = plan.difficulty,
                attributes = AttributeBlock.From(plan.classDef),
                seed = seed,
            };
            // Broń startowa: wybrana na ekranie przygotowania zastępuje domyślną broń klasy.
            var defaultWeapon = plan.classDef.DefaultWeapon;
            var weapon = plan.Weapon;
            foreach (var i in plan.classDef.startingItems)
                if (i != null) run.AddItem(new ItemInstance(i == defaultWeapon && weapon != null ? weapon : i), true);
            // Zestaw broni (klasy z Hell Unleashed): broń w prawej ręce, w lewej tarcza, grymuar, sztylet albo druga broń.
            var loadout = plan.Loadout;
            if (loadout != null)
            {
                if (loadout.weapon != null) run.AddAndEquip(new ItemInstance(loadout.weapon));
                if (loadout.offHand != null)
                {
                    var displaced = new List<ItemInstance>();
                    var off = new ItemInstance(loadout.offHand);
                    if (run.equipment.Equip(off, EquipSlot.OffHand, displaced)) run.inventory.AddRange(displaced);
                    else run.inventory.Add(off);
                }
            }
            if (plan.classDef.signatureSkill != null) { run.signatureSkill = new SpellInstance(plan.classDef.signatureSkill) { permanent = true }; run.AttachSeals(run.signatureSkill); }
            run.SyncGrantedSkills();
            foreach (var t in plan.classDef.startingTalents) if (t != null) run.AddBoon(t);
            foreach (var u in plan.extras)
            {
                switch (u.target)
                {
                    // Wybrany dodatek ląduje w ręku/na postaci; wyparty przedmiot klasy – do plecaka.
                    case ItemDefinition i: run.AddAndEquip(new ItemInstance(i)); break;
                    case SpellDefinition s: run.LearnSpell(s); break;
                    case BoonDefinition b: run.AddBoon(b); break;
                }
            }

            // Kolekcja umiejętności: klasowe i odblokowane na stałe. Sloty – według planu, a puste uzupełniane automatycznie.
            // Umiejętności klasy i odblokowane wracają w każdym podejściu – obie są „stałe”.
            foreach (var s in plan.StartingSkills) run.LearnSpell(s, true);
            if (plan.skillSlots.Any(s => s != null))
            {
                for (int i = 0; i < RunState.SkillSlotCount; i++) run.ClearSlot(i);
                for (int i = 0; i < RunState.SkillSlotCount; i++)
                {
                    var inst = plan.skillSlots[i] != null ? run.FindSpell(plan.skillSlots[i]) : null;
                    if (inst != null) run.AssignSlot(inst, i);
                }
                // Sloty, których wybór nie pasuje do tej postaci (np. umiejętność innej klasy), dostają kolejne wolne umiejętności.
                foreach (var inst in run.knownSpells)
                {
                    int free = System.Array.IndexOf(run.skillSlots, null);
                    if (free < 0) break;
                    if (run.SlotOf(inst) < 0) run.AssignSlot(inst, free);
                }
            }
            run.RefillFlasks();
            return run;
        }
    }
}
