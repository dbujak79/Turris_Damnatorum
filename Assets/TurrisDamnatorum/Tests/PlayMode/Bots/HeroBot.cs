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
    public abstract partial class HeroBot : MonoBehaviour, IPlayerDriver
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

        /// <summary>Unik z inicjatywy klasy (np. mag wabi wrogów pod czar obszarowy): kierunek teraz, przycisk w następnej klatce.</summary>
        protected bool RequestDodge(Vector3 dir, string reason)
        {
            if (pendingDodge || Pc.Stamina.Current < Pc.Build.dodge.staminaCost || !(Pc.Actions.IsIdle || Pc.Actions.PastCancelPoint)) return false;
            dir.y = 0f;
            pendingDodge = true;
            pendingDodgeDir = dir.sqrMagnitude > 1e-4f ? dir.normalized : -transform.forward;
            MoveWorld = pendingDodgeDir;
            Rec.R.dodgeReasons.Add(reason);
            return true;
        }
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
            retreatSince = -1f; retreatPauseUntil = nextRetreatDodgeAt = 0f;
            shots.Clear(); shotResponse.Clear();
            refused.Clear();
            slotsFixed = false;
            origin.Clear();
            lastAction = ActionType.None;
            Root.Controller.Driver = this;
            lastWall = Time.realtimeSinceStartup;
        }

        /// <summary>Ponowna próba piętra po śmierci (stan podejścia odtworzony z zapisu): czyste spostrzeżenia walki, ta sama telemetria.</summary>
        public void ResumeAfterRetry()
        {
            swings.Clear();
            lastSeen.Clear();
            target = null;
            blockUntil = evacuateUntil = unstickUntil = backOffUntil = 0f;
            pendingDodge = false;
            dodgeExpectedUntil = 0f; nextTacticalAt = 0f;
            retreatSince = -1f; retreatPauseUntil = nextRetreatDodgeAt = 0f;
            shots.Clear(); shotResponse.Clear();
            refused.Clear();
            lastAction = ActionType.None;
            Root.Controller.Driver = this;
        }

        public void End()
        {
            if (Root != null && Root.Controller != null && ReferenceEquals(Root.Controller.Driver, this)) Root.Controller.Driver = null;
            if (Rec != null) { RememberSkills(); Rec.Bot = null; }
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
            // Niskie życie z flaszką w zapasie: co bot robi zamiast pić (sekundy wg stanu).
            if (Root.Run.healthFlasks > 0 && Pc.Health.Fraction < P.flaskThreshold) Rec.R.lowHpTime.Add(Pc.Actions.Current == ActionType.Flask ? "pije" : lowState, Time.deltaTime);
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
                    Rec.R.skillLevelSum.Add(n, Pc.ActiveSkill.instance.level);
                    Rec.R.skillUsesByOrigin.Add($"{n}|{OriginOf(Pc.ActiveSkill.Def)}");
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

        string lowState = "";

        void Think()
        {
            MoveWorld = Vector3.zero;
            SprintHeld = false;

            lowState = "unik";
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

            bool defending = ProjectileDefense() | Defend();
            BlockHeld = now < blockUntil;
            if (BlockHeld && blockFor != null && blockFor.attack.delivery != AttackDelivery.Projectile &&
                (blockFor.enemy == null || blockFor.enemy.IsDead || blockFor.enemy.CurrentState != EnemyBrain.State.Attacking || blockFor.enemy.AttackPhase == ActionPhase.Recovery))
            {
                blockUntil = Mathf.Min(blockUntil, now + 0.08f);
            }
            // Znacznik albo strefa na ziemi: zejście od środka zagrożenia ma pierwszeństwo przed ucieczką od wroga.
            if (AvoidHazards()) { BlockHeld = false; lowState = "schodzi z zagrożenia na ziemi"; return; }
            if (now < evacuateUntil) { MoveWorld = evacuateDir; SprintHeld = Pc.Stamina.Fraction > 0.2f; lowState = "wychodzi z zasięgu ciosu"; return; }
            if (defending) { lowState = BlockHeld ? "blokuje" : "broni się (parowanie/unik)"; return; }

            if (TryShakeOffLatch()) { lowState = "strząsa czerwia"; return; }
            if (TryFlask()) { lowState = Pc.Actions.Current == ActionType.Flask ? "pije" : "odwrót do bezpiecznego miejsca"; return; }
            lowState = now < retreatPauseUntil ? "walczy (brak bezpiecznego miejsca)" : !Pc.Actions.IsIdle ? "w trakcie akcji" : "walczy";
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

        // ------------------------------------------------------------------ Zagrożenia na ziemi

        const float HazardMargin = 0.7f, HazardLookahead = 1.6f, HazardMaxRadius = 7f;
        bool inHazard;

        /// <summary>
        /// Znaczniki opóźnionych uderzeń (głazy, kolce, wybuchy) i strefy wrogów: gracz widzi je na ziemi i z nich schodzi –
        /// od środka zagrożenia, nie od wroga, który je rzucił. Gdy biegiem nie zdąży przed uderzeniem – unik w tę stronę
        /// (ciosy obszarowe są nieunikalne, więc liczy się odległość, nie nietykalność).
        /// </summary>
        bool AvoidHazards()
        {
            if (pendingDodge) return false;
            Vector3 me = transform.position; me.y = 0f;
            Vector3 push = Vector3.zero;
            float worst = 0f, strikeIn = float.MaxValue;
            foreach (var s in DelayedStrike.Active)
                if (s != null && s.Faction == Faction.Enemy && s.Remaining <= HazardLookahead) Add(s.transform.position, s.Radius, s.Remaining);
            foreach (var z in DamageZone.Active)
                if (z != null && z.Faction == Faction.Enemy && z.Remaining > 0.2f) Add(z.transform.position, z.Radius, -1f);
            if (worst <= 0f) { inHazard = false; return false; }
            if (!inHazard) { inHazard = true; Rec.R.actions.Add("zejście z zagrożenia na ziemi"); }

            Vector3 dir = push.sqrMagnitude > 1e-4f ? push.normalized : (target != null ? Strafe(DirTo(target.transform.position)) : -transform.forward);
            if (me.magnitude > 9f) dir = (dir - me.normalized * 0.6f).normalized; // nie pod ścianę areny
            float speed = Mathf.Max(3f, Root.config.balance.moveSpeed * (Pc.Stamina.Fraction > 0.2f ? 1.3f : 1f));
            if (strikeIn < worst / speed + 0.1f && RequestDodge(dir, "zejście ze znacznika na ziemi")) return true;
            MoveWorld = dir;
            SprintHeld = Pc.Stamina.Fraction > 0.2f;
            return true;

            void Add(Vector3 c, float r, float t)
            {
                if (r > HazardMaxRadius) return; // pół areny – i tak nie ma dokąd zejść
                c.y = 0f;
                Vector3 d = me - c;
                float dist = d.magnitude, depth = r + HazardMargin - dist;
                if (depth <= 0f) return;
                push += (dist > 0.05f ? d / dist : Vector3.zero) * depth;
                worst = Mathf.Max(worst, depth);
                if (t >= 0f) strikeIn = Mathf.Min(strikeIn, t);
            }
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
        /// <summary>Tester przyjmuje ciosy eksperymentalnie tylko przy zdrowiu powyżej progu flaszki – przy niskim gra poprawnie i szuka miejsca na łyk.</summary>
        protected bool Experimenting => P.experimentRate > 0f && Pc.Health.Fraction >= P.flaskThreshold;

        protected virtual DefResponse ChooseResponse(Swing s, out bool experiment)
        {
            experiment = false;
            if (Experimenting && rng.NextDouble() < P.experimentRate)
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
                    // Blok bronią przepuszcza dużo (seria specjalizacji: rani 68% ciosów wobec 49% po uniku) – bez tarczy unik.
                    if (b.CanBlock && stam > 0.35f && (shield || !canDodge)) return DefResponse.Block;
                    return canDodge ? DefResponse.Dodge : DefResponse.StepOut;
                default:
                    if (a.delivery == AttackDelivery.Projectile)
                        return shield && stam > 0.25f ? DefResponse.Block : canDodge ? DefResponse.Dodge : DefResponse.StepOut;
                    if (b.CanParry && stam > 0.2f && rng.NextDouble() < P.parryChance) return DefResponse.Parry;
                    if (b.CanBlock && stam > 0.3f && (shield || !canDodge)) return DefResponse.Block;
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
            if (Experimenting && rng.NextDouble() < P.experimentRate)
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

        // Zasada dla wszystkich botów: pić wyłącznie w bezpiecznym miejscu – picie obok wroga to wyrok śmierci.
        // Gdy nie jest bezpiecznie, bot odskakuje unikami (i biegnie) tam, gdzie wrogowie najdłużej nie mogą go dosięgnąć.

        /// <summary>Zapas ponad czas picia (s): przez tyle nikt nie może zdążyć z trafieniem.</summary>
        const float DrinkSafetyMargin = 0.15f;
        /// <summary>Po tylu sekundach bezskutecznego odwrotu bot wraca na chwilę do walki (dalej pije tylko w bezpiecznej chwili).</summary>
        const float RetreatGiveUp = 5f, RetreatPause = 2.5f;
        /// <summary>Przy tak niskim życiu walka to śmierć – odwrót trwa dłużej (RetreatGiveUpLow s).</summary>
        const float LowHealth = 0.25f, RetreatGiveUpLow = 15f;
        /// <summary>Promień, poza którym bot traktuje okolicę jako ścianę areny (jak przy schodzeniu z zagrożeń).</summary>
        const float ArenaSafeRadius = 9f;

        string unsafeReason = "";
        float retreatSince = -1f, retreatPauseUntil, nextRetreatDodgeAt;
        Vector3 retreatDir;

        /// <summary>Czas (s), przez który bohater pije i nie może się bronić – do momentu, gdy flaszka działa.</summary>
        protected float DrinkWindow => Pc.ScaleStartup(Root.config.balance.flaskDrinkTime) + 0.05f + DrinkSafetyMargin;

        /// <summary>
        /// Najkrótszy czas (s), w którym wróg może trafić bohatera stojącego w <paramref name="at"/>: trwający zamach,
        /// albo dojście na zasięg najszybszego ataku plus jego zapowiedź; pocisk – zapowiedź plus lot. Ogłuszony,
        /// zachwiany czy blokujący wróg najpierw musi z tego wyjść.
        /// </summary>
        protected float ThreatTime(EnemyBrain e, Vector3 at)
        {
            if (e.CurrentState == EnemyBrain.State.Latched) return 0f;
            Vector3 d = e.transform.position - at; d.y = 0f;
            float dist = Mathf.Max(0f, d.magnitude - 0.35f * (e.Def != null ? e.Def.scale : 1f));
            float best = 99f;
            if (e.CurrentState == EnemyBrain.State.Attacking && e.CurrentAttack != null && e.AttackPhase != ActionPhase.Recovery)
            {
                var a = e.CurrentAttack.attack;
                if (Reaches(a, dist))
                {
                    var s = swings.FirstOrDefault(x => x.enemy == e);
                    best = e.AttackPhase == ActionPhase.Active || s == null ? 0f : Mathf.Max(0f, TimeToImpact(s));
                }
            }
            float wait = 0f;
            switch (e.CurrentState)
            {
                case EnemyBrain.State.Staggered: case EnemyBrain.State.Recoil: case EnemyBrain.State.PoiseBroken:
                case EnemyBrain.State.Guarding: case EnemyBrain.State.Evading: case EnemyBrain.State.Dormant:
                    wait = Mathf.Max(0f, e.StateTimer); break;
                case EnemyBrain.State.Attacking:
                    wait = e.AttackRecoveryRemaining; break; // po ciosie – okno, które gracz souls wykorzystuje na łyk
            }
            float speed = Mathf.Max(0.5f, e.Def != null ? e.Def.moveSpeed : 3.5f);
            if (e.Def != null)
                foreach (var entry in e.Def.attacks)
                {
                    var a = entry.attack;
                    float windup = a.windup;
                    float t = a.delivery == AttackDelivery.Projectile
                        ? (dist <= 24f ? windup + dist / ProjectileSpeed : 99f)
                        : Mathf.Max(0f, dist - Range(a) - 0.5f) / speed + windup;
                    best = Mathf.Min(best, wait + t);
                }
            else best = Mathf.Min(best, wait + dist / speed);
            return best;

            static float Range(AttackDefinition a) => a.delivery == AttackDelivery.AreaAroundSelf ? a.radius : a.reach + a.lunge + a.radius;
            static bool Reaches(AttackDefinition a, float dist) => a.delivery == AttackDelivery.Projectile ? dist <= 24f : dist <= Range(a) + 0.9f;
        }

        /// <summary>Najkrótszy czas do trafienia w <paramref name="at"/> ze strony wszystkich wrogów.</summary>
        protected float ThreatTimeAt(Vector3 at)
        {
            float t = 99f;
            foreach (var e in alive) t = Mathf.Min(t, ThreatTime(e, at));
            return t;
        }

        /// <summary>
        /// Czy można teraz bezpiecznie pić: nikt nie zdąży trafić przed końcem picia (z zapasem), żaden lecący pocisk
        /// nie doleci, bohater nie stoi w strefie ani pod znacznikiem uderzenia.
        /// </summary>
        protected bool SafeToDrink()
        {
            if (!Pc.Actions.IsIdle || pendingDodge) return false;
            float window = DrinkWindow;
            foreach (var e in alive)
                if (ThreatTime(e, transform.position) < window)
                {
                    unsafeReason = e.Def != null && e.Def.attacks.Count > 0 && e.Def.attacks.All(a => a.attack.delivery == AttackDelivery.Projectile) ? "strzelec" : "wróg wręcz";
                    return false;
                }
            unsafeReason = "pocisk";
            Vector3 me = transform.position; me.y = 0f;
            foreach (var p in Projectile.Active)
            {
                if (p == null || p.Side != Faction.Enemy) continue;
                Vector3 rel = p.transform.position; rel.y = 0f; rel -= me;
                Vector3 v = p.Velocity; v.y = 0f;
                float vv = v.sqrMagnitude;
                if (vv < 0.01f) continue;
                float tClose = -Vector3.Dot(rel, v) / vv;
                if (tClose < 0f || tClose > window) continue;
                if ((rel + v * tClose).magnitude <= p.HitRadius + bodyRadius + 0.5f) return false;
            }
            unsafeReason = "strefa na ziemi";
            foreach (var s in DelayedStrike.Active)
                if (s != null && s.Faction == Faction.Enemy && s.Remaining <= window + 0.3f && Flat(s.transform.position - me) <= s.Radius + HazardMargin) return false;
            foreach (var z in DamageZone.Active)
                if (z != null && z.Faction == Faction.Enemy && z.Remaining > 0.2f && Flat(z.transform.position - me) <= z.Radius + HazardMargin) return false;
            return true;

            static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
        }

        /// <summary>
        /// Odwrót do bezpiecznego miejsca: z 16 kierunków wybiera ten, w którym wrogowie najdłużej nie dosięgną bohatera
        /// (bez ściany areny i stref na ziemi); gdy ktoś jest blisko – unik w tę stronę, inaczej bieg.
        /// </summary>
        protected void RetreatToSafety(string reason)
        {
            Vector3 me = transform.position; me.y = 0f;
            float step = Mathf.Max(3f, Pc.Build.dodge.distance);
            Vector3 bestDir = AwayFrom(CrowdCenter(10f));
            float bestScore = float.MinValue;
            for (int i = 0; i < 16; i++)
            {
                float ang = i * Mathf.PI / 8f;
                var dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
                Vector3 p = me + dir * step;
                // Jeden i dwa uniki dalej – kierunek, który daleko nie kończy się w zasięgu doskoku bossa ani pod ścianą.
                Vector3 p2 = me + dir * step * 2f;
                float score = 0.5f * Mathf.Min(ThreatTimeAt(p), 4f) + 0.5f * Mathf.Min(ThreatTimeAt(p2), 4f);
                if (p2.magnitude > ArenaSafeRadius) score -= (p2.magnitude - ArenaSafeRadius) * 0.3f;
                if (p.magnitude > ArenaSafeRadius) score -= (p.magnitude - ArenaSafeRadius) * 0.6f;
                foreach (var z in DamageZone.Active)
                    if (z != null && z.Faction == Faction.Enemy && z.Remaining > 0.2f && Vector3.Distance(new Vector3(z.transform.position.x, 0f, z.transform.position.z), p) <= z.Radius + HazardMargin) score -= 2f;
                foreach (var s in DelayedStrike.Active)
                    if (s != null && s.Faction == Faction.Enemy && Vector3.Distance(new Vector3(s.transform.position.x, 0f, s.transform.position.z), p) <= s.Radius + HazardMargin) score -= 2f;
                score += 0.15f * Vector3.Dot(dir, retreatDir); // bez szarpania w miejscu
                if (score > bestScore) { bestScore = score; bestDir = dir; }
            }
            retreatDir = bestDir;
            bool pressed = ThreatTimeAt(transform.position) < DrinkWindow + 0.4f;
            if (pressed && now >= nextRetreatDodgeAt && Pc.Stamina.Fraction > 0.3f && RequestDodge(bestDir, reason))
            {
                nextRetreatDodgeAt = now + 0.45f;
                return;
            }
            MoveWorld = bestDir;
            SprintHeld = Pc.Stamina.Fraction > 0.15f;
        }

        /// <summary>Czerw wgryziony w bohatera: unik go strząsa (z nim na plecach nigdy nie jest bezpiecznie, by pić).</summary>
        bool TryShakeOffLatch()
        {
            if (!alive.Any(e => e.CurrentState == EnemyBrain.State.Latched)) return false;
            return RequestDodge(AwayFrom(CrowdCenter(6f)), "strząśnięcie czerwia");
        }

        bool TryFlask()
        {
            if (Root.Run.healthFlasks <= 0) { retreatSince = -1f; return false; }
            // Łyk w spokojnej chwili (np. między falami): brakuje co najmniej tyle, ile flaszka leczy – nic się nie marnuje,
            // a flaszki i tak wracają na następnym piętrze. Bot nie czeka do krytycznego życia, gdy bezpiecznej chwili może nie być.
            float heal = Root.config.balance.healthFlaskFraction * (1f + Pc.Build.stats[StatType.FlaskPotency] / 100f);
            if (Pc.Health.Fraction >= P.flaskThreshold)
            {
                retreatSince = -1f;
                if (1f - Pc.Health.Fraction >= heal && SafeToDrink()) { Pc.RequestFlask(false); return true; }
                return false;
            }
            if (SafeToDrink())
            {
                Pc.RequestFlask(false);
                retreatSince = -1f;
                return true;
            }
            // Nie ma bezpiecznej chwili: odwrót. Po RetreatGiveUp s bez skutku – chwila walki (picie dalej tylko, gdy bezpiecznie).
            if (now < retreatPauseUntil) return false;
            if (retreatSince < 0f) retreatSince = now;
            if (now - retreatSince > (Pc.Health.Fraction < LowHealth ? RetreatGiveUpLow : RetreatGiveUp))
            {
                retreatSince = -1f;
                retreatPauseUntil = now + RetreatPause;
                Rec.R.actions.Add("brak bezpiecznego miejsca na picie: " + unsafeReason);
                return false;
            }
            RetreatToSafety("odskok przed piciem flaszki");
            return true;
        }

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
            TrackOrigins(true);
            if (P.persona != BotPersona.Tester) ArrangeSlots(); // gracz ustawia sloty przed walką wg doświadczenia z poprzednich podejść
        }

        bool UnusableDef(SpellInstance s)
        {
            // Tymczasowo w slocie nie jest – sprawdzamy przez obliczenie skilla (te same zasady co BuildCalculator).
            var rt = BuildCalculator.ComputeSpell(s, Pc.Build.stats, Root.config.balance);
            return Unusable(rt);
        }

        // ------------------------------------------------------------------ Atak

        /// <summary>Haczyk na początek decyzji w walce – domyślnie: aura zdobyta po drodze włączona raz i zostawiona.</summary>
        protected virtual void OnFight()
        {
            for (int slot = 0; slot < RunState.SkillSlotCount; slot++)
            {
                var sk = Pc.Skill(slot);
                if (sk != null && sk.Def.IsAura && !auraTried.Contains(sk.Def) && Pc.Actions.IsIdle)
                {
                    auraTried.Add(sk.Def);
                    Pc.RequestSkill(slot);
                }
            }
        }

        readonly HashSet<SpellDefinition> auraTried = new HashSet<SpellDefinition>();

        /// <summary>Kiedy użyć skilla danego rodzaju (wspólne dla klas; działa też dla skilli kupionych po drodze).</summary>
        protected virtual bool WantsSkill(SpellDefinition def, float d, int close, float hp)
        {
            float reach = Mathf.Max(def.attack.reach, def.attack.radius, 2.5f);
            bool tester = P.persona == BotPersona.Tester;
            switch (def.kind)
            {
                case SpellKind.Cone:
                case SpellKind.Cleave:
                case SpellKind.Flurry:
                case SpellKind.ShieldBash:
                case SpellKind.Quake:
                    return d <= reach * 0.9f && (close >= 2 || tester || target.CurrentState != EnemyBrain.State.Attacking);
                case SpellKind.Wave:
                case SpellKind.Projectile:
                case SpellKind.Chain:
                case SpellKind.WeaponThrow:
                    return d >= 3.5f && d <= reach || (close >= 2 && d <= 3f);
                case SpellKind.Charge:
                case SpellKind.Dash:
                    return d >= 4f && d <= Mathf.Max(reach, 8f);
                case SpellKind.Whirlwind:
                case SpellKind.Nova:
                case SpellKind.Warcry:
                    return close >= 2 || (tester && close >= 1);
                case SpellKind.Heal:
                case SpellKind.Barrier:
                case SpellKind.FrostArmor:
                    return hp < 0.5f && close >= 1;
                case SpellKind.Undying: // Żelazna skóra (−80% obrażeń): w tłoku, zanim życie spadnie
                    return close >= 3 || (hp < 0.6f && close >= 1);
                case SpellKind.WeaponBuff:
                    return d <= 6f && Pc.WeaponBuffTime <= 0f;
                case SpellKind.Aura:
                    return false; // aury przełącza OnFight
                case SpellKind.Zone:
                case SpellKind.Meteor:
                case SpellKind.Storm:
                    return d <= reach && (close >= 2 || target.Def.isBoss);
                default:
                    return d <= reach;
            }
        }

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

        /// <summary>Atak bronią: domyślnie wręcz (podejście, seria, odskok); mag nadpisuje walką na dystans.</summary>
        protected virtual void Offense()
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

        protected Vector3 Strafe(Vector3 to)
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
                // Czary, których klasa nie rzuci (Inteligencja), i skille na broń spoza zestawów klasy – gracz ich nie kupuje.
                .Where(u => !(u.target is SpellDefinition sd && sd.IsSpell && sd.requirements.Any(rq => rq.attribute == AttributeType.Intelligence && rq.value > cls.intelligence)))
                .Where(u => !(u.target is SpellDefinition sk) || cls.loadouts.Count == 0 || cls.loadouts.Any(l => WeaponFamilies.Usable(sk, l.weapon, l.offHand)))
                .OrderByDescending(u => u.classes != null && u.classes.Count > 0) // najpierw klasowe
                .ThenBy(u => u.kind == UnlockKind.Spell ? 0 : u.kind == UnlockKind.StartingItem ? 1 : 2)
                .ThenBy(u => u.ashCost).ToList();
            foreach (var u in candidates)
                if (meta.Purchase(u)) bought.Add(u.displayName);
            // Dodatki w budżecie: najdroższe, które się mieszczą (zwykle najsilniejsze). Bez broni i drugiej ręki – zastąpiłyby
            // broń zestawu, a skill startowy wymaga jej rodzaju (miecz → Cięcie wichru).
            foreach (var u in meta.LoadoutOptionsFor(cls).Where(u => !(u.target is ItemDefinition it && (it.IsWeapon || it.IsShield || it.kind == ItemKind.OffHand)))
                         .OrderByDescending(u => u.loadoutCost))
                if (!plan.extras.Contains(u) && plan.Cost + u.loadoutCost <= Root.config.balance.loadoutBudget) plan.extras.Add(u);
            meta.FillSkills(plan);
            return bought;
        }

        /// <summary>Kolejność rozwijania cech (klasa).</summary>
        protected abstract AttributeType NextAttribute(int pointIndex);

        /// <summary>Ekran między piętrami: cechy, kapliczka, sklep dusz (oferta, sloty, ulepszenia).</summary>
        public virtual void Intermission()
        {
            auraTried.Clear();
            TrackOrigins(false);
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
            // Przedmioty z plecaka, oferta sklepu, potem ułożenie slotów, na końcu ulepszenia skilli, które zostały w slotach.
            ManageGear();
            SellUnused();
            ManageFloorShop();
            SellUnused();
            ManageSeals();
            ConsiderShopOffer();
            ArrangeSlots();
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
