using UnityEngine;

namespace Turris
{
    public class FxFade : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<FxFade> Active = new System.Collections.Generic.List<FxFade>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        public float life = 0.3f;
        public float grow = 1f;
        float t;
        Vector3 startScale;
        Renderer rend;
        Color baseColor;

        void Start()
        {
            startScale = transform.localScale;
            rend = GetComponent<Renderer>();
            if (rend != null) baseColor = rend.material.color;
        }

        void Update()
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / life);
            transform.localScale = Vector3.Lerp(startScale, new Vector3(startScale.x * grow, startScale.y, startScale.z * grow), k);
            if (rend != null) rend.material.color = Color.Lerp(baseColor, baseColor * 0.2f, k);
            if (t >= life) Destroy(gameObject);
        }
    }
}
