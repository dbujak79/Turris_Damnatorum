namespace Turris
{
    /// <summary>Rodzaj ataku liczony przez reżysera: wręcz, z dystansu albo specjalny (szarża, uczepienie, przyzwanie…).</summary>
    public enum AggressionKind { Melee, Ranged, Special }

    /// <summary>
    /// Reżyser ataków z Hell Unleashed (<c>EnemyAttackDirector</c>): ilu demonów naraz może być w zamachu albo w trakcie ciosu
    /// na bohatera – osobno wręcz, z dystansu i specjalnych. Liczebność hordy bez zmian; demon bez wolnego miejsca krąży dalej
    /// i ponawia próbę po 0,2–0,5 s. Miejsce trwa przez zapowiedź i fazę aktywną, zwalnia się w odzyskaniu, przy przerwaniu
    /// i przy śmierci. Bossowie się nie liczą. Prosty licznik (bez kolejki i priorytetów), jak w HU.
    /// </summary>
    public static class AttackDirector
    {
        public static bool Enabled = true;

        /// <summary>Limity (wręcz, dystans, specjalne) dla aktów po 10 pięter i dla sług obok bossa – wartości HU.</summary>
        static readonly int[,] Acts = { { 3, 1, 1 }, { 4, 2, 1 }, { 5, 2, 1 } };
        static readonly int[] BossServants = { 2, 1, 1 };
        public const int FloorsPerAct = 10;

        static readonly int[] active = new int[3];
        static int[] limits = { 3, 1, 1 };

        /// <summary>Nowe piętro: liczniki od zera, limity aktu (piętro 1–30) albo sług na piętrze z bossem.</summary>
        public static void Reset(int floor, bool bossFloor)
        {
            System.Array.Clear(active, 0, active.Length);
            if (bossFloor) { limits = (int[])BossServants.Clone(); return; }
            int act = System.Math.Min(2, (System.Math.Max(1, floor) - 1) / FloorsPerAct);
            limits = new[] { Acts[act, 0], Acts[act, 1], Acts[act, 2] };
        }

        public static int Limit(AggressionKind kind) => limits[(int)kind];
        public static int Active(AggressionKind kind) => active[(int)kind];

        public static bool TryAcquire(AggressionKind kind)
        {
            if (!Enabled) return true;
            if (active[(int)kind] >= limits[(int)kind]) return false;
            active[(int)kind]++;
            return true;
        }

        public static void Release(AggressionKind kind) => active[(int)kind] = System.Math.Max(0, active[(int)kind] - 1);
    }
}
