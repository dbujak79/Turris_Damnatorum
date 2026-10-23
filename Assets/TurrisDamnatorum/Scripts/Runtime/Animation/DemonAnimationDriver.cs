using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Turris
{
    /// <summary>
    /// Sterownik demonów z Hell Unleashed (modele Generic z własnymi klipami: Idle, Walk, Attack, Special, Death, Latch…).
    /// Zamach jest prowadzony ręcznie jak w HU (EnemyActor.DriveAttackClip): klip dochodzi z wyhamowaniem do szczytu
    /// zamachu, czeka tam, a pod koniec zapowiedzi przyspiesza do klatki ciosu – pazur trafia razem z obrażeniami,
    /// niezależnie od długości zapowiedzi. Po ciosie klip gra dalej sam (odpoczynek).
    /// Przejścia między klipami przenikają się; trafienie odchyla całe ciało (sprężyna), a cios demona spłaszcza je
    /// na chwilę i rozszerza (jak Squash w HU – u bossów słabiej).
    /// </summary>
    public class DemonAnimationDriver : ICharacterAnimationDriver, IImpactReactive
    {
        const int IdleIn = 0, WalkIn = 1, ActA = 2, ActB = 3;
        const float SquashSeconds = 0.32f;

        readonly CharacterVisualDefinition def;
        readonly Transform model;
        readonly Quaternion baseRotation;
        readonly Vector3 baseScale;
        readonly bool boss;
        PlayableGraph graph;
        AnimationMixerPlayable mixer;
        AnimationClipPlayable idle, walk;
        readonly AnimationClipPlayable[] act = new AnimationClipPlayable[2];
        readonly ActionClip[] actClip = new ActionClip[2];
        int cur = -1;              // indeks bieżącego klipu akcji (0/1), -1 = brak
        float actionWeight;        // udział akcji nad ruchem
        float mix = 1f;            // udział bieżącego klipu akcji względem poprzedniego
        float fade = 0.12f;
        float walkWeight;
        float walkTime, idleTime;

        // Rozpoznanie początku nowej akcji
        AnimAction lastAction;
        AttackAnim lastAttack;
        ActionPhase lastPhase;
        float lastProgress;
        float lastActionTime;
        float afterStrike;         // sekundy od klatki ciosu (odpoczynek gra sam)

        // Ruch ciała
        Vector2 tilt, tiltVelocity; // odchylenie (stopnie) w osiach postaci: x – w bok, y – w przód/tył
        float squashAge = 1f, squashPower;

        public ActionClip CurrentAction => cur >= 0 ? actClip[cur] : null;
        public float CurrentActionTime => cur >= 0 ? (float)act[cur].GetTime() : 0f;
        public float ActionWeight => actionWeight;
        public float WalkWeight => walkWeight;
        /// <summary>Odchylenie ciała od ostatniego trafienia (stopnie).</summary>
        public float TiltAngle => tilt.magnitude;
        /// <summary>Długość klipu śmierci (s) – ciało zapada się dopiero po upadku.</summary>
        public float DeathLength { get; }

        public DemonAnimationDriver(Animator animator, CharacterVisualDefinition def, Transform model, bool boss)
        {
            this.def = def;
            this.model = model;
            this.boss = boss;
            baseRotation = model.localRotation;
            baseScale = model.localScale;
            animator.applyRootMotion = false;
            graph = PlayableGraph.Create("TurrisDemon_" + animator.name);
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
            mixer = AnimationMixerPlayable.Create(graph, 4);
            output.SetSourcePlayable(mixer);

            var still = def.idle != null ? def.idle : def.walk;
            idle = AnimationClipPlayable.Create(graph, still);
            walk = AnimationClipPlayable.Create(graph, def.walk != null ? def.walk : still);
            idle.SetSpeed(0); walk.SetSpeed(0);
            graph.Connect(idle, 0, mixer, IdleIn);
            graph.Connect(walk, 0, mixer, WalkIn);
            for (int i = 0; i < 2; i++)
            {
                act[i] = AnimationClipPlayable.Create(graph, still);
                act[i].SetSpeed(0);
                graph.Connect(act[i], 0, mixer, ActA + i);
            }
            var death = def.Find(AnimAction.Death, AttackAnim.Auto);
            DeathLength = death != null ? death.clip.length : 1.6f;
            Apply();
            graph.Evaluate(0f);
        }

        public void Dispose()
        {
            if (graph.IsValid()) graph.Destroy();
        }

        public void AddImpact(Vector3 localDir, float strength)
        {
            localDir.y = 0f;
            if (localDir.sqrMagnitude < 1e-4f) return;
            // Pchnięcie w kierunku ciosu: góra ciała odchyla się od napastnika; bossy prawie stoją.
            float k = Mathf.Clamp(strength, 0f, 1.5f) * (boss ? 70f : 260f);
            tiltVelocity += new Vector2(localDir.x, localDir.z).normalized * k;
        }

        public void Tick(in CharacterAnimState s, Vector3 worldVelocity, float dt)
        {
            if (!graph.IsValid()) return;
            var clip = Pick(s);
            bool restart = clip != null && IsNewAction(s, clip);
            if (clip != null && (restart || cur < 0 || actClip[cur] != clip)) Begin(clip, s.action);

            // Udział akcji: rośnie do 1 przy klipie, a bez klipu (ruch, drgnięcie) wraca do chodu.
            actionWeight = Mathf.MoveTowards(actionWeight, clip != null ? 1f : 0f, dt / (clip != null ? fade : 0.2f));
            mix = Mathf.MoveTowards(mix, 1f, dt / fade);
            if (clip == null && actionWeight <= 0f) cur = -1;

            if (cur >= 0 && clip != null) act[cur].SetTime(ActionTime(s, actClip[cur], dt));
            lastAction = s.action; lastAttack = s.attack; lastPhase = s.phase; lastProgress = s.phaseProgress; lastActionTime = s.actionTime;

            // Ruch: bezczynność ↔ chód, tempo kroków z prędkości (uniki – chód wstecz).
            Vector3 v = worldVelocity; v.y = 0f;
            float speed = v.magnitude;
            float walkSpeed = Mathf.Max(0.3f, def.walkSpeed);
            bool back = s.action == AnimAction.Backstep;
            walkWeight = Mathf.MoveTowards(walkWeight, Mathf.Clamp01(speed / (walkSpeed * 0.3f)), dt * 6f);
            float rate = Mathf.Clamp(speed / walkSpeed, 0.5f, 2.2f);
            if (def.walk != null) walkTime = Repeat(walkTime + dt * rate * (back ? -1f : 1f), def.walk.length);
            // Zachwianie i przełamana postawa: oddech wolniej, ciało ciężkie.
            float idleRate = s.action == AnimAction.Kneel || s.action == AnimAction.Flinch ? 0.35f : 1f;
            if (def.idle != null) idleTime = Repeat(idleTime + dt * idleRate, def.idle.length);
            walk.SetTime(walkTime);
            idle.SetTime(idleTime);

            Apply();
            graph.Evaluate(0f);
            BodyMotion(s, dt);
        }

        /// <summary>Klip dla stanu: atak według stylu (Special, Latch), uczepienie i śmierć. Drgnięcie zostaje przy ruchu.</summary>
        ActionClip Pick(in CharacterAnimState s)
        {
            switch (s.action)
            {
                case AnimAction.Attack: return def.Find(AnimAction.Attack, s.attack);
                case AnimAction.Death: return def.Find(AnimAction.Death, AttackAnim.Auto);
                case AnimAction.Latched: return def.Find(AnimAction.Latched, AttackAnim.Auto);
                default: return null;
            }
        }

        bool IsNewAction(in CharacterAnimState s, ActionClip clip)
        {
            if (cur < 0 || actClip[cur] != clip || s.action != lastAction || s.attack != lastAttack) return true;
            if (s.action != AnimAction.Attack) return s.actionTime + 1e-4f < lastActionTime;
            // Kolejny cios serii tym samym klipem: zapowiedź zaczęła się od nowa.
            return s.phase == ActionPhase.Startup && (lastPhase != ActionPhase.Startup || s.phaseProgress + 1e-4f < lastProgress);
        }

        void Begin(ActionClip clip, AnimAction action)
        {
            // Poprzedni klip zostaje zamrożony na drugim wejściu i wygasa.
            int next = cur < 0 ? 0 : 1 - cur;
            if (cur >= 0 && actionWeight > 0f) mix = 0f; else mix = 1f;
            if (act[next].GetAnimationClip() != clip.clip)
            {
                graph.Disconnect(mixer, ActA + next);
                act[next].Destroy();
                act[next] = AnimationClipPlayable.Create(graph, clip.clip);
                act[next].SetSpeed(0);
                graph.Connect(act[next], 0, mixer, ActA + next);
            }
            act[next].SetTime(0);
            actClip[next] = clip;
            cur = next;
            afterStrike = 0f;
            fade = action == AnimAction.Death ? 0.15f : 0.1f;
        }

        /// <summary>
        /// Czas klipu akcji. Atak: w zapowiedzi – wznoszenie do szczytu (wyhamowane, potem trzymanie), a w ostatnim
        /// ułamku przyspieszenie do klatki ciosu (krótkie i stałe – 0,07–0,2 s, nie więcej niż 45% zapowiedzi). Po ciosie
        /// klip biegnie w czasie rzeczywistym. Uczepienie – pętla, śmierć – raz, z zatrzymaniem na ostatniej klatce.
        /// </summary>
        float ActionTime(in CharacterAnimState s, ActionClip a, float dt)
        {
            float length = a.clip.length;
            if (a.loop) return Repeat(s.actionTime, length);
            if (s.action != AnimAction.Attack) return Mathf.Min(s.actionTime, length);
            float top = a.windupEnd * length, strike = Mathf.Max(top, a.activeEnd * length);
            if (s.phase == ActionPhase.Startup)
            {
                afterStrike = 0f;
                float telegraph = Mathf.Max(0.05f, s.windupSeconds);
                float strikeSeconds = Mathf.Min(Mathf.Clamp((strike - top) / 1.6f, 0.07f, 0.2f), telegraph * 0.45f);
                float strikeShare = strikeSeconds / telegraph;
                float raise = 1f - strikeShare;
                float u = Mathf.Clamp01(s.phaseProgress);
                if (u < raise) return top * EaseOut(Mathf.Min(1f, u / raise * 1.25f));
                float k = (u - raise) / Mathf.Max(0.001f, strikeShare);
                return top + (strike - top) * k * k;
            }
            if (lastPhase == ActionPhase.Startup) { squashAge = 0f; squashPower = 1f; }
            afterStrike += dt;
            return Mathf.Min(strike + afterStrike, length);
        }

        static float EaseOut(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }

        static float Repeat(float t, float length) => length > 1e-4f ? Mathf.Repeat(t, length) : 0f;

        void Apply()
        {
            float loco = 1f - actionWeight;
            mixer.SetInputWeight(IdleIn, loco * (1f - walkWeight));
            mixer.SetInputWeight(WalkIn, loco * walkWeight);
            for (int i = 0; i < 2; i++)
            {
                float w = i == cur ? mix : (cur >= 0 ? 1f - mix : 0f);
                mixer.SetInputWeight(ActA + i, actionWeight * w);
            }
        }

        /// <summary>Odchylenie od ciosu (tłumiona sprężyna wokół stóp) i spłaszczenie przy własnym ciosie.</summary>
        void BodyMotion(in CharacterAnimState s, float dt)
        {
            const float stiffness = 140f;
            float damping = 2f * Mathf.Sqrt(stiffness) * 0.55f;
            int steps = Mathf.Max(1, Mathf.CeilToInt(dt / 0.01f));
            float h = dt / steps;
            for (int i = 0; i < steps; i++)
            {
                tiltVelocity += (-stiffness * tilt - damping * tiltVelocity) * h;
                tilt += tiltVelocity * h;
            }
            tilt = Vector2.ClampMagnitude(tilt, boss ? 6f : 24f);
            if (s.action == AnimAction.Death) tilt = Vector2.MoveTowards(tilt, Vector2.zero, dt * 40f);
            var axis = new Vector3(tilt.y, 0f, -tilt.x);
            float angle = tilt.magnitude;
            var lean = angle > 1e-3f ? Quaternion.AngleAxis(angle, axis.normalized) : Quaternion.identity;
            model.localRotation = lean * baseRotation;

            squashAge = Mathf.Min(1f, squashAge + dt / SquashSeconds);
            float give = squashPower * (boss ? 0.4f : 1f) * (1f - squashAge);
            // Przy ciosie: niżej i szerzej, potem lekkie odbicie.
            float wobble = Mathf.Sin(squashAge * Mathf.PI * 1.5f);
            float y = 1f - 0.12f * give * wobble;
            float xz = 1f + 0.08f * give * wobble;
            model.localScale = new Vector3(baseScale.x * xz, baseScale.y * y, baseScale.z * xz);
        }
    }
}
