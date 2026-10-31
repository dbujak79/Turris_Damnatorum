using System.Collections.Generic;
using System.Linq;

namespace Turris
{
    /// <summary>
    /// Księga obrażeń podejścia (HU <c>DamageLedger</c>): ile bohater zadał według źródła, ile przyjął i od kogo,
    /// ile ciosów zatrzymała obrona. Pokazywana na ekranie śmierci i zwycięstwa.
    /// </summary>
    public class DamageLedger
    {
        public enum Source { Weapon, Skills, Ailments, Reactions }

        readonly float[] dealt = new float[4];
        readonly Dictionary<string, float> takenBy = new Dictionary<string, float>();
        public float Taken { get; private set; }
        public int Dodged, Parried, Blocked;

        public float Dealt(Source s) => dealt[(int)s];
        public float TotalDealt => dealt.Sum();

        /// <summary>Źródło trafienia bohatera: reakcja, zwykły atak bronią (wręcz albo pocisk broni), reszta to skille.</summary>
        public static Source Classify(in HitData hit) =>
            hit.isReaction ? Source.Reactions
            : hit.fromMeleeWeapon || (hit.isProjectile && hit.weapon != WeaponModel.Auto && hit.weapon != WeaponModel.None) ? Source.Weapon
            : Source.Skills;

        public void RecordDealt(Source s, float amount) { if (amount > 0f) dealt[(int)s] += amount; }

        public void RecordTaken(string enemy, in HitResult r)
        {
            switch (r.outcome)
            {
                case HitOutcome.Dodged: Dodged++; return;
                case HitOutcome.Parried: Parried++; return;
                case HitOutcome.Blocked: Blocked++; break;
            }
            if (r.healthDamage <= 0f) return;
            Taken += r.healthDamage;
            string key = string.IsNullOrEmpty(enemy) ? "inne" : enemy;
            takenBy[key] = (takenBy.TryGetValue(key, out var v) ? v : 0f) + r.healthDamage;
        }

        /// <summary>Wrogowie, od których bohater przyjął najwięcej obrażeń.</summary>
        public IEnumerable<KeyValuePair<string, float>> WorstEnemies(int count) => takenBy.OrderByDescending(kv => kv.Value).Take(count);

        public string Summary()
        {
            float total = TotalDealt;
            string Pct(Source s) => total > 0f ? $"{Dealt(s) / total * 100f:0}%" : "0%";
            var sb = new System.Text.StringBuilder();
            sb.Append($"Zadane: {total:0} – broń {Pct(Source.Weapon)}, skille {Pct(Source.Skills)}, efekty {Pct(Source.Ailments)}, reakcje {Pct(Source.Reactions)}\n");
            sb.Append($"Przyjęte: {Taken:0} · uniki {Dodged}, parowania {Parried}, bloki {Blocked}");
            var worst = WorstEnemies(3).ToList();
            if (worst.Count > 0) sb.Append("\nNajgroźniejsi: ").Append(string.Join(", ", worst.Select(kv => $"{kv.Key} ({kv.Value:0})")));
            return sb.ToString();
        }
    }
}
