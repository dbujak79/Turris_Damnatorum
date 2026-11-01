using System.Linq;
using UnityEngine;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Polityka botów dla 30 cech klasowych: jawny wybór wariantu (A i B na zmianę – B nie zostaje nieużywany, albo wymuszony
    /// <see cref="ForcedFeatureId"/>) i proste, czytelne warunki użycia na podstawie stanu cechy z HUD. Bot naciska ten sam przycisk
    /// klasy co gracz – walidacja, koszty i odmowy są wspólne. Wynik mechaniki (telemetria cechy) jest zapisywany osobno od jakości
    /// polityki (próby, odmowy z przyczyną).
    /// </summary>
    public abstract partial class HeroBot
    {
        /// <summary>Wymuszona cecha dla krótkich scenariuszy (null – A/B na zmianę według numeru podejścia).</summary>
        public static string ForcedFeatureId;
        float nextFeatureTry, nextSwap;
        const float T2 = 2f;

        /// <summary>Przygotowanie cechy zwykłym skillem (strefa, przywołanie, status) – tym samym przyciskiem slotu co gracz.</summary>
        bool CastFirst(System.Func<SpellRuntime, bool> pick)
        {
            for (int s = 0; s < RunState.SkillSlotCount; s++)
            {
                var sk = Pc.Skill(s);
                if (sk == null || !pick(sk) || !Pc.SkillUsable(s)) continue;
                Pc.RequestSkill(s);
                nextFeatureTry = now + 0.6f;
                return true;
            }
            return false;
        }

        /// <summary>Wybór cechy w przygotowaniu podejścia (ten sam MetaService co gracz).</summary>
        public void ChooseFeature(LoadoutPlan plan, int runIndex)
        {
            var own = ClassFeatureCatalog.For(plan.classDef);
            if (own.Count == 0) return;
            string id = ForcedFeatureId != null && ClassFeatureCatalog.Owns(plan.classDef, ForcedFeatureId) ? ForcedFeatureId : own[runIndex % own.Count].id;
            Root.Meta.SelectFeature(plan, id);
        }

        /// <summary>Użycie cechy w walce. True – w tej klatce naciśnięto przycisk klasy.</summary>
        protected bool TryUseFeature()
        {
            var f = Pc.Feature;
            if (f == null || now < nextFeatureTry) return false;
            var hud = f.Hud;
            float d = target != null ? FlatDist(target) : 99f;
            int close = EnemiesWithin(3.6f);
            float hp = Pc.Health.Fraction;
            bool boss = target != null && (target.Def.isBoss || target.IsElite);
            bool ready = hud.state == ClassFeatureState.Ready;
            bool want = false;
            switch (f)
            {
                case GuardFeature g:
                    if (g.Active) BlockHeld = close >= 1;
                    want = ready && close >= 1 && Pc.Build.guard != null && Pc.Stamina.Fraction > 0.6f; break;
                case ArsenalFeature _:
                    want = ready && close >= 1 && now >= nextSwap && (Pc.Actions.IsIdle || Pc.Actions.Phase == ActionPhase.Recovery);
                    if (want) nextSwap = now + 6f; break;
                case AbsolutionFeature a:
                    want = a.Grace >= Mathf.Min(15f, a.Capacity * 0.5f) && d < 4f; break;
                case JudgementFeature _:
                    if (hud.state == ClassFeatureState.Blocked && d < 10f && CastFirst(sk => sk.Def.IsAura)) return true; // najpierw aura
                    want = ready && d < 8f; break;
                case RedLineFeature _:
                    want = ready && close >= 1 && hp > 0.3f; break;
                case FurySpendFeature fs:
                    want = Pc.Raging && Pc.RageTime >= T2 && fs.Tokens == 0; break;
                case MismatchedPairFeature _:
                case UnbrokenDanceFeature _:
                case TwoBeatsFeature _:
                    want = ready && d < 3f; break;
                case ArcaneDebtFeature debt:
                    want = ready && debt.Debt <= 0f && d < 10f && (Pc.Mana.Fraction < 0.5f || Enumerable.Range(0, RunState.SkillSlotCount).Any(x => Pc.Skill(x) != null && Pc.Skill(x).Def.IsSpell && Pc.Mana.Current < Pc.Skill(x).manaCost)); break;
                case ManaCloakFeature _:
                    want = ready && (d < 8f || hp < 0.5f) && Pc.Mana.Fraction > 0.4f; break;
                case LivingFurnaceFeature lf:
                    want = ready && hp > 0.65f && d < 9f && lf.Ember <= 0f; break;
                case GreatFireFeature _:
                case CollidingFrontsFeature _:
                    want = ready && d < 9f; break;
                case EnduringFrostFeature _:
                case PinnedShadowFeature _:
                    want = ready && d < 9f; break;
                case OpenArteryFeature _:
                    want = ready && d < 4f && (boss || target.Health.Fraction > 0.5f); break;
                case DeepCurrentFeature dc:
                    if (hud.state == ClassFeatureState.Blocked && d < 9f && CastFirst(sk => sk.Def.kind == SpellKind.Zone)) return true;
                    want = ready && d < 12f; break;
                case SilentEyeFeature eye:
                    want = !eye.Open && ready && d < 12f || eye.Open && eye.Count >= Root.config.classFeatures.eyeCapacity; break;
                case BastionFeature _:
                    want = ready && EnemiesWithin(9f) >= 1; break;
                case ShatteringShellFeature sh:
                    want = ready && sh.Plates == 0 && Pc.Ward + Pc.BarrierAmount > 25f && d < 12f; break;
                case IncarnationFeature inc:
                    if (!inc.InForm && Servant.Active.Count == 0 && d < 10f && CastFirst(sk => sk.Def.kind == SpellKind.Summon)) return true;
                    want = !inc.InForm && ready && Servant.Active.Count > 0 && d < 8f; break;
                case ChimeraFeature ch:
                    if (!ch.Previewing && Servant.SlotsUsed < 2 && d < 10f && CastFirst(sk => sk.Def.kind == SpellKind.Summon)) return true;
                    want = ch.Previewing || ready && Servant.SlotsUsed >= 2 && d < 12f; break;
                case WeaponArcanaFeature _:
                    want = ready && d < 9f && Enumerable.Range(0, RunState.SkillSlotCount).Any(s => Pc.Skill(s) != null && Pc.Skill(s).Def.IsSpell); break;
                case FalseWitnessFeature _:
                    want = ready && d < 8f; break;
                case StolenTrickFeature st:
                    want = st.Trick != StolenTrickFeature.TrickKind.None ? (st.Trick == StolenTrickFeature.TrickKind.Projectile ? d < 14f : d < 3f)
                        : ready && !st.Observing && d < 10f; break;
                case HuntersNailsFeature n:
                    want = n.NailCount >= 4 || (ready && n.NailCount == 0 && d < 14f); break;
                case FiringPositionFeature fp:
                    want = ready && fp.Post == null && d > 5f && d < 16f; break;
                case SpellArrowFeature sa:
                    if (sa.Preparing)
                    {
                        for (int s = 0; s < RunState.SkillSlotCount; s++)
                            if (SpellArrowFeature.Compatible(Pc.Skill(s), out _) && Pc.Skill(s).instance.Ready) { Pc.RequestSkill(s); nextFeatureTry = now + 0.4f; return true; }
                        return false;
                    }
                    want = ready && sa.Armed == null && d < 14f && Pc.Mana.Fraction > 0.4f
                           && Enumerable.Range(0, RunState.SkillSlotCount).Any(s => SpellArrowFeature.Compatible(Pc.Skill(s), out _) && Pc.Skill(s).instance.Ready); break;
                case AnsweringArrowFeature aa:
                    if (target != null && d < 8f && AnsweringArrowFeature.Answer(target.Status) == Element.None && CastFirst(sk => sk.Def.id == "hu_snare")) return true;
                    want = ready && aa.Charges == 0 && target != null && d < 14f; break;
            }
            if (!want) return false;
            if (target != null && !target.IsDead) Pc.LockTarget = target; // ten sam wybór celu co namierzanie gracza
            Pc.RequestSkill(PlayerCombat.SignatureSlot);
            nextFeatureTry = now + 0.35f;
            return true;
        }

        /// <summary>Zapis telemetrii cechy do rekordu podejścia (mechanika osobno od polityki).</summary>
        void RecordFeature()
        {
            if (Rec == null || Pc == null) return;
            var R = Rec.R;
            foreach (var kv in Pc.PerkTelemetry) R.perkStats.Add(kv.Key, Mathf.Round(kv.Value * 100f) / 100f);
            if (Pc.Feature == null) return;
            R.featureId = Pc.Feature.Def.id;
            R.featurePresses = Pc.FeatureTelemetry.presses;
            R.featureUses = Pc.FeatureTelemetry.uses;
            foreach (var kv in Pc.FeatureTelemetry.values) R.featureStats.Add(kv.Key, Mathf.RoundToInt(kv.Value));
            foreach (var kv in Pc.FeatureTelemetry.rejections) R.featureRejections.Add(kv.Key, kv.Value);
        }
    }
}
