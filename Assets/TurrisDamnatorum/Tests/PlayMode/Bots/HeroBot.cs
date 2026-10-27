using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris.Tests.Bots
{
    /// <summary>
    /// Wspólny „mózg” bota grającego bohaterem. Steruje wyłącznie tym, co ma gracz: ruch, garda, bieg
    /// (<see cref="IPlayerDriver"/>), namierzanie i przyciski akcji. Widzi to, co widzi człowiek: stan wrogów,
    /// kolor telegrafu i początek zamachu – reaguje z opóźnieniem i błędem wyczucia zależnymi od osobowości.
    /// Klasy pochodne (np. <see cref="WarriorBot"/>) dokładają wybór zestawu, rozwój cech i użycie skilli klasy.
    /// </summary>
    public abstract class HeroBot : MonoBehaviour, IPlayerDriver
    {
        public GameRoot Root { get; private set; }
        public PersonaProfile P { get; private set; }
        public BotRunRecorder Rec { get; private set; }
        protected System.Random rng = new System.Random(1);

        protected PlayerCombat Pc => Root.Player;
        protected PlayerController Ctrl => Root.Controller;

        public Vector3 MoveWorld { get; protected set; }
        public bool BlockHeld { get; protected set; }
        public bool SprintHeld { get; protected set; }

        /// <summary>Zaobserwowany zamach wroga.</summary>
        protected class Swing
        {
            public EnemyBrain enemy;
            public AttackDefinition attack;
            public float start, noticeAt, error;
            /// <summary>Szacowana chwila trafienia (ustalana przy zauważeniu; dla pocisku – z lotem).</summary>
            public float impactAt;
            public bool decided, executed;
            public DefResponse response;
            public bool experiment;
        }

        readonly Dictionary<EnemyBrain, (EnemyBrain.State state, ActionPhase phase)> lastSeen = new Dictionary<EnemyBrain, (EnemyBrain.State, ActionPhase)>();
        protected readonly List<Swing> swings = new List<Swing>();
        protected readonly List<EnemyBrain> alive = new List<EnemyBrain>();
        protected EnemyBrain target;
        protected float now;

        float blockUntil, evacuateUntil;
        bool seekingDormant;
        Vector3 evacuateDir;
        bool pendingDodge;
        Vector3 pendingDodgeDir;
        Swing blockFor;

        float stuckT;
        Vector3 stuckPos;
        float unstickUntil;
        Vector3 unstickDir;
        float strafeSign = 1f, strafeFlipAt;
        int comboLeft;
        float backOffUntil, nextOffenseAt, opportunityRollAt;
        bool opportunity;
        int attackCounter;
        ActionType lastAction = ActionType.None;
        Vector3 lastPos;
        int experimentCursor;
        float lastWall;

        // ------------------------------------------------------------------ Cykl życia

        /// <summary>Raz na serię: gra i osobowość (przed wyborem zestawu i zakupami).</summary>
        public void Init(GameRoot root, PersonaProfile persona, int seed)
        {
            Root = root;
            P = persona;
            rng = new System.Random(seed);
        }

        /// <summary>Początek podejścia: telemetria i przejęcie sterowania.</summary>
        public void Begin(BotRunRecorder rec, int seed)
        {
            Rec = rec;
            rec.Bot = this;
            rng = new System.Random(seed);
            swings.Clear();
            lastSeen.Clear();
            target = null;
            blockUntil = evacuateUntil = unstickUntil = backOffUntil = 0f;
            pendingDodge = false;
            dodgeExpectedUntil = 0f; nextTacticalAt = 0f;
            shots.Clear(); shotResponse.Clear();
            refused.Clear();
            slotsFixed = false;
            lastAction = ActionType.None;
            Root.Controller.Driver = this;
            lastWall = Time.realtimeSinceStartup;
        }

        public void End()
        {
            if (Root != null && Root.Controller != null && ReferenceEquals(Root.Controller.Driver, this)) Root.Controller.Driver = null;
            if (Rec != null) Rec.Bot = null;
            Rec = null;
            MoveWorld = Vector3.zero;
            BlockHeld = SprintHeld = false;
        }

        void OnDestroy() => End();

        void Update()
        {
            float wall = Time.realtimeSinceStartup;
            float wallMs = (wall - lastWall) * 1000f;
            lastWall = wall;
            if (Root == null || Rec == null || Root.Screen != GameScreen.Playing || Pc == null || Pc.IsDead)
            {
                MoveWorld = Vector3.zero; BlockHeld = SprintHeld = false;
                return;
            }
            now = Time.time;
            Rec.Frame(wallMs);
            if (!slotsFixed) { slotsFixed = true; FixSlots(); }
            TrackSelf();
            Perceive();
            Think();
        }

        // ------------------------------------------------------------------ Własny stan (telemetria akcji)

        void TrackSelf()
        {
            var cur = Pc.Actions.Current;
            if (cur != lastAction)
            {
                if (cur == ActionType.Cast && Pc.ActiveSkill != null)
                {
                    string n = Pc.ActiveSkill.Def.displayName;
                    Rec.R.skillUses.Add(n);
                    Rec.R.actions.Add("umiejętność");
                }
                else if (cur == ActionType.Flask) { Rec.R.actions.Add("flaszka (próba)"); Rec.FlaskAttempt(); }
                else if (cur != ActionType.None) Rec.R.actions.Add(ActionName(cur));
                if (cur == ActionType.Dodge) dodgeExpectedUntil = 0f;
                lastAction = cur;
            }
            if (cur == ActionType.Block) Rec.R.timeBlocking += Time.deltaTime;
            if (Pc.Sprinting) Rec.R.timeSprinting += Time.deltaTime;
            Vector3 p = transform.position;
            Rec.R.distanceMoved += Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(lastPos.x, 0, lastPos.z)) < 3f ? Vector3.Distance(p, lastPos) : 0f;
            lastPos = p;
            if (p.y < -6f || float.IsNaN(p.x)) Rec.R.Note("bohater poza światem", $"pozycja {p} na piętrze {Root.Run.floorIndex + 1}");
        }

        public static string ActionName(ActionType a)
        {
            switch (a)
            {
                case ActionType.LightAttack: return "szybki atak";
                case ActionType.HeavyAttack: return "mocny atak";
                case ActionType.Riposte: return "riposta";
                case ActionType.Block: return "blok";
                case ActionType.Parry: return "parowanie";
                case ActionType.Dodge: return "unik";
                case ActionType.Flinch: return "drgnięcie (trafiony)";
                case ActionType.GuardBroken: return "przełamana garda";
                case ActionType.Dead: return "śmierć";
                case ActionType.Flask: return "picie flaszki";
                case ActionType.Cast: return "umiejętność";
                default: return a.ToString();
            }
        }

        // ------------------------------------------------------------------ Percepcja

        void Perceive()
        {
            alive.Clear();
            foreach (var e in EnemyBrain.All)
                if (e != null && !e.IsDead && e.isActiveAndEnabled && e.CurrentState != EnemyBrain.State.Dormant) alive.Add(e);

            foreach (var e in alive)
            {
                var st = (e.CurrentState, e.AttackPhase);
                lastSeen.TryGetValue(e, out var prev);
                bool attacking = e.CurrentState == EnemyBrain.State.Attacking && e.CurrentAttack != null;
                bool fresh = attacking && e.AttackPhase == ActionPhase.Startup &&
                             (prev.state != EnemyBrain.State.Attacking || prev.phase != ActionPhase.Startup);
                if (fresh)
                {
                    swings.RemoveAll(s => s.enemy == e);
                    swings.Add(new Swing
                    {
                        enemy = e, attack = e.CurrentAttack.attack, start = now,
                        noticeAt = now + P.reaction + (float)(rng.NextDouble() * 2 - 1) * P.reactionJitter,
                        error = Gauss() * P.timingError,
                    });
                    var sw = swings[swings.Count - 1];
                    string an = (e.Def != null ? e.Def.displayName : "?") + ": " + sw.attack.name;
                    Rec.R.windupSum.Add(an, e.AttackWindupDuration);
                    Rec.R.windupN.Add(an);
                    sw.impactAt = now + e.AttackWindupDuration + (sw.attack.delivery == AttackDelivery.Projectile ? FlatDist(e) / ProjectileSpeed : 0f);
                }
                lastSeen[e] = st;
            }
            // Pocisk leci dalej po zakończeniu zamachu – taki zamach pamiętamy do szacowanego trafienia.
            swings.RemoveAll(s => s.enemy == null || now - s.start > 6f ||
                                  (s.attack.delivery == AttackDelivery.Projectile ? now > s.impactAt + 0.5f
                                   : s.enemy.IsDead || s.enemy.CurrentState != EnemyBrain.State.Attacking));
            if (lastSeen.Count > 64) foreach (var k in lastSeen.Keys.Where(x => x == null || x.IsDead).ToList()) lastSeen.Remove(k);

            // Cel: najbliższy, z histerezą – zmiana dopiero, gdy inny jest wyraźnie bliżej.
            EnemyBrain nearest = null;
            float best = float.MaxValue;
            foreach (var e in alive)
            {
                float d = FlatDist(e);
                if (d < best) { best = d; nearest = e; }
            }
            if (target == null || target.IsDead || !alive.Contains(target) || (nearest != null && FlatDist(nearest) + 2f < FlatDist(target)))
                target = nearest;
            // Arena pusta, a piętro trwa: uśpieni wrogowie (np. gargulec-posąg budzi się dopiero z bliska) – podejść do najbliższego.
            if (target == null)
            {
                float bd = float.MaxValue;
                foreach (var e in EnemyBrain.All)
                    if (e != null && !e.IsDead && e.isActiveAndEnabled && e.CurrentState == EnemyBrain.State.Dormant && Root.IncomingEnemies == 0 && FlatDist(e) < bd)
                    { bd = FlatDist(e); target = e; }
                if (target != null && !seekingDormant && target.Def.huWakeRadius > 0f) { seekingDormant = true; Rec.R.Note("uśpiony wróg blokuje koniec piętra", $"piętro {Root.Run.floorIndex + 1} „{Root.CurrentFloor?.name}”: {target.Def.displayName} czeka uśpiony – trzeba do niego podejść"); }
            }
            else seekingDormant = false;
            if (target != null && FlatDist(target) < 22f) { if (Ctrl.LockTarget != target) Ctrl.LockOn(target); }
            else if (Ctrl.LockTarget != null && target == null) Ctrl.LockOn(null);
        }

        protected float FlatDist(EnemyBrain e)
        {
            Vector3 d = e.transform.position - transform.position; d.y = 0f;
            return Mathf.Max(0f, d.magnitude - 0.35f * (e.Def != null ? e.Def.scale : 1f));
        }

        protected Vector3 DirTo(Vector3 p) { Vector3 d = p - transform.position; d.y = 0f; return d.sqrMagnitude > 1e-4f ? d.normalized : transform.forward; }

        /// <summary>Czy ten zamach może dosięgnąć bohatera (zasięg i rodzaj ataku).</summary>
        protected bool Threatens(Swing s)
        {
            float d = FlatDist(s.enemy);
            var a = s.attack;
            switch (a.delivery)
            {
                case AttackDelivery.AreaAroundSelf: return d <= a.radius + 0.9f;
                case AttackDelivery.Projectile: return d <= 24f;
                default: return d <= a.reach + a.lunge + a.radius + 0.9f;
            }
        }

        /// <summary>Szacowany czas do trafienia (s): do fazy aktywnej, a dla pocisku plus lot.</summary>
        protected float TimeToImpact(Swing s)
        {
            if (s.attack.delivery == AttackDelivery.Projectile || s.enemy == null || s.enemy.CurrentState != EnemyBrain.State.Attacking) return s.impactAt - now;
            return s.enemy.AttackWindupDuration - (now - s.start);
        }

        /// <summary>Przybliżona prędkość pocisków wrogów (m/s) – tyle ocenia na oko gracz.</summary>
        protected const float ProjectileSpeed = 14f;

        protected int EnemiesWithin(float r) => alive.Count(e => FlatDist(e) <= r);

        protected Vector3 CrowdCenter(float r)
        {
            Vector3 c = Vector3.zero; int n = 0;
            foreach (var e in alive) if (FlatDist(e) <= r) { c += e.transform.position; n++; }
            return n > 0 ? c / n : transform.position + transform.forward;
        }

        // ------------------------------------------------------------------ Decyzje

        void Think()
        {
            MoveWorld = Vector3.zero;
            SprintHeld = false;

            if (pendingDodge)
            {
                // Kierunek uniku ustawiony klatkę wcześniej – teraz przycisk.
                MoveWorld = pendingDodgeDir;
                dodgeRequestedIn = Pc.Actions.Current;
                Pc.Request(ActionType.Dodge);
                pendingDodge = false;
                dodgeExpectedUntil = now + 0.2f;
                return;
            }
            if (dodgeExpectedUntil > 0f && now > dodgeExpectedUntil)
            {
                // Przycisk naciśnięty, a uniku nie było (brak wytrzymałości, unieruchomienie, akcja bez przerwania).
                dodgeExpectedUntil = 0f;
                Rec.R.dodgeReasons.Add(Pc.Stamina.Current <= 0f ? "odmowa: brak wytrzymałości"
                    : Pc.Status.Rooted ? "odmowa: unieruchomienie"
                    : "odmowa: " + (dodgeRequestedIn == ActionType.None ? "inna" : ActionName(dodgeRequestedIn)));
            }

            // Niskie życie: leczenie przed obroną, jeśli jest choć krótka luka (TryFlask sam ustępuje bliskiemu ciosowi).
            if (Pc.Health.Fraction < P.flaskThreshold && TryFlask()) { BlockHeld = false; return; }
            bool defending = ProjectileDefense() | Defend();
            BlockHeld = now < blockUntil;
            if (BlockHeld && blockFor != null && blockFor.attack.delivery != AttackDelivery.Projectile &&
                (blockFor.enemy == null || blockFor.enemy.IsDead || blockFor.enemy.CurrentState != EnemyBrain.State.Attacking || blockFor.enemy.AttackPhase == ActionPhase.Recovery))
            {
                blockUntil = Mathf.Min(blockUntil, now + 0.08f);
            }
            if (now < evacuateUntil) { MoveWorld = evacuateDir; SprintHeld = Pc.Stamina.Fraction > 0.2f; return; }
            if (defending) return;

            if (TryFlask()) return;
            if (now < unstickUntil) { MoveWorld = unstickDir; return; }

            if (target == null) { Wander(); CheckStuck(); return; }
            if (TryRiposte()) return;
            if (TryTacticalDodge()) return;
            OnFight();
            if (TryUseSkills()) return;
            Offense();
            CheckStuck();
        }

        /// <summary>Obsługa zagrożeń; true – w tej klatce bohater zajęty obroną.</summary>
        bool Defend()
        {
            Swing urgent = null;
            float soonest = float.MaxValue;
            foreach (var s in swings)
            {
                if (now < s.noticeAt || s.executed) continue;
                if (s.attack.delivery == AttackDelivery.Projectile) continue; // pociski – ProjectileDefense (widać lecący pocisk)
                if (!Threatens(s)) continue;
                float t = TimeToImpact(s);
                if (t < -0.25f) { s.executed = true; continue; }
                if (t < soonest) { soonest = t; urgent = s; }
            }
            if (urgent == null) return false;

            if (!urgent.decided)
            {
                urgent.decided = true;
                Rec.R.reactionMargin.Add($"{BotRunRecorder.KindName(urgent.attack.Telegraph)}|{MarginBucket(soonest)}");
                urgent.response = ChooseResponse(urgent, out urgent.experiment);
                if (urgent.response == DefResponse.Block) { blockFor = urgent; blockUntil = now + Mathf.Max(0.3f, soonest + urgent.attack.active + 0.25f); }
                if (urgent.response == DefResponse.StepOut)
                {
                    evacuateDir = AwayFrom(urgent.enemy.transform.position);
                    evacuateUntil = now + Mathf.Max(0.35f, soonest + urgent.attack.active + 0.1f);
                }
                if (urgent.response == DefResponse.None || urgent.response == DefResponse.Block || urgent.response == DefResponse.StepOut) urgent.executed = true;
            }
            if (urgent.executed) return urgent.response == DefResponse.Block || urgent.response == DefResponse.StepOut;

            float t2 = TimeToImpact(urgent);
            switch (urgent.response)
            {
                case DefResponse.Parry:
                {
                    var p = Pc.Build.parry;
                    if (p == null) { urgent.executed = true; return false; }
                    float lead = Pc.ScaleStartup(p.startup) + p.activeWindow * 0.45f + urgent.error;
                    // Bohater musi stać przodem do napastnika (okno parowania działa z przodu).
                    if (Ctrl.LockTarget != urgent.enemy) Ctrl.LockOn(urgent.enemy);
                    if (t2 <= lead) { Pc.Request(ActionType.Parry); urgent.executed = true; }
                    return true;
                }
                case DefResponse.Dodge:
                {
                    float lead = Pc.Build.dodge.invulnStart + 0.04f + 1f / 30f + urgent.error;
                    if (t2 <= lead)
                    {
                        pendingDodge = true;
                        pendingDodgeDir = DodgeDirection(urgent);
                        MoveWorld = pendingDodgeDir;
                        urgent.executed = true;
                        Rec.R.dodgeReasons.Add("reakcja na cios wręcz");
                    }
                    return true;
                }
            }
            return false;
        }

        public static readonly string[] MarginBuckets = { "spóźniony (<0 s)", "0–0,1 s", "0,1–0,2 s", "0,2–0,4 s", ">0,4 s" };
        static string MarginBucket(float t) => t < 0f ? MarginBuckets[0] : t < 0.1f ? MarginBuckets[1] : t < 0.2f ? MarginBuckets[2] : t < 0.4f ? MarginBuckets[3] : MarginBuckets[4];

        /// <summary>Kierunek uniku: w bok względem napastnika, lekko w tył; przy obszarowym – od środka.</summary>
        protected virtual Vector3 DodgeDirection(Swing s)
        {
            Vector3 to = DirTo(s.enemy.transform.position);
            if (s.attack.delivery == AttackDelivery.AreaAroundSelf) return -to;
            Vector3 side = Vector3.Cross(Vector3.up, to) * (rng.NextDouble() < 0.5 ? 1f : -1f);
            return (side * 0.8f - to * 0.45f).normalized;
        }

        protected Vector3 AwayFrom(Vector3 p) { Vector3 d = transform.position - p; d.y = 0; return d.sqrMagnitude > 1e-4f ? d.normalized : -transform.forward; }

        /// <summary>Wybór reakcji na zamach (po czasie reakcji). Tester część ciosów przyjmuje eksperymentalnie.</summary>
        protected virtual DefResponse ChooseResponse(Swing s, out bool experiment)
        {
            experiment = false;
            if (P.experimentRate > 0f && rng.NextDouble() < P.experimentRate)
            {
                experiment = true;
                var all = new[] { DefResponse.Block, DefResponse.Parry, DefResponse.Dodge, DefResponse.StepOut, DefResponse.None };
                var r = all[experimentCursor++ % all.Length];
                if (r == DefResponse.Parry && !Pc.Build.CanParry) r = DefResponse.Dodge;
                if (r == DefResponse.Block && !Pc.Build.CanBlock) r = DefResponse.StepOut;
                return r;
            }
            return BookResponse(s);
        }

        /// <summary>Reakcja „z podręcznika” zgodna z kolorem telegrafu.</summary>
        protected virtual DefResponse BookResponse(Swing s)
        {
            var a = s.attack;
            var b = Pc.Build;
            float stam = Pc.Stamina.Fraction;
            bool canDodge = Pc.Stamina.Current > 1f;
            bool shield = b.CanBlock && b.guardIsShield;
            switch (a.Telegraph)
            {
                case TelegraphKind.NoDodge:
                    return a.blockable && b.CanBlock && stam > 0.45f ? DefResponse.Block : DefResponse.StepOut;
                case TelegraphKind.Unblockable:
                    if (a.parryable && b.CanParry && rng.NextDouble() < P.parryChance * 0.35f) return DefResponse.Parry;
                    return canDodge ? DefResponse.Dodge : DefResponse.StepOut;
                case TelegraphKind.Heavy:
                    if (shield && stam > 0.6f && rng.NextDouble() < 0.35) return DefResponse.Block;
                    if (a.parryable && b.CanParry && rng.NextDouble() < P.parryChance * 0.5f) return DefResponse.Parry;
                    return canDodge ? DefResponse.Dodge : DefResponse.StepOut;
                case TelegraphKind.Unparryable:
                    if (b.CanBlock && stam > 0.35f) return DefResponse.Block;
                    return canDodge ? DefResponse.Dodge : DefResponse.StepOut;
                default:
                    if (a.delivery == AttackDelivery.Projectile)
                        return shield && stam > 0.25f ? DefResponse.Block : canDodge ? DefResponse.Dodge : DefResponse.StepOut;
                    if (b.CanParry && stam > 0.2f && rng.NextDouble() < P.parryChance) return DefResponse.Parry;
                    if (b.CanBlock && stam > 0.3f && (shield || rng.NextDouble() < 0.5)) return DefResponse.Block;
                    return canDodge ? DefResponse.Dodge : DefResponse.StepOut;
            }
        }

        /// <summary>Wynik ciosu, który dotarł do bohatera – zapis do macierzy reakcja × telegraf × wynik.</summary>
        public void OnPlayerImpact(EnemyBrain src, in HitData hit, in HitResult r, TelegraphKind kind)
        {
            var s = src != null ? swings.FirstOrDefault(x => x.enemy == src && x.decided && x.attack.delivery != AttackDelivery.Projectile) : null;
            string resp, kindName = BotRunRecorder.KindName(kind);
            if (s != null) resp = BotRunRecorder.ResponseName(s.response) + (s.experiment ? " (eksperyment)" : "");
            else if (src != null && shotResponse.TryGetValue(src, out var sr) && now - sr.t < 3f)
            {
                // Jedno trafienie na pocisk – w oknie dolotu; późniejsze od tego wroga to wybuch/płonąca ziemia po pocisku.
                if (!sr.used && Mathf.Abs(now - sr.impactAt) < 0.4f)
                {
                    sr.used = true;
                    resp = BotRunRecorder.ResponseName(sr.r) + (sr.exp ? " (eksperyment)" : "");
                    kindName = "pocisk";
                }
                else { resp = "strefa/wybuch po pocisku"; kindName = "pocisk"; }
            }
            else resp = "niezauważony";
            Rec.R.responses.Add($"{resp}|{kindName}|{BotRunRecorder.OutcomeName(r.outcome)}");
        }

        /// <summary>Złamana postawa wroga w pobliżu: dojść, obrócić się i nacisnąć szybki atak (riposta wychodzi sama).</summary>
        bool TryRiposte()
        {
            EnemyBrain broken = null;
            float bd = 5f;
            foreach (var e in alive) if (e.CanBeRiposted && FlatDist(e) < bd) { bd = FlatDist(e); broken = e; }
            if (broken == null) return false;
            if (Ctrl.LockTarget != broken) Ctrl.LockOn(broken);
            target = broken;
            float range = Mathf.Max(0.8f, Root.config.balance.riposteRange - 0.4f);
            Vector3 to = DirTo(broken.transform.position);
            if (bd > range) { MoveWorld = to; return true; }
            if (Vector3.Angle(transform.forward, to) < Root.config.balance.riposteAngle * 0.4f && (Pc.Actions.IsIdle || Pc.Actions.PastCancelPoint))
                Pc.Request(ActionType.LightAttack);
            return true;
        }

        /// <summary>Czar z twardym wymaganiem Inteligencji, której brak – gra i tak go odmówi.</summary>
        protected bool Unusable(SpellRuntime sk) =>
            sk != null && sk.Def.IsSpell && !sk.requirementsMet && Root.config.balance.spellRequirementsAreHard;

        readonly HashSet<SpellDefinition> refused = new HashSet<SpellDefinition>();

        // ------------------------------------------------------------------ Pociski i uniki taktyczne

        /// <summary>Lecący pocisk wroga, zauważony z ludzkim opóźnieniem.</summary>
        class Shot
        {
            public float noticeAt, error;
            public bool decided, done, experiment;
            public DefResponse response;
        }

        readonly Dictionary<Projectile, Shot> shots = new Dictionary<Projectile, Shot>();
        /// <summary>Reakcja na ostatni pocisk danego wroga: chwila decyzji, szacowany dolot i czy przypisano już trafienie.</summary>
        class ShotDecision { public DefResponse r; public bool exp, used; public float t, impactAt; }
        readonly Dictionary<EnemyBrain, ShotDecision> shotResponse = new Dictionary<EnemyBrain, ShotDecision>();
        float dodgeExpectedUntil, nextTacticalAt;
        ActionType dodgeRequestedIn;
        float bodyRadius => GetComponent<CharacterController>() is CharacterController cc ? cc.radius : 0.4f;

        /// <summary>
        /// Obrona przed pociskami: bot patrzy na prawdziwy pocisk (pozycja, prędkość) – jak gracz na ekranie – i liczy chwilę
        /// największego zbliżenia. Pocisk, który i tak minie, jest ignorowany. Unik w bok względem toru lotu.
        /// </summary>
        bool ProjectileDefense()
        {
            foreach (var p in Projectile.Active)
                if (p != null && p.Side == Faction.Enemy && !shots.ContainsKey(p))
                    shots[p] = new Shot { noticeAt = now + P.reaction + (float)(rng.NextDouble() * 2 - 1) * P.reactionJitter, error = Gauss() * P.timingError };
            if (shots.Count > 0) foreach (var k in shots.Keys.Where(k => k == null || !k.isActiveAndEnabled).ToList()) shots.Remove(k);

            Projectile urgentP = null; Shot urgent = null; float soonest = float.MaxValue;
            Vector3 me = transform.position; me.y = 0f;
            foreach (var kv in shots)
            {
                var sh = kv.Value;
                if (sh.done || now < sh.noticeAt) continue;
                Vector3 rel = kv.Key.transform.position; rel.y = 0f; rel -= me;
                Vector3 v = kv.Key.Velocity; v.y = 0f;
                float vv = v.sqrMagnitude;
                if (vv < 0.01f) continue;
                float tClose = -Vector3.Dot(rel, v) / vv;
                if (tClose < -0.05f) { sh.done = true; continue; }
                float miss = (rel + v * tClose).magnitude;
                float contact = kv.Key.HitRadius + bodyRadius;
                if (miss > contact + 0.3f) continue; // minie – człowiek widzi, że leci obok
                // Trafienie następuje przy styku (promień pocisku + kapsuła postaci), a nie w chwili największego zbliżenia środków.
                float b = Vector3.Dot(rel, v), c = rel.sqrMagnitude - contact * contact;
                float disc = b * b - vv * c;
                float t = c <= 0f ? 0f : disc > 0f ? (-b - Mathf.Sqrt(disc)) / vv : tClose;
                if (t < soonest) { soonest = t; urgent = sh; urgentP = kv.Key; }
            }
            if (urgent == null) return false;

            Vector3 vel = urgentP.Velocity; vel.y = 0f;
            Vector3 side = Vector3.Cross(Vector3.up, vel.normalized);
            Vector3 relNow = transform.position - urgentP.transform.position; relNow.y = 0f;
            if (Vector3.Dot(side, relNow) < 0f) side = -side; // w stronę, po której już stoimy – dalej od toru
            if (!urgent.decided)
            {
                urgent.decided = true;
                Rec.R.reactionMargin.Add($"pocisk|{MarginBucket(soonest)}");
                urgent.response = ChooseShotResponse(urgentP, out urgent.experiment);
                if (urgentP.Owner is EnemyBrain owner) shotResponse[owner] = new ShotDecision { r = urgent.response, exp = urgent.experiment, t = now, impactAt = now + soonest };
                switch (urgent.response)
                {
                    case DefResponse.Block: blockFor = null; blockUntil = Mathf.Max(blockUntil, now + soonest + 0.3f); urgent.done = true; break;
                    case DefResponse.StepOut: evacuateDir = side; evacuateUntil = now + Mathf.Max(0.3f, soonest + 0.15f); urgent.done = true; break;
                    case DefResponse.None: case DefResponse.Parry: urgent.done = true; break;
                }
            }
            if (urgent.done) return urgent.response == DefResponse.Block || urgent.response == DefResponse.StepOut;
            // Unik: przycisk tuż przed dolotem (początek niewrażliwości + dwie klatki na ustawienie kierunku).
            float lead = Pc.Build.dodge.invulnStart + 0.04f + 1f / 30f + urgent.error;
            if (soonest <= lead)
            {
                pendingDodge = true;
                pendingDodgeDir = (side * 0.9f + (-vel.normalized) * 0.1f).normalized;
                MoveWorld = pendingDodgeDir;
                urgent.done = true;
                Rec.R.dodgeReasons.Add("unik przed pociskiem");
            }
            return true;
        }

        DefResponse ChooseShotResponse(Projectile p, out bool experiment)
        {
            experiment = false;
            var b = Pc.Build;
            bool shield = b.CanBlock && b.guardIsShield;
            if (P.experimentRate > 0f && rng.NextDouble() < P.experimentRate)
            {
                experiment = true;
                var all = new[] { DefResponse.Block, DefResponse.Dodge, DefResponse.StepOut, DefResponse.None };
                var r = all[experimentCursor++ % all.Length];
                return r == DefResponse.Block && !b.CanBlock ? DefResponse.StepOut : r;
            }
            if (!p.Dodgeable) return p.Blockable && b.CanBlock ? DefResponse.Block : DefResponse.StepOut;
            bool stam = Pc.Stamina.Current > b.dodge.staminaCost * 0.5f;
            if (p.Blockable && shield && Pc.Stamina.Fraction > 0.35f && rng.NextDouble() < (P.persona == BotPersona.Tester ? 0.5 : 0.35)) return DefResponse.Block;
            return stam ? DefResponse.Dodge : (p.Blockable && b.CanBlock ? DefResponse.Block : DefResponse.StepOut);
        }

        /// <summary>
        /// Uniki z inicjatywy gracza (styl souls-like): wyjście z okrążenia przez lukę oraz doskok unikiem do odsłoniętego celu.
        /// </summary>
        bool TryTacticalDodge()
        {
            if (now < nextTacticalAt || pendingDodge) return false;
            if (!(Pc.Actions.IsIdle || Pc.Actions.PastCancelPoint)) return false;
            float stam = Pc.Stamina.Fraction;
            // Okrążenie: 3+ wrogów walczących wręcz w promieniu 2,6 m – przewrót w stronę najmniejszego zagęszczenia.
            int close = alive.Count(e => FlatDist(e) < 2.6f && (e.Def == null || !e.Def.attacks.All(a => a.attack.delivery == AttackDelivery.Projectile)));
            if (close >= 3 && stam > 0.4f)
            {
                pendingDodge = true;
                pendingDodgeDir = AwayFrom(CrowdCenter(3f));
                MoveWorld = pendingDodgeDir;
                nextTacticalAt = now + 2.2f;
                Rec.R.dodgeReasons.Add("wyjście z okrążenia");
                return true;
            }
            // Doskok: odsłonięty cel 1–4 m poza zasięgiem broni – gracz souls skraca dystans przewrotem/szarżą barkiem.
            if (P.persona == BotPersona.SoulsSlasher && target != null && stam > 0.6f)
            {
                float reach = Pc.Build.weapon.light.reach;
                float d = FlatDist(target);
                bool open = target.CurrentState == EnemyBrain.State.Recoil || target.CurrentState == EnemyBrain.State.Staggered ||
                            (target.CurrentState == EnemyBrain.State.Attacking && target.AttackPhase == ActionPhase.Recovery);
                if (open && d > reach + 1f && d < reach + 4f)
                {
                    pendingDodge = true;
                    pendingDodgeDir = DirTo(target.transform.position);
                    MoveWorld = pendingDodgeDir;
                    nextTacticalAt = now + 3f;
                    Rec.R.dodgeReasons.Add("doskok do odsłoniętego celu");
                    return true;
                }
            }
            return false;
        }

        // ------------------------------------------------------------------ Leczenie

        bool TryFlask()
        {
            if (Root.Run.healthFlasks <= 0) return false;
            float hp = Pc.Health.Fraction;
            if (hp >= P.flaskThreshold) return false;
            if (!Pc.Actions.IsIdle) return false;
            // Okno na picie: najbliższe zauważone trafienie musi przyjść później niż koniec picia.
            // Jak człowiek pod presją: pije, gdy najbliższe zauważone trafienie jest dalej niż ~0,3 s
            // (ryzyko przerwania akceptowane); tuż obok walczącego wręcz – najpierw krok w tył.
            float soonest = swings.Where(s => now >= s.noticeAt && Threatens(s)).Select(TimeToImpact).Where(t => t > -0.1f).DefaultIfEmpty(99f).Min();
            float nearMelee = alive.Where(e => e.Def == null || !e.Def.attacks.Any(a => a.attack.delivery == AttackDelivery.Projectile)).Select(FlatDist).DefaultIfEmpty(99f).Min();
            if ((soonest > 0.3f && (nearMelee > 2f || hp < 0.2f)) || hp < 0.12f) { Pc.RequestFlask(false); return true; }
            if (soonest <= 0.3f) return false; // najpierw obrona przed tym ciosem
            MoveWorld = AwayFrom(CrowdCenter(8f));
            SprintHeld = Pc.Stamina.Fraction > 0.3f;
            flaskWaiting += Time.deltaTime;
            return true;
        }

        float flaskWaiting;
        bool slotsFixed;

        /// <summary>Początek podejścia: czary bez wymaganej Inteligencji wyjmuje ze slotów (zamiast nich – inny znany skill).</summary>
        void FixSlots()
        {
            var run = Root.Run;
            bool changed = false;
            for (int i = 0; i < RunState.SkillSlotCount; i++)
            {
                var sk = Pc.Skill(i);
                if (!Unusable(sk)) continue;
                Rec.R.Note("skill w slocie nie do użycia", $"„{sk.Def.displayName}” w slocie {i + 1} na starcie: wymaga Inteligencji, klasa nie spełnia – bot go wyjmuje");
                var spare = run.knownSpells.FirstOrDefault(s => run.SlotOf(s) < 0 && s != run.signatureSkill &&
                                                                (!s.definition.IsSpell || Pc.Build.stats != null && !UnusableDef(s)));
                if (spare != null) run.AssignSlot(spare, i); else run.ClearSlot(i);
                changed = true;
            }
            if (changed) Pc.RefreshBuild();
        }

        bool UnusableDef(SpellInstance s)
        {
            // Tymczasowo w slocie nie jest – sprawdzamy przez obliczenie skilla (te same zasady co BuildCalculator).
            var rt = BuildCalculator.ComputeSpell(s, Pc.Build.stats, Root.config.balance);
            return Unusable(rt);
        }

        // ------------------------------------------------------------------ Atak

        /// <summary>Haczyk na początek decyzji w walce (np. przełączenie aury).</summary>
        protected virtual void OnFight() { }

        /// <summary>Skille: klasa decyduje, kiedy i który. True – w tej klatce użyto skilla.</summary>
        protected virtual bool TryUseSkills() => false;

        /// <summary>Próbuje użyć skilla ze slotu (0–2, 3 = skill klasy), jeśli jest gotowy.</summary>
        protected bool UseSkill(int slot)
        {
            var skr = Pc.Skill(slot);
            // Jak człowiek: raz dostaje odmowę („za mało Inteligencji”), potem przestaje próbować.
            if (Unusable(skr))
            {
                if (refused.Add(skr.Def)) { Pc.RequestSkill(slot); Rec.R.Note("skill w slocie nie do użycia", $"„{skr.Def.displayName}” w slocie {slot + 1}: wymaga Inteligencji, klasa nie spełnia"); return true; }
                return false;
            }
            if (!Pc.SkillUsable(slot)) return false;
            if (!(Pc.Actions.IsIdle || (Pc.Actions.PastCancelPoint && (Pc.Actions.Current == ActionType.LightAttack || Pc.Actions.Current == ActionType.HeavyAttack)))) return false;
            Pc.RequestSkill(slot);
            return true;
        }

        /// <summary>Czy cel jest teraz odsłonięty (styl souls: karanie po ciosie, drgnięcie, złamana postawa).</summary>
        protected virtual bool Opening(EnemyBrain e)
        {
            switch (e.CurrentState)
            {
                case EnemyBrain.State.PoiseBroken:
                case EnemyBrain.State.Staggered:
                case EnemyBrain.State.Recoil:
                    return true;
                case EnemyBrain.State.Attacking:
                    return e.AttackPhase == ActionPhase.Recovery;
            }
            Vector3 f = e.transform.forward; f.y = 0;
            if (Vector3.Angle(f, DirTo(transform.position)) > 100f) return true; // plecy
            if (now >= opportunityRollAt)
            {
                opportunityRollAt = now + 0.4f;
                opportunity = rng.NextDouble() < P.aggression;
            }
            return opportunity;
        }

        protected virtual float DesiredRange => Mathf.Max(1.2f, Pc.Build.weapon.light.reach * 0.85f);

        void Offense()
        {
            float d = FlatDist(target);
            Vector3 to = DirTo(target.transform.position);
            float reach = Pc.Build.weapon.light.reach + Pc.Build.weapon.light.lunge * 0.5f;
            bool lowStam = Pc.Stamina.Fraction < P.lowStamina;

            if (now < backOffUntil || (lowStam && d < reach + 1.5f))
            {
                // Odskok po serii / brak wytrzymałości – dystans i krążenie.
                MoveWorld = (-to * 0.7f + Strafe(to) * 0.6f).normalized;
                return;
            }

            if (d > reach)
            {
                MoveWorld = to;
                SprintHeld = d > 7f && Pc.Stamina.Fraction > 0.55f;
                return;
            }

            bool canChain = Pc.Actions.IsIdle || ((Pc.Actions.Current == ActionType.LightAttack || Pc.Actions.Current == ActionType.HeavyAttack) && Pc.Actions.Phase == ActionPhase.Recovery);
            if (d < DesiredRange * 0.45f) MoveWorld = -to * 0.5f;
            if (!canChain || now < nextOffenseAt) return;

            if (comboLeft <= 0)
            {
                if (!Opening(target)) { MoveWorld = Strafe(to) * 0.8f + (d > DesiredRange ? to * 0.4f : Vector3.zero); return; }
                comboLeft = rng.Next(P.comboMin, P.comboMax + 1);
            }
            Pc.Request(ChooseAttack(target));
            attackCounter++;
            comboLeft--;
            nextOffenseAt = now + 0.1f;
            if (comboLeft <= 0) { backOffUntil = now + 0.35f + (float)rng.NextDouble() * 0.4f; nextOffenseAt = now + 0.5f; }
        }

        /// <summary>Szybki czy mocny cios (riposta wychodzi sama z szybkiego na złamanej postawie).</summary>
        protected virtual ActionType ChooseAttack(EnemyBrain e)
        {
            if (e.CanBeRiposted) return ActionType.LightAttack;
            if (P.persona == BotPersona.Tester) return attackCounter % 4 == 3 ? ActionType.HeavyAttack : ActionType.LightAttack;
            Vector3 f = e.transform.forward; f.y = 0;
            bool behind = Vector3.Angle(f, DirTo(transform.position)) > 110f;
            if ((e.CurrentState == EnemyBrain.State.Staggered || behind) && Pc.Stamina.Fraction > 0.5f) return ActionType.HeavyAttack;
            return ActionType.LightAttack;
        }

        Vector3 Strafe(Vector3 to)
        {
            if (now >= strafeFlipAt) { strafeSign = rng.NextDouble() < 0.5 ? 1f : -1f; strafeFlipAt = now + 1.2f + (float)rng.NextDouble() * 1.5f; }
            return Vector3.Cross(Vector3.up, to) * strafeSign;
        }

        void Wander()
        {
            // Brak wroga na arenie (portale, przerwa między falami): środek areny.
            Vector3 p = transform.position; p.y = 0;
            if (p.magnitude > 5f) MoveWorld = -p.normalized;
        }

        void CheckStuck()
        {
            if (MoveWorld.sqrMagnitude < 0.25f || !Pc.Actions.IsIdle) { stuckT = 0f; stuckPos = transform.position; return; }
            stuckT += Time.deltaTime;
            if (stuckT < 1.5f) return;
            float moved = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(stuckPos.x, 0, stuckPos.z));
            if (moved < 0.6f)
            {
                Rec.R.stuckEvents++;
                Vector3 p = transform.position;
                Rec.R.Note("zablokowany ruch", $"piętro {Root.Run.floorIndex + 1} „{Root.CurrentFloor?.name}”: brak postępu przy ({p.x:0.0}; {p.z:0.0})");
                unstickDir = Vector3.Cross(Vector3.up, MoveWorld.normalized) * (rng.NextDouble() < 0.5 ? 1f : -1f);
                unstickUntil = now + 0.9f;
            }
            stuckT = 0f;
            stuckPos = transform.position;
        }

        protected float Gauss()
        {
            double u1 = 1.0 - rng.NextDouble(), u2 = rng.NextDouble();
            return (float)(System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2));
        }

        // ------------------------------------------------------------------ Między podejściami i piętrami

        /// <summary>Konfiguracja planu przed podejściem (zestaw, skill startowy, odblokowania). Klasa nadpisuje.</summary>
        public abstract void PreparePlan(LoadoutPlan plan, int runIndex, IReadOnlyList<BotRunRecord> history);

        public static string LoadoutName(StartingLoadout l) =>
            l == null ? "?" : !string.IsNullOrEmpty(l.label) ? l.label : l.weapon != null ? l.weapon.displayName : "?";

        /// <summary>Meta-progresja: kupno odblokowań dla klasy i dobór dodatków w budżecie.</summary>
        public List<string> MetaShopping(LoadoutPlan plan)
        {
            var bought = new List<string>();
            if (!P.metaProgression) return bought;
            var meta = Root.Meta;
            var cls = plan.classDef;
            var candidates = Root.config.unlocks
                .Where(u => u.kind != UnlockKind.Difficulty && u.AvailableFor(cls) && meta.CanPurchase(u, out _))
                // Czary, których klasa nie rzuci (Inteligencja) – gracz ich nie kupuje.
                .Where(u => !(u.target is SpellDefinition sd && sd.IsSpell && sd.requirements.Any(rq => rq.attribute == AttributeType.Intelligence && rq.value > cls.intelligence)))
                .OrderByDescending(u => u.classes != null && u.classes.Count > 0) // najpierw klasowe
                .ThenBy(u => u.kind == UnlockKind.Spell ? 0 : u.kind == UnlockKind.StartingItem ? 1 : 2)
                .ThenBy(u => u.ashCost).ToList();
            foreach (var u in candidates)
                if (meta.Purchase(u)) bought.Add(u.displayName);
            // Dodatki w budżecie: najdroższe, które się mieszczą (zwykle najsilniejsze).
            foreach (var u in meta.LoadoutOptionsFor(cls).OrderByDescending(u => u.loadoutCost))
                if (!plan.extras.Contains(u) && plan.Cost + u.loadoutCost <= Root.config.balance.loadoutBudget) plan.extras.Add(u);
            meta.FillSkills(plan);
            return bought;
        }

        /// <summary>Wybór nagrody po piętrze.</summary>
        public virtual int ChooseReward(IReadOnlyList<RewardOption> options)
        {
            if (P.persona == BotPersona.Tester) return (Root.Run.floorIndex + Rec.R.run) % options.Count;
            int bestI = 0; float best = float.MinValue;
            for (int i = 0; i < options.Count; i++)
            {
                float s = RewardScore(options[i]) + (float)rng.NextDouble() * 0.5f;
                if (s > best) { best = s; bestI = i; }
            }
            return bestI;
        }

        /// <summary>Gust gracza H&S/souls: ulepszenia używanych skilli, nowe skille do pustych slotów, potem wzmocnienia.</summary>
        protected virtual float RewardScore(RewardOption o)
        {
            switch (o.kind)
            {
                case RewardKind.UpgradeSpell when Unusable(Pc.Build.skills.FirstOrDefault(x => x != null && x.instance == o.spellToUpgrade)):
                    return -1f;
                case RewardKind.UpgradeSpell: return Root.Run.SlotOf(o.spellToUpgrade) >= 0 || o.spellToUpgrade == Root.Run.signatureSkill ? 3.2f : 1f;
                case RewardKind.LearnSpell: return System.Array.IndexOf(Root.Run.skillSlots, null) >= 0 ? 3f : 1.2f;
                case RewardKind.Boon: return 2.4f;
                default: return o.item != null && o.item.definition != null && o.item.definition.kind == ItemKind.Weapon ? 1.5f : 2f;
            }
        }

        /// <summary>Kolejność rozwijania cech (klasa).</summary>
        protected abstract AttributeType NextAttribute(int pointIndex);

        /// <summary>Ekran między piętrami: cechy, kapliczka, sklep dusz.</summary>
        public virtual void Intermission()
        {
            int i = 0;
            while (Root.Run.attributePoints > 0 && i < 20)
            {
                var a = NextAttribute(Rec.R.attributes.Sum > 0 ? (int)Rec.R.attributes.Sum : 0);
                int before = Root.Run.attributePoints;
                Root.SpendAttributePoint(a);
                if (Root.Run.attributePoints == before) break;
                Rec.R.attributes.Add(a.ToString());
                i++;
            }
            if (Root.AtRestPoint)
            {
                if (Root.CanUpgradeWeapon(out _)) { Root.UpgradeWeapon(); Rec.R.purchases.Add("ulepszenie broni"); }
                int flasks = Root.Run.bonusHealthFlasks;
                Root.BuyFlask();
                if (Root.Run.bonusHealthFlasks > flasks) Rec.R.purchases.Add("dodatkowa flaszka");
            }
            // Ulepszenie używanych skilli; tester na zmianę kupuje też losową ofertę.
            if (P.persona == BotPersona.Tester && Root.ShopOffer != null && SoulShop.TryBuyOffer(Root.ShopOffer, Root.Run, Root.config.balance))
            {
                Rec.R.purchases.Add("oferta: " + Root.ShopOffer.displayName);
                Root.Player.RefreshBuild();
            }
            var unusable = new HashSet<SpellDefinition>(Enumerable.Range(0, RunState.SkillSlotCount).Select(Pc.Skill).Where(Unusable).Select(x => x.Def));
            foreach (var s in Root.Run.skillSlots.Concat(new[] { Root.Run.signatureSkill }).Where(s => s != null && !unusable.Contains(s.definition)).ToList())
            {
                int lvl = s.level;
                Root.UpgradeSkill(s);
                if (s.level > lvl) Rec.R.purchases.Add("ulepszenie: " + s.definition.displayName);
            }
        }
    }
}
