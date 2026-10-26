using System.Collections.Generic;
using System.Linq;

namespace Turris
{
    /// <summary>Co się stało z pieczęcią, która trafiła do bohatera.</summary>
    public enum SealGain { None, Inserted, Pouch, RankUp }

    /// <summary>
    /// Pieczęcie w podejściu (port części <c>RunState</c> z HU): sakwa, poziomy pieczęci (druga i trzecia kopia podnosi
    /// poziom), dary bossów i więzi między skillami w ręce. Więzi liczone są po każdej zmianie slotów, pieczęci i poziomów.
    /// </summary>
    public partial class RunState
    {
        /// <summary>Pieczęcie bez miejsca: czekają, aż gracz włoży je do skilla (między piętrami).</summary>
        public readonly List<HuSeal> sealPouch = new List<HuSeal>();
        readonly Dictionary<string, int> sealRanks = new Dictionary<string, int>();
        /// <summary>Dary bossów: po trzy pieczęcie do wyboru jednej (czekają do następnego postoju).</summary>
        public readonly List<List<HuSeal>> sealGifts = new List<List<HuSeal>>();
        /// <summary>Więzi, które działają teraz między skillami w ręce.</summary>
        public readonly List<HuSeal> activeBonds = new List<HuSeal>();
        /// <summary>Zespolenia i więzi obudzone w tym podejściu po raz pierwszy (zbiera je profil – kodeks).</summary>
        public readonly List<HuSeal> newlyDiscovered = new List<HuSeal>();
        readonly HashSet<string> wokenThisRun = new HashSet<string>();

        /// <summary>Id zespoleń i więzi obudzonych w tym podejściu (kodeks).</summary>
        public IEnumerable<string> WokenSeals => wokenThisRun;

        public int SealRank(HuSeal seal) => seal != null && sealRanks.TryGetValue(seal.id, out int r) ? r : 1;

        /// <summary>Skille, które przyjmują pieczęcie, w kolejności: sloty, skill klasy, reszta kolekcji.</summary>
        public IEnumerable<SpellInstance> SealOrder()
        {
            foreach (var s in skillSlots) if (s != null && s.seals != null) yield return s;
            if (signatureSkill != null && signatureSkill.seals != null) yield return signatureSkill;
            foreach (var s in knownSpells) if (s.seals != null && SlotOf(s) < 0) yield return s;
        }

        /// <summary>Skille w ręce (sloty i skill klasy) – tylko między nimi powstają więzi.</summary>
        public IEnumerable<SpellInstance> Armed()
        {
            foreach (var s in skillSlots) if (s != null && s.seals != null) yield return s;
            if (signatureSkill != null && signatureSkill.seals != null) yield return signatureSkill;
        }

        /// <summary>Podpina instancję skilla pod poziomy pieczęci podejścia.</summary>
        public void AttachSeals(SpellInstance s)
        {
            if (s?.seals == null) return;
            s.seals.Rank = SealRank;
        }

        public bool OwnsSeal(HuSeal seal) => sealPouch.Contains(seal) || SealOrder().Any(s => s.seals.All.Contains(seal));

        /// <summary>
        /// Pieczęć trafia do bohatera (zakup, łup, dar): kopia posiadanej podnosi jej poziom (do III); nowa wchodzi do pierwszego
        /// skilla, który ją przyjmie – albo do sakwy (<paramref name="toPouch"/>: łup w walce zawsze do sakwy, jak w HU).
        /// </summary>
        public SealGain AddSeal(HuSeal seal, bool toPouch = false)
        {
            if (seal == null || !seal.IsItem) return SealGain.None;
            if (OwnsSeal(seal))
            {
                int rank = SealRank(seal);
                if (rank >= HuSkillShape.MaxRank) return SealGain.None;
                sealRanks[seal.id] = rank + 1;
                foreach (var s in SealOrder()) s.seals.Invalidate();
                RefreshBonds();
                return SealGain.RankUp;
            }
            if (!toPouch)
                foreach (var s in SealOrder())
                    if (s.seals.TryInsert(seal, out var displaced))
                    {
                        if (displaced != null) sealPouch.Add(displaced);
                        RefreshBonds();
                        return SealGain.Inserted;
                    }
            sealPouch.Add(seal);
            return SealGain.Pouch;
        }

        /// <summary>Wkłada pieczęć z sakwy do skilla. Wyparta forma wraca do sakwy.</summary>
        public bool InsertSeal(SpellInstance skill, HuSeal seal)
        {
            if (skill?.seals == null || !sealPouch.Contains(seal)) return false;
            if (!skill.seals.TryInsert(seal, out var displaced)) return false;
            sealPouch.Remove(seal);
            if (displaced != null) sealPouch.Add(displaced);
            RefreshBonds();
            return true;
        }

        /// <summary>Wyjmuje pieczęć ze skilla do sakwy.</summary>
        public bool RemoveSeal(SpellInstance skill, HuSeal seal)
        {
            if (skill?.seals == null || !skill.seals.Remove(seal)) return false;
            sealPouch.Add(seal);
            RefreshBonds();
            return true;
        }

        /// <summary>Skill odchodzi (przedmiot zdjęty): jego pieczęcie zostają w sakwie.</summary>
        void ReturnSealsOf(SpellInstance s)
        {
            if (s?.seals == null) return;
            sealPouch.AddRange(s.seals.RemoveAll());
        }

        /// <summary>
        /// Więzi między skillami w ręce, na kształtach bez więzi. Każda strona dostaje swoją zmianę; skill poza ręką traci więzi.
        /// Zgłasza też pierwsze obudzenie zespolenia albo więzi w podejściu (kodeks).
        /// </summary>
        public void RefreshBonds()
        {
            var armed = Armed().Distinct().ToList();
            var shapes = armed.Select(s => s.seals.Unbonded).ToList();
            var matches = HuBondRules.Holding(HuCatalog.Bonds, shapes);
            var perSkill = armed.ToDictionary(s => s, s => new List<KeyValuePair<HuSeal, bool>>());
            activeBonds.Clear();
            foreach (var m in matches)
            {
                perSkill[armed[m.first]].Add(new KeyValuePair<HuSeal, bool>(m.bond, true));
                perSkill[armed[m.second]].Add(new KeyValuePair<HuSeal, bool>(m.bond, false));
                activeBonds.Add(m.bond);
                Discover(m.bond);
            }
            foreach (var s in SealOrder()) s.seals.SetBonds(perSkill.TryGetValue(s, out var list) ? list : null);
            foreach (var s in armed) foreach (var a in s.seals.Shape.Active) if (a.kind == SealKind.Synergy) Discover(a);
        }

        void Discover(HuSeal s)
        {
            if (wokenThisRun.Add(s.id)) newlyDiscovered.Add(s);
        }

        /// <summary>Cena sprzedaży z sakwy: 35% ceny zwykłej pieczęci, stała kwota za demoniczną.</summary>
        public static int SealSalePrice(HuSeal s) => s.lootOnly ? HuSealEconomy.DemonicSaleValue : UnityEngine.Mathf.RoundToInt(s.basePrice * HuSealEconomy.SoulFactor * HuSealEconomy.SaleShare);

        public bool SellSeal(HuSeal seal)
        {
            if (!sealPouch.Remove(seal)) return false;
            souls += SealSalePrice(seal);
            return true;
        }
    }
}
