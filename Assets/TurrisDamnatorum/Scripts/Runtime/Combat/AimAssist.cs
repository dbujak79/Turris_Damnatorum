using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Pomoc w celowaniu przeniesiona z Hell Unleashed (<c>HellUnleashed.Core.Combat.AimAssist</c>): wybiera wroga,
    /// na którego cios albo skill ma się „przyciągnąć”.
    ///
    /// Reguła szanuje kierunek gracza zamiast go zastępować: kandydat liczy się tylko wtedy, gdy leży w stożku wokół
    /// kierunku celowania, a wygrywa najbliższy z nich. Pomoc wybacza kilka stopni błędu, nie celuje za gracza.
    /// Brak kierunku (gałka/klawisze w spoczynku) to ta sama reguła z otwartym stożkiem: najbliższy wróg w dowolną stronę.
    /// Remisy: mniejszy kąt, potem kolejność na liście – ta sama sytuacja zawsze daje ten sam wynik (bez migotania celu).
    /// </summary>
    public static class AimAssist
    {
        /// <summary>Półszerokość stożka przyciągania ciosu, gdy gracz wskazuje kierunek (HU: ExplicitAimConeDegrees).</summary>
        public const float ExplicitConeDegrees = 28f;
        /// <summary>Półszerokość stożka, w którym pomoc może zagiąć skill ze wskazanym kierunkiem (HU: SkillAssistConeDegrees).</summary>
        public const float SkillConeDegrees = 35f;
        /// <summary>Zasięg szukania celu dla skilla bez wskazanego kierunku (HU: SkillFacingRange).</summary>
        public const float SkillRange = 12f;
        /// <summary>Cel ciosu można złapać nieco dalej niż zasięg broni (HU: AssistRangeMargin, MinimumAssistRange).</summary>
        public const float RangeMargin = 1.5f, MinimumRange = 5f;

        /// <summary>Zasięg pomocy dla ciosu bronią o danym zasięgu.</summary>
        public static float MeleeRange(float weaponReach) => Mathf.Max(MinimumRange, weaponReach + RangeMargin);

        /// <summary>
        /// Wybiera cel albo zwraca null, gdy żaden się nie kwalifikuje i zostaje kierunek gracza.
        /// </summary>
        /// <param name="aim">Kierunek celowania na płaszczyźnie (nie musi być znormalizowany; zero = brak kierunku).</param>
        /// <param name="maxAngleDegrees">Półszerokość stożka; 180 przyjmuje każdy kierunek.</param>
        public static EnemyBrain Select(Vector3 origin, Vector3 aim, IReadOnlyList<EnemyBrain> candidates, float maxRange, float maxAngleDegrees)
        {
            if (candidates == null || candidates.Count == 0 || maxRange <= 0f) return null;
            aim.y = 0f;
            bool hasAim = aim.sqrMagnitude > 1e-6f;
            float angleLimit = hasAim ? Mathf.Clamp(maxAngleDegrees, 0f, 180f) : 180f;
            float minDot = Mathf.Cos(angleLimit * Mathf.Deg2Rad);
            Vector3 n = hasAim ? aim.normalized : Vector3.zero;
            float rangeSq = maxRange * maxRange;

            EnemyBrain best = null;
            float bestSq = 0f, bestDot = 0f;
            for (int i = 0; i < candidates.Count; i++)
            {
                var e = candidates[i];
                if (e == null || e.IsDead || !e.isActiveAndEnabled) continue;
                Vector3 off = e.transform.position - origin; off.y = 0f;
                float sq = off.sqrMagnitude;
                if (sq > rangeSq) continue;
                // Wróg dokładnie na atakującym nie ma kierunku – jest „przed nim” bardziej niż cokolwiek.
                float dot = 1f;
                if (sq > 1e-8f && angleLimit < 180f)
                {
                    dot = Vector3.Dot(off, n) / Mathf.Sqrt(sq);
                    if (dot < minDot) continue;
                }
                const float eps = 1e-6f;
                bool better = best == null || sq < bestSq - eps || (Mathf.Abs(sq - bestSq) <= eps && dot > bestDot + eps);
                if (better) { best = e; bestSq = sq; bestDot = dot; }
            }
            return best;
        }
    }
}
