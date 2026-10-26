using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Turris.EditorTools
{
    /// <summary>Celowany import modelu anime i podmiana wyglądu wyłącznie paladyna.</summary>
    public static class PaladinAnimeImport
    {
        public const string Folder = "Assets/TurrisDamnatorum/Models/PoC/PaladinAnime";
        public const string ModelPath = Folder + "/paladin_anime.fbx";
        public const string VisualPath = "Assets/TurrisDamnatorum/Content/Visuals/visual_paladin_anime.asset";
        public const string PaladinPath = "Assets/TurrisDamnatorum/Content/Classes/class_hu_paladin.asset";
        public const string OneHandSetPath = Folder + "/anim_paladin_anime_onehand.asset";
        public const string LeftCutPath = Folder + "/paladin_slash_left.fbx";
        const string OriginalVisual = "Assets/TurrisDamnatorum/Content/Visuals/visual_paladin.asset";

        [MenuItem("Turris/PoC/Włącz model anime dla paladyna")]
        public static void ImportMenu() => Debug.Log(Run());

        public static string Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Import wyglądu wymaga trybu edycji.");
            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);
            string texturePath = Folder + "/paladin_anime_base_color.png";
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
            var textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (textureImporter == null) throw new InvalidOperationException("Brak tekstury paladyna anime.");
            if (!textureImporter.sRGBTexture)
            {
                textureImporter.sRGBTexture = true;
                textureImporter.SaveAndReimport();
            }
            // Osobny shader rozproszony; HellBodySurface zachowuje go także na instancji w grze.
            var shader = Shader.Find("Turris/AnimeMatte");
            if (shader == null) throw new InvalidOperationException("Brak shadera Turris/AnimeMatte.");
            string materialPath = Folder + "/paladin_anime.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "paladin_anime" };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.shader = shader;
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_SteelTint", new Color(0.84f,0.82f,0.78f));
            material.SetColor("_TrimTint", new Color(1f,0.90f,0.72f));
            material.SetColor("_ShadowTint", new Color(0.58f,0.53f,0.46f));
            material.SetFloat("_AmbientStrength",0.28f);
            material.SetFloat("_KeyLightLimit",0.95f);
            material.SetFloat("_BandSoftness",0.10f);
            material.shaderKeywords = Array.Empty<string>();
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);

            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Brak modelu paladyna anime.");
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false;
            importer.optimizeGameObjects = false; // Gniazda i kości są używane przez kod oraz IK.
            importer.optimizeBones = false;
            importer.isReadable = true;
            ConfigureHumanoid(importer);
            var remaps = importer.GetExternalObjectMap().Keys.Where(key => key.type == typeof(Material)).ToArray();
            if (remaps.Length > 0)
                foreach (var key in remaps) importer.AddRemap(key, material);
            else
            {
                var first = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
                foreach (var original in first.GetComponentsInChildren<Renderer>().SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct())
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), original.name), material);
            }
            importer.SaveAndReimport();

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var animator = model.GetComponentInChildren<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                throw new InvalidOperationException("Model nie ma poprawnego awatara Humanoid; wygląd klasy pozostał bez zmian.");
            var template = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(OriginalVisual);
            if (template == null) throw new InvalidOperationException("Brak istniejących animacji paladyna.");
            var visual = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            if (visual == null)
            {
                visual = ScriptableObject.CreateInstance<CharacterVisualDefinition>();
                AssetDatabase.CreateAsset(visual, VisualPath);
            }
            EditorUtility.CopySerialized(template, visual);
            visual.name = "visual_paladin_anime";
            visual.modelPrefab = model;
            visual.scale = 1f;
            visual.positionOffset = Vector3.zero;
            visual.rotationOffset = Vector3.zero;
            visual.footLength = 1f;
            visual.rigidBootToes = true;
            visual.sagittalRunFeet = true;
            visual.oneHandSet = StandingComboSet(template.oneHandSet);
            visual.attachGear = true;
            visual.rightSlot = "HandSlot.R";
            visual.leftSlot = "HandSlot.L";
            EditorUtility.SetDirty(visual);
            AssetDatabase.SaveAssetIfDirty(visual);

            var paladin = AssetDatabase.LoadAssetAtPath<ClassDefinition>(PaladinPath);
            if (paladin == null) throw new InvalidOperationException("Brak klasy paladyna.");
            paladin.lookOverride = visual;
            EditorUtility.SetDirty(paladin);
            AssetDatabase.SaveAssetIfDirty(paladin);
            return "Paladyn używa modelu anime; własny Humanoid, obecne animacje i sprzęt. Wojownik bez zmian.";
        }

        static WeaponAnimSet StandingComboSet(WeaponAnimSet source)
        {
            // „slash (5)” zawiera głęboki przysiad. Drugi, stojący cios „slash (3)” kończymy przed cofnięciem nogi.
            // Kopia FBX i zestawu należy tylko do paladyna; wspólny importer i klipy pozostałych klas bez zmian.
            const string original = "Assets/TurrisDamnatorum/Models/Animations/Mixamo/Pro Sword and Shield Pack/sword and shield slash (3).fbx";
            if (AssetImporter.GetAtPath(LeftCutPath) == null && !AssetDatabase.CopyAsset(original, LeftCutPath))
                throw new InvalidOperationException("Nie można przygotować stojącego cięcia paladyna.");
            var importer = (ModelImporter)AssetImporter.GetAtPath(LeftCutPath);
            var range = importer.defaultClipAnimations[0];
            range.name = "paladin_slash_left";
            range.firstFrame = 18; range.lastFrame = 24;
            range.loopTime = false;
            range.lockRootRotation = true; range.lockRootHeightY = true; range.lockRootPositionXZ = false;
            range.keepOriginalOrientation = true; range.keepOriginalPositionY = true; range.keepOriginalPositionXZ = true;
            importer.clipAnimations = new[] { range };
            importer.SaveAndReimport();
            var clip = AssetDatabase.LoadAllAssetsAtPath(LeftCutPath).OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview"));
            var set = AssetDatabase.LoadAssetAtPath<WeaponAnimSet>(OneHandSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<WeaponAnimSet>();
                AssetDatabase.CreateAsset(set, OneHandSetPath);
            }
            EditorUtility.CopySerialized(source, set);
            set.name = "anim_paladin_anime_onehand";
            var cut = set.actions.First(a => a.action == AnimAction.Attack && a.attack == AttackAnim.SlashLeft);
            cut.clip = clip;
            cut.windupEnd = (22f-18f)/(24f-18f);
            cut.activeEnd = 1f;
            cut.strikeShare = 1f; // Krótki wycinek: pełny zamach, bez zwalniania drugiego ciosu względem pierwszego.
            cut.recoveryFadeStart = 0.08f; // Powrót do gardy, bez trzymania uniesionej ręki na końcu ciosu.
            cut.fadeWhenFinished = true;
            cut.preparationBlendShare = 1f;
            var root = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.z"));
            cut.rootForward = new AnimationCurve();
            if (root != null)
                for (int i = 0; i <= 48; i++)
                {
                    float u = i/48f;
                    cut.rootForward.AddKey(u,root.Evaluate(u*clip.length)-root.Evaluate(0));
                }
            EditorUtility.SetDirty(set); AssetDatabase.SaveAssetIfDirty(set);
            return set;
        }

        static void ConfigureHumanoid(ModelImporter importer)
        {
            // Automatyczne mapowanie wybierało tylko ważone kości: Spine2 jako Spine, bez klatki, barków i palców stóp.
            // Brak Left/RightToes wyłączał CaptureRestFeet i całą korektę kierunku stóp w ClipAnimationDriver.
            var map = new Dictionary<HumanBodyBones, string>
            {
                [HumanBodyBones.Hips] = "Hips", [HumanBodyBones.Spine] = "Spine",
                [HumanBodyBones.Chest] = "Spine1", [HumanBodyBones.UpperChest] = "Spine2",
                [HumanBodyBones.Neck] = "Neck", [HumanBodyBones.Head] = "Head",
                [HumanBodyBones.LeftShoulder] = "LeftShoulder", [HumanBodyBones.RightShoulder] = "RightShoulder",
                [HumanBodyBones.LeftUpperArm] = "LeftArm", [HumanBodyBones.RightUpperArm] = "RightArm",
                [HumanBodyBones.LeftLowerArm] = "LeftForeArm", [HumanBodyBones.RightLowerArm] = "RightForeArm",
                [HumanBodyBones.LeftHand] = "LeftHand", [HumanBodyBones.RightHand] = "RightHand",
                [HumanBodyBones.LeftUpperLeg] = "LeftUpLeg", [HumanBodyBones.RightUpperLeg] = "RightUpLeg",
                [HumanBodyBones.LeftLowerLeg] = "LeftLeg", [HumanBodyBones.RightLowerLeg] = "RightLeg",
                [HumanBodyBones.LeftFoot] = "LeftFoot", [HumanBodyBones.RightFoot] = "RightFoot",
                [HumanBodyBones.LeftToes] = "LeftToeBase", [HumanBodyBones.RightToes] = "RightToeBase"
            };
            var description = importer.humanDescription;
            description.upperLegTwist = 0.5f;
            description.lowerLegTwist = 0.5f;
            // Unity zachowuje dawną pozę awatara w meta także po zmianie pozy wiązania FBX.
            // Awatar musi być zbudowany z aktualnej pozy T, inaczej prosty model jest retargetowany jak szeroka poza A.
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var transforms = prefab.GetComponentsInChildren<Transform>(true).ToDictionary(t => t.name);
            description.skeleton = description.skeleton.Select(bone =>
            {
                if (transforms.TryGetValue(bone.name, out var t))
                {
                    bone.position = t.localPosition;
                    bone.rotation = t.localRotation;
                    bone.scale = t.localScale;
                }
                return bone;
            }).ToArray();
            description.human = map.Select(pair => new HumanBone
            {
                humanName = pair.Key.ToString(), boneName = "mixamorig:" + pair.Value,
                limit = new HumanLimit { useDefaultValues = true }
            }).ToArray();
            importer.humanDescription = description;
        }
    }
}
