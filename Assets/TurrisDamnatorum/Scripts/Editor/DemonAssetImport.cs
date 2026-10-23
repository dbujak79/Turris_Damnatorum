using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Turris.EditorTools
{
    /// <summary>
    /// Demony z pierwszych trzech aren Hell Unleashed (pliki z tools/art/convert_hu_demons.sh): import Generic z pętlami,
    /// materiały Standard z tekstur HU, definicje wyglądu (klipy z taktami ciosu z &lt;demon&gt;.beats.json) i przypisanie
    /// modeli wrogom. <see cref="ApplyContentBatch"/> dopisuje też demony do treści i przepisuje wieżę (piętra I–III z HU).
    ///
    /// Batch: Unity.exe -batchmode -projectPath . -executeMethod Turris.EditorTools.DemonAssetImport.ApplyContentBatch
    /// </summary>
    public static class DemonAssetImport
    {
        /// <summary>Dodaje tylko brakujące modele P1 i przypisuje je wyłącznie nowym demonom.</summary>
        public static void ImportMissing()
        {
            foreach (var r in HellEnemyCatalog.Rows)
            {
                if (!Heights.ContainsKey(r.model)) Heights.Add(r.model, r.height);
                if (!EnemyModels.ContainsKey(r.targetId)) EnemyModels.Add(r.targetId, r.model);
                string path = $"{HeroAssetImport.VisualsFolder}/visual_demon_{r.model}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(path);
                if (existing != null && existing.idle != null && existing.Find(AnimAction.Death, AttackAnim.Auto) != null) continue;
                ConfigureImport(Demons + r.model + "/" + r.model + ".fbx");
                HeroAssetImport.SetupModelMaterial(Demons + r.model + "/", r.model);
                DemonVisual(r.model);
            }
            AssignVisuals();
            foreach (var def in TurrisSetup.ContentOnDisk().Values.OfType<EnemyDefinition>())
                if (!string.IsNullOrEmpty(def.huModel) && def.visual == null)
                {
                    def.visual = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>($"{HeroAssetImport.VisualsFolder}/visual_demon_{def.huModel}.asset");
                    EditorUtility.SetDirty(def);
                }
            AssetDatabase.SaveAssets();
        }
        const string Demons = "Assets/TurrisDamnatorum/Models/Demons/";

        /// <summary>Model → wysokość w grze (m, jak EnemyModels.For w HU).</summary>
        public static readonly Dictionary<string, float> Heights = new Dictionary<string, float>
        {
            ["imp"] = 1.3f, ["imp_air"] = 1.3f, ["imp_earth"] = 1.3f, ["imp_water"] = 1.3f,
            ["cultist"] = 1.85f, ["worm"] = 0.6f, ["gatelord"] = 3.8f, ["minion"] = 1.0f,
        };

        /// <summary>Wróg (id) → model demona.</summary>
        public static readonly Dictionary<string, string> EnemyModels = new Dictionary<string, string>
        {
            ["enemy_imp"] = "imp", ["enemy_imp_frost"] = "imp_water", ["enemy_imp_storm"] = "imp_air", ["enemy_imp_earth"] = "imp_earth",
            ["enemy_hu_cultist"] = "cultist", ["enemy_worm"] = "worm", ["enemy_minion"] = "minion", ["enemy_gatelord"] = "gatelord",
        };

        /// <summary>
        /// Takty klipów bez pliku .beats.json (zestaw „biped” z HU, EnemyModels.ClipBeats/SpecialBeats): szczyt zamachu
        /// i klatka ciosu w sekundach przy 30 kl./s.
        /// </summary>
        static readonly Dictionary<string, (float top, float strike)> BipedBeats = new Dictionary<string, (float, float)>
        {
            ["Attack"] = (9 / 30f, 13 / 30f), ["Special"] = (13 / 30f, 18 / 30f),
        };

        public static void RunBatch()
        {
            try { Run(); EditorApplication.Exit(0); }
            catch (System.Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        [MenuItem("Turris/Demony z Hell Unleashed: import, materiały, wyglądy")]
        public static void Run()
        {
            HeroAssetImport.EnsureFolder(HeroAssetImport.VisualsFolder);
            AssetDatabase.Refresh();
            foreach (var name in Heights.Keys)
            {
                string folder = Demons + name + "/";
                ConfigureImport(folder + name + ".fbx");
                HeroAssetImport.SetupModelMaterial(folder, name);
                DemonVisual(name);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("DemonAssetImport: gotowe");
        }

        /// <summary>Batch: import, dopisanie demonów do treści (SyncContent), wieża z piętrami HU i przypisanie modeli.</summary>
        public static void ApplyContentBatch()
        {
            try
            {
                ApplyContent();
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Turris/Demony z Hell Unleashed: treść i piętra I–III")]
        public static void ApplyContent()
        {
            Run();
            TurrisSetup.SyncContent();
            TurrisSetup.OverwriteFromDefaults("tower_main");
            AssignVisuals();
        }

        // ------------------------------------------------------------------ Import

        static void ConfigureImport(string fbx)
        {
            var mi = AssetImporter.GetAtPath(fbx) as ModelImporter;
            if (mi == null) { Debug.LogWarning("Brak modelu " + fbx); return; }
            bool changed = false;
            if (mi.animationType != ModelImporterAnimationType.Generic) { mi.animationType = ModelImporterAnimationType.Generic; changed = true; }
            if (mi.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel) { mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; changed = true; }
            if (mi.materialImportMode != ModelImporterMaterialImportMode.ImportStandard) { mi.materialImportMode = ModelImporterMaterialImportMode.ImportStandard; changed = true; }
            if (changed) mi.SaveAndReimport();

            // Pętle: bezczynność, chód, uczepienie (nazwy klipów z HU: Idle, Walk, *_loop).
            var clips = mi.defaultClipAnimations;
            foreach (var c in clips)
            {
                string n = ShortName(c.name);
                c.name = n;
                c.loopTime = n.StartsWith("Idle") || n.StartsWith("Walk") || n.EndsWith("_loop");
                c.lockRootRotation = false; c.lockRootHeightY = false; c.lockRootPositionXZ = false;
            }
            var existing = mi.clipAnimations;
            bool same = existing.Length == clips.Length && existing.Zip(clips, (a, b) => a.name == b.name && a.loopTime == b.loopTime).All(x => x);
            if (!same)
            {
                mi.clipAnimations = clips;
                mi.SaveAndReimport();
            }
        }

        /// <summary>„Armature|Walk_loop” → „Walk_loop”.</summary>
        static string ShortName(string take) => take.Contains("|") ? take.Substring(take.LastIndexOf('|') + 1) : take;

        // ------------------------------------------------------------------ Wygląd

        static void DemonVisual(string name)
        {
            string fbx = Demons + name + "/" + name + ".fbx";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            if (model == null) { Debug.LogWarning("Brak modelu " + fbx); return; }
            var clips = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();
            AnimationClip Clip(params string[] names) => names.Select(n => clips.FirstOrDefault(c => c.name == n)).FirstOrDefault(c => c != null);

            string path = $"{HeroAssetImport.VisualsFolder}/visual_demon_{name}.asset";
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(path);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<CharacterVisualDefinition>();
                AssetDatabase.CreateAsset(def, path);
            }
            def.modelPrefab = model;
            def.demon = true;
            def.attachGear = false;
            def.oneHandSet = def.twoHandSet = def.bowSet = def.staffSet = null;
            // Wysokość jak w HU: model ma 1,8 m przy skali wroga 1 (skala wroga = wysokość / 1,8), stopy na ziemi.
            var (height, minY) = Measure(model);
            def.scale = height > 0.01f ? 1.8f / height : 1f;
            def.positionOffset = Vector3.zero;
            def.rotationOffset = Vector3.zero;
            def.idle = Clip("Idle", "Idle_loop");
            def.walk = Clip("Walk", "Walk_loop");
            def.run = null; def.walkBack = null; def.strafeLeft = null; def.strafeRight = null;
            def.walkSpeed = WalkSpeed(name);

            var beats = ReadBeats(name);
            def.actions = new List<ActionClip>();
            void Blow(AttackAnim style, AnimationClip clip, string beatKey)
            {
                if (clip == null) return;
                (float top, float strike) b = beats.TryGetValue(beatKey, out var v) ? v : BipedBeats.TryGetValue(beatKey, out var d) ? d : (clip.length * 0.35f, clip.length * 0.5f);
                def.actions.Add(new ActionClip
                {
                    action = AnimAction.Attack, attack = style, clip = clip,
                    windupEnd = Mathf.Clamp01(b.top / clip.length), activeEnd = Mathf.Clamp01(b.strike / clip.length),
                });
            }
            Blow(AttackAnim.Auto, Clip("Attack"), "Attack");
            Blow(AttackAnim.Special, Clip("Special"), "Special");
            Blow(AttackAnim.Latch, Clip("Latch"), "Latch");
            var death = Clip("Death");
            if (death != null) def.actions.Add(new ActionClip { action = AnimAction.Death, clip = death });
            var latched = Clip("Latched_loop");
            if (latched != null) def.actions.Add(new ActionClip { action = AnimAction.Latched, clip = latched, loop = true });
            EditorUtility.SetDirty(def);
            if (Mathf.Abs(minY) > 0.05f * height) Debug.LogWarning($"DemonAssetImport: {name} – stopy {minY:0.00} m od początku układu");
        }

        /// <summary>Prędkość, przy której klip chodu gra w tempie 1 – prędkość demona z HU (px/s / 24).</summary>
        static float WalkSpeed(string name)
        {
            var source = HellEnemyCatalog.Rows.FirstOrDefault(r => r.model == name);
            if (source != null) return source.MoveSpeed / 24f;
            switch (name)
            {
                case "cultist": return 2.54f;
                case "worm": return 3.67f;
                case "gatelord": return 2.3f;
                case "minion": return 4.0f;
                default: return 3.24f;
            }
        }

        /// <summary>Wysokość siatki w pozie spoczynkowej i położenie najniższego punktu (m, układ modelu).</summary>
        static (float height, float minY) Measure(GameObject prefab)
        {
            var go = Object.Instantiate(prefab);
            try
            {
                float min = float.MaxValue, max = float.MinValue;
                var mesh = new Mesh();
                foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    r.BakeMesh(mesh, true);
                    foreach (var v in mesh.vertices)
                    {
                        float y = r.transform.TransformPoint(v).y;
                        min = Mathf.Min(min, y); max = Mathf.Max(max, y);
                    }
                }
                foreach (var f in go.GetComponentsInChildren<MeshFilter>())
                    foreach (var v in f.sharedMesh.vertices)
                    {
                        float y = f.transform.TransformPoint(v).y;
                        min = Mathf.Min(min, y); max = Mathf.Max(max, y);
                    }
                Object.DestroyImmediate(mesh);
                return min > max ? (0f, 0f) : (max - min, min);
            }
            finally { Object.DestroyImmediate(go); }
        }

        /// <summary>Takty z &lt;demon&gt;.beats.json (HU: tools/art/blender/build_demon.py): { "Attack": { "top": s, "strike": s } }.</summary>
        static Dictionary<string, (float top, float strike)> ReadBeats(string name)
        {
            var result = new Dictionary<string, (float, float)>();
            string file = Demons + name + "/" + name + ".beats.json.txt";
            if (!File.Exists(file)) return result;
            string text = File.ReadAllText(file);
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text,
                         "\"(\\w+)\"\\s*:\\s*\\{\\s*\"top\"\\s*:\\s*([0-9.]+)\\s*,\\s*\"strike\"\\s*:\\s*([0-9.]+)"))
            {
                float top = float.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
                float strike = float.Parse(m.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                result[m.Groups[1].Value] = (top, strike);
            }
            return result;
        }

        // ------------------------------------------------------------------ Treść

        public static void AssignVisuals()
        {
            var real = TurrisSetup.ContentOnDisk();
            foreach (var kv in EnemyModels)
            {
                if (!(real.TryGetValue(kv.Key, out var c) && c is EnemyDefinition enemy)) { Debug.LogWarning("Brak wroga " + kv.Key); continue; }
                var v = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>($"{HeroAssetImport.VisualsFolder}/visual_demon_{kv.Value}.asset");
                if (v == null) { Debug.LogWarning("Brak wyglądu demona " + kv.Value); continue; }
                enemy.visual = v;
                EditorUtility.SetDirty(enemy);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("DemonAssetImport: przypisano modele demonów");
        }
    }
}
