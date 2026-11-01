using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Rejestr natywnych unikatów Turris: z kart <see cref="UniqueCatalog"/> buduje w pamięci <see cref="ItemDefinition"/> dla danej
    /// konfiguracji (bez assetów i bez <c>TurrisSetup.RunBatchForce</c>). Broń, tarcza i grymuar kopiują przedmiot bazowy z puli
    /// (ataki, model, chwyt, skill grymuaru); pancerz i biżuteria to czyste definicje. Definicje nie trafiają do zwykłej puli
    /// (sklep i nagrody ich nie widzą), lecz do indeksu zapisu (<see cref="RunSnapshot.Index"/>) i puli unikatów łupu.
    /// </summary>
    public static class UniqueRegistry
    {
        static readonly Dictionary<GameConfig, List<ItemDefinition>> built = new Dictionary<GameConfig, List<ItemDefinition>>();
        static readonly Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();

        /// <summary>Wszystkie natywne unikaty tej konfiguracji (75 – o ile istnieją przedmioty bazowe).</summary>
        public static IReadOnlyList<ItemDefinition> Items(GameConfig cfg)
        {
            if (cfg == null) return new List<ItemDefinition>();
            if (built.TryGetValue(cfg, out var list) && list.All(i => i != null)) return list;
            list = new List<ItemDefinition>();
            var bases = BaseIndex(cfg);
            foreach (var card in UniqueCatalog.All)
            {
                var item = Make(card, bases);
                if (item != null) list.Add(item);
                else Debug.LogWarning($"[Turris] Unikat {card.code} {card.id}: brak przedmiotu bazowego {card.baseItemId}.");
            }
            built[cfg] = list;
            return list;
        }

        public static ItemDefinition Find(GameConfig cfg, string id) => Items(cfg).FirstOrDefault(i => i.id == id);

        /// <summary>Karta unikatu dla definicji (null – nie natywny unikat).</summary>
        public static UniqueCard CardOf(ItemDefinition d) => d != null && d.IsNativeUnique ? UniqueCatalog.Get(d.uniquePowerId) : null;

        /// <summary>Ikona karty (Resources/UniqueIcons/&lt;kod&gt;) – null, gdy brak pliku.</summary>
        public static Texture2D Icon(UniqueCard card)
        {
            if (card == null) return null;
            if (icons.TryGetValue(card.code, out var t)) return t;
            t = Resources.Load<Texture2D>("UniqueIcons/" + card.code);
            icons[card.code] = t;
            return t;
        }

        /// <summary>Przedmioty, z których mogą powstać unikaty (pula, klasy, zestawy startowe) – po id.</summary>
        static Dictionary<string, ItemDefinition> BaseIndex(GameConfig cfg)
        {
            var map = new Dictionary<string, ItemDefinition>();
            void Add(ItemDefinition d) { if (d != null && !string.IsNullOrEmpty(d.id) && !d.IsNativeUnique && !map.ContainsKey(d.id)) map[d.id] = d; }
            foreach (var d in cfg.baseItemPool) Add(d);
            foreach (var c in cfg.classes.Where(c => c != null))
            {
                foreach (var d in c.startingItems) Add(d);
                foreach (var d in c.alternativeWeapons) Add(d);
                foreach (var l in c.loadouts) { Add(l.weapon); Add(l.offHand); }
            }
            foreach (var u in cfg.unlocks) Add(u.target as ItemDefinition);
            return map;
        }

        static float WeightFor(ItemKind k)
        {
            switch (k)
            {
                case ItemKind.Body: return 5f;
                case ItemKind.Head: return 2f;
                case ItemKind.Hands: case ItemKind.Feet: return 1.5f;
                case ItemKind.Belt: return 1f;
                default: return 0.3f;
            }
        }

        static ItemDefinition Make(UniqueCard card, Dictionary<string, ItemDefinition> bases)
        {
            ItemDefinition baseItem = null;
            if (!string.IsNullOrEmpty(card.baseItemId) && !bases.TryGetValue(card.baseItemId, out baseItem)) return null;
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            if (baseItem != null)
            {
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(baseItem), item);
                item.model = baseItem.model;
                item.grantedSkill = baseItem.grantedSkill;
                item.familyBase = baseItem;
                item.color = Color.Lerp(baseItem.color, new Color(0.85f, 0.45f, 0.2f), 0.35f);
            }
            else
            {
                item.kind = card.kind;
                item.weight = WeightFor(card.kind);
                item.color = new Color(0.62f, 0.32f, 0.18f);
            }
            item.kind = card.kind;
            item.id = item.name = card.id;
            item.displayName = card.name;
            item.description = card.promise;
            item.huSourceId = null; item.huSourceHash = null; // natywny – bez pochodzenia HU
            item.uniquePowerId = card.id;
            item.minFloor = card.minFloor;
            item.offerWeight = card.weight;
            item.modifiers = new List<StatModifier>(card.stats);
            item.effects = new List<PassiveEffect>();
            item.requirements = baseItem != null ? new List<AttributeRequirement>(baseItem.requirements) : new List<AttributeRequirement>();
            return item;
        }

        /// <summary>
        /// Czy moc karty działałaby w tym podejściu (wybrana cecha właściciela, skill karty w slotach) – ocena botów i podpowiedzi,
        /// bez kontrolera. Warunki sytuacyjne (aura włączona, sługi) nie są tu sprawdzane.
        /// </summary>
        public static bool WouldBeActive(UniqueCard card, RunState run)
        {
            if (card == null || run == null) return false;
            if (card.requiredFeature != null && !(run.classFeatureId == card.requiredFeature && ClassFeatureCatalog.Owns(run.startingClass, card.requiredFeature))) return false;
            if (card.skills.Count == 0) return true;
            foreach (var s in run.skillSlots) if (s != null && card.skills.Contains(s.definition.id)) return true;
            return false;
        }

        /// <summary>Tylko testy: odbudowa po zmianie treści w pamięci.</summary>
        public static void ResetForTests() { built.Clear(); }
    }
}
