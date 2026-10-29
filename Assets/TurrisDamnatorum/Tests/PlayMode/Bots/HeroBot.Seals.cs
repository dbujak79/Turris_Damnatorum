using System.Collections.Generic;
using System.Linq;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Pieczęcie bota między piętrami: dar bossa, sakwa → skille i sklep pieczęci. Gracz bierze to, co pasuje do skilli
    /// w slotach (najpierw forma najczęściej używanego skilla, potem wsparcie); tester wybiera dar po kolei i kupuje każdą
    /// pieczęć, którą da się gdzieś włożyć – sprawdza pokrycie pieczęci.
    /// </summary>
    public abstract partial class HeroBot
    {
        /// <summary>Skille, do których bot wkłada pieczęcie: sloty od najczęściej używanego, skill klasy na końcu.</summary>
        IEnumerable<SpellInstance> SealTargets()
        {
            var run = Root.Run;
            var slots = run.skillSlots.Where(s => s?.seals != null)
                .OrderByDescending(s => Rec.R.skillUses.Get(s.definition.displayName) + Observed(s.definition.displayName).uses);
            return run.signatureSkill?.seals != null ? slots.Append(run.signatureSkill) : slots;
        }

        /// <summary>
        /// Czy pieczęć można włożyć do skilla bez wymiany: forma tylko do skilla bez formy (gra zamieniłaby formy, a stara
        /// wróciłaby do sakwy – bot przekładałby je tam i z powrotem co piętro), wsparcie do wolnego gniazda.
        /// </summary>
        static bool FitsFree(SpellInstance skill, HuSeal seal) =>
            skill?.seals != null && skill.seals.Fit(seal) == SealFit.Fits && (!seal.IsForm || skill.seals.Form == null);

        /// <summary>Wartość pieczęci dla bota: forma do skilla w slocie 3 (+1 dla najczęściej używanego), wsparcie 2, kolejny poziom posiadanej 1,5.</summary>
        float SealValue(HuSeal seal)
        {
            if (seal == null) return 0f;
            if (Root.Run.OwnsSeal(seal)) return 1.5f;
            var targets = SealTargets().ToList();
            for (int i = 0; i < targets.Count; i++)
                if (FitsFree(targets[i], seal))
                    return seal.IsForm ? (i == 0 ? 4f : 3f) : 2f;
            return 0f;
        }

        protected void ManageSeals()
        {
            var run = Root.Run;
            bool tester = P.persona == BotPersona.Tester;
            // 1. Dar bossa: jedna z trzech, reszta przepada.
            int guard = 0;
            while (run.sealGifts.Count > 0 && guard++ < 8)
            {
                var options = run.sealGifts[0];
                int pick = tester ? (Rec.R.run + run.floorIndex) % options.Count
                    : Enumerable.Range(0, options.Count).OrderByDescending(i => SealValue(options[i])).First();
                var seal = options[pick];
                Root.TakeGift(0, pick);
                Rec.R.seals.Add("dar: " + seal.name);
            }
            // 2. Sakwa: pieczęć do pierwszego skilla, który ją przyjmie.
            foreach (var seal in run.sealPouch.ToList())
            {
                var target = SealTargets().FirstOrDefault(s => FitsFree(s, seal));
                if (target == null) continue;
                Root.InsertSeal(target, seal);
                if (!run.sealPouch.Contains(seal)) Rec.R.seals.Add("z sakwy: " + seal.name);
            }
            // 3. Sklep pieczęci: najwartościowsze najpierw, póki starcza dusz.
            foreach (var offer in Root.SealOffers.Where(o => !o.sold).OrderByDescending(o => SealValue(o.seal)).ToList())
            {
                if (run.souls < offer.price || SealValue(offer.seal) <= 0f) continue;
                if (!tester && SealValue(offer.seal) < 2f) continue; // gracz nie płaci za sam kolejny poziom
                Root.BuySealOffer(offer);
                if (offer.sold || run.OwnsSeal(offer.seal)) { Rec.R.seals.Add("kupiona: " + offer.seal.name); Rec.R.purchases.Add("pieczęć: " + offer.seal.name); }
            }
            Pc.RefreshBuild();
        }
    }
}
