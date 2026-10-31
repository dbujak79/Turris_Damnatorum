using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Turris.EditorTools
{
    /// <summary>Odwracalna korekta wyłącznie materiałów maga i berserkera; źródłowe FBX i mapy pozostają nietknięte.</summary>
    public static class AnimeHeroStyleImport
    {
        const string Models = "Assets/TurrisDamnatorum/Models/PoC/";
        public const string ShaderName = "Turris/AnimeHeroPainterly";

        [MenuItem("Turris/PoC/Korekta stylu bohaterów/Włącz korektę maga i berserkera")]
        public static void Apply()
        {
            RequireEditMode();
            ApplyModel(Models+"MageAnime","mage");
            ApplyModel(Models+"BerserkerAnime","berserker");
            Debug.Log("ANIME_HERO_STYLE_OK: mag i berserker; oryginalne materiały i tekstury zachowane.");
        }

        public static void RunBatch()
        {
            try { Apply(); }
            catch(Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }

        [MenuItem("Turris/PoC/Korekta stylu bohaterów/Przywróć materiały sprzed korekty")]
        public static void Restore()
        {
            RequireEditMode();
            foreach(var key in new[]{"mage","berserker"}) {
                string folder=Models+(key=="mage"?"MageAnime":"BerserkerAnime");
                var original=AssetDatabase.LoadAssetAtPath<Material>(folder+"/"+key+"_anime.mat");
                if(original==null)throw new InvalidOperationException("Brak zachowanego materiału: "+key);
                Remap(folder+"/"+key+"_anime.fbx",original);
            }
            Debug.Log("Przywrócono materiały sprzed korekty, bez zmiany modeli ani animacji.");
        }

        public static void ApplyModel(string folder,string key)
        {
            RequireEditMode();
            var shader=Shader.Find(ShaderName);
            if(shader==null||ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Błąd shadera korekty bohaterów.");
            var baseline=AssetDatabase.LoadAssetAtPath<Material>(folder+"/"+key+"_anime.mat");
            if(baseline==null)throw new InvalidOperationException("Brak materiału źródłowego: "+key);
            string path=folder+"/"+key+"_anime_painterly_v2.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null) {material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
            material.shader=shader;
            material.SetTexture("_BaseMap",baseline.GetTexture("_BaseMap"));
            material.SetTextureScale("_BaseMap",baseline.GetTextureScale("_BaseMap"));
            material.SetTextureOffset("_BaseMap",baseline.GetTextureOffset("_BaseMap"));
            bool mage=key=="mage";
            material.SetColor("_BaseColor",Color.white);
            material.SetColor("_ClothDark",new Color(.13f,.14f,.22f));
            material.SetColor("_ClothLight",new Color(.31f,.29f,.40f));
            material.SetColor("_ShadowTint",new Color(.65f,.70f,.84f));
            material.SetFloat("_ClothMix",mage?.78f:0f);
            material.SetFloat("_SkinMix",mage?0f:.38f);
            material.SetFloat("_PigmentExposure",mage?1.02f:1.12f);
            material.SetFloat("_DetailStrength",mage?.54f:.52f);
            material.SetFloat("_BakedGlintSuppression",mage?.45f:.72f);
            material.SetFloat("_UseStyleMask",1f);
            material.SetColor("_SteelDark",new Color(.20f,.21f,.26f));
            material.SetColor("_SteelLight",new Color(.43f,.42f,.41f));
            material.SetFloat("_EdgeStrength",.18f);
            material.SetFloat("_AmbientStrength",.30f);
            material.SetFloat("_MinimumLight",.26f);
            material.SetFloat("_BandSoftness",.075f);
            material.SetFloat("_KeyLightLimit",1f);
            material.SetFloat("_TotalLightLimit",1.15f);
            material.SetColor("_EmissionColor",Color.clear);
            material.shaderKeywords=Array.Empty<string>();
            EditorUtility.SetDirty(material);AssetDatabase.SaveAssetIfDirty(material);
            Remap(folder+"/"+key+"_anime.fbx",material);

            string dir="TestResults/hero-style-v2";Directory.CreateDirectory(dir);
            File.WriteAllText(dir+"/"+key+"-import.json",JsonUtility.ToJson(new Report {
                material=path,baseline=AssetDatabase.GetAssetPath(baseline),shader=shader.name,
                modelSHA256=Sha(folder+"/"+key+"_anime.fbx"),
                textureSHA256=Sha(AssetDatabase.GetAssetPath(baseline.GetTexture("_BaseMap"))),
                colorSpace=QualitySettings.activeColorSpace.ToString(),graphicsApi=SystemInfo.graphicsDeviceType.ToString(),
                pipeline=GraphicsSettings.currentRenderPipeline!=null?GraphicsSettings.currentRenderPipeline.name:"brak",
                shaderErrors=ShaderUtil.GetShaderMessages(shader).Count(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error)
            },true));
        }

        static void Remap(string modelPath,Material material)
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
            var ids=importer.GetExternalObjectMap().Keys.Where(k=>k.type==typeof(Material)).ToArray();
            if(ids.Length==0) {
                var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                ids=model.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null)
                    .Select(m=>new AssetImporter.SourceAssetIdentifier(typeof(Material),m.name)).Distinct().ToArray();
            }
            if(ids.Length==0)throw new InvalidOperationException("Model nie ma identyfikatora materiału: "+modelPath);
            foreach(var id in ids)importer.AddRemap(id,material);
            importer.SaveAndReimport();
        }

        static void RequireEditMode() {if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Wymagany tryb edycji.");}
        static string Sha(string path) {using(var sha=SHA256.Create())using(var stream=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
        [Serializable] class Report {public string material,baseline,shader,modelSHA256,textureSHA256,colorSpace,graphicsApi,pipeline;public int shaderErrors;}
    }
}
