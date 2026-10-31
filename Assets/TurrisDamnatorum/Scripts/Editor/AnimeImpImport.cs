using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Turris.EditorTools
{
    /// <summary>Kopie rigów i klipów chochlików dla pełnych siatek anime; oryginały pozostają zachowane.</summary>
    public static class AnimeImpImport
    {
        public const string Folder = "Assets/TurrisDamnatorum/Models/PoC/AnimeImps";
        public static string VisualPath(string kind) => "Assets/TurrisDamnatorum/Content/Visuals/visual_imp_" + kind + "_anime.asset";

        [MenuItem("Turris/PoC/Chochliki anime/Przygotuj rigi i klipy")]
        public static void PrepareMenu() { Debug.Log(Prepare()); }

        public static string Prepare()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Wymagany tryb edycji.");
            foreach (var kind in new[] { "fire", "water" }) PrepareOne(kind);
            return "Przygotowano oba modele i kopie pięciu klipów; przypisania przeciwników bez zmian.";
        }

        static void PrepareOne(string kind)
        {
            string key = kind == "fire" ? "imp" : "imp_water";
            string sourcePath = "Assets/TurrisDamnatorum/Models/Demons/" + key + "/" + key + ".fbx";
            string path = Folder + "/imp_" + kind + "_anime.fbx";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            string texturePath = Folder + "/imp_" + kind + "_anime_base_color.png";
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
            var textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            textureImporter.sRGBTexture = true;
            textureImporter.maxTextureSize = 4096;
            textureImporter.SaveAndReimport();
            var shader = Shader.Find("Turris/AnimeCreatureMatte");
            if (shader == null) throw new InvalidOperationException("Brak matowego shadera stworzeń.");
            string matPath = Folder + "/imp_" + kind + "_anime.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, matPath); }
            mat.shader = shader;
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            mat.SetColor("_BaseColor", kind == "fire" ? new Color(.94f,.80f,.74f) : new Color(.70f,.86f,1f));
            mat.SetFloat("_MinimumLight",kind=="fire"?.24f:.18f);
            mat.SetColor("_ShadowTint", new Color(.50f,.57f,.67f));
            mat.SetFloat("_AmbientStrength", .45f);
            mat.SetFloat("_BandSoftness", .13f);
            mat.SetFloat("_KeyLightLimit", .95f);
            mat.SetFloat("_TotalLightLimit", 1.1f);
            mat.shaderKeywords = Array.Empty<string>();
            EditorUtility.SetDirty(mat); AssetDatabase.SaveAssetIfDirty(mat);
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false;
            importer.optimizeGameObjects = false;
            importer.optimizeBones = false;
            importer.isReadable = true;
            importer.skinWeights = ModelImporterSkinWeights.Custom;
            importer.maxBonesPerVertex = 4;
            importer.minBoneWeight = .0001f;
            importer.importNormals = ModelImporterNormals.Calculate;
            importer.normalSmoothingAngle = 80f;
            var first = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (var original in first.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct())
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), original.name), mat);
            importer.SaveAndReimport();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            PrepareClips(kind, sourcePath, model, sourceModel);
        }

        public static string RefreshAnimations()
        {
            foreach(var kind in new[]{"fire","water"})
            {
                string key=kind=="fire"?"imp":"imp_water";
                string sourcePath="Assets/TurrisDamnatorum/Models/Demons/"+key+"/"+key+".fbx";
                PrepareClips(kind,sourcePath,AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/imp_"+kind+"_anime.fbx"),AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath));
            }
            return "Odświeżono kopie animacji na podstawie bieżących rigów.";
        }

        static void PrepareClips(string kind,string sourcePath,GameObject model,GameObject sourceModel)
        {
            string key=kind=="fire"?"imp":"imp_water";
            var clips = new Dictionary<AnimationClip, AnimationClip>();
            foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(sourcePath).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")))
                clips[clip] = Retarget(clip, sourceModel.transform, model.transform, kind);
            string originalVisual = "Assets/TurrisDamnatorum/Content/Visuals/visual_demon_" + key + ".asset";
            var sourceVisual = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(originalVisual);
            var visual = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath(kind));
            if (visual == null) { visual = ScriptableObject.CreateInstance<CharacterVisualDefinition>(); AssetDatabase.CreateAsset(visual, VisualPath(kind)); }
            EditorUtility.CopySerialized(sourceVisual, visual);
            visual.name = "visual_imp_" + kind + "_anime";
            visual.positionOffset = new Vector3(0,kind=="fire"?.078f:0,0);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(model);
            try
            {
                // Rig Generic sterowany jawnymi krzywymi Transform; awatar importera bez animacji pomija część kości.
                var animator=instance.GetComponentInChildren<Animator>();
                if(animator==null) animator=instance.AddComponent<Animator>();
                animator.avatar=null;
                visual.modelPrefab=PrefabUtility.SaveAsPrefabAsset(instance,Folder+"/imp_"+kind+"_anime.prefab");
            }
            finally{UnityEngine.Object.DestroyImmediate(instance);}
            visual.idle = clips[sourceVisual.idle]; visual.walk = clips[sourceVisual.walk];
            foreach (var action in visual.actions) if (action.clip != null && clips.ContainsKey(action.clip)) action.clip = clips[action.clip];
            EditorUtility.SetDirty(visual); AssetDatabase.SaveAssetIfDirty(visual);
        }

        static AnimationClip Retarget(AnimationClip source, Transform oldRoot, Transform newRoot, string kind)
        {
            var oldRest=BindPose(oldRoot);var newRest=BindPose(newRoot);
            string path = Folder + "/imp_" + kind + "_" + source.name + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
            else { var empty=new AnimationClip(); EditorUtility.CopySerialized(empty,clip);UnityEngine.Object.DestroyImmediate(empty); }
            clip.name = "imp_" + kind + "_" + source.name;
            clip.legacy = false;
            clip.frameRate=source.frameRate;
            // Nowy klip z jawnymi krzywymi: kopia zoptymalizowanego klipu FBX zachowuje jego natywne wiązania.
            var bindings = AnimationUtility.GetCurveBindings(source);
            foreach(var binding in bindings) AnimationUtility.SetEditorCurve(clip,binding,AnimationUtility.GetEditorCurve(source,binding));
            foreach(var binding in AnimationUtility.GetObjectReferenceCurveBindings(source))
                AnimationUtility.SetObjectReferenceCurve(clip,binding,AnimationUtility.GetObjectReferenceCurve(source,binding));
            AnimationUtility.SetAnimationClipSettings(clip,AnimationUtility.GetAnimationClipSettings(source));
            AnimationUtility.SetAnimationEvents(clip,AnimationUtility.GetAnimationEvents(source));
            foreach (var binding in bindings.Where(b => b.propertyName.StartsWith("m_LocalPosition.")))
            {
                var oldBone = oldRoot.Find(binding.path); var newBone = newRoot.Find(binding.path);
                if (oldBone == null || newBone == null) throw new InvalidOperationException("Niezgodna ścieżka klipu: " + binding.path);
                int axis = "xyz".IndexOf(binding.propertyName.Last());
                var curve = AnimationUtility.GetEditorCurve(source, binding);
                var keys = curve.keys;
                float offset = newRest[newBone].GetColumn(3)[axis] - oldRest[oldBone].GetColumn(3)[axis];
                for (int i = 0; i < keys.Length; i++) keys[i].value += offset;
                curve.keys = keys; AnimationUtility.SetEditorCurve(clip, binding, curve);
            }
            foreach (var group in bindings.Where(b => b.propertyName.StartsWith("m_LocalRotation.")).GroupBy(b => b.path))
            {
                var oldBone = oldRoot.Find(group.Key); var newBone = newRoot.Find(group.Key);
                if (oldBone == null || newBone == null) continue;
                var correction = newRest[newBone].rotation * Quaternion.Inverse(oldRest[oldBone].rotation);
                if (Quaternion.Angle(correction, Quaternion.identity) < .001f) continue;
                var curves = new AnimationCurve[4];
                foreach (var binding in group) curves["xyzw".IndexOf(binding.propertyName.Last())] = AnimationUtility.GetEditorCurve(source, binding);
                if (curves.Any(c => c == null)) throw new InvalidOperationException("Niepełny kwaternion " + group.Key);
                var times = curves.SelectMany(c => c.keys.Select(k => k.time)).Distinct().OrderBy(t => t).ToArray();
                var keys = Enumerable.Range(0,4).Select(i => new Keyframe[times.Length]).ToArray();
                Quaternion previous = Quaternion.identity;
                for (int k = 0; k < times.Length; k++)
                {
                    var q = correction * new Quaternion(curves[0].Evaluate(times[k]),curves[1].Evaluate(times[k]),curves[2].Evaluate(times[k]),curves[3].Evaluate(times[k]));
                    if (k > 0 && Quaternion.Dot(q, previous) < 0) q = new Quaternion(-q.x,-q.y,-q.z,-q.w);
                    previous=q;
                    for (int i=0;i<4;i++) keys[i][k]=new Keyframe(times[k],q[i]);
                }
                for (int i=0;i<4;i++) AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(group.Key,typeof(Transform),"m_LocalRotation."+"xyzw"[i]),new AnimationCurve(keys[i]));
            }
            if (kind == "fire") AddTail(clip,newRoot,source.name);
            clip.EnsureQuaternionContinuity();
            EditorUtility.SetDirty(clip); AssetDatabase.SaveAssetIfDirty(clip);
            return clip;
        }

        static Dictionary<Transform,Matrix4x4> BindPose(Transform root)
        {
            var worlds=new Dictionary<Transform,Matrix4x4>();
            foreach(var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var poses=renderer.sharedMesh.bindposes;
                for(int i=0;i<renderer.bones.Length;i++) worlds[renderer.bones[i]]=renderer.transform.localToWorldMatrix*poses[i].inverse;
            }
            var locals=new Dictionary<Transform,Matrix4x4>();
            foreach(var bone in root.GetComponentsInChildren<Transform>(true))
            {
                if(worlds.ContainsKey(bone))
                {
                    var parent=bone.parent!=null && worlds.ContainsKey(bone.parent)?worlds[bone.parent]:bone.parent.localToWorldMatrix;
                    locals[bone]=parent.inverse*worlds[bone];
                }
                else locals[bone]=Matrix4x4.TRS(bone.localPosition,bone.localRotation,bone.localScale);
            }
            return locals;
        }

        static void AddTail(AnimationClip clip,Transform root,string sourceName)
        {
            for(int segment=1;segment<=3;segment++)
            {
                var bone=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="tail"+segment);
                string path=AnimationUtility.CalculateTransformPath(bone,root);
                int frames=Mathf.CeilToInt(clip.length*30);
                var keys=Enumerable.Range(0,4).Select(i=>new Keyframe[frames+1]).ToArray();
                for(int f=0;f<=frames;f++)
                {
                    float u=f/(float)frames;
                    float envelope=sourceName=="Death"?1-u:1;
                    float angle=(4+segment*2)*Mathf.Sin(u*Mathf.PI*2-(segment-1)*.6f)*envelope;
                    var q=bone.localRotation*Quaternion.AngleAxis(angle,Vector3.forward);
                    for(int i=0;i<4;i++) keys[i][f]=new Keyframe(u*clip.length,q[i]);
                }
                for(int i=0;i<4;i++) AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(path,typeof(Transform),"m_LocalRotation."+"xyzw"[i]),new AnimationCurve(keys[i]));
            }
        }

        [MenuItem("Turris/PoC/Chochliki anime/Włącz ogień i wodę")]
        public static void Apply()
        {
            EnsureEditMode();
            Assign("enemy_imp","fire"); Assign("enemy_imp_frost","water");
        }
        [MenuItem("Turris/PoC/Chochliki anime/Przywróć oryginalne modele")]
        public static void Restore()
        {
            EnsureEditMode();
            foreach(var key in new[]{"imp","imp_water"})
            {
                string id=key=="imp"?"enemy_imp":"enemy_imp_frost";
                var enemy=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/TurrisDamnatorum/Content/Enemies/"+id+".asset");
                enemy.visual=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>("Assets/TurrisDamnatorum/Content/Visuals/visual_demon_"+key+".asset");
                EditorUtility.SetDirty(enemy);AssetDatabase.SaveAssetIfDirty(enemy);
            }
        }
        static void Assign(string id,string kind)
        {
            var enemy=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/TurrisDamnatorum/Content/Enemies/"+id+".asset");
            var visual=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath(kind));
            if(visual==null) throw new InvalidOperationException("Najpierw przygotuj rigi anime.");
            enemy.visual=visual; EditorUtility.SetDirty(enemy);AssetDatabase.SaveAssetIfDirty(enemy);
        }
        static void EnsureEditMode(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Zmiana przypisań wymaga trybu edycji.");}
        public static void UpdateMaterials()
        {
            EnsureEditMode();
            foreach(var kind in new[]{"fire","water"})
            {
                var mat=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/imp_"+kind+"_anime.mat");
                mat.SetColor("_BaseColor",kind=="fire"?new Color(.94f,.80f,.74f):new Color(.70f,.86f,1f));
                mat.SetFloat("_MinimumLight",kind=="fire"?.24f:.18f);
                EditorUtility.SetDirty(mat);AssetDatabase.SaveAssetIfDirty(mat);
                var visual=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(VisualPath(kind));
                visual.positionOffset=new Vector3(0,kind=="fire"?.078f:0,0);
                EditorUtility.SetDirty(visual);AssetDatabase.SaveAssetIfDirty(visual);
            }
        }
    }
}
