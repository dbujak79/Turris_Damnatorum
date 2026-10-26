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
    ///
    /// Reakcje: ogień w wychłodzony cel = szok termiczny (premia do ognia, chłód znika);
    /// błyskawica w krwawiący cel = przewodzenie (reszta krwawienia od razu);
    /// błyskawica w zamrożony cel = roztrzaskanie (wybuch wokół celu, lód pęka);
    /// ziemia w płonący cel = wybuch żaru (dodatkowy ogień); ziemia w wychłodzony cel = błoto (ogłuszenie, chłód znika).
    /// Tyknięcia i obrażenia reakcji nie wywołują reakcji
    /// i przechodzą przez pancerz tylko raz – liczone są z obrażeń, które już wylądowały.
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
        float tickTimer, pending;
        float rootClock, rootUntil = float.NegativeInfinity, webTime;
        public bool Rooted => rootClock < rootUntil;
        public bool Webbed => webTime > 0;
        public float RootRemaining => Mathf.Max(0, rootUntil - rootClock);
        /// <summary>Kokon HU: ruch i unik stoją, akcje pozostają dostępne; 2 s odporności i pół czasu przy powtórce do 6 s.</summary>
        public float TryRoot(float seconds)
        {
            if (seconds <= 0 || Rooted || rootClock < rootUntil + 2) return 0;
            float held = Mathf.Min(3, seconds) * (rootClock < rootUntil + 6 ? 0.5f : 1);
            rootUntil = rootClock + held;
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

        static float M(float v) => v <= 0f ? 1f : v; // 0 w domyślnej strukturze = brak modyfikatora

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
        /// <summary>Cel nie może się ruszać ani działać (zamrożenie albo ogłuszenie).</summary>
        public bool Incapacitated => Frozen || Stunned;
        public bool Any => bleeds.Count > 0 || Burning || ChillStacks > 0 || Frozen || Shocked || StaggerStacks > 0 || Stunned || SunderStacks > 0 || Rooted || Webbed;

        /// <summary>Łączne obrażenia, które zostały jeszcze w warstwach krwawienia.</summary>
        public float BleedRemainingDamage
        {
            get { float s = 0; foreach (var b in bleeds) s += b.dps * b.remaining; return s; }
        }

        /// <summary>Spowolnienie spoza efektów (np. uczepione czerwie z Hell Unleashed) – część prędkości, sumowana.</summary>
        public float ExternalSlow;
        public float MoveMultiplier(BalanceConfig b) => Incapacitated || Rooted ? 0f : (Webbed ? 0.5f : 1f) * Mathf.Max(0.25f, 1f - ChillStacks * b.chillSlowPerStack - StaggerStacks * b.staggerSlow - ExternalSlow);
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
            if ((ChillStacks > 0 || Frozen) && hit.bonusVsChilled > 0) m *= 1f + hit.bonusVsChilled;
            hit.physical *= m * (1f + SunderStacks * b.sunderPerStack);
            hit.magic *= m;
            if (hit.isReaction) return null;
            if (hit.element == Element.Fire && hit.magic > 0 && ChillStacks > 0)
            {
                hit.magic *= 1f + b.thermalShockBonus;
                chill = 0; chillTime = 0;
                return "SZOK TERMICZNY";
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
            reaction = null; froze = false; shatterDamage = 0f; LastConductionDamage = 0f;
            if (hit.isStatusTick || !(r.outcome == HitOutcome.Hit || r.outcome == HitOutcome.GuardBroken) || r.healthDamage <= 0) return 0f;
            float total = hit.physical + hit.magic;
            float physShare = total > 0 ? hit.physical / total : 0f;
            float landedPhys = r.healthDamage * physShare, landedMagic = r.healthDamage - landedPhys;

            float instant = 0f;
            if (!hit.isReaction && hit.element == Element.Lightning && hit.magic > 0 && bleeds.Count > 0)
            {
                instant = BleedRemainingDamage * b.conductionMultiplier * M(hit.mods.conductionMult);
                LastConductionDamage = instant;
                bleeds.Clear();
                reaction = "PRZEWODZENIE";
            }
            if (!hit.isReaction && hit.element == Element.Lightning && hit.magic > 0 && Frozen)
            {
                shatterDamage = r.healthDamage * b.shatterShare;
                frozenTime = 0f;
                reaction = "ROZTRZASKANIE";
            }
            else if (Frozen && hit.bonusVsFrozen > 0) frozenTime = 0f; // broń obuchowa rozbija lód
            if (!hit.isReaction && hit.element == Element.Earth && hit.magic > 0 && reaction == null)
            {
                if (Burning)
                {
                    // Wybuch żaru: ziemia wzbija płonące odłamki – dodatkowy ogień od razu.
                    instant += landedMagic * b.emberBurstShare;
                    reaction = "WYBUCH ŻARU";
                }
                else if (ChillStacks > 0 && !Frozen)
                {
                    // Błoto: ziemia w wychłodzony cel ogłusza.
                    chill = 0; chillTime = 0;
                    if (Stun(b)) froze = true;
                    reaction = "BŁOTO";
                }
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
            rootClock += dt; webTime = Mathf.Max(0, webTime - dt);
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
            ExternalSlow = 0;
            rootClock = webTime = 0; rootUntil = float.NegativeInfinity;
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
            return l;
        }
    }
}
