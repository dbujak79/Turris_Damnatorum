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

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
