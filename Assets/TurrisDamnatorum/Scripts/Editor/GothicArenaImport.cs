using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Turris.EditorTools
{
    /// <summary>Nowe modele i definicja PoC. Żaden stary model, materiał ani arena nie jest nadpisywany.</summary>
    public static class GothicArenaImport
    {
        public const string Folder="Assets/TurrisDamnatorum/Models/PoC/GothicArena";
        public const string ArenaPath="Assets/TurrisDamnatorum/Content/Arenas/arena_courtyard_gothic.asset";
        public const string SetPath=Folder+"/gothic_arena_set.asset";
        const string Legacy="Assets/TurrisDamnatorum/Content/Arenas/arena_courtyard.asset";
        const string Tower="Assets/TurrisDamnatorum/Content/Tower/tower_main.asset";

        [MenuItem("Turris/PoC/Arena gotycka/Włącz nowy dziedziniec")]
        public static void EnableMenu() => Debug.Log(Run());

        [MenuItem("Turris/PoC/Arena gotycka/Przywróć poprzedni dziedziniec")]
        public static void Restore()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Przełączenie wymaga trybu edycji.");
            var tower=AssetDatabase.LoadAssetAtPath<TowerDefinition>(Tower);
            tower.floors[0].arena=AssetDatabase.LoadAssetAtPath<ArenaDefinition>(Legacy);
            EditorUtility.SetDirty(tower); AssetDatabase.SaveAssetIfDirty(tower);
            Debug.Log("Przywrócono oryginalną arenę pierwszego piętra; nowe moduły zachowane.");
        }

        public static string Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Import wymaga trybu edycji.");
            EnsureFolder(Folder+"/Meshes"); EnsureFolder(Folder+"/Prefabs");
            var modelPath=Folder+"/gothic_arena_modules.fbx";
            AssetDatabase.ImportAsset(modelPath,ImportAssetOptions.ForceSynchronousImport);
            var imp=(ModelImporter)AssetImporter.GetAtPath(modelPath);
            imp.isReadable=true; imp.importAnimation=false; imp.importNormals=ModelImporterNormals.Import;
            imp.SaveAndReimport();
            AssetDatabase.ImportAsset(Folder+"/night-sky.png",ImportAssetOptions.ForceSynchronousImport);
            var ti=(TextureImporter)AssetImporter.GetAtPath(Folder+"/night-sky.png");
            ti.sRGBTexture=true; ti.maxTextureSize=4096; ti.wrapModeU=TextureWrapMode.Repeat; ti.wrapModeV=TextureWrapMode.Clamp;
            ti.mipmapEnabled=true; ti.SaveAndReimport();
            AssetDatabase.ImportAsset(Folder+"/painted-limestone.png",ImportAssetOptions.ForceSynchronousImport);
            var detail=(TextureImporter)AssetImporter.GetAtPath(Folder+"/painted-limestone.png");
            detail.sRGBTexture=true; detail.wrapMode=TextureWrapMode.Repeat; detail.mipmapEnabled=true; detail.anisoLevel=8;
            detail.SaveAndReimport();
            var shader=Shader.Find("Turris/GothicStone");
            if (shader==null) throw new InvalidOperationException("Brak shadera kamienia gotyckiego.");
            var stone=Material("gothic_stone",shader); var cloth=Material("gothic_cloth",shader);
            var backdrop=Material("gothic_distance",shader); var ground=Material("gothic_ground",shader);
            foreach (var mat in new[]{stone,cloth,backdrop,ground})
            {
                mat.SetColor("_BaseColor",Color.white); mat.SetColor("_ShadowTint",new Color(.52f,.60f,.74f));
                mat.SetFloat("_AmbientStrength",.75f); mat.SetFloat("_KeyLightLimit",1.5f); mat.SetFloat("_BandSoftness",.14f);
                mat.SetFloat("_FogWeight",1); mat.SetFloat("_Unlit",0); mat.SetFloat("_Wind",0); mat.SetFloat("_Cull",2);
                mat.SetFloat("_DetailStrength",0); mat.SetColor("_EmissionColor",Color.black);
            }
            stone.SetTexture("_DetailMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/painted-limestone.png"));
            stone.SetFloat("_DetailStrength",.55f);
            cloth.SetFloat("_Wind",.035f); cloth.SetFloat("_Cull",0); cloth.SetColor("_BaseColor",new Color(1.35f,1,1));
            backdrop.SetFloat("_Unlit",1); backdrop.SetFloat("_FogWeight",.22f); backdrop.SetColor("_BaseColor",Color.white);
            ground.SetColor("_BaseColor",new Color(.18f,.19f,.21f));
            var sky=Material("gothic_sky",Shader.Find("Turris/GothicNightSky"));
            sky.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/night-sky.png"));
            sky.SetFloat("_Rotation",.14f); sky.SetFloat("_Exposure",.65f);
            foreach (var mat in new[]{stone,cloth,backdrop,ground,sky}) { EditorUtility.SetDirty(mat); AssetDatabase.SaveAssetIfDirty(mat); }
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            var prefabs=model.GetComponentsInChildren<MeshFilter>(true).Select(filter=>
            {
                string name=filter.gameObject.name;
                var source=filter.sharedMesh;
                var copy=UnityEngine.Object.Instantiate(source); copy.name=name;
                Matrix4x4 matrix=filter.transform.localToWorldMatrix;
                copy.vertices=source.vertices.Select(matrix.MultiplyPoint3x4).ToArray();
                var normalMatrix=matrix.inverse.transpose;
                copy.normals=source.normals.Select(n=>normalMatrix.MultiplyVector(n).normalized).ToArray();
                copy.RecalculateBounds();
                string meshPath=Folder+"/Meshes/"+name+".asset";
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (mesh==null) { mesh=copy; AssetDatabase.CreateAsset(mesh,meshPath); }
                else { EditorUtility.CopySerialized(copy,mesh); UnityEngine.Object.DestroyImmediate(copy); EditorUtility.SetDirty(mesh); AssetDatabase.SaveAssetIfDirty(mesh); }
                var go=new GameObject(name); go.AddComponent<MeshFilter>().sharedMesh=mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial=name=="Sztandar_Krzyz"?cloth:name=="Zamek_Na_Skale"?backdrop:stone;
                var prefab=PrefabUtility.SaveAsPrefabAsset(go,Folder+"/Prefabs/"+name+".prefab");
                UnityEngine.Object.DestroyImmediate(go);
                return prefab;
            }).ToArray();
            var kit=AssetDatabase.LoadAssetAtPath<GothicArenaSet>(SetPath);
            if (kit==null) { kit=ScriptableObject.CreateInstance<GothicArenaSet>(); AssetDatabase.CreateAsset(kit,SetPath); }
            kit.modules=prefabs; kit.stone=stone; kit.cloth=cloth; kit.backdrop=backdrop; kit.ground=ground; kit.sky=sky;
            kit.originalArena=AssetDatabase.LoadAssetAtPath<ArenaDefinition>(Legacy);
            EditorUtility.SetDirty(kit); AssetDatabase.SaveAssetIfDirty(kit);
            var arena=AssetDatabase.LoadAssetAtPath<ArenaDefinition>(ArenaPath);
            if (arena==null) { arena=ScriptableObject.CreateInstance<ArenaDefinition>(); AssetDatabase.CreateAsset(arena,ArenaPath); }
            EditorUtility.CopySerialized(kit.originalArena,arena);
            arena.name="arena_courtyard_gothic"; arena.id="arena_courtyard_gothic"; arena.gothicSet=kit;
            arena.fogColor=new Color(.035f,.055f,.09f);
            EditorUtility.SetDirty(arena); AssetDatabase.SaveAssetIfDirty(arena);
            var tower=AssetDatabase.LoadAssetAtPath<TowerDefinition>(Tower);
            tower.floors[0].arena=arena;
            EditorUtility.SetDirty(tower); AssetDatabase.SaveAssetIfDirty(tower);
            return "Gotycki dziedziniec włączony na pierwszym piętrze. Modułów: "+prefabs.Length+". Oryginalna arena i pozostałe piętra zachowane.";
        }

        static Material Material(string name,Shader shader)
        {
            var path=Folder+"/"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material==null) { material=new Material(shader); AssetDatabase.CreateAsset(material,path); }
            material.shader=shader; return material;
        }

        static void EnsureFolder(string path)
        {
            string[] parts=path.Split('/'); string current=parts[0];
            foreach (string part in parts.Skip(1)) { if (!AssetDatabase.IsValidFolder(current+"/"+part)) AssetDatabase.CreateFolder(current,part); current+="/"+part; }
        }
    }
}
