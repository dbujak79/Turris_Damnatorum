using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace Turris.Tests
{
    /// <summary>Skille wymagają rodzaju broni (nie klasy), a ich obrażenia zależą od siły broni (wzór: Path of Exile 2).</summary>
    public class WeaponFamiliesTests
    {
        Fixture fx;

        [SetUp]
        public void SetUp() => fx = new Fixture();

        IEnumerable<SpellDefinition> AllSpells() => fx.Cfg.baseSpellPool
            .Concat(fx.Cfg.classes.SelectMany(c => c.startingSkillChoices.Concat(c.startingSpells).Append(c.signatureSkill)))
            .Concat(fx.Cfg.unlocks.Select(u => u.target).OfType<SpellDefinition>())
            .Concat(fx.Cfg.baseItemPool.Where(i => i != null && i.grantedSkill != null).Select(i => i.grantedSkill))
            .Where(s => s != null).Distinct();

        static IEnumerable<SpellDefinition> SpellAssets() =>
            AssetDatabase.FindAssets("t:SpellDefinition", new[] { "Assets/TurrisDamnatorum/Content" })
                .Select(g => AssetDatabase.LoadAssetAtPath<SpellDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(a => a != null);

        /// <summary>Wygenerowana treść i assety w projekcie mają wymagania broni zgodne z tabelą.</summary>
        [Test]
        public void SpellAssets_MatchWeaponFamilyTable()
        {
            var wrong = AllSpells().Concat(SpellAssets()).Where(s => s.requiredTags != WeaponFamilies.ForSkill(s))
                .Select(s => $"{s.id}: {s.requiredTags} ≠ {WeaponFamilies.ForSkill(s)}").Distinct().ToList();
            Assert.IsEmpty(wrong, string.Join("\n", wrong));
        }

        /// <summary>Każdy skill poza skillami klas ma jawne przypisanie do broni (także „dowolna broń”).</summary>
        [Test]
        public void EverySkill_HasExplicitWeaponAssignment()
        {
            var missing = AllSpells().Concat(SpellAssets()).Where(s => !WeaponFamilies.IsAssigned(s)).Select(s => s.id).Distinct().ToList();
            Assert.IsEmpty(missing, "bez przypisania do broni: " + string.Join(", ", missing));
        }

        [Test]
        public void Weapons_BelongToFamilies()
        {
            Assert.AreEqual(BuildTag.Blade, WeaponFamilies.Of(Item("hu_sword")));
            Assert.AreEqual(BuildTag.Blade | BuildTag.Dagger, WeaponFamilies.Of(Item("hu_dagger")), "sztylet to też ostrze");
            Assert.AreEqual(BuildTag.Blade | BuildTag.Dagger, WeaponFamilies.Of(Item("hu_sabre")), "szabla: ostrze i sztylet");
            Assert.AreEqual(BuildTag.HeavyWeapon | BuildTag.TwoHanded, WeaponFamilies.Of(Item("hu_warhammer")));
            Assert.AreEqual(BuildTag.HeavyWeapon | BuildTag.TwoHanded, WeaponFamilies.Of(Item("hu_flail2h")), "dwuręczny korbacz – broń ciężka");
            Assert.AreEqual(BuildTag.None, WeaponFamilies.Of(Item("hu_flail")), "jednoręczny korbacz nie jest bronią ciężką");
            Assert.AreEqual(BuildTag.Ranged, WeaponFamilies.Of(Item("hu_bow")));
            Assert.AreEqual(BuildTag.Focus, WeaponFamilies.Of(Item("hu_wand_fire")));
            Assert.AreEqual(BuildTag.Focus, WeaponFamilies.Of(Item("hu_grimoire_fire")), "grymuar w drugiej ręce to fokus");
            // Jednoręczne korbacze i morgensterny celowo bez rodzaju: tylko skille na każdą broń (aury, szarże, okrzyki).
            var unknown = fx.Cfg.baseItemPool.Where(i => i != null && i.IsWeapon && i != fx.Cfg.unarmed && WeaponFamilies.Of(i) == BuildTag.None
                    && !i.id.Contains("flail") && !i.id.Contains("morningstar") && !i.id.Contains("penance"))
                .Select(i => i.id).ToList();
            Assert.IsEmpty(unknown, "broń bez rodzaju: " + string.Join(", ", unknown));
        }

        /// <summary>Szarża: broń dwuręczna albo tarcza; jednoręczną bez tarczy (dwie bronie, sama jedna broń) się nie szarżuje.</summary>
        [Test]
        public void Charge_RequiresTwoHandedWeaponOrShield()
        {
            var charge = fx.Get<SpellDefinition>(TurrisSkills.ChargeTarget);
            Assert.AreEqual(BuildTag.TwoHanded | BuildTag.Shield, charge.requiredTags);
            CollectionAssert.Contains(fx.Cfg.baseSpellPool, charge);
            Assert.IsTrue(WeaponFamilies.Usable(charge, Item("hu_warhammer"), null), "młot dwuręczny");
            Assert.IsTrue(WeaponFamilies.Usable(charge, Item("hu_greatsword"), null), "miecz dwuręczny");
            Assert.IsTrue(WeaponFamilies.Usable(charge, Item("hu_flail"), Item("hu_shield")), "jednoręczna z tarczą – uderzenie tarczą");
            Assert.IsFalse(WeaponFamilies.Usable(charge, Item("hu_sword"), Item("hu_sword_broad")), "dwie jednoręczne");
            Assert.IsFalse(WeaponFamilies.Usable(charge, Item("hu_sword"), null), "sama jednoręczna");
            Assert.IsFalse(WeaponFamilies.Usable(charge, Item("hu_bow"), null), "łuk to nie broń dwuręczna do walki wręcz");
            StringAssert.Contains("dwuręcznej albo tarczy", Names.Requirement(charge.requiredTags));
            // Tarcza nadal jest wymogiem „wszystkie” dla uderzenia tarczą.
            Assert.IsFalse(WeaponFamilies.Meets(BuildTag.Shield, BuildTag.TwoHanded | BuildTag.HeavyWeapon));
        }

        [Test]
        public void EveryClassLoadout_StartsWithUsableSkill()
        {
            var bad = new List<string>();
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            foreach (var c in fx.Cfg.classes)
                for (int l = 0; l < System.Math.Max(1, c.loadouts.Count); l++)
                {
                    var plan = new LoadoutPlan { difficulty = fx.Cfg.difficulties[0] };
                    meta.SelectClass(plan, c);
                    meta.SelectLoadout(plan, l);
                    var s = plan.OpeningSkill;
                    if (s != null && !WeaponFamilies.Usable(s, plan.Weapon, plan.OffHand))
                        bad.Add($"{c.id} / {plan.Weapon?.id}+{plan.OffHand?.id}: skill startowy {s.id} wymaga {s.requiredTags}");
                    foreach (var k in c.startingSpells.Where(k => k != null && !WeaponFamilies.Usable(k, plan.Weapon, plan.OffHand)))
                        bad.Add($"{c.id} / {plan.Weapon?.id}+{plan.OffHand?.id}: skill klasy {k.id} wymaga {k.requiredTags}");
                }
            Assert.IsEmpty(bad, string.Join("\n", bad));
        }

        [Test]
        public void PrepScreen_WarnsWhenExtraWeaponBlocksStartingSkill()
        {
            var meta = new MetaService(fx.Cfg, new MemoryProfileStorage());
            var plan = new LoadoutPlan { difficulty = fx.Cfg.difficulties[0] };
            meta.SelectClass(plan, fx.Get<ClassDefinition>("class_knight"));
            meta.SelectLoadout(plan, 0); // miecz i tarcza
            var wind = fx.Get<SpellDefinition>("hu_wind_slash");
            meta.SelectStartingSkill(plan, wind);
            Assert.IsEmpty(plan.SkillsBlockedByExtras(), "bez dodatków – nic nie blokuje");
            plan.extras.Add(fx.Cfg.unlocks.First(u => u.id == "unlock_staff_storm"));
            CollectionAssert.Contains(plan.SkillsBlockedByExtras(), wind, "kostur z dodatków zastępuje miecz – Cięcie wichru (ostrze) nie zadziała");
        }

        [Test]
        public void SkillDamage_ScalesWithWeaponStrength()
        {
            var cleave = AllSpells().First(s => s.id == "hu_cleave");
            float sword = WeaponFamilies.SkillScale(cleave, cleave.requiredTags, new ItemInstance(Item("hu_sword")), null, fx.Cfg);
            float dagger = WeaponFamilies.SkillScale(cleave, cleave.requiredTags, new ItemInstance(Item("hu_dagger")), null, fx.Cfg);
            float great = WeaponFamilies.SkillScale(cleave, cleave.requiredTags, new ItemInstance(Item("hu_greatsword")), null, fx.Cfg);
            Assert.AreEqual(1f, sword, 0.01f, "miecz jednoręczny to wzorzec");
            Assert.Less(dagger, sword, "skill na sztylecie słabszy niż na mieczu");
            Assert.Greater(great, sword, "miecz dwuręczny mocniejszy");
            float upgraded = WeaponFamilies.SkillScale(cleave, cleave.requiredTags, new ItemInstance(Item("hu_sword"), 2), null, fx.Cfg);
            Assert.AreEqual(1.4f, upgraded, 0.01f, "ulepszenie broni +20% za poziom (jak w HU)");

            var knives = AllSpells().First(s => s.id == "hu_knife_throw");
            Assert.AreEqual(1f, WeaponFamilies.SkillScale(knives, knives.requiredTags, new ItemInstance(Item("hu_dagger")), null, fx.Cfg), 0.01f,
                "skill sztyletu na podstawowym sztylecie – wzorzec (startowe bronie klas bez kary)");
            Assert.Greater(WeaponFamilies.SkillScale(knives, knives.requiredTags, new ItemInstance(Item("hu_dagger_kris")), null, fx.Cfg), 1.2f, "lepszy sztylet – mocniej");

            var orb = AllSpells().First(s => s.id == "hu_infernal_orb");
            Assert.AreEqual(1f, WeaponFamilies.SkillScale(orb, orb.requiredTags, new ItemInstance(Item("hu_wand_water")), null, fx.Cfg), 0.01f,
                "podstawowa różdżka każdego żywiołu to wzorzec czarów");
            Assert.AreEqual(1f, WeaponFamilies.SkillScale(orb, orb.requiredTags, new ItemInstance(Item("hu_wand_fire")), null, fx.Cfg), 0.01f, "różdżka ognia to wzorzec czarów");
            Assert.AreEqual(1f, WeaponFamilies.SkillScale(orb, orb.requiredTags, new ItemInstance(Item("hu_sword")), new ItemInstance(Item("hu_grimoire_fire")), fx.Cfg), 0.01f,
                "miecz + grymuar: czar z grymuaru z mocą wzorcową");
        }

        ItemDefinition Item(string id)
        {
            var it = fx.Cfg.baseItemPool.FirstOrDefault(i => i != null && i.id == id)
                     ?? fx.Cfg.classes.SelectMany(c => c.loadouts).SelectMany(l => new[] { l.weapon, l.offHand }).FirstOrDefault(i => i != null && i.id == id);
            Assert.IsNotNull(it, "brak przedmiotu " + id);
            return it;
        }
    }
}
