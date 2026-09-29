using UnityEngine;

namespace Turris
{
    /// <summary>Płaski efekt na ziemi (krąg runiczny, fala). life &lt;= 0 = trwały, aż do zniszczenia.</summary>
    public class FxDecal : MonoBehaviour
    {
        static MaterialPropertyBlock block;
        static readonly int TintId = Shader.PropertyToID("_TintColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        Color color;
        float from, to, life, spin, fade, t;
        Transform follow;
        Renderer rend;
        public float Progress { get; set; } = -1f; // opcjonalne „wypełnienie” (telegraf): pulsowanie rośnie z postępem

        public void Init(Color c, float fromSize, float toSize, float lifeTime, float spinSpeed, float fadeFraction)
        {
            color = c; from = fromSize; to = toSize; life = lifeTime; spin = spinSpeed; fade = Mathf.Clamp(fadeFraction, 0.01f, 1f);
            rend = GetComponent<Renderer>();
            Apply(0f);
        }

        public FxDecal Follow(Transform t) { follow = t; return this; }

        readonly System.Collections.Generic.List<GameObject> linked = new System.Collections.Generic.List<GameObject>();
        /// <summary>Obiekt niszczony razem z tym kręgiem.</summary>
        public void Link(GameObject other) { if (other != null) linked.Add(other); }
        void OnDestroy() { foreach (var o in linked) if (o != null) Destroy(o); }

        public void SetColor(Color c) { color = c; }

        void Update()
        {
            t += Time.deltaTime;
            if (follow != null) transform.position = new Vector3(follow.position.x, follow.GetComponent<FxDecal>() != null ? follow.position.y + 0.005f : 0.03f, follow.position.z);
            transform.Rotate(0f, spin * Time.deltaTime, 0f, Space.World);
            if (life > 0f && t >= life) { Destroy(gameObject); return; }
            Apply(life > 0f ? t / life : 0f);
        }

        void Apply(float k)
        {
            float size = life > 0f ? Mathf.Lerp(from, to, 1f - (1f - k) * (1f - k)) : from;
            transform.localScale = new Vector3(size, 1f, size);
            float a = 1f;
            if (life > 0f)
            {
                float fadeStart = 1f - fade;
                a = k < 0.08f ? k / 0.08f : (k > fadeStart ? Mathf.Clamp01((1f - k) / fade) : 1f);
            }
            if (Progress >= 0f) a *= 0.55f + 0.45f * Mathf.Sin(Time.time * Mathf.Lerp(6f, 28f, Progress)) * Progress + 0.3f * Progress;
            if (block == null) block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);
            Color c = color * a;
            block.SetColor(TintId, new Color(c.r, c.g, c.b, 1f));
            block.SetColor(ColorId, c);
            rend.SetPropertyBlock(block);
        }
    }

    /// <summary>Światło gasnące w czasie.</summary>
    public class FxLight : MonoBehaviour
    {
        public float life = 0.3f;
        public float startIntensity = 2f;
        float t;
        Light l;
        void Awake() => l = GetComponent<Light>();
        void Update()
        {
            t += Time.deltaTime;
            if (l != null) l.intensity = startIntensity * Mathf.Clamp01(1f - t / life);
            if (t >= life) Destroy(gameObject);
        }
    }

    /// <summary>Migotanie płomienia.</summary>
    public class FxFlicker : MonoBehaviour
    {
        public float baseIntensity = 1.5f;
        Light l;
        float seed;
        void Awake() { l = GetComponent<Light>(); seed = Random.value * 100f; }
        void Update()
        {
            if (l == null) return;
            float n = Mathf.PerlinNoise(seed, Time.time * 6f);
            l.intensity = baseIntensity * Mathf.Lerp(0.7f, 1.2f, n);
        }
    }
}
