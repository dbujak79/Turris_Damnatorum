using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Turris.EditorTools
{
    /// <summary>
    /// Tworzy assety treści (ScriptableObject) oraz scenę Main.unity.
    /// Menu: Turris → Setup. W trybie wsadowym: -executeMethod Turris.EditorTools.TurrisSetup.RunBatch
    /// </summary>
    public static class TurrisSetup
    {
        const string ContentDir = "Assets/TurrisDamnatorum/Content";
        const string SceneDir = "Assets/TurrisDamnatorum/Scenes";
        const string ScenePath = SceneDir + "/Main.unity";
        const string ConfigPath = ContentDir + "/GameConfig.asset";

        [MenuItem("Turris/Setup (utwórz brakujące assety i scenę)")]
        public static void Setup() => Run(false);

        [MenuItem("Turris/Odtwórz domyślną treść (nadpisuje assety!)")]
        public static void Rebuild()
        {
            if (Application.isBatchMode || EditorUtility.DisplayDialog("Turris", "Nadpisać wszystkie assety treści wartościami domyślnymi?", "Tak", "Anuluj"))
                Run(true);
        }

        [MenuItem("Turris/Dodaj nową treść (bez nadpisywania)")]
        public static void SyncContentMenu() => SyncContent();

        /// <summary>Tryb wsadowy: -executeMethod Turris.EditorTools.TurrisSetup.SyncContentBatch</summary>
        public static void SyncContentBatch()
        {
            SyncContent();
            EditorApplication.Exit(0);
        }

        /// <summary>
        /// Dodaje treść, która pojawiła się w <see cref="DefaultContent"/> po utworzeniu assetów (np. nowe umiejętności),
        /// i dopisuje referencje do nich w istniejących listach (pule nagród, odblokowania, umiejętności klas).
        /// Istniejących assetów i wartości zmienionych w inspektorze NIE nadpisuje. Scena zostaje bez zmian.
        /// </summary>
        public static int SyncContent()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (config == null) { Run(false); return -1; }

            var real = new System.Collections.Generic.Dictionary<string, ContentDefinition>();
            foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { ContentDir }))
            {
                var def = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)) as ContentDefinition;
                if (def != null && !string.IsNullOrEmpty(def.id)) real[def.id] = def;
            }

            var bundle = DefaultContent.Create();
            var created = new System.Collections.Generic.HashSet<string>();
            foreach (var so in bundle.all)
            {
                if (!(so is ContentDefinition cd) || real.ContainsKey(cd.id)) continue;
                string dir = ContentDir + "/" + SubFolder(so);
                EnsureFolder(dir);
                AssetDatabase.CreateAsset(so, $"{dir}/{so.name}.asset");
                real[cd.id] = cd;
                created.Add(cd.id);
            }

            // Nowe assety mogą wskazywać na obiekty z pamięci (np. odblokowanie → istniejący talent) – podmień na assety z dysku.
            foreach (var id in created) RemapReferences(real[id], real);
            // Istniejące listy (konfiguracja, klasy, …) dostają referencje do nowych elementów.
            AppendNewReferences(bundle.config, config, real, created);
            foreach (var so in bundle.all)
                if (so is ContentDefinition cd && !created.Contains(cd.id) && real.TryGetValue(cd.id, out var existing))
                    AppendNewReferences(so, existing, real, created);

            foreach (var id in created) EditorUtility.SetDirty(real[id]);
            AssetDatabase.SaveAssets();
            // Obiekty z pamięci, które nie stały się assetami, nie są już potrzebne.
            foreach (var so in bundle.all)
                if (!(so is ContentDefinition cd && created.Contains(cd.id)) && !AssetDatabase.Contains(so)) Object.DestroyImmediate(so);
            Debug.Log($"[Turris] Dodano nową treść: {created.Count} ({string.Join(", ", created)}).");
            return created.Count;
        }

        static void RemapReferences(Object obj, System.Collections.Generic.Dictionary<string, ContentDefinition> real)
        {
            var so = new SerializedObject(obj);
            var it = so.GetIterator();
            while (it.Next(true))
            {
                if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (it.objectReferenceValue is ContentDefinition r && real.TryGetValue(r.id, out var onDisk) && onDisk != r)
                    it.objectReferenceValue = onDisk;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AppendNewReferences(Object source, Object target, System.Collections.Generic.Dictionary<string, ContentDefinition> real,
            System.Collections.Generic.HashSet<string> created)
        {
            var src = new SerializedObject(source);
            var dst = new SerializedObject(target);
            bool changed = false;
            var it = src.GetIterator();
            bool enter = true;
            while (it.Next(enter))
            {
                enter = true;
                if (!it.isArray || it.propertyType != SerializedPropertyType.Generic || it.arraySize == 0) continue;
                if (it.GetArrayElementAtIndex(0).propertyType != SerializedPropertyType.ObjectReference) continue;
                enter = false;
                var arr = dst.FindProperty(it.propertyPath);
                if (arr == null || !arr.isArray) continue;
                for (int i = 0; i < it.arraySize; i++)
                {
                    if (!(it.GetArrayElementAtIndex(i).objectReferenceValue is ContentDefinition r) || !created.Contains(r.id)) continue;
                    var onDisk = real[r.id];
                    bool present = false;
                    for (int j = 0; j < arr.arraySize; j++) if (arr.GetArrayElementAtIndex(j).objectReferenceValue == onDisk) { present = true; break; }
                    if (present) continue;
                    arr.arraySize++;
                    arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = onDisk;
                    changed = true;
                }
            }
            if (!changed) return;
            dst.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        [MenuItem("Turris/Usuń profil gracza (zapis)")]
        public static void DeleteProfile()
        {
            string path = Path.Combine(Application.persistentDataPath, "turris_profile.json");
            if (File.Exists(path)) File.Delete(path);
            Debug.Log("[Turris] Usunięto profil: " + path);
        }

        public static void RunBatch()
        {
            Run(false);
            EditorApplication.Exit(0);
        }

        public static void RunBatchForce()
        {
            Run(true);
            EditorApplication.Exit(0);
        }

        static void Run(bool overwrite)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (config == null || overwrite) config = CreateContent();
            CreateScene(config);
            CreateFxMaterials();
            AssetDatabase.SaveAssets();
            Debug.Log("[Turris] Setup zakończony. Otwórz " + ScenePath + " i naciśnij Play.");
        }

        static GameConfig CreateContent()
        {
            EnsureFolder(ContentDir);
            var bundle = DefaultContent.Create();
            foreach (var so in bundle.all)
            {
                string sub = so is GameConfig ? "" : "/" + SubFolder(so);
                string dir = ContentDir + sub;
                EnsureFolder(dir);
                string path = so is GameConfig ? ConfigPath : $"{dir}/{so.name}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (existing != null) AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(so, path);
            }
            foreach (var so in bundle.all) EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
            return bundle.config;
        }

        static string SubFolder(ScriptableObject so)
        {
            switch (so)
            {
                case ItemDefinition _: return "Items";
                case SpellDefinition _: return "Spells";
                case BoonDefinition _: return "Boons";
                case ClassDefinition _: return "Classes";
                case EnemyDefinition _: return "Enemies";
                case ArenaDefinition _: return "Arenas";
                case TowerDefinition _: return "Tower";
                case DifficultyDefinition _: return "Difficulties";
                case UnlockDefinition _: return "Unlocks";
                default: return "Misc";
            }
        }

        static void CreateScene(GameConfig config)
        {
            EnsureFolder(SceneDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.07f, 0.09f);
            cam.nearClipPlane = 0.1f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<CameraRig>();

            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);

            var rootGo = new GameObject("Game");
            var root = rootGo.AddComponent<GameRoot>();
            root.config = config;
            rootGo.AddComponent<GameUI>();

            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        /// <summary>Materiały efektów w Resources – dzięki nim shadery cząsteczek trafiają do buildu.</summary>
        static void CreateFxMaterials()
        {
            const string dir = "Assets/TurrisDamnatorum/Resources";
            EnsureFolder(dir);
            void Make(string name, string shader)
            {
                string path = $"{dir}/{name}.mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
                var sh = Shader.Find(shader);
                if (sh == null) { Debug.LogWarning("[Turris] Brak shadera " + shader); return; }
                AssetDatabase.CreateAsset(new Material(sh) { name = name }, path);
            }
            Make("TurrisFxAdditive", FxMaterials.AdditiveShader);
            Make("TurrisFxAlpha", FxMaterials.AlphaShader);
            Make("TurrisFxGlyph", FxMaterials.AdditiveShader);
            Make("TurrisFxRing", FxMaterials.AdditiveShader);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
