using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Turris.EditorTools
{
    /// <summary>Techniczne maski w kolorach wierzchołków. Nie zmienia pozycji, UV, normalnych, wag ani źródłowego FBX.</summary>
    public sealed class AnimeHeroStyleMasks : AssetPostprocessor
    {
        void OnPostprocessModel(GameObject root)
        {
            bool mage=assetPath=="Assets/TurrisDamnatorum/Models/PoC/MageAnime/mage_anime.fbx";
            bool berserker=assetPath=="Assets/TurrisDamnatorum/Models/PoC/BerserkerAnime/berserker_anime.fbx";
            if(!mage&&!berserker)return;
            string key=mage?"mage":"berserker";
            string texturePath=Path.GetDirectoryName(assetPath)+"/"+key+"_anime_base_color.png";
            if(!File.Exists(texturePath))throw new FileNotFoundException("Brak pigmentu do masek stylu",texturePath);
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false,true);
            try {
                if(!texture.LoadImage(File.ReadAllBytes(texturePath)))throw new InvalidDataException(texturePath);
                var pixels=texture.GetPixels32();int width=texture.width,height=texture.height;
                var bones=root.GetComponentsInChildren<Transform>(true);
                var head=bones.First(t=>t.name=="mixamorig:Head");
                var shoulder=bones.First(t=>t.name=="mixamorig:RightArm");
                foreach(var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
                    var mesh=renderer.sharedMesh;var vertices=mesh.vertices;var uv=mesh.uv;
                    var colors=new Color32[vertices.Length];var matrix=renderer.transform.localToWorldMatrix;
                    int headCount=0,metalCount=0;
                    for(int i=0;i<vertices.Length;i++) {
                        Vector3 point=matrix.MultiplyPoint3x4(vertices[i]);
                        int x=Mathf.Clamp(Mathf.FloorToInt(uv[i].x*width),0,width-1);
                        int y=Mathf.Clamp(Mathf.FloorToInt(uv[i].y*height),0,height-1);
                        Color32 pixel=pixels[y*width+x];
                        float r=pixel.r/255f,g=pixel.g/255f,b=pixel.b/255f;
                        float luma=.2126f*r+.7152f*g+.0722f*b;
                        float chroma=Mathf.Max(r,Mathf.Max(g,b))-Mathf.Min(r,Mathf.Min(g,b));
                        // Maska jest liczona w pozie spoczynkowej i podąża z wierzchołkiem za animacją.
                        float headMask=Smooth(head.position.y-.15f,head.position.y-.04f,point.y);
                        float upperCloth=mage?Smooth(head.position.y-.58f,head.position.y-.22f,point.y):0;
                        float metal=0;
                        if(berserker) {
                            float region=1-Smooth(.24f,.34f,Vector3.Distance(point,shoulder.position));
                            float neutral=1-Smooth(.035f,.11f,chroma);
                            float skin=Smooth(.33f,.50f,luma)*Smooth(.02f,.07f,r-g);
                            metal=region*neutral*(1-skin);
                        }
                        colors[i]=new Color32((byte)(headMask*255),(byte)(upperCloth*255),0,(byte)(metal*255));
                        if(headMask>.5f)headCount++;if(metal>.5f)metalCount++;
                    }
                    mesh.colors32=colors;
                    Debug.Log("HERO_STYLE_MASK "+key+" vertices="+vertices.Length+" head="+headCount+" steel="+metalCount);
                }
            }
            finally {Object.DestroyImmediate(texture);}
        }
        static float Smooth(float a,float b,float value) {float t=Mathf.Clamp01((value-a)/(b-a));return t*t*(3-2*t);}
    }
}
