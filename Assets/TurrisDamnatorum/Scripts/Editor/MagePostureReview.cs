using UnityEditor;
using UnityEngine;

namespace Turris.EditorTools
{
    public static class MagePostureReview
    {
        [MenuItem("Turris/PoC/Mag anime/Włącz zrównoważoną postawę")]
        public static void Enable()
        {
            var visual=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>("Assets/TurrisDamnatorum/Content/Visuals/visual_mage_anime.asset");
            visual.balancedMageStance=true;
            EditorUtility.SetDirty(visual);AssetDatabase.SaveAssetIfDirty(visual);
            Debug.Log("MAGE_POSTURE_ENABLED: tylko nowy model maga.");
        }
    }
}
