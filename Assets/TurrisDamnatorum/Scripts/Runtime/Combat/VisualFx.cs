using UnityEngine;

namespace Turris
{
    /// <summary>Proste efekty z prymitywów (placeholdery bez assetów).</summary>
    public static class VisualFx
    {
        public static GameObject Ring(Vector3 pos, float radius, Color color, float life)
        {
            if (!Application.isPlaying) return null; // EditMode (testy) – bez efektów
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Util.DestroySafe(go.GetComponent<Collider>());
            go.name = "FxRing";
            go.transform.position = pos + Vector3.up * 0.05f;
            go.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
            SetColor(go, color);
            var f = go.AddComponent<FxFade>();
            f.life = life;
            return go;
        }

        public static GameObject Flash(Vector3 pos, Color color, float life)
        {
            if (!Application.isPlaying) return null; // EditMode (testy) – bez efektów
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Util.DestroySafe(go.GetComponent<Collider>());
            go.name = "FxFlash";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.6f;
            SetColor(go, color);
            var f = go.AddComponent<FxFade>();
            f.life = life;
            f.grow = 2.5f;
            return go;
        }

        /// <summary>Znacznik obszaru ataku na ziemi (telegraf ataków obszarowych).</summary>
        public static GameObject GroundMarker(Vector3 pos, float radius, Color color)
        {
            if (!Application.isPlaying) return null; // EditMode (testy) – bez efektów
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Util.DestroySafe(go.GetComponent<Collider>());
            go.name = "FxGroundMarker";
            go.transform.position = pos + Vector3.up * 0.03f;
            go.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
            SetColor(go, color);
            return go;
        }

        public static void SetColor(GameObject go, Color c)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            r.material.color = c;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public static void SetEmission(Renderer r, Color c)
        {
            if (r == null || !r.material.HasProperty("_EmissionColor")) return;
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", c);
        }
    }
}
