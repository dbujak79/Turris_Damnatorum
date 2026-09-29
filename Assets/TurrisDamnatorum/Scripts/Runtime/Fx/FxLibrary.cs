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
            r.material = e.alpha ? FxMaterials.AlphaBlended : FxMaterials.Additive;
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
            if (!Enabled) return;
            Particles(pos, new Emit { burst = Mathf.RoundToInt(28 * amount), life = new Vector2(0.35f, 0.8f), speed = new Vector2(1.5f, 4.5f), size = new Vector2(0.07f, 0.15f), colorA = new Color(0.45f, 0.02f, 0.02f, 1f), colorB = new Color(0.25f, 0.0f, 0.0f, 1f), gravity = 1.6f, alpha = true, shape = ParticleSystemShapeType.Cone, angle = 35f, radius = 0.05f, rotation = Quaternion.LookRotation(dir.sqrMagnitude > 0.01f ? dir : Vector3.up).eulerAngles, sizeGrow = 0.6f });
            Particles(pos, new Emit { burst = 10, life = new Vector2(0.18f, 0.3f), speed = new Vector2(1f, 3f), size = new Vector2(0.08f, 0.14f), colorA = new Color(1f, 0.45f, 0.3f), stretch = true, gravity = 0.5f });
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
            if (!Enabled) return;
            Particles(pos + Vector3.up * 0.1f, new Emit { burst = 14, life = new Vector2(0.5f, 0.9f), speed = new Vector2(0.6f, 1.6f) * scale, size = new Vector2(0.25f, 0.5f) * scale, colorA = new Color(0.55f, 0.5f, 0.45f, 0.5f), alpha = true, shape = ParticleSystemShapeType.Circle, radius = 0.3f * scale, sizeGrow = 2f, rotation = new Vector3(-90, 0, 0), drag = 2f });
        }

        /// <summary>Fala uderzeniowa: krąg na ziemi + pierścień + iskry + słup światła.</summary>
        public static void Shockwave(Vector3 ground, Color c, float radius, bool pillar = true)
        {
            if (!Enabled) return;
            Vector3 g = new Vector3(ground.x, 0.02f, ground.z);
            Decal(g, FxMaterials.Ring, c, 0.3f, radius * 2.2f, 0.45f);
            Decal(g, FxMaterials.Glyph, c, radius * 2f, radius * 2.1f, 0.7f, 90f);
            Particles(g + Vector3.up * 0.2f, new Emit { burst = Mathf.RoundToInt(50 + radius * 16), life = new Vector2(0.4f, 0.7f), speed = new Vector2(radius * 1.6f, radius * 2.6f), size = new Vector2(0.12f, 0.24f), colorA = c, colorB = Color.white, shape = ParticleSystemShapeType.Circle, radius = 0.2f, stretch = true, rotation = new Vector3(-90, 0, 0), sizeGrow = 0.3f });
            Particles(g, new Emit { burst = Mathf.RoundToInt(20 + radius * 8), life = new Vector2(0.6f, 1.1f), speed = new Vector2(0.4f, 1.2f), size = new Vector2(0.3f, 0.6f), colorA = new Color(0.45f, 0.4f, 0.38f, 0.45f), alpha = true, shape = ParticleSystemShapeType.Circle, radius = radius * 0.8f, rotation = new Vector3(-90, 0, 0), sizeGrow = 2f });
            if (pillar)
                Particles(g, new Emit { burst = 70, life = new Vector2(0.5f, 0.9f), speed = new Vector2(3f, 7f), size = new Vector2(0.14f, 0.28f), colorA = c, colorB = Color.white, shape = ParticleSystemShapeType.Circle, radius = radius * 0.35f, rotation = new Vector3(-90, 0, 0), sizeGrow = 0.2f });
            PointLight(g + Vector3.up, c, 5f, radius * 2.5f, 0.5f);
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
            if (!Enabled) return;
            Particles(pos + Vector3.up * 0.9f * scale, new Emit { burst = Mathf.RoundToInt(60 * scale), life = new Vector2(1.2f, 2.4f), speed = new Vector2(0.2f, 0.8f), size = new Vector2(0.08f, 0.2f) * scale, colorA = new Color(0.12f, 0.1f, 0.1f, 0.8f), colorB = new Color(0.3f, 0.28f, 0.26f, 0.6f), alpha = true, shape = ParticleSystemShapeType.Box, radius = 0.4f, gravity = -0.12f, noise = 0.6f, sizeGrow = 1.5f });
            Particles(pos + Vector3.up * 0.9f * scale, new Emit { burst = Mathf.RoundToInt(70 * scale), life = new Vector2(1f, 2f), speed = new Vector2(0.3f, 1f), size = new Vector2(0.06f, 0.11f), colorA = new Color(1f, 0.5f, 0.15f), colorB = new Color(1f, 0.8f, 0.3f), shape = ParticleSystemShapeType.Sphere, radius = 0.5f * scale, gravity = -0.2f, noise = 0.8f, sizeGrow = 0.2f });
        }

        /// <summary>Ogień (pochodnie, koksowniki): płomień, żar, dym i migoczące światło. Efekt trwały.</summary>
        public static GameObject Fire(Vector3 pos, float scale, Transform parent)
        {
            if (!Enabled) return null;
            var root = new GameObject("Fire");
            root.transform.SetParent(parent, false);
            root.transform.position = pos;
            Loop(root.transform, new Emit { rate = 40 * scale, life = new Vector2(0.35f, 0.6f), speed = new Vector2(0.6f, 1.3f) * scale, size = new Vector2(0.18f, 0.32f) * scale, colorA = new Color(1f, 0.45f, 0.1f), colorB = new Color(1f, 0.8f, 0.3f), shape = ParticleSystemShapeType.Sphere, radius = 0.08f * scale, sizeGrow = 0.2f, noise = 0.3f });
            Loop(root.transform, new Emit { rate = 16 * scale, life = new Vector2(0.2f, 0.35f), speed = new Vector2(0.3f, 0.6f) * scale, size = new Vector2(0.1f, 0.16f) * scale, colorA = new Color(1f, 0.95f, 0.7f), shape = ParticleSystemShapeType.Sphere, radius = 0.04f * scale, sizeGrow = 0.3f });
            Loop(root.transform, new Emit { rate = 6 * scale, life = new Vector2(1f, 2f), speed = new Vector2(0.5f, 1.5f), size = new Vector2(0.02f, 0.04f), colorA = new Color(1f, 0.6f, 0.2f), shape = ParticleSystemShapeType.Sphere, radius = 0.1f * scale, noise = 0.8f, sizeGrow = 0.3f });
            Loop(root.transform, new Emit { rate = 5 * scale, life = new Vector2(1.5f, 2.5f), speed = new Vector2(0.4f, 0.8f), size = new Vector2(0.25f, 0.4f) * scale, colorA = new Color(0.15f, 0.13f, 0.12f, 0.35f), alpha = true, shape = ParticleSystemShapeType.Sphere, radius = 0.08f * scale, sizeGrow = 3f, noise = 0.3f }, 0.35f * scale);
            var l = PointLight(pos + Vector3.up * 0.3f * scale, new Color(1f, 0.6f, 0.25f), 1.6f * scale, 7f * scale, 0f, root.transform);
            if (l != null) l.gameObject.AddComponent<FxFlicker>().baseIntensity = l.intensity;
            return root;
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
        public static GameObject ProjectileVisual(Transform projectile, Color c, float size)
        {
            if (!Enabled) return null;
            var root = new GameObject("ProjectileFx");
            root.transform.SetParent(projectile, false);
            // Halo
            Loop(root.transform, new Emit { rate = 40, life = new Vector2(0.12f, 0.16f), size = new Vector2(size * 4f, size * 4.8f), colorA = c, shape = ParticleSystemShapeType.Sphere, radius = 0.01f, sizeGrow = 1.2f });
            // Smuga (świat): cząsteczki zostają w miejscu i gasną
            Loop(root.transform, new Emit { rate = 120, life = new Vector2(0.3f, 0.45f), size = new Vector2(size * 1.8f, size * 2.6f), colorA = c, colorB = Color.white, shape = ParticleSystemShapeType.Sphere, radius = size * 0.4f, sizeGrow = 0.1f });
            // Iskry odpadające
            Loop(root.transform, new Emit { rate = 40, life = new Vector2(0.3f, 0.6f), speed = new Vector2(0.5f, 1.5f), size = new Vector2(0.04f, 0.08f), colorA = Color.white, colorB = c, shape = ParticleSystemShapeType.Sphere, radius = size, noise = 1f, sizeGrow = 0.2f });
            var orb = Particles(projectile.position, new Emit { rate = 20, duration = 1f, life = new Vector2(0.3f, 0.4f), size = new Vector2(size * 0.5f, size * 0.7f), colorA = Color.white, colorB = c, shape = ParticleSystemShapeType.Circle, radius = size * 2f, orbital = 12f, localSpace = true, sizeGrow = 0.4f }, root.transform);
            if (orb != null) { var m = orb.main; orb.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); m.loop = true; orb.Play(); }
            PointLight(projectile.position, c, 2.2f, 4f, 0f, root.transform);
            return root;
        }

        /// <summary>Odczepia oprawę pocisku: emisja ustaje, smuga dogasa.</summary>
        public static void DetachProjectileVisual(GameObject fx)
        {
            if (fx == null) return;
            fx.transform.SetParent(null, true);
            foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;
                main.loop = false;
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
            foreach (var l in fx.GetComponentsInChildren<Light>())
            {
                var f = l.gameObject.AddComponent<FxLight>();
                f.life = 0.2f;
                f.startIntensity = l.intensity;
            }
            Object.Destroy(fx, 1.2f);
        }

        /// <summary>Błyskawica między dwoma punktami: poszarpana linia, rozbłysk i iskry na końcu.</summary>
        public static void Lightning(Vector3 from, Vector3 to, Color c)
        {
            if (!Enabled) return;
            var go = new GameObject("Lightning");
            var lr = go.AddComponent<LineRenderer>();
            int n = Mathf.Clamp(Mathf.RoundToInt(Vector3.Distance(from, to) * 2.5f), 4, 24);
            lr.positionCount = n + 1;
            Vector3 side = Vector3.Cross((to - from).normalized, Vector3.up);
            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n;
                Vector3 p = Vector3.Lerp(from, to, t);
                if (i > 0 && i < n) p += (side * Random.Range(-0.35f, 0.35f) + Vector3.up * Random.Range(-0.25f, 0.25f)) * Mathf.Sin(t * Mathf.PI);
                lr.SetPosition(i, p);
            }
            lr.material = FxMaterials.Additive;
            lr.startColor = lr.endColor = Color.Lerp(c, Color.white, 0.4f);
            lr.startWidth = 0.16f; lr.endWidth = 0.08f;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Object.Destroy(go, 0.18f);
            Particles(to, new Emit { burst = 18, life = new Vector2(0.15f, 0.35f), speed = new Vector2(3f, 6f), size = new Vector2(0.05f, 0.1f), colorA = c, colorB = Color.white, stretch = true, sizeGrow = 0.2f });
            PointLight((from + to) * 0.5f, c, 4f, 6f, 0.15f);
        }

        /// <summary>Podmuch mrozu w stożku przed postacią.</summary>
        public static void FrostCone(Vector3 origin, Vector3 forward, Color c, float range, float angle)
        {
            if (!Enabled) return;
            var rot = Quaternion.LookRotation(forward).eulerAngles;
            Particles(origin, new Emit { burst = 90, life = new Vector2(0.35f, 0.6f), speed = new Vector2(range * 1.4f, range * 2.2f), size = new Vector2(0.15f, 0.35f), colorA = c, colorB = Color.white, shape = ParticleSystemShapeType.Cone, angle = angle * 0.45f, radius = 0.15f, rotation = rot, drag = 1.5f, sizeGrow = 2f });
            Particles(origin, new Emit { burst = 40, life = new Vector2(0.6f, 1f), speed = new Vector2(range * 0.8f, range * 1.4f), size = new Vector2(0.4f, 0.7f), colorA = new Color(0.8f, 0.9f, 1f, 0.35f), alpha = true, shape = ParticleSystemShapeType.Cone, angle = angle * 0.45f, radius = 0.2f, rotation = rot, drag = 2f, sizeGrow = 2.5f });
            PointLight(origin + forward * range * 0.5f, c, 3f, range * 1.5f, 0.3f);
        }

        /// <summary>Płonąca ziemia: krąg i kilka ognisk na czas trwania strefy.</summary>
        public static GameObject BurningGround(Vector3 center, float radius, Color c, float life)
        {
            if (!Enabled) return null;
            var root = new GameObject("BurningGround");
            root.transform.position = center;
            Decal(center, FxMaterials.Glyph, c, radius * 2f, radius * 2.1f, life, 20f, 0.2f);
            Decal(center, FxMaterials.Ring, c, radius * 1.6f, radius * 2.05f, life, 0f, 0.2f);
            // Języki ognia nad całą strefą przez cały czas jej trwania.
            Particles(center + Vector3.up * 0.1f, new Emit { rate = 45f * radius, duration = life, life = new Vector2(0.5f, 0.9f), speed = new Vector2(0.6f, 1.6f), size = new Vector2(0.3f, 0.6f), colorA = c, colorB = new Color(1f, 0.85f, 0.4f), shape = ParticleSystemShapeType.Circle, radius = radius * 0.9f, rotation = new Vector3(-90, 0, 0), sizeGrow = 0.3f, noise = 0.4f }, root.transform);
            int fires = Mathf.Clamp(Mathf.RoundToInt(radius * 2.5f), 3, 8);
            for (int i = 0; i < fires; i++)
            {
                Vector2 p = Random.insideUnitCircle * radius * 0.8f;
                Fire(center + new Vector3(p.x, 0.05f, p.y), Random.Range(0.6f, 1f), root.transform);
            }
            Object.Destroy(root, life);
            return root;
        }

        /// <summary>Wybuch pocisku przy trafieniu.</summary>
        public static void Impact(Vector3 pos, Color c, float scale = 1f)
        {
            if (!Enabled) return;
            Particles(pos, new Emit { burst = 1, life = new Vector2(0.14f, 0.2f), size = new Vector2(1.1f, 1.4f) * scale, colorA = c, colorB = Color.white, sizeGrow = 1.8f });
            Particles(pos, new Emit { burst = 40, life = new Vector2(0.35f, 0.7f), speed = new Vector2(2f, 6f) * scale, size = new Vector2(0.08f, 0.14f), colorA = c, colorB = Color.white, stretch = true, gravity = 0.4f, sizeGrow = 0.2f });
            Particles(pos, new Emit { burst = 12, life = new Vector2(0.4f, 0.8f), speed = new Vector2(0.3f, 1f), size = new Vector2(0.15f, 0.3f) * scale, colorA = c * 0.6f, noise = 0.6f, sizeGrow = 1.8f });
            PointLight(pos, c, 4f * scale, 5f * scale, 0.3f);
        }
    }
}
