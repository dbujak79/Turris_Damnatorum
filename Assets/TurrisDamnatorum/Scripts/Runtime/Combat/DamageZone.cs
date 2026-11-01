using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Strefa na ziemi (np. płonąca ziemia): co <c>interval</c> sekund trafia każdego wroga w promieniu.
    /// Trafienia liczone zegarem, nie klatkami. Efekty (podpalenie) pochodzą z danych trafienia.
    /// </summary>
    public class DamageZone : MonoBehaviour
    {
        public static readonly List<DamageZone> Active = new List<DamageZone>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        HitData hit;
        Faction faction;
        IHitReceiver owner;
        float radius, interval, life, elapsed;
        int ticksDone;
        readonly HitTracker tracker = new HitTracker();
        GameObject visual;

        public float Radius => radius;
        public Faction Faction => faction;
        public IHitReceiver Owner => owner;
        /// <summary>Prąd głębin: strefę wolno przesuwać (tylko strefy z Spawn – nie aury, totemy, miny, ściany ani stanowiska).</summary>
        public bool Relocatable { get; private set; } = true;
        /// <summary>Ile razy strefa została przesunięta (Prąd głębin – telemetria; czas i tyknięcia bez zmian).</summary>
        public int Moves { get; private set; }
        public int Ticks { get; private set; }
        /// <summary>Wszystkie tyknięcia stref od startu (telemetria, testy harmonogramu).</summary>
        public static int TotalTicks;
        /// <summary>
        /// Pomiar realizacji budżetu (audyt Gwałtownej manifestacji): kwalifikowana strefa czaru gracza zlicza trafienia celów w całym
        /// czasie i w pierwszej połowie; po zniknięciu oddaje liczby rzucającemu. Nie zmienia walki.
        /// </summary>
        public PlayerCombat MeasuredBy;
        int measuredHits, measuredFirstHalf; float span; bool measuredReported;
        Vector3 glideFrom, glideTo; float glideTime, glideLeft;

        /// <summary>
        /// Przesuwa strefę do punktu w podanym czasie, zachowując pozostały czas, rytm tyknięć, pochodzenie i statusy;
        /// bez ponownego wybuchu początkowego.
        /// </summary>
        public void GlideTo(Vector3 to, float seconds)
        {
            glideFrom = transform.position; glideTo = to; glideTime = Mathf.Max(0.05f, seconds); glideLeft = glideTime;
            Moves++;
        }
        /// <summary>Sekundy do zgaśnięcia strefy.</summary>
        public float Remaining => life * TimeScale;
        public float Interval => interval * TimeScale;
        public HitData ZoneHit => hit;
        /// <summary>Budżet pozostały w strefie (obrażenia jednego tyknięcia × pozostałe tyknięcia).</summary>
        public float RemainingBudget => hit.Total * Mathf.Max(0f, life) / Mathf.Max(0.1f, interval);
        /// <summary>Zjedzona/rozwinięta: strefa znika bez skutków.</summary>
        public void Expire() { life = 0f; Destroy(gameObject); }
        public void SetLife(float seconds) => life = Mathf.Max(0f, seconds) / TimeScale;
        /// <summary>Stały punkt strefy (Kaptur odwróconego celownika – deszcz za przewodnikiem).</summary>
        public void MoveTo(Vector3 p) { transform.position = p; if (visual != null) visual.transform.position = p; }

        /// <summary>Parametry strefy tworzonej po czymś innym (np. po wybuchu meteoru).</summary>
        public struct Settings { public float radius, duration, interval; public HitData hit; public float timeScale; public PlayerCombat measuredBy; }

        /// <summary>
        /// Lokalny zegar strefy (Gwałtowna manifestacja): czas świata × skala = czas strefy. Interwał i czas życia zostają w jednostkach
        /// bazowego harmonogramu, więc minimum 0,1 s nie obcina skompresowanego tyknięcia, a liczba i siła trafień są takie same.
        /// </summary>
        public float TimeScale { get; private set; } = 1f;
        public float LocalRemaining => life;
        public float LocalInterval => interval;

        public static DamageZone Spawn(Vector3 center, float radius, float duration, float interval, HitData hit, Faction faction, IHitReceiver owner, Color color, float timeScale = 1f)
        {
            var go = new GameObject("DamageZone");
            go.transform.position = center;
            var z = go.AddComponent<DamageZone>();
            if (owner is EnemyBrain enemy) enemy.TrackOwnedEffect(z);
            z.hit = hit; z.hit.isGroundHazard = true; z.faction = faction; z.owner = owner;
            z.radius = radius; z.interval = Mathf.Max(0.1f, interval); z.life = duration; z.span = duration;
            z.TimeScale = timeScale > 0f && timeScale <= 1f ? timeScale : 1f;
            z.visual = FxLibrary.ElementGround(center, radius, hit.element, color, duration * z.TimeScale, hit.visualElement); // VFX = rzeczywisty czas
            Sfx.Play(Sfx.ForElement(hit.element, SoundId.Chains), center, 0.8f, 0.8f);
            return z;
        }

        void Update() => Step(Time.deltaTime);
        public void Step(float dt)
        {
            if (dt <= 0) return;
            if (owner != null && owner.IsDead) { Util.DestroySafe(gameObject); return; }
            if (glideLeft > 0f)
            {
                glideLeft = Mathf.Max(0f, glideLeft - dt);
                transform.position = Vector3.Lerp(glideFrom, glideTo, 1f - glideLeft / glideTime);
                if (visual != null) visual.transform.position = transform.position;
            }
            float ldt = dt / TimeScale; // lokalny zegar strefy (×2 przy manifestacji)
            // Chwile tyknięć z indeksu (k × interwał): pierwsze w 0, koniec przedziału wykluczony; liczba trafień nie zależy od kroku czasu.
            float limit = elapsed + life, end = Mathf.Min(elapsed + ldt, limit);
            while (ticksDone * interval <= end + 1e-5f && ticksDone * interval < limit - 1e-4f)
            {
                tracker.Reset(); hit.sourcePosition = transform.position; Ticks++; TotalTicks++;
                int n = 0;
                HitQuery.Sphere(transform.position + Vector3.up * 0.6f, radius, faction, tracker, r => { n++; HitQuery.Apply(owner, r, hit, false); });
                if (MeasuredBy != null) { measuredHits += n; if (ticksDone * interval < span * 0.5f - 1e-4f) measuredFirstHalf += n; }
                ticksDone++;
            }
            elapsed += ldt;
            life -= ldt;
            if (life <= 0) Util.DestroySafe(gameObject);
        }

        void OnDestroy()
        {
            if (MeasuredBy != null && !measuredReported) { measuredReported = true; MeasuredBy.ZoneMeasured(Ticks, measuredHits, measuredFirstHalf, TimeScale < 1f); }
            if (visual == null) return;
            var particles = visual.GetComponentInChildren<HellParticles>();
            if (particles != null) particles.StopAndRelease();
            Destroy(visual);
        }
    }
}
