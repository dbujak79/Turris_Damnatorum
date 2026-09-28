using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Turris.EditorTools
{
    /// <summary>
    /// Import modeli FBX wrzuconych do Assets/TurrisDamnatorum/Models/:
    /// rig Humanoid, klipy ruchu zapętlone, ruch korzenia „wypieczony” w pozę (ruch prowadzi logika gry).
    /// </summary>
    public class TurrisModelPostprocessor : AssetPostprocessor
    {
        public const string ModelsFolder = "Assets/TurrisDamnatorum/Models/";

        static bool InFolder(string path) => path.Replace('\\', '/').StartsWith(ModelsFolder);

        void OnPreprocessModel()
        {
            if (!InFolder(assetPath)) return;
            var mi = (ModelImporter)assetImporter;
            mi.animationType = ModelImporterAnimationType.Human;
            mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            mi.importAnimation = true;
        }

        void OnPreprocessAnimation()
        {
            if (!InFolder(assetPath)) return;
            var mi = (ModelImporter)assetImporter;
            var clips = mi.defaultClipAnimations;
            if (clips == null || clips.Length == 0) return;
            string file = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
            foreach (var c in clips)
            {
                string n = (file + " " + c.name).ToLowerInvariant();
                c.loopTime = ClipMatcher.IsLoop(n);
                c.lockRootRotation = true;
                c.lockRootHeightY = true;
                c.lockRootPositionXZ = true;
                c.keepOriginalOrientation = true;
                c.keepOriginalPositionY = true;
                c.keepOriginalPositionXZ = true;
            }
            mi.clipAnimations = clips;
        }
    }

    /// <summary>Dopasowanie klipów do akcji gry na podstawie nazw (Mixamo, Quaternius i podobne).</summary>
    public static class ClipMatcher
    {
        public static bool IsLoop(string n) =>
            Has(n, "idle", "walk", "run", "jog", "strafe", "sprint") && !Has(n, "attack", "slash", "death", "dying", "hit", "impact", "roll", "dodge", "turn");

        /// <summary>Dopasowanie całych słów z typowymi końcówkami („slash” → „slashing”, ale „stab” ≠ „stable”).</summary>
        static bool Has(string n, params string[] keys) =>
            keys.Any(k => System.Text.RegularExpressions.Regex.IsMatch(n,
                @"\b" + System.Text.RegularExpressions.Regex.Escape(k.Trim()) + @"(s|es|ed|ing|ning|ging|bing|ping|ting)?\b"));

        public enum Slot { None, Idle, Walk, Run, WalkBack, StrafeLeft, StrafeRight, Action }

        public static Slot Classify(string n, out AnimAction action, out AttackAnim attack)
        {
            action = AnimAction.None;
            attack = AttackAnim.Auto;
            if (Has(n, "death", "dying", "die", "killed")) { action = AnimAction.Death; return Slot.Action; }
            if (Has(n, "roll", "dodge", "dive", "evade")) { action = Has(n, "back") && !Has(n, "roll") ? AnimAction.Backstep : AnimAction.Dodge; return Slot.Action; }
            if (Has(n, "parry", "deflect")) { action = AnimAction.Parry; return Slot.Action; }
            if (Has(n, "block")) { action = AnimAction.Block; return Slot.Action; }
            if (Has(n, "drink", "potion", "heal")) { action = AnimAction.Drink; return Slot.Action; }
            if (Has(n, "cast", "spell", "magic")) { action = AnimAction.Cast; return Slot.Action; }
            if (Has(n, "stun", "dizzy", "stagger")) { action = AnimAction.GuardBroken; return Slot.Action; }
            if (Has(n, "kneel", "knocked")) { action = AnimAction.Kneel; return Slot.Action; }
            if (Has(n, "hit", "react", "impact", "flinch", "damage")) { action = AnimAction.Flinch; return Slot.Action; }
            if (Has(n, "attack", "slash", "swing", "stab", "thrust", "lunge", "combo", "overhead", "downward", "punch", "claw", "swipe"))
            {
                action = AnimAction.Attack;
                if (Has(n, "stab", "thrust", "lunge")) attack = AttackAnim.Thrust;
                else if (Has(n, "jump")) attack = AttackAnim.Leap;
                else if (Has(n, "heavy", "overhead", "downward", "power", "vertical")) attack = AttackAnim.Overhead;
                else if (Has(n, "claw", "swipe")) attack = AttackAnim.Claw;
                else if (Has(n, "backhand", "left")) attack = AttackAnim.SlashLeft;
                else attack = AttackAnim.SlashRight;
                return Slot.Action;
            }
            if (Has(n, "strafe", "walk left", "left walk", "side", "sidestep")) return Has(n, "right") ? Slot.StrafeRight : Slot.StrafeLeft;
            if (Has(n, "back")) return Slot.WalkBack;
            if (Has(n, "run", "jog", "sprint")) return Slot.Run;
            if (Has(n, "walk")) return Slot.Walk;
            if (Has(n, "idle")) return Slot.Idle;
            return Slot.None;
        }
    }

    public static class CharacterVisualCreator
    {
        [MenuItem("Turris/Utwórz definicję wyglądu z zaznaczonego folderu (FBX)")]
        public static void CreateFromSelectedFolder()
        {
            string folder = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
            {
                EditorUtility.DisplayDialog("Turris", "Zaznacz w oknie Project folder z modelem postaci i animacjami (FBX).", "OK");
                return;
            }
            var def = CreateFromFolder(folder, out string report);
            if (def != null)
            {
                Selection.activeObject = def;
                EditorGUIUtility.PingObject(def);
            }
            Debug.Log(report);
            EditorUtility.DisplayDialog("Turris", report, "OK");
        }

        public static CharacterVisualDefinition CreateFromFolder(string folder, out string report)
        {
            var sb = new StringBuilder();
            var modelPaths = AssetDatabase.FindAssets("t:Model", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath).ToList();
            if (modelPaths.Count == 0) { report = "Brak plików FBX w folderze " + folder; return null; }

            // Model postaci: plik z największą liczbą renderów skórowanych (animacje Mixamo „without skin” nie mają siatki).
            string characterPath = modelPaths
                .OrderByDescending(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)?.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length ?? 0)
                .First();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);
            if (prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 0)
            {
                report = "Nie znaleziono modelu z siatką (SkinnedMeshRenderer). Pobierz postać „With Skin”.";
                return null;
            }

            var def = ScriptableObject.CreateInstance<CharacterVisualDefinition>();
            def.modelPrefab = prefab;
            sb.AppendLine($"Model: {Path.GetFileName(characterPath)}");

            int slashCount = 0;
            foreach (var path in modelPaths)
            {
                string file = Path.GetFileNameWithoutExtension(path);
                foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>())
                {
                    if (clip.name.StartsWith("__preview__")) continue;
                    // Mixamo nazywa klipy „mixamo.com” – wtedy decyduje nazwa pliku.
                    string n = (file + " " + clip.name).ToLowerInvariant().Replace('_', ' ').Replace('-', ' ');
                    var slot = ClipMatcher.Classify(n, out var action, out var attack);
                    switch (slot)
                    {
                        case ClipMatcher.Slot.Idle: if (def.idle == null) def.idle = clip; break;
                        case ClipMatcher.Slot.Walk: if (def.walk == null) def.walk = clip; break;
                        case ClipMatcher.Slot.Run: if (def.run == null) def.run = clip; break;
                        case ClipMatcher.Slot.WalkBack: if (def.walkBack == null) def.walkBack = clip; break;
                        case ClipMatcher.Slot.StrafeLeft: if (def.strafeLeft == null) def.strafeLeft = clip; break;
                        case ClipMatcher.Slot.StrafeRight: if (def.strafeRight == null) def.strafeRight = clip; break;
                        case ClipMatcher.Slot.Action:
                        {
                            if (action == AnimAction.Attack && attack == AttackAnim.SlashRight)
                            {
                                // Kolejne zwykłe cięcia: prawe, lewe, prawe…
                                attack = slashCount % 2 == 0 ? AttackAnim.SlashRight : AttackAnim.SlashLeft;
                                slashCount++;
                            }
                            if (def.actions.Any(a => a.action == action && a.attack == attack)) break;
                            var ac = new ActionClip
                            {
                                action = action, attack = attack, clip = clip,
                                upperBodyOnly = action == AnimAction.Block || action == AnimAction.Drink || action == AnimAction.Cast,
                                loop = action == AnimAction.Block || action == AnimAction.Kneel || action == AnimAction.GuardBroken,
                            };
                            if (action == AnimAction.Parry) { ac.windupEnd = 0.2f; ac.activeEnd = 0.45f; }
                            def.actions.Add(ac);
                            break;
                        }
                        default:
                            sb.AppendLine($"  ? nierozpoznany klip: {file} / {clip.name}");
                            continue;
                    }
                    sb.AppendLine($"  {clip.name} ({file}) → {(slot == ClipMatcher.Slot.Action ? action + (action == AnimAction.Attack ? "/" + attack : "") : slot.ToString())}");
                }
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/CharacterVisual_{Path.GetFileNameWithoutExtension(characterPath)}.asset");
            AssetDatabase.CreateAsset(def, assetPath);
            AssetDatabase.SaveAssets();
            sb.AppendLine($"Utworzono: {assetPath}");
            sb.AppendLine("Sprawdź w inspektorze granice faz ataków (windupEnd / activeEnd) i przypisz definicję w polu 'visual' klasy lub przeciwnika.");
            if (def.idle == null) sb.AppendLine("UWAGA: brak klipu idle.");
            if (!def.actions.Any(a => a.action == AnimAction.Attack)) sb.AppendLine("UWAGA: brak klipów ataku.");
            report = sb.ToString();
            return def;
        }
    }
}
