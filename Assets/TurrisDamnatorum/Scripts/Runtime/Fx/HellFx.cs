using UnityEngine;

namespace Turris
{
    /// <summary>Efekty HU w Unity. Ta warstwa nigdy nie nalicza obrażeń ani nie modyfikuje danych umiejętności.</summary>
    public static class HellFx
    {
        public static HellFxElement For(Element element) => element switch
        {
            Element.Fire => HellFxElement.Fire, Element.Frost => HellFxElement.Ice,
            Element.Lightning => HellFxElement.Air, Element.Earth => HellFxElement.Earth, _ => HellFxElement.Physical
        };

        /// <summary>Adapter starych wywołań operujących kolorem. Nowe miejsca powinny przekazywać żywioł wprost.</summary>
        public static HellFxElement FromColor(Color c)
        {
            if (c.r > 0.65f && c.g < 0.35f) return HellFxElement.Enemy;
            if (c.r > 0.8f && c.g < 0.7f && c.b < 0.4f) return HellFxElement.Fire;
            if (c.b > c.r * 1.25f && c.g > c.r * 1.15f) return HellFxElement.Ice;
            if (c.b > c.g * 1.15f) return HellFxElement.Soul;
            if (c.r > c.b * 1.4f && c.g > c.b * 1.25f) return HellFxElement.Earth;
            return HellFxElement.Physical;
        }

        public static HellParticles Burst(Vector3 at, HellFxElement element, float radius = 1)
        {
            if (!FxLibrary.Enabled) return null;
            var fx = HellFxPool.Instance.Burst(element, at, Mathf.Max(0.1f, radius));
            if (element == HellFxElement.Fire) FireAccent(at, radius);
            return fx;
        }

        static HellParticleLayer[] fireFlash;
        static readonly RaycastHit[] groundHits = new RaycastHit[8];

        /// <summary>
        /// Warstwy Turris nad wybuchem ognia HU: krótki błysk HDR w punkcie uderzenia (bloom) i ślad na ziemi –
        /// zwęglenie ~2 promienie, żar ~1,2 promienia, który stygnie szybciej (wzorzec scorch + hot core).
        /// </summary>
        static void FireAccent(Vector3 at, float radius)
        {
            fireFlash ??= new[]
            {
                new HellParticleLayer("glow", new[] { new Color(1, 0.75f, 0.4f, 1), new Color(1, 0.5f, 0.15f, 0.7f), new Color(1, 0.25f, 0.04f, 0) },
                    true, 1.6f, 2.0f, 0, 0, 0, Vector3.zero, 0.16f, 1, EmitRadius: 0.01f, Look: "glow_hot"),
            };
            var flash = HellParticles.Create("HU_FireFlash", fireFlash, true);
            flash.transform.position = at; flash.SetScale(Mathf.Clamp(radius, 0.4f, 2.5f)); flash.Play();
            if (radius < 0.6f) return;
            // Najniższe trafienie w dół to podłoże (wyżej mogą być kolidery postaci w miejscu wybuchu).
            int count = Physics.RaycastNonAlloc(at + Vector3.up * 0.2f, Vector3.down, groundHits, 3f, ~0, QueryTriggerInteraction.Ignore);
            if (count == 0) return;
            Vector3 ground = groundHits[0].point;
            for (int i = 1; i < count; i++) if (groundHits[i].point.y < ground.y) ground = groundHits[i].point;
            float scorch = radius * 2f;
            FxLibrary.Decal(ground, HellVfxMaterials.Scorch(false), Color.white, scorch * 0.85f, scorch, 6f, 0, 0.45f);
            FxLibrary.Decal(ground + Vector3.up * 0.002f, HellVfxMaterials.Scorch(true), Color.white, scorch * 0.6f, scorch * 0.6f, 1.6f, 0, 0.9f);
        }

        public static HellParticles Ambient(Transform parent, Vector3 offset, HellFxElement element, float radius, float duration = 0)
        {
            if (!FxLibrary.Enabled) return null;
            var fx = HellParticles.Create("HU_Ambient_" + element, HellParticleRecipes.LayersFor(element, 1), false, parent);
            fx.transform.localPosition = offset; fx.SetDisc(radius); fx.Play(duration); return fx;
        }

        public static HellParticles Trail(Transform parent, HellFxElement element, float size)
        {
            if (!FxLibrary.Enabled || parent == null) return null;
            var fx = HellParticles.Create("HU_Trail_" + element, HellParticleRecipes.LayersFor(element, 2), false, parent);
            fx.SetScale(Mathf.Clamp(size / 0.3f, 0.3f, 2)); fx.Play(); return fx;
        }

        public static HellParticles Gather(Transform hand, HellFxElement family, float duration)
        {
            if (!FxLibrary.Enabled || hand == null) return null;
            var fx = HellParticles.Create("HU_Gather_" + family, HellParticleRecipes.LayersFor(family, 2), false, hand);
            fx.SetScale(0.45f); fx.SetAmount(0.35f); fx.Play(Mathf.Max(0.05f, duration)); return fx;
        }

        public static HellParticles Cone(Vector3 at, Vector3 forward, HellFxElement family, float range, float angle)
        {
            if (!FxLibrary.Enabled) return null;
            var fx = HellParticles.Create("HU_Cone_" + family, HellParticleRecipes.LayersFor(family, 0), true);
            fx.transform.position = at; fx.SetBurstRadius(1.2f); fx.SetAmount(0.5f);
            fx.PointAlong(forward, angle * 0.5f, range * 1.5f); fx.Play(); return fx;
        }

        public static void Shockwave(Vector3 at, Element element, Color color, float radius, bool dust = false)
            => Shockwave(at, For(element), color, radius, dust);

        public static void Shockwave(Vector3 at, HellFxElement family, Color color, float radius, bool dust = false)
        {
            if (!FxLibrary.Enabled) return;
            Burst(at + Vector3.up * 0.1f, family, radius);
            FxLibrary.Decal(at, HellVfxMaterials.Ground(family), color, radius * 2, radius * 2, 0.55f, 0, 0.6f);
            FxLibrary.Decal(at, FxMaterials.Ring, color, 0.3f, radius * 2, 0.4f);
            if (dust) SlamDust(at, radius);
            HellFxPool.Instance.FlashLight(at + Vector3.up, color, 3.5f, radius * 2, 0.3f);
        }

        public static HellParticles Flame(Transform parent, Vector3 offset, float size)
        {
            if (!FxLibrary.Enabled) return null;
            var fx = HellParticles.Create("HU_HandFlame", HellParticleRecipes.HandFlame(size), false, parent);
            fx.transform.localPosition = offset; fx.Play(); return fx;
        }

        public static void Impact(Vector3 at, HellFxElement element, Color color, float radius)
        {
            if (!FxLibrary.Enabled) return;
            Burst(at, element, radius);
            HellFxPool.Instance.FlashLight(at, color, 3f, Mathf.Max(2, radius * 2), 0.22f);
        }

        public static GameObject Ground(Vector3 center, float radius, HellFxElement element, Color color, float life)
        {
            if (!FxLibrary.Enabled) return null;
            var go = new GameObject("HU_Ground_" + element); go.transform.position = center;
            var fx = Ambient(go.transform, Vector3.up * 0.08f, element, radius, life);
            var decal = FxLibrary.Decal(center, HellVfxMaterials.Ground(element), color, radius * 2, radius * 2, life, 0, 0.15f);
            if (decal != null) decal.transform.SetParent(go.transform, true);
            // Cząsteczki odczepiają się i dogasają; krąg znika wraz z końcem obrażeń.
            Object.Destroy(go, life + fx.MaxLifetime + 0.2f);
            return go;
        }

        public static HellParticles Wave(Transform parent, HellFxElement family, float halfWidth, float duration)
        {
            var fx = Ambient(parent, Vector3.up * 0.3f, family, 1, duration);
            if (fx == null) return null;
            fx.SetBox(halfWidth, 0.12f); return fx;
        }

        public static HellParticles Shadow(Transform parent, Vector3 at, bool trail = false, float duration = 0, float scale = 1)
        {
            if (!FxLibrary.Enabled) return null;
            var smoke = new Color(0.24f, 0.14f, 0.32f);
            var fx = HellParticles.Create("HU_Shadow", new[]
            {
                new HellParticleLayer("smoke", Fade(smoke, 0.85f), false, trail ? 0.5f : 1.5f, trail ? 0.9f : 2.5f,
                    trail ? 0.1f : 1.2f, trail ? 0.4f : 2.8f, 180, new Vector3(0,0.4f,0), 0.8f, trail ? 18 : 30, Grow: true, Damping: 5),
                new HellParticleLayer("spark", new[] { new Color(0.62f,0.3f,1), new Color(0.62f,0.3f,1,0) }, true,
                    0.05f,0.12f,2,4.5f,180,Vector3.zero,0.4f,14,AlignToVelocity:true,Damping:8)
            }, !trail, parent);
            fx.transform.position = at + Vector3.up * 0.6f; fx.SetScale(scale); fx.Play(duration); return fx;
        }

        /// <summary>BlinkFx.PlaySparks HU: zbieganie iskier przy zniknięciu i rozprysk przy powrocie.</summary>
        public static void BlinkSparks(Vector3 at, Color color, bool inward)
        {
            if (!FxLibrary.Enabled) return;
            Color hot = Color.Lerp(color, Color.white, 0.25f);
            var ramp = inward ? new[] { new Color(hot.r,hot.g,hot.b,0), hot }
                : new[] { hot, new Color(hot.r,hot.g,hot.b,0) };
            var fx = HellParticles.Create("HU_Blink", new[]
            {
                new HellParticleLayer("spark",ramp,true,0.05f,0.09f,inward ? 0 : 3,inward ? 0.5f : 6,180,
                    inward ? new Vector3(0,3,0) : Vector3.zero,inward ? 0.2f : 0.3f,44,
                    AlignToVelocity:true,Damping:inward ? 0 : 8,EmitRadius:inward ? 1 : 0.25f)
            },true);
            fx.transform.position=at+Vector3.up;
            if (inward) fx.SetOutward(-6,-4);
            fx.Play();
        }

        public static void Crackle(Vector3 at)
        {
            if (!FxLibrary.Enabled) return;
            var fx = HellParticles.Create("HU_Crackle", new[]
            {
                new HellParticleLayer("electric_sparks", new[] { Color.white, Color.white, new Color(1,1,1,0) }, true,
                    0.45f,1,0.3f,1.2f,180,Vector3.zero,0.2f,7,Atlas:new Vector2Int(4,4))
            }, true);
            fx.transform.position = at; fx.Play();
        }

        public static HellParticles ElectricAura(Transform parent, Vector3 offset)
        {
            if (!FxLibrary.Enabled) return null;
            var fx = HellParticles.Create("HU_ElectricAura", new[]
            {
                new HellParticleLayer("electric_sparks", new[] { Color.white, new Color(1,1,1,0) }, true,
                    0.22f,0.55f,0.05f,0.2f,180,Vector3.zero,0.18f,3,EmitRadius:0.45f,Atlas:new Vector2Int(4,4))
            }, false, parent);
            fx.transform.localPosition = offset; fx.Play(); return fx;
        }

        public static void Blood(Vector3 at, Vector3 direction, float amount)
        {
            if (!FxLibrary.Enabled) return;
            var fx = HellParticles.Create("HU_Blood", new[]
            {
                new HellParticleLayer("blood_splash", Fade(new Color(0.55f,0.1f,0.08f),1), false,0.35f,0.7f,1.5f,4.5f,35,
                    new Vector3(0,-16,0),0.5f,Mathf.Clamp(Mathf.RoundToInt(12 * amount),1,48),Atlas:new Vector2Int(4,4)),
                new HellParticleLayer("blood_droplets", Fade(new Color(0.5f,0.08f,0.06f),1), false,0.1f,0.25f,2,5,35,
                    new Vector3(0,-16,0),0.8f,Mathf.Clamp(Mathf.RoundToInt(18 * amount),1,60),Atlas:new Vector2Int(4,4))
            }, true);
            fx.transform.position = at; fx.PointAlong(direction); fx.Play();
        }

        public static void SlamDust(Vector3 at, float radius)
        {
            if (!FxLibrary.Enabled) return;
            float s = Mathf.Clamp(radius / 3, 0.5f,1.4f);
            var fx = HellParticles.Create("HU_SlamDust", new[]
            {
                new HellParticleLayer("smoke", Fade(new Color(0.38f,0.32f,0.25f),0.65f),false,0.9f*s,1.8f*s,0.2f,0.8f,10,
                    new Vector3(0,0.15f,0),1.1f,36,Grow:true,Damping:8)
            }, true);
            fx.transform.position = at + Vector3.up * 0.15f; fx.SetDisc(Mathf.Max(0.3f,radius*0.25f),true);
            fx.SetOutward(radius*4,radius*6.5f); fx.Play();
        }

        static Color[] Fade(Color c, float alpha) => new[]
        {
            new Color(c.r,c.g,c.b,0), new Color(c.r,c.g,c.b,alpha), new Color(c.r,c.g,c.b,alpha*0.5f), new Color(c.r,c.g,c.b,0)
        };
    }
}
