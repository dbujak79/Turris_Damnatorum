using System;
using System.Collections.Generic;
using System.Linq;

namespace Turris
{
    /// <summary>Dlaczego pieczęć pasuje albo nie pasuje do skilla.</summary>
    public enum SealFit { Fits, OtherSkill, Unsupported, GroupTaken, Duplicate, NoSocket, NotSettable }

    /// <summary>
    /// Skill po włożeniu pieczęci (port <c>SkillShape</c> z HU): komponenty, tagi, celowanie, żywioł i parametry poziomu
    /// po wszystkich operacjach. Budowany przez <see cref="Compose"/> i nigdy nie zmieniany.
    ///
    /// Kolejność: poziom, forma, wsparcia w kolejności gniazd, obudzone zespolenia, na końcu więzi. Wsparcie, którego
    /// wymagań skill w swojej kolejce już nie spełnia, jest pomijane i zgłaszane jako uśpione (szare, nie usuwane).
    ///
    /// Bezpieczniki balansu z HU: mnożniki obrażeń wsparć, zespoleń i więzi sumują się (×1,8 i ×1,4 daje +120%), z limitem
    /// +200% i podłogą −80%; tylko forma mnoży sama. Poziomy pieczęci ofensywnych dodają razem najwyżej +32%.
    /// </summary>
    public sealed class HuSkillShape
    {
        public const int MaxRank = 3;
        public const double RankBonus = 0.08, RankBonusCap = 0.32;
        public const double SupportBonusCap = 2.0, SupportBonusFloor = -0.8;
        public const double UtilityRankStrength = 0.125, UtilityRankStrengthCap = 0.25;

        HuSkillShape(List<string> components, HashSet<string> tags, HuTargeting targeting, HuElement element, HuParams parameters,
            List<HuSeal> active, List<HuSeal> dormant, List<HuSeal> bonds)
        {
            Components = components; Tags = tags; Targeting = targeting; Element = element; Parameters = parameters;
            Active = active; Dormant = dormant; Bonds = bonds ?? new List<HuSeal>();
        }

        public IReadOnlyList<string> Components { get; }
        public HashSet<string> Tags { get; }
        public HuTargeting Targeting { get; }
        public HuElement Element { get; }
        public HuParams Parameters { get; }
        /// <summary>Pieczęcie, które zadziałały (z zespoleniami), w kolejności nałożenia.</summary>
        public IReadOnlyList<HuSeal> Active { get; }
        /// <summary>Pieczęcie w gniazdach, których przerobiony skill nie obsługuje.</summary>
        public IReadOnlyList<HuSeal> Dormant { get; }
        /// <summary>Więzi z innym skillem w ręce, które zmieniły ten.</summary>
        public IReadOnlyList<HuSeal> Bonds { get; }

        public double Cooldown => Math.Max(0.0, Parameters.Get(HuParams.Cooldown, 5.0));
        public int MaxCharges => Math.Max(1, (int)Math.Round(Parameters.Get(HuParams.Charges, 1.0)));
        public bool HasComponent(string c) { foreach (var x in Components) if (string.Equals(x, c, StringComparison.OrdinalIgnoreCase)) return true; return false; }
        public bool IsActive(string sealId) { foreach (var s in Active) if (s.id == sealId) return true; return false; }

        /// <summary>Skill tak, jak definiują go dane na poziomie HU (1–3), bez pieczęci.</summary>
        public static HuSkillShape Plain(HuSkillData data, int level) =>
            new HuSkillShape(new List<string>(data.components), new HashSet<string>(data.tags, StringComparer.OrdinalIgnoreCase), data.targeting, data.element,
                data.At(level), new List<HuSeal>(), new List<HuSeal>(), null);

        /// <summary>Przerobiony skill: poziom, forma, wsparcia po kolei, zespolenia, które obudziły, i więzi.</summary>
        public static HuSkillShape Compose(HuSkillData data, int level, HuSeal form, IReadOnlyList<HuSeal> supports,
            IReadOnlyList<HuSeal> synergies = null, IReadOnlyList<KeyValuePair<HuSeal, bool>> bonds = null, Func<HuSeal, int> rank = null)
        {
            var b = new Builder(data, level);
            var active = new List<HuSeal>();
            var dormant = new List<HuSeal>();
            int RankOf(HuSeal s) => rank == null ? 1 : Math.Max(1, Math.Min(MaxRank, rank(s)));

            if (form != null)
            {
                if (form.skillId == data.id && b.Meets(form)) { b.Apply(form.ops, true, UtilityStrength(form, RankOf(form))); active.Add(form); }
                else dormant.Add(form);
            }
            if (supports != null)
                foreach (var s in supports)
                {
                    if (b.Meets(s)) { b.Apply(s.ops, false, UtilityStrength(s, RankOf(s))); active.Add(s); }
                    else dormant.Add(s);
                }
            if (synergies != null)
                foreach (var syn in synergies)
                {
                    if (syn.kind == SealKind.Synergy && syn.requiresSeals.Length > 0
                        && syn.requiresSeals.All(sid => active.Exists(a => a.id == sid))
                        && (string.IsNullOrEmpty(syn.skillId) || syn.skillId == data.id))
                    {
                        b.Apply(syn.ops, false);
                        active.Add(syn);
                    }
                }

            // Pieczęć ofensywna powyżej poziomu I wzmacnia skill, w którym działa: +8% za poziom, razem najwyżej +32%.
            double rankBonus = 0;
            foreach (var s in active) if (s.IsItem && IsOffensive(s)) rankBonus += RankBonus * (RankOf(s) - 1);

            // Więzi na końcu: oceniano je na skillu, jakim zrobiły go jego własne pieczęcie, i tylko do niego dokładają.
            var bonded = new List<HuSeal>();
            if (bonds != null)
                foreach (var kv in bonds) { b.Apply(kv.Value ? kv.Key.ops : kv.Key.partnerOps, false); bonded.Add(kv.Key); }

            b.FinishDamage(Math.Min(RankBonusCap, rankBonus));
            return b.Build(active, dormant, bonded);
        }

        /// <summary>
        /// Pieczęć dokładająca obrażeń: mnoży je w górę, dodaje część raniącą (echo, wachlarz, strefa) albo premię przeciw stanowi.
        /// Tylko takie biorą premię obrażeń swojego poziomu.
        /// </summary>
        public static bool IsOffensive(HuSeal seal) => seal.ops.Any(op =>
            (op.Kind == SealOpKind.Multiply && op.Key == SealOp.AllDamage && op.Value > 1.0)
            || ((op.Kind == SealOpKind.Set || op.Kind == SealOpKind.Add) && op.Key.StartsWith(HuParams.DamagePrefix, StringComparison.Ordinal))
            || ((op.Kind == SealOpKind.Set || op.Kind == SealOpKind.Add || op.Kind == SealOpKind.Multiply)
                && (op.Key.EndsWith("_share", StringComparison.Ordinal) || op.Key.StartsWith("bonus_vs_", StringComparison.Ordinal) || op.Key == "crescendo_step")));

        static double UtilityStrength(HuSeal seal, int rank) =>
            rank <= 1 || IsOffensive(seal) ? 1.0 : 1.0 + Math.Min(UtilityRankStrengthCap, UtilityRankStrength * (rank - 1));

        static readonly HashSet<string> UtilityKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            HuParams.Knockback, "area_scale", "cast_barrier", "cast_barrier_seconds", "cast_reduction", "cast_reduction_seconds",
            "implode_radius", "implode_pull", "impact_vortex_seconds", "impact_vortex_pull", "homing", "explosion_radius",
            "zone_seconds", "zone_radius", "trail_zone_seconds", "trail_zone_radius", "totem_seconds",
        };

        static SealOp Strengthened(SealOp op, double strength)
        {
            if (strength == 1.0 || !UtilityKeys.Contains(op.Key) || !(op.Kind == SealOpKind.Set || op.Kind == SealOpKind.Add || op.Kind == SealOpKind.Multiply))
                return op;
            bool ratio = op.Kind == SealOpKind.Multiply || op.Key == "area_scale";
            return op.WithValue(ratio ? 1.0 + (op.Value - 1.0) * strength : op.Value * strength);
        }

        /// <summary>Czy własne wymagania pieczęci są spełnione na tym kształcie (bez gniazd, grup i rodzaju).</summary>
        public bool Supports(HuSeal seal)
        {
            if (seal.requiresComponents.Length > 0 && !seal.requiresComponents.Any(HasComponent)) return false;
            if (seal.forbidsComponents.Any(HasComponent)) return false;
            foreach (var t in seal.requiresTags) if (!Tags.Contains(t)) return false;
            foreach (var t in seal.forbidsTags) if (Tags.Contains(t)) return false;
            if (seal.requiresTargeting.HasValue && seal.requiresTargeting.Value != Targeting) return false;
            if (seal.notOfElement.HasValue && OnlyOf(seal.notOfElement.Value)) return false;
            return true;
        }

        bool OnlyOf(HuElement type)
        {
            double total = Parameters.TotalDamage;
            return total <= 0.0 ? Element == type : Math.Abs(Parameters.Damage(type) - total) < 1e-9;
        }

        /// <summary>Efekt, który żywioł niesie sam z siebie; zmiana żywiołu wymienia jeden na drugi (powietrze nie ma – odrzuca).</summary>
        public static string ElementEffect(HuElement type)
        {
            switch (type)
            {
                case HuElement.Physical: return "Bleed";
                case HuElement.Fire: return "Ignite";
                case HuElement.Water: return "Soaked";
                case HuElement.Earth: return "Stagger";
                default: return null;
            }
        }

        static bool IsElementEffect(string kind) => kind == "Bleed" || kind == "Ignite" || kind == "Soaked" || kind == "Slow" || kind == "Stagger";

        public static string TagFor(HuElement e)
        {
            switch (e)
            {
                case HuElement.Fire: return "fire";
                case HuElement.Water: return "water";
                case HuElement.Air: return "air";
                case HuElement.Earth: return "earth";
                default: return "physical";
            }
        }

        /// <summary>Kopia robocza edytowana przez jedno złożenie.</summary>
        sealed class Builder
        {
            readonly HuSkillData data;
            readonly List<string> components;
            readonly HashSet<string> tags;
            readonly Dictionary<string, double> values;
            HuTargeting targeting;
            HuElement element;
            double supportBonus;

            public Builder(HuSkillData data, int level)
            {
                this.data = data;
                components = new List<string>(data.components);
                tags = new HashSet<string>(data.tags, StringComparer.OrdinalIgnoreCase);
                values = new Dictionary<string, double>(StringComparer.Ordinal);
                foreach (var kv in data.At(level).Values) values[kv.Key] = kv.Value;
                targeting = data.targeting;
                element = data.element;
            }

            public bool Meets(HuSeal seal) => Snapshot().Supports(seal);

            HuSkillShape Snapshot() => new HuSkillShape(new List<string>(components), new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase),
                targeting, element, new HuParams(values), new List<HuSeal>(), new List<HuSeal>(), null);

            /// <summary>Forma mnoży obrażenia od razu; mnożnik każdej innej pieczęci czeka i dodaje się do pozostałych.</summary>
            public void Apply(IReadOnlyList<SealOp> ops, bool isForm, double strength = 1.0)
            {
                foreach (var op in ops)
                {
                    if (!isForm && op.Kind == SealOpKind.Multiply && op.Key == SealOp.AllDamage) { supportBonus += op.Value - 1.0; continue; }
                    Apply(Strengthened(op, strength));
                }
            }

            public void FinishDamage(double rankBonus)
            {
                double factor = (1.0 + Math.Max(SupportBonusFloor, Math.Min(SupportBonusCap, supportBonus))) * (1.0 + Math.Max(0.0, rankBonus));
                if (Math.Abs(factor - 1.0) > 1e-12) Apply(new SealOp(SealOpKind.Multiply, SealOp.AllDamage, factor));
            }

            int IndexOf(string name) => components.FindIndex(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));

            void Apply(SealOp op)
            {
                switch (op.Kind)
                {
                    case SealOpKind.AddComponent:
                        if (IndexOf(op.Key) < 0) components.Add(op.Key);
                        break;
                    case SealOpKind.RemoveComponent:
                        components.RemoveAll(n => string.Equals(n, op.Key, StringComparison.OrdinalIgnoreCase));
                        break;
                    case SealOpKind.ReplaceComponent:
                    {
                        if (IndexOf(op.Key) < 0) break;
                        components.RemoveAll(n => string.Equals(n, op.Other, StringComparison.OrdinalIgnoreCase));
                        components[IndexOf(op.Key)] = op.Other;
                        break;
                    }
                    case SealOpKind.AddTag: tags.Add(op.Key); break;
                    case SealOpKind.RemoveTag: tags.Remove(op.Key); break;
                    case SealOpKind.Targeting:
                        if (Enum.TryParse(op.Other, true, out HuTargeting t)) targeting = t;
                        break;
                    case SealOpKind.Element:
                        if (Enum.TryParse(op.Other, true, out HuElement e)) ConvertTo(e);
                        break;
                    case SealOpKind.Set:
                        // Koszty w zdrowiu sumują się: pakt i ofiara razem kosztują oba, w dowolnej kolejności.
                        values[op.Key] = op.Key == HuParams.LifeCost ? Get(op.Key, 0) + op.Value : op.Value;
                        break;
                    case SealOpKind.Multiply:
                        if (op.Key == SealOp.AllDamage)
                        {
                            foreach (var k in values.Keys.Where(HuParams.IsDamageKey).ToList()) values[k] *= op.Value;
                        }
                        else if (values.TryGetValue(op.Key, out double cur)) values[op.Key] = cur * op.Value;
                        break;
                    case SealOpKind.Add:
                        values[op.Key] = Get(op.Key, HuParams.ImplicitValue(op.Key)) + op.Value;
                        break;
                }
            }

            double Get(string key, double fallback) => values.TryGetValue(key, out double v) ? v : fallback;

            /// <summary>Wszystkie obrażenia w jeden żywioł; warstwy efektów starych żywiołów przechodzą na efekt nowego.</summary>
            void ConvertTo(HuElement e)
            {
                double total = 0;
                foreach (var k in values.Keys.Where(HuParams.IsDamageKey).ToList()) { total += values[k]; values.Remove(k); }
                values[HuParams.DamageKey(e)] = total;

                int stacks = 0;
                foreach (var k in values.Keys.Where(k => k.StartsWith(HuParams.EffectPrefix, StringComparison.Ordinal)).ToList())
                {
                    if (IsElementEffect(k.Substring(HuParams.EffectPrefix.Length)))
                    {
                        stacks = Math.Max(stacks, (int)Math.Round(values[k]));
                        values.Remove(k);
                    }
                }
                if (stacks > 0)
                {
                    var effect = ElementEffect(e);
                    if (effect != null) values[HuParams.EffectPrefix + effect] = stacks;
                    else values[HuParams.Knockback] = Get(HuParams.Knockback, 0) + 1.0;
                }
                if (e != HuElement.Water) values.Remove("ice");
                foreach (HuElement x in Enum.GetValues(typeof(HuElement))) tags.Remove(TagFor(x));
                tags.Add(TagFor(e));
                element = e;
            }

            public HuSkillShape Build(List<HuSeal> active, List<HuSeal> dormant, List<HuSeal> bonds) =>
                new HuSkillShape(new List<string>(components), new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase), targeting, element,
                    new HuParams(values), active, dormant, bonds);

            public override string ToString() => data.id;
        }
    }
}
