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
    }
}
