using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Źródło animacji dla podglądu: postać stoi w spoczynku.</summary>
    public class IdleAnimSource : ICharacterAnimSource
    {
        public CharacterAnimState GetAnimState() => new CharacterAnimState();
    }

    /// <summary>
    /// Scena bestiariusza poza areną (głęboko pod światem): portrety kart (render do tekstur, raz na otwarcie) i duży podgląd
    /// wybranego wroga, który obraca się powoli i oddycha w spoczynku. 
    /// </summary>
    public class BestiaryStage : MonoBehaviour
    {
        static readonly Vector3 Origin = new Vector3(0f, -1000f, 0f);
        const int PortraitSize = 192;

        Camera cam;
        readonly Dictionary<(EnemyDefinition, bool), RenderTexture> portraits = new Dictionary<(EnemyDefinition, bool), RenderTexture>();
        readonly Queue<(EnemyDefinition def, bool seen, RenderTexture rt)> pending = new Queue<(EnemyDefinition, bool, RenderTexture)>();
        RenderTexture preview;
        CharacterVisual shown;
        EnemyDefinition shownDef;
        bool shownSeen;
        float spin;

        public static BestiaryStage Create()
        {
            var go = new GameObject("BestiaryStage");
            go.transform.position = Origin;
            var s = go.AddComponent<BestiaryStage>();
            s.Build();
            return s;
        }

        void Build()
        {
            var camGo = new GameObject("BestiaryCam");
            camGo.transform.SetParent(transform, false);
            cam = camGo.AddComponent<Camera>();
            cam.enabled = false;
            RenderingSetup.Prepare(cam);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.015f, 0.02f, 1f);
            cam.fieldOfView = 28f;
            cam.nearClipPlane = 0.05f; cam.farClipPlane = 40f;
            // Światło od przodu-góry i ciepły kontur od tyłu (jak w podglądzie HU).
            foreach (var at in new[] { Origin, PortraitSpot })
            {
                Light(at, new Vector3(35f, 160f, 0f), new Color(1f, 0.95f, 0.9f), 1.3f);
                Light(at, new Vector3(20f, -20f, 0f), new Color(1f, 0.45f, 0.25f), 0.9f);
            }
            preview = new RenderTexture(768, 768, 24) { name = "BestiaryPreview" };
        }

        void Light(Vector3 at, Vector3 euler, Color c, float intensity)
        {
            var go = new GameObject("BestiaryLight");
            go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(euler);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = c; l.range = 12f; l.intensity = RenderingSetup.PointIntensity(intensity * 3f, l.range);
            go.transform.position = at + Quaternion.Euler(euler) * Vector3.back * 4f + Vector3.up * 2f;
        }

        /// <summary>Portrety powstają obok sceny podglądu – obracający się wróg nie wchodzi do kadru kart.</summary>
        static readonly Vector3 PortraitSpot = Origin + new Vector3(60f, 0f, 0f);

        CharacterVisual Spawn(EnemyDefinition def, bool seen, Vector3 at)
        {
            var v = WorldBuilder.CreateEnemyVisual(def, at, transform);
            v.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            v.Init(new IdleAnimSource());
            for (int i = 0; i < 20; i++) v.Tick(1f / 30f);
            if (def.element != Element.None) v.SetTint(Names.ElementColor(def.element), 0.25f);
            return v;
        }


        /// <summary>Kamera tak, by cała postać mieściła się w kadrze (z marginesem).</summary>
        void Frame(CharacterVisual v, float margin, float yaw)
        {
            var b = new Bounds(v.transform.position + Vector3.up, Vector3.one * 0.5f);
            foreach (var r in v.GetComponentsInChildren<Renderer>()) if (!(r is ParticleSystemRenderer)) b.Encapsulate(r.bounds);
            float size = Mathf.Max(b.size.y, b.size.x * 0.9f, 0.6f) * margin;
            float dist = size * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            Vector3 dir = Quaternion.Euler(8f, yaw, 0f) * Vector3.forward;
            cam.transform.position = b.center - dir * dist;
            cam.transform.LookAt(b.center);
        }

        /// <summary>Portret karty: tekstura od razu, rysowana w najbliższej klatce (nie w OnGUI – render kamery psuje GUI).</summary>
        public Texture Portrait(EnemyDefinition def, bool seen)
        {
            if (def == null) return null;
            if (portraits.TryGetValue((def, seen), out var rt) && rt != null) return rt;
            rt = new RenderTexture(PortraitSize, PortraitSize, 24) { name = "Portrait_" + def.id };
            portraits[(def, seen)] = rt;
            pending.Enqueue((def, seen, rt));
            return rt;
        }

        /// <summary>Gotowe portrety (dla testów).</summary>
        public int PendingPortraits => pending.Count;

        void RenderPortrait(EnemyDefinition def, bool seen, RenderTexture rt)
        {
            var v = Spawn(def, seen, PortraitSpot);
            Frame(v, 1.15f, 20f);
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = null;
            DestroyImmediate(v.gameObject);
        }

        /// <summary>Duży podgląd: wybrany wróg obraca się powoli.</summary>
        public Texture Preview(EnemyDefinition def, bool seen)
        {
            if (def != shownDef || seen != shownSeen)
            {
                if (shown != null) Destroy(shown.gameObject);
                shownDef = def; shownSeen = seen;
                shown = def != null ? Spawn(def, seen, Origin) : null;
                spin = 0f;
            }
            return preview;
        }

        void LateUpdate()
        {
            for (int i = 0; i < 4 && pending.Count > 0; i++) { var p = pending.Dequeue(); RenderPortrait(p.def, p.seen, p.rt); }
            if (shown == null) return;
            spin += Time.unscaledDeltaTime * 25f;
            // Animację liczy sama wizualizacja (LateUpdate) – drugi Tick tutaj przyspieszał ją dwukrotnie (nietoperz machał za szybko).
            shown.transform.rotation = Quaternion.Euler(0f, 160f + spin, 0f);
            Frame(shown, 1.25f, 0f);
            cam.targetTexture = preview;
            cam.Render();
            cam.targetTexture = null;
        }

        void OnDestroy()
        {
            foreach (var rt in portraits.Values) if (rt != null) rt.Release();
            if (preview != null) preview.Release();
        }
    }
}
