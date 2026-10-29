using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Turris
{
    /// <summary>Dziedziniec z autorskich modułów 3D: ukruszone płyty, mury, ostrołuki, kolumny, haft i zamek.</summary>
    public static class GothicArenaBuilder
    {
        sealed class Builder
        {
            readonly GothicArenaSet kit;
            readonly Transform root;
            readonly System.Random random = new System.Random(261026);
            readonly Dictionary<string,List<CombineInstance>> chunks = new Dictionary<string,List<CombineInstance>>();
            public Builder(GothicArenaSet kit,Transform root) { this.kit=kit; this.root=root; }
            public float R(float a,float b) => Mathf.Lerp(a,b,(float)random.NextDouble());
            public int I(int n) => random.Next(n);

            public void Put(string name,Vector3 pos,Vector3 euler,Vector3 scale)
            {
                var module=kit.Module(name);
                if (module==null) throw new InvalidOperationException("Brak modułu gotyckiej areny: "+name);
                if (name=="Sztandar_Krzyz" || name=="Zamek_Na_Skale")
                {
                    var obj=UnityEngine.Object.Instantiate(module,root,false);
                    obj.name=name; obj.transform.SetLocalPositionAndRotation(pos,Quaternion.Euler(euler)); obj.transform.localScale=scale;
                    return;
                }
                string key=Mathf.FloorToInt(pos.x/9)+"_"+Mathf.FloorToInt(pos.z/9);
                if (!chunks.TryGetValue(key,out var list)) chunks[key]=list=new List<CombineInstance>();
                var filter=module.GetComponent<MeshFilter>();
                list.Add(new CombineInstance { mesh=filter.sharedMesh, transform=Matrix4x4.TRS(pos,Quaternion.Euler(euler),scale) });
            }

            public void Bake()
            {
                var owner=root.GetComponent<GothicArenaMeshes>() ?? root.gameObject.AddComponent<GothicArenaMeshes>();
                foreach (var pair in chunks)
                {
                    var mesh=new Mesh { name="GotyckiDziedziniec_"+pair.Key,indexFormat=IndexFormat.UInt32 };
                    mesh.CombineMeshes(pair.Value.ToArray(),true,true); mesh.RecalculateBounds();
                    owner.meshes.Add(mesh);
                    var go=new GameObject(mesh.name); go.transform.SetParent(root,false);
                    go.AddComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=go.AddComponent<MeshRenderer>(); renderer.sharedMaterial=kit.stone;
                    renderer.shadowCastingMode=ShadowCastingMode.On; renderer.receiveShadows=true;
                }
            }
        }

        public static GameObject Build(ArenaDefinition definition,Transform parent)
        {
            var kit=definition.gothicSet;
            var root=new GameObject("Arena_Gotycka_"+definition.id); root.transform.SetParent(parent,false);
            var b=new Builder(kit,root.transform);
            float radius=definition.size, step=1.36f;
            // Równe kolizje pozostają na poziomie zero. Posadzka ma drobną fazę i realne fugi, bez progów w walce.
            Collider(root.transform,"Podloga",new Vector3(0,-.5f,0),new Vector3((radius+3)*2,1,(radius+3)*2));
            var baseFloor=new GameObject("PodlozeFug"); baseFloor.transform.SetParent(root.transform,false);
            var ground=GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name="CiemnePodloze";
            ground.transform.SetParent(baseFloor.transform,false); ground.transform.localPosition=new Vector3(0,-.16f,0);
            ground.transform.localScale=new Vector3(radius*2+.5f,.10f,radius*2+.5f);
            UnityEngine.Object.Destroy(ground.GetComponent<Collider>());
            ground.GetComponent<Renderer>().sharedMaterial=kit.ground;
            int n=Mathf.CeilToInt(radius/step);
            for (int z=-n;z<=n;z++) for (int x=-n;x<=n;x++)
            {
                Vector3 p=new Vector3((x+(z%2)*.5f)*step,0,z*step);
                if (p.sqrMagnitude>(radius+.2f)*(radius+.2f)) continue;
                b.Put("Plyta_"+b.I(14).ToString("00"),p+new Vector3(b.R(-.025f,.025f),-.008f,b.R(-.025f,.025f)),
                    new Vector3(0,b.R(-1.7f,1.7f),0),new Vector3(step-.040f,1,step-.040f));
            }
            int bays=Mathf.RoundToInt(Mathf.PI*2*(radius+.7f)/4.6f);
            float bayWidth=2*(radius+.7f)*Mathf.Sin(Mathf.PI/bays);
            for (int i=0;i<bays;i++)
            {
                float yaw=i*360f/bays;
                Vector3 outward=Quaternion.Euler(0,yaw,0)*Vector3.forward;
                Vector3 pos=outward*(radius+.7f);
                b.Put("Mur_"+(i%3==0?"01":"00"),pos,new Vector3(0,yaw+180,0),new Vector3(bayWidth/4.6f,1,1));
                Collider(root.transform,"MurKolizja",pos+Vector3.up*2.5f,new Vector3(bayWidth+.18f,5,1),yaw);
                // Puste przęsła i różne poziomy złamania pozostawiają czytelny widok na nocny zamek.
                if (i%5!=1 && i%5!=2)
                    b.Put(i%4==0?"Ostroluk_Zlamany":"Ostroluk_Caly",pos+Vector3.up*4.42f,new Vector3(0,yaw+180,0),new Vector3(bayWidth/4.6f,1,1));
                if (i%3==0)
                    b.Put("Sztandar_Krzyz",pos-outward*.55f+Vector3.up*3.95f,new Vector3(0,yaw+180,0),Vector3.one);
                if (i%3==1)
                {
                    Vector3 torch=pos-outward*.6f+Vector3.up*2.3f;
                    b.Put("Pochodnia_Kosz",torch,new Vector3(0,yaw+180,0),Vector3.one);
                    var fire=FxLibrary.Fire(torch-outward*.6f+Vector3.up*.72f,1.2f,root.transform);
                    if(fire!=null) foreach(var glow in fire.GetComponentsInChildren<Light>()) glow.intensity*=.55f;
                    var lightGo=new GameObject("Pochodnia_CiepleSwiatlo"); lightGo.transform.SetParent(root.transform,false);
                    lightGo.transform.localPosition=torch-outward*.9f+Vector3.up*.75f;
                    var light=lightGo.AddComponent<Light>(); light.type=LightType.Point;
                    light.color=new Color(1,.67f,.34f); light.range=8f; light.intensity=2f;
                    light.shadows=LightShadows.None;
                }
            }
            for (int i=0;i<definition.pillars.Count;i++)
            {
                Vector3 p=definition.pillars[i]; bool broken=i==0 || i==2;
                b.Put(broken?"Kolumna_Zlamana":"Kolumna_Cala",p,new Vector3(0,12*i,0),Vector3.one);
                Collider(root.transform,"KolumnaKolizja",p+Vector3.up*2.5f,new Vector3(1.3f,5,1.3f));
                if (broken)
                    b.Put("Trzon_Powalony",p+new Vector3(i==0?-2.2f:2.2f,.4f,.25f),new Vector3(90,15+i*30,0),Vector3.one);
            }
            // Obręcze i promienie są osobną, popękaną intarsją; nie nakładają się na całą posadzkę.
            Inlay(root.transform,kit.stone);
            for (int i=0;i<210;i++)
            {
                float angle=b.R(0,Mathf.PI*2),r=b.R(radius-2.5f,radius-.15f),size=b.R(.10f,.43f);
                b.Put("Gruz_"+b.I(6).ToString("00"),new Vector3(Mathf.Sin(angle)*r,-.02f,Mathf.Cos(angle)*r),
                    new Vector3(b.R(-15,15),b.R(0,360),b.R(-15,15)),new Vector3(size*1.8f,size,size*1.3f));
            }
            b.Put("Zamek_Na_Skale",new Vector3(12,10,118),new Vector3(0,168,0),Vector3.one*.85f);
            foreach(var p in new[]{new Vector3(5.8f,0,-13.8f),new Vector3(-6.5f,0,-13.1f),new Vector3(13.5f,0,5.2f)})
                for(int i=0;i<5;i++)
                    b.Put("Gruz_"+b.I(6).ToString("00"),p+new Vector3(b.R(-.9f,.9f),0,b.R(-.7f,.7f)),
                        new Vector3(0,b.R(0,360),0),Vector3.one*b.R(.35f,.9f));
            b.Bake();
            return root;
        }

        static void Collider(Transform parent,string name,Vector3 pos,Vector3 size,float yaw=0)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false);
            go.transform.localPosition=pos; go.transform.localRotation=Quaternion.Euler(0,yaw,0);
            go.AddComponent<BoxCollider>().size=size;
        }

        static void Inlay(Transform parent,Material material)
        {
            var verts=new List<Vector3>(); var faces=new List<int>(); var colors=new List<Color>();
            Color gold=new Color(.61f,.51f,.36f,1);
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d)
            {
                int k=verts.Count; verts.AddRange(new[]{a,b,c,d}); colors.AddRange(new[]{gold,gold,gold,gold});
                faces.AddRange(Vector3.Cross(b-a,c-a).y>0 ? new[]{k,k+1,k+2,k,k+2,k+3} : new[]{k,k+2,k+1,k,k+3,k+2});
            }
            for (int ring=0;ring<3;ring++)
            {
                float radius=ring==0?1.15f:ring==1?4.2f:5.35f, width=ring==0?.12f:.11f;
                for (int i=0;i<160;i++)
                {
                    if ((i+ring*7)%29==0) continue;
                    float a=i*Mathf.PI*2/160,b=(i+1f)*Mathf.PI*2/160;
                    Vector3 A(float t,float r)=>new Vector3(Mathf.Cos(t)*r,.004f,Mathf.Sin(t)*r);
                    Quad(A(a,radius-width),A(a,radius+width),A(b,radius+width),A(b,radius-width));
                }
            }
            for (int i=0;i<12;i++)
            {
                Quaternion q=Quaternion.Euler(0,i*30,0); float end=i%3==0?5.7f:4.0f;
                Quad(q*new Vector3(-.10f,.005f,1.25f),q*new Vector3(-.10f,.005f,end),q*new Vector3(.10f,.005f,end),q*new Vector3(.10f,.005f,1.25f));
            }
            var mesh=new Mesh { name="Rozeta_Intarsja" }; mesh.SetVertices(verts); mesh.SetTriangles(faces,0); mesh.SetColors(colors); mesh.RecalculateNormals();
            var go=new GameObject(mesh.name); go.transform.SetParent(parent,false); go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=material;
            var owner=parent.GetComponent<GothicArenaMeshes>() ?? parent.gameObject.AddComponent<GothicArenaMeshes>(); owner.meshes.Add(mesh);
        }

        public static void ConfigureLighting(ArenaDefinition arena,Light sun,Camera camera)
        {
            bool gothic=arena.gothicSet!=null;
            sun.color=gothic?new Color(.86f,.80f,.68f):arena.lightColor;
            sun.intensity=gothic?1.05f:RenderingSetup.SunIntensity;
            sun.transform.rotation=Quaternion.Euler(gothic?38:50,-30,0);
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=gothic?new Color(.24f,.29f,.40f):new Color(.32f,.30f,.33f)*RenderingSetup.AmbientScale;
            RenderSettings.skybox=gothic?arena.gothicSet.sky:null;
            RenderSettings.fog=true; RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=arena.fogColor;
            if (gothic) { RenderSettings.fogStartDistance=35; RenderSettings.fogEndDistance=185; }
            if (camera!=null)
            {
                camera.backgroundColor=arena.fogColor;
                camera.clearFlags=gothic?CameraClearFlags.Skybox:CameraClearFlags.SolidColor;
                camera.farClipPlane=Mathf.Max(camera.farClipPlane,300);
            }
        }
    }

    /// <summary>Właściciel wyłącznie siatek zbudowanych dla instancji; importowane moduły nie są zwalniane.</summary>
    public sealed class GothicArenaMeshes : MonoBehaviour
    {
        public readonly List<Mesh> meshes=new List<Mesh>();
        void OnDestroy() { foreach (var mesh in meshes) if (mesh!=null) Destroy(mesh); }
    }
}
