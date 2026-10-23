using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Gotowe efekty wizualne złożone z systemów cząsteczek, kręgów na ziemi i dynamicznych świateł.
    /// Wszystkie metody są bezpieczne poza trybem gry (zwracają null), więc testy EditMode ich nie uruchamiają.
    /// </summary>
    public static class FxLibrary
    {
        public static bool Enabled => Application.isPlaying;

        // ------------------------------------------------------------------ Budulec

        public struct Emit
        {
            public int burst;               // cząsteczki naraz
            public float rate;              // cząsteczki na sekundę (ciągłe)
            public float duration;          // czas emisji ciągłej
            public Vector2 life, speed, size;
            public Color colorA, colorB;
            public float gravity;
            public ParticleSystemShapeType shape;
            public float radius, angle;
            public bool stretch;
            public float stretchLength;
            public bool alpha;              // dym (mieszanie alfa) zamiast świecenia
            public float orbital;           // wirowanie wokół osi Y
            public float radial;            // prędkość radialna (ujemna = zbieganie się do środka)
            public float noise;
            public bool localSpace;
            public float sizeGrow;          // >1: rośnie, <1: maleje
            public float drag;
            public Vector3 rotation;        // obrót kształtu emitera
            public string sprite;           // oryginalna tekstura HU (domyślnie glow/spark/smoke)
            public Vector2Int atlas;        // losowany, nieruchomy obraz z atlasu
            public int flipbook;            // liczba kolumn i wierszy animowanej planszy
        }

        public static ParticleSystem Particles(Vector3 pos, in Emit e, Transform parent = null)
        {
            if (!Enabled) return null;
            var go = new GameObject("Fx");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(e.rotation);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = Mathf.Max(0.05f, e.duration);
            main.startLifetime = new ParticleSystem.MinMaxCurve(e.life.x, Mathf.Max(e.life.x, e.life.y));
            main.startSpeed = new ParticleSystem.MinMaxCurve(e.speed.x, e.speed.y);
            main.startSize = new ParticleSystem.MinMaxCurve(e.size.x, Mathf.Max(e.size.x, e.size.y));
            main.startColor = new ParticleSystem.MinMaxGradient(e.colorA, e.colorB.a > 0 ? e.colorB : e.colorA);
            main.gravityModifier = e.gravity;
            main.simulationSpace = e.localSpace ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
            main.maxParticles = Mathf.Max(8, e.burst + Mathf.CeilToInt(e.rate * Mathf.Max(0.1f, e.duration) * 1.2f) + 8);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.stopAction = parent == null ? ParticleSystemStopAction.Destroy : ParticleSystemStopAction.None;

            var em = ps.emission;
            em.rateOverTime = e.rate;
            if (e.burst > 0) em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)e.burst) });
            else em.SetBursts(new ParticleSystem.Burst[0]);

            var sh = ps.shape;
            sh.enabled = true;
            sh.shapeType = e.shape == 0 ? ParticleSystemShapeType.Sphere : e.shape;
            sh.radius = Mathf.Max(0.001f, e.radius);
            sh.angle = e.angle;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.08f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(g);

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            float grow = e.sizeGrow <= 0f ? 0.3f : e.sizeGrow;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, grow));

            if (e.orbital != 0f || e.radial != 0f)
            {
                var vel = ps.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.orbitalX = 0f; vel.orbitalZ = 0f;
                vel.orbitalY = e.orbital;
                vel.radial = e.radial;
                vel.x = 0f; vel.y = 0f; vel.z = 0f;
            }
            if (e.noise > 0f)
            {
                var n = ps.noise;
                n.enabled = true;
                n.strength = e.noise;
                n.frequency = 0.8f;
                n.scrollSpeed = 0.5f;
            }
            if (e.drag > 0f)
            {
                var lim = ps.limitVelocityOverLifetime;
                lim.enabled = true;
                lim.drag = e.drag;
            }

            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = HellVfxMaterials.Sprite(e.sprite ?? (e.alpha ? "smoke" : e.stretch ? "spark" : "glow"), !e.alpha);
            HellVfxMaterials.ApplyStreams(ps, r);
            if (e.flipbook > 0 || e.atlas.x > 0)
            {
                var sheet = ps.textureSheetAnimation;
                sheet.enabled = true;
                sheet.numTilesX = e.flipbook > 0 ? e.flipbook : e.atlas.x;
                sheet.numTilesY = e.flipbook > 0 ? e.flipbook : e.atlas.y;
                sheet.startFrame = e.flipbook > 0 ? new ParticleSystem.MinMaxCurve(0f) : new ParticleSystem.MinMaxCurve(0f, 1f);
                sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0, 1, e.flipbook > 0 ? 1 : 0));
            }
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            if (e.stretch)
            {
                r.renderMode = ParticleSystemRenderMode.Stretch;
                r.velocityScale = 0.04f;
                r.lengthScale = e.stretchLength > 0 ? e.stretchLength : 2.5f;
            }
            else r.renderMode = ParticleSystemRenderMode.Billboard;
            r.sortingFudge = e.alpha ? 10f : 0f;

            ps.Play();
            return ps;
        }

        public static Light PointLight(Vector3 pos, Color color, float intensity, float range, float life, Transform parent = null)
        {
            if (!Enabled) return null;
            var go = new GameObject("FxLight");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            intensity = RenderingSetup.PointIntensity(intensity, range);
            l.intensity = intensity;
            l.range = range;
            l.shadows = LightShadows.None;
            l.renderMode = LightRenderMode.ForcePixel;
            if (life > 0f)
            {
                var f = go.AddComponent<FxLight>();
                f.life = life;
                f.startIntensity = intensity;
            }
            return l;
        }

        public static FxDecal Decal(Vector3 pos, Material mat, Color color, float fromSize, float toSize, float life, float spin = 0f, float fade = 0.35f)
        {
            if (!Enabled) return null;
            var go = new GameObject("FxDecal");
            go.transform.position = pos + Vector3.up * 0.03f;
            go.AddComponent<MeshFilter>().sharedMesh = ProcMesh.GroundQuad();
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            var d = go.AddComponent<FxDecal>();
            d.Init(color, fromSize, toSize, life, spin, fade);
            return d;
        }

        static readonly System.Collections.Generic.Stack<FxDecal> decalPool = new System.Collections.Generic.Stack<FxDecal>();

        /// <summary>
        /// Decal z puli: tylko dla śladów z określonym życiem, do których wywołujący nie trzyma referencji
        /// (wypalenia, szron, pęknięcia). Po wygaśnięciu wraca do puli zamiast Destroy.
        /// </summary>
        public static FxDecal TimedDecal(Vector3 pos, Material mat, Color color, float fromSize, float toSize, float life, float fade = 0.35f)
        {
            if (!Enabled || life <= 0f) return null;
            FxDecal d = null;
            while (d == null && decalPool.Count > 0) d = decalPool.Pop();
            if (d == null)
            {
                var go = new GameObject("FxDecalPooled");
                go.AddComponent<MeshFilter>().sharedMesh = ProcMesh.GroundQuad();
                var r = go.AddComponent<MeshRenderer>();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
                d = go.AddComponent<FxDecal>();
                d.release = x => { x.gameObject.SetActive(false); decalPool.Push(x); };
            }
            d.transform.position = pos + Vector3.up * 0.03f;
            d.transform.rotation = Quaternion.identity;
            d.GetComponent<MeshRenderer>().sharedMaterial = mat;
            d.gameObject.SetActive(true);
            d.Init(color, fromSize, toSize, life, 0f, fade);
            return d;
        }

        // ------------------------------------------------------------------ Efekty złożone

        /// <summary>Rozbłysk: jądro + iskry + krótkie światło.</summary>
        public static void Flash(Vector3 pos, Color c, float scale = 1f)
        {
            if (!Enabled) return;
            Particles(pos, new Emit { burst = 1, life = new Vector2(0.12f, 0.16f), size = new Vector2(0.9f, 1.1f) * scale, colorA = c, colorB = Color.white, sizeGrow = 1.6f });
            Particles(pos, new Emit { burst = 26, life = new Vector2(0.25f, 0.55f), speed = new Vector2(3f, 7f) * scale, size = new Vector2(0.07f, 0.13f) * scale, colorA = c, colorB = Color.white, gravity = 0.6f, stretch = true, sizeGrow = 0.2f });
            PointLight(pos, c, 3.5f * scale, 5f * scale, 0.25f);
        }

        /// <summary>Iskry przy bloku (metal o metal).</summary>
        public static void Sparks(Vector3 pos, Vector3 normal, Color c)
        {
            if (!Enabled) return;
            Particles(pos, new Emit { burst = 40, life = new Vector2(0.3f, 0.7f), speed = new Vector2(3f, 8f), size = new Vector2(0.05f, 0.1f), colorA = c, colorB = new Color(1f, 0.95f, 0.7f), gravity = 1.2f, shape = ParticleSystemShapeType.Cone, angle = 45f, radius = 0.05f, stretch = true, stretchLength = 3f, sizeGrow = 0.3f, rotation = Quaternion.LookRotation(normal.sqrMagnitude > 0.01f ? normal : Vector3.up).eulerAngles });
            Particles(pos, new Emit { burst = 2, life = new Vector2(0.12f, 0.16f), size = new Vector2(1.0f, 1.3f), colorA = c, colorB = Color.white, sizeGrow = 1.4f });
            PointLight(pos, c, 2.5f, 4f, 0.18f);
        }

        /// <summary>Trafienie w ciało: ciemnoczerwone krople i kurz.</summary>
        public static void Blood(Vector3 pos, Vector3 dir, float amount = 1f)
        {
            HellFx.Blood(pos, dir, amount);
        }

        /// <summary>Parowanie: złoto-biała gwiazda, pierścień i fala iskier.</summary>
        public static void Parry(Vector3 pos)
        {
            if (!Enabled) return;
            Color gold = new Color(1f, 0.85f, 0.4f);
            Particles(pos, new Emit { burst = 1, life = new Vector2(0.18f, 0.22f), size = new Vector2(1.5f, 1.7f), colorA = gold, colorB = Color.white, sizeGrow = 1.8f });
            Particles(pos, new Emit { burst = 60, life = new Vector2(0.35f, 0.7f), speed = new Vector2(5f, 11f), size = new Vector2(0.07f, 0.13f), colorA = gold, colorB = Color.white, stretch = true, stretchLength = 4f, gravity = 0.3f, sizeGrow = 0.2f });
            Particles(pos, new Emit { burst = 30, life = new Vector2(0.6f, 1.1f), speed = new Vector2(0.5f, 1.8f), size = new Vector2(0.1f, 0.18f), colorA = gold, colorB = Color.white, noise = 1f, sizeGrow = 0.2f });
            Decal(pos - Vector3.up * (pos.y - 0.02f), FxMaterials.Ring, gold, 0.5f, 5f, 0.45f);
            PointLight(pos, gold, 4.5f, 7f, 0.35f);
        }

        /// <summary>Przełamanie gardy: odłamki i pomarańczowy wybuch.</summary>
        public static void GuardBreak(Vector3 pos)
        {
            if (!Enabled) return;
            Color o = new Color(1f, 0.5f, 0.15f);
            Particles(pos, new Emit { burst = 45, life = new Vector2(0.5f, 0.9f), speed = new Vector2(2f, 6f), size = new Vector2(0.08f, 0.16f), colorA = new Color(0.8f, 0.82f, 0.9f), colorB = o, gravity = 1.5f, sizeGrow = 0.5f });
            Flash(pos, o, 1.2f);
        }

        /// <summary>Unik: kurz spod stóp.</summary>
        public static void Dust(Vector3 pos, float scale = 1f)
        {
            HellFx.SlamDust(pos, Mathf.Max(0.5f, scale * 1.2f));
        }

        /// <summary>Fala uderzeniowa: krąg na ziemi + pierścień + iskry + słup światła.</summary>
        public static void Shockwave(Vector3 ground, Color c, float radius, bool pillar = true)
        {
            if (!Enabled) return;
            var family = HellFx.FromColor(c);
            HellFx.Burst(ground + Vector3.up * 0.1f, family, radius);
            Decal(ground, HellVfxMaterials.Ground(family), c, radius * 2, radius * 2, 0.55f, 0, 0.6f);
            Decal(ground, FxMaterials.Ring, c, 0.3f, radius * 2, 0.4f);
            if (pillar) HellFx.SlamDust(ground, radius);
            HellFxPool.Instance.FlashLight(ground + Vector3.up, c, 3.5f, radius * 2, 0.3f);
        }

        /// <summary>Energia zbierająca się w dłoni podczas rzucania czaru.</summary>
        public static ParticleSystem Gather(Transform hand, Color c, float duration)
        {
            if (!Enabled || hand == null) return null;
            var ps = Particles(hand.position, new Emit { rate = 110, duration = duration, life = new Vector2(0.3f, 0.35f), speed = new Vector2(-2.2f, -2.8f), size = new Vector2(0.07f, 0.13f), colorA = c, colorB = Color.white, shape = ParticleSystemShapeType.Sphere, radius = 0.7f, localSpace = true, sizeGrow = 0.3f }, hand);
            Particles(hand.position, new Emit { rate = 16, duration = duration, life = new Vector2(0.2f, 0.25f), size = new Vector2(0.4f, 0.55f), colorA = c, colorB = Color.white, localSpace = true, sizeGrow = 1.3f }, hand);
            var l = PointLight(hand.position, c, 1.5f, 3f, duration + 0.2f, hand);
            AutoDestroy(ps != null ? ps.gameObject : null, duration + 0.6f);
            return ps;
        }

        /// <summary>Leczenie: krąg i unoszące się spiralnie drobinki.</summary>
        public static void Heal(Transform who, Color c, float duration)
        {
            if (!Enabled || who == null) return;
            Decal(who.position, FxMaterials.Glyph, c, 2.2f, 2.4f, duration, 40f, 0.5f).Follow(who);
            var ps = Particles(who.position, new Emit { rate = 70, duration = duration, life = new Vector2(0.8f, 1.3f), speed = new Vector2(0.8f, 1.6f), size = new Vector2(0.09f, 0.16f), colorA = c, colorB = Color.white, shape = ParticleSystemShapeType.Circle, radius = 0.6f, rotation = new Vector3(-90, 0, 0), orbital = 2.5f, localSpace = true, sizeGrow = 0.2f }, who);
            AutoDestroy(ps != null ? ps.gameObject : null, duration + 1.5f);
            PointLight(who.position + Vector3.up, c, 2f, 4f, duration + 0.3f, who);
        }

        /// <summary>Picie flaszki: pomarańczowe drobinki wokół postaci.</summary>
        public static void Drink(Transform who, Color c)
        {
            if (!Enabled || who == null) return;
            var ps = Particles(who.position + Vector3.up * 0.2f, new Emit { burst = 40, life = new Vector2(0.8f, 1.2f), speed = new Vector2(0.6f, 1.4f), size = new Vector2(0.09f, 0.15f), colorA = c, colorB = Color.white, shape = ParticleSystemShapeType.Circle, radius = 0.5f, rotation = new Vector3(-90, 0, 0), orbital = 3f, localSpace = true, sizeGrow = 0.2f }, who);
            AutoDestroy(ps != null ? ps.gameObject : null, 2f);
            PointLight(who.position + Vector3.up, c, 1.8f, 3f, 0.8f, who);
        }

        /// <summary>Śmierć przeciwnika: rozsypuje się w popiół i żar.</summary>
        public static void DeathAsh(Vector3 pos, float scale)
        {
            HellFx.Burst(pos + Vector3.up * 0.9f * scale, HellFxElement.Ash, scale * 2.4f);
        }

        /// <summary>Ogień (pochodnie, koksowniki): płomień, żar, dym i migoczące światło. Efekt trwały.</summary>
        public static GameObject Fire(Vector3 pos, float scale, Transform parent)
        {
            if (!Enabled) return null;
            var root = new GameObject("Fire");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.position = pos;
            HellFx.Flame(root.transform, Vector3.zero, Mathf.Max(0.1f, scale * 0.6f));
            var light = PointLight(pos + Vector3.up * 0.3f * scale, new Color(1f,0.6f,0.25f), 1.6f*scale, 7f*scale, 0, root.transform);
            if (light != null) light.gameObject.AddComponent<FxFlicker>().baseIntensity = light.intensity;
            return root;
        }

        /// <summary>Ciągła emisja przyczepiona do obiektu, w jego orientacji (kształt emitera obraca się z nim).</summary>
        public static ParticleSystem LoopEmit(Transform parent, Vector3 localOffset, Emit e)
        {
            var ps = Particles(parent.TransformPoint(localOffset), e, parent);
            if (ps == null) return null;
            ps.transform.rotation = parent.rotation * Quaternion.Euler(e.rotation);
            var main = ps.main;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            main.loop = true;
            main.duration = 1f;
            main.maxParticles = Mathf.CeilToInt(e.rate * e.life.y * 1.5f) + 8;
            var em = ps.emission;
            em.rateOverTime = e.rate;
            ps.Play();
            return ps;
        }

        static void Loop(Transform parent, Emit e, float yOffset = 0f)
        {
            var ps = Particles(parent.position + Vector3.up * yOffset, e, parent);
            if (ps == null) return;
            var main = ps.main;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            main.loop = true;
            main.duration = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = Mathf.CeilToInt(e.rate * e.life.y * 1.5f) + 8;
            var em = ps.emission;
            em.rateOverTime = e.rate;
            ps.Play();
        }

        /// <summary>Stała poświata/aura przyczepiona do obiektu (np. zaklęte ostrze).</summary>
        public static ParticleSystem Aura(Transform parent, Vector3 localOffset, Color c, float rate, float radius, float size)
        {
            if (!Enabled) return null;
            var ps = Particles(parent.TransformPoint(localOffset), new Emit { rate = rate, duration = 1f, life = new Vector2(0.4f, 0.7f), speed = new Vector2(0.1f, 0.4f), size = new Vector2(size * 0.6f, size), colorA = c, colorB = Color.white, shape = ParticleSystemShapeType.Sphere, radius = radius, sizeGrow = 0.2f, noise = 0.4f }, parent);
            var main = ps.main;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ps.Play();
            return ps;
        }

        public static void AutoDestroy(GameObject go, float seconds)
        {
            if (go != null && Enabled) Object.Destroy(go, seconds);
        }

        /// <summary>Oprawa pocisku: jądro, halo, smuga, krążące iskry i światło. Zwraca obiekt do odczepienia przy trafieniu.</summary>
        public static GameObject ProjectileVisual(Transform projectile, Color c, float size, Element element = Element.None, bool hostile = false, HellFxElement? visualFamily = null)
        {
            if (!Enabled) return null;
            var root = new GameObject("ProjectileFx");
            root.transform.SetParent(projectile, false);
            HellFxElement family = visualFamily ?? (element != Element.None ? HellFx.For(element) : hostile ? HellFxElement.Enemy : HellFx.FromColor(c));
            var trail = HellFx.Trail(root.transform, family, size);
            var bolt = root.AddComponent<HellBolt>();
            bolt.Init(trail, element, c, size);
            return root;
        }

        /// <summary>Odczepia oprawę pocisku: emisja ustaje, smuga dogasa.</summary>
        public static void DetachProjectileVisual(GameObject fx)
        {
            if (fx == null) return;
            var bolt = fx.GetComponent<HellBolt>();
            if (bolt != null) { bolt.Release(); return; }
            var layers = fx.GetComponent<HellParticles>();
            if (layers != null) { layers.StopAndRelease(); return; }
            fx.transform.SetParent(null, true);
            foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>()) ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            Object.Destroy(fx, 2.5f);
        }

        /// <summary>Błyskawica między dwoma punktami: poszarpana linia, rozbłysk i iskry na końcu.</summary>
        public static void Lightning(Vector3 from, Vector3 to, Color c)
        {
            if (!Enabled) return;
            LightningBolt.Strike(from, to, c);
            HellFx.Crackle(to);
            HellFxPool.Instance.FlashLight((from + to) * 0.5f, c, 4f, 6f, 0.15f);
            // Uderzenie z nieba w ziemię zostawia biało-niebieski, szybko stygnący ślad.
            if (from.y - to.y > 4f && Physics.Raycast(to + Vector3.up * 0.3f, Vector3.down, out var hit, 1.5f, ~0, QueryTriggerInteraction.Ignore))
            {
                TimedDecal(hit.point, HellVfxMaterials.Scorch(false), Color.white, 0.9f, 1.1f, 3f, 0.5f);
                TimedDecal(hit.point + Vector3.up * 0.002f, HellVfxMaterials.Scorch(true, bolt: true), Color.white, 0.8f, 0.8f, 0.7f, 0.9f);
            }
        }

        /// <summary>Podmuch mrozu w stożku przed postacią.</summary>
        public static void FrostCone(Vector3 origin, Vector3 forward, Color c, float range, float angle)
        {
            if (!Enabled) return;
            var rot = Quaternion.LookRotation(forward).eulerAngles;
            Particles(origin, new Emit { burst = 90, life = new Vector2(0.35f, 0.6f), speed = new Vector2(range * 1.4f, range * 2.2f), size = new Vector2(0.15f, 0.35f), colorA = c, colorB = Color.white, shape = ParticleSystemShapeType.Cone, angle = angle * 0.45f, radius = 0.15f, rotation = rot, drag = 1.5f, sizeGrow = 2f });
            Particles(origin, new Emit { burst = 40, life = new Vector2(0.6f, 1f), speed = new Vector2(range * 0.8f, range * 1.4f), size = new Vector2(0.4f, 0.7f), colorA = new Color(0.8f, 0.9f, 1f, 0.35f), alpha = true, shape = ParticleSystemShapeType.Cone, angle = angle * 0.45f, radius = 0.2f, rotation = rot, drag = 2f, sizeGrow = 2.5f });
            PointLight(origin + forward * range * 0.5f, c, 3f, range * 1.5f, 0.3f);
            // Lód zostaje po podmuchu: kryształy rosnące z odległością i szron w stożku.
            if (HellFx.GroundBelow(origin, out var ground))
            {
                Vector3 flat = new Vector3(forward.x, 0, forward.z).normalized;
                IceCrystals.Arc(ground, flat, range, angle, Mathf.Clamp(Mathf.RoundToInt(range * 1.2f), 3, 8), 0.55f);
                HellFx.FrostGround(ground + flat * range * 0.55f, range * 1.1f, 2.5f);
            }
        }

        /// <summary>Płonąca ziemia: krąg i kilka ognisk na czas trwania strefy.</summary>
        public static GameObject BurningGround(Vector3 center, float radius, Color c, float life)
        {
            return HellFx.Ground(center, radius, HellFxElement.Fire, c, life);
        }

        /// <summary>
        /// Strefa na ziemi w barwach żywiołu: ogień – płonąca ziemia; mróz – szron i mgła; błyskawica – iskry i pioruny;
        /// ziemia – pył i odłamki; bez żywiołu – kolce i linki (sidła).
        /// </summary>
        public static GameObject ElementGround(Vector3 center, float radius, Element element, Color c, float life, HellFxElement? visualFamily = null)
        {
            var ground = HellFx.Ground(center, radius, visualFamily ?? HellFx.For(element), c, life);
            if (ground != null && element == Element.Lightning) ground.AddComponent<GroundBolts>().Init(center, radius, c, life);
            return ground;
        }

        /// <summary>Pioruny bijące w losowe miejsca strefy (nawałnica).</summary>
        class GroundBolts : MonoBehaviour
        {
            Vector3 center; float radius, life, timer; Color color;
            public void Init(Vector3 c, float r, Color col, float l) { center = c; radius = r; color = col; life = l; }
            void Update()
            {
                life -= Time.deltaTime; timer -= Time.deltaTime;
                if (life <= 0 || timer > 0) return;
                timer = Random.Range(0.15f, 0.35f);
                Vector2 p = Random.insideUnitCircle * radius * 0.9f;
                Vector3 g = center + new Vector3(p.x, 0.05f, p.y);
                Lightning(g + Vector3.up * 7f + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)), g, color);
                Flash(g + Vector3.up * 0.3f, color, 0.6f);
            }
        }

        /// <summary>Wybuch pocisku przy trafieniu.</summary>
        public static void Impact(Vector3 pos, Color c, float scale = 1f)
        {
            HellFx.Impact(pos, HellFx.FromColor(c), c, scale * 1.5f);
        }
    }
}
