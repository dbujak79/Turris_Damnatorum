using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Rodzaje broni i przypisanie skilli do broni (wzór: Path of Exile 2). Skill nie należy do klasy – wymaga rodzaju broni
    /// (ostrze, ciężka broń, sztylet, łuk, fokus), a broń ma wymagania atrybutów. Każda klasa użyje więc każdego skilla,
    /// jeśli trzyma pasującą broń (w ręce głównej albo drugiej: grymuar, sztylet do rzucania).
    /// Obrażenia skilla zależą od siły broni: technika – DPS broni względem miecza wzorcowego, czar – DPS fokusu względem
    /// różdżki ognia. Ten sam skill na sztylecie jest słabszy niż na mieczu.
    /// </summary>
    public static class WeaponFamilies
    {
        public const BuildTag All = BuildTag.Blade | BuildTag.HeavyWeapon | BuildTag.Dagger | BuildTag.Focus | BuildTag.Ranged;

        /// <summary>Bronie Turris spoza katalogu HU (rodzaj z kształtu broni).</summary>
        static readonly Dictionary<string, BuildTag> LegacyWeapons = new Dictionary<string, BuildTag>
        {
            { "weapon_longsword", BuildTag.Blade }, { "weapon_serratedsword", BuildTag.Blade }, { "weapon_flamesword", BuildTag.Blade },
            { "weapon_dagger", BuildTag.Blade | BuildTag.Dagger }, { "weapon_knives", BuildTag.Blade | BuildTag.Dagger },
            { "weapon_axe", BuildTag.HeavyWeapon }, { "weapon_frostaxe", BuildTag.HeavyWeapon }, { "weapon_greataxe", BuildTag.HeavyWeapon },
            { "weapon_mace", BuildTag.HeavyWeapon }, { "weapon_stormhammer", BuildTag.HeavyWeapon }, { "weapon_spear", BuildTag.HeavyWeapon },
            { "weapon_scythe", BuildTag.Blade | BuildTag.HeavyWeapon },
            { "weapon_staff", BuildTag.Focus }, { "weapon_staff_fire", BuildTag.Focus }, { "weapon_staff_frost", BuildTag.Focus }, { "weapon_staff_storm", BuildTag.Focus },
            { "weapon_bow", BuildTag.Ranged },
        };

        /// <summary>Bronie pośrednie należące do kilku rodzajów naraz (ponad to, co wynika z tagów HU).</summary>
        static readonly Dictionary<string, BuildTag> Extra = new Dictionary<string, BuildTag>
        {
            { "hu_sabre", BuildTag.Dagger }, // szabla (Siła + Zręczność): ostrze i sztylet
        };

        /// <summary>
        /// Wymagany rodzaj broni każdego skilla (id assetu) – jawnie dla każdego skilla poza skillami klas (test pilnuje kompletności).
        /// Nowy skill bez wpisu dostaje domyślnie: czar – fokus, technika – dowolna broń (patrz <see cref="ForSkill"/>).
        /// </summary>
        static readonly Dictionary<string, BuildTag> Skills = new Dictionary<string, BuildTag>
        {
            // Ostrza: miecze, szable, sztylety.
            { "hu_wind_slash", BuildTag.Blade }, { "hu_cross_cut", BuildTag.Blade }, { "hu_breaker_wave", BuildTag.Blade },
            { "skill_flurry", BuildTag.Blade }, { "skill_rend", BuildTag.Blade }, { "skill_rupture", BuildTag.Blade },
            { "skill_lunge", BuildTag.Blade }, { "skill_execute", BuildTag.Blade },
            // Ostrze lub ciężka broń (wystarczy jedno): szerokie cięcia i ciosy z zamachu.
            { "hu_cleave", BuildTag.Blade | BuildTag.HeavyWeapon }, { "skill_cleave", BuildTag.Blade | BuildTag.HeavyWeapon },
            { "hu_whirlwind", BuildTag.Blade | BuildTag.HeavyWeapon }, { "skill_whirlwind", BuildTag.Blade | BuildTag.HeavyWeapon },
            { "hu_executioners_fall", BuildTag.Blade | BuildTag.HeavyWeapon }, { "skill_leapslash", BuildTag.Blade | BuildTag.HeavyWeapon },
            // Ciężka broń: topory, młoty, buławy, korbacze, cepy.
            { "hu_warrior_quake", BuildTag.HeavyWeapon }, { "skill_quake", BuildTag.HeavyWeapon },
            { "skill_thunder", BuildTag.HeavyWeapon }, { "hu_axe_throw", BuildTag.HeavyWeapon }, { "skill_throw", BuildTag.HeavyWeapon },
            { "hu_slag_spray", BuildTag.HeavyWeapon },
            // Sztylety (też sztylet do rzucania w drugiej ręce).
            { "hu_knife_throw", BuildTag.Dagger }, { "skill_knives", BuildTag.Dagger }, { "skill_bladefan", BuildTag.Dagger },
            { "hu_thorn_scatter", BuildTag.Dagger }, { "hu_sulfur_bomb", BuildTag.Dagger }, { "hu_snare", BuildTag.Dagger },
            { "hu_shadow_step", BuildTag.Dagger },
            // Łuki.
            { "hu_piercing_shot", BuildTag.Ranged }, { "hu_splinter_arrow", BuildTag.Ranged }, { "hu_sulfur_arrow", BuildTag.Ranged },
            { "hu_volley", BuildTag.Ranged }, { "hu_arrow_rain", BuildTag.Ranged }, { "skill_volley", BuildTag.Ranged },
            { "skill_firearrow", BuildTag.Ranged }, { "skill_frostarrow", BuildTag.Ranged },
            // Fokus (różdżka, kostur, grymuar): wszystkie czary i techniki o naturze czaru.
            { "hu_storm_verdict", BuildTag.Focus },
            { "hu_brimstone_rain", BuildTag.Focus }, { "hu_cyclone_eye", BuildTag.Focus }, { "hu_damned_chain", BuildTag.Focus },
            { "hu_ember_ring", BuildTag.Focus }, { "hu_frost_shard", BuildTag.Focus }, { "hu_glacial_tear", BuildTag.Focus },
            { "hu_infernal_orb", BuildTag.Focus }, { "hu_nova_air", BuildTag.Focus }, { "hu_nova_earth", BuildTag.Focus },
            { "hu_nova_fire", BuildTag.Focus }, { "hu_nova_water", BuildTag.Focus }, { "hu_stone_spikes", BuildTag.Focus },
            { "hu_stone_ward", BuildTag.Focus }, { "hu_summon_golem", BuildTag.Focus }, { "hu_summon_hounds", BuildTag.Focus },
            { "hu_summon_imps", BuildTag.Focus }, { "hu_tidal_wave", BuildTag.Focus }, { "hu_wind_step", BuildTag.Focus },
            { "spell_barrier", BuildTag.Focus }, { "spell_bloodpact", BuildTag.Focus }, { "spell_bolt", BuildTag.Focus },
            { "spell_burningground", BuildTag.Focus }, { "spell_chain", BuildTag.Focus }, { "spell_chains", BuildTag.Focus },
            { "spell_enchant", BuildTag.Focus }, { "spell_fireball", BuildTag.Focus }, { "spell_firering", BuildTag.Focus },
            { "spell_flameblade", BuildTag.Focus }, { "spell_frostarmor", BuildTag.Focus }, { "spell_frostcone", BuildTag.Focus },
            { "spell_frostnova", BuildTag.Focus }, { "spell_heal", BuildTag.Focus }, { "spell_icelance", BuildTag.Focus },
            { "spell_meteor", BuildTag.Focus }, { "spell_nova", BuildTag.Focus }, { "spell_scatter", BuildTag.Focus },
            { "spell_spark", BuildTag.Focus }, { "spell_spear", BuildTag.Focus }, { "spell_storm", BuildTag.Focus },
            // Dowolna broń: szarże, odtrącenie, tupnięcie, okrzyki i aury.
            { "hu_infernal_charge", BuildTag.None }, { "hu_repel", BuildTag.None }, { "skill_charge", BuildTag.None },
            { "skill_stomp", BuildTag.None }, { "skill_warcry", BuildTag.None }, { "hu_aura_defiance", BuildTag.None },
            { "hu_aura_holy_fire", BuildTag.None }, { "hu_aura_might", BuildTag.None }, { "hu_aura_purity", BuildTag.None },
            { "hu_aura_renewal", BuildTag.None }, { "hu_aura_zeal", BuildTag.None },
            // Tarcza i parowanie (bez zmian).
            { "skill_shieldbash", BuildTag.Shield }, { "skill_counter", BuildTag.Parry },
        };

        /// <summary>Skille klas pod osobnym przyciskiem (tożsamość klasy) – bez wymagań broni.</summary>
        static readonly HashSet<string> Signatures = new HashSet<string>
        {
            "hu_warcry", "hu_overload", "hu_blade_fan", "hu_arrow_storm", "hu_blade_dance", "hu_death_mark", "hu_elemental_arrow",
            "hu_judgement", "hu_legion", "hu_maelstrom", "hu_meteor", "hu_quake", "hu_storm_cleave", "hu_tempest", "hu_undying",
        };

        /// <summary>Czy skill ma jawny wpis w tabeli albo jest skillem klasy (jedynym wymogiem jest klasa).</summary>
        public static bool IsAssigned(SpellDefinition s) => s != null && (Signatures.Contains(s.id) || Skills.ContainsKey(s.id));

        /// <summary>Wymagany rodzaj broni skilla (źródło prawdy dla <c>SpellDefinition.requiredTags</c>).</summary>
        public static BuildTag ForSkill(SpellDefinition s)
        {
            if (s == null || Signatures.Contains(s.id)) return BuildTag.None;
            if (Skills.TryGetValue(s.id, out var t)) return t;
            return s.IsSpell ? BuildTag.Focus : BuildTag.None;
        }

        /// <summary>
        /// Czy tagi buildu spełniają wymaganie skilla: rodzaje broni działają jak „którykolwiek” (Rozpłatanie: ostrze lub
        /// ciężka broń – jak „Requires: Axe or Sword” w PoE2), pozostałe tagi (tarcza, parowanie) – wszystkie.
        /// </summary>
        public static bool Meets(BuildTag required, BuildTag have)
        {
            var families = required & All;
            var rest = required & ~All;
            return (rest & have) == rest && (families == BuildTag.None || (families & have) != BuildTag.None);
        }

        /// <summary>Czy broń (ręka główna + druga) spełnia wymaganie rodzaju broni i tarczy skilla.</summary>
        public static bool Usable(SpellDefinition s, ItemDefinition main, ItemDefinition off)
        {
            var have = Of(main) | Of(off) | (off != null && off.IsShield ? BuildTag.Shield : BuildTag.None);
            return Meets(s.requiredTags & (All | BuildTag.Shield), have);
        }

        static readonly Dictionary<ItemDefinition, BuildTag> cache = new Dictionary<ItemDefinition, BuildTag>();

        /// <summary>Rodzaje broni przedmiotu (broń, grymuar, sztylet do rzucania); tarcza i pancerz – brak.</summary>
        public static BuildTag Of(ItemDefinition def)
        {
            if (def == null) return BuildTag.None;
            if (cache.TryGetValue(def, out var t)) return t;
            if (!LegacyWeapons.TryGetValue(def.id, out t))
            {
                var row = HuRow(def);
                t = row != null ? FromHuTags(row.Tags) : BuildTag.None;
            }
            if (Extra.TryGetValue(def.id, out var extra)) t |= extra;
            cache[def] = t;
            return t;
        }

        static HellWeaponCatalog.Row HuRow(ItemDefinition def) =>
            !string.IsNullOrEmpty(def.huSourceId) ? HellWeaponCatalog.Find(def.huSourceId) : HellWeaponCatalog.ForTarget(def.id);

        static BuildTag FromHuTags(string[] tags)
        {
            var t = BuildTag.None;
            foreach (var tag in tags)
                switch (tag)
                {
                    case "sword": case "greatsword": t |= BuildTag.Blade; break;
                    case "dagger": t |= BuildTag.Blade | BuildTag.Dagger; break;
                    case "axe": case "blunt": case "hammer": case "flail": case "morningstar": t |= BuildTag.HeavyWeapon; break;
                    case "bow": t |= BuildTag.Ranged; break;
                    case "wand": case "staff": case "grimoire": t |= BuildTag.Focus; break;
                }
            return t;
        }

        // ------------------------------------------------------------------ Siła broni dla skilli

        /// <summary>
        /// Mnożnik obrażeń skilla od broni, którą jest użyty: ręka z wymaganym rodzajem (najpierw główna), w przeciwnym razie
        /// broń w ręce głównej. Wzorzec zależy od rodzaju, przez który broń spełnia wymaganie skilla: ostrze i ciężka broń –
        /// miecz jednoręczny, sztylet – sztylet, łuk – łuk myśliwski, czar – podstawowa różdżka żywiołu fokusu. Skill ostrza
        /// na sztylecie bije więc słabiej (×0,7), a lepsza broń danego rodzaju – mocniej. Skill bez wymagań (szarża, skill klasy)
        /// liczy wzorzec z rodzaju trzymanej broni. Poziom ulepszenia broni (+10% za poziom) też się liczy. Grymuar i sztylet
        /// do rzucania nie mają własnego DPS – czar ×1, technika bierze broń z ręki głównej.
        /// </summary>
        public static float SkillScale(SpellDefinition def, BuildTag required, ItemInstance main, ItemInstance off, GameConfig cfg)
        {
            bool spell = def.IsSpell;
            var wanted = required & All;
            ItemInstance hand = main;
            if (wanted != BuildTag.None && (main == null || (Of(main.definition) & wanted) == BuildTag.None) && off != null && (Of(off.definition) & wanted) != BuildTag.None)
                hand = off;
            float? p = Power(hand, wanted, spell, cfg);
            if (!p.HasValue && hand != main) p = spell ? (float?)null : Power(main, wanted, false, cfg);
            return p ?? 1f;
        }

        /// <summary>Wzorzec (id broni HU) dla rodzaju, przez który broń spełnia wymaganie skilla.</summary>
        static string ReferenceFor(BuildTag via, bool spell, HellWeaponCatalog.Row row)
        {
            if (spell || (via & BuildTag.Focus) != 0)
            {
                var tags = row.Tags;
                return tags.Contains("water") ? "weapon.wand.water" : tags.Contains("air") ? "weapon.wand.air"
                    : tags.Contains("earth") ? "weapon.wand.earth" : "weapon.wand";
            }
            if ((via & BuildTag.Dagger) != 0) return "weapon.dagger";
            if ((via & BuildTag.Ranged) != 0) return "weapon.bow";
            return HellWeaponCatalog.ReferenceWeapon;
        }

        /// <summary>Siła broni względem wzorca; null – przedmiot bez własnych obrażeń (grymuar, tarcza, sztylet do rzucania).</summary>
        static float? Power(ItemInstance it, BuildTag wanted, bool spell, GameConfig cfg)
        {
            if (it == null || it.definition == null || it.definition.weapon == null || it.definition.weapon.light == null) return null;
            var own = Of(it.definition);
            var via = wanted & own;
            if (via == BuildTag.None) via = own;
            var row = HuRow(it.definition);
            float ratio;
            if (row != null)
            {
                if (row.HuDps <= 0f) return null;
                var reference = HellWeaponCatalog.Find(ReferenceFor(via, spell, row));
                ratio = reference != null && reference.HuDps > 0f ? row.HuDps / reference.HuDps : 1f;
            }
            else
            {
                // Broń Turris: DPS lekkich ciosów względem miecza długiego (szablon miecza wzorcowego, lekki cios 40).
                var sword = LegacyReference(cfg);
                float refDps = sword != null ? HellWeaponCatalog.TurrisDps(sword) : 0f;
                ratio = refDps > 0f ? HellWeaponCatalog.TurrisDps(it.definition.weapon) / refDps : 1f;
                if (spell || (via & (BuildTag.Focus | BuildTag.Dagger | BuildTag.Ranged)) != 0) ratio = 1f; // fokus, sztylet, łuk Turris – wzorcowe
            }
            return ratio * it.WeaponLevelFactor;
        }

        static WeaponData legacyReference;

        static WeaponData LegacyReference(GameConfig cfg)
        {
            if (legacyReference != null || cfg == null) return legacyReference;
            var sword = cfg.baseItemPool.FirstOrDefault(i => i != null && i.id == "weapon_longsword");
            legacyReference = sword != null ? sword.weapon : null;
            return legacyReference;
        }
    }
}
