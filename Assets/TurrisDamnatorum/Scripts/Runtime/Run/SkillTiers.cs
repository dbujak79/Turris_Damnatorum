using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Poziom startowy skilla zdobytego w trakcie podejścia (nagroda, sklep) – progi jak w Hell Unleashed: skill zdobyty
    /// od fali 7 przychodzi na poziomie II, od fali 16 na III (skille HU mają trzy poziomy). Skille Turris z pięcioma
    /// poziomami dostają proporcjonalnie +0, +2, +4. Piętro wieży = fala HU.
    /// </summary>
    public static class SkillTiers
    {
        /// <summary>Pierwsze piętro (1–30) z drugim i trzecim progiem.</summary>
        public const int SecondTierFloor = 7, ThirdTierFloor = 16;

        /// <summary>Próg (0–2) dla oferty po ukończeniu <paramref name="floorsCleared"/> pięter (oferta jest na następne piętro).</summary>
        public static int Tier(int floorsCleared)
        {
            int next = floorsCleared + 1;
            return next >= ThirdTierFloor ? 2 : next >= SecondTierFloor ? 1 : 0;
        }

        public static int StartLevel(SpellDefinition def, int floorsCleared)
        {
            if (def == null || def.maxLevel <= 0) return 0;
            return Mathf.Min(def.maxLevel, Mathf.RoundToInt(Tier(floorsCleared) * def.maxLevel / 2f));
        }
    }
}
