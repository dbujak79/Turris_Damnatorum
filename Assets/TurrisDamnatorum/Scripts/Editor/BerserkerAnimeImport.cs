using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Turris.EditorTools
{
    /// <summary>Celowana podmiana berserkera; kopia jego rigu i wszystkie dotychczasowe klipy.</summary>
    public static class BerserkerAnimeImport
    {
        public const string Folder = "Assets/TurrisDamnatorum/Models/PoC/BerserkerAnime";
        public const string ModelPath = Folder + "/berserker_anime.fbx";
        public const string VisualPath = "Assets/TurrisDamnatorum/Content/Visuals/visual_berserker_anime.asset";
        public const string ClassPath = "Assets/TurrisDamnatorum/Content/Classes/class_hu_berserker.asset";
        public const string OriginalVisual = "Assets/TurrisDamnatorum/Content/Visuals/visual_hero_warrior_berserker.asset";

        [MenuItem("Turris/PoC/Berserker anime/Włącz nowy model")]
        public static void ApplyMenu() { Debug.Log(Apply()); }

        [MenuItem("Turris/PoC/Berserker anime/Przywróć oryginalny model")]
        public static void Restore()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Wymagany tryb edycji.");
            var definition = AssetDatabase.LoadAssetAtPath<ClassDefinition>(ClassPath);
            definition.lookOverride = null;
            EditorUtility.SetDirty(definition); AssetDatabase.SaveAssetIfDirty(definition);
        }

        public static void RunBatch()
        {
            try { Debug.Log("BERSERKER_IMPORT_OK " + Apply()); }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }

        public static string Apply()
        {
            return ApplyCore(Folder,ModelPath,VisualPath,ClassPath,OriginalVisual,false);
        }

        [MenuItem("Turris/PoC/Mag anime/Włącz nowy model")]
        public static void ApplyMageMenu() { Debug.Log(ApplyMage()); }

        [MenuItem("Turris/PoC/Mag anime/Przywróć oryginalny model")]
        public static void RestoreMage()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Wymagany tryb edycji.");
            var definition=AssetDatabase.LoadAssetAtPath<ClassDefinition>("Assets/TurrisDamnatorum/Content/Classes/class_mage.asset");
            definition.lookOverride=null;EditorUtility.SetDirty(definition);AssetDatabase.SaveAssetIfDirty(definition);
        }

        public static string ApplyMage()
        {
            const string folder="Assets/TurrisDamnatorum/Models/PoC/MageAnime";
            return ApplyCore(folder,folder+"/mage_anime.fbx","Assets/TurrisDamnatorum/Content/Visuals/visual_mage_anime.asset",
                "Assets/TurrisDamnatorum/Content/Classes/class_mage.asset","Assets/TurrisDamnatorum/Content/Visuals/visual_mage_witch.asset",true);
        }

        public static void RunBothBatch()
        {
            try { Debug.Log("ANIME_HEROES_IMPORT_OK "+Apply()+" "+ApplyMage()); }
            catch(Exception error) {Debug.LogException(error);EditorApplication.Exit(1);}
        }

        static string ApplyCore(string Folder,string ModelPath,string VisualPath,string ClassPath,string OriginalVisual,bool mage)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Wymagany tryb edycji.");
            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);
            string key=mage?"mage":"berserker";
            string texturePath = Folder + "/"+key+"_anime_base_color.png";
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
            var ti = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            ti.sRGBTexture = true; ti.maxTextureSize = 4096; ti.SaveAndReimport();
            var shader = Shader.Find("Turris/AnimeCreatureMatte");
            if (shader == null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Niepoprawny matowy shader.");
            string materialPath = Folder + "/"+key+"_anime.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, materialPath); }
            material.shader = shader;
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetColor("_BaseColor", mage?new Color(.90f,.88f,.98f):new Color(1f,.94f,.87f));
            material.SetColor("_ShadowTint", new Color(.51f,.56f,.66f));
            material.SetFloat("_AmbientStrength",.40f); material.SetFloat("_MinimumLight",.20f);
            material.SetFloat("_BandSoftness",.14f); material.SetFloat("_KeyLightLimit",.95f);
            material.SetFloat("_TotalLightLimit",1.1f); material.shaderKeywords = Array.Empty<string>();
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
            var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false; importer.isReadable = true;
            importer.optimizeGameObjects = false; importer.optimizeBones = false;
            importer.skinWeights = ModelImporterSkinWeights.Custom;
            importer.maxBonesPerVertex = 4; importer.minBoneWeight = .00001f;
            importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 80f;
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            foreach (var mat in model.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).Distinct())
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),mat.name),material);
            importer.SaveAndReimport();
            ConfigureHumanoid(importer,ModelPath);
            importer.SaveAndReimport();
            model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var animator = model.GetComponentInChildren<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isHuman || !animator.avatar.isValid)
                throw new InvalidOperationException("Brak poprawnego Humanoid — klasa pozostała bez zmian.");
            var template = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(OriginalVisual);
            var visual = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath);
            if (visual == null) { visual = ScriptableObject.CreateInstance<CharacterVisualDefinition>(); AssetDatabase.CreateAsset(visual,VisualPath); }
            EditorUtility.CopySerialized(template,visual);
            visual.name = "visual_"+key+"_anime"; visual.modelPrefab = model;
            visual.scale = 1; visual.positionOffset = Vector3.zero; visual.rotationOffset = Vector3.zero;
            visual.footLength = 1; visual.rigidBootToes = true; visual.sagittalRunFeet = false;
            visual.anatomicalGripSlots = true;
            visual.balancedMageStance = mage;
            visual.anatomicalFeet = true;
            visual.attachGear = true; visual.rightSlot = "HandSlot.R"; visual.leftSlot = "HandSlot.L";
            EditorUtility.SetDirty(visual); AssetDatabase.SaveAssetIfDirty(visual);
            var definition = AssetDatabase.LoadAssetAtPath<ClassDefinition>(ClassPath);
            definition.lookOverride = visual;
            EditorUtility.SetDirty(definition); AssetDatabase.SaveAssetIfDirty(definition);
            Directory.CreateDirectory("TestResults/berserker-anime");
            File.WriteAllText("TestResults/berserker-anime/"+key+"-import.json",JsonUtility.ToJson(new Report {
                model=ModelPath, visual=VisualPath, vertices=model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.vertexCount,
                humanoid=animator.avatar.isValid && animator.avatar.isHuman,
                mage=AssetDatabase.LoadAssetAtPath<ClassDefinition>("Assets/TurrisDamnatorum/Content/Classes/class_mage.asset").Visual.name,
                warrior=AssetDatabase.LoadAssetAtPath<ClassDefinition>("Assets/TurrisDamnatorum/Content/Classes/class_knight.asset").Visual.name
            },true));
            AnimeHeroStyleImport.ApplyModel(Folder,key);
            return key+" anime: Humanoid, istniejące klipy i zestawy broni, malarska korekta materiału; wojownik bez zmian.";
        }

        [Serializable] class Report { public string model,visual,mage,warrior; public int vertices; public bool humanoid; }

        static void ConfigureHumanoid(ModelImporter importer,string ModelPath)
        {
            var map = new Dictionary<HumanBodyBones,string> {
                {HumanBodyBones.Hips,"Hips"},{HumanBodyBones.Spine,"Spine"},{HumanBodyBones.Chest,"Spine1"},
                {HumanBodyBones.UpperChest,"Spine2"},{HumanBodyBones.Neck,"Neck"},{HumanBodyBones.Head,"Head"},
                {HumanBodyBones.LeftShoulder,"LeftShoulder"},{HumanBodyBones.RightShoulder,"RightShoulder"},
                {HumanBodyBones.LeftUpperArm,"LeftArm"},{HumanBodyBones.RightUpperArm,"RightArm"},
                {HumanBodyBones.LeftLowerArm,"LeftForeArm"},{HumanBodyBones.RightLowerArm,"RightForeArm"},
                {HumanBodyBones.LeftHand,"LeftHand"},{HumanBodyBones.RightHand,"RightHand"},
                {HumanBodyBones.LeftUpperLeg,"LeftUpLeg"},{HumanBodyBones.RightUpperLeg,"RightUpLeg"},
                {HumanBodyBones.LeftLowerLeg,"LeftLeg"},{HumanBodyBones.RightLowerLeg,"RightLeg"},
                {HumanBodyBones.LeftFoot,"LeftFoot"},{HumanBodyBones.RightFoot,"RightFoot"},
                {HumanBodyBones.LeftToes,"LeftToeBase"},{HumanBodyBones.RightToes,"RightToeBase"}};
            var description = importer.humanDescription;
            var transforms = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath).GetComponentsInChildren<Transform>(true).ToDictionary(t=>t.name);
            description.skeleton = description.skeleton.Select(b=> {
                if (transforms.TryGetValue(b.name,out var t)) { b.position=t.localPosition;b.rotation=t.localRotation;b.scale=t.localScale; }
                return b;
            }).ToArray();
            description.human = map.Select(p=>new HumanBone {humanName=p.Key.ToString(),boneName="mixamorig:"+p.Value,
                limit=new HumanLimit {useDefaultValues=true}}).ToArray();
            description.upperLegTwist=.5f;description.lowerLegTwist=.5f;
            importer.humanDescription=description;
        }
    }
}
