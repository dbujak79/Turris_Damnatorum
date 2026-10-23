namespace Turris
{
    /// <summary>
    /// Wszystkie efekty dźwiękowe gry. Nazwa wartości = nazwa pliku zastępczego w Resources/Audio/
    /// (np. Resources/Audio/Parry.wav albo Parry_1.wav, Parry_2.wav jako warianty) – plik ma pierwszeństwo przed syntezą.
    /// Nowe wartości dopisywać na końcu.
    /// </summary>
    public enum SoundId
    {
        // Interfejs
        UiMove, UiConfirm, UiBack, UiBuy, UiReward, UiLevelUp,
        // Broń i ruch
        SwingLight, SwingHeavy, BowRelease, ArrowHit, Footstep, Dodge,
        // Trafienia i obrona
        HitFlesh, HitFleshHeavy, HitPlayer, Block, Parry, GuardBreak, Whiff, Riposte,
        // Przedmioty
        Flask,
        // Magia i techniki
        CastCharge, ArcaneBolt, ArcaneImpact, FireCast, FireExplosion, FrostCast, FrostImpact, Freeze, Shatter,
        LightningZap, Thunder, Heal, Enchant, Barrier, Shockwave, Quake, Warcry, BloodMagic, Chains,
        // Efekty trwające
        BurnTick, BleedTick,
        // Wrogowie
        TelegraphHeavy, TelegraphUnblockable, GhoulGrowl, BossRoar, DeathGhoul, DeathBones, DeathArmored, BossDeath,
        // Przebieg gry
        FloorStart, BossIntro, WaveHorn, FloorCleared, Victory, PlayerDeath, SoulGain, Heartbeat,
        // Warstwy trafień (odczucie ciosu)
        HitBlunt, HitArmor, KillBlow, BodyFall,
    }

    /// <summary>Utwory muzyczne i tła aren (pętle). Nazwa = plik zastępczy w Resources/Audio/Music/.</summary>
    public enum MusicId { None, Menu, Combat, Boss, AmbCourtyard, AmbCrypt, AmbSummit }

    /// <summary>Kanał głośności.</summary>
    public enum SoundBus { Sfx, Ui, Stinger }

    /// <summary>Parametry odtwarzania jednego efektu.</summary>
    public struct SoundInfo
    {
        public float volume;       // głośność względna
        public int variants;       // ile wariantów syntezować (losowane przy odtwarzaniu)
        public float pitchJitter;  // losowe odchylenie wysokości (±)
        public float spatial;      // 0 = 2D, 1 = w pełni przestrzenny
        public int maxVoices;      // limit jednoczesnych odtworzeń
        public float minInterval;  // minimalny odstęp między odtworzeniami (s)
        public SoundBus bus;

        public static SoundInfo For(SoundId id)
        {
            var s = new SoundInfo { volume = 0.8f, variants = 3, pitchJitter = 0.05f, spatial = 0.75f, maxVoices = 4, minInterval = 0.03f, bus = SoundBus.Sfx };
            switch (id)
            {
                case SoundId.UiMove: s.volume = 0.22f; s.variants = 2; s.spatial = 0; s.maxVoices = 2; s.bus = SoundBus.Ui; s.pitchJitter = 0.03f; break;
                case SoundId.UiConfirm: s.volume = 0.55f; s.variants = 2; s.spatial = 0; s.bus = SoundBus.Ui; s.pitchJitter = 0.02f; break;
                case SoundId.UiBack: s.volume = 0.5f; s.variants = 1; s.spatial = 0; s.bus = SoundBus.Ui; s.pitchJitter = 0.02f; break;
                case SoundId.UiBuy: s.volume = 0.6f; s.variants = 2; s.spatial = 0; s.bus = SoundBus.Ui; break;
                case SoundId.UiReward: s.volume = 0.6f; s.variants = 1; s.spatial = 0; s.bus = SoundBus.Ui; s.pitchJitter = 0; break;
                case SoundId.UiLevelUp: s.volume = 0.6f; s.variants = 1; s.spatial = 0; s.bus = SoundBus.Ui; s.pitchJitter = 0; break;

                case SoundId.SwingLight: s.volume = 0.7f; s.variants = 4; s.pitchJitter = 0.08f; break;
                case SoundId.SwingHeavy: s.volume = 0.8f; s.variants = 3; s.pitchJitter = 0.06f; break;
                case SoundId.BowRelease: s.volume = 0.8f; s.pitchJitter = 0.05f; break;
                case SoundId.ArrowHit: s.volume = 0.9f; break;
                case SoundId.Footstep: s.volume = 0.28f; s.variants = 6; s.pitchJitter = 0.08f; s.maxVoices = 3; s.minInterval = 0.12f; break;
                case SoundId.Dodge: s.volume = 0.55f; s.spatial = 0.4f; break;

                case SoundId.HitFlesh: s.volume = 0.7f; s.variants = 4; s.maxVoices = 5; break;
                case SoundId.HitFleshHeavy: s.volume = 1.0f; s.maxVoices = 3; break;
                case SoundId.HitPlayer: s.volume = 0.9f; s.spatial = 0.3f; s.maxVoices = 2; break;
                case SoundId.Block: s.volume = 0.8f; s.pitchJitter = 0.06f; break;
                case SoundId.Parry: s.volume = 1.0f; s.variants = 2; s.pitchJitter = 0.03f; s.maxVoices = 2; s.spatial = 0.4f; break;
                case SoundId.GuardBreak: s.volume = 0.7f; s.variants = 2; s.maxVoices = 2; s.spatial = 0.4f; break;
                case SoundId.Whiff: s.volume = 0.5f; break;
                case SoundId.Riposte: s.volume = 1f; s.variants = 2; s.maxVoices = 1; s.spatial = 0.4f; break;

                case SoundId.Flask: s.volume = 0.6f; s.variants = 2; s.spatial = 0.3f; s.maxVoices = 1; break;

                case SoundId.CastCharge: s.volume = 0.45f; s.variants = 2; s.spatial = 0.4f; s.maxVoices = 2; break;
                case SoundId.ArcaneBolt: s.volume = 0.8f; break;
                case SoundId.ArcaneImpact: s.volume = 0.8f; break;
                case SoundId.FireCast: s.volume = 0.7f; break;
                case SoundId.FireExplosion: s.volume = 0.95f; s.variants = 2; s.maxVoices = 3; s.minInterval = 0.06f; break;
                case SoundId.FrostCast: s.volume = 0.65f; break;
                case SoundId.FrostImpact: s.volume = 0.65f; break;
                case SoundId.Freeze: s.volume = 0.8f; s.variants = 2; s.maxVoices = 2; break;
                case SoundId.Shatter: s.volume = 0.9f; s.variants = 2; s.maxVoices = 2; break;
                case SoundId.LightningZap: s.volume = 0.7f; s.variants = 4; s.maxVoices = 4; break;
                case SoundId.Thunder: s.volume = 0.9f; s.variants = 2; s.maxVoices = 2; s.minInterval = 0.2f; s.spatial = 0.3f; break;
                case SoundId.Heal: s.volume = 0.6f; s.variants = 1; s.spatial = 0.2f; s.pitchJitter = 0; break;
                case SoundId.Enchant: s.volume = 0.65f; s.variants = 2; s.spatial = 0.3f; break;
                case SoundId.Barrier: s.volume = 0.6f; s.variants = 1; s.spatial = 0.3f; break;
                case SoundId.Shockwave: s.volume = 0.85f; s.variants = 2; s.maxVoices = 3; break;
                case SoundId.Quake: s.volume = 1f; s.variants = 2; s.maxVoices = 2; break;
                case SoundId.Warcry: s.volume = 0.5f; s.variants = 1; s.spatial = 0.3f; s.pitchJitter = 0.02f; break;
                case SoundId.BloodMagic: s.volume = 0.7f; s.variants = 2; s.spatial = 0.3f; break;
                case SoundId.Chains: s.volume = 0.7f; s.variants = 2; break;

                case SoundId.BurnTick: s.volume = 0.3f; s.maxVoices = 3; s.minInterval = 0.1f; break;
                case SoundId.BleedTick: s.volume = 0.4f; s.maxVoices = 3; s.minInterval = 0.1f; break;

                case SoundId.TelegraphHeavy: s.volume = 0.65f; s.variants = 2; s.maxVoices = 2; s.minInterval = 0.15f; s.spatial = 0.6f; break;
                case SoundId.TelegraphUnblockable: s.volume = 0.8f; s.variants = 1; s.maxVoices = 2; s.minInterval = 0.2f; s.spatial = 0.5f; s.pitchJitter = 0.01f; break;
                case SoundId.GhoulGrowl: s.volume = 0.3f; s.maxVoices = 2; s.minInterval = 0.4f; break;
                case SoundId.BossRoar: s.volume = 1f; s.variants = 1; s.maxVoices = 1; s.spatial = 0.3f; s.pitchJitter = 0; break;
                case SoundId.DeathGhoul: s.volume = 0.55f; s.variants = 2; s.maxVoices = 3; break;
                case SoundId.DeathBones: s.volume = 0.9f; s.variants = 2; s.maxVoices = 3; break;
                case SoundId.DeathArmored: s.volume = 0.6f; s.variants = 2; s.maxVoices = 3; break;
                case SoundId.BossDeath: s.volume = 1f; s.variants = 1; s.maxVoices = 1; s.spatial = 0.2f; s.pitchJitter = 0; break;

                case SoundId.FloorStart: s.volume = 0.8f; s.variants = 1; s.spatial = 0; s.maxVoices = 1; s.pitchJitter = 0; s.bus = SoundBus.Stinger; break;
                case SoundId.BossIntro: s.volume = 0.9f; s.variants = 1; s.spatial = 0; s.maxVoices = 1; s.pitchJitter = 0; s.bus = SoundBus.Stinger; break;
                case SoundId.WaveHorn: s.volume = 0.5f; s.variants = 1; s.spatial = 0; s.maxVoices = 1; s.pitchJitter = 0; s.bus = SoundBus.Stinger; break;
                case SoundId.FloorCleared: s.volume = 0.7f; s.variants = 1; s.spatial = 0; s.maxVoices = 1; s.pitchJitter = 0; s.bus = SoundBus.Stinger; break;
                case SoundId.Victory: s.volume = 0.85f; s.variants = 1; s.spatial = 0; s.maxVoices = 1; s.pitchJitter = 0; s.bus = SoundBus.Stinger; break;
                case SoundId.PlayerDeath: s.volume = 0.9f; s.variants = 1; s.spatial = 0; s.maxVoices = 1; s.pitchJitter = 0; s.bus = SoundBus.Stinger; break;
                case SoundId.SoulGain: s.volume = 0.35f; s.variants = 2; s.spatial = 0; s.maxVoices = 2; s.minInterval = 0.1f; break;
                case SoundId.Heartbeat: s.volume = 0.7f; s.variants = 1; s.spatial = 0; s.maxVoices = 1; s.pitchJitter = 0; break;

                case SoundId.HitBlunt: s.volume = 0.75f; s.variants = 3; s.maxVoices = 3; s.pitchJitter = 0.06f; break;
                case SoundId.HitArmor: s.volume = 0.6f; s.variants = 4; s.maxVoices = 3; s.pitchJitter = 0.07f; break;
                case SoundId.KillBlow: s.volume = 0.8f; s.variants = 2; s.maxVoices = 2; s.minInterval = 0.08f; s.spatial = 0.5f; break;
                case SoundId.BodyFall: s.volume = 0.7f; s.variants = 3; s.maxVoices = 3; s.minInterval = 0.05f; break;
            }
            return s;
        }
    }
}
