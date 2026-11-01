using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>Jedna oferta sklepu między piętrami: broń (nowa albo kolejny poziom trzymanej), perk albo przedmiot.</summary>
    public class FloorOffer
    {
        public enum Kind { WeaponLevel, Weapon, Perk, Item }
        public Kind kind;
        public ItemInstance item;       // broń/przedmiot (egzemplarz z rzadkością i affixami) albo trzymana broń przy poziomie
        public BoonDefinition perk;
        public int price;
        public bool sold;

        public string Title => kind == Kind.WeaponLevel ? $"{item.definition.displayName}: poziom {item.level + 2}"
            : kind == Kind.Perk ? perk.displayName
            : item.rarity == ItemRarity.Common ? item.Name : $"{item.Name} ({HellEconomy.RarityName(item.rarity)})";
    }

    /// <summary>
    /// Sklep po każdym piętrze, jak w Hell Unleashed (<c>ShopGenerator</c>): 3 bronie (zawsze kolejny poziom trzymanej, dopóki ma
    /// poziom), 4 perki aktywne dla buildu, 6 przedmiotów (3 dla klasy bohatera + 3 dowolne, co najmniej jeden magiczny i jeden
    /// rzadki, od piętra 5 także legendarny). Bez unikatów (te tylko z łupów). Ceny HU × 2,5 × inflacja 15% za piętro.
    /// Pieczęcie i oferta skilla mają własne sekcje sklepu dusz.
    /// </summary>
    public static class FloorShop
    {
        public const int WeaponCount = 3, PerkCount = 4, ClassItemCount = 3, ItemCount = 3;

        /// <summary>Rodzina klasy HU (warrior / mage / rogue) – do wagi treści własnej klasy (inna ×0,35).</summary>
        public static string Family(ClassDefinition c)
        {
            var root = c != null && c.parent != null ? c.parent : c;
            string id = root?.id ?? "";
            return id.Contains("mage") ? "mage" : id.Contains("rogue") ? "rogue" : "warrior";
        }

        /// <summary>Klasa pancerza HU dla rodziny: wojownik – ciężki, łotrzyk – lekki, mag – szata.</summary>
        static string ArmourOf(string family) => family == "mage" ? "robe" : family == "rogue" ? "light" : "heavy";

        public const float OffClassWeight = 0.35f;

        /// <summary>Perk HU dla wzmocnienia Turris (null – zwykłe wzmocnienie Turris).</summary>
        public static HellEconomy.UpgradeRow PerkRow(BoonDefinition b) =>
            b == null || !b.id.StartsWith("hu_perk_") ? null : HellEconomy.Upgrades.FirstOrDefault(r => DefaultContent.HuPerkId(r) == b.id);

        /// <summary>Czy perk działa z obecnym wyposażeniem (HU <c>StatSystem.IsActive</c>): broń główna, druga ręka albo dowolna musi mieć wymagane tagi.</summary>
        public static bool PerkActive(HellEconomy.UpgradeRow r, RunState run) => PerkMechanics.Active(r, run);

        public static int PerkPrice(HellEconomy.UpgradeRow r, int ownedLevel, RunState run, BalanceConfig b) =>
            Mathf.RoundToInt(r.BasePrice * Mathf.Pow(r.PriceGrowth, ownedLevel) * HellEconomy.PriceScale * SoulShop.Inflation(run, b));

        static T Pick<T>(List<T> pool, System.Func<T, float> weight, System.Random rng)
        {
            float total = pool.Sum(weight), roll = (float)rng.NextDouble() * total;
            for (int i = 0; i < pool.Count; i++) { roll -= weight(pool[i]); if (roll <= 0f) return pool[i]; }
            return pool[pool.Count - 1];
        }

        public static List<FloorOffer> Generate(RunState run, GameConfig cfg, System.Random rng)
        {
            var b = cfg.balance;
            float infl = SoulShop.Inflation(run, b);
            int wave = run.floorsCleared + 1;
            string family = Family(run.startingClass);
            float Affinity(string classId) => string.IsNullOrEmpty(classId) || classId.StartsWith(family) ? 1f : OffClassWeight;
            var offers = new List<FloorOffer>();

            // Bronie: kolejny poziom trzymanej (nie unikat) + nowe egzemplarze.
            var held = run.equipment.Get(EquipSlot.MainHand);
            // Natywny unikat Turris ma tę samą drogę ulepszeń (poziom broni, droższy ×1,5) – unikat HU nie.
            if (held != null && held.definition.IsWeapon && (!HellEconomy.IsUnique(held.definition) || held.definition.IsNativeUnique) && held.level < HellEconomy.MaxLevelOf(held.definition))
                offers.Add(new FloorOffer { kind = FloorOffer.Kind.WeaponLevel, item = held, price = Mathf.RoundToInt(HellEconomy.WeaponLevelPrice(held) * infl) });
            // Tarcza: kolejny poziom jak w HU (blok bierny +2 pkt, krótsze odnowienie ładunku).
            var shield = run.equipment.Get(EquipSlot.OffHand);
            if (shield != null && shield.definition.IsShield && shield.level < HellEconomy.MaxLevelOf(shield.definition))
                offers.Add(new FloorOffer { kind = FloorOffer.Kind.WeaponLevel, item = shield, price = Mathf.RoundToInt(HellEconomy.WeaponLevelPrice(shield) * infl) });
            var weapons = cfg.baseItemPool.Where(d => d != null && d.IsWeapon && !HellEconomy.IsUnique(d) && d.minFloor <= run.floorsCleared && !string.IsNullOrEmpty(d.huSourceId)
                                                       && (held == null || d != held.definition)).ToList();
            while (offers.Count(o => o.kind != FloorOffer.Kind.Perk && o.kind != FloorOffer.Kind.Item) < WeaponCount && weapons.Count > 0)
            {
                var d = Pick(weapons, w => w.offerWeight * Affinity(HellWeaponCatalog.Find(w.huSourceId)?.ClassId), rng);
                weapons.Remove(d);
                var it = HellEconomy.Roll(d, LootSource.Shop, wave, rng);
                offers.Add(new FloorOffer { kind = FloorOffer.Kind.Weapon, item = it, price = Mathf.RoundToInt(HellEconomy.Value(it) * infl) });
            }

            // Perki: aktywne dla wyposażenia, poniżej ostatniego poziomu.
            var perks = cfg.baseBoonPool.Where(x => x != null).Select(x => (def: x, row: PerkRow(x))).Where(x => x.row != null && PerkActive(x.row, run) && run.BoonStacks(x.def) < x.def.maxStacks).ToList();
            // Natywne perki Turris we wspólnej puli (neutralne klasowo, tylko gdy build może je wykorzystać, od swojego piętra).
            var nt = NativePerkCatalog.Tuning(cfg);
            var natives = NativePerkCatalog.Boons(cfg).Where(d => run.BoonStacks(d) < d.maxStacks && run.floorsCleared + 1 >= NativePerkCatalog.MinFloor(d.id, nt)
                                                             && NativePerkCatalog.Usable(d.id, run)).ToList();
            for (int i = 0; i < PerkCount && perks.Count + natives.Count > 0; i++)
            {
                float huTotal = perks.Sum(x => x.row.Weight * Affinity(x.row.ClassId)), natTotal = natives.Sum(d => NativePerkCatalog.Weight(d.id, nt));
                if (natives.Count > 0 && (float)rng.NextDouble() * (huTotal + natTotal) < natTotal)
                {
                    var d = Pick(natives, x => NativePerkCatalog.Weight(x.id, nt), rng);
                    natives.Remove(d);
                    offers.Add(new FloorOffer { kind = FloorOffer.Kind.Perk, perk = d, price = NativePerkCatalog.Price(d.id, nt, run, b) });
                    continue;
                }
                if (perks.Count == 0) break;
                var p = Pick(perks, x => x.row.Weight * Affinity(x.row.ClassId), rng);
                perks.Remove(p);
                offers.Add(new FloorOffer { kind = FloorOffer.Kind.Perk, perk = p.def, price = PerkPrice(p.row, run.BoonStacks(p.def), run, b) });
            }

            // Przedmioty: 3 dla klasy (jej klasa pancerza), 3 dowolne; gwarantowane rzadkości jak w HU, bez unikatów.
            var gear = cfg.baseItemPool.Where(d => d != null && !d.IsWeapon && !d.IsShield && d.kind != ItemKind.OffHand && !HellEconomy.IsUnique(d)
                                                   && d.minFloor <= run.floorsCleared && HellEconomy.ItemRowOf(d) is HellEconomy.ItemRow row && row.InShop).ToList();
            string armour = ArmourOf(family);
            var own = gear.Where(d => HellEconomy.ItemRowOf(d).Tags.Contains(armour)).ToList();
            var picked = new List<ItemDefinition>();
            for (int i = 0; i < ClassItemCount && own.Count > 0; i++) { var d = Pick(own, x => x.offerWeight, rng); own.Remove(d); gear.Remove(d); picked.Add(d); }
            while (picked.Count < ItemCount + ClassItemCount && gear.Count > 0)
            {
                var d = Pick(gear, x => x.offerWeight * Affinity(HellEconomy.ItemRowOf(x).ClassId), rng);
                gear.Remove(d); picked.Add(d);
            }
            var rarities = new List<ItemRarity> { ItemRarity.Magic, ItemRarity.Rare };
            if (wave >= HellEconomy.LegendaryMinWave) rarities.Add(ItemRarity.Legendary);
            while (rarities.Count < picked.Count) rarities.Add(HellEconomy.RollRarity(LootSource.Shop, wave, rng));
            rarities = rarities.Take(picked.Count).OrderBy(_ => rng.Next()).ToList();
            for (int i = 0; i < picked.Count; i++)
            {
                var it = HellEconomy.Roll(picked[i], LootSource.Shop, wave, rng, rarities[i]);
                offers.Add(new FloorOffer { kind = FloorOffer.Kind.Item, item = it, price = Mathf.RoundToInt(HellEconomy.Value(it) * infl) });
            }
            return offers;
        }

        /// <summary>Zakup atomowy: dusze znikają tylko, gdy oferta została zastosowana.</summary>
        public static bool Buy(FloorOffer o, RunState run)
        {
            if (o == null || o.sold || run.souls < o.price) return false;
            switch (o.kind)
            {
                case FloorOffer.Kind.WeaponLevel:
                    if (o.item.level >= HellEconomy.MaxLevelOf(o.item.definition)) return false;
                    o.item.level++;
                    break;
                case FloorOffer.Kind.Weapon:
                case FloorOffer.Kind.Item:
                    if (!run.CanReceive(o.item)) return false; // HU SlotBlocked: slot zajęty i pełny plecak
                    run.AddItem(o.item, true);
                    break;
                case FloorOffer.Kind.Perk:
                    if (run.BoonStacks(o.perk) >= o.perk.maxStacks) return false;
                    if (NativePerkCatalog.IsNative(o.perk) && !NativePerkCatalog.Usable(o.perk.id, run)) return false; // kwalifikacja ponownie przy zakupie
                    run.AddBoon(o.perk);
                    break;
            }
            run.souls -= o.price;
            o.sold = true;
            return true;
        }
    }
}
