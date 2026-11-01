using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>Rzadkość egzemplarza przedmiotu (Hell Unleashed): liczba affixów i mnożnik ceny.</summary>
    public enum ItemRarity { Common, Magic, Rare, Legendary, Unique }

    /// <summary>Źródło przedmiotu – od niego zależą szanse rzadkości i jakość.</summary>
    public enum LootSource { Normal, Elite, Boss, Shop }

    /// <summary>
    /// Ekonomia bohatera z Hell Unleashed (etap 6): katalogi przedmiotów, affixów i perków (z <c>tools/art/import_hu_economy.py</c>),
    /// przeliczenie statystyk HU na Turris i losowanie przedmiotów z rzadkością, jakością i affixami. Piętro wieży = fala HU.
    /// Ceny HU × <see cref="PriceScale"/> (dusze Turris są ok. 2,5× „większe” – jak przy pieczęciach) × inflacja sklepu.
    /// </summary>
    public static class HellEconomy
    {
        public const float PriceScale = 2.5f;

        [Serializable] class ItemCatalog { public ItemRow[] items; }
        [Serializable] class AffixCatalog { public AffixRow[] affixes; }
        [Serializable] class UpgradeCatalog { public UpgradeRow[] upgrades; }

        [Serializable] public class ItemRow { public string id, file, name, desc, ClassId, KindName; public int Kind, Rarity, MinWave = 1, Power; public string[] Modifiers = new string[0], Tags = new string[0]; public float BasePrice = 40, Weight = 1, Load, PowerValue; public bool InShop = true; }
        /// <summary>
        /// Rekord affiksu. <c>Filter</c> (nakładka Turris) – wąski warunek kwalifikacji przedmiotu (<see cref="PassesFilter"/>),
        /// niezależny od <c>RequiredTags</c> (te działają jak dawniej: którykolwiek). <c>Decimals</c> ≥ 0 – precyzja wylosowanej
        /// wartości (jednostki HU); −1 – dawna reguła zaokrąglania.
        /// </summary>
        [Serializable] public class AffixRow { public string id, name, Group, Filter = ""; public int Kind, Tier = 1, MinWave = 1, Decimals = -1; public float Weight = 1; public string[] Categories = new string[0], RequiredTags = new string[0], Ranges = new string[0]; }
        [Serializable] public class UpgradeRow { public string id, name, desc, ClassId, Conversion; public int Scope, MaxLevel = 5, RequiredSlot; public float BasePrice = 20, PriceGrowth = 1.5f, Weight = 1; public string[] Modifiers = new string[0], RequiredTags = new string[0], RequiresAny = new string[0], OnHitEffects = new string[0]; }

        static ItemRow[] items; static AffixRow[] affixes; static UpgradeRow[] upgrades;
        static T Load<T>(string name) => JsonUtility.FromJson<T>(Resources.Load<TextAsset>("HellCatalog/" + name).text);
        public static IReadOnlyList<ItemRow> Items => items ?? (items = Load<ItemCatalog>("items").items);
        public static IReadOnlyList<AffixRow> Affixes => affixes ?? (affixes = Load<AffixCatalog>("affixes").affixes);
        public static IReadOnlyList<UpgradeRow> Upgrades => upgrades ?? (upgrades = Load<UpgradeCatalog>("upgrades").upgrades);

        // ------------------------------------------------------------------ Statystyki HU → Turris

        public static float Num(string s) => float.Parse(s, CultureInfo.InvariantCulture);

        /// <summary>
        /// Jedna wartość HU („Stat|Op|v”) jako modyfikator Turris; null – statystyka bez odpowiednika (penetracja, unik, blok bierny,
        /// pociski, zasięg wręcz, odrzut…). Przeliczniki jak przy unikatach broni: życie ×2, pancerz ×6, ułamki → %, dodane obrażenia
        /// płaskie → % obrażeń broni (5% za punkt, ok. tyle co na mieczu wzorcowym).
        /// </summary>
        public static StatModifier? Map(string stat, string op, float v)
        {
            bool pct = op.StartsWith("Percent");
            StatModifier P(StatType t, float x) => new StatModifier(t, x, ModifierMode.Percent);
            StatModifier F(StatType t, float x) => new StatModifier(t, x);
            switch (stat)
            {
                case "MaxHealth": return pct ? P(StatType.MaxHealth, v * 100f) : F(StatType.MaxHealth, v * 2f);
                case "MaxStamina": return pct ? P(StatType.MaxStamina, v * 100f) : F(StatType.MaxStamina, v);
                case "MaxMana": return pct ? P(StatType.MaxMana, v * 100f) : F(StatType.MaxMana, v);
                case "HealthRegen": return F(StatType.HealthRegen, v * 2f);
                case "ManaRegen": return F(StatType.ManaRegen, v);
                case "StaminaRegen": return pct ? P(StatType.StaminaRegen, v * 100f) : F(StatType.StaminaRegen, v);
                case "Armour": return F(StatType.PhysicalDefense, v * 6f);
                case "PhysicalResistance": return F(StatType.PhysicalDefense, v * 100f);
                case "DamageReduction": return F(StatType.DamageReduction, v * 100f);
                case "FireResistance": return F(StatType.FireResist, v * 100f);
                case "WaterResistance": return F(StatType.FrostResist, v * 100f);
                case "AirResistance": return F(StatType.LightningResist, v * 100f);
                case "EarthResistance": return F(StatType.EarthResist, v * 100f);
                case "PhysicalDamage": return F(StatType.PhysicalDamage, pct ? v * 100f : v * 5f);
                case "FireDamage": return pct ? F(StatType.FireDamage, v * 100f) : F(StatType.PhysicalDamage, v * 5f);
                case "WaterDamage": return pct ? F(StatType.FrostDamage, v * 100f) : F(StatType.PhysicalDamage, v * 5f);
                case "AirDamage": return pct ? F(StatType.LightningDamage, v * 100f) : F(StatType.PhysicalDamage, v * 5f);
                case "EarthDamage": return pct ? F(StatType.EarthDamage, v * 100f) : F(StatType.PhysicalDamage, v * 5f);
                case "GlobalDamage": return F(StatType.GlobalDamage, v * 100f);
                case "SpellDamage": return F(StatType.SpellPower, v * 100f);
                case "AttackSpeed": return F(StatType.AttackSpeed, v * 100f);
                case "MoveSpeed": return F(StatType.MoveSpeed, v * 100f);
                case "SkillCooldownReduction": return F(StatType.CooldownReduction, v * 100f);
                case "LifeLeech": return F(StatType.LifeLeech, v * 100f);
                case "ExecuteDamage": return F(StatType.ExecuteDamage, v * 100f);
                case "BleedDamage": return F(StatType.BleedDamage, v * 100f);
                case "EquipLoad": return F(StatType.EquipLoad, v);
                default: return null;
            }
        }

        /// <summary>
        /// Statystyka affiksu: jak <see cref="Map(string, string, float)"/>, ale płaskie obrażenia żywiołu to rzeczywisty dodany
        /// składnik żywiołu broni (5% bazy broni za punkt HU), a nie premia fizyczna, oraz statystyki affiksów specjalizacyjnych.
        /// Tylko losowe affiksy – przedmioty bazowe i perki HU zostają przy <see cref="Map(string, string, float)"/>.
        /// </summary>
        /// <summary>
        /// Procent bazy broni za punkt affiksu „dodany żywioł” (punkt HU). 3,5 zamiast dawnych 5: pomiar na 34 demonach (docs/AUDYT_BUILDCRAFT.md)
        /// dawał drodze żywiołowej ×1,8 wobec fizycznej przy pełnej inwestycji. Działa przy losowaniu: przedmioty z zapisu trwającego podejścia zachowują wylosowane wartości.
        /// </summary>
        public const float AddedElementScale = 3.5f;

        public static StatModifier? MapAffix(string stat, string op, float v)
        {
            bool pct = op.StartsWith("Percent");
            StatModifier F(StatType t, float x) => new StatModifier(t, x);
            switch (stat)
            {
                case "FireDamage": if (!pct) return F(StatType.AddedWeaponFire, v * AddedElementScale); break;
                case "WaterDamage": if (!pct) return F(StatType.AddedWeaponFrost, v * AddedElementScale); break;
                case "AirDamage": if (!pct) return F(StatType.AddedWeaponLightning, v * AddedElementScale); break;
                case "EarthDamage": if (!pct) return F(StatType.AddedWeaponEarth, v * AddedElementScale); break;
                case "CastSpeed": return F(StatType.CastSpeed, v * 100f);
                case "MinionDamage": return F(StatType.MinionDamage, v * 100f);
                case "HeavyPoiseDamage": return F(StatType.HeavyPoiseDamage, v * 100f);
                case "ShieldBlockCost": return F(StatType.ShieldBlockCost, v * 100f);
            }
            return Map(stat, op, v);
        }

        /// <summary>Statystyka affiksu ma odpowiednik (statystykę albo efekt pasywny).</summary>
        public static bool AffixSupported(string stat) => MapAffix(stat, "Flat", 1f).HasValue || MapEffect(stat, 1f).HasValue;

        /// <summary>Wersja znaczenia affiksów egzemplarza: 0 – stary zapis (dodane żywioły jako premia fizyczna), 2 – pakiet 2026-11-01.</summary>
        public const int AffixVersion = 2;

        /// <summary>
        /// Wąski filtr kwalifikacji przedmiotu (opisuje przedmiot, nie klasę gracza): przebicie – broń, grymuar, amulet; postawa –
        /// ciężka broń wręcz; sługi – hełm lub grymuar przywołań; blok – tarcza; rzucanie – broń-fokus albo grymuar.
        /// </summary>
        public static bool PassesFilter(string filter, ItemDefinition d)
        {
            if (string.IsNullOrEmpty(filter)) return true;
            if (d == null) return false;
            switch (filter)
            {
                case "penetration": return d.IsWeapon || d.kind == ItemKind.Amulet || WeaponFamilies.IsGrimoire(d);
                case "heavy_melee": return WeaponFamilies.IsHeavyMelee(d);
                case "minion": return d.kind == ItemKind.Head || WeaponFamilies.GrantsSummon(d);
                case "shield": return d.IsShield;
                case "caster": return WeaponFamilies.IsFocusWeapon(d) || WeaponFamilies.IsGrimoire(d);
                default: return false; // nieznany filtr nie dopuszcza – błąd danych nie rozdaje premii wszędzie
            }
        }

        /// <summary>Affiks może wypaść na tym przedmiocie (kategoria, dawne tagi „którykolwiek”, filtr Turris).</summary>
        public static bool Qualifies(AffixRow a, ItemDefinition d, string cat, HashSet<string> tags)
            => a.Categories.Contains(cat) && (a.RequiredTags.Length == 0 || a.RequiredTags.Any(tags.Contains)) && PassesFilter(a.Filter, d);

        public static StatModifier? Map(string entry)
        {
            var p = entry.Split('|');
            return p.Length < 3 ? null : Map(p[0], p[1], Num(p[2].Split(',')[0]));
        }

        /// <summary>
        /// Statystyka HU bez odpowiednika w statystykach Turris → efekt pasywny (te same jednostki co perki HU:
        /// ułamki → %, liczby sztuk bez zmian, życie ×2). Salwa i udział wybuchu to tabele poziomów perka – tylko perki.
        /// </summary>
        public static PassiveEffect? MapEffect(string stat, float v)
        {
            PassiveEffect E(PassiveEffectType t, float x) => new PassiveEffect(t, x);
            switch (stat)
            {
                case "AreaSize": return E(PassiveEffectType.AreaSize, v * 100f);
                case "Evasion": return E(PassiveEffectType.Evasion, v * 100f);
                case "PassiveBlockChance": return E(PassiveEffectType.PassiveBlockChance, v * 100f);
                case "BlockCostReduction": return E(PassiveEffectType.BlockCostReduction, v * 100f);
                case "Knockback": return E(PassiveEffectType.MeleeKnockback, v);
                case "MeleeRange": return E(PassiveEffectType.MeleeRange, v * 100f);
                case "MomentumAttackSpeed": return E(PassiveEffectType.MomentumAttackSpeed, v * 100f);
                case "PhysicalPenetration": return E(PassiveEffectType.PenetrationPhysical, v * 100f);
                case "FirePenetration": return E(PassiveEffectType.PenetrationFire, v * 100f);
                case "WaterPenetration": return E(PassiveEffectType.PenetrationFrost, v * 100f);
                case "AirPenetration": return E(PassiveEffectType.PenetrationLightning, v * 100f);
                case "EarthPenetration": return E(PassiveEffectType.PenetrationEarth, v * 100f);
                case "ProjectilePierce": return E(PassiveEffectType.ProjectilePierce, v);
                case "ProjectileRicochet": return E(PassiveEffectType.ProjectileRicochet, v);
                case "ProjectileSplit": return E(PassiveEffectType.ProjectileSplit, v);
                case "ProjectileResidue": return E(PassiveEffectType.ProjectileResidue, v);
                case "ProjectileSpeed": return E(PassiveEffectType.ProjectileSpeed, v * 100f);
                case "ParryReflect": return E(PassiveEffectType.ParryReflect, v);
                case "BleedMaxStacks": return E(PassiveEffectType.BleedMaxStacksBonus, v);
                case "LifeOnKill": return E(PassiveEffectType.HealOnKill, v * 2f);
                case "ManaOnKill": return E(PassiveEffectType.ManaOnKill, v);
                case "SoulGain": return E(PassiveEffectType.SoulGain, v * 100f);
                case "Accuracy": return E(PassiveEffectType.Accuracy, v * 100f);
                default: return null;
            }
        }

        /// <summary>Moc unikatu HU (<c>ItemPower</c> 1–8) jako efekt pasywny; null = brak mocy.</summary>
        public static PassiveEffect? PowerEffect(int power, float value)
        {
            switch (power)
            {
                case 1: return new PassiveEffect(PassiveEffectType.CheatDeath, value);
                case 2: return new PassiveEffect(PassiveEffectType.CorpseExplosion, value);
                case 3: return new PassiveEffect(PassiveEffectType.EmberAura, value * PlayerCombat.HuDamageScale);
                case 4: return new PassiveEffect(PassiveEffectType.Thorns, value * PlayerCombat.HuDamageScale);
                case 5: return new PassiveEffect(PassiveEffectType.WeaponChainLightning, value);
                case 6: return new PassiveEffect(PassiveEffectType.FrostWard, value);
                case 7: return new PassiveEffect(PassiveEffectType.LowHealthDamage, value);
                case 8: return new PassiveEffect(PassiveEffectType.SoulHarvest, value);
                default: return null;
            }
        }

        /// <summary>Statystyka HU ma odpowiednik w Turris – statystykę albo efekt pasywny (affixy bez niego są pomijane).</summary>
        public static bool Supported(string stat) => Map(stat, "Flat", 1f).HasValue || MapEffect(stat, 1f).HasValue;

        // ------------------------------------------------------------------ Rzadkość

        /// <summary>Wagi rzadkości (zwykły, magiczny, rzadki, legendarny, unikat) wg źródła – jak <c>LootSettings</c> HU.</summary>
        static readonly Dictionary<LootSource, float[]> Odds = new Dictionary<LootSource, float[]>
        {
            { LootSource.Normal, new[] { 55f, 35f, 9f, 1f, 0f } },
            { LootSource.Elite, new[] { 15f, 45f, 28f, 9f, 3f } },
            { LootSource.Boss, new[] { 0f, 10f, 35f, 35f, 20f } },
            { LootSource.Shop, new[] { 45f, 40f, 13f, 2f, 0f } },
        };

        /// <summary>Legendarne od fali 5 (wcześniej – rzadkie).</summary>
        public const int LegendaryMinWave = 5;

        public static ItemRarity RollRarity(LootSource source, int wave, System.Random rng)
        {
            var w = Odds[source];
            float roll = (float)rng.NextDouble() * w.Sum();
            int i = 0;
            for (; i < w.Length - 1; i++) { roll -= w[i]; if (roll <= 0f) break; }
            var r = (ItemRarity)i;
            if (r == ItemRarity.Legendary && wave < LegendaryMinWave) r = ItemRarity.Rare;
            return r;
        }

        /// <summary>Affiksy przedrostka i przyrostka wg rzadkości: magiczny 1+1, rzadki 2+2, legendarny 3+3.</summary>
        public static int AffixesPerKind(ItemRarity r) => r == ItemRarity.Magic ? 1 : r == ItemRarity.Rare ? 2 : r == ItemRarity.Legendary ? 3 : 0;

        public static float RarityPrice(ItemRarity r) => r == ItemRarity.Magic ? 1.6f : r == ItemRarity.Rare ? 2.4f : r == ItemRarity.Legendary ? 3.6f : 1f;

        public static string RarityName(ItemRarity r) =>
            r == ItemRarity.Magic ? "magiczny" : r == ItemRarity.Rare ? "rzadki" : r == ItemRarity.Legendary ? "legendarny" : r == ItemRarity.Unique ? "unikat" : "zwykły";

        public static Color RarityColor(ItemRarity r) =>
            r == ItemRarity.Magic ? new Color(0.45f, 0.65f, 1f) : r == ItemRarity.Rare ? new Color(1f, 0.85f, 0.3f)
            : r == ItemRarity.Legendary ? new Color(1f, 0.55f, 0.15f) : r == ItemRarity.Unique ? new Color(0.85f, 0.45f, 0.2f) : new Color(0.85f, 0.85f, 0.85f);

        // ------------------------------------------------------------------ Affiksy

        /// <summary>Kategoria affixów HU dla przedmiotu Turris.</summary>
        public static string Category(ItemDefinition d)
        {
            switch (d.kind)
            {
                case ItemKind.Weapon: return "Weapon";
                case ItemKind.Shield: case ItemKind.OffHand: return "Offhand";
                case ItemKind.Head: return "Head";
                case ItemKind.Body: return "Body";
                case ItemKind.Hands: return "Hands";
                case ItemKind.Feet: return "Feet";
                case ItemKind.Amulet: return "Amulet";
                case ItemKind.Ring: return "Ring";
                default: return "";
            }
        }

        /// <summary>Tagi przedmiotu dla wymagań affixów (klasa pancerza HU, broń wręcz/dystans/czar, tarcza).</summary>
        public static HashSet<string> TagsOf(ItemDefinition d)
        {
            var t = new HashSet<string>();
            var row = ItemRowOf(d);
            if (row != null) foreach (var x in row.Tags) t.Add(x);
            var weaponRow = string.IsNullOrEmpty(d.huSourceId) ? null : HellWeaponCatalog.Find(d.huSourceId);
            if (weaponRow != null) foreach (var x in weaponRow.Tags) t.Add(x);
            // Żywioł i rodzaj obrażeń broni (warunki perków: „fire”, „physical”).
            var la = d.weapon?.light;
            if (d.IsWeapon && la != null)
            {
                if (la.damageType == DamageType.Physical && (la.element == Element.None || la.elementShare < 1f)) t.Add("physical");
                if (la.element == Element.Fire) t.Add("fire");
                else if (la.element == Element.Frost) t.Add("water");
                else if (la.element == Element.Lightning) t.Add("air");
                else if (la.element == Element.Earth) t.Add("earth");
            }
            if (d.IsShield) t.Add("shield");
            if (d.IsWeapon && d.weapon?.light != null)
            {
                if (d.weapon.light.delivery == AttackDelivery.Projectile) t.Add("projectile"); else t.Add("melee");
                if ((WeaponFamilies.Of(d) & BuildTag.Focus) != 0) t.Add("spell");
            }
            return t;
        }

        public static ItemRow ItemRowOf(ItemDefinition d) => d == null ? null : Items.FirstOrDefault(r => r.id == d.huSourceId);

        /// <summary>Unikat HU: przedmiot z rzadkością 4 w items.json albo unikatowa broń (id z „.unique.”, katalog broni).</summary>
        public static bool IsUnique(ItemDefinition d) =>
            d != null && (d.IsNativeUnique || ItemRowOf(d)?.Rarity == 4 || (d.huSourceId != null && d.huSourceId.Contains(".unique.")));

        /// <summary>Unikat z katalogu Hell Unleashed (nie natywny Turris).</summary>
        public static bool IsHuUnique(ItemDefinition d) => d != null && !d.IsNativeUnique && IsUnique(d);

        /// <summary>
        /// Losuje affixy (HU <c>LootTable</c>): przedrostki, potem przyrostki; grupa raz na przedmiot, waga grupy = najwyższa waga
        /// jej odblokowanych tierów, tier z wagą równą numerowi (nowsze częściej), wartość = min + (max−min)·q, legendarny – lepsze q.
        /// Statystyki bez odpowiednika w statystykach Turris idą jako efekty pasywne (<see cref="MapEffect"/>).
        /// </summary>
        public static void RollAffixes(ItemInstance item, int wave, System.Random rng)
        {
            item.affixes.Clear(); item.affixEffects.Clear(); item.affixNames.Clear(); item.affixIds.Clear();
            item.affixVersion = AffixVersion;
            int per = AffixesPerKind(item.rarity);
            if (per == 0) return;
            string cat = Category(item.definition);
            var tags = TagsOf(item.definition);
            var usedGroups = new HashSet<string>();
            for (int kind = 0; kind < 2; kind++)
                for (int n = 0; n < per; n++)
                {
                    var pool = Affixes.Where(a => a.Kind == kind && !usedGroups.Contains(a.Group) && a.MinWave <= wave && Qualifies(a, item.definition, cat, tags)
                                                  && a.Ranges.All(r => AffixSupported(r.Split('|')[0]))).ToList();
                    if (pool.Count == 0) break;
                    var groups = pool.GroupBy(a => a.Group).ToList();
                    var g = Pick(groups, x => x.Max(a => a.Weight), rng);
                    var tier = Pick(g.ToList(), a => a.Tier, rng);
                    usedGroups.Add(tier.Group);
                    double q = rng.NextDouble();
                    if (item.rarity == ItemRarity.Legendary) q = Math.Max(q, rng.NextDouble());
                    foreach (var range in tier.Ranges)
                    {
                        var p = range.Split('|');
                        float min = Num(p[2]), max = Num(p[3]);
                        float v = min + (max - min) * (float)q;
                        if (tier.Decimals >= 0) { float k = Mathf.Pow(10f, tier.Decimals); v = Mathf.Round(v * k) / k; } // precyzja zadeklarowana w nakładce
                        else v = Mathf.Abs(max) >= 2f && !p[1].StartsWith("Percent") ? Mathf.Round(v) : Mathf.Round(v * 1000f) / 1000f;
                        var m = MapAffix(p[0], p[1], v);
                        if (m.HasValue) item.affixes.Add(m.Value);
                        else if (MapEffect(p[0], v) is PassiveEffect fx) item.affixEffects.Add(fx);
                    }
                    item.affixNames.Add(tier.name);
                    item.affixIds.Add(tier.id);
                }
        }

        static T Pick<T>(IList<T> list, Func<T, float> weight, System.Random rng)
        {
            float total = list.Sum(weight), roll = (float)rng.NextDouble() * total;
            foreach (var x in list) { roll -= weight(x); if (roll <= 0f) return x; }
            return list[list.Count - 1];
        }

        /// <summary>Egzemplarz z rzadkością, jakością (elita 0–1, boss 1–2) i affixami; unikat i zwykły – bez affixów.</summary>
        public static ItemInstance Roll(ItemDefinition def, LootSource source, int wave, System.Random rng, ItemRarity? rarity = null)
        {
            var it = new ItemInstance(def);
            bool unique = IsUnique(def);
            it.rarity = unique ? ItemRarity.Unique : rarity ?? RollRarity(source, wave, rng);
            if (it.rarity == ItemRarity.Unique && !unique) it.rarity = wave < LegendaryMinWave ? ItemRarity.Rare : ItemRarity.Legendary;
            it.huQuality = source == LootSource.Elite ? rng.Next(0, 2) : source == LootSource.Boss ? rng.Next(1, 3) : 0;
            RollAffixes(it, wave, rng);
            return it;
        }

        /// <summary>Wartość egzemplarza w duszach (przed inflacją): cena bazowa HU × rzadkość × jakość × skala Turris.</summary>
        public static int Value(ItemInstance it)
        {
            var row = ItemRowOf(it.definition);
            var card = UniqueRegistry.CardOf(it.definition);
            // Natywny unikat: cena karty × 3,6 (jak legendarny – moc zamiast affixów).
            float basePrice = card != null ? card.basePrice * 3.6f : row != null ? row.BasePrice : WeaponBasePrice(it.definition);
            float level = it.definition.IsWeapon ? 1f + 0.5f * it.level : 1f; // HU: broń × (1 + 0,5 · (poziom − 1))
            return Mathf.RoundToInt(basePrice * RarityPrice(it.rarity) * (1f + 0.25f * it.huQuality) * level * PriceScale);
        }

        // ------------------------------------------------------------------ Broń: poziomy

        /// <summary>Cena bazowa broni z katalogu HU (25 dla broni spoza katalogu).</summary>
        public static float WeaponBasePrice(ItemDefinition d)
        {
            if (d != null && d.IsNativeUnique && d.familyBase != null) return WeaponBasePrice(d.familyBase) * 1.5f; // unikat: droższe poziomy
            var row = d != null && !string.IsNullOrEmpty(d.huSourceId) ? HellWeaponCatalog.Find(d.huSourceId) : null;
            return row != null && row.BasePrice > 0f ? row.BasePrice : 25f;
        }

        /// <summary>Ostatni poziom broni w Turris (HU: MaxLevel 5 = poziomy 1–5, Turris: 0–4).</summary>
        public const int MaxWeaponLevel = 4;

        /// <summary>Cena kolejnego poziomu broni (HU: BasePrice × 1,6^poziom) w duszach przed inflacją.</summary>
        public static int WeaponLevelPrice(ItemInstance w)
        {
            if (w.definition != null && w.definition.IsShield) return Mathf.RoundToInt(ShieldBasePrice * Mathf.Pow(ShieldPriceGrowth, w.level + 1) * PriceScale);
            var row = w.definition != null && !string.IsNullOrEmpty(w.definition.huSourceId) ? HellWeaponCatalog.Find(w.definition.huSourceId) : null;
            float growth = row != null && row.PriceGrowth > 0f ? row.PriceGrowth : 1.6f;
            return Mathf.RoundToInt(WeaponBasePrice(w.definition) * Mathf.Pow(growth, w.level + 1) * PriceScale);
        }

        /// <summary>Ostatni poziom tej broni w Turris (MaxLevel HU − 1; tarcze i grymuary w HU mają 3 poziomy).</summary>
        /// <summary>Tarcza HU (<c>weapon.shield</c>): 3 poziomy, cena 28 × 1,7^poziom; poziom rośnie z ponownego zakupu w sklepie.</summary>
        public const int ShieldMaxLevel = 2;
        public const float ShieldBasePrice = 28f, ShieldPriceGrowth = 1.7f;

        public static int MaxLevelOf(ItemDefinition d)
        {
            if (d != null && d.IsShield) return ShieldMaxLevel;
            var row = d != null && !string.IsNullOrEmpty(d.huSourceId) ? HellWeaponCatalog.Find(d.huSourceId) : null;
            return row != null ? Mathf.Clamp(row.MaxLevel - 1, 0, MaxWeaponLevel) : MaxWeaponLevel;
        }
    }
}
