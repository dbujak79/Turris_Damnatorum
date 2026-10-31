using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Sługa przyzwany umiejętnością (Hell Unleashed): chochlik pluje ogniem z dystansu, ogar gryzie z bliska, kościany golem
    /// uderza wokół siebie. Jak w Hell Unleashed sługa nie ma życia – wrogowie go nie atakują, a znika po upływie czasu
    /// (i między piętrami). Najwyżej <see cref="MaxAlive"/> miejsc naraz – najstarszy odchodzi.
    /// Chimera legionu (cecha Przywoływacza B) to jeden sługa ze składnikami: każdy wnosi swój atak i gaśnie we własnym czasie,
    /// a chimera zajmuje tyle miejsc w limicie, ile żywych składników (bez ukrytego podwojenia limitu).
    /// </summary>
    public class Servant : MonoBehaviour, ICharacterAnimSource
    {
        public static readonly List<Servant> Active = new List<Servant>();
        public const int MaxAlive = 6;
        /// <summary>Wróg musi być najwyżej tak daleko od pana, by sługa go zaatakował.</summary>
        public const float LeashRange = 12f;

        /// <summary>Składnik chimery: rodzaj, pozostały czas wkładu, rytm ataku i migawka trafienia z przyzwania.</summary>
        public sealed class Part
        {
            public ServantKind kind;
            public float remaining, interval, timer;
            public Func<HitData> makeHit;
        }

        public ServantKind Kind { get; private set; }
        public float Remaining { get; private set; }
        /// <summary>Składniki chimery (null – zwykły sługa).</summary>
        public List<Part> Parts { get; private set; }
        public bool IsChimera => Parts != null;
        /// <summary>Miejsca zajmowane w limicie sług: chimera – żywe składniki.</summary>
        public int Slots => Parts != null ? Mathf.Max(1, Parts.Count) : 1;
        public static int SlotsUsed { get { int n = 0; foreach (var s in Active) if (s != null) n += s.Slots; return n; } }
        /// <summary>Ataki zadane przez sługę (telemetria form i chimer).</summary>
        public int Attacks { get; private set; }

        PlayerCombat master;
        Func<HitData> makeHit;
        float attackInterval, attackTimer, speed, reach;
        Color color;
        CharacterVisual visual;
        EnemyBrain prey;
        float swingTime = -1f;
        ServantKind swingKind;
        const float SwingLength = 0.45f;

        public static Servant Spawn(PlayerCombat master, ServantKind kind, float duration, float attackInterval, Func<HitData> makeHit, Color color)
        {
            while (Active.Count > 0 && SlotsUsed >= MaxAlive) Active[0].Dismiss();
            float a = UnityEngine.Random.Range(0f, 360f);
            Vector3 pos = master.transform.position + Quaternion.Euler(0, a, 0) * Vector3.forward * 1.4f;
            var go = new GameObject("Servant_" + kind);
            go.transform.SetPositionAndRotation(pos, master.transform.rotation);
            var s = go.AddComponent<Servant>();
            s.master = master; s.Kind = kind; s.Remaining = duration; s.makeHit = makeHit; s.color = color;
            s.attackInterval = Mathf.Max(0.2f, attackInterval);
            s.speed = SpeedOf(kind); s.reach = ReachOf(kind);
            Active.Add(s);
            if (Application.isPlaying)
            {
                s.visual = go.AddComponent<CharacterVisual>();
                s.visual.BuildProcedural(LookFor(kind), ScaleOf(kind));
                s.visual.Init(s);
                s.visual.SetTint(new Color(0.45f, 1f, 0.65f), 0.55f);
                var aura = HellFx.Ambient(go.transform, Vector3.up * 0.8f, HellFxElement.Soul, 0.4f);
                aura.SetAmount(0.35f);
                FxLibrary.Flash(pos + Vector3.up, new Color(0.45f, 1f, 0.65f), 1.2f);
                FxLibrary.Dust(pos, 1f);
            }
            return s;
        }

        /// <summary>
        /// Chimera ze składników (już odwołanych sług). Korpus – najcięższy składnik (golem &gt; ogar &gt; chochlik), pozostałe widoczne
        /// jako dosiadające formy. Czas każdego wkładu biegnie osobno; chimera znika z ostatnim.
        /// </summary>
        public static Servant SpawnChimera(PlayerCombat master, List<Part> parts, Vector3 pos, Color color)
        {
            if (parts == null || parts.Count == 0) return null;
            parts.Sort((x, y) => Weight(y.kind).CompareTo(Weight(x.kind)));
            while (Active.Count > 0 && SlotsUsed + parts.Count > MaxAlive) Active[0].Dismiss();
            var go = new GameObject("Servant_Chimera");
            go.transform.SetPositionAndRotation(pos, master.transform.rotation);
            var s = go.AddComponent<Servant>();
            s.master = master; s.Kind = parts[0].kind; s.Parts = parts; s.color = color;
            s.RecomputeChimera();
            Active.Add(s);
            if (Application.isPlaying)
            {
                s.visual = go.AddComponent<CharacterVisual>();
                s.visual.BuildProcedural(LookFor(s.Kind), ScaleOf(s.Kind) * 1.3f);
                s.visual.Init(s);
                s.visual.SetTint(new Color(0.75f, 1f, 0.45f), 0.6f);
                for (int i = 1; i < parts.Count; i++)
                {
                    // Dodatkowe składniki widoczne na grzbiecie korpusu – chimera komunikuje, z czego powstała.
                    var rider = new GameObject("ChimeraPart_" + parts[i].kind);
                    rider.transform.SetParent(go.transform, false);
                    rider.transform.localPosition = new Vector3(i == 1 ? 0.35f : -0.35f, 1.3f * ScaleOf(s.Kind), -0.2f);
                    var rv = rider.AddComponent<CharacterVisual>();
                    rv.BuildProcedural(LookFor(parts[i].kind), ScaleOf(parts[i].kind) * 0.55f);
                    rv.Init(s);
                    rv.SetTint(new Color(0.45f, 1f, 0.65f), 0.55f);
                }
                HellFx.Ambient(go.transform, Vector3.up * 0.9f, HellFxElement.Soul, 0.6f).SetAmount(0.5f);
                HellFx.Shockwave(pos, HellFxElement.Soul, color, 2f, true);
            }
            return s;
        }

        static int Weight(ServantKind k) => k == ServantKind.Golem ? 3 : k == ServantKind.Hound ? 2 : 1;
        static float SpeedOf(ServantKind k) => k == ServantKind.Imp ? 5.5f : k == ServantKind.Hound ? 8f : 4.2f;
        static float ReachOf(ServantKind k) => k == ServantKind.Imp ? 7f : k == ServantKind.Hound ? 1.3f : 1.7f;
        static float ScaleOf(ServantKind k) => k == ServantKind.Golem ? 1.15f : k == ServantKind.Imp ? 0.75f : 0.85f;

        void RecomputeChimera()
        {
            if (Parts == null) return;
            float rem = 0f, spd = 99f, rch = 0f; bool melee = false;
            foreach (var p in Parts) { rem = Mathf.Max(rem, p.remaining); spd = Mathf.Min(spd, SpeedOf(p.kind)); if (p.kind != ServantKind.Imp) melee = true; }
            foreach (var p in Parts) rch = melee ? (p.kind != ServantKind.Imp ? Mathf.Max(rch, ReachOf(p.kind)) : rch) : ReachOf(ServantKind.Imp);
            Remaining = rem; speed = Parts.Count > 0 ? spd : 4f; reach = Mathf.Max(1.2f, rch);
        }

        /// <summary>Migawka trafienia z chwili przyzwania (Wcielenie: jedno źródło skalowania przywołania).</summary>
        public HitData SnapshotHit() => makeHit != null ? makeHit() : default;
        public float AttackInterval => attackInterval;

        /// <summary>Składniki do scalenia: zwykły sługa – on sam; chimera – jej żywe wkłady (z zachowanym czasem).</summary>
        public List<Part> ToParts()
        {
            if (Parts != null) return new List<Part>(Parts);
            return new List<Part> { new Part { kind = Kind, remaining = Remaining, interval = attackInterval, makeHit = makeHit } };
        }

        /// <summary>Widmowy wygląd sługi (zielona poświata nakładana w Spawn).</summary>
        public static RigLook LookFor(ServantKind kind)
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
            if (Parts != null)
            {
                for (int i = Parts.Count - 1; i >= 0; i--) { Parts[i].remaining -= dt; Parts[i].timer -= dt; if (Parts[i].remaining <= 0f) Parts.RemoveAt(i); }
                if (Parts.Count == 0 || master == null || master.IsDead) { Dismiss(); return; }
                RecomputeChimera();
            }
            else Remaining -= dt;
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
            if (prey == null) return;
            if (Parts != null)
            {
                foreach (var p in Parts)
                {
                    if (p.timer > 0f || dist > ReachOf(p.kind)) continue;
                    p.timer = Mathf.Max(0.2f, p.interval);
                    swingTime = 0f; swingKind = p.kind;
                    Attack(p.kind, p.makeHit());
                }
                return;
            }
            if (dist <= reach && attackTimer <= 0f)
            {
                attackTimer = attackInterval;
                swingTime = 0f; swingKind = Kind;
                Attack(Kind, makeHit());
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

        void Attack(ServantKind kind, HitData hit)
        {
            Attacks++;
            hit.sourcePosition = transform.position;
            hit.attacker = master;
            Deliver(master, kind, hit, transform, prey, ReachOf(kind), color);
        }

        /// <summary>Atak danego rodzaju sługi z punktu <paramref name="from"/> (wspólny dla sług, chimer i Wcielenia gracza).</summary>
        public static void Deliver(PlayerCombat master, ServantKind kind, HitData hit, Transform from, IHitReceiver prey, float reach, Color color, float radiusScale = 1f)
        {
            switch (kind)
            {
                case ServantKind.Imp:
                {
                    Vector3 origin = from.position + Vector3.up * 1.0f + from.forward * 0.4f;
                    Vector3 dir = prey != null ? prey.AimPoint - origin : from.forward;
                    Projectile.Spawn(origin, dir, 14f, hit, Faction.Player, master, color, 0.2f, 1f);
                    Sfx.Play(SoundId.FireCast, origin, 0.4f, 1.3f);
                    break;
                }
                case ServantKind.Hound:
                {
                    var t = new HitTracker();
                    HitQuery.Sphere(from.position + Vector3.up * 0.7f + from.forward * reach * 0.7f, 0.8f * radiusScale, Faction.Player, t, r => HitQuery.Apply(master, r, hit, true));
                    Sfx.Play(SoundId.SwingLight, from.position, 0.5f, 1.3f);
                    break;
                }
                default:
                {
                    var t = new HitTracker();
                    HitQuery.Sphere(from.position + Vector3.up * 0.7f, 1.4f * radiusScale, Faction.Player, t, r => HitQuery.Apply(master, r, hit, true));
                    if (Application.isPlaying) FxLibrary.Shockwave(from.position, color, 1.4f * radiusScale, false);
                    Sfx.Play(SoundId.Quake, from.position, 0.5f, 1.2f);
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
                attack = swingKind == ServantKind.Imp ? AttackAnim.Cast : swingKind == ServantKind.Hound ? AttackAnim.Claw : AttackAnim.Slam,
            };
        }
    }
}
