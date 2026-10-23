using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Turris.Tests
{
    /// <summary>
    /// Treść gry do testów w silniku: domyślna treść z kodu, ale bohaterowie z modelami FBX z Hell Unleashed (wygląd z assetów),
    /// a nie proceduralne postacie. Klasyczne zestawy do testów mechanik (legacy_*) dostają modele typów bohatera.
    /// </summary>
    public static class TestContent
    {
        const string Visuals = "Assets/TurrisDamnatorum/Content/Visuals/";
        const string Classes = "Assets/TurrisDamnatorum/Content/Classes/";

        static readonly Dictionary<string, string> LegacyLooks = new Dictionary<string, string>
        {
            ["legacy_knight"] = "hero_warrior", ["legacy_mage"] = "mage_witch", ["legacy_rogue"] = "rogue_hooded_meshy",
        };

        public static DefaultContent.Bundle Create()
        {
            var bundle = DefaultContent.Create();
            foreach (var so in bundle.all) Assign(so as ClassDefinition);
            foreach (var so in bundle.legacy) Assign(so as ClassDefinition);
            return bundle;
        }

        static void Assign(ClassDefinition c)
        {
#if UNITY_EDITOR
            if (c == null || c.visual != null) return;
            CharacterVisualDefinition v = null;
            if (LegacyLooks.TryGetValue(c.id, out var look)) v = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Visuals + "visual_" + look + ".asset");
            else
            {
                var asset = AssetDatabase.LoadAssetAtPath<ClassDefinition>(Classes + c.id + ".asset");
                if (asset != null) v = asset.visual;
            }
            c.visual = v;
#endif
        }
    }
}
