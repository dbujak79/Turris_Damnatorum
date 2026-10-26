using System;
using System.Collections.Generic;
using System.Linq;

namespace Turris
{
    /// <summary>Kto upuścił pieczęć.</summary>
    public enum SealSource { Normal, Elite, Boss }

    /// <summary>Oferta pieczęci w sklepie dusz: pieczęć i cena (kolejny poziom posiadanej jest droższy).</summary>
    public sealed class SealOffer
    {
        public HuSeal seal;
        public int price;
        public bool rankUp;
        public bool sold;
    }

    /// <summary>
    /// Pieczęcie w ekonomii podejścia (port <c>SealLoot</c> i ofert sklepu z HU, ceny w duszach Turris).
    ///
    /// Łup skłania się ku temu, czego bohater użyje: forma tylko dla posiadanego skilla, wsparcie przyjmowane przez któryś
    /// skill 3× częstsze niż takie do sakwy, a pieczęć budząca zespolenie albo więź 3× częstsza w sklepie i w darze bossa.
    /// Posiadana pieczęć wraca jako kolejny poziom (do III) – za ×1,5 / ×2 ceny i z połową wagi.
    /// </summary>
    public static class HuSealEconomy
    {
        /// <summary>Ceny HU → dusze Turris (skill HU ~42 = oferta skilla Turris 110 dusz).</summary>
        public const float SoulFactor = 2.5f;
        public const float SaleShare = 0.35f;
        public const int DemonicSaleValue = 100;
        public const int RerollBase = 38, RerollStep = 25;
        public const double UsableWeight = 3.0, WakingWeight = 3.0;
        public const double NormalChance = 0.005, EliteChance = 0.35, BossChance = 1.0;
        public const double DemonicEliteShare = 0.25, DemonicBossShare = 0.7;
        public const int GiftSize = 3, ShopOffers = 3;

        static IEnumerable<SealSet> Sealable(RunState run) => run.SealOrder().Select(s => s.seals);

        public static bool CanTake(RunState run, HuSeal seal) => seal.IsItem && (!run.OwnsSeal(seal) || run.SealRank(seal) < HuSkillShape.MaxRank);

        static bool Usable(RunState run, HuSeal seal) =>
            seal.IsForm ? Sealable(run).Any(s => s.data.id == seal.skillId) : Sealable(run).Any(s => s.Shape.Supports(seal));

        /// <summary>Czy pieczęć obudziłaby zespolenie w skillu, do którego by trafiła, albo więź w ręce.</summary>
        public static bool Wakes(RunState run, HuSeal seal)
        {
            if (run.OwnsSeal(seal)) return false;
            var target = run.SealOrder().FirstOrDefault(s => s.seals.Fit(seal) == SealFit.Fits);
            if (target == null) return false;
            var set = target.seals;
            var form = seal.IsForm ? seal : set.Form;
            var supports = new List<HuSeal>(set.Supports);
            if (!seal.IsForm) supports.Add(seal);
            var after = HuSkillShape.Compose(set.data, set.Level(), form, supports, HuCatalog.Synergies.ToList(), null, run.SealRank);
            if (after.Active.Any(a => a.kind == SealKind.Synergy && !set.Shape.IsActive(a.id))) return true;
            var armed = run.Armed().Distinct().ToList();
            if (!armed.Contains(target)) return false;
            var shapes = armed.Select(s => s == target ? after : s.seals.Unbonded).ToList();
            return HuBondRules.Holding(HuCatalog.Bonds, shapes).Any(m => !run.activeBonds.Contains(m.bond));
        }

        static List<(HuSeal seal, double weight)> Candidates(RunState run, bool demonic)
        {
            var pool = new List<(HuSeal, double)>();
            foreach (var seal in HuCatalog.Seals)
            {
                if (!seal.IsItem || seal.lootOnly != demonic || !CanTake(run, seal)) continue;
                if (run.OwnsSeal(seal)) { pool.Add((seal, seal.weight)); continue; }
                if (seal.IsForm) { if (Usable(run, seal)) pool.Add((seal, seal.weight * UsableWeight)); continue; }
                pool.Add((seal, seal.weight * (Usable(run, seal) ? UsableWeight : 1.0)));
            }
            return pool;
        }

        static HuSeal Pick(List<(HuSeal seal, double weight)> pool, Random rng)
        {
            double total = pool.Sum(p => Math.Max(0.001, p.weight));
            double roll = rng.NextDouble() * total;
            foreach (var p in pool) { roll -= Math.Max(0.001, p.weight); if (roll <= 0) return p.seal; }
            return pool[pool.Count - 1].seal;
        }

        /// <summary>Pieczęć z zabitego wroga albo null (zwykły rzadko, elita często, boss zawsze).</summary>
        public static HuSeal Roll(RunState run, SealSource source, Random rng)
        {
            double chance = source == SealSource.Boss ? BossChance : source == SealSource.Elite ? EliteChance : NormalChance;
            if (rng.NextDouble() >= chance) return null;
            double share = source == SealSource.Boss ? DemonicBossShare : source == SealSource.Elite ? DemonicEliteShare : 0.0;
            bool demonic = share > 0 && rng.NextDouble() < share;
            var pool = Candidates(run, demonic);
            if (pool.Count == 0 && demonic) pool = Candidates(run, false);
            return pool.Count == 0 ? null : Pick(pool, rng);
        }

        /// <summary>Dar bossa: trzy różne pieczęcie do wyboru jednej – tylko takie, które któryś skill przyjmie (albo poziomy posiadanych).</summary>
        public static List<HuSeal> RollGift(RunState run, Random rng, int count = GiftSize)
        {
            var gift = new List<HuSeal>();
            for (int i = 0; i < count; i++)
            {
                bool demonic = rng.NextDouble() < DemonicBossShare;
                var pool = UsableOnly(Candidates(run, demonic), run, gift);
                if (pool.Count == 0) pool = UsableOnly(Candidates(run, !demonic), run, gift);
                if (pool.Count == 0) pool = Candidates(run, demonic).Where(e => !gift.Contains(e.seal)).ToList();
                if (pool.Count == 0) break;
                gift.Add(Pick(pool, rng));
            }
            return gift;
        }

        static List<(HuSeal, double)> UsableOnly(List<(HuSeal seal, double weight)> pool, RunState run, List<HuSeal> taken) =>
            pool.Where(e => !taken.Contains(e.seal) && (run.OwnsSeal(e.seal) || e.seal.IsForm || Usable(run, e.seal)))
                .Select(e => (e.seal, e.weight * (Wakes(run, e.seal) ? WakingWeight : 1.0))).ToList();

        public static int Price(RunState run, HuSeal seal, BalanceConfig b)
        {
            float rank = run.OwnsSeal(seal) ? (run.SealRank(seal) >= 2 ? 2f : 1.5f) : 1f;
            return UnityEngine.Mathf.RoundToInt(seal.basePrice * SoulFactor * rank * SoulShop.Inflation(run, b));
        }

        /// <summary>
        /// Oferty pieczęci na postoju: zawsze jedna forma dla posiadanego skilla (jeśli istnieje), wsparcia tylko przyjmowane
        /// przez któryś skill, budzące zespolenie/więź 3× częściej, posiadane jako kolejny poziom z połową wagi. Bez demonicznych.
        /// </summary>
        public static List<SealOffer> Offers(RunState run, Random rng, BalanceConfig b, int count = ShopOffers)
        {
            var offers = new List<SealOffer>();
            var candidates = HuCatalog.Seals.Where(s => s.IsItem && s.shop && !s.lootOnly && CanTake(run, s) && (run.OwnsSeal(s) || Usable(run, s))).ToList();
            if (candidates.Count == 0) return offers;
            double Weight(HuSeal s) => s.weight * (run.OwnsSeal(s) ? 0.5 : 1.0) * (Wakes(run, s) ? WakingWeight : 1.0);
            var forms = candidates.Where(s => s.IsForm && !run.OwnsSeal(s)).ToList();
            if (forms.Count > 0) Add(Pick(forms.Select(s => (s, Weight(s))).ToList(), rng));
            while (offers.Count < count)
            {
                var left = candidates.Where(s => offers.All(o => o.seal != s)).ToList();
                if (left.Count == 0) break;
                Add(Pick(left.Select(s => (s, Weight(s))).ToList(), rng));
            }
            return offers;

            void Add(HuSeal s) => offers.Add(new SealOffer { seal = s, price = Price(run, s, b), rankUp = run.OwnsSeal(s) });
        }

        public static int RerollPrice(RunState run, int rerollsThisVisit, BalanceConfig b) =>
            UnityEngine.Mathf.RoundToInt((RerollBase + RerollStep * rerollsThisVisit) * SoulShop.Inflation(run, b));

        /// <summary>Zakup atomowy: dusze znikają i pieczęć trafia do bohatera albo nic się nie zmienia.</summary>
        public static SealGain Buy(RunState run, SealOffer offer)
        {
            if (offer == null || offer.sold || run.souls < offer.price || !CanTake(run, offer.seal)) return SealGain.None;
            var gain = run.AddSeal(offer.seal);
            if (gain == SealGain.None) return gain;
            run.souls -= offer.price;
            offer.sold = true;
            return gain;
        }
    }
}
