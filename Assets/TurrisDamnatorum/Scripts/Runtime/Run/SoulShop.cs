using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Sklep dusz między piętrami (etap D planu): ulepszenie umiejętności, zakup losowej umiejętności na to podejście
    /// i przerzucenie nagród. Ceny rosną z każdym ukończonym piętrem (inflacja), więc nie da się wykupić wszystkiego.
    /// Czysta logika – testowalna bez sceny.
    /// </summary>
    public static class SoulShop
    {
        /// <summary>Mnożnik cen: ×(1 + inflacja × (ukończone piętra − 1)).</summary>
        public static float Inflation(RunState run, BalanceConfig b) => 1f + b.shopInflationPerFloor * Mathf.Max(0, run.floorsCleared - 1);

        public static int UpgradeCost(SpellInstance s, RunState run, BalanceConfig b) =>
            Mathf.RoundToInt(b.skillUpgradeSoulCost * (1 + s.level) * Inflation(run, b));

        public static int OfferCost(RunState run, BalanceConfig b) => Mathf.RoundToInt(b.skillOfferSoulCost * Inflation(run, b));

        public static bool CanUpgrade(SpellInstance s, RunState run, BalanceConfig b, out string reason)
        {
            reason = null;
            if (s == null) { reason = "Brak umiejętności"; return false; }
            if (s.IsMaxLevel) { reason = "Maksymalny poziom"; return false; }
            if (run.souls < UpgradeCost(s, run, b)) { reason = "Za mało dusz"; return false; }
            return true;
        }

        /// <summary>Zakup jest atomowy: albo poziom rośnie i dusze znikają, albo nic się nie zmienia.</summary>
        public static bool TryUpgrade(SpellInstance s, RunState run, BalanceConfig b)
        {
            if (!CanUpgrade(s, run, b, out _)) return false;
            run.souls -= UpgradeCost(s, run, b);
            s.level++;
            return true;
        }

        /// <summary>
        /// Losowa umiejętność, której postać nie zna i której może od razu użyć: wyposażenie pasuje,
        /// a czar nie ma niespełnionych twardych wymagań Inteligencji (za dusze nie sprzedajemy „martwej” umiejętności).
        /// </summary>
        public static SpellDefinition PickOffer(RunState run, BuildSnapshot snap, RewardPools pools, System.Random rng, BalanceConfig b = null) =>
            PickOffers(run, snap, pools, rng, b, 1).FirstOrDefault();

        /// <summary>Ofert skilli naraz w sklepie po piętrze (HU <c>ShopGenerator</c>: trzy skille do wyboru).</summary>
        public const int SkillOfferCount = 3;

        /// <summary>Do <paramref name="count"/> różnych skilli na sprzedaż (zasady jak <see cref="PickOffer"/>).</summary>
        public static List<SpellDefinition> PickOffers(RunState run, BuildSnapshot snap, RewardPools pools, System.Random rng, BalanceConfig b = null, int count = SkillOfferCount)
        {
            var tags = run.CurrentTags(snap);
            bool hard = b == null || b.spellRequirementsAreHard;
            var candidates = pools.spells.Where(s => s != null && run.FindSpell(s) == null && WeaponFamilies.Meets(s.requiredTags, tags) && run.floorsCleared >= s.minFloor
                && !(hard && s.IsSpell && !StatCalculator.RequirementsMet(s.requirements, snap.stats))).Distinct().ToList();
            var picked = new List<SpellDefinition>();
            while (picked.Count < count && candidates.Count > 0)
            {
                int i = rng.Next(candidates.Count);
                picked.Add(candidates[i]);
                candidates.RemoveAt(i);
            }
            return picked;
        }

        public static bool TryBuyOffer(SpellDefinition offer, RunState run, BalanceConfig b)
        {
            if (offer == null || run.FindSpell(offer) != null || run.souls < OfferCost(run, b)) return false;
            run.souls -= OfferCost(run, b);
            run.LearnSpell(offer, level: SkillTiers.StartLevel(offer, run.floorsCleared)); // progi HU: od piętra 7 poziom II, od 16 – III
            return true;
        }
    }
}
