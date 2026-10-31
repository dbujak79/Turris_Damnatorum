using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Turris.EditorTools
{
    /// <summary>Import przyrostowy P0/P1. Nie resetuje treści, sceny, wieży ani istniejących animacji.</summary>
    public static class HellMigrationImport
    {
        [Serializable] class Entry { public string id, json; }
        [Serializable] class Snapshot { public Entry[] entries; }
        public static void ExportAudit()
        {
            var bundle = DefaultContent.Create();
            try
            {
                Directory.CreateDirectory("TestResults");
                File.WriteAllText("TestResults/hu-factory.json", JsonUtility.ToJson(new Snapshot { entries = bundle.all.OfType<ContentDefinition>().Select(d => new Entry { id = d.id, json = JsonUtility.ToJson(d) }).ToArray() }, true));
            }
            finally { foreach (var item in bundle.all.Concat(bundle.legacy)) if (item != null) Object.DestroyImmediate(item); }
        }

        [MenuItem("Turris/Hell Unleashed/Import P1 (dodaj brakujące)")]
        public static void Run()
        {
            TurrisSetup.SyncContent();
            var oldImport = TurrisSetup.ContentOnDisk().Values.OfType<EnemyDefinition>().Where(e => !string.IsNullOrEmpty(e.huSourceId) && e.huMigrationVersion < HellEnemyCatalog.Version).Select(e => e.id).ToArray();
            if (oldImport.Length > 0) TurrisSetup.OverwriteFromDefaults(oldImport);
            DemonAssetImport.ImportMissing();
            var models = new Dictionary<string, string>
            {
                ["hu_flail2h"] = "flail_2h", ["hu_flail2h_iron"] = "flail_2h", ["hu_morningstar2h"] = "morningstar_2h", ["hu_morningstar2h_runic"] = "morningstar_2h",
                ["hu_staff"] = "mage_staff", ["hu_staff_summoner"] = "mage_staff_summoner", ["hu_staff_witch"] = "mage_staff_witch",
                ["hu_greataxe_bearded"] = "greataxe_hu_p1", ["hu_greataxe_bardiche"] = "greataxe_hu_p1", ["hu_warhammer_runic"] = "warhammer_hu_p1", ["hu_warhammer_siege"] = "warhammer_hu_p1",
            };
            var real = TurrisSetup.ContentOnDisk();
            foreach (var model in models.Values.Distinct().Concat(new[] { "greataxe_hu_p1", "warhammer_hu_p1" }))
                HeroAssetImport.SetupModelMaterial($"Assets/TurrisDamnatorum/Models/Weapons/{model}/", model);
            foreach (var pair in models)
                if (real.TryGetValue(pair.Key, out var def) && def is ItemDefinition item && item.model == null)
                {
                    item.model = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/TurrisDamnatorum/Models/Weapons/{pair.Value}/{pair.Value}.fbx");
                    EditorUtility.SetDirty(item);
                }
            AssetDatabase.SaveAssets();
            var cfg = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/TurrisDamnatorum/Content/GameConfig.asset");
            foreach (var row in HellEnemyCatalog.Rows)
                if (real.TryGetValue(row.targetId, out var value) && value is EnemyDefinition enemy && !cfg.enemyCatalog.Contains(enemy)) cfg.enemyCatalog.Add(enemy);
            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();
            ExportAudit();
        }
        public static void RunBatch() { Run(); EditorApplication.Exit(0); }

        /// <summary>
        /// Przemoczenie i spowolnienie HU były zapisane w assetach demonów i broni HU jako chłód (zamrażał po 3 warstwach).
        /// Podmienia tylko te wpisy na <see cref="StatusKind.Soaked"/> / <see cref="StatusKind.Slow"/> według treści domyślnej;
        /// reszta assetów (także zmiany z inspektora) zostaje bez zmian.
        /// </summary>
        [MenuItem("Turris/Hell Unleashed/Przemoczenie i spowolnienie w assetach")]
        public static int MigrateSoakedSlow()
        {
            var real = TurrisSetup.ContentOnDisk();
            var bundle = DefaultContent.Create();
            int changed = 0;
            try
            {
                void Fix(Object owner, List<StatusApplication> src, List<StatusApplication> dst)
                {
                    if (src == null || dst == null) return;
                    for (int i = 0; i < Mathf.Min(src.Count, dst.Count); i++)
                        if (dst[i] != null && src[i] != null && dst[i].kind == StatusKind.Chill && (src[i].kind == StatusKind.Soaked || src[i].kind == StatusKind.Slow))
                        {
                            dst[i].kind = src[i].kind; changed++;
                            EditorUtility.SetDirty(owner);
                        }
                }
                foreach (var src in bundle.all.OfType<ContentDefinition>())
                {
                    if (!real.TryGetValue(src.id, out var dst)) continue;
                    if (src is EnemyDefinition se && dst is EnemyDefinition de && !string.IsNullOrEmpty(se.huSourceId))
                        for (int i = 0; i < Mathf.Min(se.attacks.Count, de.attacks.Count); i++)
                            if (se.attacks[i]?.attack != null && de.attacks[i]?.attack != null && se.attacks[i].attack.name == de.attacks[i].attack.name)
                                Fix(de, se.attacks[i].attack.statuses, de.attacks[i].attack.statuses);
                    if (src is ItemDefinition si && dst is ItemDefinition di && si.weapon != null && di.weapon != null && !string.IsNullOrEmpty(si.huSourceId))
                    {
                        Fix(di, si.weapon.light?.statuses, di.weapon.light?.statuses);
                        Fix(di, si.weapon.heavy?.statuses, di.weapon.heavy?.statuses);
                    }
                }
                AssetDatabase.SaveAssets();
            }
            finally { foreach (var item in bundle.all.Concat(bundle.legacy)) if (item != null) Object.DestroyImmediate(item); }
            Debug.Log($"[Turris] Przemoczenie/spowolnienie: zmienione wpisy {changed}");
            return changed;
        }
        public static void MigrateSoakedSlowBatch() { MigrateSoakedSlow(); EditorApplication.Exit(0); }

        /// <summary>
        /// Plan migracji, rozdz. 7.1/7.2: przenosi z treści domyślnej tylko pola zmienione przy porcie – efekty przedmiotów HU
        /// (statystyki bez odpowiednika, moce unikatów, odrzut i rykoszet broni), premie unikatowych broni, zasięg ciosu broni HU,
        /// drugi żywioł ataków demonów HU i efekty perków HU. Reszta assetów (także zmiany z inspektora) zostaje.
        /// </summary>
        [MenuItem("Turris/Hell Unleashed/Przedmioty, perki i żywioły (7.1, 7.2)")]
        public static int MigrateItemsAndElements()
        {
            var real = TurrisSetup.ContentOnDisk();
            var bundle = DefaultContent.Create();
            int changed = 0;
            try
            {
                foreach (var src in bundle.all.OfType<ContentDefinition>())
                {
                    if (!real.TryGetValue(src.id, out var dst)) continue;
                    bool dirty = false;
                    if (src is ItemDefinition si && dst is ItemDefinition di && !string.IsNullOrEmpty(si.huSourceId))
                    {
                        if (!si.effects.SequenceEqual(di.effects)) { di.effects = new List<PassiveEffect>(si.effects); dirty = true; }
                        if (si.huSourceId.Contains(".unique.") && si.IsWeapon && !si.modifiers.SequenceEqual(di.modifiers)) { di.modifiers = new List<StatModifier>(si.modifiers); dirty = true; }
                        if (si.weapon?.light != null && di.weapon?.light != null)
                            foreach (var (a, b) in new[] { (si.weapon.light, di.weapon.light), (si.weapon.heavy, di.weapon.heavy) })
                                if (a != null && b != null && !Mathf.Approximately(a.reach, b.reach)) { b.reach = a.reach; dirty = true; }
                    }
                    if (src is EnemyDefinition sev && dst is EnemyDefinition dev && !string.IsNullOrEmpty(sev.huSourceId) && !Mathf.Approximately(sev.evasion, dev.evasion))
                    {
                        dev.evasion = sev.evasion; dirty = true; // unik demona HU (celność bohatera go zmniejsza)
                    }
                    if (src is EnemyDefinition se && dst is EnemyDefinition de && !string.IsNullOrEmpty(se.huSourceId))
                        for (int i = 0; i < Mathf.Min(se.attacks.Count, de.attacks.Count); i++)
                        {
                            var a = se.attacks[i]?.attack; var b = de.attacks[i]?.attack;
                            if (a == null || b == null || a.name != b.name) continue;
                            if (a.element2 != b.element2 || !Mathf.Approximately(a.element2Share, b.element2Share)) { b.element2 = a.element2; b.element2Share = a.element2Share; dirty = true; }
                        }
                    if (src is BoonDefinition sb && dst is BoonDefinition db && src.id.StartsWith("hu_perk_") && !sb.effects.SequenceEqual(db.effects))
                    {
                        db.effects = new List<PassiveEffect>(sb.effects); dirty = true;
                    }
                    if (dirty) { EditorUtility.SetDirty(dst); changed++; }
                }
                AssetDatabase.SaveAssets();
            }
            finally { foreach (var item in bundle.all.Concat(bundle.legacy)) if (item != null) Object.DestroyImmediate(item); }
            Debug.Log($"[Turris] Przedmioty, perki i żywioły HU: zmienione assety {changed}");
            return changed;
        }
        public static void MigrateItemsAndElementsBatch() { MigrateItemsAndElements(); EditorApplication.Exit(0); }
    }
}
