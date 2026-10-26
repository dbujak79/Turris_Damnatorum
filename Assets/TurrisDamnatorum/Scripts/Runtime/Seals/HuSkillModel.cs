using System;
using System.Collections.Generic;
using System.Globalization;

namespace Turris
{
    /// <summary>Żywioł skilla w zapisie Hell Unleashed (kolejność zgodna z HU: 0 fizyczny … 4 ziemia).</summary>
    public enum HuElement { Physical = 0, Fire = 1, Water = 2, Air = 3, Earth = 4 }

    /// <summary>Sposób celowania skilla HU: wokół siebie, w kierunku, na wskazany punkt.</summary>
    public enum HuTargeting { Self = 0, Direction = 1, Point = 2 }

    /// <summary>
    /// Nazwane parametry jednego poziomu skilla HU (np. <c>cooldown</c>, <c>damage.fire</c>, <c>effect.Ignite</c>).
    /// Same liczby – zachowanie czyta, czego potrzebuje, a poziom włącza cechę wartością powyżej zera.
    /// Niezmienne po utworzeniu: pieczęcie budują nową kopię (<see cref="HuSkillShape"/>).
    /// </summary>
    public sealed class HuParams
    {
        public static readonly HuParams Empty = new HuParams(new Dictionary<string, double>());

        public const string Cooldown = "cooldown", Charges = "charges", Range = "range", Knockback = "knockback";
        public const string DamagePrefix = "damage.", EffectPrefix = "effect.";
        public const string AuraFlatPrefix = "aura.flat.", AuraPercentPrefix = "aura.pct.";
        public const string LifeCost = "life_cost";

        static readonly string[] DamageKeys = { "damage.physical", "damage.fire", "damage.water", "damage.air", "damage.earth" };

        readonly Dictionary<string, double> values;

        public HuParams(IDictionary<string, double> source)
        {
            values = new Dictionary<string, double>(source, StringComparer.Ordinal);
        }

        /// <summary>Parametry z wpisów <c>klucz=liczba</c> (format katalogu).</summary>
        public static HuParams Parse(IEnumerable<string> entries)
        {
            var d = new Dictionary<string, double>(StringComparer.Ordinal);
            if (entries != null)
                foreach (var e in entries)
                {
                    int eq = e.IndexOf('=');
                    if (eq <= 0) continue;
                    if (double.TryParse(e.Substring(eq + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                        d[e.Substring(0, eq).Trim()] = v;
                }
            return new HuParams(d);
        }

        public IReadOnlyDictionary<string, double> Values => values;
        public double Get(string key, double fallback = 0.0) => values.TryGetValue(key, out double v) ? v : fallback;
        public float GetF(string key, float fallback = 0f) => values.TryGetValue(key, out double v) ? (float)v : fallback;
        public bool Has(string key) => values.ContainsKey(key);
        /// <summary>Cecha włączona wartością powyżej zera.</summary>
        public bool On(string key) => Get(key) > 0.0;

        /// <summary>Obrażenia w kolejności <see cref="HuElement"/>.</summary>
        public double Damage(HuElement e) => Get(DamageKeys[(int)e]);
        public double TotalDamage { get { double t = 0; foreach (var k in DamageKeys) t += Get(k); return t; } }

        public static bool IsDamageKey(string key) => key.StartsWith(DamagePrefix, StringComparison.Ordinal) && key.Length > DamagePrefix.Length;
        public static string DamageKey(HuElement e) => DamageKeys[(int)e];

        /// <summary>Efekty trafienia z wpisów <c>effect.Rodzaj=warstwy</c> (nazwy HU: Bleed, Ignite, Soaked, Slow, Stagger, Stun, Sunder).</summary>
        public IEnumerable<KeyValuePair<string, int>> Effects
        {
            get
            {
                foreach (var kv in values)
                    if (kv.Key.StartsWith(EffectPrefix, StringComparison.Ordinal) && kv.Value >= 1.0)
                        yield return new KeyValuePair<string, int>(kv.Key.Substring(EffectPrefix.Length), (int)Math.Round(kv.Value));
            }
        }

        /// <summary>
        /// Wartość parametru licznikowego, którego skill nie podaje: jeden pocisk, jeden ładunek, jeden impuls.
        /// Pieczęć dodająca do brakującego licznika dodaje do tej wartości („dwa pociski więcej” przy jednej kuli daje trzy).
        /// </summary>
        public static double ImplicitValue(string key)
        {
            switch (key)
            {
                case Charges: case "count": case "pulses": case "strikes": case "area_scale": return 1.0;
                default: return 0.0;
            }
        }
    }

    /// <summary>
    /// Skill z Hell Unleashed w postaci źródłowej: komponenty (Projectile, Explosion, Zone, Nova, Fall, Mine…), tagi,
    /// celowanie, żywioł i skumulowane parametry trzech poziomów. Jedno źródło prawdy dla wykonania i pieczęci;
    /// asset <see cref="SpellDefinition"/> wskazuje go przez <c>huSkillId</c> i dokłada koszt rzucenia, animację i barwę.
    /// </summary>
    public sealed class HuSkillData
    {
        public string id, targetId, name, desc, l2, l3, classId;
        public HuElement element;
        public HuTargeting targeting;
        public readonly HashSet<string> tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly List<string> components = new List<string>();
        public HuParams[] levels = Array.Empty<HuParams>();
        public int minWave = 1;
        public bool shop = true;

        public int MaxLevel => Math.Max(1, levels.Length);
        /// <summary>Parametry poziomu HU (1–3); Turris liczy poziomy od 0, więc wywołujący dodaje 1.</summary>
        public HuParams At(int level) => levels.Length == 0 ? HuParams.Empty : levels[Math.Max(1, Math.Min(level, levels.Length)) - 1];
        public bool HasComponent(string c) => components.Exists(x => string.Equals(x, c, StringComparison.OrdinalIgnoreCase));
        public bool IsSignature => tags.Contains("signature");
    }
}
