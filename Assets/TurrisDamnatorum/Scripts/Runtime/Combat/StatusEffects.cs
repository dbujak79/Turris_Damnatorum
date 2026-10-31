using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Efekty trwające na postaci (gracz lub wróg) i reakcje żywiołów. Czysta logika bez MonoBehaviour.
    ///
    /// | Efekt       | Warstwy                         | Działanie |
    /// |-------------|---------------------------------|-----------|
    /// | Krwawienie  | do bleedMaxStacks, każda osobno | obrażenia z wylądowanych fizycznych; ×2, gdy cel się rusza |
    /// | Podpalenie  | 1, odświeżane (silniejsze wygrywa) | obrażenia z wylądowanego ognia |
    /// | Chłód       | wspólny czas                     | spowolnienie ruchu i akcji; trzecia warstwa → zamrożenie |
    /// | Zamrożenie  | –                                | brak ruchu i akcji; potem odporność na kolejne |
    /// | Porażenie   | 1, odświeżane                    | cel otrzymuje więcej obrażeń |
    /// | Zachwianie  | wspólny czas                     | spowolnienie ruchu; trzecia warstwa → ogłuszenie |
    /// | Ogłuszenie  | –                                | brak ruchu i akcji; potem odporność na kolejne |
    /// | Pęknięcie pancerza | do sunderMaxStacks, wspólny czas | więcej obrażeń fizycznych |
    /// | Przemoczenie (HU) | 1, odświeżane                   | sam znacznik pod reakcje |
    /// | Spowolnienie (HU) | 1, odświeżane                   | wolniejszy ruch; nie zamraża |
    ///
    /// Reakcje: ogień w wychłodzony cel = szok termiczny (premia do ognia, chłód znika);
    /// błyskawica w przemoczony cel = przewodnictwo (premia do błyskawicy, HU SoakedAir);
    /// błyskawica w krwawiący cel = przewodzenie (reszta krwawienia od razu);
    /// błyskawica w zamrożony cel = roztrzaskanie (wybuch wokół celu, lód pęka);
    /// ziemia w płonący cel = wybuch żaru (dodatkowy ogień, u wrogów w obszarze); ziemia w wychłodzony albo przemoczony cel = błoto (ogłuszenie, chłód i woda znikają);
    /// woda (mróz) w płonący cel = para (ogień gaśnie, chmura, w której demony chybiają – HU Steam).
    /// Jak w HU reakcje wywołuje tylko bezpośrednie trafienie (nie tyknięcia efektów, stref ani aur, nie obrażenia reakcji),
    /// a ta sama reakcja na tym samym celu zachodzi najwyżej raz na reactionCooldown.
    /// Obrażenia reakcji przechodzą przez pancerz tylko raz – liczone są z obrażeń, które już wylądowały.
    /// </summary>
    public class StatusEffects
    {
        struct Stack { public float dps, remaining, movingMult; }

        readonly List<Stack> bleeds = new List<Stack>();
        float burnDps, burnTime;
        int chill; float chillTime;
        float frozenTime, freezeImmunity;
        float shockTime;
        int stagger; float staggerTime;
        float stunTime, stunImmunity;
        int sunder; float sunderTime;
        float soakedTime, slowTime;
        float tickTimer, pending;
        enum Reaction { ThermalShock, SoakedAir, Conduction, Shatter, EmberBurst, Mud, Steam }
        readonly float[] reactionReadyAt = new float[7];
        float clock, rootUntil = float.NegativeInfinity, webTime;
        public bool Rooted => clock < rootUntil;
        public bool Webbed => webTime > 0;
        public float RootRemaining => Mathf.Max(0, rootUntil - clock);
        /// <summary>Kokon HU: ruch i unik stoją, akcje pozostają dostępne; 2 s odporności i pół czasu przy powtórce do 6 s.</summary>
        public float TryRoot(float seconds)
        {
            if (seconds <= 0 || Rooted || clock < rootUntil + 2) return 0;
            float held = Mathf.Min(3, seconds) * (clock < rootUntil + 6 ? 0.5f : 1);
            rootUntil = clock + held;
            return held;
        }
        public void TouchWeb() => webTime = Mathf.Max(webTime, 0.15f);

        /// <summary>Mnożnik czasu kontroli (chłód, zamrożenie): bossy i elity krócej.</summary>
        public float ControlMultiplier = 1f;
        /// <summary>Mnożnik obrażeń krwawienia (0 = odporność).</summary>
        public float BleedMultiplier = 1f;
        /// <summary>Odporność na podpalenie (np. płaszcz popiołu).</summary>
        public bool BurnImmune;
        /// <summary>Obrażenia ostatniego przewodzenia (dla przeskoku na kolejnego wroga).</summary>
        public float LastConductionDamage { get; private set; }
        /// <summary>Właściciel zadaje wybuch żaru w obszarze (HU IgnitedEarth: 1,8 m) – wtedy nie wchodzi on w obrażenia natychmiastowe.</summary>
        public bool AreaReactions;
        /// <summary>Pęknięcie pancerza obniża odporność fizyczną celu (HU: −4% za warstwę, wróg) zamiast mnożyć obrażenia (bohater).</summary>
        public bool SunderAsResistance;
        /// <summary>Obniżenie odporności fizycznej z pęknięcia pancerza (gdy <see cref="SunderAsResistance"/>).</summary>
        public float SunderResistReduction(BalanceConfig b) => SunderAsResistance ? SunderStacks * b.sunderPerStack : 0f;
        /// <summary>Obrażenia ostatniego wybuchu żaru do zadania w obszarze (gdy <see cref="AreaReactions"/>).</summary>
        public float LastEmberBurstDamage { get; private set; }
        /// <summary>Ostatnie trafienie wywołało parę – właściciel stawia chmurę (jak w HU tylko po stronie wrogów, od ciosów bohatera).</summary>
        public bool LastSteam { get; private set; }

        static float M(float v) => v <= 0f ? 1f : v; // 0 w domyślnej strukturze = brak modyfikatora

        /// <summary>Reakcja możliwa: bezpośrednie trafienie (HU IsDirectHit) i minął limit tej reakcji na tym celu.</summary>
        bool CanReact(in HitData hit, Reaction kind) =>
            !hit.isStatusTick && !hit.isReaction && !hit.isGroundHazard && clock >= reactionReadyAt[(int)kind];
        void Reacted(Reaction kind, BalanceConfig b) => reactionReadyAt[(int)kind] = clock + b.reactionCooldown;

        public int BleedStacks => bleeds.Count;
        public bool Burning => burnTime > 0;
        public int ChillStacks => chillTime > 0 ? chill : 0;
        public bool Frozen => frozenTime > 0;
        public float FrozenRemaining => frozenTime;
        public bool Shocked => shockTime > 0;
        public int StaggerStacks => staggerTime > 0 ? stagger : 0;
        public bool Stunned => stunTime > 0;
        public float StunRemaining => stunTime;
        public int SunderStacks => sunderTime > 0 ? sunder : 0;
        public bool Soaked => soakedTime > 0;
        public bool Slowed => slowTime > 0;
        /// <summary>Cel nie może się ruszać ani działać (zamrożenie albo ogłuszenie).</summary>
        public bool Incapacitated => Frozen || Stunned;
        public bool Any => bleeds.Count > 0 || Burning || ChillStacks > 0 || Frozen || Shocked || StaggerStacks > 0 || Stunned || SunderStacks > 0 || Soaked || Slowed || Rooted || Webbed;

        /// <summary>Łączne obrażenia, które zostały jeszcze w warstwach krwawienia.</summary>
        public float BleedRemainingDamage
        {
            get { float s = 0; foreach (var b in bleeds) s += b.dps * b.remaining; return s; }
        }

        /// <summary>Spowolnienie spoza efektów (np. uczepione czerwie z Hell Unleashed) – część prędkości, sumowana.</summary>
        public float ExternalSlow;
        public float MoveMultiplier(BalanceConfig b) => Incapacitated || Rooted ? 0f : (Webbed ? 0.5f : 1f) * (Slowed ? b.slowMoveMultiplier : 1f) * Mathf.Max(0.25f, 1f - ChillStacks * b.chillSlowPerStack - StaggerStacks * b.staggerSlow - ExternalSlow);
        public float ActionSpeedMultiplier(BalanceConfig b) => Incapacitated ? 0f : Mathf.Max(0.25f, 1f - ChillStacks * b.chillSlowPerStack);
        public float DamageTakenMultiplier(BalanceConfig b) => Shocked ? 1f + b.shockDamageBonus : 1f;

        /// <summary>Przed rozstrzygnięciem trafienia: porażenie i szok termiczny zwiększają obrażenia. Zwraca nazwę reakcji albo null.</summary>
        public string ModifyIncoming(ref HitData hit, BalanceConfig b)
        {
            if (hit.isStatusTick) return null;
            float m = DamageTakenMultiplier(b);
            // Premie sytuacyjne ataku: przeciw zamrożonym (rozbija lód) i krwawiącym.
            if (Frozen && hit.bonusVsFrozen > 0) m *= 1f + hit.bonusVsFrozen;
            if (bleeds.Count > 0 && hit.bonusVsBleeding > 0) m *= 1f + hit.bonusVsBleeding;
            if ((ChillStacks > 0 || Frozen || Soaked) && hit.bonusVsChilled > 0) m *= 1f + hit.bonusVsChilled;
            hit.physical *= m * (SunderAsResistance ? 1f : 1f + SunderStacks * b.sunderPerStack);
            hit.magic *= m;
            if (hit.Has(Element.Fire) && hit.magic > 0 && ChillStacks > 0 && CanReact(hit, Reaction.ThermalShock))
            {
                hit.magic *= 1f + b.thermalShockBonus * hit.ShareOf(Element.Fire);
                chill = 0; chillTime = 0;
                Reacted(Reaction.ThermalShock, b);
                return "SZOK TERMICZNY";
            }
            if (hit.Has(Element.Lightning) && hit.magic > 0 && Soaked && CanReact(hit, Reaction.SoakedAir))
            {
                hit.magic *= 1f + b.soakedAirBonus * hit.ShareOf(Element.Lightning);
                Reacted(Reaction.SoakedAir, b);
                return "PRZEWODNICTWO";
            }
            return null;
        }

        /// <summary>
        /// Po trafieniu, które dotarło do celu: nakłada efekty ataku i rozstrzyga przewodzenie.
        /// Zwraca obrażenia natychmiastowe (przewodzenie). <paramref name="froze"/> = cel właśnie zamarzł.
        /// </summary>
        public float OnLanded(in HitData hit, in HitResult r, BalanceConfig b, out string reaction, out bool froze)
            => OnLanded(hit, r, b, out reaction, out froze, out _);

        /// <summary>
        /// Jak wyżej; <paramref name="shatterDamage"/> &gt; 0 oznacza roztrzaskanie – właściciel zadaje wybuch wokół siebie.
        /// </summary>
        public float OnLanded(in HitData hit, in HitResult r, BalanceConfig b, out string reaction, out bool froze, out float shatterDamage)
        {
            reaction = null; froze = false; shatterDamage = 0f; LastConductionDamage = 0f; LastEmberBurstDamage = 0f; LastSteam = false;
            if (hit.isStatusTick || !(r.outcome == HitOutcome.Hit || r.outcome == HitOutcome.GuardBroken) || r.healthDamage <= 0) return 0f;
            float total = hit.physical + hit.magic;
            float physShare = total > 0 ? hit.physical / total : 0f;
            float landedPhys = r.healthDamage * physShare, landedMagic = r.healthDamage - landedPhys;

            float instant = 0f;
            if (hit.Has(Element.Lightning) && hit.magic > 0 && bleeds.Count > 0 && CanReact(hit, Reaction.Conduction))
            {
                Reacted(Reaction.Conduction, b);
                instant = BleedRemainingDamage * b.conductionMultiplier * M(hit.mods.conductionMult);
                LastConductionDamage = instant;
                bleeds.Clear();
                reaction = "PRZEWODZENIE";
            }
            if (hit.Has(Element.Lightning) && hit.magic > 0 && Frozen && CanReact(hit, Reaction.Shatter))
            {
                Reacted(Reaction.Shatter, b);
                shatterDamage = r.healthDamage * b.shatterShare;
                frozenTime = 0f;
                reaction = "ROZTRZASKANIE";
            }
            else if (Frozen && hit.bonusVsFrozen > 0) frozenTime = 0f; // broń obuchowa rozbija lód
            if (hit.Has(Element.Earth) && hit.magic > 0 && reaction == null)
            {
                if (Burning)
                {
                    if (CanReact(hit, Reaction.EmberBurst))
                    {
                        // Wybuch żaru: ziemia wzbija płonące odłamki – dodatkowy ogień od razu.
                        Reacted(Reaction.EmberBurst, b);
                        float ember = landedMagic * hit.ShareOf(Element.Earth) * b.emberBurstShare;
                        if (AreaReactions) LastEmberBurstDamage = ember;
                        else instant += ember;
                        reaction = "WYBUCH ŻARU";
                    }
                }
                else if ((ChillStacks > 0 || Soaked) && !Frozen && CanReact(hit, Reaction.Mud))
                {
                    // Błoto: ziemia w wychłodzony albo przemoczony cel ogłusza.
                    Reacted(Reaction.Mud, b);
                    chill = 0; chillTime = 0; soakedTime = 0;
                    if (Stun(b)) froze = true;
                    reaction = "BŁOTO";
                }
            }
            if (AreaReactions && hit.Has(Element.Frost) && hit.magic > 0 && Burning && reaction == null && CanReact(hit, Reaction.Steam))
            {
                // Para: woda gasi ogień; przed efektami trafienia, jak w HU.
                Reacted(Reaction.Steam, b);
                burnTime = 0f; burnDps = 0f;
                LastSteam = true;
                reaction = "PARA";
            }
            if (hit.statuses != null)
                foreach (var s in hit.statuses)
                {
                    if (s == null || (s.chance < 0.999f && Random.value > s.chance)) continue;
                    for (int i = 0; i < Mathf.Max(1, s.stacks); i++) froze |= Apply(s.kind, landedPhys, landedMagic, b, hit.mods);
                }
            return instant;
        }

        /// <summary>Nakłada jedną warstwę efektu. Zwraca true, jeśli cel właśnie zamarzł.</summary>
        public bool Apply(StatusKind kind, float landedPhysical, float landedMagic, BalanceConfig b) => Apply(kind, landedPhysical, landedMagic, b, StatusModifiers.None);

        /// <summary>Nakłada jedną warstwę efektu z modyfikatorami atakującego.</summary>
        public bool Apply(StatusKind kind, float landedPhysical, float landedMagic, BalanceConfig b, StatusModifiers mods)
        {
            switch (kind)
            {
                case StatusKind.Bleed:
                {
                    if (landedPhysical <= 0 || BleedMultiplier <= 0) return false;
                    var st = new Stack
                    {
                        dps = landedPhysical * b.bleedShare * BleedMultiplier * M(mods.bleedDamageMult) / Mathf.Max(0.1f, b.bleedDuration),
                        remaining = b.bleedDuration,
                        movingMult = b.bleedMovingMultiplier + mods.bleedMovingBonus,
                    };
                    if (bleeds.Count < Mathf.Max(1, b.bleedMaxStacks + mods.bleedStackBonus)) { bleeds.Add(st); return false; }
                    // Limit warstw: nowa zastępuje najsłabszą (jeśli jest silniejsza).
                    int weakest = 0;
                    for (int i = 1; i < bleeds.Count; i++) if (bleeds[i].dps < bleeds[weakest].dps) weakest = i;
                    if (st.dps >= bleeds[weakest].dps) bleeds[weakest] = st;
                    return false;
                }
                case StatusKind.Burn:
                {
                    if (BurnImmune) return false;
                    float source = landedMagic > 0 ? landedMagic : landedPhysical;
                    if (source <= 0) return false;
                    float duration = b.burnDuration + mods.burnDurationBonus;
                    float dps = source * b.burnShare / Mathf.Max(0.1f, b.burnDuration); // dłuższe = więcej łącznie
                    burnDps = Burning ? Mathf.Max(burnDps, dps) : dps;
                    burnTime = Mathf.Max(burnTime, duration);
                    return false;
                }
                case StatusKind.Chill:
                {
                    if (Frozen) return false;
                    int needed = Mathf.Max(1, b.chillStacksToFreeze - mods.freezeReduction);
                    chill = Mathf.Min(ChillStacks + 1, needed);
                    chillTime = b.chillDuration * ControlMultiplier;
                    if (chill >= needed && freezeImmunity <= 0) { Freeze(b); return true; }
                    if (chill >= needed) chill = needed - 1; // odporność po zamrożeniu
                    return false;
                }
                case StatusKind.Frozen:
                    if (Frozen || freezeImmunity > 0) return false;
                    Freeze(b);
                    return true;
                case StatusKind.Shock:
                    shockTime = b.shockDuration;
                    return false;
                case StatusKind.Stagger:
                {
                    if (Stunned) return false;
                    int needed = Mathf.Max(1, b.staggerStacksToStun);
                    stagger = Mathf.Min(StaggerStacks + 1, needed);
                    staggerTime = b.staggerDuration;
                    if (stagger >= needed) { stagger = 0; staggerTime = 0; return Stun(b); }
                    return false;
                }
                case StatusKind.Stun:
                    return Stun(b);
                case StatusKind.Sunder:
                    sunder = Mathf.Min(SunderStacks + 1, Mathf.Max(1, b.sunderMaxStacks));
                    sunderTime = b.sunderDuration;
                    return false;
                case StatusKind.Soaked:
                    soakedTime = Mathf.Max(soakedTime, b.soakedDuration);
                    return false;
                case StatusKind.Slow:
                    slowTime = Mathf.Max(slowTime, b.slowDuration * ControlMultiplier);
                    return false;
            }
            return false;
        }

        /// <summary>Powalenie (Odtrącenie HU): ogłuszenie na podany czas, z odpornością na kontrolę; bez progu zachwiania.</summary>
        public bool KnockDown(float seconds, BalanceConfig b)
        {
            if (Stunned || stunImmunity > 0) return false;
            stunTime = seconds * ControlMultiplier;
            stunImmunity = stunTime + b.stunImmunity;
            return true;
        }

        /// <summary>Ogłuszenie (jak zamrożenie bez lodu). Zwraca true, jeśli cel właśnie został ogłuszony.</summary>
        bool Stun(BalanceConfig b)
        {
            if (Stunned || stunImmunity > 0) return false;
            stunTime = b.stunDuration * ControlMultiplier;
            stunImmunity = stunTime + b.stunImmunity;
            return true;
        }

        void Freeze(BalanceConfig b)
        {
            frozenTime = b.freezeDuration * ControlMultiplier;
            freezeImmunity = frozenTime + b.freezeImmunity;
            chill = 0; chillTime = 0;
        }

        /// <summary>Upływ czasu. Zwraca obrażenia do zadania w tej klatce (wypłacane co statusTickInterval).</summary>
        public float Tick(float dt, bool moving, BalanceConfig b)
        {
            if (dt <= 0) return 0f;
            clock += dt; webTime = Mathf.Max(0, webTime - dt);
            for (int i = bleeds.Count - 1; i >= 0; i--)
            {
                var st = bleeds[i];
                float step = Mathf.Min(dt, st.remaining);
                pending += st.dps * step * (moving ? (st.movingMult > 0 ? st.movingMult : b.bleedMovingMultiplier) : 1f);
                st.remaining -= dt;
                if (st.remaining <= 0) bleeds.RemoveAt(i); else bleeds[i] = st;
            }
            if (burnTime > 0) { pending += burnDps * Mathf.Min(dt, burnTime); burnTime -= dt; }
            if (chillTime > 0) { chillTime -= dt; if (chillTime <= 0) chill = 0; }
            if (frozenTime > 0) frozenTime -= dt;
            if (freezeImmunity > 0) freezeImmunity -= dt;
            if (shockTime > 0) shockTime -= dt;
            if (staggerTime > 0) { staggerTime -= dt; if (staggerTime <= 0) stagger = 0; }
            if (stunTime > 0) stunTime -= dt;
            if (stunImmunity > 0) stunImmunity -= dt;
            if (sunderTime > 0) { sunderTime -= dt; if (sunderTime <= 0) sunder = 0; }
            if (soakedTime > 0) soakedTime -= dt;
            if (slowTime > 0) slowTime -= dt;

            tickTimer -= dt;
            bool flush = tickTimer <= 0 || (bleeds.Count == 0 && !Burning);
            if (!flush || pending <= 0) return 0f;
            tickTimer = Mathf.Max(0.05f, b.statusTickInterval);
            float outDmg = pending;
            pending = 0f;
            return outDmg;
        }

        /// <summary>Rozdarcie ran: zwraca całe pozostałe krwawienie i usuwa warstwy.</summary>
        public float ConsumeBleed()
        {
            float rest = BleedRemainingDamage;
            bleeds.Clear();
            return rest;
        }

        public void Clear()
        {
            bleeds.Clear();
            burnTime = 0; chill = 0; chillTime = 0; frozenTime = 0; freezeImmunity = 0; shockTime = 0; pending = 0;
            stagger = 0; staggerTime = 0; stunTime = 0; stunImmunity = 0; sunder = 0; sunderTime = 0;
            soakedTime = 0; slowTime = 0;
            System.Array.Clear(reactionReadyAt, 0, reactionReadyAt.Length);
            ExternalSlow = 0;
            clock = webTime = 0; rootUntil = float.NegativeInfinity;
        }

        /// <summary>Opis aktywnych efektów do HUD.</summary>
        public List<(StatusKind kind, string text)> Describe()
        {
            var l = new List<(StatusKind, string)>();
            if (Frozen) l.Add((StatusKind.Frozen, "ZAMROŻONY"));
            if (bleeds.Count > 0) l.Add((StatusKind.Bleed, bleeds.Count > 1 ? $"krwawienie ×{bleeds.Count}" : "krwawienie"));
            if (Burning) l.Add((StatusKind.Burn, "płonie"));
            if (ChillStacks > 0) l.Add((StatusKind.Chill, ChillStacks > 1 ? $"chłód ×{ChillStacks}" : "chłód"));
            if (Shocked) l.Add((StatusKind.Shock, "porażony"));
            if (Stunned) l.Add((StatusKind.Stun, "OGŁUSZONY"));
            if (StaggerStacks > 0) l.Add((StatusKind.Stagger, StaggerStacks > 1 ? $"zachwianie ×{StaggerStacks}" : "zachwianie"));
            if (SunderStacks > 0) l.Add((StatusKind.Sunder, SunderStacks > 1 ? $"pęknięty pancerz ×{SunderStacks}" : "pęknięty pancerz"));
            if (Soaked) l.Add((StatusKind.Soaked, "przemoczony"));
            if (Slowed) l.Add((StatusKind.Slow, "spowolniony"));
            return l;
        }
    }
}
