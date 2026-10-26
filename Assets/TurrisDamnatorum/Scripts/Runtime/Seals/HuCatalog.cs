using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Katalog skilli i pieczęci Hell Unleashed, wczytywany z <c>Resources/HellCatalog/skills.json</c> i <c>seals.json</c>
    /// (generuje je <c>tools/art/import_hu_catalog.py</c> z plików <c>.tres</c> HU – nie edytować ręcznie).
    /// </summary>
    public static class HuCatalog
    {
        [Serializable] class SkillFile { public SkillRow[] skills; }
        [Serializable] class LevelRow { public string[] values; }
        [Serializable] class SkillRow
        {
            public string id, targetId, sha256, name, desc, l2, l3, classId;
            public int element, targeting, minWave = 1;
            public bool shop = true;
            public string[] tags, components;
            public LevelRow[] levels;
        }
        [Serializable] class SealFile { public SealRow[] seals; }
        [Serializable] class SealRow
        {
            public string id, sha256, name, desc, skillId, classId, group, requiresTargeting, notOfElement;
            public int kind;
            public string[] requiresComponents, requiresTags, forbidsComponents, forbidsTags, requiresSeals, ops, partnerOps;
            public bool lootOnly, shop = true;
            public float basePrice = 45, weight = 1;
        }

        static Dictionary<string, HuSkillData> skills;
        static Dictionary<string, HuSkillData> byTarget;
        static List<HuSeal> seals;
        static Dictionary<string, HuSeal> sealsById;

        /// <summary>Błędy parsowania operacji pieczęci (test pilnuje, by lista była pusta).</summary>
        public static readonly List<string> Errors = new List<string>();

        public static IReadOnlyDictionary<string, HuSkillData> Skills { get { Load(); return skills; } }
        public static IReadOnlyList<HuSeal> Seals { get { Load(); return seals; } }
        public static IEnumerable<HuSeal> Synergies => Seals.Where(s => s.kind == SealKind.Synergy);
        public static IEnumerable<HuSeal> Bonds => Seals.Where(s => s.kind == SealKind.Bond);

        public static HuSkillData Skill(string huId) { Load(); return huId != null && skills.TryGetValue(huId, out var s) ? s : null; }
        /// <summary>Skill HU wskazany przez id assetu Turris (np. <c>hu_infernal_orb</c>).</summary>
        public static HuSkillData ForTarget(string turrisId) { Load(); return turrisId != null && byTarget.TryGetValue(turrisId, out var s) ? s : null; }
        public static HuSeal Seal(string id) { Load(); return id != null && sealsById.TryGetValue(id, out var s) ? s : null; }

        static void Load()
        {
            if (skills != null) return;
            skills = new Dictionary<string, HuSkillData>(StringComparer.Ordinal);
            byTarget = new Dictionary<string, HuSkillData>(StringComparer.Ordinal);
            seals = new List<HuSeal>();
            sealsById = new Dictionary<string, HuSeal>(StringComparer.Ordinal);
            Errors.Clear();

            var skillText = Resources.Load<TextAsset>("HellCatalog/skills");
            if (skillText != null)
                foreach (var r in JsonUtility.FromJson<SkillFile>(skillText.text).skills)
                {
                    var d = new HuSkillData
                    {
                        id = r.id, targetId = r.targetId, name = r.name, desc = r.desc, l2 = r.l2, l3 = r.l3, classId = r.classId,
                        element = (HuElement)r.element, targeting = (HuTargeting)r.targeting, minWave = r.minWave, shop = r.shop,
                        levels = (r.levels ?? new LevelRow[0]).Select(l => HuParams.Parse(l.values)).ToArray(),
                    };
                    foreach (var t in r.tags ?? new string[0]) d.tags.Add(t);
                    d.components.AddRange(r.components ?? new string[0]);
                    skills[d.id] = d;
                    if (!string.IsNullOrEmpty(d.targetId)) byTarget[d.targetId] = d;
                }

            HuBalance.Apply(skills);

            var sealText = Resources.Load<TextAsset>("HellCatalog/seals");
            if (sealText != null)
                foreach (var r in JsonUtility.FromJson<SealFile>(sealText.text).seals)
                {
                    var s = new HuSeal
                    {
                        id = r.id, name = r.name, desc = r.desc, kind = (SealKind)r.kind, skillId = r.skillId ?? "", classId = r.classId ?? "", group = r.group ?? "",
                        requiresComponents = r.requiresComponents ?? new string[0], forbidsComponents = r.forbidsComponents ?? new string[0],
                        requiresSeals = r.requiresSeals ?? new string[0], lootOnly = r.lootOnly, basePrice = r.basePrice, weight = r.weight, shop = r.shop,
                    };
                    foreach (var t in r.requiresTags ?? new string[0]) s.requiresTags.Add(t);
                    foreach (var t in r.forbidsTags ?? new string[0]) s.forbidsTags.Add(t);
                    if (!string.IsNullOrEmpty(r.requiresTargeting) && Enum.TryParse(r.requiresTargeting, true, out HuTargeting tg)) s.requiresTargeting = tg;
                    if (!string.IsNullOrEmpty(r.notOfElement) && Enum.TryParse(r.notOfElement, true, out HuElement el)) s.notOfElement = el;
                    var errs = new List<string>();
                    s.ops = SealOp.ParseAll(r.ops, errs);
                    s.partnerOps = SealOp.ParseAll(r.partnerOps, errs);
                    foreach (var e in errs) Errors.Add(r.id + ": " + e);
                    seals.Add(s);
                    sealsById[s.id] = s;
                }
            TurrisSkills.AddTo(skills, byTarget, seals, sealsById, Errors);
            HuBalance.Apply(sealsById, Errors);
        }
    }
}
