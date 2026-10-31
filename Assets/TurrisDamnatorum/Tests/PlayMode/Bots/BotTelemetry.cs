using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Turris.Tests.Bots
{
    /// <summary>Licznik klucz → wartość, który przechodzi przez JsonUtility (słowniki nie przechodzą).</summary>
    [Serializable]
    public class Tally
    {
        public List<string> k = new List<string>();
        public List<float> v = new List<float>();

        public void Add(string key, float val = 1f)
        {
            int i = k.IndexOf(key);
            if (i < 0) { k.Add(key); v.Add(val); } else v[i] += val;
        }

        public void Max(string key, float val)
        {
            int i = k.IndexOf(key);
            if (i < 0) { k.Add(key); v.Add(val); } else v[i] = Mathf.Max(v[i], val);
        }

        public float Get(string key) { int i = k.IndexOf(key); return i < 0 ? 0f : v[i]; }
        public IEnumerable<(string key, float val)> Items => k.Select((key, i) => (key, v[i]));
        public float Sum => v.Sum();

        public static Tally Merge(IEnumerable<Tally> all)
        {
            var t = new Tally();
            foreach (var a in all) foreach (var (key, val) in a.Items) t.Add(key, val);
            return t;
        }
    }

    [Serializable]
    public class FloorStat
    {
        public int index;
        public string name;
        public float time, hpLost, damageTaken, damageDealt;
        public int flasks, kills, waves;
        public bool cleared;
    }

    /// <summary>Pełny zapis jednego podejścia bota.</summary>
    [Serializable]
    public class BotRunRecord
    {
        public string className, classId, persona, loadout, startSkill;
        public int run, seed;
        /// <summary>„Zwycięstwo”, „Śmierć” albo „Limit czasu”.</summary>
        public string result;
        public int floorReached, wave;
        public string floorName, killer, killerAttack;
        public float killerHitDamage;
        public float gameTime, wallTime, wallMsPerFrameAvg, wallMsPerFrameMax;
        public float damageDealt, damageTaken, healed;
        public int kills, souls, ash, flasksUsed, flasksLeft, stuckEvents;
        /// <summary>Próby picia (wejście w akcję) – część przerywa trafienie, zanim flaszka zadziała.</summary>
        public int flaskAttempts;
        /// <summary>Śmierć: obrażenia w ostatnich 3 s jako ułamek maks. HP i HP na początku tego okna.</summary>
        public float deathBurst3s, hpBeforeBurst;
        public float minHpFraction = 1f, timeBlocking, timeSprinting, timeLowStamina, distanceMoved, maxHealth;
        public Tally actions = new Tally();          // wykonane akcje (zmiana stanu gracza)
        public Tally skillUses = new Tally();        // użycia umiejętności po nazwie
        public Tally skillLevelSum = new Tally();    // suma poziomów skilla przy użyciach (średni poziom = suma / użycia)
        public Tally skillUsesByOrigin = new Tally(); // „skill|pochodzenie” → użycia (startowy, odblokowanie, zestaw, nagroda, sklep, przedmiot)
        public Tally skillSwaps = new Tally();       // „wyjęty → włożony” przy układaniu slotów
        public Tally shopOffers = new Tally();       // oferty sklepu dusz: widziana / kupiona / odrzucona / za mało dusz
        public Tally damageBySource = new Tally();   // obrażenia zadane wg nazwy ataku/umiejętności
        public Tally incoming = new Tally();         // telegraf|wynik
        public Tally responses = new Tally();        // reakcja|telegraf|wynik
        public Tally enemyKills = new Tally();
        public Tally enemyEngagedKills = new Tally(); // zabici z co najmniej jednym trafieniem bohatera (mianownik czasu/trafień)
        public Tally enemyTtkSum = new Tally();      // suma czasu od pierwszego trafienia do śmierci
        public Tally enemyHitsSum = new Tally();     // suma trafień bohatera potrzebnych do zabicia
        public Tally enemyDamageToPlayer = new Tally();
        public Tally enemyHitsOnPlayer = new Tally();
        public Tally windupSum = new Tally();        // „wróg: atak” → suma czasu zamachu (s)
        public Tally windupN = new Tally();
        public Tally reactionMargin = new Tally();
        public Tally lowHpTime = new Tally();        // niskie życie z flaszką: stan bota → sekundy
        public Tally dodgeReasons = new Tally();     // powód naciśnięcia uniku (i odmowy gry)   // telegraf|zapas czasu po reakcji bota
        public Tally messages = new Tally();         // komunikaty gry (np. „Za mało many”)
        public Tally attributes = new Tally();
        public Tally purchases = new Tally();        // sklep dusz / kapliczka
        public Tally anomalies = new Tally();        // rodzaj anomalii → liczba
        public List<string> anomalyNotes = new List<string>();
        public List<FloorStat> floors = new List<FloorStat>();
        public List<string> metaUnlocks = new List<string>();
        public Tally seals = new Tally();            // pieczęcie: „dar: X”, „z sakwy: X”, „kupiona: X”
        public Tally floorRetries = new Tally();     // piętro → ponowne próby po śmierci
        public List<string> retryNotes = new List<string>();
        public List<string> challenges = new List<string>(); // zapisane wyzwania (piętro nieprzejęte w trzech próbach)
        public List<string> replays = new List<string>();    // powtórki ostatecznych porażek (powtorki/*.html)

        public bool Victory => result == "Zwycięstwo";

        public void Note(string kind, string note)
        {
            anomalies.Add(kind);
            if (anomalyNotes.Count < 40 && !anomalyNotes.Contains(note)) anomalyNotes.Add(note);
        }
    }

    [Serializable]
    public class BotSeries
    {
        public string className, classId, persona, personaName, personaDescription, generated;
        public List<BotRunRecord> runs = new List<BotRunRecord>();
    }

    /// <summary>
    /// Zbiera telemetrię jednego podejścia: zdarzenia walki, śmierci, komunikaty i błędy z logu.
    /// Bot dopisuje własne decyzje (reakcje, akcje, zakupy).
    /// </summary>
    public class BotRunRecorder : IDisposable
    {
        public readonly BotRunRecord R;
        readonly GameRoot root;
        readonly Dictionary<EnemyBrain, (float first, int hits)> engaged = new Dictionary<EnemyBrain, (float, int)>();
        FloorStat floor;
        float floorStart, floorHpStart;
        string lastHitter, lastAttack;
        float lastHitDamage;
        float wallStart;
        int frames;
        float frameMsSum;
        public HeroBot Bot;

        public BotRunRecorder(GameRoot root, BotRunRecord record)
        {
            this.root = root;
            R = record;
            wallStart = Time.realtimeSinceStartup;
            CombatEvents.Impact += OnImpact;
            CombatEvents.Died += OnDied;
            CombatEvents.Message += OnMessage;
            Application.logMessageReceived += OnLog;
        }

        public void Dispose()
        {
            CombatEvents.Impact -= OnImpact;
            CombatEvents.Died -= OnDied;
            CombatEvents.Message -= OnMessage;
            Application.logMessageReceived -= OnLog;
        }

        public static string KindName(TelegraphKind k)
        {
            switch (k)
            {
                case TelegraphKind.Heavy: return "ciężki";
                case TelegraphKind.Unparryable: return "nieparowalny";
                case TelegraphKind.Unblockable: return "nieblokowalny";
                case TelegraphKind.NoDodge: return "obszarowy";
                default: return "zwykły";
            }
        }

        public static TelegraphKind KindOf(in HitData h) =>
            !h.dodgeable ? TelegraphKind.NoDodge : !h.blockable ? TelegraphKind.Unblockable : !h.parryable ? TelegraphKind.Unparryable
            : h.heavy ? TelegraphKind.Heavy : TelegraphKind.Normal;

        public static string OutcomeName(HitOutcome o)
        {
            switch (o)
            {
                case HitOutcome.Hit: return "trafienie";
                case HitOutcome.Dodged: return "unik";
                case HitOutcome.Parried: return "parowanie";
                case HitOutcome.Blocked: return "blok";
                case HitOutcome.GuardBroken: return "przełamana garda";
                default: return "zignorowane";
            }
        }

        public static string ResponseName(DefResponse r)
        {
            switch (r)
            {
                case DefResponse.Block: return "blok";
                case DefResponse.Parry: return "parowanie";
                case DefResponse.Dodge: return "unik";
                case DefResponse.StepOut: return "wyjście z zasięgu";
                default: return "brak reakcji";
            }
        }

        // ------------------------------------------------------------------ Piętra

        public void FloorStarted()
        {
            var f = root.CurrentFloor;
            floor = new FloorStat { index = root.Run.floorIndex, name = f != null ? f.name : "?", waves = f != null ? f.WaveCount : 0 };
            R.floors.Add(floor);
            floorStart = Time.time;
            floorHpStart = root.Player.Health.Current;
            R.floorReached = floor.index + 1;
            R.floorName = floor.name;
        }

        public void FloorEnded(bool cleared)
        {
            if (floor == null) return;
            floor.time = Time.time - floorStart;
            floor.cleared = cleared;
            floor.hpLost = Mathf.Max(0f, floorHpStart - root.Player.Health.Current);
            floor = null;
        }

        public float FloorTime => floor != null ? Time.time - floorStart : 0f;

        public void Frame(float wallDtMs)
        {
            FlushKills();
            frames++;
            frameMsSum += wallDtMs;
            R.wallMsPerFrameMax = Mathf.Max(R.wallMsPerFrameMax, wallDtMs);
            var pc = root.Player;
            if (pc != null && pc.Health != null) { R.minHpFraction = Mathf.Min(R.minHpFraction, pc.Health.Fraction); TrackFlasksAndHp(pc); }
            if (pc != null && pc.Stamina != null && pc.Stamina.Fraction < 0.1f) R.timeLowStamina += Time.deltaTime;
        }

        public void FlaskAttempt() => R.flaskAttempts++;

        int lastFlasks = -1;
        readonly Queue<(float t, float hp)> hpTrace = new Queue<(float, float)>();

        void TrackFlasksAndHp(PlayerCombat pc)
        {
            int f = root.Run.healthFlasks;
            if (lastFlasks >= 0 && f < lastFlasks) { R.flasksUsed += lastFlasks - f; if (floor != null) floor.flasks += lastFlasks - f; }
            lastFlasks = f;
            hpTrace.Enqueue((Time.time, pc.Health.Current));
            while (hpTrace.Count > 0 && Time.time - hpTrace.Peek().t > 3f) hpTrace.Dequeue();
        }

        public void Finish(string result)
        {
            FloorEnded(result == "Zwycięstwo");
            R.result = result;
            R.wave = root.WaveIndex + 1;
            var run = root.Run; // null, gdy gra wróciła do menu
            if (run != null) { R.kills = run.kills; R.souls = run.souls; R.ash = run.ashEarned; R.flasksLeft = run.healthFlasks; }
            R.maxHealth = root.Player.Health.Max;
            R.gameTime = R.floors.Sum(f => f.time);
            R.wallTime = Time.realtimeSinceStartup - wallStart;
            R.wallMsPerFrameAvg = frames > 0 ? frameMsSum / frames : 0f;
            if (result == "Śmierć" && hpTrace.Count > 0 && R.maxHealth > 0f)
            {
                R.hpBeforeBurst = hpTrace.Peek().hp / R.maxHealth;
                R.deathBurst3s = hpTrace.Max(x => x.hp) / R.maxHealth;
            }
            if (result == "Śmierć") { R.killer = lastHitter ?? "?"; R.killerAttack = lastAttack ?? "?"; R.killerHitDamage = lastHitDamage; }
        }

        // ------------------------------------------------------------------ Zdarzenia

        void OnImpact(IHitReceiver target, HitData hit, HitResult r)
        {
            var pc = root.Player;
            if (pc == null) return;
            if (ReferenceEquals(target, pc))
            {
                var kind = KindOf(hit);
                string kn = KindName(kind), on = OutcomeName(r.outcome);
                var src = hit.attacker as EnemyBrain;
                string enemy = src != null && src.Def != null ? src.Def.displayName : "?";
                if (!hit.isStatusTick) R.incoming.Add($"{kn}|{on}");
                R.damageTaken += r.healthDamage;
                if (floor != null) floor.damageTaken += r.healthDamage;
                R.enemyDamageToPlayer.Add(enemy, r.healthDamage);
                if (!hit.isStatusTick) R.enemyHitsOnPlayer.Add(enemy);
                if (r.healthDamage > 0f) { lastHitter = enemy; lastAttack = hit.isStatusTick ? "efekt (tyknięcie)" : hit.attackName; lastHitDamage = r.healthDamage; }
                if (!hit.isStatusTick) Bot?.OnPlayerImpact(src, hit, r, kind);
                CheckInvariants(pc, hit, r, kind, enemy);
            }
            else if (target is EnemyBrain e && (ReferenceEquals(hit.attacker, pc) || hit.attacker == null))
            {
                R.damageDealt += r.healthDamage;
                if (floor != null) floor.damageDealt += r.healthDamage;
                R.damageBySource.Add(string.IsNullOrEmpty(hit.attackName) ? "?" : hit.attackName, r.healthDamage);
                if (!hit.isStatusTick) engaged[e] = engaged.TryGetValue(e, out var st) ? (st.first, st.hits + 1) : (Time.time, 1);
                // Śmierć ogłaszana jest przed zdarzeniem trafienia – cios kończący domyka statystykę tutaj.
                if (pendingDead.Remove(e)) CloseKill(e);
            }
        }

        void CheckInvariants(PlayerCombat pc, in HitData hit, in HitResult r, TelegraphKind kind, string enemy)
        {
            if (hit.isStatusTick) return;
            if (r.outcome == HitOutcome.Blocked && !hit.blockable) R.Note("reguła: zablokowano nieblokowalny", $"{enemy}: „{hit.attackName}” zablokowany mimo blockable=false");
            if (r.outcome == HitOutcome.Dodged && !hit.dodgeable) R.Note("reguła: unik przed obszarowym", $"{enemy}: „{hit.attackName}” uniknięty mimo dodgeable=false");
            if (r.outcome == HitOutcome.Parried && !hit.parryable) R.Note("reguła: sparowano nieparowalny", $"{enemy}: „{hit.attackName}” sparowany mimo parryable=false");
            // Okno parowania było aktywne, cios do sparowania – a i tak trafił. Zwykle: cios z boku/tyłu.
            var act = pc.Actions;
            Vector3 to = hit.sourcePosition - pc.transform.position; to.y = 0f;
            float angle = to.sqrMagnitude > 1e-4f ? Vector3.Angle(pc.transform.forward, to) : 0f;
            if (act.Current == ActionType.Parry && act.Phase == ActionPhase.Active && hit.parryable && r.outcome != HitOutcome.Parried)
                R.Note("parowanie w oknie nie zadziałało", $"{enemy}: „{hit.attackName}” trafił w aktywnym oknie parowania (kąt do źródła {angle:0}°)");
            if (act.Current == ActionType.Block && hit.blockable && r.outcome == HitOutcome.Hit)
                R.Note("garda nie zatrzymała ciosu", $"{enemy}: „{hit.attackName}” przeszedł przez gardę (kąt {angle:0}°)");
            if (r.outcome == HitOutcome.Hit && angle > 110f) R.anomalies.Add("trafienie z tyłu (poza polem widzenia)");
            if (float.IsNaN(r.healthDamage) || r.healthDamage < 0f) R.Note("obrażenia NaN/ujemne", $"{enemy}: „{hit.attackName}” → {r.healthDamage}");
        }

        void OnDied(IHitReceiver who)
        {
            if (who is EnemyBrain e && e.Def != null)
            {
                R.enemyKills.Add(KillName(e));
                if (floor != null) floor.kills++;
                pendingDead.Add(e);
            }
        }

        readonly HashSet<EnemyBrain> pendingDead = new HashSet<EnemyBrain>();

        static string KillName(EnemyBrain e) => e.IsElite ? e.Def.displayName + " (elita)" : e.Def.displayName;

        void CloseKill(EnemyBrain e)
        {
            if (e == null || !engaged.TryGetValue(e, out var st)) return; // zabity bez trafienia bohatera (np. efekt, sługa)
            string n = KillName(e);
            R.enemyEngagedKills.Add(n);
            R.enemyTtkSum.Add(n, Time.time - st.first);
            R.enemyHitsSum.Add(n, st.hits);
            engaged.Remove(e);
        }

        /// <summary>Śmierci bez ciosu kończącego w tej klatce (tyknięcia efektów) – domknięcie na końcu klatki.</summary>
        void FlushKills()
        {
            if (pendingDead.Count == 0) return;
            foreach (var e in pendingDead) CloseKill(e);
            pendingDead.Clear();
        }

        void OnMessage(string msg, Color c)
        {
            if (string.IsNullOrEmpty(msg)) return;
            // Nazwy pięter i numery fal są zmienne – zostawiamy tylko komunikaty o odmowie akcji i odkryciach.
            if (msg.StartsWith("Fala") || msg.Contains("ukończone") || msg.StartsWith("Nowa umiejętność")) return;
            if (root.CurrentFloor != null && msg == root.CurrentFloor.name) return;
            R.messages.Add(msg.Length > 70 ? msg.Substring(0, 70) : msg);
        }

        void OnLog(string condition, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            string first = condition.Split('\n')[0];
            if (first.Length > 160) first = first.Substring(0, 160);
            R.Note(type == LogType.Exception ? "wyjątek w logu" : "błąd w logu", first);
        }
    }
}
