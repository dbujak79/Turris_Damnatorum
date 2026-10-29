using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Kolekcja skilli bota: zakup ofert sklepu dusz, zamiana skilli w slotach i pamięć ich skuteczności między podejściami.
    /// Gracz ocenia skill po obrażeniach na sekundę odnowienia (z własnych podejść), nowe próbuje z ciekawości, dba o skill
    /// dystansowy. Tester co piętro wkłada do slotu najmniej sprawdzony skill z kolekcji (pokrycie danych).
    /// </summary>
    public abstract partial class HeroBot
    {
        /// <summary>Ile użyć wystarcza, by gracz ocenił skill z własnych obserwacji (wcześniej – ciekawość).</summary>
        const float MinUsesToJudge = 5f;

        /// <summary>Obserwacje skilla z poprzednich podejść serii.</summary>
        class SkillMemory { public float dmg, uses, levelSum; }

        readonly Dictionary<string, SkillMemory> skillMemory = new Dictionary<string, SkillMemory>();
        /// <summary>Pochodzenie skilli bieżącego podejścia: startowy, odblokowanie, zestaw, nagroda, sklep, przedmiot.</summary>
        readonly Dictionary<SpellDefinition, string> origin = new Dictionary<SpellDefinition, string>();

        protected enum SkillRole { Ranged, MeleeArea, Mobility, Support, Other }

        protected static SkillRole RoleOf(SpellDefinition d)
        {
            switch (d.kind)
            {
                case SpellKind.Wave: case SpellKind.Projectile: case SpellKind.Chain: case SpellKind.WeaponThrow:
                case SpellKind.Zone: case SpellKind.Meteor: case SpellKind.Storm:
                    return SkillRole.Ranged;
                case SpellKind.Cone: case SpellKind.Cleave: case SpellKind.Flurry: case SpellKind.ShieldBash:
                case SpellKind.Quake: case SpellKind.Whirlwind: case SpellKind.Nova:
                    return SkillRole.MeleeArea;
                case SpellKind.Charge: case SpellKind.Dash:
                    return SkillRole.Mobility;
                case SpellKind.Heal: case SpellKind.Barrier: case SpellKind.FrostArmor: case SpellKind.Undying:
                case SpellKind.WeaponBuff: case SpellKind.Aura: case SpellKind.Warcry:
                    return SkillRole.Support;
                default:
                    return SkillRole.Other;
            }
        }

        protected string OriginOf(SpellDefinition d) => d != null && origin.TryGetValue(d, out var o) ? o : "?";

        /// <summary>Nowe skille w kolekcji dostają pochodzenie (na starcie podejścia – start/odblokowanie, potem – nagroda).</summary>
        void TrackOrigins(bool runStart)
        {
            var sig = Root.Run.signatureSkill;
            if (sig != null && !origin.ContainsKey(sig.definition)) origin[sig.definition] = "klasa";
            foreach (var s in Root.Run.knownSpells)
            {
                if (s == null || origin.ContainsKey(s.definition)) continue;
                string o;
                if (s.fromItem) o = "przedmiot";
                else if (!runStart) o = "nagroda";
                else if (s.definition.displayName == Rec.R.startSkill) o = "startowy";
                else o = s.permanent ? "odblokowanie" : "zestaw";
                origin[s.definition] = o;
            }
        }

        /// <summary>Czy postać może użyć skilla teraz: twarde wymagania Inteligencji i wymagane wyposażenie.</summary>
        protected bool CanUseDef(SpellDefinition d)
        {
            var b = Pc.Build;
            if (b == null || b.stats == null) return false;
            if (d.IsSpell && Root.config.balance.spellRequirementsAreHard && !StatCalculator.RequirementsMet(d.requirements, b.stats)) return false;
            var tags = Root.Run.CurrentTags(b);
            return WeaponFamilies.Meets(d.requiredTags, tags);
        }

        /// <summary>Obserwacje skilla: pamięć serii + bieżące podejście.</summary>
        (float dmg, float uses, float avgLevel) Observed(string name)
        {
            float dmg = 0, uses = 0, lvl = 0;
            if (skillMemory.TryGetValue(name, out var m)) { dmg += m.dmg; uses += m.uses; lvl += m.levelSum; }
            if (Rec != null) { dmg += Rec.R.damageBySource.Get(name); uses += Rec.R.skillUses.Get(name); lvl += Rec.R.skillLevelSum.Get(name); }
            return (dmg, uses, uses > 0 ? lvl / uses : 0f);
        }

        /// <summary>Obrażenia na sekundę odnowienia z obserwacji; null, gdy za mało danych albo skill nie zadaje obrażeń wprost.</summary>
        float? ObservedValue(SpellDefinition d, int level)
        {
            if (RoleOf(d) == SkillRole.Support) return null;
            var (dmg, uses, avgLvl) = Observed(d.displayName);
            if (uses < MinUsesToJudge || dmg <= 0f) return null;
            float cd = Mathf.Max(1.5f, d.CooldownAt(level));
            return dmg / uses * (1f + 0.1f * (level - avgLvl)) / cd;
        }

        /// <summary>Wartość skilla dla gracza. Skill bez danych – ciekawość: trochę więcej niż najlepszy znany.</summary>
        float SkillValue(SpellDefinition d, int level)
        {
            var known = Root.Run.knownSpells.Where(s => s != null).Select(s => ObservedValue(s.definition, s.level))
                .Where(v => v.HasValue).Select(v => v.Value).ToList();
            float best = known.Count > 0 ? known.Max() : 10f;
            float median = known.Count > 0 ? known.OrderBy(v => v).ElementAt(known.Count / 2) : 10f;
            if (RoleOf(d) == SkillRole.Support) return median * 0.6f;
            var obs = ObservedValue(d, level);
            // Skill ruchu (szarża, doskok) zarabia skróceniem dystansu do strzelców, nie obrażeniami – co najmniej 90% mediany.
            if (RoleOf(d) == SkillRole.Mobility) return Mathf.Max(obs ?? 0f, median * 0.9f);
            if (obs.HasValue) return obs.Value;
            var (dmg, uses, _) = Observed(d.displayName);
            if (uses >= MinUsesToJudge && dmg <= 0f) return median * 0.6f; // obrażenia nie trafiają do telemetrii – traktuj jak wsparcie
            return best * 1.1f;
        }

        /// <summary>Wartość zestawu: najlepsze pierwsze; pierwszy skill dystansowy ×1,3, powtórzona rola ×0,8, drugie wsparcie ×0,5.</summary>
        float SetValue(IEnumerable<(SpellDefinition d, int level)> set)
        {
            var roles = new List<SkillRole>();
            float total = 0f;
            foreach (var (d, level) in set.OrderByDescending(x => SkillValue(x.d, x.level)))
            {
                var r = RoleOf(d);
                float f = roles.Contains(r) ? (r == SkillRole.Support ? 0.5f : 0.8f) : (r == SkillRole.Ranged ? 1.3f : 1f);
                total += SkillValue(d, level) * f;
                roles.Add(r);
            }
            return total;
        }

        List<SpellInstance> SlotPool() => Root.Run.knownSpells
            .Where(s => s != null && s != Root.Run.signatureSkill && !s.definition.IsAura && CanUseDef(s.definition)).ToList();

        /// <summary>Najlepszy zestaw trzech skilli z kolekcji (zachłannie, z premią za różne role).</summary>
        List<SpellInstance> BestSet(List<SpellInstance> pool)
        {
            var chosen = new List<SpellInstance>();
            while (chosen.Count < RunState.SkillSlotCount)
            {
                SpellInstance pick = null; float best = float.MinValue;
                foreach (var s in pool)
                {
                    if (chosen.Contains(s)) continue;
                    float v = SetValue(chosen.Append(s).Select(x => (x.definition, x.level)));
                    if (v > best) { best = v; pick = s; }
                }
                if (pick == null) break;
                chosen.Add(pick);
            }
            return chosen;
        }

        /// <summary>Wkłada skill do slotu, zapisując zamianę w telemetrii.</summary>
        void PutInSlot(SpellInstance s, int slot)
        {
            var old = Root.Run.skillSlots[slot];
            Root.Run.AssignSlot(s, slot);
            Rec.R.skillSwaps.Add($"{(old != null ? old.definition.displayName : "pusty")} → {s.definition.displayName}");
        }

        /// <summary>Układa sloty: gracz – najlepszy zestaw (z progiem, by nie żonglować), tester – rotacja najmniej sprawdzonych.</summary>
        protected void ArrangeSlots()
        {
            var run = Root.Run;
            var pool = SlotPool();
            var slotted = run.skillSlots.ToList();
            bool changed = false;
            if (P.persona == BotPersona.Tester)
            {
                // Jedna zamiana na piętro: najmniej używany skill spoza slotów zastępuje najczęściej używany w slocie.
                var cand = pool.Where(s => run.SlotOf(s) < 0).OrderBy(s => Observed(s.definition.displayName).uses).FirstOrDefault();
                if (cand != null)
                {
                    int slot = System.Array.IndexOf(run.skillSlots, null);
                    if (slot < 0)
                        slot = Enumerable.Range(0, RunState.SkillSlotCount)
                            .OrderByDescending(i => run.skillSlots[i] == null || !pool.Contains(run.skillSlots[i]) ? float.MaxValue : Observed(run.skillSlots[i].definition.displayName).uses)
                            .First();
                    PutInSlot(cand, slot);
                    changed = true;
                }
            }
            else
            {
                var best = BestSet(pool);
                var current = slotted.Where(s => s != null && pool.Contains(s)).ToList();
                bool hasEmpty = current.Count < Mathf.Min(RunState.SkillSlotCount, pool.Count);
                float cur = SetValue(current.Select(x => (x.definition, x.level)));
                float bst = SetValue(best.Select(x => (x.definition, x.level)));
                if (hasEmpty || bst > cur * 1.08f)
                {
                    var incoming = best.Where(s => run.SlotOf(s) < 0).ToList();
                    foreach (var s in incoming)
                    {
                        int slot = Enumerable.Range(0, RunState.SkillSlotCount)
                            .FirstOrDefault(i => run.skillSlots[i] == null || !best.Contains(run.skillSlots[i]));
                        PutInSlot(s, slot);
                        changed = true;
                    }
                }
            }
            if (changed) Pc.RefreshBuild();
        }

        /// <summary>
        /// Oferty skilli sklepu dusz (trzy naraz): tester kupuje losową, gracz – pierwszą, którą wstawiłby do slotu (pusty slot,
        /// brakująca rola albo lepiej niż najsłabszy w slocie). Najwyżej jeden zakup na piętro.
        /// </summary>
        void ConsiderShopOffer()
        {
            var offers = Root.ShopOffers.ToList();
            if (offers.Count == 0) return;
            foreach (var _ in offers) Rec.R.shopOffers.Add("widziana");
            if (Root.Run.souls < SoulShop.OfferCost(Root.Run, Root.config.balance)) { Rec.R.shopOffers.Add("za mało dusz"); return; }
            var offer = P.persona == BotPersona.Tester ? offers[UnityEngine.Random.Range(0, offers.Count)] : offers.FirstOrDefault(WouldSlot);
            if (offer == null) { Rec.R.shopOffers.Add("odrzucona"); return; }
            Root.BuyShopOffer(offer);
            if (Root.Run.FindSpell(offer) == null) return;
            origin[offer] = "sklep";
            Rec.R.shopOffers.Add("kupiona");
            Rec.R.purchases.Add("oferta: " + offer.displayName);
        }

        /// <summary>Czy gracz wstawiłby ten skill do slotu (nowy, poziom 0).</summary>
        protected bool WouldSlot(SpellDefinition d)
        {
            if (!CanUseDef(d) || d.IsAura) return false;
            var current = Root.Run.skillSlots.Where(s => s != null && CanUseDef(s.definition)).Select(s => (s.definition, s.level)).ToList();
            if (current.Count < RunState.SkillSlotCount) return true;
            float cur = SetValue(current);
            for (int i = 0; i < current.Count; i++)
            {
                var alt = current.ToList();
                alt[i] = (d, 0);
                if (SetValue(alt) > cur * 1.08f) return true;
            }
            return false;
        }

        /// <summary>Koniec podejścia: obserwacje skilli trafiają do pamięci serii.</summary>
        void RememberSkills()
        {
            foreach (var (name, uses) in Rec.R.skillUses.Items)
            {
                if (!skillMemory.TryGetValue(name, out var m)) skillMemory[name] = m = new SkillMemory();
                m.uses += uses;
                m.dmg += Rec.R.damageBySource.Get(name);
                m.levelSum += Rec.R.skillLevelSum.Get(name);
            }
        }
    }
}
