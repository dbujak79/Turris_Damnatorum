using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Turris.Tests
{
    public class GothicArenaTests
    {
        const string Folder="Assets/TurrisDamnatorum/Models/PoC/GothicArena";
        [Test] public void NewDefinition_PreservesOldArenaAndOtherFloors()
        {
            var tower=AssetDatabase.LoadAssetAtPath<TowerDefinition>("Assets/TurrisDamnatorum/Content/Tower/tower_main.asset");
            var legacy=AssetDatabase.LoadAssetAtPath<ArenaDefinition>("Assets/TurrisDamnatorum/Content/Arenas/arena_courtyard.asset");
            var arena=tower.floors[0].arena;
            Assert.AreNotSame(legacy,arena);
            Assert.IsNotNull(arena.gothicSet);
            Assert.AreSame(legacy,arena.gothicSet.originalArena);
            Assert.IsNull(legacy.gothicSet);
            Assert.AreSame(legacy,tower.floors[2].arena); Assert.AreSame(legacy,tower.floors[4].arena);
            Assert.AreEqual(legacy.size,arena.size); CollectionAssert.AreEqual(legacy.pillars,arena.pillars);
        }

        [Test] public void Modules_HavePaintedColorsNormalsAndCorrectDimensions()
        {
            var set=AssetDatabase.LoadAssetAtPath<GothicArenaSet>(Folder+"/gothic_arena_set.asset");
            Assert.AreEqual(30,set.modules.Length);
            foreach (var prefab in set.modules)
            {
                var mesh=prefab.GetComponent<MeshFilter>().sharedMesh;
                Assert.Greater(mesh.vertexCount,10,prefab.name);
                Assert.AreEqual(mesh.vertexCount,mesh.colors.Length,prefab.name+": paleta wierzchołków");
                Assert.AreEqual(mesh.vertexCount,mesh.normals.Length,prefab.name+": normalne");
                Assert.IsFalse(prefab.GetComponentsInChildren<Collider>().Any(),"Moduły artystyczne nie zmieniają kolizji walki");
            }
            var slab=set.Module("Plyta_00").GetComponent<MeshFilter>().sharedMesh.bounds;
            Assert.That(slab.size.x,Is.InRange(.98f,1.02f)); Assert.That(slab.size.z,Is.InRange(.98f,1.02f));
            Assert.That(slab.max.y,Is.InRange(-.002f,.006f),"Detal rysy mieści się w 6 mm nad płaszczyzną płyty");
            var column=set.Module("Kolumna_Cala").GetComponent<MeshFilter>().sharedMesh.bounds;
            Assert.Greater(column.size.y,9f); Assert.Less(column.size.x,1.6f);
            var arch=set.Module("Ostroluk_Caly").GetComponent<MeshFilter>().sharedMesh.bounds;
            Assert.Greater(arch.size.y,6f);
        }

        [Test] public void Materials_CompileAndSkyTextureIsPresent()
        {
            var set=AssetDatabase.LoadAssetAtPath<GothicArenaSet>(Folder+"/gothic_arena_set.asset");
            foreach (var material in new[]{set.stone,set.cloth,set.backdrop,set.ground,set.sky})
                Assert.IsFalse(ShaderUtil.ShaderHasError(material.shader),material.name);
            foreach (string pass in new[]{"AnimeForward","ShadowCaster","DepthOnly","DepthNormals"})
                Assert.GreaterOrEqual(set.stone.FindPass(pass),0,pass);
            Assert.Greater(set.sky.GetTexture("_BaseMap").width,1900);
            Assert.AreEqual(0,set.cloth.GetFloat("_Cull"),"Sztandar widoczny z obu stron");
        }
    }
}
