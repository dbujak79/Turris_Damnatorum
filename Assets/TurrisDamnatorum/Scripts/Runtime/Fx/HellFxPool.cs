using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Pula wybuchów HU przypisana do sceny. Najstarszy efekt ustępuje nowemu przy pełnej puli.</summary>
    public sealed class HellFxPool : MonoBehaviour
    {
        public const int BurstsPerElement = 6;
        public const int LightBudget = 6;
        static HellFxPool instance;
        readonly Dictionary<HellFxElement, HellParticles[]> bursts = new Dictionary<HellFxElement, HellParticles[]>();
        readonly Dictionary<HellFxElement, int> next = new Dictionary<HellFxElement, int>();
        readonly Light[] lights = new Light[LightBudget];
        readonly float[] lightAge = new float[LightBudget], lightLife = new float[LightBudget], lightPower = new float[LightBudget];
        int nextLight;

        public static HellFxPool Instance
        {
            get
            {
                if (!FxLibrary.Enabled) return null;
                if (instance == null) instance = new GameObject("HellFxPool").AddComponent<HellFxPool>();
                return instance;
            }
        }

        readonly Dictionary<string, HellParticles[]> oneShots = new Dictionary<string, HellParticles[]>();
        readonly Dictionary<string, int> nextOneShot = new Dictionary<string, int>();

        /// <summary>Jednorazowy efekt Turris (błysk uderzenia) z tej samej puli cyklicznej co wybuchy, po kluczu.</summary>
        internal HellParticles OneShot(string key, HellParticleLayer[] layers, Vector3 at, float scale)
        {
            if (!oneShots.TryGetValue(key, out var pool))
            {
                oneShots[key] = pool = new HellParticles[BurstsPerElement]; nextOneShot[key] = 0;
            }
            int index = nextOneShot[key]; nextOneShot[key] = (index + 1) % pool.Length;
            var fx = pool[index];
            if (fx == null)
            {
                fx = HellParticles.Create("HU_" + key, layers, true, transform);
                fx.pooled = true; pool[index] = fx;
            }
            fx.transform.position = at; fx.SetScale(scale); fx.Play();
            return fx;
        }

        public HellParticles Burst(HellFxElement element, Vector3 at, float radius)
        {
            if (!bursts.TryGetValue(element, out var pool))
            {
                bursts[element] = pool = new HellParticles[BurstsPerElement]; next[element] = 0;
            }
            int index = next[element]; next[element] = (index + 1) % pool.Length;
            var fx = pool[index];
            if (fx == null)
            {
                fx = HellParticles.Create("HU_Burst_" + element, HellParticleRecipes.LayersFor(element, 0), true, transform);
                fx.pooled = true; pool[index] = fx;
            }
            fx.transform.position = at; fx.SetBurstRadius(radius); fx.Play();
            return fx;
        }

        public void FlashLight(Vector3 at, Color color, float intensity, float range, float seconds)
        {
            int index = nextLight; nextLight = (index + 1) % LightBudget;
            if (lights[index] == null)
            {
                var go = new GameObject("HU_FlashLight"); go.transform.SetParent(transform, false);
                lights[index] = go.AddComponent<Light>(); lights[index].type = LightType.Point; lights[index].shadows = LightShadows.None;
            }
            intensity = RenderingSetup.PointIntensity(intensity, range);
            var light = lights[index]; light.enabled = true; light.transform.position = at;
            light.color = color; light.range = range; light.intensity = intensity;
            lightAge[index] = 0; lightLife[index] = Mathf.Max(0.01f, seconds); lightPower[index] = intensity;
        }

        void Update()
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] == null || !lights[i].enabled) continue;
                lightAge[i] += Time.deltaTime;
                lights[i].intensity = lightPower[i] * Mathf.Clamp01(1 - lightAge[i] / lightLife[i]);
                if (lightAge[i] >= lightLife[i]) lights[i].enabled = false;
            }
        }

        void OnDestroy() { if (instance == this) instance = null; }
    }
}
