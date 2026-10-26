using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Coś, co skill HU zostawił działające w świecie (fala, wir, mina, słup…). Tyka host, zatrzymuje koniec piętra.</summary>
    public interface IHuRuntime
    {
        /// <summary>false – runtime skończył się i zostanie zatrzymany.</summary>
        bool Tick(float dt);
        void Stop();
    }

    /// <summary>Miejsce, z którego może wybuchnąć inny skill (Przekaźnik): pocisk w locie, mina, słup.</summary>
    public interface IHuAnchor
    {
        Vector3 AnchorPosition { get; }
        bool AnchorLive { get; }
        /// <summary>Odpala kotwicę teraz (kula wybucha, mina idzie w górę, słup się kruszy).</summary>
        void Trigger();
    }

    /// <summary>Pole żaru, które użycza żywiołu pociskom skilli bohatera przelatującym przez nie (Krąg żaru).</summary>
    public sealed class HuInfusion
    {
        public int id;
        public Vector3 center;
        public float radius, until;
        public float physical, magic;
        public Element element;
        public List<StatusApplication> statuses;
        public Color color;
    }

    /// <summary>
    /// Host runtime'ów skilli HU i wspólne operacje świata (trafienia w okręgu i pasie, wybuchy, przyciąganie, kotwice, pola).
    /// Jeden na scenę; tyka czasem gry (pauza je zatrzymuje). <see cref="Clear"/> kończy wszystko między piętrami.
    /// </summary>
    public sealed class HuWorld : MonoBehaviour
    {
        static HuWorld instance;
        readonly List<IHuRuntime> runtimes = new List<IHuRuntime>();
        readonly List<IHuRuntime> added = new List<IHuRuntime>();
        static readonly List<IHuAnchor> anchors = new List<IHuAnchor>();
        static readonly List<HuInfusion> infusions = new List<HuInfusion>();
        static int infusionSerial;
        static float clock;

        /// <summary>Najwięcej stref skilli naraz (płonąca ziemia, smugi) – limit HU dla kosztu klatki.</summary>
        public const int MaxZones = 40;
        public static int LiveZones;

        public static float Clock => clock;
        public static int Count => instance == null ? 0 : instance.runtimes.Count + instance.added.Count;

        static HuWorld Instance
        {
            get
            {
                if (instance == null && Application.isPlaying)
                {
                    var go = new GameObject("HuWorld");
                    instance = go.AddComponent<HuWorld>();
                }
                return instance;
            }
        }

        public static void Add(IHuRuntime r) { var w = Instance; if (w != null && r != null) w.added.Add(r); }

        public static void After(float seconds, Action a) => Add(new HuDelayed(seconds, a));

        void Update() => Step(Time.deltaTime);

        public void Step(float dt)
        {
            if (dt <= 0f) return;
            clock += dt;
            if (added.Count > 0) { runtimes.AddRange(added); added.Clear(); }
            for (int i = runtimes.Count - 1; i >= 0; i--)
            {
                if (i >= runtimes.Count) continue;
                var r = runtimes[i];
                bool alive;
                try { alive = r.Tick(dt); }
                catch (Exception e) { Debug.LogException(e); alive = false; }
                if (alive) continue;
                r.Stop();
                runtimes.Remove(r);
            }
            infusions.RemoveAll(f => f.until <= clock);
            anchors.RemoveAll(a => a == null || !a.AnchorLive);
        }

        /// <summary>Koniec piętra, śmierć, wyjście z poligonu: wszystko, co skille zostawiły, znika bez skutków.</summary>
        public static void Clear()
        {
            if (instance != null)
            {
                foreach (var r in instance.runtimes) { try { r.Stop(); } catch (Exception e) { Debug.LogException(e); } }
                foreach (var r in instance.added) { try { r.Stop(); } catch (Exception e) { Debug.LogException(e); } }
                instance.runtimes.Clear(); instance.added.Clear();
            }
            anchors.Clear(); infusions.Clear(); LiveZones = 0;
        }

        void OnDestroy() { if (instance == this) { instance = null; anchors.Clear(); infusions.Clear(); LiveZones = 0; } }

        // ------------------------------------------------------------------ Kotwice i pola

        public static void RegisterAnchor(IHuAnchor a) { if (a != null && !anchors.Contains(a)) anchors.Add(a); }

        /// <summary>Żywe kotwice najbliżej punktu, najwyżej <paramref name="max"/>.</summary>
        public static List<IHuAnchor> LiveAnchors(Vector3 near, int max)
        {
            var list = new List<IHuAnchor>();
            foreach (var a in anchors) if (a != null && a.AnchorLive) list.Add(a);
            list.Sort((x, y) => Flat(x.AnchorPosition, near).CompareTo(Flat(y.AnchorPosition, near)));
            if (list.Count > max) list.RemoveRange(max, list.Count - max);
            return list;
        }

        public static HuInfusion AddInfusion(Vector3 center, float radius, float seconds, float physical, float magic, Element element, List<StatusApplication> statuses, Color color)
        {
            var f = new HuInfusion { id = ++infusionSerial, center = center, radius = radius, until = clock + seconds, physical = physical, magic = magic, element = element, statuses = statuses, color = color };
            infusions.Add(f);
            return f;
        }

        public static HuInfusion InfusionAt(Vector3 at, HashSet<int> used)
        {
            foreach (var f in infusions)
                if (f.until > clock && !used.Contains(f.id) && Flat(f.center, at) <= f.radius) return f;
            return null;
        }

        // ------------------------------------------------------------------ Geometria

        public static float Flat(Vector3 a, Vector3 b) { a.y = 0; b.y = 0; return Vector3.Distance(a, b); }

        public static Vector3 FlatDir(Vector3 from, Vector3 to)
        {
            Vector3 d = to - from; d.y = 0;
            return d.sqrMagnitude > 1e-6f ? d.normalized : Vector3.forward;
        }

        /// <summary>Punkt na podłodze pod (albo nad) wskazanym miejscem.</summary>
        public static Vector3 Ground(Vector3 p)
        {
            if (Physics.Raycast(p + Vector3.up * 3f, Vector3.down, out var hit, 8f, ~0, QueryTriggerInteraction.Ignore)
                && hit.collider.GetComponentInParent<IHitReceiver>() == null) { p.y = hit.point.y; }
            return p;
        }

        /// <summary>Punkt przycięty do areny: zatrzymuje się przed ścianą na drodze od <paramref name="from"/>.</summary>
        public static Vector3 Clamp(Vector3 from, Vector3 to, float margin = 0.5f)
        {
            Vector3 a = from + Vector3.up * 0.6f, b = to; b.y = a.y;
            var hits = Physics.RaycastAll(a, (b - a).normalized, Vector3.Distance(a, b), ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            foreach (var h in hits)
                if (h.collider.GetComponentInParent<IHitReceiver>() == null && h.distance < best) best = h.distance;
            if (best < float.MaxValue) to = a + (b - a).normalized * Mathf.Max(0f, best - margin);
            return Ground(to);
        }

        public static EnemyBrain Nearest(Vector3 at, float range, Func<EnemyBrain, bool> accept = null)
        {
            EnemyBrain best = null; float bd = float.MaxValue;
            foreach (var e in EnemyBrain.All)
            {
                if (e == null || e.IsDead || (accept != null && !accept(e))) continue;
                float d = Flat(e.transform.position, at);
                if (d <= range && d < bd) { bd = d; best = e; }
            }
            return best;
        }

        /// <summary>Wróg najbliżej kierunku celowania w zasięgu i stożku (pomoc celowania).</summary>
        public static EnemyBrain AimTarget(Vector3 from, Vector3 dir, float range, float maxAngle)
        {
            EnemyBrain best = null; float bs = float.MaxValue;
            dir.y = 0;
            foreach (var e in EnemyBrain.All)
            {
                if (e == null || e.IsDead) continue;
                Vector3 to = e.transform.position - from; to.y = 0;
                float d = to.magnitude;
                if (d > range) continue;
                float ang = d < 0.3f ? 0f : Vector3.Angle(dir, to);
                if (ang > maxAngle) continue;
                float score = d + ang * 0.05f;
                if (score < bs) { bs = score; best = e; }
            }
            return best;
        }

        // ------------------------------------------------------------------ Trafienia

        /// <summary>Każdy wróg w okręgu dostaje trafienie (raz na wywołanie, albo raz na tracker).</summary>
        public static int HitCircle(IHitReceiver owner, Vector3 center, float radius, HitData hit, HitTracker tracker = null, Func<IHitReceiver, bool> accept = null)
        {
            int n = 0;
            hit.sourcePosition = center;
            HitQuery.Sphere(center + Vector3.up * 0.8f, radius, Faction.Player, tracker ?? new HitTracker(), r =>
            {
                if (accept != null && !accept(r)) return;
                HitQuery.Apply(owner, r, hit, false); n++;
            });
            return n;
        }

        /// <summary>Pas od <paramref name="from"/> do <paramref name="to"/> o połowie szerokości; opcjonalne pchnięcie wzdłuż pasa.</summary>
        public static int HitBand(IHitReceiver owner, Vector3 from, Vector3 to, float halfWidth, HitData hit, HitTracker tracker = null, Func<IHitReceiver, bool> accept = null, Vector3 push = default)
        {
            int n = 0;
            Vector3 up = Vector3.up * 0.8f;
            HitQuery.Capsule(from + up, to + up, Mathf.Max(0.2f, halfWidth), Faction.Player, tracker ?? new HitTracker(), r =>
            {
                if (accept != null && !accept(r)) return;
                var h = hit;
                h.sourcePosition = r.Transform.position - (push.sqrMagnitude > 0 ? push : FlatDir(from, to));
                HitQuery.Apply(owner, r, h, false); n++;
            });
            return n;
        }

        /// <summary>Wybuch: trafienie w promieniu, fala uderzeniowa i dźwięk w barwie żywiołu.</summary>
        public static int Explode(IHitReceiver owner, Vector3 point, float radius, HitData hit, Color color, HitTracker tracker = null, bool quiet = false)
        {
            int n = HitCircle(owner, point, radius, hit, tracker);
            if (quiet) return n;
            HellFx.Shockwave(point, hit.visualElement ?? HellFx.For(hit.element), color, radius);
            Sfx.Play(hit.element == Element.Fire ? SoundId.FireExplosion : hit.element == Element.None ? SoundId.Shockwave : Sfx.ImpactFor(hit.element), point, 0.8f);
            return n;
        }

        public static void ScheduleExplosion(IHitReceiver owner, float delay, Vector3 point, float radius, HitData hit, Color color, Transform follow = null)
        {
            if (delay <= 0.001f) { Explode(owner, follow != null ? follow.position : point, radius, hit, color); return; }
            After(delay, () =>
            {
                if (owner != null && owner.IsDead) return;
                Explode(owner, follow != null ? follow.position : point, radius, hit, color);
            });
        }

        /// <summary>Wciąga wrogów w promieniu o <paramref name="step"/> metrów w stronę punktu (bossy słabiej – reguła EnemyBrain).</summary>
        public static void PullEnemies(Vector3 at, float radius, float step)
        {
            if (step <= 0f) return;
            foreach (var e in EnemyBrain.All)
            {
                if (e == null || e.IsDead) continue;
                if (Flat(e.transform.position, at) > radius) continue;
                e.Pull(at, step, 0.12f);
            }
        }

        /// <summary>Odrzuca wrogów w promieniu od punktu.</summary>
        public static void PushEnemies(Vector3 at, float radius, float distance)
        {
            foreach (var e in EnemyBrain.All)
            {
                if (e == null || e.IsDead) continue;
                if (Flat(e.transform.position, at) > radius) continue;
                e.Knockback(FlatDir(at, e.transform.position), distance, 0.2f);
            }
        }
    }

    /// <summary>Działanie po czasie (echo, kaskada, zapalnik, kolejne skoki łańcucha).</summary>
    public sealed class HuDelayed : IHuRuntime
    {
        float left; Action action;
        public HuDelayed(float seconds, Action a) { left = seconds; action = a; }
        public bool Tick(float dt)
        {
            left -= dt;
            if (left > 0f) return true;
            var a = action; action = null; a?.Invoke();
            return false;
        }
        public void Stop() { action = null; }
    }
}
