using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Zapis stanu podejścia na początku piętra (JSON): klasa, trudność, cechy, wyposażenie, plecak, skille z poziomami,
    /// slotami i pieczęciami, wzmocnienia, sakwa pieczęci, dusze i flaszki. Służy botom do ponownej próby piętra po śmierci
    /// oraz jako „wyzwanie” – piętro, którego bot nie przeszedł trzy razy, do rozegrania przez gracza z buildem bota.
    /// Treść wskazują id (asset → <see cref="ContentDefinition.id"/>, pieczęć → id HU).
    /// </summary>
    [Serializable]
    public class RunSnapshot
    {
        [Serializable] public class Item
        {
            public string id; public int level; public int slot = -1;
            public int rarity, quality; public List<StatModifier> affixes = new List<StatModifier>(); public List<string> affixNames = new List<string>();
            public List<PassiveEffect> affixEffects = new List<PassiveEffect>();
            public static Item Of(ItemInstance it, int slot = -1) => new Item
            {
                id = it.definition.id, level = it.level, slot = slot, rarity = (int)it.rarity, quality = it.huQuality,
                affixes = new List<StatModifier>(it.affixes), affixNames = new List<string>(it.affixNames),
                affixEffects = new List<PassiveEffect>(it.affixEffects),
            };
            public ItemInstance Make(ItemDefinition def) => new ItemInstance(def, level)
            {
                rarity = (ItemRarity)rarity, huQuality = quality, affixes = new List<StatModifier>(affixes), affixNames = new List<string>(affixNames),
                affixEffects = affixEffects != null ? new List<PassiveEffect>(affixEffects) : new List<PassiveEffect>(),
            };
        }
        [Serializable] public class Skill { public string id; public int level; public bool permanent, fromItem; public int slot = -1; public string form; public List<string> supports = new List<string>(); }
        [Serializable] public class Stack { public string id; public int count; }

        public string title, note, created;
        public string classId, appearanceId;
        public int difficultyTier, seed, floorIndex, floorsCleared, souls, ashEarned, kills, attributePoints;
        public int toughness, strength, dexterity, intelligence;
        public int healthFlasks, manaFlasks, bonusHealthFlasks;
        public float healthFraction = 1f, manaFraction = 1f;
        public List<Item> equipment = new List<Item>(), inventory = new List<Item>();
        public List<Skill> skills = new List<Skill>();
        public Skill signature;
        public List<Stack> boons = new List<Stack>(), sealRanks = new List<Stack>();
        public List<string> sealPouch = new List<string>();

        static Skill SkillOf(RunState run, SpellInstance s) => new Skill
        {
            id = s.definition.id, level = s.level, permanent = s.permanent, fromItem = s.fromItem, slot = run.SlotOf(s),
            form = s.seals?.Form?.id, supports = s.seals != null ? s.seals.Supports.Select(x => x.id).ToList() : new List<string>(),
        };

        public static RunSnapshot Capture(RunState run, string title = null)
        {
            var s = new RunSnapshot
            {
                title = title, created = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                classId = run.startingClass?.id, appearanceId = run.appearance?.id, difficultyTier = run.difficulty != null ? run.difficulty.tier : 0,
                seed = run.seed, floorIndex = run.floorIndex, floorsCleared = run.floorsCleared, souls = run.souls, ashEarned = run.ashEarned,
                kills = run.kills, attributePoints = run.attributePoints,
                toughness = run.attributes.toughness, strength = run.attributes.strength, dexterity = run.attributes.dexterity, intelligence = run.attributes.intelligence,
                healthFlasks = run.healthFlasks, manaFlasks = run.manaFlasks, bonusHealthFlasks = run.bonusHealthFlasks,
                healthFraction = run.healthFraction, manaFraction = run.manaFraction,
            };
            foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
            {
                var it = run.equipment.Get(slot);
                if (it != null) s.equipment.Add(Item.Of(it, (int)slot));
            }
            foreach (var it in run.inventory) s.inventory.Add(Item.Of(it));
            foreach (var sp in run.knownSpells.Where(x => x != null && !x.fromItem)) s.skills.Add(SkillOf(run, sp));
            if (run.signatureSkill != null) s.signature = SkillOf(run, run.signatureSkill);
            foreach (var b in run.boons) s.boons.Add(new Stack { id = b.definition.id, count = b.stacks });
            foreach (var p in run.sealPouch) s.sealPouch.Add(p.id);
            foreach (var kv in run.SealRanks) s.sealRanks.Add(new Stack { id = kv.Key, count = kv.Value });
            return s;
        }

        /// <summary>Wszystkie definicje treści osiągalne z konfiguracji (pule, klasy, odblokowania, wieża) – po id.</summary>
        public static Dictionary<string, ContentDefinition> Index(GameConfig cfg)
        {
            var all = new List<ContentDefinition>();
            all.AddRange(cfg.classes); all.AddRange(cfg.difficulties); all.AddRange(cfg.baseItemPool); all.AddRange(cfg.baseSpellPool); all.AddRange(cfg.baseBoonPool);
            all.AddRange(cfg.unlocks.Select(u => u.target));
            all.Add(cfg.unarmed);
            foreach (var c in cfg.classes.Where(c => c != null))
            {
                all.AddRange(c.startingItems); all.AddRange(c.alternativeWeapons); all.AddRange(c.startingSkillChoices); all.AddRange(c.startingSpells);
                all.AddRange(c.startingTalents); all.Add(c.signatureSkill);
                foreach (var l in c.loadouts) { all.Add(l.weapon); all.Add(l.offHand); }
            }
            all.AddRange(all.OfType<ItemDefinition>().Select(i => i.grantedSkill).ToList());
            var map = new Dictionary<string, ContentDefinition>();
            foreach (var d in all) if (d != null && !string.IsNullOrEmpty(d.id) && !map.ContainsKey(d.id)) map[d.id] = d;
            return map;
        }

        /// <summary>Odtwarza stan podejścia; null, gdy brakuje klasy albo trudności.</summary>
        public RunState Restore(GameConfig cfg)
        {
            var map = Index(cfg);
            T Get<T>(string id) where T : ContentDefinition => id != null && map.TryGetValue(id, out var d) ? d as T : null;
            var cls = Get<ClassDefinition>(classId);
            var diff = cfg.difficulties.FirstOrDefault(d => d != null && d.tier == difficultyTier) ?? cfg.difficulties.FirstOrDefault();
            if (cls == null || diff == null) return null;
            var run = new RunState
            {
                startingClass = cls, appearance = Get<ClassDefinition>(appearanceId), difficulty = diff, seed = seed,
                attributes = new AttributeBlock { toughness = toughness, strength = strength, dexterity = dexterity, intelligence = intelligence },
                floorIndex = floorIndex, floorsCleared = floorsCleared, souls = souls, ashEarned = ashEarned, kills = kills, attributePoints = attributePoints,
                healthFlasks = healthFlasks, manaFlasks = manaFlasks, bonusHealthFlasks = bonusHealthFlasks,
                healthFraction = healthFraction, manaFraction = manaFraction,
            };
            foreach (var e in equipment)
            {
                var def = Get<ItemDefinition>(e.id);
                if (def == null) continue;
                var displaced = new List<ItemInstance>();
                run.equipment.Equip(e.Make(def), (EquipSlot)e.slot, displaced);
                run.inventory.AddRange(displaced);
            }
            foreach (var e in inventory) { var def = Get<ItemDefinition>(e.id); if (def != null) run.inventory.Add(e.Make(def)); }
            foreach (var b in boons) { var def = Get<BoonDefinition>(b.id); if (def != null) run.boons.Add(new BoonStack { definition = def, stacks = b.count }); }
            foreach (var r in sealRanks) run.SetSealRank(r.id, r.count);
            void Seals(SpellInstance inst, Skill k)
            {
                if (inst?.seals == null) return;
                if (k.form != null) { var f = HuCatalog.Seal(k.form); if (f != null) inst.seals.TryInsert(f, out _); }
                foreach (var id in k.supports) { var sp = HuCatalog.Seal(id); if (sp != null) inst.seals.TryInsert(sp, out _); }
            }
            for (int i = 0; i < RunState.SkillSlotCount; i++) run.ClearSlot(i);
            foreach (var k in skills)
            {
                var def = Get<SpellDefinition>(k.id);
                if (def == null) continue;
                var inst = run.LearnSpell(def, k.permanent, k.level);
                inst.level = k.level;
                for (int i = 0; i < RunState.SkillSlotCount; i++) if (run.skillSlots[i] == inst) run.ClearSlot(i);
                Seals(inst, k);
            }
            foreach (var k in skills.Where(k => k.slot >= 0)) { var inst = run.FindSpell(Get<SpellDefinition>(k.id)); if (inst != null) run.AssignSlot(inst, k.slot); }
            if (signature != null && Get<SpellDefinition>(signature.id) is SpellDefinition sig)
            {
                run.signatureSkill = new SpellInstance(sig, signature.level) { permanent = true };
                run.AttachSeals(run.signatureSkill);
                Seals(run.signatureSkill, signature);
            }
            foreach (var id in sealPouch) { var p = HuCatalog.Seal(id); if (p != null) run.sealPouch.Add(p); }
            run.SyncGrantedSkills();
            run.RefreshBonds();
            return run;
        }

        // ------------------------------------------------------------------ Wyzwania (pliki)

        /// <summary>Katalog wyzwań: piętra, których bot nie przeszedł w trzech próbach – do rozegrania z menu głównego.</summary>
        public static string ChallengeDir => Path.Combine(Application.persistentDataPath, "Wyzwania");

        public void Save(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(this, true));
        }

        public static RunSnapshot Load(string path)
        {
            try { return JsonUtility.FromJson<RunSnapshot>(File.ReadAllText(path)); }
            catch (Exception e) { Debug.LogWarning("Nie wczytano wyzwania " + path + ": " + e.Message); return null; }
        }

        /// <summary>Wyzwania od najnowszych.</summary>
        public static List<string> ChallengeFiles() => Directory.Exists(ChallengeDir)
            ? Directory.GetFiles(ChallengeDir, "*.json").OrderByDescending(File.GetLastWriteTime).ToList()
            : new List<string>();
    }
}
