using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Natywne perki specjalizacyjne (docs/PROMPT_WDROZENIE_PERKOW_SPECJALIZACYJNYCH.md) w walce gracza:
    /// Pojedynkowicz – dodatek do sumy tempa kwalifikowanego ataku (StartAttack); Wykorzystaj wyłom – okno per cel po rzeczywistym
    /// przełamaniu postawy (zdarzenie <see cref="EnemyBrain.PoiseBroke"/>), premia przed obroną celu w OutgoingAgainst, zużycie po
    /// trafieniu z obrażeniami życia (OnHitResolved), riposta w PerformRiposte; Różnorodna sfora – mnożnik w chwili ataku sługi
    /// (<see cref="Servant"/>); Gwałtowna manifestacja – lokalny zegar kwalifikowanej strefy (<see cref="DamageZone"/>).
    /// </summary>
    public partial class PlayerCombat
    {
        /// <summary>Liczniki perków (telemetria, testy).</summary>
        public readonly Dictionary<string, float> PerkTelemetry = new Dictionary<string, float>();
        void PerkCount(string key, float v = 1f) { PerkTelemetry.TryGetValue(key, out var x); PerkTelemetry[key] = x + v; }

        NativePerkTuning PerkT => NativePerkCatalog.Tuning(Config);

        /// <summary>Gracz ma perk i build pozwala go wykorzystać (uśpiony perk nic nie robi).</summary>
        public bool PerkActive(string id)
        {
            if (Run == null) return false;
            foreach (var b in Run.boons) if (b.definition != null && b.definition.id == id && b.stacks > 0) return NativePerkCatalog.Usable(id, Run);
            return false;
        }

        // ================================================================== Pojedynkowicz

        /// <summary>Dodatek tempa dla zwykłego lekkiego/mocnego ataku główną jednoręczną bronią wręcz przy pustej drugiej ręce.</summary>
        public float DuelistBonus(ActionType type, AttackDefinition atk, bool offhand)
        {
            if ((type != ActionType.LightAttack && type != ActionType.HeavyAttack) || offhand || WeaponThrown || atk == null || atk.delivery != AttackDelivery.Melee) return 0f;
            if (!PerkActive(NativePerkCatalog.Duelist)) return 0f;
            PerkCount("pojedynkowicz: ataki");
            return PerkT.duelistSpeed;
        }

        // ================================================================== Wykorzystaj wyłom

        struct Breach { public float until; public int origin; }
        readonly Dictionary<EnemyBrain, Breach> breaches = new Dictionary<EnemyBrain, Breach>();
        EnemyBrain breachPendingTarget; int breachPendingOrigin;

        public int BreachWindows { get { PruneBreaches(); return breaches.Count; } }
        public bool HasBreach(EnemyBrain e) { PruneBreaches(); return e != null && breaches.ContainsKey(e); }

        void PruneBreaches()
        {
            if (breaches.Count == 0) return;
            List<EnemyBrain> gone = null;
            foreach (var kv in breaches) if (kv.Key == null || kv.Key.IsDead || clock > kv.Value.until) (gone ?? (gone = new List<EnemyBrain>())).Add(kv.Key);
            if (gone != null) foreach (var e in gone) { breaches.Remove(e); PerkCount("wyłom: okna wygasłe"); }
        }

        /// <summary>Bezpośrednie trafienie ręcznej akcji gracza (bez sług, stref, statusów, reakcji, kopii i efektów cech/unikatów).</summary>
        static bool ManualDirect(in HitData h) =>
            !h.featureEffect && !h.uniqueEffect && !h.isStatusTick && !h.isReaction && !h.isGroundHazard && !h.servantHit && !h.copyHit && h.originId != 0;

        void OnPoiseBroke(EnemyBrain e, PlayerCombat by, HitData? hit)
        {
            if (by != this || e == null || e.IsDead || !PerkActive(NativePerkCatalog.Breach)) return;
            int origin;
            if (hit.HasValue) { if (!ManualDirect(hit.Value)) return; origin = hit.Value.originId; }
            else origin = ActionOrigin; // parowanie – tożsamość akcji obrony
            if (e.Poise == null || e.Poise.Max < PerkT.breachMinPoise) { PerkCount("wyłom: pominięte (słaba postawa)"); return; } // tylko twardy przeciwnik
            breaches[e] = new Breach { until = clock + PerkT.breachSeconds, origin = origin }; // nowe przełamanie zastępuje okno
            PerkCount(hit.HasValue ? "wyłom: okna z ciosu" : "wyłom: okna z parowania");
            CombatEvents.RaiseWorldText(e.AimPoint + Vector3.up * 0.9f, "WYŁOM", new Color(1f, 0.75f, 0.3f));
        }

        /// <summary>Premia wyłomu przed obroną celu: raz na cel, tylko nowa ręczna akcja; zużycie po trafieniu z obrażeniami.</summary>
        void BreachOutgoing(ref HitData hit, EnemyBrain target)
        {
            if (target == null || breaches.Count == 0 || !ManualDirect(hit)) return;
            if (!breaches.TryGetValue(target, out var b) || clock > b.until || b.origin == hit.originId) return;
            float k = PerkT.breachMultiplier;
            hit.physical *= k; hit.magic *= k; // cały budżet bezpośredni (fizyczny, magia i żywioły w podziale) – bez postawy i statusów
            breachPendingTarget = target; breachPendingOrigin = hit.originId;
        }

        void BreachResolved(IHitReceiver target, in HitData hit, in HitResult result)
        {
            if (breachPendingTarget == null || !ReferenceEquals(target, breachPendingTarget) || hit.originId != breachPendingOrigin) return;
            var e = breachPendingTarget;
            breachPendingTarget = null;
            if (result.healthDamage > 0f && breaches.Remove(e)) { PerkCount("wyłom: okna zużyte"); FxLibrary.Flash(e.AimPoint, new Color(1f, 0.75f, 0.3f), 0.8f); }
        }

        /// <summary>Riposta (osobna ścieżka): mnożnik dla oznaczonego celu i od razu zużycie (riposta zawsze trafia).</summary>
        public float ConsumeBreachRiposte(EnemyBrain target)
        {
            if (target == null || !breaches.TryGetValue(target, out var b) || clock > b.until || b.origin == ActionOrigin) return 1f;
            breaches.Remove(target);
            PerkCount("wyłom: okna zużyte"); PerkCount("wyłom: riposty");
            return PerkT.breachMultiplier;
        }

        // ================================================================== Różnorodna sfora

        /// <summary>Liczba różnych aktywnych rodzajów własnych sług (chimera – jej działające wkłady).</summary>
        public int PackKinds
        {
            get
            {
                int mask = 0;
                foreach (var s in Servant.Active)
                {
                    if (s == null || s.Master != this) continue;
                    if (s.Parts != null) { foreach (var p in s.Parts) if (p != null && (s.Mortal || p.remaining > 0f)) mask |= 1 << (int)p.kind; }
                    else mask |= 1 << (int)s.Kind;
                }
                int k = 0; for (int i = 0; i < 3; i++) if ((mask & (1 << i)) != 0) k++;
                return k;
            }
        }

        /// <summary>Mnożnik obrażeń ataku sługi w chwili emisji: 1 + premia × max(0, K − 1).</summary>
        public float PackMultiplier => PerkActive(NativePerkCatalog.Pack) ? 1f + PerkT.packPerKind * Mathf.Max(0, PackKinds - 1) : 1f;

        // ================================================================== Gwałtowna manifestacja

        /// <summary>Mnożnik czasu strefy ręcznego ofensywnego czaru (1 – bez zmian). Odczyt raz przy rzucie.</summary>
        public float ManifestationScale(SpellDefinition spell, bool triggered) =>
            ManifestationQualifies(spell, triggered) && PerkActive(NativePerkCatalog.Manifestation) ? PerkT.manifestationTime : 1f;

        /// <summary>Strefa ręcznego ofensywnego czaru (bez aur i wyzwalaczy) – ta, którą perk by skrócił.</summary>
        public bool ManifestationQualifies(SpellDefinition spell, bool triggered) => spell != null && spell.IsSpell && !spell.IsAura && !triggered;

        /// <summary>
        /// Pomiar realizacji budżetu kwalifikowanej strefy (z perkiem i bez): trafienia celów w całym czasie i w pierwszej połowie.
        /// Bez perka „2 × pierwsza połowa / całość” szacuje, ile dałaby kompresja przy tym samym zachowaniu wrogów.
        /// </summary>
        internal void ZoneMeasured(int ticks, int hits, int firstHalf, bool compressed)
        {
            string p = compressed ? "strefy skrócone" : "strefy zwykłe";
            PerkCount(p + ": liczba"); PerkCount(p + ": tyknięcia", ticks); PerkCount(p + ": trafienia celów", hits); PerkCount(p + ": trafienia w 1. połowie", firstHalf);
        }

        internal void CountManifestation(float baseSeconds, float scale) { if (scale < 1f) { PerkCount("manifestacja: strefy"); PerkCount("manifestacja: czas bazowy", baseSeconds); PerkCount("manifestacja: czas efektywny", baseSeconds * scale); } }

        // ================================================================== Cykl życia

        void PerksInit()
        {
            EnemyBrain.PoiseBroke -= OnPoiseBroke;
            EnemyBrain.PoiseBroke += OnPoiseBroke;
            breaches.Clear(); breachPendingTarget = null; PerkTelemetry.Clear();
        }

        void PerksFloorStart() { breaches.Clear(); breachPendingTarget = null; }
        void PerksDestroy() { EnemyBrain.PoiseBroke -= OnPoiseBroke; breaches.Clear(); }

        /// <summary>Stan perków do HUD (null – nic do pokazania).</summary>
        public string PerkHud
        {
            get
            {
                var parts = new List<string>();
                if (PerkActive(NativePerkCatalog.Pack)) { int k = PackKinds; parts.Add($"sfora: {k} rodz. +{Mathf.Max(0, k - 1) * PerkT.packPerKind * 100f:0}%"); }
                if (BreachWindows > 0) parts.Add($"wyłom: {BreachWindows}");
                return parts.Count == 0 ? null : string.Join(" · ", parts);
            }
        }
    }
}
