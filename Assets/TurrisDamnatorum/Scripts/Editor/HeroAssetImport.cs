using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Turris.EditorTools
{
    /// <summary>
    /// Bohaterowie, bronie i animacje przeniesione z Hell Unleashed (pliki z tools/art/convert_hu_*.py):
    /// materiały Standard z tekstur, zestawy animacji broni z klipów Mixamo (z granicami faz ciosów)
    /// i definicje wyglądu 15 bohaterów. Nie dotyka treści gry (Content/Classes, Items…).
    ///
    /// Batch: Unity.exe -batchmode -projectPath . -executeMethod Turris.EditorTools.HeroAssetImport.RunBatch
    /// </summary>
    public static class HeroAssetImport
    {
        const string Models = "Assets/TurrisDamnatorum/Models/";
        const string Heroes = Models + "Heroes/";
        const string Weapons = Models + "Weapons/";
        const string Mixamo = Models + "Animations/Mixamo/";
        public const string VisualsFolder = "Assets/TurrisDamnatorum/Content/Visuals";

        /// <summary>Hell Unleashed normalizuje bohaterów stałą skalą 1,8 / 1,85 (a nie z obrysu).</summary>
        public const float HeroScale = 1.8f / 1.85f;

        public static readonly string[] HeroNames =
        {
            "paladin", "mage_witch", "rogue_hooded_meshy", "hero_warrior", "hero_warrior_berserker", "hero_warrior_dual",
            "hero_mage_fire", "hero_mage_water", "hero_mage_air", "hero_mage_earth", "hero_mage_summoner", "hero_mage_battle",
            "hero_rogue_archer", "hero_rogue_assassin", "hero_rogue_arcane_archer",
        };

        public static void RunBatch()
        {
            try
            {
                Run();
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Turris/Bohaterowie z Hell Unleashed: materiały, animacje, wyglądy")]
        public static void Run()
        {
            EnsureFolder(VisualsFolder);
            AssetDatabase.Refresh();
            foreach (var h in HeroNames) SetupModelMaterial(Heroes + h + "/", h);
            foreach (var dir in Directory.GetDirectories(Weapons))
                SetupModelMaterial(Weapons + Path.GetFileName(dir) + "/", Path.GetFileName(dir));

            var oneHand = SwordShieldSet();
            var twoHand = GreatswordSet();
            var bow = BowSet();
            foreach (var h in HeroNames) HeroVisual(h, oneHand, twoHand, bow);
            AssetDatabase.SaveAssets();
            Debug.Log("HeroAssetImport: gotowe");
        }

        // ------------------------------------------------------------------ Materiały

        internal static void SetupModelMaterial(string folder, string name)
        {
            string fbx = folder + name + ".fbx";
            var mi = AssetImporter.GetAtPath(fbx) as ModelImporter;
            if (mi == null) { Debug.LogWarning("Brak modelu " + fbx); return; }
            string matPath = folder + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find(MaterialLibrary.LitShader));
                AssetDatabase.CreateAsset(mat, matPath);
            }
            var albedo = Tex(folder, name, "_albedo");
            var metallic = Tex(folder, name, "_metallic");
            var normal = Tex(folder, name, "_normal");
            var emission = Tex(folder, name, "_emission");
            mat.SetTexture("_BaseMap", albedo);
            mat.SetColor("_BaseColor", Color.white);
            if (metallic != null)
            {
                mat.SetTexture("_MetallicGlossMap", metallic);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                mat.SetFloat("_Smoothness", 1f);
                mat.SetFloat("_SmoothnessTextureChannel", 0f);
            }
            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", 1f);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (emission != null)
            {
                mat.SetTexture("_EmissionMap", emission);
                mat.SetColor("_EmissionColor", Color.white);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            EditorUtility.SetDirty(mat);

            // Wszystkie materiały z FBX → ten jeden materiał.
            bool changed = false;
            foreach (var src in AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Material>())
            {
                var id = new AssetImporter.SourceAssetIdentifier(typeof(Material), src.name);
                if (mi.GetExternalObjectMap().TryGetValue(id, out var cur) && cur == mat) continue;
                mi.AddRemap(id, mat);
                changed = true;
            }
            if (changed) { mi.SaveAndReimport(); }
        }

        static Texture2D Tex(string folder, string name, string suffix)
        {
            foreach (var ext in new[] { ".png", ".jpg" })
            {
                var t = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + name + suffix + ext);
                if (t != null) return t;
            }
            return null;
        }

        // ------------------------------------------------------------------ Klipy Mixamo

        /// <summary>
        /// Klip z pliku FBX; z podanym zakresem klatek (od 0) – jako osobny wycinek ustawiony w imporcie.
        /// <paramref name="dropRoot"/>: przesunięcie korzenia w poziomie (klipy ruchu Mixamo idą do przodu) jest wyjmowane
        /// z pozy i odrzucane, bo ruch prowadzi logika gry. Wpieczone w pozę przesuwałoby ciało i cofało je skokiem co pętlę.
        /// </summary>
        static AnimationClip Clip(string path, int first = -1, int last = -1, bool loop = false, bool dropRoot = false)
        {
            var mi = AssetImporter.GetAtPath(path) as ModelImporter;
            if (mi == null) { Debug.LogWarning("Brak klipu " + path); return null; }
            var defaults = mi.defaultClipAnimations;
            if (defaults.Length == 0) { Debug.LogWarning("Plik bez animacji " + path); return null; }
            var c = defaults[0];
            c.name = Path.GetFileNameWithoutExtension(path);
            if (first >= 0) { c.firstFrame = first; c.lastFrame = last; }
            c.loopTime = loop;
            c.lockRootRotation = true;
            c.lockRootHeightY = true;
            c.lockRootPositionXZ = !dropRoot;
            c.keepOriginalOrientation = true;
            c.keepOriginalPositionY = true;
            c.keepOriginalPositionXZ = true;
            var existing = mi.clipAnimations;
            bool same = existing.Length == 1 && existing[0].name == c.name && existing[0].loopTime == c.loopTime
                        && existing[0].lockRootPositionXZ == c.lockRootPositionXZ
                        && Mathf.Approximately(existing[0].firstFrame, c.firstFrame) && Mathf.Approximately(existing[0].lastFrame, c.lastFrame);
            if (!same)
            {
                mi.clipAnimations = new[] { c };
                mi.SaveAndReimport();
            }
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(a => !a.name.StartsWith("__preview__"));
        }

        /// <summary>Pętla ruchu albo trzymania (bez przesuwania ciała).</summary>
        static AnimationClip Loop(string path) => Clip(path, loop: true, dropRoot: true);

        /// <summary>
        /// Akcja w miejscu (cios, parowanie, czar, drgnięcie): krok z nagrania nie przesuwa ciała względem postaci –
        /// przemieszczenie daje gra (wypad, odrzut). Wpieczony krok wysuwał ciało do 0,5 m przed postać, a przejście
        /// do następnej akcji ściągało je z powrotem (dłoń ~30 m/s).
        /// </summary>
        static AnimationClip InPlace(string path, int first = -1, int last = -1) => Clip(path, first, last, dropRoot: true);

        /// <summary>Unik: przemieszczenie daje gra, klip – tylko ruch ciała.</summary>
        static AnimationClip Roll() => Clip(Mixamo + "Loose/Falling To Roll.fbx", 5, 44, dropRoot: true);

        /// <summary>Cios: klatki zamachu (strike) i końca fazy aktywnej (follow) przeliczone na czas znormalizowany.</summary>
        static ActionClip Blow(AttackAnim attack, AnimationClip clip, int start, int strike, int follow, int end) => new ActionClip
        {
            action = AnimAction.Attack, attack = attack, clip = clip,
            windupEnd = Mathf.Clamp01((strike - start) / (float)(end - start)),
            activeEnd = Mathf.Clamp01((follow - start) / (float)(end - start)),
            rootForward = RootForward(clip),
        };

        /// <summary>
        /// Wysunięcie ciała do przodu w nagraniu ciosu (RootT.z – środek ciężkości, jednostki awatara), od pierwszej klatki,
        /// w czasie znormalizowanym. Klip gra w miejscu (ruch korzenia odrzucony), więc bez tego stopy jechałyby względem
        /// ciała do tyłu o tyle, o ile ciało w nagraniu idzie do przodu.
        /// </summary>
        static AnimationCurve RootForward(AnimationClip clip)
        {
            if (clip == null) return null;
            var curve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.z"));
            if (curve == null || curve.length < 2) return null;
            var result = new AnimationCurve();
            float v0 = curve.Evaluate(0f);
            const int samples = 48;
            for (int i = 0; i <= samples; i++)
            {
                float u = i / (float)samples;
                result.AddKey(new Keyframe(u, curve.Evaluate(u * clip.length) - v0));
            }
            return result;
        }

        static ActionClip Act(AnimAction action, AnimationClip clip, bool upper = false, bool loop = false, float windupEnd = 0.35f, float activeEnd = 0.55f) => new ActionClip
        {
            action = action, clip = clip, upperBodyOnly = upper, loop = loop, windupEnd = windupEnd, activeEnd = activeEnd,
        };

        static WeaponAnimSet SaveSet(string name, WeaponAnimSet set)
        {
            string path = $"{VisualsFolder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<WeaponAnimSet>(path);
            if (existing == null) { AssetDatabase.CreateAsset(set, path); return set; }
            EditorUtility.CopySerialized(set, existing);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        // Klatki ciosów (start/strike/follow/end) z Hell Unleashed (tools/art/blender/export_mocap.py, liczone od 0).
        static WeaponAnimSet SwordShieldSet()
        {
            string ss = Mixamo + "Pro Sword and Shield Pack/sword and shield ";
            var set = ScriptableObject.CreateInstance<WeaponAnimSet>();
            set.grip = GripStyle.OneHand;
            // Spoczynek: swobodne stanie z paczki łuku (broń i tarcza opuszczone przy boku). Bojowe „idle” z paczki
            // miecza wyglądały źle: (4) – szeroki przysiad z tarczą trzymaną płasko, bez nazwy – obrót ciała o 90°,
            // (2) i (3) – gesty, w których broń odrywa się od dłoni.
            set.idle = Loop(Mixamo + "Pro Longbow Pack/unarmed idle 01.fbx");
            // Jak w Hell Unleashed: bohater stoi we własnej postawie bojowej modelu (Meshy „Idle_Combat”: szeroko, miecz uniesiony
            // nad barkiem, tarcza z przodu) – uwaga użytkownika: „w HU wojownik tak trzyma miecz”. Swobodne stanie zostaje dla
            // modeli bez tego klipu. Osobna postawa po ciosie jest wtedy zbędna.
            set.useModelIdle = true;
            set.combatIdle = null;
            set.walk = Loop(ss + "walk.fbx");
            set.walkBack = Loop(ss + "walk (2).fbx");
            set.strafeRight = Loop(ss + "strafe.fbx");
            set.strafeLeft = Loop(ss + "strafe (2).fbx");
            set.run = Loop(ss + "run.fbx");
            // Bieg w tył i bokiem (namierzony cel) – prędkości z ruchu korzenia: 4,1 / 3,2 / 3,1 m/s.
            set.runBack = Loop(ss + "run (2).fbx");
            set.runLeft = Loop(ss + "strafe (3).fbx");
            set.runRight = Loop(ss + "strafe (4).fbx");
            set.walkSpeed = 1.4f; set.runSpeed = 3.87f;
            // Chwyt tarczy z pozy „block idle” (tak samo u wszystkich bohaterów – gniazdo jest wyrównane w T-pose).
            set.hasShieldGrip = true;
            set.shieldGripPosition = new Vector3(-0.145f, -0.026f, 0.008f);
            set.shieldGripEuler = new Vector3(16.8f, 12.4f, 248.1f);
            set.actions = new List<ActionClip>
            {
                Blow(AttackAnim.SlashRight, InPlace(ss + "slash.fbx"), 0, 16, 25, 45),
                // Drugie cięcie: „slash (3)” kończy się cofnięciem wykrocznej nogi o 0,7 m – „slash (5)” dosuwa nogi naprzód.
                Blow(AttackAnim.SlashLeft, InPlace(ss + "slash (5).fbx"), 0, 16, 22, 41),
                Blow(AttackAnim.Thrust, InPlace(Mixamo + "Loose/Standing Melee Combo Attack Ver. 3.fbx", 42, 82), 42, 47, 56, 82),
                Blow(AttackAnim.Overhead, InPlace(Mixamo + "Loose/Standing Melee Attack Downward.fbx"), 0, 24, 29, 68),
                Blow(AttackAnim.Slam, InPlace(Mixamo + "Loose/Standing Melee Attack Downward.fbx"), 0, 24, 29, 68),
                Blow(AttackAnim.Leap, InPlace(ss + "attack (2).fbx"), 0, 15, 22, 39),
                Blow(AttackAnim.Burst, InPlace(Mixamo + "Loose/Standing Melee Attack 360 High.fbx"), 0, 28, 39, 95),
                Act(AnimAction.Block, Loop(ss + "block idle.fbx"), upper: true, loop: true),
                Act(AnimAction.Parry, InPlace(ss + "block.fbx"), windupEnd: 0.2f, activeEnd: 0.45f),
                Act(AnimAction.Cast, InPlace(ss + "casting.fbx")),
                Act(AnimAction.Flinch, InPlace(ss + "impact.fbx")),
                Act(AnimAction.Death, Clip(ss + "death.fbx")),
                Act(AnimAction.Dodge, Roll()),
            };
            return SaveSet("anim_sword_shield", set);
        }

        static WeaponAnimSet GreatswordSet()
        {
            string gs = Mixamo + "Great Sword Pack/great sword ";
            var set = ScriptableObject.CreateInstance<WeaponAnimSet>();
            set.grip = GripStyle.TwoHand;
            set.gripSpan = 0.16f;
            set.idle = Loop(gs + "idle.fbx");
            set.walk = Loop(gs + "walk.fbx");
            set.walkBack = Loop(gs + "walk (2).fbx");
            set.strafeLeft = Loop(gs + "strafe.fbx");
            set.strafeRight = Loop(gs + "strafe (2).fbx");
            set.run = Loop(gs + "run (2).fbx");
            set.runBack = Loop(gs + "run.fbx");
            set.runLeft = Loop(gs + "strafe (3).fbx");
            set.runRight = Loop(gs + "strafe (4).fbx");
            set.walkSpeed = 1.03f; set.runSpeed = 4.08f;
            set.actions = new List<ActionClip>
            {
                Blow(AttackAnim.SlashRight, InPlace(gs + "slash.fbx"), 0, 14, 23, 38),
                // „slash (3)” i „slash (5)” cofają wykroczną nogę na koniec; „slash (4)” – cięcie z lewej z krokiem, który zostaje.
                Blow(AttackAnim.SlashLeft, InPlace(gs + "slash (4).fbx"), 0, 25, 32, 54),
                Blow(AttackAnim.Thrust, InPlace(gs + "attack.fbx"), 0, 15, 20, 36),
                Blow(AttackAnim.Overhead, InPlace(gs + "high spin attack.fbx"), 0, 30, 37, 56),
                Blow(AttackAnim.Slam, InPlace(gs + "high spin attack.fbx"), 0, 30, 37, 56),
                Blow(AttackAnim.Leap, InPlace(gs + "jump attack.fbx"), 0, 29, 38, 65),
                Act(AnimAction.Block, Loop(gs + "blocking (2).fbx"), upper: true, loop: true),
                Act(AnimAction.Parry, InPlace(gs + "blocking.fbx"), windupEnd: 0.2f, activeEnd: 0.45f),
                Act(AnimAction.Cast, InPlace(gs + "casting.fbx")),
                Act(AnimAction.Flinch, InPlace(gs + "impact.fbx")),
                Act(AnimAction.Death, Clip(Mixamo + "Great Sword Pack/two handed sword death.fbx")),
                Act(AnimAction.Dodge, Roll()),
            };
            return SaveSet("anim_greatsword", set);
        }

        static WeaponAnimSet BowSet()
        {
            string ll = Mixamo + "Longbow Locomotion Pack/standing ";
            string lb = Mixamo + "Pro Longbow Pack/standing ";
            var set = ScriptableObject.CreateInstance<WeaponAnimSet>();
            set.grip = GripStyle.Bow;
            set.idle = Loop(ll + "idle 01.fbx");
            set.walk = Loop(ll + "walk forward.fbx");
            set.walkBack = Loop(ll + "walk back.fbx");
            set.strafeLeft = Loop(ll + "walk left.fbx");
            set.strafeRight = Loop(ll + "walk right.fbx");
            set.run = Loop(ll + "run forward.fbx");
            set.runBack = Loop(ll + "run back.fbx");
            set.runLeft = Loop(ll + "run left.fbx");
            set.runRight = Loop(ll + "run right.fbx");
            set.walkSpeed = 1.08f; set.runSpeed = 3.39f;
            // Strzał: naciągnięcie (31 kl.) + odrzut (22 kl.); pełne naciągnięcie w kl. 30, zwolnienie ok. 36.
            var shot = InPlace(Mixamo + "Composed/bow shot.fbx");
            set.actions = new List<ActionClip>
            {
                Blow(AttackAnim.BowDraw, shot, 0, 30, 38, 52),
                Blow(AttackAnim.Auto, shot, 0, 30, 38, 52),
                Act(AnimAction.Block, Loop(lb + "block.fbx"), upper: true, loop: true),
                Act(AnimAction.Flinch, InPlace(lb + "react small from front.fbx")),
                Act(AnimAction.Death, Clip(lb + "death backward 01.fbx")),
                Act(AnimAction.Dodge, Roll()),
                Act(AnimAction.Backstep, Clip(lb + "dodge backward.fbx", dropRoot: true)),
            };
            return SaveSet("anim_bow", set);
        }

        // ------------------------------------------------------------------ Bohaterowie

        static AnimationClip ModelClip(AnimationClip[] clips, string name) =>
            clips.FirstOrDefault(c => c.name.EndsWith("|" + name) || c.name == name);

        static void HeroVisual(string hero, WeaponAnimSet oneHand, WeaponAnimSet twoHand, WeaponAnimSet bow)
        {
            string fbx = Heroes + hero + "/" + hero + ".fbx";
            // Skracanie stóp (FootMesh) czyta wierzchołki w grze – siatka modelu musi mieć Read/Write.
            bool readable = hero == "hero_warrior";
            if (AssetImporter.GetAtPath(fbx) is ModelImporter mi && mi.isReadable != readable) { mi.isReadable = readable; mi.SaveAndReimport(); }
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            if (model == null) { Debug.LogWarning("Brak modelu " + fbx); return; }
            // Klipy w osobnym pliku, z awatarem modelu (T-pose).
            string clipsFbx = Heroes + hero + "/" + hero + "_clips.fbx";
            var avatar = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Avatar>().FirstOrDefault();
            var ci = AssetImporter.GetAtPath(clipsFbx) as ModelImporter;
            if (ci != null && avatar != null && (ci.avatarSetup != ModelImporterAvatarSetup.CopyFromOther || ci.sourceAvatar != avatar))
            {
                ci.animationType = ModelImporterAnimationType.Human;
                ci.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                ci.sourceAvatar = avatar;
                ci.SaveAndReimport();
            }
            var clips = AssetDatabase.LoadAllAssetsAtPath(clipsFbx).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();

            string path = $"{VisualsFolder}/visual_{hero}.asset";
            var def = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(path);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<CharacterVisualDefinition>();
                AssetDatabase.CreateAsset(def, path);
            }
            def.modelPrefab = model;
            def.scale = HeroScale;
            def.rigidBootToes = hero == "paladin" || hero == "hero_warrior";
            // Model wojownika ma za długie buty względem sylwetki (ocena użytkownika: o ~15%).
            def.footLength = hero == "hero_warrior" ? 0.85f : 1f;
            def.attachGear = true;
            def.rightSlot = "HandSlot.R";
            def.leftSlot = "HandSlot.L";
            def.oneHandSet = oneHand;
            def.twoHandSet = twoHand;
            def.bowSet = bow;
            def.staffSet = null;
            // Spokojny „bojowy” spoczynek zamiast kołyszącego Idle_A (jak UseCalmIdle w Hell Unleashed).
            def.idle = ModelClip(clips, "Idle_Combat") ?? ModelClip(clips, "Idle_A");
            def.walk = ModelClip(clips, "Walking_A");
            // Meshy ma tylko chód; bieg bez zestawu broni (kostur, różdżka) – z paczki miecza i tarczy (jedna ręka z przodu).
            def.run = Loop(Mixamo + "Pro Sword and Shield Pack/sword and shield run.fbx");
            def.walkBack = null; def.strafeLeft = null; def.strafeRight = null;
            def.walkSpeed = 1.4f; def.runSpeed = 3.87f;

            var cast = ModelClip(clips, "Cast_Push") ?? ModelClip(clips, "Throw") ?? ModelClip(clips, "Interact");
            var fling = ModelClip(clips, "Cast_Fling") ?? cast;
            def.actions = new List<ActionClip>
            {
                Act(AnimAction.Death, ModelClip(clips, "Death")),
                Act(AnimAction.Flinch, ModelClip(clips, "Hit_A")),
                Act(AnimAction.Block, ModelClip(clips, "Block"), upper: true, loop: true),
                Act(AnimAction.Cast, cast, windupEnd: 0.3f, activeEnd: 0.5f),
                // Pocisk różdżki/kostura: krótki gest z klipu rzucania (czas klipu idzie za fazami ataku).
                new ActionClip { action = AnimAction.Attack, attack = AttackAnim.Cast, clip = fling, windupEnd = 0.3f, activeEnd = 0.5f },
                new ActionClip { action = AnimAction.Attack, attack = AttackAnim.Auto, clip = ModelClip(clips, "Interact") ?? fling, windupEnd = 0.35f, activeEnd = 0.55f },
            };
            def.actions.RemoveAll(a => a.clip == null);
            EditorUtility.SetDirty(def);
        }

        // ------------------------------------------------------------------ Treść gry: klasy, modele przedmiotów

        /// <summary>
        /// Batch: dopisuje treść bohaterów (SyncContent – nowe id), nadpisuje trzy dawne klasy danymi Wojownika/Maga/Łotrzyka
        /// z Hell Unleashed i przypisuje wygląd klasom oraz modele 3D przedmiotom. Pozostałej treści nie zmienia.
        /// </summary>
        public static void ApplyContentBatch()
        {
            try
            {
                Run();
                TurrisSetup.SyncContent();
                TurrisSetup.OverwriteFromDefaults("class_knight", "class_mage", "class_rogue");
                AssignContentAssets();
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>Klasa (id) → model bohatera (folder w Models/Heroes).</summary>
        public static readonly Dictionary<string, string> ClassLooks = new Dictionary<string, string>
        {
            ["class_knight"] = "hero_warrior", ["class_hu_paladin"] = "paladin", ["class_hu_berserker"] = "hero_warrior_berserker", ["class_hu_dual"] = "hero_warrior_dual",
            ["class_mage"] = "mage_witch", ["class_hu_mage_fire"] = "hero_mage_fire", ["class_hu_mage_water"] = "hero_mage_water", ["class_hu_mage_air"] = "hero_mage_air",
            ["class_hu_mage_earth"] = "hero_mage_earth", ["class_hu_summoner"] = "hero_mage_summoner", ["class_hu_battlemage"] = "hero_mage_battle",
            ["class_rogue"] = "rogue_hooded_meshy", ["class_hu_archer"] = "hero_rogue_archer", ["class_hu_assassin"] = "hero_rogue_assassin", ["class_hu_arcane_archer"] = "hero_rogue_arcane_archer",
        };

        /// <summary>Przedmiot (id) → model broni (folder w Models/Weapons), skala i przesunięcie chwytu wzdłuż drzewca.</summary>
        public static readonly Dictionary<string, (string model, float scale, float grip)> ItemModels = new Dictionary<string, (string model, float scale, float grip)>
        {
            ["hu_sword"] = ("knight_sword", 1f, 0f), ["hu_sword_broad"] = ("knight_sword", 1.05f, 0f),
            ["hu_sword_fire"] = ("knight_sword", 1f, 0f), ["hu_sword_water"] = ("knight_sword", 1f, 0f), ["hu_sword_air"] = ("knight_sword", 1f, 0f), ["hu_sword_earth"] = ("knight_sword", 1f, 0f),
            ["hu_greatsword"] = ("knight_sword", 1.3f, 0f), ["hu_greataxe"] = ("greataxe", 1f, 0f), ["hu_warhammer"] = ("warhammer", 0.8f, 0.24f), ["hu_flail"] = ("flail", 1f, 0f),
            ["hu_shield"] = ("knight_shield", 1f, 0f),
            ["hu_wand_fire"] = ("mage_staff_fire", 1f, 0f), ["hu_wand_water"] = ("mage_staff_water", 1f, 0f), ["hu_wand_air"] = ("mage_staff_air", 1f, 0f), ["hu_wand_earth"] = ("mage_staff_earth", 1f, 0f),
            ["hu_grimoire_fire"] = ("mage_grimoire", 1f, 0f), ["hu_grimoire_water"] = ("mage_grimoire", 1f, 0f), ["hu_grimoire_air"] = ("mage_grimoire", 1f, 0f),
            ["hu_grimoire_earth"] = ("mage_grimoire", 1f, 0f), ["hu_grimoire_summoning"] = ("mage_grimoire", 1f, 0f),
            ["hu_bow"] = ("rogue_bow_rigged", 1f, 0f), ["hu_dagger"] = ("rogue_dagger", 1f, 0f), ["hu_poniard"] = ("rogue_dagger", 1f, 0f), ["hu_offdagger"] = ("rogue_dagger", 1f, 0f),
            // Pozostałe bronie HU: warianty i unikaty rysowane modelem swojej bazy (jak WeaponVisual.VariantBases/UniqueBases w HU).
            ["hu_sabre"] = ("knight_sword", 0.98f, 0f), ["hu_broadsword"] = ("knight_sword", 1.1f, 0f), ["hu_unique_szczerbiec"] = ("knight_sword", 1.05f, 0f), ["hu_unique_perun_blade"] = ("knight_sword", 1f, 0f),
            ["hu_greatsword_executioner"] = ("knight_sword", 1.3f, 0f), ["hu_greatsword_flamberge"] = ("knight_sword", 1.35f, 0f), ["hu_unique_last_word"] = ("knight_sword", 1.32f, 0f),
            ["hu_flail_spiked"] = ("flail", 1f, 0f), ["hu_flail_knight"] = ("flail", 1.1f, 0f), ["hu_unique_penance"] = ("flail", 1f, 0f),
            ["hu_morningstar"] = ("morningstar", 1f, 0f), ["hu_morningstar_heavy"] = ("morningstar", 1.1f, 0f), ["hu_unique_svarog_hammer"] = ("warhammer", 0.85f, 0.24f),
            ["hu_dagger_kris"] = ("rogue_dagger", 1f, 0f), ["hu_dagger_misericorde"] = ("rogue_dagger", 1.05f, 0f), ["hu_unique_widows_lament"] = ("rogue_dagger", 1f, 0f),
            ["hu_bow_recurve"] = ("rogue_bow_rigged", 1f, 0f), ["hu_bow_composite"] = ("rogue_bow_rigged", 1.05f, 0f), ["hu_unique_dziewanna_bow"] = ("rogue_bow_rigged", 1.05f, 0f),
            // Bronie Turris z podobnym modelem – postać FBX trzyma je zamiast proceduralnych.
            ["weapon_longsword"] = ("knight_sword", 1f, 0f), ["weapon_flamesword"] = ("knight_sword", 1f, 0f), ["weapon_serratedsword"] = ("knight_sword", 1f, 0f),
            ["weapon_greataxe"] = ("greataxe", 1f, 0f), ["weapon_mace"] = ("morningstar", 1f, 0f), ["weapon_bow"] = ("rogue_bow_rigged", 1f, 0f), ["weapon_dagger"] = ("rogue_dagger", 1f, 0f),
            ["weapon_staff"] = ("mage_staff_earth", 1f, 0f), ["weapon_staff_fire"] = ("mage_staff_fire", 1f, 0f), ["weapon_staff_frost"] = ("mage_staff_water", 1f, 0f),
            ["weapon_staff_storm"] = ("mage_staff_air", 1f, 0f), ["shield_heater"] = ("knight_shield", 1f, 0f),
        };

        public static void AssignContentAssets()
        {
            var real = TurrisSetup.ContentOnDisk();
            foreach (var kv in ClassLooks)
            {
                if (!(real.TryGetValue(kv.Key, out var c) && c is ClassDefinition cls)) { Debug.LogWarning("Brak klasy " + kv.Key); continue; }
                var v = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>($"{VisualsFolder}/visual_{kv.Value}.asset");
                if (v == null) { Debug.LogWarning("Brak wyglądu " + kv.Value); continue; }
                cls.visual = v;
                EditorUtility.SetDirty(cls);
            }
            foreach (var kv in ItemModels)
            {
                if (!(real.TryGetValue(kv.Key, out var c) && c is ItemDefinition item)) continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{Weapons}{kv.Value.model}/{kv.Value.model}.fbx");
                if (model == null) { Debug.LogWarning("Brak modelu " + kv.Value.model); continue; }
                item.model = model; item.modelScale = kv.Value.scale; item.modelGripShift = kv.Value.grip;
                EditorUtility.SetDirty(item);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("HeroAssetImport: przypisano wygląd klas i modele przedmiotów");
        }

        internal static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
