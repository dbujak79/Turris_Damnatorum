using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    // ====================================================================== Przywoływacz A – Wcielenie

    /// <summary>
    /// Zużywa własnego sługę (deterministycznie: najbliższy punktowi przed graczem) i daje graczowi jego zestaw ataków na czas
    /// najwyżej pozostałego wkładu sługi: chochlik – ostrzał ognia, ogar – szybkie krwawiące ugryzienia, golem – uderzenie wokół.
    /// Atak formy bierze migawkę trafienia z przyzwania (jedno źródło skalowania). Życie, obrona i otrzymywane ciosy – gracza;
    /// w formie bez czarów, bloku i parowania. Model bohatera chowa się pod formą i wraca po końcu (bez leczenia).
    /// </summary>
    public sealed class IncarnationFeature : ClassFeatureController, ICharacterAnimSource
    {
        float active, attackReady, swing = -1f;
        ServantKind kind;
        HitData formHit;
        GameObject form;
        readonly List<Renderer> hidden = new List<Renderer>();
        bool heavySwing;
        public bool InForm => active > 0f;
        public ServantKind Kind => kind;
        public int FormAttacks { get; private set; }
        protected override void OnBind() { color = new Color(0.45f, 1f, 0.65f); }

        protected override bool AwaitingSecondPress => InForm;

        protected override void OnPress()
        {
            if (InForm) { End("Koniec wcielenia"); return; }
            Vector3 probe = pc.transform.position + pc.FeatureAim() * 2f;
            var s = Servant.Active.Where(x => x != null && !x.IsChimera).OrderBy(x => HuWorld.Flat(x.transform.position, probe)).FirstOrDefault();
            if (s == null) { Fail("brak sługi", "Wcielenie: brak własnego sługi"); return; }
            kind = s.Kind;
            formHit = s.SnapshotHit();
            active = Mathf.Min(T.incarnationSeconds, s.Remaining);
            s.Dismiss(); // sługa naprawdę znika z armii i z Więzi dusz
            BuildForm();
            Telemetry.Add("wcielenie: formy " + ClassFeatureCatalog.ServantName(kind));
            Used(T.incarnationCooldown, "WCIELENIE: " + ClassFeatureCatalog.ServantName(kind).ToUpper());
        }

        void BuildForm()
        {
            ClearForm();
            if (!Application.isPlaying) return;
            var hero = pc.GetComponent<CharacterVisual>();
            foreach (var r in pc.GetComponentsInChildren<Renderer>())
                if (r.enabled && !(r is ParticleSystemRenderer) && !(r is TrailRenderer) && !(r is LineRenderer)) { r.enabled = false; hidden.Add(r); }
            form = new GameObject("IncarnationForm");
            form.transform.SetParent(pc.transform, false);
            var cv = form.AddComponent<CharacterVisual>();
            cv.BuildProcedural(Servant.LookFor(kind), kind == ServantKind.Golem ? 1.3f : kind == ServantKind.Imp ? 0.95f : 1.05f);
            cv.Init(this);
            cv.SetTint(new Color(0.45f, 1f, 0.65f), 0.35f);
            HellFx.Shockwave(pc.transform.position, HellFxElement.Soul, color, 1.8f, true);
        }

        void ClearForm()
        {
            foreach (var r in hidden) if (r != null) r.enabled = true;
            hidden.Clear();
            if (form != null) Util.DestroySafe(form);
            form = null;
        }

        void End(string message)
        {
            if (!InForm && form == null) return;
            active = 0f;
            ClearForm();
            if (message != null && pc != null && !pc.IsDead) CombatEvents.RaiseMessage(message, color);
        }

        public override bool InterceptAttack(ActionType type)
        {
            if (!InForm) return false;
            if (pc.Clock < attackReady || pc.Actions.Current != ActionType.None && pc.Actions.Current != ActionType.Block) return true;
            bool heavy = type == ActionType.HeavyAttack;
            float stamina = heavy ? 16f : 8f;
            if (pc.Stamina.Current <= 0f) return true;
            pc.Stamina.Drain(stamina, pc.Config.balance.staminaRegenDelay);
            var h = formHit;
            float mult = heavy ? 2f : 1f;
            h.physical *= mult; h.magic *= mult; h.attacker = pc; h.featureEffect = false; h.originId = pc.NewOrigin();
            if (kind == ServantKind.Hound) h.statuses = new List<StatusApplication> { new StatusApplication(StatusKind.Bleed, heavy ? 2 : 1) };
            if (kind == ServantKind.Golem && heavy) { h.knockdown = 1f; h.knockback = 1.5f; }
            var target = pc.FeatureTarget(kind == ServantKind.Imp ? 14f : 3f);
            if (target != null) { Vector3 d = target.transform.position - pc.transform.position; d.y = 0; if (d.sqrMagnitude > 0.01f) pc.transform.rotation = Quaternion.LookRotation(d); }
            Servant.Deliver(pc, kind, h, pc.transform, target, kind == ServantKind.Hound ? 1.6f : kind == ServantKind.Golem ? 1.9f : 7f, color, heavy ? 1.6f : 1.2f);
            attackReady = pc.Clock + (kind == ServantKind.Hound ? 0.35f : kind == ServantKind.Imp ? 0.45f : 0.9f) * (heavy ? 1.8f : 1f);
            swing = 0f; heavySwing = heavy;
            FormAttacks++;
            Telemetry.Add("wcielenie: ataki formy");
            return true;
        }

        public override bool BlocksGuard => InForm;
        public override string BlockCast(SpellRuntime skill, int slot) => InForm ? "Wcielenie: w formie nie da się rzucać skilli" : null;

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (swing >= 0f) { swing += dt; if (swing > 0.45f) swing = -1f; }
            if (!InForm) return;
            active -= dt;
            if (active <= 0f) End("Koniec wcielenia");
        }

        public CharacterAnimState GetAnimState()
        {
            if (swing < 0f) return new CharacterAnimState { action = pc.MoveIntent.sqrMagnitude > 0.01f ? AnimAction.None : AnimAction.None };
            float k = swing / 0.45f;
            var phase = k < 0.4f ? ActionPhase.Startup : k < 0.6f ? ActionPhase.Active : ActionPhase.Recovery;
            return new CharacterAnimState { action = AnimAction.Attack, phase = phase, phaseProgress = Mathf.Repeat(k * 2.5f, 1f), actionTime = swing, actionDuration = 0.45f,
                attack = kind == ServantKind.Imp ? AttackAnim.Cast : kind == ServantKind.Hound ? AttackAnim.Claw : AttackAnim.Slam };
        }

        public override void ForcedInterrupt() { swing = -1f; }
        public override void FloorStart() { base.FloorStart(); End(null); }
        public override void Death() { End(null); }
        public override ClassFeatureHud Hud => InForm ? Timed(ClassFeatureState.Active, $"forma: {ClassFeatureCatalog.ServantName(kind)} {active:0.0} s", active, T.incarnationSeconds)
            : Servant.Active.Count == 0 && Cooldown <= 0f ? new ClassFeatureHud { state = ClassFeatureState.Blocked, label = "brak sługi", color = color * 0.6f } : ReadyOrCooldown();
    }

    // ====================================================================== Przywoływacz B – Chimera legionu

    /// <summary>
    /// Pierwsze G pokazuje składniki (najbliżsi słudzy w promieniu, z istniejącą chimerą włącznie), drugie – scala je w jedną
    /// istotę: scaleni znikają, każdy wkład zachowuje własny pozostały czas (bez odnawiania), chimera rezerwuje tyle miejsc w limicie,
    /// ile żywych składników, a Więź dusz liczy ją raz.
    /// </summary>
    public sealed class ChimeraFeature : ClassFeatureController
    {
        float preview;
        readonly List<Servant> picked = new List<Servant>();
        public bool Previewing => preview > 0f;
        protected override bool AwaitingSecondPress => Previewing;
        protected override void OnBind() { color = new Color(0.75f, 1f, 0.45f); }

        List<Servant> Candidates()
        {
            var list = Servant.Active.Where(s => s != null && HuWorld.Flat(s.transform.position, pc.transform.position) <= T.chimeraRadius)
                .OrderByDescending(s => s.IsChimera).ThenBy(s => HuWorld.Flat(s.transform.position, pc.transform.position)).ToList();
            var outList = new List<Servant>(); int parts = 0;
            foreach (var s in list) { if (parts + s.Slots > T.chimeraMaxParts) continue; outList.Add(s); parts += s.Slots; }
            return outList;
        }

        public static string Describe(IEnumerable<ServantKind> kinds) => string.Join(" + ", kinds.Select(ClassFeatureCatalog.ServantName));

        protected override void OnPress()
        {
            if (!Previewing)
            {
                var c = Candidates();
                int parts = c.Sum(s => s.Slots);
                if (parts < 2 || (c.Count == 1 && c[0].IsChimera && parts >= T.chimeraMaxParts)) { Fail("za mało sług", "Chimera: potrzeba co najmniej dwóch własnych sług w pobliżu"); return; }
                picked.Clear(); picked.AddRange(c);
                preview = T.chimeraPreview;
                foreach (var s in picked) FxLibrary.Flash(s.transform.position + Vector3.up, color, 1f);
                CombatEvents.RaiseMessage("Chimera: " + Describe(picked.SelectMany(s => s.ToParts()).Select(p => p.kind)) + $" – {pcKey()} scala", color);
                return;
            }
            preview = 0f;
            picked.RemoveAll(s => s == null || !Servant.Active.Contains(s));
            var all = picked.SelectMany(s => s.ToParts()).Where(p => p.remaining > 0.1f).ToList();
            if (all.Count < 2) { Fail("za mało sług", "Chimera: składniki zniknęły"); picked.Clear(); return; }
            Vector3 at = picked[0].transform.position;
            foreach (var s in picked) s.Dismiss();
            picked.Clear();
            var ch = Servant.SpawnChimera(pc, all, at, color);
            Telemetry.Add("chimera: scalenia");
            Telemetry.Add("chimera: składniki", all.Count);
            Used(T.chimeraCooldown, "CHIMERA: " + Describe(all.Select(p => p.kind)).ToUpper());
        }

        string pcKey() => "przycisk klasy";

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (preview > 0f) preview -= dt;
        }

        public override void FloorStart() { base.FloorStart(); preview = 0f; picked.Clear(); }
        public override void Death() { preview = 0f; picked.Clear(); }
        public override ClassFeatureHud Hud
        {
            get
            {
                if (Previewing) return Timed(ClassFeatureState.SecondPhase, "scal: " + Describe(picked.Where(s => s != null).SelectMany(s => s.ToParts()).Select(p => p.kind)), preview, T.chimeraPreview);
                var ch = Servant.Active.FirstOrDefault(s => s != null && s.IsChimera);
                if (ch != null && Cooldown > 0f) return new ClassFeatureHud { state = ClassFeatureState.Active, label = "chimera: " + Describe(ch.Parts.Select(p => p.kind)) + $" {ch.Remaining:0} s", fill = Cooldown > 0 ? 1f - Cooldown / cooldownLength : 1f, color = color };
                return ReadyOrCooldown();
            }
        }
    }

    // ====================================================================== Mag bitewny A – Arkana oręża

    /// <summary>
    /// Okno, w którym broń wręcz Maga bitewnego jest fokusem, a jej główny atrybut (Siła albo Zręczność wg skalowania broni) liczy się
    /// za Inteligencję czarów (BuildCalculator). Koszt, odnowienie i czas rzutu bez zmian; rzut zaczęty w oknie kończy się na
    /// warunkach startu (koniec okna czeka na wypuszczenie trwającego czaru).
    /// </summary>
    public sealed class WeaponArcanaFeature : ClassFeatureController
    {
        float active;
        bool pendingEnd;
        public bool Active => Run != null && Run.weaponArcanaActive;
        protected override void OnBind() { color = new Color(0.5f, 0.6f, 1f); }

        protected override void OnPress()
        {
            if (Active) { Fail("już działa", "Arkana oręża już trwają"); return; }
            var main = Run.equipment.Get(EquipSlot.MainHand);
            var fam = main != null ? WeaponFamilies.Of(main.definition) : BuildTag.None;
            if ((fam & (BuildTag.Blade | BuildTag.HeavyWeapon)) == 0 || (fam & BuildTag.Ranged) != 0) { Fail("brak broni wręcz", "Arkana oręża: potrzebna broń do walki wręcz"); return; }
            Run.weaponArcanaActive = true;
            active = T.arcanaSeconds;
            pc.FeatureRefresh();
            Used(T.arcanaCooldown, "ARKANA ORĘŻA");
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Soul, 0.5f, T.arcanaSeconds);
        }

        public override void CastFired(SpellRuntime skill, int origin)
        {
            if (skill != null && skill.byArcana) Telemetry.Add("arkana: czary bez fokusu");
            if (pendingEnd) Stop();
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (!Active) return;
            active -= dt;
            if (active > 0f) return;
            // Rzut zaczęty w oknie kończy się z warunkami startu.
            if (pc.Actions.Current == ActionType.Cast && pc.Actions.Phase == ActionPhase.Startup) { pendingEnd = true; return; }
            Stop();
        }

        void Stop()
        {
            pendingEnd = false; active = 0f;
            if (Run == null || !Run.weaponArcanaActive) return;
            Run.weaponArcanaActive = false;
            if (pc != null && !pc.IsDead) pc.FeatureRefresh();
        }

        public override void ForcedInterrupt() { if (pendingEnd) Stop(); }
        public override void FloorStart() { base.FloorStart(); Stop(); }
        public override void Death() { Stop(); }
        public override ClassFeatureHud Hud => Active ? Timed(ClassFeatureState.Active, $"arkana ({pc.Build.arcanaAttribute?.ToString() ?? "-"}) {Mathf.Max(0f, active):0.0} s", active, T.arcanaSeconds) : ReadyOrCooldown();
    }

    // ====================================================================== Mag bitewny B – Dwa takty

    /// <summary>
    /// W oknie czar ze slotu idzie osobnym torem z własnym czasem rzutu (koszt, odnowienie i walidacja jak zwykle), a ciało zostaje
    /// wolne dla broni – jedna akcja bronią i jeden czar naraz. Orientacja: ciało prowadzi broń; czar celuje w namierzony cel
    /// (albo przód postaci). Przerwanie przed wyzwoleniem oddaje to, co pobrano za czar – nie dotyka zasobów broni.
    /// </summary>
    public sealed class TwoBeatsFeature : ClassFeatureController
    {
        float active;
        SpellRuntime lane;
        float laneTime;
        float laneMana; bool laneFreeCast, laneLife;
        CastReceipt laneReceipt;
        HellParticles gather;
        public bool Active => active > 0f;
        public bool LaneBusy => lane != null;
        public int Overlaps { get; private set; }
        protected override void OnBind() { color = new Color(0.55f, 0.65f, 1f); }

        protected override void OnPress()
        {
            if (Active) { Fail("już działa", "Dwa takty już trwają"); return; }
            active = T.beatsSeconds;
            Used(T.beatsCooldown, "DWA TAKTY");
            HellFx.Ambient(pc.transform, Vector3.up, HellFxElement.Air, 0.45f, T.beatsSeconds);
        }

        public override bool RoutesCastToLane(SpellRuntime skill, int slot) => Active && lane == null && PlayerCombat.LaneCompatible(skill);
        public override string BlockCast(SpellRuntime skill, int slot) => Active && lane != null && skill != null && skill.Def.IsSpell ? "Dwa takty: czar już w drodze" : null;

        public void QueueLane(SpellRuntime skill, float castTime, CastReceipt receipt, bool freeCast, bool lifeCost)
        {
            lane = skill; laneTime = Mathf.Max(0.05f, castTime); laneReceipt = receipt; laneFreeCast = freeCast; laneLife = lifeCost;
            laneMana = !receipt.active && !freeCast && !lifeCost && skill.Def.IsSpell ? skill.manaCost : 0f;
            var hand = pc.GetComponent<CharacterVisual>();
            gather = HellFx.Gather(hand != null && hand.RightHand != null ? hand.RightHand : pc.transform, HellSkillFx.Family(skill.Def), laneTime);
            Telemetry.Add("dwa takty: czary w torze");
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (active > 0f) active -= dt;
            if (lane == null) return;
            laneTime -= dt * pc.Status.ActionSpeedMultiplier(pc.Config.balance);
            if (laneTime > 0f) return;
            var sk = lane; lane = null; gather = null;
            var cur = pc.Actions.Current;
            if (cur == ActionType.LightAttack || cur == ActionType.HeavyAttack) { Overlaps++; Telemetry.Add("dwa takty: nakładanie z bronią"); }
            pc.FireLaneSpell(sk);
        }

        public override void ForcedInterrupt() => CancelLane();
        void CancelLane()
        {
            if (lane == null) return;
            // Zwrot tylko kosztu czaru (mana/żeton/dług) i ładunku – zasoby broni zostają nietknięte.
            if (laneReceipt.active) pc.Feature?.Refund(laneReceipt);
            else if (laneMana > 0f) pc.Mana.Restore(laneMana);
            lane.instance.Refund();
            if (gather != null) gather.StopAndRelease();
            gather = null; lane = null;
            Telemetry.Add("dwa takty: przerwane czary");
        }

        public override void FloorStart() { base.FloorStart(); active = 0f; CancelLane(); }
        public override void Death() { active = 0f; if (gather != null) gather.StopAndRelease(); gather = null; lane = null; }
        public override ClassFeatureHud Hud => Active ? Timed(lane != null ? ClassFeatureState.SecondPhase : ClassFeatureState.Active, lane != null ? $"czar w torze: {lane.Def.displayName}" : $"dwa takty {active:0.0} s", active, T.beatsSeconds) : ReadyOrCooldown();
    }
}
