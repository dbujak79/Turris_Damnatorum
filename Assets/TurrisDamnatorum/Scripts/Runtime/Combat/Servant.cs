using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Sługa przyzwany umiejętnością (Hell Unleashed): chochlik pluje ogniem z dystansu, ogar gryzie z bliska, kościany golem
    /// uderza wokół siebie. Jak w Hell Unleashed sługa nie ma życia – wrogowie go nie atakują, a znika po upływie czasu
    /// (i między piętrami). Najwyżej <see cref="MaxAlive"/> naraz – najstarszy odchodzi.
    /// </summary>
    public class Servant : MonoBehaviour, ICharacterAnimSource
    {
        public static readonly List<Servant> Active = new List<Servant>();
        public const int MaxAlive = 6;
        /// <summary>Wróg musi być najwyżej tak daleko od pana, by sługa go zaatakował.</summary>
        public const float LeashRange = 12f;

        public ServantKind Kind { get; private set; }
        public float Remaining { get; private set; }

        PlayerCombat master;
        Func<HitData> makeHit;
        float attackInterval, attackTimer, speed, reach;
        Color color;
        CharacterVisual visual;
        EnemyBrain prey;
        float swingTime = -1f;
        const float SwingLength = 0.45f;

        public static Servant Spawn(PlayerCombat master, ServantKind kind, float duration, float attackInterval, Func<HitData> makeHit, Color color)
        {
            while (Active.Count >= MaxAlive) Active[0].Dismiss();
            float a = UnityEngine.Random.Range(0f, 360f);
            Vector3 pos = master.transform.position + Quaternion.Euler(0, a, 0) * Vector3.forward * 1.4f;
            var go = new GameObject("Servant_" + kind);
            go.transform.SetPositionAndRotation(pos, master.transform.rotation);
            var s = go.AddComponent<Servant>();
            s.master = master; s.Kind = kind; s.Remaining = duration; s.makeHit = makeHit; s.color = color;
            s.attackInterval = Mathf.Max(0.2f, attackInterval);
            switch (kind)
            {
                case ServantKind.Imp: s.speed = 5.5f; s.reach = 7f; break;
                case ServantKind.Hound: s.speed = 8f; s.reach = 1.3f; break;
                default: s.speed = 4.2f; s.reach = 1.7f; break;
            }
            Active.Add(s);
            if (Application.isPlaying)
            {
                s.visual = go.AddComponent<CharacterVisual>();
                s.visual.BuildProcedural(LookFor(kind), kind == ServantKind.Golem ? 1.15f : kind == ServantKind.Imp ? 0.75f : 0.85f);
                s.visual.Init(s);
                s.visual.SetTint(new Color(0.45f, 1f, 0.65f), 0.55f);
                var aura = HellFx.Ambient(go.transform, Vector3.up * 0.8f, HellFxElement.Soul, 0.4f);
                aura.SetAmount(0.35f);
                FxLibrary.Flash(pos + Vector3.up, new Color(0.45f, 1f, 0.65f), 1.2f);
                FxLibrary.Dust(pos, 1f);
            }
            return s;
        }

        /// <summary>Widmowy wygląd sługi (zielona poświata nakładana w Spawn).</summary>
        static RigLook LookFor(ServantKind kind)
        {
            switch (kind)
            {
                case ServantKind.Imp:
                    return new RigLook { style = RigStyle.Heretic, head = HeadGear.Skull, body = BodyGear.Rags, hunch = 20f, limbThickness = 0.7f, weapon = WeaponModel.None, glowingEyes = true, eyes = new Color(1f, 0.5f, 0.1f) };
                case ServantKind.Hound:
                    return new RigLook { style = RigStyle.Ghoul, head = HeadGear.Skull, body = BodyGear.Rags, hunch = 35f, armLength = 1.3f, limbThickness = 0.75f, weapon = WeaponModel.Claws, glowingEyes = true, eyes = new Color(1f, 0.2f, 0.1f) };
                default:
                    return new RigLook { style = RigStyle.Warden, head = HeadGear.Skull, body = BodyGear.Rags, bulk = 1.35f, limbThickness = 1.3f, weapon = WeaponModel.None, glowingEyes = true, eyes = new Color(0.5f, 1f, 0.6f) };
            }
        }

        public void Dismiss()
        {
            Active.Remove(this);
            if (Application.isPlaying) FxLibrary.Flash(transform.position + Vector3.up, color, 0.8f);
            Util.DestroySafe(gameObject);
        }

        void OnDestroy() => Active.Remove(this);

        public static void DismissAll()
        {
            foreach (var s in Active.ToArray()) if (s != null) s.Dismiss();
            Active.Clear();
        }

        void Update() => Tick(Time.deltaTime);

        /// <summary>Krok sługi (publiczny na potrzeby testów).</summary>
        public void Tick(float dt)
        {
            Remaining -= dt;
            if (Remaining <= 0f || master == null || master.IsDead) { Dismiss(); return; }
            if (swingTime >= 0f) swingTime += dt;
            if (swingTime > SwingLength) swingTime = -1f;
            attackTimer -= dt;

            if (prey == null || prey.IsDead || Vector3.Distance(prey.transform.position, master.transform.position) > LeashRange) prey = FindPrey();
            Vector3 pos = transform.position;
            Vector3 target;
            float stopAt;
            if (prey != null) { target = prey.transform.position; stopAt = reach * 0.9f; }
            else { target = master.transform.position; stopAt = 2.2f; }
            Vector3 to = target - pos; to.y = 0f;
            float dist = to.magnitude;
            if (dist > 0.05f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 1f - Mathf.Exp(-12f * dt));
            if (dist > stopAt) transform.position = pos + to / dist * Mathf.Min(speed * dt, dist - stopAt);
            if (prey != null && dist <= reach && attackTimer <= 0f)
            {
                attackTimer = attackInterval;
                swingTime = 0f;
                Attack();
            }
        }

        EnemyBrain FindPrey()
        {
            EnemyBrain best = null; float bestD = float.MaxValue;
            foreach (var e in EnemyBrain.All)
            {
                if (e == null || e.IsDead) continue;
                if (Vector3.Distance(e.transform.position, master.transform.position) > LeashRange) continue;
                float d = Vector3.Distance(e.transform.position, transform.position);
                if (d < bestD) { bestD = d; best = e; }
            }
            return best;
        }

        void Attack()
        {
            var hit = makeHit();
            hit.sourcePosition = transform.position;
            hit.attacker = master;
            switch (Kind)
            {
                case ServantKind.Imp:
                {
                    Vector3 origin = transform.position + Vector3.up * 1.0f + transform.forward * 0.4f;
                    Vector3 dir = prey.AimPoint - origin;
                    Projectile.Spawn(origin, dir, 14f, hit, Faction.Player, master, color, 0.2f, 1f);
                    Sfx.Play(SoundId.FireCast, origin, 0.4f, 1.3f);
                    break;
                }
                case ServantKind.Hound:
                {
                    var t = new HitTracker();
                    HitQuery.Sphere(transform.position + Vector3.up * 0.7f + transform.forward * reach * 0.7f, 0.8f, Faction.Player, t, r => HitQuery.Apply(master, r, hit, true));
                    Sfx.Play(SoundId.SwingLight, transform.position, 0.5f, 1.3f);
                    break;
                }
                default:
                {
                    var t = new HitTracker();
                    HitQuery.Sphere(transform.position + Vector3.up * 0.7f, 1.4f, Faction.Player, t, r => HitQuery.Apply(master, r, hit, true));
                    FxLibrary.Shockwave(transform.position, color, 1.4f, false);
                    Sfx.Play(SoundId.Quake, transform.position, 0.5f, 1.2f);
                    break;
                }
            }
        }

        public CharacterAnimState GetAnimState()
        {
            if (swingTime < 0f) return new CharacterAnimState();
            float k = swingTime / SwingLength;
            var phase = k < 0.4f ? ActionPhase.Startup : k < 0.6f ? ActionPhase.Active : ActionPhase.Recovery;
            float p = k < 0.4f ? k / 0.4f : k < 0.6f ? (k - 0.4f) / 0.2f : (k - 0.6f) / 0.4f;
            return new CharacterAnimState
            {
                action = AnimAction.Attack, phase = phase, phaseProgress = p, actionTime = swingTime, actionDuration = SwingLength,
                attack = Kind == ServantKind.Imp ? AttackAnim.Cast : Kind == ServantKind.Hound ? AttackAnim.Claw : AttackAnim.Slam,
            };
        }
    }
}
