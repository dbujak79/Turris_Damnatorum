using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Dojrzewanie dusz z Hell Unleashed (<c>SoulAbsorptionSystem</c>): dusze z zabitego wroga dojrzewają 3 s, zanim trafią do
    /// sakiewki. Bezpośredni cios, który zabrał życie, niszczy 10% każdej dojrzewającej porcji (najwyżej raz na sekundę) –
    /// tyknięcia efektów, unik, parowanie, blok i osłona nic nie zabierają. Koniec piętra dojrzewa wszystko bez kary; śmierć – przepadają.
    /// </summary>
    public partial class GameRoot
    {
        public const float SoulAbsorptionSeconds = 3f, SoulLossShare = 0.1f, SoulLossCooldown = 1f;

        struct SoulBatch { public float amount, matureAt; }
        readonly List<SoulBatch> pendingSouls = new List<SoulBatch>();
        float soulLossReadyAt, soulClock;

        /// <summary>Dusze w trakcie dojrzewania (zaokrąglone w dół).</summary>
        public int PendingSouls => Mathf.FloorToInt(pendingSouls.Sum(b => b.amount));

        void AddPendingSouls(float amount)
        {
            if (amount <= 0f) return;
            pendingSouls.Add(new SoulBatch { amount = amount, matureAt = soulClock + SoulAbsorptionSeconds });
        }

        void TickSouls(float dt)
        {
            soulClock += dt;
            for (int i = pendingSouls.Count - 1; i >= 0; i--)
            {
                if (pendingSouls[i].matureAt > soulClock) continue;
                BankSouls(pendingSouls[i].amount);
                pendingSouls.RemoveAt(i);
            }
        }

        float soulRemainder;

        void BankSouls(float amount)
        {
            soulRemainder += amount;
            int whole = Mathf.FloorToInt(soulRemainder + 1e-4f);
            if (whole <= 0 || Run == null) return;
            Run.souls += whole;
            soulRemainder -= whole;
        }

        /// <summary>Koniec piętra: wszystkie porcje dojrzewają od razu.</summary>
        void MatureAllSouls()
        {
            foreach (var b in pendingSouls) BankSouls(b.amount);
            pendingSouls.Clear();
            if (soulRemainder > 0.5f && Run != null) Run.souls++;
            soulRemainder = 0f;
        }

        /// <summary>Nowe piętro albo nowe podejście: niedojrzałe dusze przepadają.</summary>
        void ResetSouls()
        {
            pendingSouls.Clear();
            soulRemainder = 0f;
            soulLossReadyAt = 0f;
        }

        /// <summary>Bohater stracił życie od bezpośredniego ciosu: każda dojrzewająca porcja traci 10%.</summary>
        void OnPlayerHealthHit(float damage)
        {
            if (damage <= 0f || pendingSouls.Count == 0 || soulClock < soulLossReadyAt) return;
            soulLossReadyAt = soulClock + SoulLossCooldown;
            float lost = 0f;
            for (int i = 0; i < pendingSouls.Count; i++)
            {
                var b = pendingSouls[i];
                lost += b.amount * SoulLossShare;
                b.amount *= 1f - SoulLossShare;
                pendingSouls[i] = b;
            }
            if (lost >= 1f && Player != null)
                CombatEvents.RaiseWorldText(Player.AimPoint + Vector3.up * 0.9f, $"−{lost:0} dusz", new Color(0.75f, 0.6f, 1f));
        }
    }
}
