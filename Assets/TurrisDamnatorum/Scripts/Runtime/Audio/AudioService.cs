using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Krótki dostęp do dźwięku z dowolnego miejsca w kodzie gry. Bez <see cref="AudioService"/> (np. w testach EditMode) nic nie robi.
    /// </summary>
    public static class Sfx
    {
        /// <summary>Efekt w świecie (przestrzenny zgodnie z <see cref="SoundInfo.spatial"/>).</summary>
        public static void Play(SoundId id, Vector3 pos, float volume = 1f, float pitch = 1f) => AudioService.Instance?.PlayAt(id, pos, volume, pitch);
        /// <summary>Efekt bez położenia (interfejs, zdarzenia gry).</summary>
        public static void Ui(SoundId id, float volume = 1f, float pitch = 1f) => AudioService.Instance?.PlayAt(id, null, volume, pitch);

        /// <summary>Dźwięk wypuszczenia umiejętności dobrany do rodzaju i żywiołu.</summary>
        public static SoundId ForElement(Element e, SoundId none)
        {
            switch (e)
            {
                case Element.Fire: return SoundId.FireCast;
                case Element.Frost: return SoundId.FrostCast;
                case Element.Lightning: return SoundId.LightningZap;
                case Element.Earth: return SoundId.Quake;
                default: return none;
            }
        }

        public static SoundId ImpactFor(Element e)
        {
            switch (e)
            {
                case Element.Fire: return SoundId.FireExplosion;
                case Element.Frost: return SoundId.FrostImpact;
                case Element.Lightning: return SoundId.LightningZap;
                case Element.Earth: return SoundId.HitBlunt;
                default: return SoundId.ArcaneImpact;
            }
        }
    }

    /// <summary>
    /// Dźwięk gry: synteza efektów i muzyki w wątku tła (raz na uruchomienie), pula źródeł z limitem głosów,
    /// muzyka i tło areny z przenikaniem, pogłos zależny od areny, bicie serca przy niskim zdrowiu.
    ///
    /// Pliki zastępcze: Resources/Audio/&lt;SoundId&gt; (plik lub folder z wariantami) oraz Resources/Audio/Music/&lt;MusicId&gt;
    /// mają pierwszeństwo przed syntezą – dźwięk docelowy podmienia się bez zmian w kodzie.
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        // ------------------------------------------------------------------ Wspólny (statyczny) bank dźwięków

        struct Generated { public int sound; public int variant; public int music; public float[] data; }

        static readonly int SoundCount = Enum.GetValues(typeof(SoundId)).Length;
        static readonly int MusicCount = Enum.GetValues(typeof(MusicId)).Length;
        static AudioClip[][] sfxClips;
        static AudioClip[] musicClips;
        static bool[] sfxOverridden, musicOverridden;
        static readonly ConcurrentQueue<Generated> ready = new ConcurrentQueue<Generated>();
        static Task generation;
        static int pendingSfx, pendingMusic;

        /// <summary>Wszystkie efekty gotowe (zsyntezowane lub wczytane z plików).</summary>
        public static bool SfxReady => sfxClips != null && pendingSfx == 0;
        public static bool MusicReady => musicClips != null && pendingMusic == 0;
        public static int LoadedSfxCount { get; private set; }

        public static AudioClip Clip(SoundId id, int variant = 0)
        {
            var arr = sfxClips?[(int)id];
            if (arr == null || arr.Length == 0) return null;
            return arr[Mathf.Clamp(variant, 0, arr.Length - 1)];
        }

        public static AudioClip Music(MusicId id) => musicClips?[(int)id];

        static void EnsureBank()
        {
            if (sfxClips != null) return;
            sfxClips = new AudioClip[SoundCount][];
            musicClips = new AudioClip[MusicCount];
            sfxOverridden = new bool[SoundCount];
            musicOverridden = new bool[MusicCount];

            // Pliki zastępcze z Resources.
            for (int i = 0; i < SoundCount; i++)
            {
                string name = ((SoundId)i).ToString();
                var clips = Resources.LoadAll<AudioClip>("Audio/" + name);
                if (clips == null || clips.Length == 0)
                {
                    var single = Resources.Load<AudioClip>("Audio/" + name);
                    clips = single != null ? new[] { single } : null;
                }
                if (clips != null && clips.Length > 0) { sfxClips[i] = clips; sfxOverridden[i] = true; }
            }
            for (int i = 1; i < MusicCount; i++)
            {
                var c = Resources.Load<AudioClip>("Audio/Music/" + (MusicId)i);
                if (c != null) { musicClips[i] = c; musicOverridden[i] = true; }
            }

            // Synteza reszty w tle: najpierw efekty, potem muzyka menu, tła aren, walka, boss.
            var jobs = new System.Collections.Generic.List<(int s, int v)>();
            for (int i = 0; i < SoundCount; i++)
            {
                if (sfxOverridden[i]) continue;
                int n = Mathf.Max(1, SoundInfo.For((SoundId)i).variants);
                sfxClips[i] = new AudioClip[0];
                for (int v = 0; v < n; v++) jobs.Add((i, v));
            }
            var music = new System.Collections.Generic.List<int>();
            foreach (var m in new[] { MusicId.Menu, MusicId.AmbSummit, MusicId.AmbCourtyard, MusicId.AmbCrypt, MusicId.Combat, MusicId.Boss })
                if (!musicOverridden[(int)m]) music.Add((int)m);
            pendingSfx = jobs.Count;
            pendingMusic = music.Count;
            generation = Task.Run(() =>
            {
                foreach (var (s, v) in jobs)
                {
                    float[] data;
                    try { data = SoundSynth.Generate((SoundId)s, v); }
                    catch (Exception) { data = new float[64]; }
                    ready.Enqueue(new Generated { sound = s, variant = v, music = -1, data = data });
                }
                foreach (var m in music)
                {
                    float[] data;
                    try { data = MusicSynth.Generate((MusicId)m); }
                    catch (Exception) { data = new float[128]; }
                    ready.Enqueue(new Generated { sound = -1, music = m, data = data });
                }
            });
        }

        /// <summary>Przenosi gotowe dane do klipów (tylko w wątku głównym).</summary>
        static void DrainReady(int budget)
        {
            while (budget-- > 0 && ready.TryDequeue(out var g))
            {
                if (g.sound >= 0)
                {
                    var id = (SoundId)g.sound;
                    var clip = AudioClip.Create($"{id}_{g.variant}", g.data.Length, 1, SoundSynth.SampleRate, false);
                    clip.SetData(g.data, 0);
                    clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    var arr = sfxClips[g.sound];
                    Array.Resize(ref arr, arr.Length + 1);
                    arr[arr.Length - 1] = clip;
                    sfxClips[g.sound] = arr;
                    pendingSfx--;
                    LoadedSfxCount++;
                }
                else
                {
                    var id = (MusicId)g.music;
                    var clip = AudioClip.Create(id.ToString(), g.data.Length / 2, 2, MusicSynth.SampleRate, false);
                    clip.SetData(g.data, 0);
                    clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    musicClips[g.music] = clip;
                    pendingMusic--;
                    budget = 0; // muzyka jest duża – jedna na klatkę
                }
            }
        }

        // ------------------------------------------------------------------ Głośność

        public float MasterVolume { get; private set; } = 0.8f;
        public float MusicVolume { get; private set; } = 0.7f;
        public float SfxVolume { get; private set; } = 0.9f;

        public void SetVolumes(float master, float music, float sfx)
        {
            MasterVolume = Mathf.Clamp01(master);
            MusicVolume = Mathf.Clamp01(music);
            SfxVolume = Mathf.Clamp01(sfx);
            AudioListener.volume = MasterVolume;
        }

        // ------------------------------------------------------------------ Pula źródeł

        const int PoolSize = 40;
        AudioSource[] pool;
        SoundId[] poolIds;
        float[] poolStart;
        float[] lastPlayed;
        int[] lastVariant;

        /// <summary>Ile razy odtworzono dany efekt (do testów i diagnostyki).</summary>
        public int[] PlayCounts { get; private set; }

        void Awake()
        {
            // Nowa gra (np. kolejny test) przejmuje rolę – poprzednia instancja zaraz zniknie razem ze swoim obiektem.
            Instance = this;
            EnsureBank();
            DrainReady(64);

            pool = new AudioSource[PoolSize];
            poolIds = new SoundId[PoolSize];
            poolStart = new float[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                var go = new GameObject("Sfx" + i);
                go.transform.SetParent(transform, false);
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.dopplerLevel = 0f;
                s.rolloffMode = AudioRolloffMode.Logarithmic;
                s.minDistance = 7f;
                s.maxDistance = 60f;
                pool[i] = s;
            }
            lastPlayed = new float[SoundCount];
            for (int i = 0; i < lastPlayed.Length; i++) lastPlayed[i] = -10f;
            lastVariant = new int[SoundCount];
            PlayCounts = new int[SoundCount];

            music = new Layer(gameObject, "Music");
            ambience = new Layer(gameObject, "Ambience");

            var zoneGo = new GameObject("Reverb");
            zoneGo.transform.SetParent(transform, false);
            zone = zoneGo.AddComponent<AudioReverbZone>();
            zone.minDistance = 400f;
            zone.maxDistance = 500f;
            zone.reverbPreset = AudioReverbPreset.Mountains;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            AudioListener.pause = false;
        }

        /// <summary>Odtwarza efekt; pos == null – bez położenia (2D). Zwraca źródło albo null (brak klipu, limit).</summary>
        public AudioSource PlayAt(SoundId id, Vector3? pos, float volume = 1f, float pitch = 1f)
        {
            int k = (int)id;
            PlayCounts[k]++;
            var clips = sfxClips[k];
            if (clips == null || clips.Length == 0) return null;
            var info = SoundInfo.For(id);
            float now = Time.unscaledTime;
            if (now - lastPlayed[k] < info.minInterval) return null;

            // Limit głosów: ten sam efekt po raz (maxVoices+1) zastępuje swoje najstarsze odtworzenie.
            int same = 0, oldestSame = -1, free = -1, oldest = 0;
            for (int i = 0; i < pool.Length; i++)
            {
                bool busy = pool[i].isPlaying;
                if (busy && poolIds[i] == id)
                {
                    same++;
                    if (oldestSame < 0 || poolStart[i] < poolStart[oldestSame]) oldestSame = i;
                }
                if (!busy && free < 0) free = i;
                if (poolStart[i] < poolStart[oldest]) oldest = i;
            }
            int slot = same >= info.maxVoices ? oldestSame : free >= 0 ? free : oldest;

            int variant = 0;
            if (clips.Length > 1)
            {
                variant = UnityEngine.Random.Range(0, clips.Length - 1);
                if (variant >= lastVariant[k]) variant++; // nigdy ten sam wariant dwa razy z rzędu
            }
            lastVariant[k] = variant;
            lastPlayed[k] = now;

            var s = pool[slot];
            s.Stop();
            s.clip = clips[variant];
            float bus = info.bus == SoundBus.Sfx ? SfxVolume : info.bus == SoundBus.Ui ? SfxVolume : Mathf.Max(SfxVolume, MusicVolume);
            s.volume = Mathf.Clamp01(info.volume * volume * bus);
            s.pitch = pitch * (1f + UnityEngine.Random.Range(-info.pitchJitter, info.pitchJitter));
            s.ignoreListenerPause = info.bus != SoundBus.Sfx;
            s.reverbZoneMix = info.bus == SoundBus.Sfx ? 1f : 0f;
            if (pos.HasValue && info.spatial > 0f)
            {
                s.transform.position = pos.Value;
                s.spatialBlend = info.spatial;
            }
            else
            {
                s.transform.localPosition = Vector3.zero;
                s.spatialBlend = 0f;
            }
            s.Play();
            poolIds[slot] = id;
            poolStart[slot] = now;
            return s;
        }

        // ------------------------------------------------------------------ Muzyka i tło

        /// <summary>Warstwa z dwoma źródłami do płynnego przenikania pętli.</summary>
        sealed class Layer
        {
            readonly AudioSource[] src = new AudioSource[2];
            readonly float[] level = new float[2];
            readonly float[] target = new float[2];
            int active;
            public MusicId Current { get; private set; } = MusicId.None;
            public float FadeSeconds = 2.5f;

            public Layer(GameObject host, string name)
            {
                for (int i = 0; i < 2; i++)
                {
                    var go = new GameObject(name + i);
                    go.transform.SetParent(host.transform, false);
                    var s = go.AddComponent<AudioSource>();
                    s.playOnAwake = false; s.loop = true; s.spatialBlend = 0f; s.ignoreListenerPause = true; s.reverbZoneMix = 0f; s.priority = 0;
                    src[i] = s;
                }
            }

            public AudioSource ActiveSource => src[active];

            public void Want(MusicId id)
            {
                if (id == Current) return;
                if (id == MusicId.None)
                {
                    Current = id;
                    target[0] = target[1] = 0f;
                    return;
                }
                var clip = Music(id);
                if (clip == null) return; // jeszcze się syntezuje – spróbujemy w kolejnej klatce
                Current = id;
                target[active] = 0f;
                active = 1 - active;
                var s = src[active];
                s.clip = clip;
                s.time = 0f;
                level[active] = 0f; target[active] = 1f;
                s.Play();
            }

            public void Tick(float dt, float volume)
            {
                for (int i = 0; i < 2; i++)
                {
                    level[i] = Mathf.MoveTowards(level[i], target[i], dt / Mathf.Max(0.05f, FadeSeconds));
                    var s = src[i];
                    s.volume = level[i] * volume;
                    if (level[i] <= 0f && s.isPlaying) s.Stop();
                }
            }
        }

        Layer music, ambience;
        AudioReverbZone zone;
        MusicId arenaAmbience = MusicId.AmbSummit;
        GameRoot root;
        float musicGain = 1f, ambienceGain = 1f;
        float heartbeatTimer;

        public MusicId CurrentMusic => music?.Current ?? MusicId.None;
        public MusicId CurrentAmbience => ambience?.Current ?? MusicId.None;

        public void Bind(GameRoot gameRoot) => root = gameRoot;

        /// <summary>Tło i pogłos areny.</summary>
        public void SetArena(ArenaStyle style)
        {
            switch (style)
            {
                case ArenaStyle.Crypt:
                    arenaAmbience = MusicId.AmbCrypt;
                    zone.reverbPreset = AudioReverbPreset.Cave;
                    break;
                case ArenaStyle.Summit:
                    arenaAmbience = MusicId.AmbSummit;
                    zone.reverbPreset = AudioReverbPreset.Mountains;
                    break;
                default:
                    arenaAmbience = MusicId.AmbCourtyard;
                    zone.reverbPreset = AudioReverbPreset.StoneCorridor;
                    break;
            }
        }

        void Update()
        {
            DrainReady(24);
            float dt = Time.unscaledDeltaTime;
            ChooseMusic(out var wantMusic, out float wantMusicGain, out float wantAmbGain);
            music.Want(wantMusic);
            ambience.Want(arenaAmbience);
            musicGain = Mathf.MoveTowards(musicGain, wantMusicGain, dt * 0.8f);
            ambienceGain = Mathf.MoveTowards(ambienceGain, wantAmbGain, dt * 0.8f);
            music.Tick(dt, MusicVolume * musicGain);
            ambience.Tick(dt, MusicVolume * ambienceGain);

            // Pauza (czas stoi) wstrzymuje efekty gry; muzyka i interfejs grają dalej.
            AudioListener.pause = Time.timeScale == 0f;
            TickHeartbeat();
        }

        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam != null) zone.transform.position = cam.transform.position;
        }

        void ChooseMusic(out MusicId id, out float gain, out float ambGain)
        {
            id = MusicId.Menu; gain = 1f; ambGain = 0.6f;
            if (root == null) return;
            var screen = root.Screen;
            bool inRun = root.Run != null && !root.Run.IsFinished;
            if (screen == GameScreen.Equipment) screen = root.ScreenBeforeEquipment == GameScreen.Equipment ? GameScreen.Intermission : root.ScreenBeforeEquipment;
            if (screen == GameScreen.Controls && inRun) screen = GameScreen.Paused;
            switch (screen)
            {
                case GameScreen.Playing:
                case GameScreen.Paused:
                {
                    var floor = root.CurrentFloor;
                    bool boss = floor != null && floor.isBoss;
                    bool fighting = false;
                    foreach (var e in root.Enemies) if (e != null && !e.IsDead) { fighting = true; break; }
                    id = boss ? MusicId.Boss : fighting ? MusicId.Combat : MusicId.None;
                    gain = screen == GameScreen.Paused ? 0.5f : 1f;
                    ambGain = 1f;
                    break;
                }
                case GameScreen.Intermission:
                    id = MusicId.Menu; gain = 0.6f; ambGain = 0.8f;
                    break;
                case GameScreen.Death:
                    id = MusicId.None; ambGain = 0.5f;
                    break;
            }
        }

        void TickHeartbeat()
        {
            var p = root != null ? root.Player : null;
            if (p == null || root.Screen != GameScreen.Playing || p.IsDead || !p.isActiveAndEnabled || p.Health == null || p.Health.Fraction > 0.3f)
            {
                heartbeatTimer = 0f;
                return;
            }
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer > 0f) return;
            float severity = 1f - p.Health.Fraction / 0.3f;
            heartbeatTimer = Mathf.Lerp(1.05f, 0.62f, severity);
            PlayAt(SoundId.Heartbeat, null, Mathf.Lerp(0.5f, 1f, severity));
        }
    }
}
