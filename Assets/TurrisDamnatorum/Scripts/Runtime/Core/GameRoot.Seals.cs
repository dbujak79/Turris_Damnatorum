using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Pieczęcie w przebiegu wieży: łup z wrogów (do sakwy), dar bossa (trzy do wyboru), oferty w sklepie dusz na postoju,
    /// przelosowanie, wkładanie, wyjmowanie i sprzedaż. Pieczęcie zmienia się tylko poza walką (postój, ekwipunek).
    /// </summary>
    public partial class GameRoot
    {
        /// <summary>Oferty pieczęci w sklepie dusz na tym postoju.</summary>
        public List<SealOffer> SealOffers { get; private set; } = new List<SealOffer>();
        int sealRerolls;

        public int SealRerollCost => HuSealEconomy.RerollPrice(Run, sealRerolls, config.balance);

        void RollSealOffers()
        {
            SealOffers = HuSealEconomy.Offers(Run, rng, config.balance);
            sealRerolls = 0;
        }

        /// <summary>Wróg zabity w wieży: może upuścić pieczęć (zwykły rzadko, elita często); boss daje dar.</summary>
        void SealDrop(EnemyBrain e)
        {
            if (Run == null || IsEnemyTrial || e == null || e.Def == null || e.Def.isMinion) return;
            if (e.Def.isBoss)
            {
                var gift = HuSealEconomy.RollGift(Run, rng);
                if (gift.Count == 0) return;
                Run.sealGifts.Add(gift);
                CombatEvents.RaiseMessage("DAR BOSSA: trzy pieczęcie do wyboru na postoju", new Color(1f, 0.82f, 0.35f));
                HellFx.Shockwave(e.transform.position, HellFxElement.Soul, new Color(1f, 0.82f, 0.35f), 2.2f);
                return;
            }
            var seal = HuSealEconomy.Roll(Run, e.IsElite ? SealSource.Elite : SealSource.Normal, rng);
            if (seal == null) return;
            var gain = Run.AddSeal(seal, toPouch: true);
            if (gain == SealGain.None) return;
            Color c = seal.lootOnly ? new Color(0.95f, 0.15f, 0.12f) : new Color(0.7f, 0.45f, 1f);
            string what = gain == SealGain.RankUp ? $"poziom {Names.Roman(Run.SealRank(seal))}" : "do sakwy";
            CombatEvents.RaiseWorldText(e.AimPoint + Vector3.up, seal.lootOnly ? "PIECZĘĆ DEMONICZNA!" : "PIECZĘĆ", c);
            CombatEvents.RaiseMessage($"Pieczęć: {seal.name} ({what})", c);
            HellFx.Burst(e.transform.position, HellFxElement.Soul, 1.2f);
            if (gain == SealGain.RankUp) Player.RefreshBuild();
        }

        public void BuySealOffer(SealOffer offer)
        {
            var gain = HuSealEconomy.Buy(Run, offer);
            if (gain == SealGain.None) return;
            Sfx.Ui(SoundId.UiBuy);
            CombatEvents.RaiseMessage(gain == SealGain.RankUp ? $"{offer.seal.name}: poziom {Names.Roman(Run.SealRank(offer.seal))}"
                : gain == SealGain.Inserted ? $"{offer.seal.name} – w skillu" : $"{offer.seal.name} – w sakwie", new Color(0.7f, 0.45f, 1f));
            Player.RefreshBuild();
        }

        public void RerollSealOffers()
        {
            int cost = SealRerollCost;
            if (Run.souls < cost) return;
            Run.souls -= cost;
            int used = sealRerolls + 1;
            SealOffers = HuSealEconomy.Offers(Run, rng, config.balance);
            sealRerolls = used;
            Sfx.Ui(SoundId.UiBuy);
        }

        /// <summary>Wybór z daru bossa: wybrana pieczęć trafia jak kupiona, pozostałe przepadają.</summary>
        public void TakeGift(int gift, int choice)
        {
            if (gift < 0 || gift >= Run.sealGifts.Count) return;
            var options = Run.sealGifts[gift];
            if (choice < 0 || choice >= options.Count) return;
            var seal = options[choice];
            Run.sealGifts.RemoveAt(gift);
            if (Run.AddSeal(seal) != SealGain.None) { Sfx.Ui(SoundId.UiReward); CombatEvents.RaiseMessage($"Dar: {seal.name}", new Color(1f, 0.82f, 0.35f)); }
            Player.RefreshBuild();
        }

        /// <summary>Dar z opcjami, które w międzyczasie bohater zdobył, traci je; pusty znika.</summary>
        public void PruneGifts()
        {
            foreach (var g in Run.sealGifts) g.RemoveAll(s => !HuSealEconomy.CanTake(Run, s));
            Run.sealGifts.RemoveAll(g => g.Count == 0);
        }

        public void InsertSeal(SpellInstance skill, HuSeal seal) { if (Run.InsertSeal(skill, seal)) { Sfx.Ui(SoundId.UiBuy); Player.RefreshBuild(); } }
        public void RemoveSeal(SpellInstance skill, HuSeal seal) { if (Run.RemoveSeal(skill, seal)) { Sfx.Ui(SoundId.UiBuy); Player.RefreshBuild(); } }
        public void SellSeal(HuSeal seal) { if (Run.SellSeal(seal)) Sfx.Ui(SoundId.UiBuy); }
    }
}
