using System;
using System.Collections.Generic;
using System.Globalization;

namespace Turris
{
    /// <summary>Rodzaj pieczęci (wartości zgodne z HU).</summary>
    public enum SealKind
    {
        /// <summary>Przeróbka jednego, nazwanego skilla (kula, która spada jako kometa). Jedna na skill, nie zajmuje gniazda.</summary>
        Form = 0,
        /// <summary>Wsparcie pasujące do każdego skilla spełniającego wymagania; jedno na gniazdo.</summary>
        Support = 1,
        /// <summary>Zespolenie: budzi się samo, gdy wszystkie nazwane pieczęcie działają w jednym skillu. Nie w sprzedaży.</summary>
        Synergy = 2,
        /// <summary>Więź między dwoma skillami w ręce (sloty i skill klasy). Nie w sprzedaży ani w łupach.</summary>
        Bond = 3,
    }

    public enum SealOpKind { AddComponent, RemoveComponent, ReplaceComponent, AddTag, RemoveTag, Targeting, Element, Set, Multiply, Add }

    /// <summary>Jedna operacja pieczęci: <c>+Zone</c>, <c>Projectile&gt;Fall</c>, <c>-#projectile</c>, <c>@targeting=Point</c>,
    /// <c>@element=Water</c>, <c>pierce=99</c>, <c>damage*=0.7</c>, <c>count+=2</c>.</summary>
    public readonly struct SealOp
    {
        /// <summary>Pseudo-klucz oznaczający wszystkie wpisy <c>damage.*</c> w mnożeniu.</summary>
        public const string AllDamage = "damage";

        public readonly SealOpKind Kind;
        public readonly string Key;
        public readonly double Value;
        public readonly string Other;

        public SealOp(SealOpKind kind, string key, double value = 0.0, string other = "")
        {
            Kind = kind; Key = key; Value = value; Other = other ?? "";
        }

        public SealOp WithValue(double v) => new SealOp(Kind, Key, v, Other);

        public override string ToString() => $"{Kind} {Key} {Value} {Other}";

        static readonly string[] HuStatuses = { "Bleed", "Ignite", "Soaked", "Slow", "Stagger", "Stun", "Sunder" };

        /// <summary>Parser zgodny z <c>ContentSyntax.TryParseSealOp</c> w HU.</summary>
        public static bool TryParse(string raw, out SealOp op, out string error)
        {
            op = default; error = null;
            string entry = (raw ?? "").Trim();
            if (entry.Length < 2) return false;

            if (entry[0] == '@')
            {
                int eq = entry.IndexOf('=');
                string name = eq > 0 ? entry.Substring(1, eq - 1).Trim() : "";
                string value = eq > 0 ? entry.Substring(eq + 1).Trim() : "";
                if (name == "targeting" && Enum.TryParse(value, true, out HuTargeting _)) { op = new SealOp(SealOpKind.Targeting, name, 0, value); return true; }
                if (name == "element" && Enum.TryParse(value, true, out HuElement el) && Enum.IsDefined(typeof(HuElement), el)) { op = new SealOp(SealOpKind.Element, name, 0, value); return true; }
                error = $"nieznany przełącznik w '{entry}'";
                return false;
            }

            if ((entry[0] == '+' || entry[0] == '-') && entry.IndexOf('=') < 0)
            {
                bool add = entry[0] == '+';
                string name = entry.Substring(1).Trim();
                if (name.StartsWith("#", StringComparison.Ordinal))
                {
                    op = new SealOp(add ? SealOpKind.AddTag : SealOpKind.RemoveTag, name.Substring(1).Trim());
                    return name.Length > 1;
                }
                op = new SealOp(add ? SealOpKind.AddComponent : SealOpKind.RemoveComponent, name);
                return name.Length > 0;
            }

            int arrow = entry.IndexOf('>');
            if (arrow > 0 && entry.IndexOf('=') < 0)
            {
                op = new SealOp(SealOpKind.ReplaceComponent, entry.Substring(0, arrow).Trim(), 0, entry.Substring(arrow + 1).Trim());
                return op.Key.Length > 0 && op.Other.Length > 0;
            }

            int equals = entry.IndexOf('=');
            if (equals <= 0) return false;
            char before = entry[equals - 1];
            var kind = before == '*' ? SealOpKind.Multiply : before == '+' ? SealOpKind.Add : SealOpKind.Set;
            string key = entry.Substring(0, kind == SealOpKind.Set ? equals : equals - 1).Trim();
            if (key.Length == 0 || !double.TryParse(entry.Substring(equals + 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            {
                error = $"'{entry}' musi mieć postać klucz=liczba, klucz*=liczba albo klucz+=liczba";
                return false;
            }
            if (key.StartsWith(HuParams.EffectPrefix, StringComparison.Ordinal) && Array.IndexOf(HuStatuses, key.Substring(HuParams.EffectPrefix.Length)) < 0)
            {
                error = $"nieznany efekt w '{entry}'";
                return false;
            }
            op = new SealOp(kind, key, number);
            return true;
        }

        public static List<SealOp> ParseAll(IEnumerable<string> entries, List<string> errors = null)
        {
            var ops = new List<SealOp>();
            if (entries == null) return ops;
            foreach (var e in entries)
            {
                if (TryParse(e, out var op, out var err)) ops.Add(op);
                else errors?.Add(err ?? $"'{e}' nie jest operacją pieczęci");
            }
            return ops;
        }
    }

    /// <summary>
    /// Pieczęć skilla (port <c>SkillSealData</c> z HU). Same dane: komponenty dodane, usunięte albo zamienione, parametry
    /// ustawione lub przeskalowane, tagi i celowanie zmienione – przerobiony skill działa na tym samym kodzie komponentów.
    /// Wymagania sprawdza się na skillu takim, jakim zrobiły go wcześniejsze pieczęcie.
    /// </summary>
    public sealed class HuSeal
    {
        public string id, name, desc, skillId = "", classId = "", group = "";
        public SealKind kind = SealKind.Support;
        public string[] requiresComponents = Array.Empty<string>(), forbidsComponents = Array.Empty<string>();
        public readonly HashSet<string> requiresTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<string> forbidsTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HuTargeting? requiresTargeting;
        public HuElement? notOfElement;
        public string[] requiresSeals = Array.Empty<string>();
        public List<SealOp> ops = new List<SealOp>(), partnerOps = new List<SealOp>();
        /// <summary>Pieczęć demoniczna: nigdy w ofercie, tylko z elit i bossów; dziksza i droższa.</summary>
        public bool lootOnly;
        public float basePrice = 45f, weight = 1f;
        public bool shop = true;

        public bool IsForm => kind == SealKind.Form;
        /// <summary>Pieczęć, którą bohater może trzymać: forma albo wsparcie (zespolenia i więzi budzą się same).</summary>
        public bool IsItem => kind == SealKind.Form || kind == SealKind.Support;

        /// <summary>Czy jeden warunek więzi zachodzi dla skilla: id pieczęci, <c>#tag</c> albo <c>@Komponent</c>.</summary>
        public static bool BondConditionHolds(string condition, HuSkillShape shape) =>
            condition.StartsWith("#", StringComparison.Ordinal) ? shape.Tags.Contains(condition.Substring(1))
            : condition.StartsWith("@", StringComparison.Ordinal) ? shape.HasComponent(condition.Substring(1))
            : shape.IsActive(condition);

        public override string ToString() => id;
    }
}
