using System;

namespace Turris
{
    /// <summary>
    /// Przepisy efektów dźwiękowych (klimat dark fantasy): stal, kość, kamień, żar, dzwony i chór.
    /// Każdy efekt powstaje w kodzie z szumu, rezonatorów i oscylatorów – deterministycznie dla (id, wariant).
    /// Czysty C#: działa w wątku tła i w testach EditMode.
    /// </summary>
    public static class SoundSynth
    {
        public const int SampleRate = 44100;

        public static float[] Generate(SoundId id, int variant, int sr = SampleRate)
        {
            var d = Build(id, variant, sr);
            d.FadeOut(0.01f);
            d.Normalize(0.89f);
            return d.buf;
        }

        static Dsp New(float seconds, int sr, SoundId id, int variant) => new Dsp(seconds, sr, (int)id * 7919 + variant * 104729 + 17);

        static Dsp Build(SoundId id, int v, int sr)
        {
            switch (id)
            {
                case SoundId.UiMove: return UiMove(New(0.09f, sr, id, v));
                case SoundId.UiConfirm: return UiConfirm(New(0.35f, sr, id, v));
                case SoundId.UiBack: return UiBack(New(0.3f, sr, id, v));
                case SoundId.UiBuy: return UiBuy(New(0.9f, sr, id, v));
                case SoundId.UiReward: return UiReward(New(2.2f, sr, id, v));
                case SoundId.UiLevelUp: return UiLevelUp(New(2.2f, sr, id, v));
                case SoundId.SwingLight: return Swing(New(0.32f, sr, id, v), false);
                case SoundId.SwingHeavy: return Swing(New(0.55f, sr, id, v), true);
                case SoundId.BowRelease: return BowRelease(New(0.5f, sr, id, v));
                case SoundId.ArrowHit: return ArrowHit(New(0.3f, sr, id, v));
                case SoundId.Footstep: return Footstep(New(0.2f, sr, id, v));
                case SoundId.Dodge: return Dodge(New(0.5f, sr, id, v));
                case SoundId.HitFlesh: return HitFlesh(New(0.35f, sr, id, v), false);
                case SoundId.HitFleshHeavy: return HitFlesh(New(0.6f, sr, id, v), true);
                case SoundId.HitPlayer: return HitPlayer(New(0.45f, sr, id, v));
                case SoundId.Block: return Block(New(0.8f, sr, id, v));
                case SoundId.Parry: return Parry(New(1.8f, sr, id, v));
                case SoundId.GuardBreak: return GuardBreak(New(1.2f, sr, id, v));
                case SoundId.Whiff: return Whiff(New(0.25f, sr, id, v));
                case SoundId.Riposte: return Riposte(New(1.3f, sr, id, v));
                case SoundId.Flask: return Flask(New(1.2f, sr, id, v));
                case SoundId.CastCharge: return CastCharge(New(0.8f, sr, id, v));
                case SoundId.ArcaneBolt: return ArcaneBolt(New(0.45f, sr, id, v));
                case SoundId.ArcaneImpact: return ArcaneImpact(New(0.7f, sr, id, v));
                case SoundId.FireCast: return FireCast(New(0.8f, sr, id, v));
                case SoundId.FireExplosion: return FireExplosion(New(1.8f, sr, id, v));
                case SoundId.FrostCast: return FrostCast(New(0.9f, sr, id, v));
                case SoundId.FrostImpact: return FrostImpact(New(0.8f, sr, id, v));
                case SoundId.Freeze: return Freeze(New(0.9f, sr, id, v));
                case SoundId.Shatter: return Shatter(New(1.2f, sr, id, v));
                case SoundId.LightningZap: return Zap(New(0.5f, sr, id, v));
                case SoundId.Thunder: return Thunder(New(3.2f, sr, id, v));
                case SoundId.Heal: return Heal(New(2.2f, sr, id, v));
                case SoundId.Enchant: return Enchant(New(1.0f, sr, id, v));
                case SoundId.Barrier: return Barrier(New(1.3f, sr, id, v));
                case SoundId.Shockwave: return Shockwave(New(1.1f, sr, id, v));
                case SoundId.Quake: return Quake(New(1.6f, sr, id, v));
                case SoundId.Warcry: return Warcry(New(1.5f, sr, id, v));
                case SoundId.BloodMagic: return BloodMagic(New(1.1f, sr, id, v));
                case SoundId.Chains: return Chains(New(0.9f, sr, id, v));
                case SoundId.BurnTick: return BurnTick(New(0.3f, sr, id, v));
                case SoundId.BleedTick: return BleedTick(New(0.25f, sr, id, v));
                case SoundId.TelegraphHeavy: return TelegraphHeavy(New(1.0f, sr, id, v));
                case SoundId.TelegraphUnblockable: return TelegraphUnblockable(New(1.3f, sr, id, v));
                case SoundId.GhoulGrowl: return Growl(New(0.9f, sr, id, v), 1f);
                case SoundId.BossRoar: return BossRoar(New(2.6f, sr, id, v));
                case SoundId.DeathGhoul: return DeathGhoul(New(1.4f, sr, id, v));
                case SoundId.DeathBones: return DeathBones(New(1.3f, sr, id, v));
                case SoundId.DeathArmored: return DeathArmored(New(1.4f, sr, id, v));
                case SoundId.BossDeath: return BossDeath(New(5f, sr, id, v));
                case SoundId.FloorStart: return FloorStart(New(6.5f, sr, id, v));
                case SoundId.BossIntro: return BossIntro(New(7f, sr, id, v));
                case SoundId.WaveHorn: return WaveHorn(New(3.5f, sr, id, v));
                case SoundId.FloorCleared: return FloorCleared(New(4.5f, sr, id, v));
                case SoundId.Victory: return Victory(New(7f, sr, id, v));
                case SoundId.PlayerDeath: return PlayerDeath(New(6f, sr, id, v));
                case SoundId.SoulGain: return SoulGain(New(1.0f, sr, id, v));
                case SoundId.Heartbeat: return Heartbeat(New(0.7f, sr, id, v));
                case SoundId.HitBlunt: return HitBlunt(New(0.5f, sr, id, v));
                case SoundId.HitArmor: return HitArmor(New(0.6f, sr, id, v));
                case SoundId.KillBlow: return KillBlow(New(1.1f, sr, id, v));
                case SoundId.BodyFall: return BodyFall(New(0.6f, sr, id, v));
            }
            return New(0.1f, sr, id, v);
        }

        static float Exp(float t, float tau) => (float)Math.Exp(-t / tau);
        static float Sin(float x) => (float)Math.Sin(x);

        // ================================================================== Interfejs

        static Dsp UiMove(Dsp d)
        {
            // Stuknięcie w kamień – krótkie i ciche.
            d.AddClick(0, 0.4f, 2f, 2500f);
            d.AddModes(0, d.Range(780, 820), new[] { 1f, 2.3f }, new[] { 1f, 0.3f }, new[] { 0.025f, 0.012f }, 0.6f);
            return d;
        }

        static Dsp UiConfirm(Dsp d)
        {
            // Głuchy „klunk” żelaznego rygla.
            d.AddClick(0, 0.6f, 3f, 4000f);
            d.AddModes(0, d.Range(300, 320), Dsp.PlateRatios, new[] { 1f, 0.6f, 0.5f, 0.3f, 0.3f, 0.2f, 0.15f, 0.1f }, new[] { 0.12f, 0.08f, 0.06f, 0.05f, 0.04f, 0.03f, 0.03f, 0.02f }, 0.5f);
            d.AddThump(0, 0.5f, 160f, 90f, 0.05f);
            return d;
        }

        static Dsp UiBack(Dsp d)
        {
            d.AddClick(0, 0.4f, 3f, 2000f);
            d.AddModes(0, 190f, Dsp.PlateRatios, new[] { 1f, 0.5f, 0.4f, 0.2f }, new[] { 0.1f, 0.06f, 0.04f, 0.03f }, 0.5f);
            d.AddThump(0, 0.5f, 120f, 60f, 0.06f);
            return d;
        }

        static Dsp UiBuy(Dsp d)
        {
            // Dusze spływają do naczynia: trzy chłodne dzwoneczki i szept.
            float[] notes = { 1174.7f, 1396.9f, 1760f };
            for (int i = 0; i < 3; i++)
                d.AddModes(i * 0.07f, notes[i] * d.Range(0.99f, 1.01f), new[] { 1f, 2.76f, 5.4f }, new[] { 1f, 0.3f, 0.1f }, new[] { 0.35f, 0.12f, 0.05f }, 0.35f);
            d.AddNoise(0, 0.7f, 0.12f, t => Dsp.Window(t, 0.7f, 0.2f, 0.4f), t => Dsp.ExpLerp(800, 3000, t / 0.7f), 3f, Dsp.Mode.Band);
            d.Reverb(0.6f, 0.4f, 0.25f);
            return d;
        }

        static Dsp UiReward(Dsp d)
        {
            // Mały dzwon kapliczki i ciche „aa”.
            d.AddBell(0, 587.3f, 0.6f, 1.6f);
            d.AddChoir(0.02f, 1.8f, 0.25f, 293.7f, 0.3f, 1.0f);
            d.AddChoir(0.02f, 1.8f, 0.18f, 440f, 0.3f, 1.0f);
            d.Reverb(0.8f, 0.4f, 0.35f);
            return d;
        }

        static Dsp UiLevelUp(Dsp d)
        {
            d.AddBell(0, 440f, 0.5f, 1.4f);
            d.AddBell(0.18f, 659.3f, 0.5f, 1.6f);
            d.AddNoise(0, 1.2f, 0.06f, t => Dsp.Window(t, 1.2f, 0.3f, 0.8f), _ => 6000f, 1f, Dsp.Mode.High);
            d.Reverb(0.8f, 0.4f, 0.35f);
            return d;
        }

        // ================================================================== Broń i ruch

        static Dsp Swing(Dsp d, bool heavy)
        {
            // Świst: szum pasmowy, którego pasmo rośnie i opada razem z prędkością ostrza.
            float dur = d.Duration * d.Range(0.85f, 1f);
            float peak = dur * (heavy ? 0.5f : 0.4f);
            float lo = heavy ? d.Range(220, 280) : d.Range(500, 650);
            float hi = heavy ? d.Range(900, 1200) : d.Range(2000, 2800);
            Func<float, float> env = t => t < peak ? (float)Math.Pow(t / peak, 2) : Exp(t - peak, dur * 0.18f);
            Func<float, float> cut = t => t < peak ? Dsp.ExpLerp(lo, hi, t / peak) : Dsp.ExpLerp(hi, lo * 0.8f, (t - peak) / (dur - peak));
            d.AddNoise(0, dur, 1f, env, cut, heavy ? 2.2f : 2.8f, Dsp.Mode.Band);
            d.AddNoise(0, dur, 0.35f, env, t => cut(t) * 0.5f, 0.7f, Dsp.Mode.Low);
            if (heavy) d.AddTone(0, dur, 0.15f, t => 55f + 25f * env(t), env, Dsp.Wave.Sine);
            return d;
        }

        static Dsp BowRelease(Dsp d)
        {
            // Brzdęk cięciwy + świst strzały.
            float f = d.Range(95, 120);
            d.AddClick(0, 0.8f, 2f, 5000f);
            d.AddModes(0, f, new[] { 1f, 2f, 3.01f, 4.03f, 5.1f }, new[] { 1f, 0.6f, 0.35f, 0.2f, 0.1f }, new[] { 0.09f, 0.07f, 0.05f, 0.04f, 0.03f }, 0.8f);
            d.AddThump(0, 0.35f, 180f, 70f, 0.04f);
            d.AddNoise(0.01f, 0.3f, 0.35f, t => Dsp.AD(t, 0.02f, 0.08f), t => Dsp.ExpLerp(3000, 1200, t / 0.3f), 2f, Dsp.Mode.Band);
            return d;
        }

        static Dsp ArrowHit(Dsp d)
        {
            // Grot wbija się w drewno/ciało: stuk i drgające drzewce.
            d.AddClick(0, 0.9f, 2f, 5000f);
            d.AddThump(0, 0.8f, 200f, 80f, 0.04f);
            d.AddModes(0, d.Range(210, 240), new[] { 1f, 2.52f, 3.9f }, new[] { 1f, 0.5f, 0.25f }, new[] { 0.05f, 0.03f, 0.02f }, 0.5f);
            d.AddTone(0.005f, 0.2f, 0.2f, t => 38f + 4f * Sin(t * 60f), t => Exp(t, 0.06f), Dsp.Wave.Triangle, 400f);
            return d;
        }

        static Dsp Footstep(Dsp d)
        {
            // Skórzany but na kamieniu: tąpnięcie + chrzęst żwiru.
            d.AddThump(0, 0.6f, d.Range(110, 140), 60f, 0.03f);
            float c = d.Range(1500, 2500);
            d.AddNoise(0, 0.12f, 0.6f, t => Dsp.AD(t, 0.003f, 0.03f), _ => c, 1.2f, Dsp.Mode.Band);
            d.AddCrackle(0.005f, 0.08f, 120f, 0.25f, 2500, 5000, t => Exp(t, 0.03f));
            return d;
        }

        static Dsp Dodge(Dsp d)
        {
            // Szelest płaszcza i świst, na końcu przyklęk na kamieniu.
            float dur = 0.34f;
            Func<float, float> env = t => Dsp.Window(t, dur, 0.1f, 0.2f);
            d.AddNoise(0, dur, 0.8f, t => env(t) * (0.6f + 0.4f * Math.Abs(Sin(t * 90f + Sin(t * 37f) * 3f))), t => Dsp.ExpLerp(500, 1600, Math.Min(1f, t / 0.15f)), 1.5f, Dsp.Mode.Band);
            d.AddNoise(0, dur, 0.3f, env, _ => 350f, 0.7f, Dsp.Mode.Low);
            d.AddThump(0.3f, 0.35f, 120f, 60f, 0.04f);
            d.AddCrackle(0.3f, 0.08f, 100f, 0.2f, 2000, 4500, t => Exp(t, 0.03f));
            return d;
        }

        // ================================================================== Trafienia

        static Dsp HitFlesh(Dsp d, bool heavy)
        {
            // Cios w ciało: głuche uderzenie, mokre mlaśnięcie, (ciężki) chrupnięcie kości.
            float k = heavy ? 1.6f : 1f;
            d.AddClick(0, 0.8f, 2f, 7000f);
            d.AddThump(0, heavy ? 0.6f : 1f, d.Range(120, 150) / k, 45f / k, 0.07f * k);
            d.AddNoise(0, 0.12f * k, 0.7f, t => Dsp.AD(t, 0.002f, 0.04f * k), _ => 1400f, 0.7f, Dsp.Mode.Low);
            // Mlaśnięcie: pasmo 500–900 Hz z szybką, nieregularną modulacją.
            float wet = d.Range(550, 800);
            d.AddNoise(0.01f, 0.18f * k, 0.55f, t => Exp(t, 0.05f * k) * (0.5f + 0.5f * Math.Abs(Sin(t * 260f + Sin(t * 90f) * 4f))), t => wet * (1f - t * 1.5f), 4f, Dsp.Mode.Band);
            d.AddNoise(0, 0.04f, 0.3f, t => Exp(t, 0.01f), _ => 4000f, 0.7f, Dsp.Mode.High);
            if (heavy)
            {
                d.AddCrackle(0.01f, 0.05f, 180f, 0.6f, 1200, 2600);
                d.AddThump(0.005f, 0.3f, 70f, 35f, 0.15f);
            }
            d.Saturate(heavy ? 3f : 1.6f); // ciężki cios mocniej ściśnięty – głośniejszy od lekkiego przy tym samym szczycie
            return d;
        }

        static Dsp HitBlunt(Dsp d)
        {
            // Obuch: głęboki łomot i miażdżenie – chrupnięcie kości, bez mlaśnięcia ostrza.
            d.AddClick(0, 0.9f, 3f, 3500f);
            d.AddThump(0, 1f, d.Range(95, 115), 38f, 0.12f, 0.04f);
            d.AddThump(0.004f, 0.5f, 220f, 90f, 0.04f);
            d.AddCrackle(0.004f, 0.07f, 260f, 0.7f, 900, 2400, t => Exp(t, 0.025f));
            d.AddNoise(0, 0.2f, 0.6f, t => Dsp.AD(t, 0.002f, 0.06f), t => Dsp.ExpLerp(1800, 300, t / 0.2f), 0.9f, Dsp.Mode.Low);
            d.Saturate(2.6f);
            return d;
        }

        static Dsp HitArmor(Dsp d)
        {
            // Cios w pancerz: krótki, stłumiony brzęk płyty i szczęk kolczugi (bez długiego dzwonienia).
            d.AddClick(0, 1f, 2f, 9000f);
            float f = d.Range(420, 640);
            d.AddModes(0, f, Dsp.PlateRatios, new[] { 1f, 0.8f, 0.6f, 0.5f, 0.4f, 0.3f }, new[] { 0.12f, 0.09f, 0.07f, 0.05f, 0.04f, 0.03f }, 0.55f, 0.012f);
            d.AddCrackle(0.002f, 0.1f, 300f, 0.35f, 3500, 8000, t => Exp(t, 0.03f));
            d.AddNoise(0, 0.06f, 0.35f, t => Exp(t, 0.015f), _ => 5000f, 0.7f, Dsp.Mode.High);
            d.Saturate(1.4f);
            return d;
        }

        static Dsp KillBlow(Dsp d)
        {
            // Cios kończący: ciężkie „tąpnięcie” w dole pasma, świst uchodzącego powietrza i krótki pogłos.
            d.AddThump(0, 1f, 75f, 28f, 0.3f, 0.08f);
            d.AddThump(0, 0.6f, 160f, 60f, 0.07f);
            d.AddClick(0, 0.7f, 4f, 3000f);
            d.AddNoise(0.02f, 0.5f, 0.5f, t => Exp(t, 0.14f), t => Dsp.ExpLerp(2200, 250, t / 0.5f), 0.8f, Dsp.Mode.Low);
            d.AddNoise(0, 0.35f, 0.25f, t => Dsp.Window(t, 0.35f, 0.01f, 0.3f), t => Dsp.ExpLerp(3000, 900, t / 0.35f), 2.5f, Dsp.Mode.Band);
            d.Saturate(2.2f);
            d.Reverb(0.6f, 0.5f, 0.2f);
            return d;
        }

        static Dsp BodyFall(Dsp d)
        {
            // Ciało uderza o kamień: głuchy łomot, drugie mniejsze odbicie i szelest żwiru.
            d.AddThump(0, 1f, d.Range(90, 110), 42f, 0.1f, 0.04f);
            d.AddNoise(0, 0.15f, 0.7f, t => Dsp.AD(t, 0.003f, 0.05f), _ => 600f, 0.7f, Dsp.Mode.Low);
            float t1 = d.Range(0.11f, 0.16f);
            d.AddThump(t1, 0.45f, 120f, 55f, 0.06f);
            d.AddNoise(t1, 0.08f, 0.3f, t => Exp(t, 0.03f), _ => 700f, 0.7f, Dsp.Mode.Low);
            d.AddCrackle(0.01f, 0.35f, 90f, 0.18f, 1800, 5000, t => Exp(t, 0.1f));
            d.Saturate(1.6f);
            return d;
        }

        static Dsp HitPlayer(Dsp d)
        {
            // Bohater dostaje: tąpnięcie przez pancerz + brzęk kolczugi.
            d.AddClick(0, 0.7f, 2f, 5000f);
            d.AddThump(0, 1f, 110f, 42f, 0.1f);
            d.AddNoise(0, 0.15f, 0.6f, t => Dsp.AD(t, 0.002f, 0.05f), _ => 900f, 0.7f, Dsp.Mode.Low);
            d.AddCrackle(0.003f, 0.12f, 160f, 0.25f, 3000, 7000, t => Exp(t, 0.04f));
            d.AddModes(0, d.Range(850, 950), new[] { 1f, 1.5f, 2.35f }, new[] { 0.5f, 0.3f, 0.2f }, new[] { 0.06f, 0.04f, 0.03f }, 0.25f);
            d.Saturate(1.8f);
            return d;
        }

        static Dsp Block(Dsp d)
        {
            // Stal o tarczę: przytłumiony brzęk płyty i drewno.
            d.AddClick(0, 1f, 3f, 8000f);
            float f = d.Range(300, 380);
            d.AddModes(0, f, Dsp.PlateRatios, new[] { 1f, 0.8f, 0.7f, 0.6f, 0.5f, 0.35f, 0.3f, 0.2f }, new[] { 0.3f, 0.22f, 0.16f, 0.14f, 0.1f, 0.08f, 0.06f, 0.05f }, 0.5f, 0.01f);
            d.AddThump(0, 0.8f, 150f, 70f, 0.06f);
            d.AddNoise(0, 0.1f, 0.4f, t => Exp(t, 0.02f), _ => 3000f, 0.7f, Dsp.Mode.High);
            d.Saturate(1.3f);
            return d;
        }

        static Dsp Parry(Dsp d)
        {
            // Parowanie: jasne, długo dzwoniące ostrze (sztaba) + dudniący podwójny ton i iskry.
            float f = d.Range(1050, 1200);
            d.AddClick(0, 1f, 2f, 10000f);
            var amps = new[] { 1f, 0.7f, 0.45f, 0.3f, 0.2f };
            var dec = new[] { 1.3f, 0.8f, 0.5f, 0.3f, 0.2f };
            d.AddModes(0, f, Dsp.BarRatios, amps, dec, 0.45f);
            d.AddModes(0, f * 1.004f, Dsp.BarRatios, amps, dec, 0.35f); // dudnienie
            d.AddModes(0, f * 0.5f, Dsp.PlateRatios, new[] { 0.6f, 0.4f, 0.3f }, new[] { 0.2f, 0.12f, 0.08f }, 0.4f);
            d.AddNoise(0, 0.4f, 0.3f, t => Exp(t, 0.08f), _ => 7000f, 0.7f, Dsp.Mode.High);
            d.AddThump(0, 0.5f, 140f, 70f, 0.06f);
            d.Reverb(0.7f, 0.3f, 0.25f);
            return d;
        }

        static Dsp GuardBreak(Dsp d)
        {
            // Przełamanie gardy: ciężki brzęk, trzask pękającej obrony i głuchy wstrząs.
            d.AddClick(0, 1f, 4f, 8000f);
            d.AddModes(0, d.Range(200, 240), Dsp.PlateRatios, new[] { 1f, 0.9f, 0.8f, 0.7f, 0.6f, 0.4f, 0.3f, 0.2f }, new[] { 0.5f, 0.4f, 0.3f, 0.25f, 0.2f, 0.15f, 0.1f, 0.08f }, 0.5f, 0.01f);
            d.AddCrackle(0.01f, 0.12f, 220f, 0.7f, 1000, 3000, t => Exp(t, 0.05f));
            d.AddThump(0, 1f, 90f, 35f, 0.25f, 0.08f);
            d.AddNoise(0, 0.6f, 0.5f, t => Exp(t, 0.15f), t => Dsp.ExpLerp(2000, 200, t / 0.6f), 0.7f, Dsp.Mode.Low);
            d.Saturate(2f);
            d.Reverb(0.6f, 0.4f, 0.2f);
            return d;
        }

        static Dsp Whiff(Dsp d)
        {
            // Chybienie: ostrze tnie powietrze tuż obok.
            float dur = 0.22f;
            d.AddNoise(0, dur, 1f, t => Dsp.Window(t, dur, 0.06f, 0.14f), t => Dsp.ExpLerp(3500, 1200, t / dur), 3f, Dsp.Mode.Band);
            return d;
        }

        static Dsp Riposte(Dsp d)
        {
            // Riposta: zgrzyt ostrza, pchnięcie w ciało i głęboki pogłos.
            d.AddNoise(0, 0.12f, 0.5f, t => Dsp.Window(t, 0.12f, 0.02f, 0.08f), t => Dsp.ExpLerp(4000, 2500, t / 0.12f), 6f, Dsp.Mode.Band);
            d.AddModes(0, d.Range(1500, 1700), Dsp.BarRatios, new[] { 0.6f, 0.4f, 0.2f }, new[] { 0.15f, 0.1f, 0.06f }, 0.25f);
            float t0 = 0.09f;
            d.AddClick(t0, 1f, 3f, 6000f);
            d.AddThump(t0, 1f, 130f, 38f, 0.12f);
            d.AddThump(t0, 0.8f, 60f, 30f, 0.4f, 0.1f);
            d.AddNoise(t0, 0.3f, 0.6f, t => Exp(t, 0.07f) * (0.5f + 0.5f * Math.Abs(Sin(t * 220f + Sin(t * 70f) * 4f))), _ => 700f, 3f, Dsp.Mode.Band);
            d.AddCrackle(t0, 0.06f, 200f, 0.5f, 1200, 2500);
            d.Saturate(1.8f);
            d.Reverb(0.8f, 0.4f, 0.3f);
            return d;
        }

        static Dsp Flask(Dsp d)
        {
            // Korek, trzy łyki, stuk szkła.
            d.AddClick(0, 0.6f, 2f, 5000f);
            d.AddTone(0, 0.06f, 0.4f, t => Dsp.ExpLerp(900, 400, t / 0.06f), t => Exp(t, 0.02f));
            for (int i = 0; i < 3; i++)
            {
                float t0 = 0.2f + i * 0.24f + d.Range(-0.02f, 0.02f);
                d.AddNoise(t0, 0.12f, 0.5f, t => Dsp.Window(t, 0.12f, 0.02f, 0.08f), _ => 350f, 2f, Dsp.Mode.Band);
                for (int b = 0; b < 3; b++)
                {
                    float f0 = d.Range(250, 450), tb = t0 + d.Range(0, 0.08f);
                    d.AddTone(tb, 0.05f, 0.25f, t => f0 * (1f + t * 12f), t => Exp(t, 0.012f));
                }
            }
            d.AddModes(0.95f, d.Range(2100, 2400), new[] { 1f, 2.21f, 3.6f }, new[] { 1f, 0.4f, 0.2f }, new[] { 0.15f, 0.08f, 0.05f }, 0.25f);
            return d;
        }

        // ================================================================== Magia

        static Dsp CastCharge(Dsp d)
        {
            // Narastający, mroczny szum mocy: akord molowy wspinający się o oktawę.
            float dur = 0.75f;
            float root = d.Range(105, 115);
            float[] ch = { 1f, 1.189f, 1.498f };
            foreach (var r in ch)
                d.AddTone(0, dur, 0.18f, t => root * r * Dsp.ExpLerp(1f, 2f, t / dur) * (1f + 0.01f * Sin(t * 38f)), t => Dsp.Window(t, dur, 0.5f, 0.12f), Dsp.Wave.Saw, 900f);
            d.AddNoise(0, dur, 0.3f, t => Dsp.Window(t, dur, 0.6f, 0.1f), t => Dsp.ExpLerp(400, 3000, t / dur), 4f, Dsp.Mode.Band);
            return d;
        }

        static Dsp ArcaneBolt(Dsp d)
        {
            // Wypuszczenie pocisku: opadający, „brudny” ton i podmuch.
            float dur = 0.35f;
            float f0 = d.Range(700, 900);
            d.AddTone(0, dur, 0.4f, t => Dsp.ExpLerp(f0, f0 * 0.3f, t / dur) * (1f + 0.3f * Sin(t * 400f)), t => Dsp.AD(t, 0.005f, 0.1f), Dsp.Wave.Saw, 2500f);
            d.AddNoise(0, dur, 0.5f, t => Dsp.AD(t, 0.01f, 0.08f), t => Dsp.ExpLerp(2500, 600, t / dur), 1.5f, Dsp.Mode.Band);
            d.AddThump(0, 0.4f, 150f, 70f, 0.05f);
            return d;
        }

        static Dsp ArcaneImpact(Dsp d)
        {
            d.AddClick(0, 0.8f, 3f, 6000f);
            d.AddThump(0, 0.9f, 140f, 45f, 0.12f);
            d.AddNoise(0, 0.5f, 0.4f, t => Exp(t, 0.1f), t => Dsp.ExpLerp(3000, 300, t / 0.5f), 1.2f, Dsp.Mode.Low);
            d.AddNoise(0, 0.4f, 0.15f, t => Exp(t, 0.12f), _ => 6000f, 0.7f, Dsp.Mode.High);
            d.AddModes(0, d.Range(600, 700), new[] { 1f, 1.5f, 2.1f }, new[] { 0.5f, 0.3f, 0.2f }, new[] { 0.2f, 0.12f, 0.08f }, 0.2f);
            d.Reverb(0.6f, 0.4f, 0.2f);
            return d;
        }

        static Dsp FireCast(Dsp d)
        {
            // Buchnięcie płomienia: ryk szumu i trzaski.
            float dur = 0.7f;
            d.AddNoise(0, dur, 1f, t => Dsp.AD(t, 0.08f, 0.2f), t => Dsp.ExpLerp(300, 2500, Math.Min(1f, t / 0.12f)) * (1f - 0.6f * Math.Min(1f, t / dur)), 0.9f, Dsp.Mode.Low);
            d.AddNoise(0, dur, 0.25f, t => Dsp.AD(t, 0.05f, 0.15f), _ => 150f, 0.7f, Dsp.Mode.Low);
            d.AddCrackle(0.02f, dur, 60f, 0.35f, 1500, 5000, t => Exp(t, 0.2f));
            d.Saturate(1.4f);
            return d;
        }

        static Dsp FireExplosion(Dsp d)
        {
            // Wybuch ognia: grzmot, fala szumu, opadający żar.
            d.AddClick(0, 1f, 5f, 5000f);
            d.AddThump(0, 1f, 90f, 30f, 0.35f, 0.08f);
            d.AddNoise(0, 1.2f, 1f, t => Dsp.AD(t, 0.005f, 0.25f), t => Dsp.ExpLerp(3500, 250, Math.Min(1f, t / 0.8f)), 0.8f, Dsp.Mode.Low);
            d.AddCrackle(0.05f, 1.5f, 50f, 0.3f, 1500, 5000, t => Exp(t, 0.5f));
            d.Saturate(2.2f);
            d.Reverb(0.7f, 0.5f, 0.2f);
            return d;
        }

        static Dsp FrostCast(Dsp d)
        {
            // Mroźny podmuch: syk, szklane dzwonienie kryształków.
            float dur = 0.8f;
            d.AddNoise(0, dur, 0.4f, t => Dsp.AD(t, 0.05f, 0.25f), t => Dsp.ExpLerp(6000, 2500, t / dur), 1.5f, Dsp.Mode.High);
            d.AddNoise(0, dur, 0.3f, t => Dsp.AD(t, 0.1f, 0.3f), _ => 500f, 1.5f, Dsp.Mode.Band);
            for (int i = 0; i < 14; i++)
            {
                float t0 = d.Range(0, 0.5f);
                d.AddModes(t0, d.Range(2500, 6000), new[] { 1f, 2.7f }, new[] { 1f, 0.3f }, new[] { d.Range(0.05f, 0.2f), 0.04f }, 0.08f);
            }
            d.Reverb(0.6f, 0.2f, 0.25f);
            return d;
        }

        static Dsp FrostImpact(Dsp d)
        {
            d.AddClick(0, 1f, 2f, 9000f);
            d.AddThump(0, 0.6f, 160f, 60f, 0.06f);
            for (int i = 0; i < 18; i++)
                d.AddModes(d.Range(0, 0.08f), d.Range(2500, 7500), new[] { 1f, 2.4f }, new[] { 1f, 0.4f }, new[] { d.Range(0.04f, 0.25f), 0.05f }, 0.1f);
            d.AddNoise(0, 0.3f, 0.3f, t => Exp(t, 0.06f), _ => 5000f, 0.7f, Dsp.Mode.High);
            d.Reverb(0.5f, 0.2f, 0.2f);
            return d;
        }

        static Dsp Freeze(Dsp d)
        {
            // Zamarzanie: przyspieszające trzaski lodu i niskie skrzypienie.
            for (int i = 0; i < 16; i++)
            {
                float u = i / 16f;
                float t0 = 0.5f * (float)Math.Sqrt(u) + d.Range(-0.01f, 0.01f);
                float band = d.Range(1500, 4000);
                d.AddNoise(t0, 0.02f, d.Range(0.4f, 0.9f), t => Exp(t, 0.004f), _ => band, 3f, Dsp.Mode.Band);
            }
            d.AddTone(0.05f, 0.6f, 0.25f, t => 70f + 15f * Sin(t * 13f) + 8f * Sin(t * 41f), t => Dsp.Window(t, 0.6f, 0.1f, 0.3f), Dsp.Wave.Saw, 600f);
            d.AddModes(0.55f, d.Range(3000, 3500), new[] { 1f, 2.3f, 3.7f }, new[] { 1f, 0.5f, 0.3f }, new[] { 0.25f, 0.12f, 0.08f }, 0.25f);
            d.Reverb(0.5f, 0.3f, 0.2f);
            return d;
        }

        static Dsp Shatter(Dsp d)
        {
            // Roztrzaskanie: pęknięcie bryły lodu, deszcz odłamków.
            d.AddClick(0, 1f, 3f, 10000f);
            d.AddThump(0, 0.7f, 150f, 50f, 0.1f);
            d.AddNoise(0, 0.25f, 0.8f, t => Exp(t, 0.05f), _ => 2500f, 0.7f, Dsp.Mode.High);
            for (int i = 0; i < 45; i++)
            {
                float t0 = (float)Math.Pow(d.Rand(), 1.6f) * 0.5f;
                d.AddModes(t0, d.Range(2000, 9000), new[] { 1f, 2.6f }, new[] { 1f, 0.3f }, new[] { d.Range(0.03f, 0.3f), 0.03f }, d.Range(0.05f, 0.15f));
            }
            d.Reverb(0.6f, 0.2f, 0.25f);
            return d;
        }

        static Dsp Zap(Dsp d)
        {
            // Wyładowanie: trzask, brzęczenie łuku elektrycznego, iskry.
            d.AddClick(0, 1f, 1.5f, 12000f);
            float f = d.Range(55, 75);
            d.AddTone(0, 0.35f, 0.5f, t => f * (1f + 0.15f * (float)Math.Sin(t * 170f)), t => Exp(t, 0.08f), Dsp.Wave.Square, 3000f);
            d.AddNoise(0, 0.35f, 0.7f, t => Exp(t, 0.06f) * (d.Rand() < 0.3f ? 1f : 0.2f), _ => 3000f, 0.7f, Dsp.Mode.High);
            d.AddCrackle(0.01f, 0.3f, 90f, 0.5f, 3000, 8000, t => Exp(t, 0.08f));
            d.Saturate(2.5f);
            return d;
        }

        static Dsp Thunder(Dsp d)
        {
            // Piorun: ostry trzask i przetaczający się grzmot.
            d.AddClick(0, 1f, 2f, 12000f);
            d.AddNoise(0, 0.15f, 0.9f, t => Exp(t, 0.03f), _ => 3500f, 0.7f, Dsp.Mode.High);
            float a = d.Range(3f, 6f), b = d.Range(1f, 2f);
            d.AddNoise(0.03f, 3f, 1f, t => Dsp.AD(t, 0.05f, 0.8f) * (0.55f + 0.45f * Sin(t * a + Sin(t * b) * 2f)), t => Dsp.ExpLerp(500, 90, Math.Min(1f, t / 1.5f)), 0.9f, Dsp.Mode.Low);
            d.AddThump(0.02f, 0.8f, 70f, 30f, 0.5f, 0.1f);
            d.Saturate(1.8f);
            d.Reverb(0.85f, 0.5f, 0.3f);
            return d;
        }

        static Dsp Heal(Dsp d)
        {
            // Leczenie: jasny, ale smutny akord (sus2) z chórem i migotaniem.
            float root = 220f;
            foreach (var r in new[] { 1f, 1.122f, 1.498f, 2f })
                d.AddTone(0, 1.9f, 0.14f, t => root * r * (1f + 0.003f * Sin(t * 30f)), t => Dsp.Window(t, 1.9f, 0.35f, 1.2f));
            d.AddChoir(0.05f, 1.8f, 0.3f, root * 2f, 0.4f, 1.1f, 0.004f, 3, 0.5f);
            for (int i = 0; i < 10; i++)
                d.AddModes(d.Range(0.1f, 1.2f), d.Range(1800, 3500), new[] { 1f }, new[] { 1f }, new[] { 0.3f }, 0.06f);
            d.Reverb(0.85f, 0.3f, 0.4f);
            return d;
        }

        static Dsp Enchant(Dsp d)
        {
            // Nasycenie broni: zgrzyt dłoni po ostrzu i narastający ton.
            d.AddNoise(0, 0.5f, 0.5f, t => Dsp.Window(t, 0.5f, 0.1f, 0.3f), t => Dsp.ExpLerp(2000, 5000, t / 0.5f), 5f, Dsp.Mode.Band);
            d.AddModes(0.35f, d.Range(1300, 1500), Dsp.BarRatios, new[] { 1f, 0.5f, 0.3f }, new[] { 0.6f, 0.3f, 0.15f }, 0.35f);
            d.AddTone(0, 0.8f, 0.2f, t => Dsp.ExpLerp(220, 440, t / 0.5f), t => Dsp.Window(t, 0.8f, 0.3f, 0.4f), Dsp.Wave.Saw, 1500f);
            d.Reverb(0.6f, 0.3f, 0.25f);
            return d;
        }

        static Dsp Barrier(Dsp d)
        {
            // Osłona: szklany, drżący akord.
            float root = 330f;
            foreach (var r in new[] { 1f, 1.5f, 2.01f })
                d.AddTone(0, 1.2f, 0.2f, t => root * r, t => Dsp.Window(t, 1.2f, 0.08f, 0.8f) * (0.7f + 0.3f * Sin(t * 45f)));
            d.AddModes(0, 1320f, new[] { 1f, 2.3f, 3.1f }, new[] { 0.6f, 0.3f, 0.2f }, new[] { 0.8f, 0.4f, 0.3f }, 0.2f);
            d.AddNoise(0, 0.3f, 0.2f, t => Exp(t, 0.08f), _ => 4000f, 0.7f, Dsp.Mode.High);
            d.Reverb(0.7f, 0.3f, 0.3f);
            return d;
        }

        static Dsp Shockwave(Dsp d)
        {
            // Fala uderzeniowa: głuche „łup” i szum rozchodzący się na boki.
            d.AddThump(0, 1f, 90f, 32f, 0.2f, 0.06f);
            d.AddNoise(0, 0.9f, 0.9f, t => Dsp.AD(t, 0.02f, 0.2f), t => Dsp.ExpLerp(2500, 150, t / 0.9f), 1f, Dsp.Mode.Low);
            d.AddClick(0, 0.6f, 4f, 4000f);
            d.Saturate(1.6f);
            d.Reverb(0.6f, 0.5f, 0.2f);
            return d;
        }

        static Dsp Quake(Dsp d)
        {
            // Uderzenie w ziemię: wstrząs, dudnienie, sypiący się gruz.
            d.AddClick(0, 1f, 6f, 3000f);
            d.AddThump(0, 1f, 70f, 28f, 0.4f, 0.08f);
            d.AddNoise(0, 1.4f, 0.8f, t => Dsp.AD(t, 0.01f, 0.4f) * (0.7f + 0.3f * Sin(t * 25f)), _ => 160f, 0.8f, Dsp.Mode.Low);
            d.AddCrackle(0.05f, 1.0f, 70f, 0.4f, 800, 3000, t => Exp(t, 0.3f));
            d.Saturate(2f);
            d.Reverb(0.6f, 0.5f, 0.15f);
            return d;
        }

        static Dsp Warcry(Dsp d)
        {
            // Okrzyk wojenny: ochrypły, zbiorowy ryk w formantach „a”, z rogiem pod spodem.
            d.AddChoir(0, 1.2f, 0.9f, d.Range(125, 135), 0.06f, 0.5f, 0.012f, 5, 0.2f);
            d.AddChoir(0, 1.2f, 0.6f, 98f, 0.08f, 0.5f, 0.01f, 4, 0.1f);
            d.AddNoise(0, 1.1f, 0.4f, t => Dsp.Window(t, 1.1f, 0.05f, 0.5f), _ => 900f, 1.5f, Dsp.Mode.Band);
            d.Saturate(2.2f);
            d.Reverb(0.75f, 0.4f, 0.3f);
            return d;
        }

        static Dsp BloodMagic(Dsp d)
        {
            // Krwawy pakt: dwa uderzenia serca, mokre ciągnięcie i niski pomruk.
            d.AddThump(0, 1f, 70f, 40f, 0.08f);
            d.AddThump(0.18f, 0.8f, 65f, 38f, 0.08f);
            d.AddNoise(0.05f, 0.7f, 0.5f, t => Dsp.Window(t, 0.7f, 0.1f, 0.4f) * (0.5f + 0.5f * Math.Abs(Sin(t * 120f + Sin(t * 40f) * 4f))), t => Dsp.ExpLerp(400, 900, t / 0.7f), 4f, Dsp.Mode.Band);
            d.AddTone(0, 1f, 0.3f, t => 55f * (1f + 0.02f * Sin(t * 20f)), t => Dsp.Window(t, 1f, 0.2f, 0.6f), Dsp.Wave.Saw, 300f);
            d.Saturate(1.5f);
            d.Reverb(0.7f, 0.5f, 0.25f);
            return d;
        }

        static Dsp Chains(Dsp d)
        {
            // Łańcuchy potępionych: grzechot ogniw i świst.
            for (int i = 0; i < 26; i++)
            {
                float t0 = (float)Math.Pow(d.Rand(), 1.3f) * 0.6f;
                d.AddModes(t0, d.Range(1400, 3600), new[] { 1f, 1.7f, 2.9f }, new[] { 1f, 0.5f, 0.3f }, new[] { d.Range(0.02f, 0.08f), 0.03f, 0.02f }, d.Range(0.15f, 0.35f));
                d.AddClick(t0, 0.2f, 1f, 6000f);
            }
            d.AddNoise(0, 0.4f, 0.3f, t => Dsp.Window(t, 0.4f, 0.1f, 0.2f), t => Dsp.ExpLerp(800, 2500, t / 0.4f), 2f, Dsp.Mode.Band);
            d.Reverb(0.6f, 0.3f, 0.2f);
            return d;
        }

        // ================================================================== Efekty trwające

        static Dsp BurnTick(Dsp d)
        {
            d.AddNoise(0, 0.25f, 0.4f, t => Dsp.AD(t, 0.03f, 0.07f), _ => 1200f, 0.7f, Dsp.Mode.Low);
            d.AddCrackle(0, 0.2f, 60f, 0.8f, 1500, 5000, t => Exp(t, 0.08f));
            return d;
        }

        static Dsp BleedTick(Dsp d)
        {
            float band = d.Range(450, 600);
            d.AddNoise(0, 0.1f, 0.6f, t => Dsp.AD(t, 0.005f, 0.03f), _ => band, 4f, Dsp.Mode.Band);
            float f = d.Range(260, 320);
            d.AddTone(0.01f, 0.08f, 0.4f, t => f * (1f - t * 6f), t => Exp(t, 0.02f));
            return d;
        }

        // ================================================================== Wrogowie

        static Dsp TelegraphHeavy(Dsp d)
        {
            // Zapowiedź ciężkiego ciosu: niski gong/pomruk – ostrzega, nie męczy.
            float f = d.Range(80, 92);
            d.AddModes(0, f, new[] { 1f, 1.52f, 2.13f, 2.9f }, new[] { 1f, 0.6f, 0.4f, 0.2f }, new[] { 0.6f, 0.4f, 0.3f, 0.2f }, 0.7f, 0.004f, 0.03f);
            d.AddNoise(0, 0.5f, 0.3f, t => Dsp.Window(t, 0.5f, 0.15f, 0.3f), _ => 300f, 1.5f, Dsp.Mode.Band);
            d.Saturate(1.5f);
            return d;
        }

        static Dsp TelegraphUnblockable(Dsp d)
        {
            // Cios nie do zablokowania: dysonansowy dzwon z trytonem – wyraźnie „uciekaj”.
            d.AddBell(0, 523.3f, 0.6f, 0.9f);
            d.AddBell(0, 740f, 0.45f, 0.8f);
            d.AddTone(0, 0.8f, 0.15f, t => Dsp.ExpLerp(300, 600, t / 0.8f), t => Dsp.Window(t, 0.8f, 0.3f, 0.2f), Dsp.Wave.Saw, 1500f);
            d.Reverb(0.6f, 0.3f, 0.25f);
            return d;
        }

        static Dsp Growl(Dsp d, float scale)
        {
            // Warczenie: chrapliwa piła przez formanty, szorstka modulacja amplitudy.
            float dur = d.Duration * 0.9f;
            float f = d.Range(60, 80) / scale;
            var tmp = new Dsp(dur, d.sr, (int)(d.Rand() * 100000));
            tmp.AddTone(0, dur, 0.8f, t => f * (1f + 0.08f * Sin(t * 23f) + 0.05f * tmp.Noise()), t => Dsp.Window(t, dur, 0.1f, 0.4f) * (0.6f + 0.4f * Math.Abs(Sin(t * 70f))), Dsp.Wave.Saw);
            tmp.AddNoise(0, dur, 0.5f, t => Dsp.Window(t, dur, 0.1f, 0.4f), _ => 700f, 0.7f, Dsp.Mode.Low);
            var fa = new Dsp.Svf(); var fb = new Dsp.Svf();
            for (int i = 0; i < tmp.buf.Length && i < d.buf.Length; i++)
            {
                float t = i / (float)d.sr;
                float x = tmp.buf[i];
                d.buf[i] += fa.Process(x, Dsp.Lerp(500, 350, t / dur) / scale, 4f, d.sr, Dsp.Mode.Band) + 0.6f * fb.Process(x, 1100f / scale, 5f, d.sr, Dsp.Mode.Band);
            }
            d.Saturate(2.5f);
            return d;
        }

        static Dsp BossRoar(Dsp d)
        {
            // Ryk Kasztelana: dwa warczenia oktawę od siebie, subbas i echo sali.
            Growl(d, 1.6f);
            var hi = new Dsp(d.Duration, d.sr, 991);
            Growl(hi, 0.8f);
            for (int i = 0; i < d.buf.Length && i < hi.buf.Length; i++) d.buf[i] += hi.buf[i] * 0.5f;
            d.AddTone(0, 2f, 0.6f, t => 40f + 5f * Sin(t * 9f), t => Dsp.Window(t, 2f, 0.2f, 1f), Dsp.Wave.Sine);
            d.Saturate(1.8f);
            d.Reverb(0.85f, 0.4f, 0.35f);
            return d;
        }

        static Dsp DeathGhoul(Dsp d)
        {
            // Śmierć ghula: bulgoczący, opadający charkot i upadek ciała.
            float dur = 0.8f;
            d.AddTone(0, dur, 0.6f, t => Dsp.ExpLerp(110, 45, t / dur) * (1f + 0.1f * Sin(t * 30f)), t => Dsp.Window(t, dur, 0.05f, 0.4f) * (0.5f + 0.5f * Math.Abs(Sin(t * 55f + Sin(t * 17f) * 3f))), Dsp.Wave.Saw, 900f);
            d.AddNoise(0, dur, 0.4f, t => Dsp.Window(t, dur, 0.05f, 0.4f) * (0.5f + 0.5f * Math.Abs(Sin(t * 140f))), _ => 600f, 3f, Dsp.Mode.Band);
            d.AddThump(0.85f, 0.9f, 100f, 40f, 0.12f);
            d.AddCrackle(0.85f, 0.2f, 80f, 0.3f, 800, 2000, t => Exp(t, 0.06f));
            d.Saturate(2f);
            return d;
        }

        static Dsp DeathBones(Dsp d)
        {
            // Rozsypujący się szkielet: seria kościanych stuknięć odbijających się coraz słabiej.
            float t = 0;
            for (int i = 0; i < 28; i++)
            {
                t += d.Range(0.005f, 0.05f) * (1f + i * 0.04f);
                if (t > d.Duration - 0.1f) break;
                float a = Dsp.Lerp(1f, 0.25f, i / 28f) * d.Range(0.5f, 1f);
                d.AddModes(t, d.Range(700, 2200), new[] { 1f, 2.3f, 3.8f }, new[] { 1f, 0.4f, 0.2f }, new[] { d.Range(0.015f, 0.04f), 0.02f, 0.01f }, a * 0.6f);
                d.AddClick(t, a * 0.3f, 1f, 5000f);
            }
            d.AddThump(0.05f, 0.5f, 120f, 50f, 0.06f);
            return d;
        }

        static Dsp DeathArmored(Dsp d)
        {
            // Upadek w zbroi: stęknięcie, brzęk płyt o kamień, głuchy łomot.
            d.AddNoise(0, 0.35f, 0.3f, t => Dsp.Window(t, 0.35f, 0.05f, 0.25f), t => Dsp.ExpLerp(900, 500, t / 0.35f), 3f, Dsp.Mode.Band);
            float[] times = { 0.35f, 0.48f, 0.62f, 0.7f };
            foreach (var t0 in times)
            {
                d.AddClick(t0, 0.6f, 2f, 7000f);
                d.AddModes(t0, d.Range(380, 700), Dsp.PlateRatios, new[] { 1f, 0.7f, 0.6f, 0.4f, 0.3f }, new[] { 0.2f, 0.15f, 0.1f, 0.08f, 0.05f }, d.Range(0.25f, 0.4f), 0.01f);
            }
            d.AddThump(0.4f, 1f, 90f, 38f, 0.15f);
            d.Saturate(1.4f);
            d.Reverb(0.5f, 0.4f, 0.15f);
            return d;
        }

        static Dsp BossDeath(Dsp d)
        {
            // Upadek Kasztelana: gasnący ryk, grzmot i żałobny dzwon.
            var roar = new Dsp(2f, d.sr, 55);
            Growl(roar, 1.8f);
            for (int i = 0; i < roar.buf.Length && i < d.buf.Length; i++)
            {
                float t = i / (float)d.sr;
                d.buf[i] += roar.buf[i] * 0.6f * Exp(t, 0.8f);
            }
            d.AddThump(0.3f, 1f, 70f, 25f, 0.8f, 0.15f);
            d.AddNoise(0.3f, 3f, 0.8f, t => Dsp.AD(t, 0.02f, 0.9f), t => Dsp.ExpLerp(1500, 100, Math.Min(1f, t / 2f)), 0.8f, Dsp.Mode.Low);
            d.AddBell(1.2f, 110f, 0.6f, 4f);
            d.Saturate(1.5f);
            d.Reverb(0.9f, 0.4f, 0.4f);
            return d;
        }

        // ================================================================== Przebieg gry

        static Dsp FloorStart(Dsp d)
        {
            // Wejście na piętro: pojedyncze uderzenie wielkiego dzwonu wieży.
            d.AddBell(0, 110f, 0.8f, 5f);
            d.AddThump(0, 0.3f, 60f, 40f, 0.5f, 0.2f);
            d.Reverb(0.92f, 0.4f, 0.45f);
            return d;
        }

        static Dsp BossIntro(Dsp d)
        {
            // Sala bossa: dwa niskie dzwony, narastający pomruk i bęben.
            d.AddBell(0, 73.4f, 0.8f, 5f);
            d.AddBell(1.6f, 69.3f, 0.8f, 5f);
            d.AddTone(0, 6f, 0.25f, t => 36.7f, t => Dsp.Window(t, 6f, 2.5f, 2.5f), Dsp.Wave.Saw, 180f);
            d.AddChoir(1.2f, 5f, 0.3f, 146.8f, 1.5f, 2.5f, 0.005f, 4, 0.8f);
            d.AddChoir(1.2f, 5f, 0.25f, 155.6f, 1.5f, 2.5f, 0.005f, 4, 0.8f); // półton – niepokój
            d.AddThump(3.2f, 0.9f, 80f, 35f, 0.4f, 0.1f);
            d.Reverb(0.92f, 0.4f, 0.45f);
            return d;
        }

        static Dsp WaveHorn(Dsp d)
        {
            // Róg wojenny: niska piła z formantem, zadęcie z podjazdem tonu.
            float f = 98f;
            float dur = 2.2f;
            for (int i = 0; i < 3; i++)
            {
                float det = 1f + (i - 1) * 0.004f;
                d.AddTone(0, dur, 0.35f, t => f * det * (t < 0.15f ? Dsp.Lerp(0.94f, 1f, t / 0.15f) : 1f) * (1f + 0.004f * Sin(t * 30f)),
                    t => Dsp.Window(t, dur, 0.18f, 0.8f), Dsp.Wave.Saw, 0f);
            }
            d.Filter(700f, Dsp.Mode.Low, 1.2f);
            d.AddNoise(0, dur, 0.08f, t => Dsp.Window(t, dur, 0.1f, 0.8f), _ => 1200f, 2f, Dsp.Mode.Band);
            d.Saturate(1.5f);
            d.Reverb(0.9f, 0.5f, 0.4f);
            return d;
        }

        static Dsp FloorCleared(Dsp d)
        {
            // Piętro oczyszczone: chór rozwiązuje się z molowego w durowy (tercja pikardyjska).
            float root = 146.8f;
            d.AddChoir(0, 2.2f, 0.4f, root, 0.4f, 0.8f);
            d.AddChoir(0, 2.2f, 0.3f, root * 1.189f, 0.4f, 0.8f);
            d.AddChoir(0, 2.2f, 0.3f, root * 1.498f, 0.4f, 0.8f);
            d.AddChoir(1.6f, 2.8f, 0.4f, root, 0.6f, 1.6f);
            d.AddChoir(1.6f, 2.8f, 0.3f, root * 1.26f, 0.6f, 1.6f);
            d.AddChoir(1.6f, 2.8f, 0.3f, root * 1.498f, 0.6f, 1.6f);
            d.AddBell(1.6f, 293.7f, 0.25f, 2.5f);
            d.Reverb(0.9f, 0.4f, 0.45f);
            return d;
        }

        static Dsp Victory(Dsp d)
        {
            // Zwycięstwo: dzwony wieży i szeroki chór – triumf, ale okupiony.
            float root = 146.8f;
            d.AddBell(0, 220f, 0.6f, 4f);
            d.AddBell(0.5f, 293.7f, 0.6f, 4f);
            d.AddBell(1.0f, 146.8f, 0.8f, 5f);
            foreach (var r in new[] { 1f, 1.26f, 1.498f, 2f })
                d.AddChoir(0.9f, 5.5f, 0.3f, root * r, 1.2f, 2.5f, 0.005f, 3, 0.3f);
            d.AddTone(0.9f, 5.5f, 0.3f, t => 73.4f, t => Dsp.Window(t, 5.5f, 1.5f, 2.5f), Dsp.Wave.Saw, 250f);
            d.Reverb(0.92f, 0.4f, 0.45f);
            return d;
        }

        static Dsp PlayerDeath(Dsp d)
        {
            // Śmierć bohatera: głuchy cios, opadający dysonansowy chór i dzwon żałobny.
            d.AddThump(0, 1f, 80f, 28f, 0.6f, 0.12f);
            d.AddNoise(0, 1.5f, 0.6f, t => Dsp.AD(t, 0.01f, 0.5f), t => Dsp.ExpLerp(1200, 80, t / 1.5f), 0.8f, Dsp.Mode.Low);
            float[] cluster = { 146.8f, 155.6f, 207.7f, 220f };
            foreach (var f in cluster)
            {
                float ff = f;
                d.AddChoir(0.2f, 5f, 0.3f, ff, 1.2f, 2.5f, 0.006f, 3, 0.6f);
            }
            d.AddBell(0.8f, 98f, 0.5f, 4f);
            d.Reverb(0.93f, 0.4f, 0.5f);
            return d;
        }

        static Dsp SoulGain(Dsp d)
        {
            // Dusza wchłonięta: szept i chłodny dzwoneczek.
            d.AddNoise(0, 0.7f, 0.5f, t => Dsp.Window(t, 0.7f, 0.25f, 0.35f), t => Dsp.ExpLerp(600, 2800, t / 0.7f), 5f, Dsp.Mode.Band);
            d.AddModes(0.35f, d.Range(1500, 1800), new[] { 1f, 2.76f }, new[] { 1f, 0.2f }, new[] { 0.35f, 0.1f }, 0.3f);
            d.Reverb(0.7f, 0.3f, 0.35f);
            return d;
        }

        static Dsp Heartbeat(Dsp d)
        {
            d.AddThump(0, 1f, 60f, 38f, 0.07f, 0.03f);
            d.AddThump(0.22f, 0.75f, 55f, 36f, 0.08f, 0.03f);
            d.Filter(200f, Dsp.Mode.Low);
            return d;
        }
    }
}
