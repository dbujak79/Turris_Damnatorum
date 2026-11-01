using System.Collections.Generic;
using System.Linq;

namespace Turris
{
    /// <summary>
    /// Pula unikatów łupu: unikaty HU z puli (jak dotąd) i natywne unikaty Turris z rejestru. Rzadkość Unique losuje
    /// <see cref="HellEconomy.RollRarity"/> bez zmian – tu wybiera się tylko, który unikat wypadnie.
    ///
    /// Wagi natywnych kart (po progu piętra i źródle): własna klasa ×1, inna klasa tej samej rodziny ×0,3, reszta ×0,1;
    /// wskazany „poszukiwany” odkryty unikat ×<see cref="SoughtFactor"/>. Przy typowym podejściu własny przedmiot to ok. 5%
    /// wylosowanych unikatów, poszukiwany ok. 18% (symulacja w <c>UniqueLootTests</c>) – preferencja, nie gwarancja.
    /// </summary>
    public static class UniqueLoot
    {
        public const float OwnClass = 1f, SameFamily = 0.3f, OtherClass = 0.1f, SoughtFactor = 4f;

        /// <summary>Rodzina klasy po id karty (warrior / mage / rogue) – jak <see cref="FloorShop.Family"/>.</summary>
        public static string FamilyOf(string classId)
        {
            if (string.IsNullOrEmpty(classId)) return "";
            if (classId == "class_knight" || classId.Contains("paladin") || classId.Contains("berserker") || classId.Contains("dual")) return "warrior";
            if (classId == "class_rogue" || classId.Contains("archer") || classId.Contains("assassin")) return "rogue";
            return "mage";
        }

        public static float Affinity(UniqueCard card, ClassDefinition cls)
        {
            if (card == null || cls == null) return OtherClass;
            if (card.classId == cls.id) return OwnClass;
            return FamilyOf(card.classId) == FloorShop.Family(cls) ? SameFamily : OtherClass;
        }

        /// <summary>Kandydaci z wagą; puste – brak unikatu na tym piętrze.</summary>
        public static List<KeyValuePair<ItemDefinition, float>> Pool(GameConfig cfg, RunState run, LootSource source, string sought)
        {
            var list = new List<KeyValuePair<ItemDefinition, float>>();
            if (cfg == null || run == null) return list;
            string family = FloorShop.Family(run.startingClass);
            foreach (var d in cfg.baseItemPool)
            {
                if (d == null || !HellEconomy.IsHuUnique(d) || d.minFloor > run.floorsCleared || d.IsShield || d.kind == ItemKind.OffHand) continue;
                string cls = HellEconomy.ItemRowOf(d)?.ClassId ?? HellWeaponCatalog.Find(d.huSourceId ?? "")?.ClassId;
                float aff = string.IsNullOrEmpty(cls) || cls.StartsWith(family) ? 1f : FloorShop.OffClassWeight;
                list.Add(new KeyValuePair<ItemDefinition, float>(d, d.offerWeight * aff));
            }
            foreach (var d in UniqueRegistry.Items(cfg))
            {
                var card = UniqueRegistry.CardOf(d);
                if (card == null || card.minFloor > run.floorsCleared || (card.bossOnly && source != LootSource.Boss)) continue;
                float w = card.weight * Affinity(card, run.startingClass) * (card.id == sought ? SoughtFactor : 1f);
                list.Add(new KeyValuePair<ItemDefinition, float>(d, w));
            }
            return list;
        }

        public static ItemDefinition Pick(GameConfig cfg, RunState run, LootSource source, string sought, System.Random rng)
        {
            var pool = Pool(cfg, run, source, sought);
            float total = pool.Sum(p => p.Value);
            if (pool.Count == 0 || total <= 0f) return null;
            float roll = (float)rng.NextDouble() * total;
            foreach (var p in pool) { roll -= p.Value; if (roll <= 0f) return p.Key; }
            return pool[pool.Count - 1].Key;
        }
    }
}
