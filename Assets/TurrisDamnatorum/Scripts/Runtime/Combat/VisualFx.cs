using UnityEngine;

namespace Turris
{
    /// <summary>Dawny interfejs prostych efektów – teraz przekierowany do szczegółowych efektów z <see cref="FxLibrary"/>.</summary>
    public static class VisualFx
    {
        public static GameObject Ring(Vector3 pos, float radius, Color color, float life)
        {
            if (!Application.isPlaying) return null;
            FxLibrary.Shockwave(pos, color, radius, radius > 2f);
            return null;
        }

        public static GameObject Flash(Vector3 pos, Color color, float life)
        {
            if (!Application.isPlaying) return null;
            FxLibrary.Flash(pos, color, 0.7f);
            return null;
        }

        /// <summary>Trwały krąg runiczny na ziemi (telegraf ataku obszarowego) – do zniszczenia przez wywołującego.</summary>
        public static GameObject GroundMarker(Vector3 pos, float radius, Color color)
        {
            if (!Application.isPlaying) return null;
            var d = FxLibrary.Decal(pos, FxMaterials.Glyph, color, radius * 2f, radius * 2f, 0f, 35f);
            d.Progress = 0f;
            var inner = FxLibrary.Decal(pos, FxMaterials.Ring, color, radius * 2f, radius * 2f, 0f, 0f);
            inner.Follow(d.transform);
            d.Link(inner.gameObject);
            return d.gameObject;
        }

        public static void SetColor(GameObject go, Color c)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            if (Application.isPlaying) r.material.color = c;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public static void SetEmission(Renderer r, Color c)
        {
            if (r == null || !Application.isPlaying || !r.material.HasProperty("_EmissionColor")) return;
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", c);
        }
    }
}
