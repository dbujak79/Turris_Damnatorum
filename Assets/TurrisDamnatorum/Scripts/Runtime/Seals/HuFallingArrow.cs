using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Strzała spadająca z nieba (Deszcz strzał): sam wygląd – obrażenia daje strefa skilla. Spada skośnie, wbija się
    /// w ziemię z iskrą i po chwili znika.
    /// </summary>
    public sealed class HuFallingArrow : MonoBehaviour
    {
        const float FallSeconds = 0.28f, StuckSeconds = 0.7f;
        Vector3 from, to;
        float t;
        bool landed;
        Color color;

        public static void Drop(Vector3 ground, Color color)
        {
            if (!Application.isPlaying || !FxLibrary.Enabled) return;
            var go = new GameObject("HuFallingArrow");
            var a = go.AddComponent<HuFallingArrow>();
            Vector2 tilt = Random.insideUnitCircle * 1.4f;
            a.to = ground + Vector3.up * 0.25f;
            a.from = a.to + new Vector3(tilt.x, 7f, tilt.y);
            a.color = color;
            go.transform.SetPositionAndRotation(a.from, Quaternion.LookRotation(a.to - a.from));
            var model = new GameObject("Arrow").transform;
            model.SetParent(go.transform, false);
            PartBuilder.Add(model, ProcMesh.Box(), Surface.Wood, new Color(0.45f, 0.32f, 0.2f), new Vector3(0, 0, -0.2f), new Vector3(0.018f, 0.018f, 0.7f));
            PartBuilder.Add(model, ProcMesh.Cone(), Surface.Metal, new Color(0.75f, 0.75f, 0.78f), new Vector3(0, 0, 0.17f), new Vector3(0.035f, 0.08f, 0.035f), new Vector3(90f, 0, 0));
            for (int i = 0; i < 3; i++)
                PartBuilder.Add(model, ProcMesh.Box(), Surface.Cloth, new Color(0.8f, 0.2f, 0.15f), new Vector3(0, 0, -0.5f), new Vector3(0.004f, 0.06f, 0.1f), new Vector3(0, 0, i * 60f));
            PartBuilder.BakeAll(model);
            Projectile.AddTrail(go, Color.Lerp(color, Color.white, 0.6f), 0.04f, 0.12f);
        }

        void Update()
        {
            t += Time.deltaTime;
            if (!landed)
            {
                float k = Mathf.Clamp01(t / FallSeconds);
                transform.position = Vector3.Lerp(from, to, k * k);
                if (k >= 1f)
                {
                    landed = true; t = 0f;
                    FxLibrary.Sparks(to, Vector3.up, color);
                    if (Random.value < 0.3f) Sfx.Play(SoundId.ArrowHit, to, 0.35f, 1.1f);
                }
                return;
            }
            if (t >= StuckSeconds) Destroy(gameObject);
        }
    }
}
