using System;

namespace Turris
{
    /// <summary>
    /// Budulec syntezy dźwięku: szum, oscylatory, filtry, obwiednie, rezonatory (metal, dzwony), pogłos.
    /// Czysty C# (bez API Unity) – można go wywoływać w wątku tła i w testach EditMode.
    /// Wszystkie metody „Add…” dopisują dźwięk do bufora mono od chwili t0 (sekundy).
    /// </summary>
    public sealed class Dsp
    {
        public readonly int sr;
        public readonly float[] buf;
        uint state;

        public Dsp(float seconds, int sampleRate, int seed)
        {
            sr = sampleRate;
            buf = new float[Math.Max(1, (int)(seconds * sampleRate))];
            state = (uint)(seed * 747796405 + 2891336453u) | 1u;
        }

        public float Duration => buf.Length / (float)sr;

        // ------------------------------------------------------------------ Losowość (deterministyczna)

        /// <summary>Losowa liczba 0..1 (xorshift – szybki i powtarzalny dla danego ziarna).</summary>
        public float Rand()
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            return (state & 0xFFFFFF) / 16777216f;
        }

        public float Range(float a, float b) => a + (b - a) * Rand();
        public float Noise() => Rand() * 2f - 1f;

        // ------------------------------------------------------------------ Obwiednie

        /// <summary>Atak liniowy, potem zanik wykładniczy (decay = czas spadku do ~37%).</summary>
        public static float AD(float t, float attack, float decay)
        {
            if (t < 0) return 0;
            if (t < attack) return t / attack;
            return (float)Math.Exp(-(t - attack) / Math.Max(1e-4f, decay));
        }

        /// <summary>Gładkie okno: narasta przez attack, trwa, opada przez release (sinus²).</summary>
        public static float Window(float t, float dur, float attack, float release)
        {
            if (t < 0 || t > dur) return 0;
            float a = attack > 0 && t < attack ? t / attack : 1f;
            float r = release > 0 && t > dur - release ? (dur - t) / release : 1f;
            float v = Math.Min(a, r);
            float s = (float)Math.Sin(v * Math.PI * 0.5);
            return s * s;
        }

        public static float Lerp(float a, float b, float t) => a + (b - a) * (t < 0 ? 0 : t > 1 ? 1 : t);
        /// <summary>Interpolacja wykładnicza (dla częstotliwości).</summary>
        public static float ExpLerp(float a, float b, float t) => a * (float)Math.Pow(b / a, t < 0 ? 0 : t > 1 ? 1 : t);

        // ------------------------------------------------------------------ Filtr stanu (TPT SVF)

        public enum Mode { Low, Band, High }

        /// <summary>Filtr zmiennej stanu (topologia TPT) – stabilny przy szybkiej zmianie częstotliwości.</summary>
        public struct Svf
        {
            float ic1, ic2;
            public float Process(float x, float cutoff, float q, int sr, Mode mode)
            {
                cutoff = Math.Max(20f, Math.Min(cutoff, sr * 0.45f));
                float g = (float)Math.Tan(Math.PI * cutoff / sr);
                float k = 1f / Math.Max(0.1f, q);
                float a1 = 1f / (1f + g * (g + k)), a2 = g * a1, a3 = g * a2;
                float v3 = x - ic2;
                float v1 = a1 * ic1 + a2 * v3;
                float v2 = ic2 + a2 * ic1 + a3 * v3;
                ic1 = 2f * v1 - ic1; ic2 = 2f * v2 - ic2;
                switch (mode)
                {
                    case Mode.Low: return v2;
                    case Mode.Band: return v1;
                    default: return x - k * v1 - v2;
                }
            }
        }

        // ------------------------------------------------------------------ Źródła

        /// <summary>Szum przez filtr o zmiennej częstotliwości. env i cutoff dostają czas względny (0..dur, s).</summary>
        public void AddNoise(float t0, float dur, float amp, Func<float, float> env, Func<float, float> cutoff, float q = 0.7f, Mode mode = Mode.Low)
        {
            var f = new Svf();
            int s0 = (int)(t0 * sr), n = (int)(dur * sr);
            for (int i = 0; i < n; i++)
            {
                int idx = s0 + i;
                if (idx < 0) continue;
                if (idx >= buf.Length) break;
                float t = i / (float)sr;
                float e = env(t);
                float y = f.Process(Noise(), cutoff(t), q, sr, mode);
                buf[idx] += y * e * amp;
            }
        }

        public enum Wave { Sine, Saw, Square, Triangle }

        static float Osc(Wave w, double phase)
        {
            double p = phase - Math.Floor(phase);
            switch (w)
            {
                case Wave.Saw: return (float)(2.0 * p - 1.0);
                case Wave.Square: return p < 0.5 ? 1f : -1f;
                case Wave.Triangle: return (float)(p < 0.5 ? 4.0 * p - 1.0 : 3.0 - 4.0 * p);
                default: return (float)Math.Sin(2.0 * Math.PI * p);
            }
        }

        /// <summary>Oscylator o zmiennej częstotliwości (freq(t) w Hz), opcjonalnie przez filtr dolnoprzepustowy.</summary>
        public void AddTone(float t0, float dur, float amp, Func<float, float> freq, Func<float, float> env, Wave wave = Wave.Sine, float lowpass = 0f, float phase0 = 0f)
        {
            var f = new Svf();
            int s0 = (int)(t0 * sr), n = (int)(dur * sr);
            double phase = phase0;
            for (int i = 0; i < n; i++)
            {
                int idx = s0 + i;
                float t = i / (float)sr;
                phase += freq(t) / sr;
                if (idx < 0) continue;
                if (idx >= buf.Length) break;
                float y = Osc(wave, phase);
                if (lowpass > 0) y = f.Process(y, lowpass, 0.7f, sr, Mode.Low);
                buf[idx] += y * env(t) * amp;
            }
        }

        /// <summary>
        /// Rezonator modalny: suma tłumionych sinusów (metal, szkło, dzwon, drewno).
        /// ratios – stosunki częstotliwości do bazowej, amps – głośności, decays – czasy zaniku (s).
        /// </summary>
        public void AddModes(float t0, float baseFreq, float[] ratios, float[] amps, float[] decays, float amp, float detune = 0f, float attack = 0.0005f)
        {
            float maxDecay = 0;
            foreach (var d in decays) maxDecay = Math.Max(maxDecay, d);
            int s0 = (int)(t0 * sr), n = (int)(Math.Min(maxDecay * 6f, 12f) * sr);
            for (int m = 0; m < ratios.Length; m++)
            {
                double f = baseFreq * ratios[m] * (1f + detune * Noise());
                if (f >= sr * 0.48) continue;
                double w = 2.0 * Math.PI * f / sr;
                double ph = Rand() * Math.PI * 2;
                float a = amps[Math.Min(m, amps.Length - 1)] * amp;
                float d = decays[Math.Min(m, decays.Length - 1)];
                double k = Math.Exp(-1.0 / (d * sr));
                double e = 1.0;
                int len = Math.Min(n, (int)(d * 7f * sr));
                // Rekurencyjny sinus (szybki): y[n] = 2cos(w) y[n-1] − y[n-2].
                double c = 2.0 * Math.Cos(w), y1 = Math.Sin(ph - w), y2 = Math.Sin(ph - 2 * w);
                int atk = Math.Max(1, (int)(attack * sr));
                for (int i = 0; i < len; i++)
                {
                    int idx = s0 + i;
                    double y = c * y1 - y2; y2 = y1; y1 = y;
                    e *= k;
                    if (idx < 0) continue;
                    if (idx >= buf.Length) break;
                    float at = i < atk ? i / (float)atk : 1f;
                    buf[idx] += (float)(y * e) * a * at;
                }
            }
        }

        /// <summary>Krótki trzask (impuls szumu) – początek uderzenia.</summary>
        public void AddClick(float t0, float amp, float ms = 3f, float cutoff = 6000f)
        {
            AddNoise(t0, ms / 1000f, amp, t => 1f - t / (ms / 1000f), _ => cutoff, 0.7f, Mode.Low);
        }

        /// <summary>Głuche tąpnięcie: sinus opadający w tonie (bęben, uderzenie ciała, wybuch).</summary>
        public void AddThump(float t0, float amp, float fromHz, float toHz, float decay, float sweep = 0.05f)
        {
            float dur = decay * 6f;
            AddTone(t0, dur, amp, t => toHz + (fromHz - toHz) * (float)Math.Exp(-t / sweep), t => AD(t, 0.002f, decay));
        }

        /// <summary>Rozsiane trzaski (żar, kości, lód). density – liczba na sekundę; band – środek pasma.</summary>
        public void AddCrackle(float t0, float dur, float density, float amp, float bandLo, float bandHi, Func<float, float> env = null)
        {
            int count = (int)(density * dur);
            for (int i = 0; i < count; i++)
            {
                float t = Range(0, dur);
                float e = env != null ? env(t) : 1f;
                if (e <= 0.001f) continue;
                float f = Range(bandLo, bandHi);
                float a = amp * e * Range(0.3f, 1f);
                AddNoise(t0 + t, 0.012f, a, x => AD(x, 0.0005f, 0.002f), _ => f, 2.5f, Mode.Band);
            }
        }

        /// <summary>Śpiew chóru „aa”: rozstrojone piły przez formanty samogłoski.</summary>
        public void AddChoir(float t0, float dur, float amp, float freq, float attack, float release, float vibrato = 0.004f, int voices = 3, float vowel = 0f)
        {
            // Formanty między „a” (vowel=0) a „o” (vowel=1).
            float f1 = Lerp(730, 570, vowel), f2 = Lerp(1090, 840, vowel), f3 = Lerp(2440, 2410, vowel);
            var tmp = new float[(int)(dur * sr)];
            for (int v = 0; v < voices; v++)
            {
                float det = 1f + (v - (voices - 1) * 0.5f) * 0.006f;
                double phase = Rand();
                float vibRate = Range(4.5f, 5.8f), vibPh = Range(0, 6.28f);
                for (int i = 0; i < tmp.Length; i++)
                {
                    float t = i / (float)sr;
                    float f = freq * det * (1f + vibrato * (float)Math.Sin(6.2831853f * vibRate * t + vibPh));
                    phase += f / sr;
                    tmp[i] += (float)(2.0 * (phase - Math.Floor(phase)) - 1.0);
                }
            }
            Svf a = new Svf(), b = new Svf(), c = new Svf(), lp = new Svf();
            int s0 = (int)(t0 * sr);
            for (int i = 0; i < tmp.Length; i++)
            {
                int idx = s0 + i;
                if (idx < 0) continue;
                if (idx >= buf.Length) break;
                float x = tmp[i] / voices;
                float y = a.Process(x, f1, 6f, sr, Mode.Band) * 1.0f + b.Process(x, f2, 7f, sr, Mode.Band) * 0.5f + c.Process(x, f3, 8f, sr, Mode.Band) * 0.25f;
                y += lp.Process(x, freq * 2.5f, 0.7f, sr, Mode.Low) * 0.35f;
                buf[idx] += y * Window(i / (float)sr, dur, attack, release) * amp;
            }
        }

        // ------------------------------------------------------------------ Obróbka całego bufora

        public void Saturate(float drive)
        {
            float norm = (float)Math.Tanh(drive);
            for (int i = 0; i < buf.Length; i++) buf[i] = (float)Math.Tanh(buf[i] * drive) / norm;
        }

        public void Filter(float cutoff, Mode mode, float q = 0.7f)
        {
            var f = new Svf();
            for (int i = 0; i < buf.Length; i++) buf[i] = f.Process(buf[i], cutoff, q, sr, mode);
        }

        public void FadeOut(float seconds)
        {
            int n = Math.Min(buf.Length, (int)(seconds * sr));
            for (int i = 0; i < n; i++) buf[buf.Length - 1 - i] *= i / (float)n;
        }

        public void Normalize(float peak = 0.89f)
        {
            float m = 0;
            for (int i = 0; i < buf.Length; i++) m = Math.Max(m, Math.Abs(buf[i]));
            if (m < 1e-6f) return;
            float g = peak / m;
            for (int i = 0; i < buf.Length; i++) buf[i] *= g;
        }

        /// <summary>Pogłos (Freeverb: 8 filtrów grzebieniowych + 4 wszechprzepustowe). wet 0..1.</summary>
        public void Reverb(float room = 0.8f, float damp = 0.4f, float wet = 0.3f, int stereoSpread = 0)
        {
            var y = Reverberate(buf, sr, room, damp, stereoSpread);
            for (int i = 0; i < buf.Length; i++) buf[i] = buf[i] * (1f - wet * 0.5f) + y[i] * wet;
        }

        static readonly int[] CombTuning = { 1116, 1188, 1277, 1356, 1422, 1491, 1557, 1617 };
        static readonly int[] AllpassTuning = { 556, 441, 341, 225 };

        public static float[] Reverberate(float[] x, int sr, float room, float damp, int spread)
        {
            float scale = sr / 44100f;
            var output = new float[x.Length];
            float feedback = 0.7f + 0.28f * room;
            foreach (int tune in CombTuning)
            {
                int len = Math.Max(1, (int)((tune + spread) * scale));
                var line = new float[len];
                int p = 0; float store = 0;
                for (int i = 0; i < x.Length; i++)
                {
                    float o = line[p];
                    store = o * (1f - damp) + store * damp;
                    line[p] = x[i] * 0.015f + store * feedback;
                    output[i] += o;
                    if (++p >= len) p = 0;
                }
            }
            foreach (int tune in AllpassTuning)
            {
                int len = Math.Max(1, (int)((tune + spread) * scale));
                var line = new float[len];
                int p = 0;
                for (int i = 0; i < output.Length; i++)
                {
                    float b = line[p];
                    float o = -output[i] + b;
                    line[p] = output[i] + b * 0.5f;
                    output[i] = o;
                    if (++p >= len) p = 0;
                }
            }
            return output;
        }

        /// <summary>Zawinięcie końca bufora na początek: bufor długości loop+ogon staje się płynną pętlą długości loop.</summary>
        public static float[] FoldLoop(float[] x, int loopLength)
        {
            var y = new float[loopLength];
            for (int i = 0; i < x.Length; i++) y[i % loopLength] += x[i];
            return y;
        }

        // ------------------------------------------------------------------ Gotowe barwy

        /// <summary>Stosunki partii dzwonu kościelnego (hum, prime, tercja mała, kwinta, nominal…).</summary>
        public static readonly float[] BellRatios = { 0.5f, 1f, 1.183f, 1.506f, 2f, 2.514f, 2.662f, 3.011f, 4.166f, 5.433f };
        public static readonly float[] BellAmps = { 0.7f, 0.9f, 0.6f, 0.35f, 0.6f, 0.3f, 0.25f, 0.2f, 0.12f, 0.08f };

        /// <summary>Uderzenie dzwonu o zadanej częstotliwości „prime” i długości wybrzmienia.</summary>
        public void AddBell(float t0, float freq, float amp, float length)
        {
            var decays = new float[BellRatios.Length];
            // Wyższe partie gasną szybciej, ale dość wolno, by dzwon „śpiewał” (nie tylko buczał).
            for (int i = 0; i < decays.Length; i++) decays[i] = length / (0.7f + i * 0.3f);
            decays[0] = length * 1.1f;
            AddModes(t0, freq, BellRatios, BellAmps, decays, amp, 0.0015f, 0.001f);
            AddClick(t0, amp * 0.8f, 5f, 5000f);
        }

        /// <summary>Stosunki drgań sztaby (miecz, ostrze) – nieharmoniczne.</summary>
        public static readonly float[] BarRatios = { 1f, 2.756f, 5.404f, 8.933f, 13.34f };
        /// <summary>Płyta/tarcza – gęstsze, niższe partie.</summary>
        public static readonly float[] PlateRatios = { 1f, 1.59f, 2.14f, 2.3f, 2.65f, 3.16f, 3.5f, 4.15f };
    }
}
