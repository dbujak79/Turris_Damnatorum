using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>Wpis bestiariusza: wróg bazowy z wariantami żywiołów (jedna karta, jak chochlik w Hell Unleashed).</summary>
    public class BestiaryEntry
    {
        public EnemyDefinition def;
        public readonly List<EnemyDefinition> variants = new List<EnemyDefinition>();
        /// <summary>Demon z Hell Unleashed (model HU) – zakładka „Demony”; pozostali – „Wrogowie wieży”.</summary>
        public bool demon;
        /// <summary>Pierwsze piętro, na którym się pojawia (1..), 0 – tylko przyzywany.</summary>
        public int floor;
    }

    /// <summary>
    /// Bestiariusz: lista wrogów zebrana z wieży (piętra, fale, portale, warianty, przyzywane sługi) i ich mocne oraz słabe
    /// strony wyliczone z danych – odporności, pancerz, postawa, ataki specjalne, fazy bossa. Słabości odkrywa się, zabijając.
    /// </summary>
    public static class Bestiary
    {
        /// <summary>Po tylu zabitych bestiariusz pokazuje słabe strony (boss – po pierwszym).</summary>
        public const int RevealKills = 20;

        static GameConfig cachedFor;
        static List<BestiaryEntry> cached;
        static Dictionary<EnemyDefinition, EnemyDefinition> baseOf;

        public static List<BestiaryEntry> Entries(GameConfig cfg)
        {
            if (cfg == cachedFor && cached != null) return cached;
            cachedFor = cfg;
            cached = new List<BestiaryEntry>();
            baseOf = new Dictionary<EnemyDefinition, EnemyDefinition>();
            var byDef = new Dictionary<EnemyDefinition, BestiaryEntry>();
            void Add(EnemyDefinition d, int floor)
            {
                if (d == null) return;
                if (baseOf.TryGetValue(d, out var b)) d = b;
                if (byDef.TryGetValue(d, out var existing)) { if (existing.floor == 0 && floor > 0) existing.floor = floor; return; }
                var e = new BestiaryEntry { def = d, floor = floor, demon = d.visual != null && d.visual.demon };
                byDef[d] = e;
                baseOf[d] = d;
                cached.Add(e);
                foreach (var v in d.variants) if (v != null && !baseOf.ContainsKey(v)) { baseOf[v] = d; e.variants.Add(v); }
                foreach (var a in d.attacks) if (a.summon != null) Add(a.summon, 0);
                if (d.huSummon != null) Add(d.huSummon, 0);
            }
            if (cfg != null && cfg.tower != null)
                for (int i = 0; i < cfg.tower.floors.Count; i++)
                {
                    var f = cfg.tower.floors[i];
                    foreach (var d in f.enemies) Add(d, i + 1);
                    foreach (var w in f.extraWaves) foreach (var d in w.enemies) Add(d, i + 1);
                    if (f.portals != null) foreach (var c in f.portals.composition) Add(c.enemy, i + 1);
                }
            if (cfg != null) foreach (var d in cfg.enemyCatalog) Add(d, 0);
            return cached;
        }

        /// <summary>Id wpisu bestiariusza: wariant żywiołu liczy się do swojego wroga bazowego.</summary>
        public static string BaseId(GameConfig cfg, EnemyDefinition def)
        {
            if (def == null) return null;
            Entries(cfg);
            return baseOf != null && baseOf.TryGetValue(def, out var b) ? b.id : def.id;
        }

        public static int RevealAt(EnemyDefinition def) => def.isBoss ? 1 : RevealKills;

        // ------------------------------------------------------------------ Opis

        /// <summary>„Szybki (wręcz) · Zdrowie 55 · Atak: fizyczne + ogień”.</summary>
        public static string Summary(EnemyDefinition d)
        {
            var main = d.attacks.FirstOrDefault(a => a.weight > 0) ?? d.attacks.FirstOrDefault();
            string atk = "";
            if (main != null)
            {
                var a = main.attack;
                atk = a.delivery == AttackDelivery.Projectile ? "pociski" : Names.Damage(a.damageType);
                if (a.element != Element.None) atk += " + " + Names.Element(a.element).ToLowerInvariant();
            }
            return $"{d.archetype} · Zdrowie {d.maxHealth:0} · Atak: {atk}";
        }

        static void Elem(List<string> strong, List<string> weak, string name, float mult)
        {
            int pct = Mathf.RoundToInt((1f - mult) * 100f);
            if (pct >= 10) strong.Add($"Odporny: {name} +{pct}%");
            else if (pct <= -10) weak.Add($"Podatny: {name} {pct}%");
        }

        /// <summary>Mocne strony – zawsze widoczne (to widać w walce).</summary>
        public static List<string> Strengths(EnemyDefinition d)
        {
            var strong = new List<string>(); var weak = new List<string>();
            Resistances(d, strong, weak);
            if (d.physicalResist >= 0.1f) strong.Add($"Pancerz: −{d.physicalResist * 100f:0}% obrażeń fizycznych");
            if (d.magicResist >= 0.1f) strong.Add($"Odporność na magię: −{d.magicResist * 100f:0}%");
            if (d.guardChance > 0f) strong.Add("Zasłania się tarczą przed ciosami z przodu");
            if (d.evadeChance >= 0.2f) strong.Add("Często odskakuje przed ciosem");
            if (d.maxPoise >= 80f) strong.Add("Trudno go zachwiać – ciężkie ataki z pancerzem");
            foreach (var e in d.attacks)
            {
                string line = Special(e);
                if (line == null)
                {
                    var a = e.attack;
                    if (!a.blockable) line = $"{a.name}: nie do zablokowania";
                    else if (!a.parryable && a.heavy) line = $"{a.name}: ciężki, nie do sparowania";
                }
                if (line != null) strong.Add(e.eliteOnly ? line + " (elita)" : line);
            }
            if (d.phases.Count > 0)
                foreach (var p in d.phases) strong.Add($"Przy {p.healthThreshold * 100f:0}% zdrowia: {p.announcement.TrimEnd('!', '.').ToLowerInvariant()}");
            if (d.packSize > 1) strong.Add($"Wychodzi stadem po {d.packSize}");
            return strong.Distinct().ToList();
        }

        /// <summary>Słabe strony – odkrywane po <see cref="RevealAt"/> zabitych.</summary>
        public static List<string> Weaknesses(EnemyDefinition d)
        {
            var strong = new List<string>(); var weak = new List<string>();
            Resistances(d, strong, weak);
            if (d.maxHealth <= 80f) weak.Add("Mało zdrowia: jeden szeroki cios rozrzuca całą grupę");
            if (d.maxPoise <= 20f) weak.Add("Łatwo traci postawę – każdy cios go zachwiewa");
            if (d.retreatRange > 0f) weak.Add("Słaby w zwarciu – dopadnij go");
            if (d.guardChance > 0f) weak.Add("Bok i plecy bez osłony tarczy");
            if (d.attacks.Any(a => a.attack.parryable && a.weight > 0 && a.attack.delivery == AttackDelivery.Melee))
                weak.Add("Zwykłe ciosy da się sparować – riposta");
            if (d.attacks.Any(a => a.special == EnemySpecial.Latch)) weak.Add("Unik strząsa uczepionego");
            if (d.attacks.Any(a => a.special == EnemySpecial.Summon)) weak.Add("Sługi giną razem z nim");
            if (d.isMinion) weak.Add("Ginie razem z przyzywającym");
            if (d.attacks.Any(a => a.hyperArmor && a.attack.recovery >= 1f)) weak.Add("Długi odpoczynek po ciężkich atakach – okno na kontrę");
            return weak.Distinct().ToList();
        }

        static void Resistances(EnemyDefinition d, List<string> strong, List<string> weak)
        {
            Elem(strong, weak, "ogień", d.fireMultiplier);
            Elem(strong, weak, "mróz", d.frostMultiplier);
            Elem(strong, weak, "błyskawice", d.lightningMultiplier);
            Elem(strong, weak, "ziemia", d.earthMultiplier);
            if (d.bleedMultiplier <= 0.6f) strong.Add("Słabo krwawi");
            else if (d.bleedMultiplier >= 1.3f) weak.Add("Mocno krwawi");
        }

        static string Special(EnemyAttackEntry e)
        {
            switch (e.special)
            {
                case EnemySpecial.Latch: return $"{e.attack.name}: skacze i wgryza się – spowalnia i rani co sekundę";
                case EnemySpecial.Charge: return $"{e.attack.name}: szarża po zaznaczonej linii, zostawia płonący ślad";
                case EnemySpecial.FireZones: return $"{e.attack.name}: zapowiedziane kręgi ognia wokół bohatera";
                case EnemySpecial.Orbs: return $"{e.attack.name}: wolne kule, po których płonie ziemia";
                case EnemySpecial.Summon: return $"{e.attack.name}: przyzywa sługi ({(e.summon != null ? e.summon.displayName : "?")})";
                case EnemySpecial.Leap: return $"{e.attack.name}: skok na miejsce bohatera i uderzenie w krąg";
                default: return null;
            }
        }
    }
}
