using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Matematyka affiksów specjalizacyjnych (2026-11-01) – jedno źródło prawdy dla gry, tooltipów i testów, bez silnika.
    /// Wartości wejściowe to sumy statystyk po jakości HU (jakość skaluje premie raz, przy agregowaniu wyposażenia).
    /// </summary>
    public static class AffixMath
    {
        /// <summary>Limit nowej części redukcji kosztu bloku tarczą (affiksy); stare perki mają własne zasady.</summary>
        public const float ShieldBlockCap = 0.5f;

        /// <summary>Dodany składnik żywiołu: B × suma dodanych procentów / 100 (B – baza ataku broni przed premiami).</summary>
        public static float AddedElement(float weaponBase, float addedPercent) => weaponBase > 0f && addedPercent > 0f ? weaponBase * addedPercent / 100f : 0f;

        /// <summary>Mnożnik odporności żywiołu po przebiciu: odporność spada o przebicie, najwyżej do 0; podatność bez zmian.</summary>
        public static float ResistMultiplier(float baseMultiplier, float penetration)
            => baseMultiplier < 1f && penetration > 0f ? Mathf.Min(1f, baseMultiplier + penetration) : baseMultiplier;

        /// <summary>Mnożnik kosztu aktywnego bloku tarczą z affiksów: 1 − clamp(A/100, 0, 0,5).</summary>
        public static float ShieldBlockFactor(float affixPercent) => 1f - Mathf.Clamp(affixPercent / 100f, 0f, ShieldBlockCap);

        /// <summary>Czy suma affiksów bloku przekracza limit (UI pokazuje, że część premii nie działa).</summary>
        public static bool ShieldBlockCapped(float affixPercent) => affixPercent / 100f > ShieldBlockCap + 1e-4f;

        /// <summary>Czas przygotowania czaru: czas / (1 + C/100); czas zero zostaje zerem.</summary>
        public static float CastTime(float time, float castSpeedPercent)
            => time <= 0f ? 0f : time / Mathf.Max(0.01f, 1f + Mathf.Max(0f, castSpeedPercent) / 100f);

        /// <summary>Mnożnik postawy ciężkiej broni: 1 + suma premii / 100 (sumowanie addytywne, jeden mnożnik).</summary>
        public static float PoiseMultiplier(float heavyPoisePercent) => 1f + Mathf.Max(0f, heavyPoisePercent) / 100f;

        /// <summary>Mnożnik obrażeń sług (migawka przy przyzwaniu).</summary>
        public static float MinionMultiplier(float minionPercent) => 1f + Mathf.Max(0f, minionPercent) / 100f;
    }
}
