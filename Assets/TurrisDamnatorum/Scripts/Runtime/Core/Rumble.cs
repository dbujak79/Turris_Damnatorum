using UnityEngine;
using UnityEngine.InputSystem;

namespace Turris
{
    /// <summary>
    /// Wibracje pada: krótkie impulsy (niski silnik – ciężar, wysoki – ostrość), które wygasają liniowo.
    /// Kilka impulsów naraz daje maksimum, a nie sumę. Siła z ustawień (0 = wyłączone). W pauzie silniki stoją.
    /// </summary>
    public static class Rumble
    {
        /// <summary>Siła wibracji z ustawień (0–1).</summary>
        public static float Strength = 1f;

        struct Pulse { public float low, high, duration, left; }
        static readonly Pulse[] pulses = new Pulse[8];
        static int count;
        static float lastLow = -1f, lastHigh = -1f;

        /// <summary>Ostatnio wysłane natężenia (dla testów).</summary>
        public static float CurrentLow { get; private set; }
        public static float CurrentHigh { get; private set; }

        public static void Play(float low, float high, float duration)
        {
            if (Strength <= 0f || duration <= 0f) return;
            var p = new Pulse { low = Mathf.Clamp01(low), high = Mathf.Clamp01(high), duration = duration, left = duration };
            if (count < pulses.Length) { pulses[count++] = p; return; }
            // Pełna kolejka – zastąp impuls, któremu zostało najmniej.
            int weakest = 0;
            for (int i = 1; i < count; i++) if (pulses[i].left < pulses[weakest].left) weakest = i;
            pulses[weakest] = p;
        }

        /// <summary>Krok w czasie rzeczywistym (wibracja nie zwalnia razem z grą). paused – zatrzymaj silniki.</summary>
        public static void Tick(float unscaledDt, bool paused)
        {
            float low = 0f, high = 0f;
            for (int i = count - 1; i >= 0; i--)
            {
                var p = pulses[i];
                p.left -= unscaledDt;
                if (p.left <= 0f) { pulses[i] = pulses[--count]; continue; }
                pulses[i] = p;
                float k = p.left / p.duration;
                low = Mathf.Max(low, p.low * k);
                high = Mathf.Max(high, p.high * k);
            }
            if (paused) { low = 0f; high = 0f; }
            low *= Strength; high *= Strength;
            CurrentLow = low; CurrentHigh = high;
            if (Mathf.Abs(low - lastLow) < 0.01f && Mathf.Abs(high - lastHigh) < 0.01f) return;
            lastLow = low; lastHigh = high;
            var pad = Gamepad.current;
            if (pad != null) pad.SetMotorSpeeds(low, high);
        }

        public static void Stop()
        {
            count = 0;
            CurrentLow = CurrentHigh = 0f;
            lastLow = lastHigh = -1f;
            var pad = Gamepad.current;
            if (pad != null) pad.SetMotorSpeeds(0f, 0f);
        }
    }
}
