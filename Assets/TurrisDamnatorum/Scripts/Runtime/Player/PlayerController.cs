using UnityEngine;

namespace Turris
{
    /// <summary>Ruch względem kamery, namierzanie i ruch wynikający z akcji (wypad, unik).</summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public CameraRig cameraRig;
        [Tooltip("Awaryjna szybkość obrotu, gdy brak konfiguracji (właściwa wartość: GameConfig → balance.playerTurnSpeed).")]
        CharacterVisual visual;
        /// <summary>Sterownik klipów modelu bohatera (null – postać proceduralna).</summary>
        ClipAnimationDriver ClipDriver
        {
            get
            {
                if (visual == null) visual = GetComponent<CharacterVisual>();
                return visual != null ? visual.Driver as ClipAnimationDriver : null;
            }
        }

        public float turnSpeed = 540f;
        /// <summary>Wygaszanie końcówki swobodnego obrotu (1/s).</summary>
        const float TurnEase = 12f;
        public float lockOnRange = 25f;
        public float flickThreshold = 6f;

        PlayerInputReader input;
        PlayerCombat combat;
        CharacterController cc;
        float verticalVelocity;
        /// <summary>Wygładzona prędkość pozioma swobodnego ruchu (przyspieszanie/hamowanie).</summary>
        Vector3 planarVelocity;
        float switchCooldown;

        public EnemyBrain LockTarget { get; private set; }
        public bool InputEnabled { get; set; } = true;
        public System.Action PauseRequested;

        void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            combat = GetComponent<PlayerCombat>();
            cc = GetComponent<CharacterController>();

            input.LightPressed += () => { if (InputEnabled) combat.Request(ActionType.LightAttack); };
            input.HeavyPressed += () => { if (InputEnabled) combat.Request(ActionType.HeavyAttack); };
            input.ParryPressed += () => { if (InputEnabled) combat.Request(ActionType.Parry); };
            input.DodgePressed += () => { if (InputEnabled) combat.Request(ActionType.Dodge); };
            input.SkillPressed += slot => { if (InputEnabled) combat.RequestSkill(slot); };
            input.FlaskHealthPressed += () => { if (InputEnabled) combat.RequestFlask(false); };
            input.FlaskManaPressed += () => { if (InputEnabled) combat.RequestFlask(true); };
            input.LockOnPressed += () => { if (InputEnabled) ToggleLock(); };
            input.SwitchTargetPressed += dir => { if (InputEnabled) SwitchTarget(dir); };
            input.PausePressed += () => PauseRequested?.Invoke();
        }

        public void Teleport(Vector3 pos, Quaternion rot)
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(pos, rot);
            cc.enabled = true;
            verticalVelocity = 0;
            planarVelocity = Vector3.zero;
            LockTarget = null;
            combat.LockTarget = null;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0) return;
            var b = combat.Config != null ? combat.Config.balance : null;
            if (b == null) return;

            ValidateLock();
            combat.LockTarget = LockTarget;

            Vector3 intent = Vector3.zero;
            if (InputEnabled && !combat.IsDead)
            {
                Vector2 m = input.Move;
                intent = cameraRig.FlatForward * m.y + cameraRig.FlatRight * m.x;
                if (intent.sqrMagnitude > 1f) intent.Normalize();
                combat.SetBlockHeld(input.BlockHeld);

                if (LockTarget != null && switchCooldown <= 0 && Mathf.Abs(input.LookDelta.x) > flickThreshold)
                {
                    SwitchTarget(input.LookDelta.x > 0 ? 1 : -1);
                    switchCooldown = 0.4f;
                }
            }
            switchCooldown -= dt;
            combat.MoveIntent = intent;

            var actions = combat.Actions;
            var cur = actions.Current;
            // Chłód spowalnia ruch (zamrożenie zatrzymuje go przez wymuszone drgnięcie).
            float speed = b.moveSpeed * (1f + combat.Build.stats[StatType.MoveSpeed] / 100f) * combat.Status.MoveMultiplier(b);
            Vector3 horizontal = Vector3.zero;
            Vector3? face = null;
            // Krok z klipu liczy się w ciosie i w powrocie z niego do spoczynku (dostawienie nogi), gdy gracz stoi;
            // w innych akcjach i w ruchu przepada.
            float settle = 0f;
            if (cur != ActionType.LightAttack && cur != ActionType.HeavyAttack)
            {
                float adv = ClipDriver != null ? ClipDriver.ConsumeAdvance() : 0f;
                if (cur == ActionType.None && intent.sqrMagnitude < 0.01f && dt > 0f) settle = adv / dt;
            }
            float faceSpeed = b.playerTurnSpeed > 0 ? b.playerTurnSpeed : turnSpeed;
            // Swobodny obrót domyka się łagodnie (TurnEase); garda i śledzenie celu w ciosie zostają stałym tempem mechaniki.
            bool smoothTurn = true;
            // true: prędkość wynika z ruchu swobodnego i jest wygładzana; false: narzuca ją akcja (unik, wypad).
            bool steered = true;

            bool sprint = false;
            switch (cur)
            {
                case ActionType.None:
                    sprint = InputEnabled && input.SprintHeld && intent.sqrMagnitude > 0.1f && combat.Stamina.Current > 0;
                    horizontal = intent * speed * (sprint ? b.sprintMultiplier : 1f);
                    if (settle > 0f) { horizontal = transform.forward * settle; steered = false; }
                    face = (LockTarget != null && !sprint) ? DirTo(LockTarget.transform.position) : (intent.sqrMagnitude > 0.01f ? intent : (Vector3?)null);
                    break;
                case ActionType.Block:
                {
                    horizontal = intent * speed * b.guardMoveMultiplier;
                    face = LockTarget != null ? DirTo(LockTarget.transform.position) : (intent.sqrMagnitude > 0.01f ? intent : (Vector3?)null);
                    // Garda zasłania kierunek ataku: najpierw cios, który właśnie przyjęto z boku, potem wróg, który się zamachuje.
                    var guardDir = combat.GuardFace ?? IncomingThreat();
                    if (guardDir.HasValue) { face = guardDir; faceSpeed = Mathf.Max(faceSpeed, b.guardTurnSpeed); smoothTurn = false; }
                    break;
                }
                case ActionType.Flask:
                    horizontal = intent * speed * b.flaskMoveSpeedMultiplier;
                    break;
                case ActionType.Dodge:
                {
                    // Przemieszczenie trwa tyle co przewrót: prawie stała prędkość w toczeniu i łagodne wyhamowanie
                    // przy wstawaniu (profil 1 − u², współczynnik 1,5 zachowuje dystans).
                    var d = combat.Build.dodge;
                    float moveTime = Mathf.Max(0.2f, d.rollDuration);
                    float u = actions.Elapsed / moveTime;
                    horizontal = u < 1f ? combat.DodgeDirection * (1.5f * d.distance / moveTime * (1f - u * u)) : Vector3.zero;
                    steered = false;
                    break;
                }
                case ActionType.LightAttack:
                case ActionType.HeavyAttack:
                {
                    var atk = cur == ActionType.LightAttack ? combat.Build.weapon.light : combat.Build.weapon.heavy;
                    float t = combat.AttackWindup + combat.AttackActive; // już ze Zręcznością i tempem
                    // Bohater z modelem: krok naprzód dokładnie taki, jak w nagraniu ciosu (stopa podporowa stoi w miejscu).
                    var clips = ClipDriver;
                    if (clips != null && clips.StepsFromClips) horizontal = dt > 0f ? transform.forward * (clips.ConsumeAdvance() / dt) : Vector3.zero;
                    else if (actions.Elapsed < t && t > 0) horizontal = transform.forward * (atk.lunge / t);
                    steered = false;
                    if (actions.Elapsed < combat.AttackWindup * 0.6f)
                    {
                        face = LockTarget != null ? DirTo(LockTarget.transform.position) : (intent.sqrMagnitude > 0.01f ? intent : (Vector3?)null);
                        faceSpeed = atk.tracking * Mathf.Max(0.1f, b.playerActionSpeed);
                        smoothTurn = false;
                    }
                    break;
                }
                case ActionType.Cast:
                {
                    var skill = combat.ActiveSkill?.Def;
                    if (actions.Phase == ActionPhase.Startup)
                        face = LockTarget != null ? DirTo(LockTarget.transform.position) : (skill != null && !skill.IsSpell && intent.sqrMagnitude > 0.01f ? intent : (Vector3?)null);
                    if (combat.SkillMotion != Vector3.zero) { horizontal = combat.SkillMotion; steered = false; }
                    else if (combat.SkillMoveMultiplier.HasValue && actions.Phase == ActionPhase.Active)
                        horizontal = intent * speed * combat.SkillMoveMultiplier.Value; // młynek HU: sterowanie w trakcie wirowania
                    else if (skill != null && skill.kind == SpellKind.Whirlwind && actions.Phase == ActionPhase.Active)
                        horizontal = intent * speed * skill.moveMultiplier; // młynek: powolne sterowanie w trakcie wirowania
                    break;
                }
                case ActionType.Riposte:
                    if (LockTarget != null) face = DirTo(LockTarget.transform.position);
                    break;
            }
            combat.Sprinting = sprint;

            // Odrzut po otrzymanym ciosie (drgnięcie, blok, przełamana garda) – krótki poślizg zamiast ruchu z akcji.
            Vector3 push = combat.PushVelocity;
            if (push != Vector3.zero) { horizontal = push + horizontal * 0.3f; steered = false; face = null; }
            // Hitstop: bohater zastyga (wypad ataku też czeka), grawitacja działa dalej.
            if (combat.InHitStop) { horizontal = Vector3.zero; steered = false; face = null; }
            if (combat.Status.Rooted) { horizontal = Vector3.zero; steered = false; }

            // Swobodny ruch przyspiesza i hamuje płynnie; akcje (unik, wypad) narzucają prędkość wprost,
            // a po nich ruch płynnie przejmuje pęd zamiast zatrzymywać się w miejscu.
            if (steered)
            {
                // Pęd po akcji (doskok 25 m/s) najwyżej z prędkością sprintu – inaczej hamowanie ślizgało bohatera o kilka metrów.
                float carry = speed * b.sprintMultiplier;
                if (planarVelocity.sqrMagnitude > carry * carry) planarVelocity = planarVelocity.normalized * carry;
                bool braking = horizontal.sqrMagnitude < planarVelocity.sqrMagnitude || Vector3.Dot(horizontal, planarVelocity) < 0f;
                float accel = braking ? b.moveDeceleration : b.moveAcceleration;
                planarVelocity = accel > 0 ? Vector3.MoveTowards(planarVelocity, horizontal, accel * dt) : horizontal;
            }
            else planarVelocity = horizontal;

            if (face.HasValue && face.Value.sqrMagnitude > 0.001f)
            {
                var target = Quaternion.LookRotation(face.Value, Vector3.up);
                float step = faceSpeed * dt;
                // Duży zwrot – z limitem szybkości; końcówka zwalnia wykładniczo zamiast urwać się w miejscu.
                if (smoothTurn) step = Mathf.Min(step, Quaternion.Angle(transform.rotation, target) * (1f - Mathf.Exp(-TurnEase * dt)) + 20f * dt);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, step);
            }

            if (cc.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
            verticalVelocity += Physics.gravity.y * dt;
            cc.Move((planarVelocity + Vector3.up * verticalVelocity) * dt);
            TickFootsteps(steered, sprint, dt);
        }

        float stepDistance;

        /// <summary>Kroki co długość kroku (dłuższy w biegu); cichsze przy powolnym ruchu i z podniesioną gardą.</summary>
        void TickFootsteps(bool steered, bool sprint, float dt)
        {
            float v = new Vector3(planarVelocity.x, 0f, planarVelocity.z).magnitude;
            if (!steered || v < 0.6f || !cc.isGrounded) { stepDistance = Mathf.Min(stepDistance, 0.9f); return; }
            stepDistance += v * dt;
            float stride = sprint ? 1.03f : 0.9f; // pół cyklu chodu z animacji (cykl ≈ 1,8 m w biegu, ≈ 2,05 m w sprincie)
            if (stepDistance < stride) return;
            stepDistance -= stride;
            Sfx.Play(SoundId.Footstep, transform.position, Mathf.Clamp(v / 5f, 0.4f, 1.2f), sprint ? 0.92f : 1f);
        }

        /// <summary>
        /// Kierunek do wroga, którego cios nadchodzi najwcześniej: atakujący (zamach lub faza aktywna) w zasięgu 10 m,
        /// bliższy ma pierwszeństwo, a faza aktywna – przed zamachem. Null – nikt nie atakuje.
        /// </summary>
        Vector3? IncomingThreat()
        {
            EnemyBrain best = null;
            float bestScore = float.MaxValue;
            foreach (var e in EnemyBrain.All)
            {
                if (e == null || e.IsDead || e.CurrentState != EnemyBrain.State.Attacking) continue;
                float dist = Vector3.Distance(e.transform.position, transform.position);
                if (dist > 10f) continue;
                float score = dist - (e.AttackPhase == ActionPhase.Active ? 100f : 0f);
                if (score < bestScore) { bestScore = score; best = e; }
            }
            if (best == null) return null;
            var d = DirTo(best.transform.position);
            return d.sqrMagnitude > 1e-4f ? d : (Vector3?)null;
        }

        Vector3 DirTo(Vector3 p)
        {
            Vector3 d = p - transform.position; d.y = 0;
            return d;
        }

        void ValidateLock()
        {
            if (LockTarget == null) return;
            if (LockTarget.IsDead || !LockTarget.gameObject.activeInHierarchy ||
                Vector3.Distance(LockTarget.transform.position, transform.position) > lockOnRange * 1.2f)
            {
                LockTarget = null;
                // Automatycznie przejdź na kolejnego wroga, jeśli walczymy z grupą.
                LockTarget = FindBest(null, 0);
            }
        }

        public void ToggleLock()
        {
            if (LockTarget != null) { LockTarget = null; cameraRig.RecenterBehind(); return; }
            LockTarget = FindBest(null, 0);
            if (LockTarget == null) cameraRig.RecenterBehind();
        }

        public void SwitchTarget(int dir)
        {
            if (LockTarget == null) return;
            var next = FindBest(LockTarget, dir);
            if (next != null) LockTarget = next;
        }

        /// <summary>dir = 0: najlepszy cel przed kamerą; dir = ±1: najbliższy cel po lewej/prawej od obecnego.</summary>
        EnemyBrain FindBest(EnemyBrain current, int dir)
        {
            EnemyBrain best = null;
            float bestScore = float.MaxValue;
            Vector3 camF = cameraRig.FlatForward;
            Vector3 camR = cameraRig.FlatRight;
            float currentSide = current != null ? Vector3.Dot(camR, DirTo(current.transform.position).normalized) : 0f;

            foreach (var e in EnemyBrain.All)
            {
                if (e == current || e.IsDead) continue;
                Vector3 to = DirTo(e.transform.position);
                float dist = to.magnitude;
                if (dist > lockOnRange) continue;
                float angle = Vector3.Angle(camF, to);
                float score;
                if (dir == 0) score = angle + dist * 2f;
                else
                {
                    float side = Vector3.Dot(camR, to.normalized);
                    if ((side - currentSide) * dir <= 0.01f) continue;
                    score = Mathf.Abs(side - currentSide) * 100f + dist;
                }
                if (score < bestScore) { bestScore = score; best = e; }
            }
            return best;
        }
    }
}
