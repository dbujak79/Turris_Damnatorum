using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Turris.Tests.Bots
{
    /// <summary>Zapis jednej próby piętra (widok z góry): klatki co 0,1 s czasu gry i zdarzenia walki.</summary>
    [Serializable]
    public class BotReplay
    {
        [Serializable] public class EnemyType { public string id, name; public bool boss; public float radius; }
        /// <summary>Klatka: bohater i spakowani wrogowie (uid, typ, x, z, życie 0–1, flagi: 1 elita, 2 atakuje, 4 martwy) oraz pociski (x, z, strona).</summary>
        [Serializable] public class Frame { public float t, x, z, yaw, hp, st; public string act; public List<float> e = new List<float>(), p = new List<float>(); }
        [Serializable] public class Event { public float t, x, z, v; public string kind, text; }

        public string title, className, persona, floorName, result, killer, created;
        public int floorIndex, attempt, run;
        public float heroMaxHp, heroMaxSt;
        public List<EnemyType> types = new List<EnemyType>();
        public List<Frame> frames = new List<Frame>();
        public List<Event> events = new List<Event>();
    }

    /// <summary>
    /// Nagrywa próbę piętra w trakcie serii botów (działa w trybie wsadowym bez grafiki – czyta stan gry, nie obraz).
    /// Ostateczna porażka na piętrze trafia do odtwarzacza HTML (<c>tools/replay/powtorka_szablon.html</c>) obok raportu.
    /// </summary>
    public sealed class BotReplayRecorder : IDisposable
    {
        const float Step = 0.1f;
        readonly GameRoot root;
        BotReplay replay;
        float start, next;
        ActionType lastAction = ActionType.None;
        readonly Dictionary<EnemyBrain, int> uid = new Dictionary<EnemyBrain, int>();
        readonly Dictionary<EnemyDefinition, int> typeIndex = new Dictionary<EnemyDefinition, int>();

        public BotReplayRecorder(GameRoot root)
        {
            this.root = root;
            CombatEvents.Impact += OnImpact;
            CombatEvents.Died += OnDied;
        }

        public void Dispose()
        {
            CombatEvents.Impact -= OnImpact;
            CombatEvents.Died -= OnDied;
        }

        float Now => Time.time - start;

        /// <summary>Nowa próba piętra – poprzedni zapis przepada (zostaje tylko ostatnia, ostateczna porażka).</summary>
        public void Begin(BotRunRecord record, int attempt)
        {
            var pc = root.Player;
            replay = new BotReplay
            {
                className = record.className, persona = record.persona, run = record.run, attempt = attempt,
                floorIndex = root.Run.floorIndex, floorName = root.CurrentFloor?.name ?? "?",
                heroMaxHp = pc.Health.Max, heroMaxSt = pc.Stamina.Max, created = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            };
            uid.Clear(); typeIndex.Clear();
            start = Time.time; next = 0f;
            lastAction = ActionType.None;
        }

        int TypeOf(EnemyDefinition d)
        {
            if (typeIndex.TryGetValue(d, out int i)) return i;
            i = replay.types.Count;
            replay.types.Add(new BotReplay.EnemyType { id = d.id, name = d.displayName, boss = d.isBoss, radius = 0.45f * Mathf.Max(0.6f, d.scale) });
            typeIndex[d] = i;
            return i;
        }

        void Add(string kind, string text, Vector3 pos, float v = 0f) =>
            replay?.events.Add(new BotReplay.Event { t = Now, kind = kind, text = text, x = pos.x, z = pos.z, v = v });

        /// <summary>Wywoływane co klatkę w trakcie walki: próbka co 0,1 s i zdarzenia akcji bohatera.</summary>
        public void Tick()
        {
            if (replay == null || root.Player == null) return;
            var pc = root.Player;
            var cur = pc.Actions.Current;
            if (cur != lastAction)
            {
                if (cur == ActionType.Cast && pc.ActiveSkill != null) Add("skill", pc.ActiveSkill.Def.displayName, pc.transform.position);
                else if (cur == ActionType.Dodge) Add("dodge", "unik", pc.transform.position);
                else if (cur == ActionType.Flask) Add("flask", "flaszka", pc.transform.position);
                lastAction = cur;
            }
            if (Now < next) return;
            next = Now + Step;
            var f = new BotReplay.Frame
            {
                t = Now, x = pc.transform.position.x, z = pc.transform.position.z, yaw = pc.transform.eulerAngles.y,
                hp = pc.Health.Current, st = pc.Stamina.Current, act = cur == ActionType.None ? "" : cur.ToString(),
            };
            foreach (var e in EnemyBrain.All)
            {
                if (e == null || e.Def == null) continue;
                if (!uid.TryGetValue(e, out int id)) { id = uid.Count + 1; uid[e] = id; }
                int flags = (e.IsElite ? 1 : 0) | (e.CurrentState == EnemyBrain.State.Attacking ? 2 : 0) | (e.IsDead ? 4 : 0);
                var p = e.transform.position;
                f.e.Add(id); f.e.Add(TypeOf(e.Def)); f.e.Add(Round(p.x)); f.e.Add(Round(p.z)); f.e.Add(Round(e.Health.Fraction)); f.e.Add(flags);
            }
            foreach (var pr in Projectile.Active)
            {
                if (pr == null) continue;
                var p = pr.transform.position;
                f.p.Add(Round(p.x)); f.p.Add(Round(p.z)); f.p.Add(pr.Side == Faction.Player ? 1 : 0);
            }
            replay.frames.Add(f);
        }

        static float Round(float v) => Mathf.Round(v * 100f) / 100f;

        void OnImpact(IHitReceiver target, HitData hit, HitResult result)
        {
            if (replay == null || hit.isStatusTick) return;
            if (target is PlayerCombat pc)
            {
                string who = hit.attacker is EnemyBrain eb && eb.Def != null ? eb.Def.displayName : "?";
                Add("hurt", $"{who}: {hit.attackName} – {result.outcome}", pc.transform.position, result.healthDamage);
            }
            else if (target is EnemyBrain e && hit.attacker is PlayerCombat && result.healthDamage > 0f)
                Add("hit", $"{hit.attackName} → {e.Def?.displayName}", e.transform.position, result.healthDamage);
        }

        void OnDied(IHitReceiver who)
        {
            if (replay == null) return;
            if (who is EnemyBrain e) Add("kill", e.Def?.displayName ?? "?", e.transform.position);
            else if (who is PlayerCombat pc) Add("death", "bohater poległ", pc.transform.position);
        }

        /// <summary>Ostatnie trafienie w bohatera w tej próbie („Kultysta: Kula ognia – Hit”) albo null.</summary>
        public string LastHurt() => replay?.events.LastOrDefault(e => e.kind == "hurt")?.text;

        /// <summary>Zapisuje próbę jako odtwarzacz HTML (dane wklejone w szablon) i JSON obok.</summary>
        public string Save(string dir, string result, string killer, string title)
        {
            if (replay == null) return null;
            // Zabójca z telemetrii bywa jeszcze pusty w chwili śmierci – wtedy ostatnie trafienie w bohatera.
            var lastHurt = replay.events.LastOrDefault(e => e.kind == "hurt");
            if (string.IsNullOrWhiteSpace(killer?.Replace("–", "")) && lastHurt != null) killer = lastHurt.text;
            replay.result = result; replay.killer = killer; replay.title = title;
            string folder = Path.Combine(dir, "powtorki");
            Directory.CreateDirectory(folder);
            string name = $"{replay.className}_{replay.persona}_podejscie{replay.run:00}_pietro{replay.floorIndex + 1:00}".Replace(' ', '_');
            string json = JsonUtility.ToJson(replay);
            File.WriteAllText(Path.Combine(folder, name + ".json"), json);
            string template = Path.Combine(Application.dataPath, "..", "tools", "replay", "powtorka_szablon.html");
            if (File.Exists(template))
                File.WriteAllText(Path.Combine(folder, name + ".html"), File.ReadAllText(template).Replace("/*DANE_POWTORKI*/null", json));
            return name;
        }
    }
}
