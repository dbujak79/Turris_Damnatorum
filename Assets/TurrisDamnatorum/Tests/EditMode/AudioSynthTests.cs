using System;
using System.IO;
using NUnit.Framework;

namespace Turris.Tests
{
    /// <summary>Synteza dźwięku: poprawność sygnału, powtarzalność, bezszwowe pętle muzyki. Opcjonalny eksport WAV.</summary>
    public class AudioSynthTests
    {
        static float Peak(float[] x) { float m = 0; foreach (var v in x) m = Math.Max(m, Math.Abs(v)); return m; }

        [Test]
        public void AllSounds_AreFinite_Normalized_AndEndInSilence()
        {
            foreach (SoundId id in Enum.GetValues(typeof(SoundId)))
            {
                var info = SoundInfo.For(id);
                Assert.GreaterOrEqual(info.variants, 1, id.ToString());
                Assert.GreaterOrEqual(info.maxVoices, 1, id.ToString());
                for (int v = 0; v < info.variants; v++)
                {
                    var x = SoundSynth.Generate(id, v);
                    Assert.Greater(x.Length, SoundSynth.SampleRate / 20, $"{id}: za krótki");
                    Assert.Less(x.Length, SoundSynth.SampleRate * 8, $"{id}: za długi");
                    foreach (var s in x) Assert.IsFalse(float.IsNaN(s) || float.IsInfinity(s), $"{id}_{v}: NaN/Inf");
                    float peak = Peak(x);
                    Assert.LessOrEqual(peak, 0.9f, $"{id}_{v}: przester");
                    Assert.Greater(peak, 0.5f, $"{id}_{v}: cisza");
                    Assert.Less(Math.Abs(x[x.Length - 1]), 0.01f, $"{id}_{v}: urwany koniec (trzask)");
                }
            }
        }

        [Test]
        public void Sounds_AreDeterministic_AndVariantsDiffer()
        {
            var a = SoundSynth.Generate(SoundId.HitFlesh, 0);
            var b = SoundSynth.Generate(SoundId.HitFlesh, 0);
            CollectionAssert.AreEqual(a, b, "Ten sam wariant = ten sam dźwięk");
            var c = SoundSynth.Generate(SoundId.HitFlesh, 1);
            int n = Math.Min(a.Length, c.Length);
            double diff = 0;
            for (int i = 0; i < n; i++) diff += Math.Abs(a[i] - c[i]);
            Assert.Greater(diff / n, 0.005, "Warianty brzmią inaczej");
        }

        [Test]
        public void Music_IsStereo_Finite_AndLoopsWithoutClick()
        {
            foreach (MusicId id in Enum.GetValues(typeof(MusicId)))
            {
                if (id == MusicId.None) continue;
                var x = MusicSynth.Generate(id);
                Assert.AreEqual(0, x.Length % 2, id.ToString());
                Assert.Greater(x.Length / 2, MusicSynth.SampleRate * 20, $"{id}: pętla za krótka");
                foreach (var s in x) Assert.IsFalse(float.IsNaN(s) || float.IsInfinity(s), $"{id}: NaN/Inf");
                Assert.LessOrEqual(Peak(x), 1f, id.ToString());
                // Szew pętli: skok między ostatnią a pierwszą próbką nie większy niż typowe skoki wewnątrz utworu.
                int frames = x.Length / 2;
                float maxStep = 0;
                for (int i = 1; i < frames; i++) maxStep = Math.Max(maxStep, Math.Abs(x[2 * i] - x[2 * i - 2]));
                float seam = Math.Abs(x[0] - x[2 * (frames - 1)]);
                Assert.LessOrEqual(seam, maxStep + 1e-4f, $"{id}: trzask na szwie pętli");
            }
        }

        [Test]
        public void Loop_FoldsTailOntoStart()
        {
            var x = new float[] { 1, 2, 3, 4, 5, 6 };
            CollectionAssert.AreEqual(new float[] { 6, 8, 3, 4 }, Dsp.FoldLoop(x, 4));
        }

        /// <summary>Zapisuje wszystkie dźwięki do WAV (katalog w TURRIS_SOUND_DIR) – do odsłuchu i analizy.</summary>
        [Test]
        public void ExportWav_WhenRequested()
        {
            string dir = Environment.GetEnvironmentVariable("TURRIS_SOUND_DIR");
            if (string.IsNullOrEmpty(dir)) { Assert.Ignore("Ustaw TURRIS_SOUND_DIR, aby zapisać pliki WAV."); return; }
            Directory.CreateDirectory(dir);
            foreach (SoundId id in Enum.GetValues(typeof(SoundId)))
                for (int v = 0; v < SoundInfo.For(id).variants; v++)
                    WriteWav(Path.Combine(dir, $"{id}_{v}.wav"), SoundSynth.Generate(id, v), SoundSynth.SampleRate, 1);
            foreach (MusicId id in Enum.GetValues(typeof(MusicId)))
                if (id != MusicId.None)
                    WriteWav(Path.Combine(dir, $"music_{id}.wav"), MusicSynth.Generate(id), MusicSynth.SampleRate, 2);
        }

        static void WriteWav(string path, float[] data, int rate, int channels)
        {
            using (var w = new BinaryWriter(File.Create(path)))
            {
                int bytes = data.Length * 2;
                w.Write(new[] { 'R', 'I', 'F', 'F' }); w.Write(36 + bytes);
                w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' }); w.Write(16);
                w.Write((short)1); w.Write((short)channels); w.Write(rate); w.Write(rate * channels * 2);
                w.Write((short)(channels * 2)); w.Write((short)16);
                w.Write(new[] { 'd', 'a', 't', 'a' }); w.Write(bytes);
                foreach (var s in data) w.Write((short)Math.Round(Math.Max(-1f, Math.Min(1f, s)) * 32767f));
            }
        }
    }
}
