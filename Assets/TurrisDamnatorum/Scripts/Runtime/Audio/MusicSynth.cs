using System;

namespace Turris
{
    /// <summary>
    /// Muzyka i tła aren syntezowane w kodzie (D-moll, frygijskie półtony, chór, dzwony, bębny wojenne).
    /// Każdy utwór to pętla bez szwu: zdarzenia zawijają się na początek, a pogłos jest liczony okresowo.
    /// Wynik: stereo z przeplotem (L, R, L, R…). Czysty C# – działa w wątku tła.
    /// </summary>
    public static class MusicSynth
    {
        public const int SampleRate = 24000;

        public static float Hz(int midi) => 440f * (float)Math.Pow(2, (midi - 69) / 12.0);

        /// <summary>Ścieżka stereo o długości pętli; zdarzenia wychodzące poza koniec lądują na początku.</summary>
        sealed class Track
        {
            public readonly int sr, loop;
            public readonly float[] L, R;
            int seed = 1;

            public Track(float seconds, int sampleRate)
            {
                sr = sampleRate;
                loop = (int)(seconds * sampleRate);
                L = new float[loop]; R = new float[loop];
            }

            public float Seconds => loop / (float)sr;
            public Dsp Voice(float dur) => new Dsp(dur, sr, seed++ * 7717);

            /// <summary>Wmiksowuje głos od chwili t0 z panoramą (−1 lewo … 1 prawo).</summary>
            public void Mix(Dsp v, float t0, float gain, float pan = 0f)
            {
                float gl = gain * (float)Math.Cos((pan + 1) * Math.PI / 4), gr = gain * (float)Math.Sin((pan + 1) * Math.PI / 4);
                int s0 = (int)(t0 * sr);
                for (int i = 0; i < v.buf.Length; i++)
                {
                    int idx = ((s0 + i) % loop + loop) % loop;
                    L[idx] += v.buf[i] * gl; R[idx] += v.buf[i] * gr;
                }
            }

            /// <summary>Okresowy pogłos (różne strojenie kanałów daje szerokość).</summary>
            public void Reverb(float room, float damp, float wet, float tail = 5f)
            {
                Apply(L, 0); Apply(R, 23);
                void Apply(float[] ch, int spread)
                {
                    int ext = loop + (int)(tail * sr);
                    var x = new float[ext];
                    Array.Copy(ch, x, loop);
                    var y = Dsp.FoldLoop(Dsp.Reverberate(x, sr, room, damp, spread), loop);
                    for (int i = 0; i < loop; i++) ch[i] = ch[i] * (1f - wet * 0.4f) + y[i] * wet;
                }
            }

            public float[] Interleaved(float peak)
            {
                float m = 1e-6f;
                for (int i = 0; i < loop; i++) m = Math.Max(m, Math.Max(Math.Abs(L[i]), Math.Abs(R[i])));
                float g = peak / m;
                var o = new float[loop * 2];
                for (int i = 0; i < loop; i++) { o[2 * i] = (float)Math.Tanh(L[i] * g * 1.1f) / 1.1f; o[2 * i + 1] = (float)Math.Tanh(R[i] * g * 1.1f) / 1.1f; }
                return o;
            }
        }

        public static float[] Generate(MusicId id, int sr = SampleRate)
        {
            switch (id)
            {
                case MusicId.Menu: return Menu(sr);
                case MusicId.Combat: return Combat(sr);
                case MusicId.Boss: return Boss(sr);
                case MusicId.AmbCourtyard: return Courtyard(sr);
                case MusicId.AmbCrypt: return Crypt(sr);
                case MusicId.AmbSummit: return Summit(sr);
            }
            return new float[2];
        }

        // ================================================================== Instrumenty

        /// <summary>Warstwa ciągła z nakładających się segmentów sin² (suma okien = 1, więc bez pulsowania).</summary>
        static void Continuous(Track tr, float segment, Func<Dsp, float, Dsp> render, float gain, float pan = 0f)
        {
            float hop = segment / 2f;
            int count = (int)Math.Round(tr.Seconds / hop);
            for (int k = 0; k < count; k++)
            {
                var v = render(tr.Voice(segment), k * hop);
                for (int i = 0; i < v.buf.Length; i++) v.buf[i] *= Dsp.Window(i / (float)v.sr, segment, hop, hop);
                tr.Mix(v, k * hop, gain, pan);
            }
        }

        static void Wind(Track tr, float lo, float hi, float gain, float gust = 0.5f, float q = 1.2f)
        {
            Continuous(tr, 8f, (v, start) =>
            {
                float a = v.Range(0.1f, 0.35f), b = v.Range(0.4f, 0.9f), ph = v.Range(0, 6.28f);
                v.AddNoise(0, 8f, 1f, t => 1f - gust + gust * (0.5f + 0.5f * (float)Math.Sin(t * a * 6.28f + ph) * (float)Math.Sin(t * b + ph)),
                    t => Dsp.ExpLerp(lo, hi, 0.5f + 0.5f * (float)Math.Sin(t * a * 4f + ph * 2f)), q, Dsp.Mode.Band);
                return v;
            }, gain, 0f);
        }

        static void Choir(Track tr, float t0, float dur, int[] notes, float gain, float vowel = 0.3f, float attack = 2f, float release = 2.5f)
        {
            for (int i = 0; i < notes.Length; i++)
            {
                var v = tr.Voice(dur);
                v.AddChoir(0, dur, 1f, Hz(notes[i]), attack, release, 0.005f, 3, vowel);
                tr.Mix(v, t0, gain, (i - (notes.Length - 1) * 0.5f) * 0.35f);
            }
        }

        static void Organ(Track tr, float t0, float dur, int note, float gain, float cutoff = 500f)
        {
            var v = tr.Voice(dur);
            float f = Hz(note);
            v.AddTone(0, dur, 0.5f, t => f * 1.002f, t => Dsp.Window(t, dur, dur * 0.25f, dur * 0.35f), Dsp.Wave.Saw, cutoff);
            v.AddTone(0, dur, 0.5f, t => f * 0.998f, t => Dsp.Window(t, dur, dur * 0.25f, dur * 0.35f), Dsp.Wave.Saw, cutoff);
            v.AddTone(0, dur, 0.6f, t => f * 0.5f, t => Dsp.Window(t, dur, dur * 0.25f, dur * 0.35f));
            tr.Mix(v, t0, gain);
        }

        static void Taiko(Track tr, float t0, float gain, bool big = false)
        {
            var v = tr.Voice(big ? 2f : 1f);
            v.AddClick(0, 0.4f, 3f, 2500f);
            v.AddThump(0, 1f, big ? 75f : 95f, big ? 38f : 55f, big ? 0.35f : 0.2f, 0.04f);
            v.AddNoise(0, 0.2f, 0.35f, t => Dsp.AD(t, 0.002f, 0.05f), _ => 500f, 0.7f, Dsp.Mode.Low);
            tr.Mix(v, t0, gain, v.Range(-0.15f, 0.15f));
        }

        static void Snare(Track tr, float t0, float gain)
        {
            // Werbel wojskowy o skórzanym brzmieniu (bez metalu – bardziej „obóz” niż perkusja).
            var v = tr.Voice(0.5f);
            v.AddThump(0, 0.7f, 220f, 150f, 0.05f);
            v.AddNoise(0, 0.3f, 0.8f, t => Dsp.AD(t, 0.002f, 0.07f), _ => 1800f, 1.2f, Dsp.Mode.Band);
            tr.Mix(v, t0, gain, 0.2f);
        }

        static void Tick(Track tr, float t0, float gain)
        {
            var v = tr.Voice(0.15f);
            v.AddThump(0, 0.6f, 400f, 260f, 0.025f);
            v.AddNoise(0, 0.05f, 0.4f, t => Dsp.AD(t, 0.001f, 0.015f), _ => 3000f, 1f, Dsp.Mode.Band);
            tr.Mix(v, t0, gain, -0.3f);
        }

        static void Bass(Track tr, float t0, float dur, int note, float gain)
        {
            // Niskie smyczki grane staccato: dwie piły przez filtr, ostry atak.
            var v = tr.Voice(dur + 0.3f);
            float f = Hz(note);
            Func<float, float> env = t => Dsp.AD(t, 0.01f, dur * 0.6f);
            v.AddTone(0, dur + 0.3f, 0.5f, t => f * 1.003f, env, Dsp.Wave.Saw, 700f);
            v.AddTone(0, dur + 0.3f, 0.5f, t => f * 0.997f, env, Dsp.Wave.Saw, 700f);
            tr.Mix(v, t0, gain);
        }

        static void Brass(Track tr, float t0, float dur, int[] notes, float gain)
        {
            // Dęciaki: zespół pił, filtr otwierany obwiednią (zadęcie).
            foreach (var n in notes)
            {
                var v = tr.Voice(dur + 0.5f);
                float f = Hz(n);
                for (int k = 0; k < 3; k++)
                {
                    float det = 1f + (k - 1) * 0.004f;
                    v.AddTone(0, dur + 0.5f, 0.4f, t => f * det, t => Dsp.Window(t, dur + 0.5f, 0.04f, 0.5f), Dsp.Wave.Saw);
                }
                var flt = new Dsp.Svf();
                for (int i = 0; i < v.buf.Length; i++)
                {
                    float t = i / (float)v.sr;
                    float c = 300f + 1600f * (float)Math.Exp(-t / 0.25f) + 300f;
                    v.buf[i] = flt.Process(v.buf[i], c, 1.1f, v.sr, Dsp.Mode.Low);
                }
                v.Saturate(1.4f);
                tr.Mix(v, t0, gain);
            }
        }

        static void Bell(Track tr, float t0, int note, float gain, float length = 5f, float pan = 0f)
        {
            var v = tr.Voice(length + 1f);
            v.AddBell(0, Hz(note), 1f, length);
            tr.Mix(v, t0, gain, pan);
        }

        // ================================================================== Menu / kapliczka

        static float[] Menu(int sr)
        {
            var tr = new Track(32f, sr);
            // Pedał D w organach przez cały utwór.
            Continuous(tr, 16f, (v, s) =>
            {
                float f = Hz(38);
                v.AddTone(0, 16f, 0.5f, t => f * 1.002f, t => 1f, Dsp.Wave.Saw, 350f);
                v.AddTone(0, 16f, 0.5f, t => f * 0.998f, t => 1f, Dsp.Wave.Saw, 350f);
                v.AddTone(0, 16f, 0.6f, t => f * 0.5f, t => 1f);
                return v;
            }, 0.25f);
            // Chór: Dm – B♭ – Gm – A (dominanta harmonicznego mola).
            int[][] chords =
            {
                new[] { 50, 53, 57, 62 },
                new[] { 46, 50, 53, 58 },
                new[] { 43, 50, 55, 58 },
                new[] { 45, 49, 52, 57 },
            };
            for (int c = 0; c < 4; c++) Choir(tr, c * 8f - 0.5f, 9.5f, chords[c], 0.22f, 0.4f, 2.5f, 3f);
            // Odległe dzwony.
            Bell(tr, 0.2f, 50, 0.18f, 6f, -0.4f);
            Bell(tr, 16.2f, 45, 0.15f, 6f, 0.4f);
            Wind(tr, 300f, 900f, 0.05f);
            tr.Reverb(0.92f, 0.45f, 0.5f);
            return tr.Interleaved(0.7f);
        }

        // ================================================================== Walka

        static float[] Combat(int sr)
        {
            const float bpm = 90f;
            float beat = 60f / bpm, e8 = beat / 2f;
            var tr = new Track(48 * beat, sr); // 12 taktów
            int[] roots = { 38, 38, 39, 39, 38, 38, 36, 36, 34, 34, 33, 33 }; // D D E♭ E♭ D D C C B♭ B♭ A A
            int[] ost = { 0, 0, 12, 0, 0, 12, 0, 7 };
            int[] taiko = { 1, 0, 0, 1, 0, 0, 1, 0 };
            int[] fill = { 1, 0, 1, 0, 1, 1, 1, 1 };
            for (int bar = 0; bar < 12; bar++)
            {
                float b0 = bar * 4 * beat;
                var pat = bar % 4 == 3 ? fill : taiko;
                for (int k = 0; k < 8; k++)
                {
                    float t = b0 + k * e8;
                    if (pat[k] == 1) Taiko(tr, t, k == 0 ? 0.9f : 0.6f);
                    Tick(tr, t, k % 2 == 0 ? 0.12f : 0.07f);
                    Bass(tr, t, e8 * 0.9f, roots[bar] + ost[k] - 12 + 12, k == 0 ? 0.3f : 0.2f);
                }
                Snare(tr, b0 + beat, 0.22f);
                Snare(tr, b0 + 3 * beat, 0.22f);
            }
            // Chór co dwa takty: Dm, E♭, Dm, C, B♭, A.
            int[][] ch =
            {
                new[] { 50, 53, 57 }, new[] { 51, 55, 58 }, new[] { 50, 53, 57 },
                new[] { 48, 52, 55 }, new[] { 46, 50, 53 }, new[] { 45, 49, 52 },
            };
            for (int c = 0; c < 6; c++) Choir(tr, c * 8 * beat - 0.3f, 8 * beat + 0.8f, ch[c], 0.12f, 0.2f, 1.2f, 1.5f);
            for (int p = 0; p < 3; p++) Brass(tr, p * 16 * beat, beat * 1.5f, new[] { roots[p * 4] - 12 + 12, roots[p * 4] + 7 }, 0.18f);
            tr.Reverb(0.8f, 0.5f, 0.3f, 4f);
            return tr.Interleaved(0.75f);
        }

        // ================================================================== Boss

        static float[] Boss(int sr)
        {
            const float bpm = 100f;
            float beat = 60f / bpm, e8 = beat / 2f;
            var tr = new Track(64 * beat, sr); // 16 taktów
            int[][] ch =
            {
                new[] { 50, 53, 57, 62 }, new[] { 46, 50, 53, 58 }, new[] { 43, 50, 55, 58 }, new[] { 45, 49, 52, 57 },
                new[] { 50, 53, 57, 62 }, new[] { 44, 48, 51, 56 }, new[] { 43, 50, 55, 58 }, new[] { 45, 49, 52, 57 },
            }; // Dm B♭ Gm A Dm A♭(tryton) Gm A
            int[] taiko = { 1, 0, 1, 0, 1, 0, 1, 1 };
            for (int bar = 0; bar < 16; bar++)
            {
                float b0 = bar * 4 * beat;
                int root = ch[bar / 2][0] - 12;
                for (int k = 0; k < 8; k++)
                {
                    float t = b0 + k * e8;
                    if (taiko[k] == 1) Taiko(tr, t, k == 0 ? 1f : 0.65f);
                    Tick(tr, t, 0.1f);
                    Tick(tr, t + e8 / 2f, 0.05f);
                    Bass(tr, t, e8 * 0.9f, root + (k % 4 == 3 ? 12 : 0), k == 0 ? 0.32f : 0.22f);
                }
                if (bar % 4 == 0) Taiko(tr, b0, 1f, true);
                Snare(tr, b0 + beat, 0.3f);
                Snare(tr, b0 + 3 * beat, 0.3f);
                Brass(tr, b0, beat * 0.8f, new[] { root, root + 7 }, 0.2f);
                Brass(tr, b0 + 1.5f * beat, beat * 0.5f, new[] { root, root + 7 }, 0.15f);
                if (bar % 4 == 0) Bell(tr, b0, 38, 0.3f, 4f);
            }
            for (int c = 0; c < 8; c++) Choir(tr, c * 8 * beat - 0.2f, 8 * beat + 0.6f, ch[c], 0.2f, 0.1f, 0.6f, 1.2f);
            Continuous(tr, 16f * beat, (v, s) => { v.AddTone(0, v.Duration, 0.8f, t => Hz(26), t => 1f); return v; }, 0.2f);
            tr.Reverb(0.85f, 0.45f, 0.3f, 4f);
            return tr.Interleaved(0.8f);
        }

        // ================================================================== Tła aren

        static float[] Courtyard(int sr)
        {
            var tr = new Track(32f, sr);
            Wind(tr, 350f, 1000f, 0.5f, 0.6f);
            // Trzask pochodni i koksowników.
            Continuous(tr, 4f, (v, s) => { v.AddCrackle(0, 4f, 10f, 0.4f, 1200, 4000); v.AddNoise(0, 4f, 0.05f, t => 1f, _ => 600f, 0.7f, Dsp.Mode.Low); return v; }, 0.25f, -0.3f);
            // Daleki dzwon i skrzypienie.
            Bell(tr, 9f, 45, 0.12f, 6f, 0.6f);
            var creak = tr.Voice(1.2f);
            creak.AddTone(0, 1.2f, 0.3f, t => 180f + 40f * (float)Math.Sin(t * 5f) + 20f * (float)Math.Sin(t * 23f), t => Dsp.Window(t, 1.2f, 0.3f, 0.5f), Dsp.Wave.Saw, 900f);
            tr.Mix(creak, 21f, 0.08f, -0.6f);
            tr.Reverb(0.75f, 0.5f, 0.25f, 4f);
            return tr.Interleaved(0.45f);
        }

        static float[] Crypt(int sr)
        {
            var tr = new Track(32f, sr);
            // Niski, dudniący pomruk grobowca (dwa bliskie tony – wolne dudnienie).
            Continuous(tr, 16f, (v, s) =>
            {
                v.AddTone(0, 16f, 0.5f, t => 43.65f, t => 1f);
                v.AddTone(0, 16f, 0.4f, t => 44.2f, t => 1f);
                v.AddTone(0, 16f, 0.2f, t => 87.3f, t => 1f, Dsp.Wave.Saw, 200f);
                return v;
            }, 0.35f);
            // Jęk wiatru w korytarzach – rezonansowy szum sunący w górę i w dół.
            Continuous(tr, 8f, (v, s) =>
            {
                float ph = v.Range(0, 6.28f);
                v.AddNoise(0, 8f, 1f, t => 0.5f + 0.5f * (float)Math.Sin(t * 0.7f + ph), t => 260f + 140f * (float)Math.Sin(t * 0.5f + ph), 7f, Dsp.Mode.Band);
                return v;
            }, 0.18f);
            // Kapiąca woda.
            var rnd = tr.Voice(0.01f);
            for (int i = 0; i < 12; i++)
            {
                var drip = tr.Voice(0.3f);
                float f = rnd.Range(1200, 2200);
                drip.AddTone(0, 0.08f, 1f, t => f * (1f + t * 10f), t => Dsp.AD(t, 0.001f, 0.02f));
                tr.Mix(drip, rnd.Range(0, 32f), rnd.Range(0.06f, 0.14f), rnd.Range(-0.8f, 0.8f));
            }
            // Łańcuch gdzieś w ciemności.
            var chain = tr.Voice(1f);
            for (int i = 0; i < 12; i++) chain.AddModes(chain.Range(0, 0.6f), chain.Range(1500, 3200), new[] { 1f, 1.7f }, new[] { 1f, 0.4f }, new[] { 0.05f, 0.03f }, 0.2f);
            tr.Mix(chain, 19f, 0.12f, 0.7f);
            tr.Reverb(0.95f, 0.35f, 0.55f, 6f);
            return tr.Interleaved(0.5f);
        }

        static float[] Summit(int sr)
        {
            var tr = new Track(32f, sr);
            // Porywisty wiatr: niski ryk i wysoki świst.
            Wind(tr, 180f, 600f, 0.7f, 0.7f, 0.8f);
            Continuous(tr, 8f, (v, s) =>
            {
                float ph = v.Range(0, 6.28f);
                v.AddNoise(0, 8f, 1f, t => Math.Max(0f, (float)Math.Sin(t * 0.6f + ph)), t => 900f + 500f * (float)Math.Sin(t * 0.9f + ph), 9f, Dsp.Mode.Band);
                return v;
            }, 0.12f, 0.3f);
            // Odległe grzmoty.
            foreach (var t0 in new[] { 7f, 23f })
            {
                var th = tr.Voice(4f);
                th.AddNoise(0, 3.8f, 1f, t => Dsp.AD(t, 0.3f, 1f) * (0.6f + 0.4f * (float)Math.Sin(t * 5f)), _ => 140f, 0.8f, Dsp.Mode.Low);
                tr.Mix(th, t0, 0.5f, t0 < 10 ? -0.5f : 0.5f);
            }
            tr.Reverb(0.85f, 0.5f, 0.25f, 5f);
            return tr.Interleaved(0.5f);
        }
    }
}
