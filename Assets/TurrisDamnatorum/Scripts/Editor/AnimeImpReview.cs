using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Turris.EditorTools
{
    /// <summary>Powtarzalna kontrola rzeczywistej siatki w istniejących animacjach Generic.</summary>
    public static class AnimeImpReview
    {
        public static string Capture()
        {
            string dir=Path.GetFullPath("TestResults/anime-imps");Directory.CreateDirectory(dir);
            var lines=new List<string>();
            foreach(var kind in new[]{"fire","water"})
            {
                var def=AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(AnimeImpImport.VisualPath(kind));
                if(def==null) throw new InvalidOperationException("Brak wyglądu "+kind);
                var preview=new PreviewRenderUtility();
                var model=Object.Instantiate(def.modelPrefab);preview.AddSingleGO(model);
                // Pełne krzywe Transform Generic nie potrzebują awatara Humanoid.
                var animator=model.GetComponentInChildren<Animator>();
                if(animator!=null) animator.avatar=null;
                var bake=new Mesh();
                var gallery=new Texture2D(2800,1400,TextureFormat.RGB24,false);
                try
                {
                    var render=model.GetComponentInChildren<SkinnedMeshRenderer>();
                    // Kilka póz w jednej klatce edytora może ponownie użyć poprzedniego bufora GPU skinningu.
                    // Galeria rysuje rzeczywisty BakeMesh bieżącej próbki zamiast takiego bufora.
                    var posedObject=new GameObject("Wypieczona próbka animacji");
                    posedObject.transform.SetParent(render.transform,false);
                    var posedFilter=posedObject.AddComponent<MeshFilter>();
                    posedObject.AddComponent<MeshRenderer>().sharedMaterials=render.sharedMaterials;
                    render.enabled=false;
                    var tris=render.sharedMesh.triangles;
                    var rest=render.sharedMesh.vertices;
                    var attack=def.Find(AnimAction.Attack,AttackAnim.Auto);
                    var special=def.Find(AnimAction.Attack,AttackAnim.Special);
                    if(special==null || special==attack) special=def.actions.First(a=>a.clip!=null && a.clip.name.Contains("Special"));
                    var death=def.Find(AnimAction.Death,AttackAnim.Auto);
                    var poses=new[]{
                        Tuple.Create("idle",def.idle,.3f),Tuple.Create("walk-25",def.walk,def.walk.length*.25f),
                        Tuple.Create("walk-75",def.walk,def.walk.length*.75f),Tuple.Create("windup",attack.clip,attack.clip.length*attack.windupEnd),
                        Tuple.Create("strike",attack.clip,attack.clip.length*attack.activeEnd),Tuple.Create("recovery",attack.clip,Mathf.Min(attack.clip.length,attack.clip.length*attack.activeEnd+.15f)),
                        Tuple.Create("special",special.clip,special.clip.length*special.activeEnd),Tuple.Create("death",death.clip,death.clip.length*.75f)};
                    preview.camera.fieldOfView=32;
                    preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=30;
                    preview.camera.clearFlags=CameraClearFlags.SolidColor;
                    preview.camera.backgroundColor=new Color(.20f,.23f,.29f);
                    preview.camera.transform.position=new Vector3(.7f,1.0f,2.8f);
                    preview.camera.transform.LookAt(new Vector3(0,.62f,0));
                    preview.lights[0].intensity=1.6f;preview.lights[0].transform.rotation=Quaternion.Euler(35,210,0);
                    preview.lights[1].intensity=.9f;preview.lights[1].transform.rotation=Quaternion.Euler(340,280,0);
                    preview.ambientColor=new Color(.4f,.43f,.5f);
                    for(int poseIndex=0;poseIndex<poses.Length;poseIndex++)
                    {
                        var pose=poses[poseIndex];pose.Item2.SampleAnimation(model,pose.Item3);
                        render.BakeMesh(bake);
                        var vertices=bake.vertices;
                        if(vertices.Any(v=>float.IsNaN(v.x)||float.IsInfinity(v.x)||float.IsNaN(v.y)||float.IsInfinity(v.y)||float.IsNaN(v.z)||float.IsInfinity(v.z)))
                            throw new InvalidOperationException("Nieprawidłowa deformacja "+kind+" "+pose.Item1);
                        var stretch=new List<float>();
                        for(int i=0;i<tris.Length-1;i+=Mathf.Max(3,(tris.Length/6000/3)*3))
                        {
                            int a=tris[i],b=tris[i+1];float length=(rest[a]-rest[b]).magnitude;
                            if(length>.002f) stretch.Add((vertices[a]-vertices[b]).magnitude/length);
                        }
                        stretch.Sort();
                        lines.Add(kind+" "+pose.Item1+" t="+pose.Item3.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)
                            +" bounds="+bake.bounds+" stretchP99="+(stretch.Count>0?stretch[(int)((stretch.Count-1)*.99f)]:0).ToString("F4")
                            +" max="+(stretch.Count>0?stretch[stretch.Count-1]:0).ToString("F4"));
                        var posedMesh=Object.Instantiate(bake);posedFilter.sharedMesh=posedMesh;
                        preview.BeginStaticPreview(new Rect(0,0,700,700));preview.Render(true,true);
                        var image=preview.EndStaticPreview();
                        File.WriteAllBytes(Path.Combine(dir,kind+"-"+pose.Item1+".png"),image.EncodeToPNG());
                        gallery.SetPixels((poseIndex%4)*700,(1-poseIndex/4)*700,700,700,image.GetPixels());Object.DestroyImmediate(image);
                        posedFilter.sharedMesh=null;Object.DestroyImmediate(posedMesh);
                    }
                    gallery.Apply();File.WriteAllBytes(Path.Combine(dir,kind+"-gallery.png"),gallery.EncodeToPNG());
                }
                finally{preview.Cleanup();Object.DestroyImmediate(bake);Object.DestroyImmediate(gallery);}
            }
            File.WriteAllLines(Path.Combine(dir,"deformation-metrics.txt"),lines);
            return string.Join("\n",lines);
        }
    }
}
