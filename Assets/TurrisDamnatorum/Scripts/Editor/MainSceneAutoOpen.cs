using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Turris.EditorTools
{
    /// <summary>
    /// Edytor uruchomiony z pustą, niezapisaną sceną (np. po testach PlayMode w oknie, które kończą się sceną tymczasową)
    /// otwiera scenę gry. Bez tego Play pokazuje samo niebo, bo w scenie nie ma GameRoot.
    /// Nie działa w trybie wsadowym ani przy uruchamianiu testów.
    /// </summary>
    [InitializeOnLoad]
    static class MainSceneAutoOpen
    {
        const string MainScene = "Assets/TurrisDamnatorum/Scenes/Main.unity";
        const string SessionKey = "Turris.MainSceneAutoOpen.Done";

        static MainSceneAutoOpen()
        {
            if (UnityEngine.Application.isBatchMode || SessionState.GetBool(SessionKey, false)) return;
            if (Array.Exists(Environment.GetCommandLineArgs(), a => a.Equals("-runTests", StringComparison.OrdinalIgnoreCase))) return;
            EditorApplication.delayCall += Open;
        }

        static void Open()
        {
            SessionState.SetBool(SessionKey, true); // tylko przy starcie edytora, nie po każdej rekompilacji
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.sceneCount != 1) return;
            var active = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(active.path) || active.isDirty) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScene) == null) return;
            EditorSceneManager.OpenScene(MainScene);
        }
    }
}
