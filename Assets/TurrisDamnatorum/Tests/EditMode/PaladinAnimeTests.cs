using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Turris.Tests
{
    public class PaladinAnimeTests
    {
        const string Content = "Assets/TurrisDamnatorum/Content/";
        CharacterVisualDefinition Definition => AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Content + "Visuals/visual_paladin_anime.asset");

        [Test] public void Override_ReplacesOnlyPaladin_AndPreservesSharedAnimations()
        {
            var paladin = AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content + "Classes/class_hu_paladin.asset");
            var warrior = AssetDatabase.LoadAssetAtPath<ClassDefinition>(Content + "Classes/class_knight.asset");
            var previous = AssetDatabase.LoadAssetAtPath<CharacterVisualDefinition>(Content + "Visuals/visual_paladin.asset");
            var definition = Definition;
            Assert.AreSame(definition, paladin.lookOverride);
            Assert.AreSame(previous, paladin.visual, "Pierwotny paladyn pozostaje do przywrócenia");
            Assert.IsNull(warrior.lookOverride);
            Assert.AreEqual("visual_hero_warrior", warrior.Visual.name);
            Assert.AreEqual(0.85f, warrior.Visual.footLength, 0.001f);
            Assert.AreNotSame(previous.oneHandSet, definition.oneHandSet, "Korekta drugiego cięcia ma własny zestaw paladyna");
            Assert.AreSame(previous.oneHandSet.run, definition.oneHandSet.run);
            Assert.AreSame(previous.oneHandSet.walk, definition.oneHandSet.walk);
            Assert.AreSame(previous.oneHandSet.idle, definition.oneHandSet.idle);
            foreach (var action in previous.oneHandSet.actions.Where(a => a.attack != AttackAnim.SlashLeft || a.action != AnimAction.Attack))
                Assert.AreSame(action.clip, definition.oneHandSet.actions.First(a => a.action == action.action && a.attack == action.attack).clip);
            Assert.AreEqual("sword and shield slash (5)",previous.oneHandSet.Find(AnimAction.Attack,AttackAnim.SlashLeft).clip.name);
            Assert.AreEqual("paladin_slash_left",definition.oneHandSet.Find(AnimAction.Attack,AttackAnim.SlashLeft).clip.name);
            Assert.AreSame(previous.twoHandSet, definition.twoHandSet);
            Assert.IsTrue(definition.oneHandSet.alternatingLightSteps);
            Assert.IsFalse(previous.oneHandSet.alternatingLightSteps,"Pierwotny zestaw jednoręczny pozostaje bez korekty kroków");
            Assert.IsFalse(warrior.Visual.oneHandSet.alternatingLightSteps,"Wojownik zachowuje swoje animacje");
            Assert.AreSame(previous.idle, definition.idle);
            Assert.AreSame(previous.run, definition.run);
            Assert.AreEqual(previous.actions.Count, definition.actions.Count);
            Assert.IsTrue(definition.rigidBootToes);
            Assert.IsTrue(definition.sagittalRunFeet);
            Assert.IsFalse(warrior.Visual.sagittalRunFeet, "Stabilizacja PoC nie zmienia wojownika");
            Assert.AreEqual(1f, definition.footLength);
        }

        [Test] public void Model_HasHumanoidSkin_AllVerticesWeighted_AndTexture2K()
        {
            var model = Definition.modelPrefab;
            var animator = model.GetComponentInChildren<Animator>();
            Assert.IsNotNull(animator);
            Assert.IsTrue(animator.avatar.isHuman && animator.avatar.isValid);
            Assert.IsNotNull(Util.FindDeep(model.transform, "HandSlot.R"));
            Assert.IsNotNull(Util.FindDeep(model.transform, "HandSlot.L"));
            var skins = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            Assert.AreEqual(1, skins.Length);
            var mesh = skins[0].sharedMesh;
            Assert.AreEqual(45832, mesh.triangles.Length / 3);
            foreach (var weight in mesh.boneWeights)
                Assert.That(weight.weight0 + weight.weight1 + weight.weight2 + weight.weight3, Is.EqualTo(1f).Within(0.0001f));
            var texture = skins[0].sharedMaterial.GetTexture("_BaseMap");
            Assert.IsNotNull(texture);
            Assert.AreEqual(2048, texture.width);
            Assert.AreEqual(2048, texture.height);
            Assert.IsFalse(skins.Any(skin => skin.sharedMaterials.Any(material => material.shader.name == "Hidden/InternalErrorShader")));
            foreach (string name in new[] { "mixamorig:Spine", "mixamorig:Spine1", "mixamorig:Spine2" })
            {
                int bone = System.Array.FindIndex(skins[0].bones, b => b.name == name);
                Assert.GreaterOrEqual(bone,0,name);
                float sum = mesh.boneWeights.Sum(w => (w.boneIndex0==bone?w.weight0:0) + (w.boneIndex1==bone?w.weight1:0)
                    + (w.boneIndex2==bone?w.weight2:0) + (w.boneIndex3==bone?w.weight3:0));
                Assert.Greater(sum,50f,"Tułów nie może wisieć wyłącznie na jednej kości: "+name);
            }
        }

        [Test] public void Avatar_MapsToesAndSpine_AndArmorIsMatte()
        {
            var model = Object.Instantiate(Definition.modelPrefab);
            try
            {
                var animator = model.GetComponentInChildren<Animator>();
                foreach (var bone in new[] { HumanBodyBones.LeftToes, HumanBodyBones.RightToes,
                    HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.UpperChest,
                    HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder })
                    Assert.IsNotNull(animator.GetBoneTransform(bone), "Mapowanie: " + bone);
                var material = model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
                Assert.AreEqual("Turris/AnimeMatte", material.shader.name);
                Assert.IsFalse(material.HasProperty("_Metallic"), "Shader nie ma ścieżki odbić metalu");
                Assert.IsFalse(material.HasProperty("_Smoothness"), "Shader nie ma połysku");
                Assert.IsFalse(ShaderUtil.ShaderHasError(material.shader));
                foreach (var pass in new[] { "AnimeForward", "ShadowCaster", "DepthOnly", "DepthNormals" })
                    Assert.GreaterOrEqual(material.FindPass(pass), 0, pass);
            }
            finally { Object.DestroyImmediate(model); }
        }
    }
}
