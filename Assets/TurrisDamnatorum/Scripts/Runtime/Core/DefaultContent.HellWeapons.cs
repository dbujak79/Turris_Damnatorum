using UnityEngine;

namespace Turris
{
    public static partial class DefaultContent
    {
        /// <summary>Rodzina szablonu Turris i broń HU, która jest jej wzorcem tempa.</summary>
        enum WeaponFamily { Sword, Flail, Mace, GreatBlade, GreatBlunt, Dagger, Bow, Wand, Staff }

        static string TemplateId(WeaponFamily f)
        {
            switch (f)
            {
                case WeaponFamily.GreatBlade: case WeaponFamily.GreatBlunt: return "weapon_greataxe";
                case WeaponFamily.Dagger: return "weapon_dagger";
                case WeaponFamily.Bow: return "weapon_bow";
                case WeaponFamily.Staff: return "weapon_staff";
                default: return "weapon_longsword";
            }
        }

        static string ReferenceRow(WeaponFamily f)
        {
            switch (f)
            {
                case WeaponFamily.GreatBlade: case WeaponFamily.GreatBlunt: return "weapon.greataxe";
                case WeaponFamily.Dagger: return "weapon.dagger";
                case WeaponFamily.Bow: return "weapon.bow";
                case WeaponFamily.Wand: return "weapon.wand";
                case WeaponFamily.Staff: return "weapon.staff";
                default: return "weapon.sword";
            }
        }

        /// <summary>
        /// Bronie z Hell Unleashed: wszystkie przeliczone jedną zasadą DPS (<see cref="HellWeaponCatalog"/>) – także te dodane
        /// wcześniej z ręcznymi liczbami (audyt: broń dwuręczna ×1,45 miecza zamiast ×1,25, różdżka ziemi w tempie ognia).
        /// Nowe warianty i unikaty powstają z szablonów Turris; modele przypisuje edytor (model bazy, jak w HU).
        /// </summary>
        static void AddHellWeapons(GameConfig cfg)
        {
            var swordTemplate = bundle.Get<ItemDefinition>("weapon_longsword").weapon;
            void Rule(string id, WeaponFamily family)
            {
                var item = bundle.Get<ItemDefinition>(id);
                HellWeaponCatalog.Apply(item, HellWeaponCatalog.ForTarget(id), HellWeaponCatalog.Find(ReferenceRow(family)), swordTemplate);
            }
            foreach (var id in new[] { "hu_sword", "hu_sword_broad", "hu_sword_fire", "hu_sword_water", "hu_sword_air", "hu_sword_earth" }) Rule(id, WeaponFamily.Sword);
            Rule("hu_flail", WeaponFamily.Flail);
            foreach (var id in new[] { "hu_greatsword", "hu_greataxe" }) Rule(id, WeaponFamily.GreatBlade);
            Rule("hu_warhammer", WeaponFamily.GreatBlunt);
            foreach (var id in new[] { "hu_dagger", "hu_poniard" }) Rule(id, WeaponFamily.Dagger);
            Rule("hu_bow", WeaponFamily.Bow);
            foreach (var id in new[] { "hu_wand_fire", "hu_wand_water", "hu_wand_air", "hu_wand_earth" }) Rule(id, WeaponFamily.Wand);

            // Warianty P1 (wcześniej z tempem HU wklejonym w szablon) i bronie etapu 4.
            New("weapon.flail2h", "hu_flail2h", WeaponFamily.GreatBlunt, WeaponModel.GreatAxe, cfg);
            New("weapon.flail2h.iron", "hu_flail2h_iron", WeaponFamily.GreatBlunt, WeaponModel.GreatAxe, cfg);
            New("weapon.morningstar2h", "hu_morningstar2h", WeaponFamily.GreatBlunt, WeaponModel.Hammer, cfg);
            New("weapon.morningstar2h.runic", "hu_morningstar2h_runic", WeaponFamily.GreatBlunt, WeaponModel.Hammer, cfg);
            New("weapon.staff", "hu_staff", WeaponFamily.Staff, WeaponModel.Staff, cfg);
            New("weapon.staff", "hu_staff_summoner", WeaponFamily.Staff, WeaponModel.Staff, cfg, "Kostur przywoływacza");
            New("weapon.staff", "hu_staff_witch", WeaponFamily.Staff, WeaponModel.Staff, cfg, "Kostur wiedźmy");
            New("weapon.greataxe.bearded", "hu_greataxe_bearded", WeaponFamily.GreatBlade, WeaponModel.GreatAxe, cfg);
            New("weapon.greataxe.bardiche", "hu_greataxe_bardiche", WeaponFamily.GreatBlade, WeaponModel.GreatAxe, cfg);
            New("weapon.warhammer.runic", "hu_warhammer_runic", WeaponFamily.GreatBlunt, WeaponModel.Hammer, cfg);
            New("weapon.warhammer.siege", "hu_warhammer_siege", WeaponFamily.GreatBlunt, WeaponModel.Hammer, cfg);

            New("weapon.sword.sabre", "hu_sabre", WeaponFamily.Sword, WeaponModel.Sword, cfg);
            New("weapon.sword.broadsword", "hu_broadsword", WeaponFamily.Sword, WeaponModel.Sword, cfg);
            New("weapon.flail.spiked", "hu_flail_spiked", WeaponFamily.Flail, WeaponModel.Mace, cfg);
            New("weapon.flail.knight", "hu_flail_knight", WeaponFamily.Flail, WeaponModel.Mace, cfg);
            New("weapon.morningstar", "hu_morningstar", WeaponFamily.Mace, WeaponModel.Mace, cfg);
            New("weapon.morningstar.heavy", "hu_morningstar_heavy", WeaponFamily.Mace, WeaponModel.Mace, cfg);
            New("weapon.greatsword.executioner", "hu_greatsword_executioner", WeaponFamily.GreatBlade, WeaponModel.GreatSword, cfg);
            New("weapon.greatsword.flamberge", "hu_greatsword_flamberge", WeaponFamily.GreatBlade, WeaponModel.GreatSword, cfg);
            New("weapon.dagger.kris", "hu_dagger_kris", WeaponFamily.Dagger, WeaponModel.Dagger, cfg);
            New("weapon.dagger.misericorde", "hu_dagger_misericorde", WeaponFamily.Dagger, WeaponModel.Dagger, cfg);
            New("weapon.bow.recurve", "hu_bow_recurve", WeaponFamily.Bow, WeaponModel.Bow, cfg);
            New("weapon.bow.composite", "hu_bow_composite", WeaponFamily.Bow, WeaponModel.Bow, cfg);

            // Unikaty: moc specjalna i premie z HU; rzadziej w nagrodach (w HU wypadały tylko z łupów).
            New("weapon.unique.szczerbiec", "hu_unique_szczerbiec", WeaponFamily.Sword, WeaponModel.Sword, cfg);
            New("weapon.unique.perun_blade", "hu_unique_perun_blade", WeaponFamily.Sword, WeaponModel.Sword, cfg);
            New("weapon.unique.last_word", "hu_unique_last_word", WeaponFamily.GreatBlade, WeaponModel.GreatSword, cfg);
            New("weapon.unique.penance", "hu_unique_penance", WeaponFamily.Flail, WeaponModel.Mace, cfg);
            New("weapon.unique.svarog_hammer", "hu_unique_svarog_hammer", WeaponFamily.GreatBlunt, WeaponModel.Hammer, cfg);
            New("weapon.unique.widows_lament", "hu_unique_widows_lament", WeaponFamily.Dagger, WeaponModel.Dagger, cfg);
            New("weapon.unique.dziewanna_bow", "hu_unique_dziewanna_bow", WeaponFamily.Bow, WeaponModel.Bow, cfg);
        }

        static void New(string huId, string id, WeaponFamily family, WeaponModel model, GameConfig cfg, string title = null)
        {
            var row = HellWeaponCatalog.Find(huId);
            var template = bundle.Get<ItemDefinition>(TemplateId(family));
            var item = Make<ItemDefinition>(id, title ?? row.displayName, "");
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(template), item);
            item.id = item.name = id;
            item.displayName = title ?? row.displayName;
            item.description = string.IsNullOrEmpty(row.description) ? "Broń z Hell Unleashed." : row.description;
            item.weaponModel = model;
            item.model = null;
            item.modifiers.Clear(); item.effects.Clear();
            item.grantedSkill = null;
            item.tags = TagsFor(family);
            var w = item.weapon;
            switch (family)
            {
                case WeaponFamily.Flail:
                    // Kula na łańcuchu (jak Korbacz): szeroki zamach, mocniej łamie postawę, nie paruje.
                    w.canParry = false;
                    foreach (var a in new[] { w.light, w.heavy }) { a.radius *= 1.2f; a.poiseDamage *= 1.3f; }
                    break;
                case WeaponFamily.Mace:
                    foreach (var a in new[] { w.light, w.heavy }) a.poiseDamage *= 1.2f;
                    break;
                case WeaponFamily.GreatBlunt:
                    foreach (var a in new[] { w.light, w.heavy }) { a.poiseDamage *= 1.4f; if (model == WeaponModel.Hammer) a.bonusVsFrozen = 0.5f; }
                    break;
            }
            item.color = HuWeaponColor(row);
            HellWeaponCatalog.Apply(item, row, HellWeaponCatalog.Find(ReferenceRow(family)), bundle.Get<ItemDefinition>("weapon_longsword").weapon);
            if (row.Rarity >= 4 || row.Power > 0) HellWeaponCatalog.ApplyUnique(item, row);
            cfg.baseItemPool.Add(item);
        }

        static BuildTag TagsFor(WeaponFamily f)
        {
            switch (f)
            {
                case WeaponFamily.Sword: return BuildTag.Melee | BuildTag.Guard | BuildTag.Parry;
                case WeaponFamily.Flail: case WeaponFamily.Mace: return BuildTag.Melee | BuildTag.Heavy;
                case WeaponFamily.GreatBlade: case WeaponFamily.GreatBlunt: return BuildTag.Melee | BuildTag.Heavy | BuildTag.Guard;
                case WeaponFamily.Dagger: return BuildTag.Melee | BuildTag.Agile | BuildTag.Parry;
                case WeaponFamily.Bow: return BuildTag.Agile;
                default: return BuildTag.Magic;
            }
        }

        static Color HuWeaponColor(HellWeaponCatalog.Row row)
        {
            if (row.DamageFire > 0) return new Color(1f, 0.5f, 0.15f);
            if (row.DamageAir > 0) return new Color(0.78f, 0.82f, 1f);
            if (row.DamageWater > 0) return new Color(0.55f, 0.85f, 1f);
            if (row.DamageEarth > 0) return new Color(0.78f, 0.6f, 0.35f);
            if (System.Array.IndexOf(row.Tags, "bow") >= 0) return new Color(0.5f, 0.35f, 0.22f);
            return Steel;
        }
    }
}
