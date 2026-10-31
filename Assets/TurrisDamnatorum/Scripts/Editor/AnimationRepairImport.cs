using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Turris.EditorTools
{
    /// <summary>
    /// Podpięcie klipów naprawionych w Blenderze (ArtSource/AnimationRepair) do wspólnych zestawów animacji.
    /// Lekkie cięcia miecza dwuręcznego / topora / młota (anim_greatsword):
    /// – „great sword slash” (lekki 1: Cleave, SlashRight) – pełny zamach, dociągnięcie nisko w lewo, tułów za ciosem;
    /// – „great sword slash (3)” (lekki 2: CleaveBack, SlashLeft) zamiast „slash (4)” (obrót całego ciała o 360°):
    ///   cięcie z lewej na prawą, zakończone przekrokiem w przód zamiast cofnięcia nogi.
    /// Przelicza też wysunięcie ciała (rootForward) z nowych nagrań. Nie przebudowuje wyglądów ani innych zestawów.
    /// </summary>
    public static class AnimationRepairImport
    {
        const string Gs = "Assets/TurrisDamnatorum/Models/Animations/Mixamo/Great Sword Pack/great sword ";

        [MenuItem("Turris/Naprawa animacji/Lekkie cięcia miecza dwuręcznego")]
        public static void ApplyLightSlashes()
        {
            var set = AssetDatabase.LoadAssetAtPath<WeaponAnimSet>(HeroAssetImport.VisualsFolder + "/anim_greatsword.asset");
            var slash = HeroAssetImport.InPlace(Gs + "slash.fbx");
            var slash3 = HeroAssetImport.InPlace(Gs + "slash (3).fbx");
            var moves = new[]
            {
                HeroAssetImport.Blow(AttackAnim.SlashRight, slash, 0, 14, 23, 38),
                HeroAssetImport.Blow(AttackAnim.Cleave, slash, 0, 14, 23, 38),
                // Klatki (od 0): kontakt – ostrze z lewej przez przód 26–32; koniec przekroku 55.
                HeroAssetImport.Blow(AttackAnim.SlashLeft, slash3, 0, 26, 32, 55),
                HeroAssetImport.Blow(AttackAnim.CleaveBack, slash3, 0, 26, 32, 55),
            };
            foreach (var m in moves)
            {
                if (m.attack == AttackAnim.Cleave || m.attack == AttackAnim.CleaveBack) { m.strikeShare = 1f; m.authoredSwing = true; }
                set.actions.RemoveAll(a => a != null && a.action == AnimAction.Attack && a.attack == m.attack);
            }
            set.actions.AddRange(moves);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            foreach (var m in moves)
                Debug.Log($"AnimationRepairImport: {m.attack} → {m.clip.name}, zamach {m.windupEnd:F3}, trafienie {m.activeEnd:F3}, " +
                          $"wysunięcie na końcu {(m.rootForward != null ? m.rootForward.Evaluate(1f) : 0f):F3}, autorski {m.authoredSwing}");
        }

        /// <summary>
        /// Ciężki cios topora/młota (Chop, także ładowany): „Composed/great axe chop” – na bazie „Standing Melee Attack Downward”:
        /// wolne cofnięcie topora w tył i w dół (pozycja ładowania w punkcie zatrzymania gry), szybki łuk górą, ostrze w ziemi przed postacią.
        /// Oryginał zostaje dla miecza z tarczą (Overhead/Slam) i rąbnięcia po uniku.
        /// </summary>
        [MenuItem("Turris/Naprawa animacji/Ciężki cios topora")]
        public static void ApplyHeavyChop()
        {
            var set = AssetDatabase.LoadAssetAtPath<WeaponAnimSet>(HeroAssetImport.VisualsFolder + "/anim_greatsword.asset");
            var chop = HeroAssetImport.Blow(AttackAnim.Chop, HeroAssetImport.InPlace(HeroAssetImport.GreatAxeChopPath), 0, 24, 29, 68);
            chop.strikeShare = 1f; chop.authoredSwing = true;
            set.actions.RemoveAll(a => a != null && a.action == AnimAction.Attack && a.attack == AttackAnim.Chop);
            set.actions.Add(chop);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            Debug.Log($"AnimationRepairImport: Chop → {chop.clip.name}, zamach {chop.windupEnd:F3}, trafienie {chop.activeEnd:F3}, " +
                      $"wysunięcie na końcu {(chop.rootForward != null ? chop.rootForward.Evaluate(1f) : 0f):F3}");
        }

        /// <summary>
        /// Unik „szarża barkiem” (DodgeStyle.ShoulderCharge – berserker, wojownik, paladyn HU): 4 kierunki × 2 układy rąk,
        /// na bazie biegów z zestawów – prawie normalny bieg ze skrętem tułowia, bark z przodu (ArtSource/AnimationRepair/shoulder-charge, spec_charge_run.py). Broń dwuręczna – anim_greatsword;
        /// tarcza – anim_sword_shield i zestaw paladyna anime. Klasy z innym stylem uniku dalej grają przewrót.
        /// </summary>
        [MenuItem("Turris/Naprawa animacji/Szarża barkiem")]
        public static void ApplyShoulderCharge()
        {
            const string C = "Assets/TurrisDamnatorum/Models/Animations/Mixamo/Composed/shoulder charge ";
            var dirs = new (AnimAction action, string name, int last)[]
            {
                (AnimAction.ChargeForward, "forward", 24), (AnimAction.ChargeBack, "back", 24),
                (AnimAction.ChargeLeft, "left", 24), (AnimAction.ChargeRight, "right", 24),
            };
            var sets = new (string path, string arms)[]
            {
                (HeroAssetImport.VisualsFolder + "/anim_greatsword.asset", "twohand"),
                (HeroAssetImport.VisualsFolder + "/anim_sword_shield.asset", "shield"),
                ("Assets/TurrisDamnatorum/Models/PoC/PaladinAnime/anim_paladin_anime_onehand.asset", "shield"),
            };
            foreach (var (path, arms) in sets)
            {
                var set = AssetDatabase.LoadAssetAtPath<WeaponAnimSet>(path);
                if (set == null) { Debug.LogWarning("AnimationRepairImport: brak zestawu " + path); continue; }
                set.actions.RemoveAll(a => a != null && AnimResolve.IsCharge(a.action));
                foreach (var d in dirs)
                {
                    var clip = HeroAssetImport.InPlace(C + d.name + " (" + arms + ").fbx", 0, d.last);
                    set.actions.Add(new ActionClip { action = d.action, clip = clip });
                }
                EditorUtility.SetDirty(set);
                Debug.Log($"AnimationRepairImport: szarża barkiem ({arms}) → {System.IO.Path.GetFileNameWithoutExtension(path)}");
            }
            AssetDatabase.SaveAssets();
        }

        public static void ApplyShoulderChargeBatch()
        {
            try { ApplyShoulderCharge(); EditorApplication.Exit(0); }
            catch (System.Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        /// <summary>
        /// Ciężki 2 (ChopFollow) – oryginał z obrotem; ciężki z biegu (LeapChop – wyskok i rąbnięcie)
        /// i lekki z biegu (RunningSweep – poziome cięcie nisko przez przód) – ArtSource/AnimationRepair/heavy-chop.
        /// </summary>
        [MenuItem("Turris/Naprawa animacji/Ataki z biegu (ciężki 2 – oryginał)")]
        public static void ApplyFollowAndRunning()
        {
            var set = AssetDatabase.LoadAssetAtPath<WeaponAnimSet>(HeroAssetImport.VisualsFolder + "/anim_greatsword.asset");
            var moves = HeroAssetImport.FollowAndRunningMoves();
            // Ciężki 2 wraca do oryginału z obrotem (użytkownik: „atk_heavy2 był dobry”).
            var follow = HeroAssetImport.Blow(AttackAnim.ChopFollow,
                HeroAssetImport.InPlace("Assets/TurrisDamnatorum/Models/Animations/Mixamo/Great Sword Pack/great sword high spin attack.fbx"), 0, 30, 37, 56);
            follow.strikeShare = 1f;
            moves.Add(follow);
            foreach (var m in moves) set.actions.RemoveAll(a => a != null && a.action == AnimAction.Attack && a.attack == m.attack);
            set.actions.AddRange(moves);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            foreach (var m in moves)
                Debug.Log($"AnimationRepairImport: {m.attack} → {m.clip.name}, zamach {m.windupEnd:F3}, trafienie {m.activeEnd:F3}, " +
                          $"wysunięcie na końcu {(m.rootForward != null ? m.rootForward.Evaluate(1f) : 0f):F3}");
        }

        public static void ApplyFollowAndRunningBatch()
        {
            try { ApplyFollowAndRunning(); EditorApplication.Exit(0); }
            catch (System.Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        /// <summary>Trafienie (wariant z kierunku i siły ciosu) i śmierć z upadkiem do przodu po ciosie z tyłu – anim_greatsword.</summary>
        [MenuItem("Turris/Naprawa animacji/Trafienie i śmierć")]
        public static void ApplyReactions()
        {
            var set = AssetDatabase.LoadAssetAtPath<WeaponAnimSet>(HeroAssetImport.VisualsFolder + "/anim_greatsword.asset");
            var r = HeroAssetImport.Reactions();
            foreach (var a in r) set.actions.RemoveAll(x => x != null && x.action == a.action);
            set.actions.AddRange(r);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            foreach (var a in r) Debug.Log($"AnimationRepairImport: {a.action} → {a.clip.name} ({a.clip.length:F2} s)");
        }

        public static void ApplyReactionsBatch()
        {
            try { ApplyReactions(); EditorApplication.Exit(0); }
            catch (System.Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        public static void ApplyHeavyChopBatch()
        {
            try { ApplyHeavyChop(); EditorApplication.Exit(0); }
            catch (System.Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        public static void ApplyLightSlashesBatch()
        {
            try { ApplyLightSlashes(); EditorApplication.Exit(0); }
            catch (System.Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }
    }
}
