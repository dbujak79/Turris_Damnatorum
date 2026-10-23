using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Wrogowie wychodzą z portali (czerwone, stojące owale jak „town portal” z Diablo – <see cref="DemonPortal"/>) w różnych
    /// miejscach areny, nigdy tuż przy bohaterze:
    /// – piętra z portalami (fale Hell Unleashed): przez czas piętra co kilka sekund mała grupa demonów, skład z wag, budżet;
    ///   kto nie zabija na bieżąco, ma coraz ciaśniej; co 20–30 demonów złota elita (licznik przez cały run);
    /// – zwykłe fale Turris: wrogowie fali wychodzą po jednym co <see cref="StreamInterval"/> s, zamiast pojawić się naraz.
    /// Bossowie wchodzą od razu (pojedynek).
    /// </summary>
    public partial class GameRoot
    {
        /// <summary>Otwarty portal: miejsce, kierunek wyjścia, wrogowie do wypuszczenia, czas do wyjścia.</summary>
        class PendingPortal
        {
            public Vector3 position, facing;
            public readonly List<(EnemyDefinition def, bool elite)> demons = new List<(EnemyDefinition, bool)>();
            public float timer;
            public DemonPortal fx;
        }

        /// <summary>Czas od otwarcia portalu do wyjścia wroga – widać, skąd nadejdzie.</summary>
        public const float PortalDelay = 1.1f;
        /// <summary>Odstęp między kolejnymi wrogami zwykłej fali (s).</summary>
        public const float StreamInterval = 2.5f;

        readonly List<PendingPortal> portals = new List<PendingPortal>();
        readonly List<(EnemyDefinition def, bool elite)> stream = new List<(EnemyDefinition, bool)>();
        float streamTimer;
        bool portalsActive, portalsDone;
        float portalTime, portalBudget, portalNext;
        int eliteCountdown = -1;

        /// <summary>Wrogowie jeszcze nadchodzą: piętro z portalami wypuszcza demony, fala czeka w kolejce albo w otwartych portalach.</summary>
        public bool PortalsRunning => (portalsActive && !portalsDone) || portals.Count > 0 || stream.Count > 0;
        /// <summary>Piętro z portalami Hell Unleashed (czas i budżet).</summary>
        public bool PortalFloor => portalsActive;
        /// <summary>Ile sekund portale będą się jeszcze otwierać (0 – koniec, trzeba dobić resztę).</summary>
        public float PortalTimeLeft => portalsActive && !portalsDone ? Mathf.Max(0f, CurrentFloor.portals.duration - portalTime) : 0f;
        /// <summary>Wypuszczone dotąd z portali na tym piętrze (dla testów i HUD).</summary>
        public int PortalSpawned { get; private set; }
        /// <summary>Wrogowie, którzy jeszcze nie wyszli (kolejka fali i otwarte portale).</summary>
        public int IncomingEnemies => stream.Count + portals.Sum(p => p.demons.Count);
        /// <summary>Otwarte portale (pozycje) – dla testów.</summary>
        public IEnumerable<Vector3> OpenPortals => portals.Select(p => p.position);

        void StartPortals(FloorDefinition floor)
        {
            portals.Clear();
            portalsActive = floor.portals != null && floor.portals.enabled && floor.portals.composition.Any(c => c.enemy != null && c.weight > 0);
            portalsDone = !portalsActive;
            portalTime = 0f;
            portalBudget = portalsActive ? floor.portals.budget : 0f;
            portalNext = 0.8f;
            PortalSpawned = 0;
            if (portalsActive) CombatEvents.RaiseMessage("Portale się otwierają – zabijaj, zanim zrobi się ciasno!", new Color(1f, 0.55f, 0.3f));
        }

        void StopPortals()
        {
            foreach (var p in portals) if (p.fx != null) Destroy(p.fx.gameObject);
            portals.Clear();
            stream.Clear();
            portalsActive = false;
            portalsDone = true;
        }

        /// <summary>Fala zwykłego piętra: wrogowie wychodzą z portali po kolei (pierwszy po chwili), boss – od razu.</summary>
        void QueueWave(List<EnemyDefinition> defs, bool elite)
        {
            for (int i = 0; i < defs.Count; i++)
            {
                var def = defs[i];
                if (def == null) continue;
                if (def.variants != null && def.variants.Count > 0 && rng.NextDouble() < def.variantChance)
                    def = def.variants[rng.Next(def.variants.Count)] ?? def;
                stream.Add((def, elite && i == 0));
            }
            if (streamTimer <= 0f || streamTimer > 0.8f) streamTimer = 0.8f;
        }

        void TickPortals(float dt)
        {
            var floor = CurrentFloor;
            if (floor == null) return;

            for (int i = portals.Count - 1; i >= 0; i--)
            {
                var p = portals[i];
                p.timer -= dt;
                if (p.timer > 0f) continue;
                portals.RemoveAt(i);
                EmergeFromPortal(floor, p);
            }

            // Zwykła fala: kolejny wróg co kilka sekund, każdy z innego miejsca.
            if (stream.Count > 0)
            {
                streamTimer -= dt;
                if (streamTimer <= 0f)
                {
                    streamTimer = StreamInterval;
                    var next = stream[0];
                    stream.RemoveAt(0);
                    var portal = new PendingPortal { timer = PortalDelay };
                    portal.demons.Add(next);
                    OpenAt(floor, portal, floor.portals != null ? floor.portals.minDistance : 7f);
                }
            }

            if (!portalsActive) return;
            var s = floor.portals;
            portalTime += dt;
            if (!portalsDone)
            {
                float cheapest = s.composition.Where(c => c.enemy != null && c.weight > 0).Min(c => Mathf.Max(0.01f, c.enemy.spawnCost));
                if (portalTime >= s.duration || portalBudget < cheapest - 1e-4f)
                {
                    portalsDone = true;
                    if (Alive > 0 || portals.Count > 0) CombatEvents.RaiseMessage("Portale gasną – dobij pozostałe demony", new Color(1f, 0.8f, 0.4f));
                }
                else
                {
                    portalNext -= dt;
                    int waiting = portals.Sum(p => p.demons.Count);
                    if (portalNext <= 0f && Alive + waiting < s.maxAlive)
                    {
                        portalNext = s.groupInterval;
                        OpenPortal(floor);
                    }
                }
            }

            if (portalsDone && portals.Count == 0 && Alive == 0 && clearTimer < 0 && Player != null && !Player.IsDead)
                clearTimer = 1.5f;
        }

        int Alive => enemies.Count(e => e != null && !e.IsDead);

        void OpenPortal(FloorDefinition floor)
        {
            var s = floor.portals;
            int size = rng.Next(Mathf.Max(1, s.minGroup), Mathf.Max(s.minGroup, s.maxGroup) + 1);
            var portal = new PendingPortal { timer = PortalDelay };
            for (int i = 0; i < size; i++)
            {
                var def = PickPortalDemon(s);
                if (def == null) break;
                portalBudget -= Mathf.Max(0.01f, def.spawnCost);
                // Stado (czerwie) wychodzi razem za jeden koszt; elitą może być tylko pierwszy.
                for (int k = 0; k < Mathf.Max(1, def.packSize); k++)
                    portal.demons.Add((def, k == 0 && RollElite(s, def)));
            }
            if (portal.demons.Count == 0) return;
            OpenAt(floor, portal, s.minDistance);
        }

        /// <summary>Miejsce portalu, kierunek wyjścia (do bohatera) i efekt – wysokość według najwyższego wroga.</summary>
        void OpenAt(FloorDefinition floor, PendingPortal portal, float minDistance)
        {
            Vector3 player = Player != null ? Player.transform.position : Vector3.zero;
            portal.position = WorldBuilder.PortalPoint(floor.arena, player, minDistance, rng);
            portal.facing = player - portal.position; portal.facing.y = 0f;
            if (portal.facing.sqrMagnitude < 1e-3f) portal.facing = -portal.position;
            portal.facing.Normalize();
            float tallest = portal.demons.Max(d => d.def.scale) * 1.8f;
            bool elite = portal.demons.Any(d => d.elite);
            portal.fx = DemonPortal.Open(new Vector3(portal.position.x, 0.02f, portal.position.z), portal.facing,
                Mathf.Clamp(tallest * 1.3f, 2.2f, 3.6f), PortalDelay + 0.9f, elite);
            portals.Add(portal);
        }

        EnemyDefinition PickPortalDemon(PortalSpawning s)
        {
            var options = s.composition.Where(c => c.enemy != null && c.weight > 0 && c.enemy.spawnCost <= portalBudget + 1e-4f).ToList();
            if (options.Count == 0) return null;
            float total = options.Sum(c => c.weight);
            float roll = (float)rng.NextDouble() * total;
            foreach (var c in options)
            {
                roll -= c.weight;
                if (roll <= 0f) return c.enemy;
            }
            return options[options.Count - 1].enemy;
        }

        bool RollElite(PortalSpawning s, EnemyDefinition def)
        {
            if (def.isBoss || def.isMinion) return false;
            if (eliteCountdown < 0) eliteCountdown = rng.Next(s.eliteEvery.x, s.eliteEvery.y + 1);
            if (--eliteCountdown > 0) return false;
            eliteCountdown = rng.Next(s.eliteEvery.x, s.eliteEvery.y + 1);
            return true;
        }

        /// <summary>Wrogowie wychodzą z portalu: pojawiają się w owalu i robią krok w stronę bohatera; portal się zamyka.</summary>
        void EmergeFromPortal(FloorDefinition floor, PendingPortal p)
        {
            Vector3 side = Vector3.Cross(Vector3.up, p.facing);
            for (int i = 0; i < p.demons.Count; i++)
            {
                var (def, elite) = p.demons[i];
                // Wariant żywiołu (chochlik zawsze w jednym z żywiołów); elita HU jest zwykła.
                if (portalsActive && !elite && def.variants != null && def.variants.Count > 0 && rng.NextDouble() < def.variantChance)
                    def = def.variants[rng.Next(def.variants.Count)] ?? def;
                float lateral = p.demons.Count == 1 ? 0f : (i - (p.demons.Count - 1) * 0.5f) * 0.7f;
                var e = SpawnEnemy(def, p.position + side * lateral, floor.statScale, elite);
                e.transform.rotation = Quaternion.LookRotation(p.facing);
                e.Pull(e.transform.position + p.facing * 3f, 1.1f, 0.45f); // wyjście z portalu
                if (elite) CombatEvents.RaiseWorldText(e.AimPoint + Vector3.up * 0.8f, "ELITA", new Color(1f, 0.82f, 0.3f));
                PortalSpawned++;
            }
            FxLibrary.Dust(p.position, 1.4f);
            if (p.fx != null) p.fx.Invoke(nameof(DemonPortal.Close), 0.5f);
        }

        /// <summary>Nowy wróg na piętrze: zwrócony do bohatera, dopisany do listy piętra.</summary>
        EnemyBrain SpawnEnemy(EnemyDefinition def, Vector3 pos, float statScale, bool elite)
        {
            if (!IsEnemyTrial) Meta.RecordSeen(def);
            var e = WorldBuilder.CreateEnemy(def, pos, world);
            Vector3 look = Player != null ? Player.transform.position - pos : -pos;
            look.y = 0f;
            if (look.sqrMagnitude > 1e-3f) e.transform.rotation = Quaternion.LookRotation(look);
            e.Setup(def, Player, statScale, Run.difficulty, elite, config.balance);
            enemies.Add(e);
            return e;
        }

        /// <summary>Sługi przyzywane przez demona (nietoperze Władcy Bramy) – giną razem z nim, nie są elitami.</summary>
        void OnSummonRequested(EnemyBrain owner, EnemyDefinition def, Vector3 pos)
        {
            if (Run == null || Screen != GameScreen.Playing || def == null) return;
            var floor = CurrentFloor;
            var e = SpawnEnemy(def, pos, floor != null ? floor.statScale : 1f, false);
            e.Owner = owner;
        }
    }
}
