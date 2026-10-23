using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Turris
{
    /// <summary>Rodzina wizualna HU niezależna od mechaniki obrażeń Turris.</summary>
    public enum HellFxElement { Physical, Fire, Water, Air, Earth, Enemy, Soul, Ash, Ice }

    /// <summary>Parametry jednej warstwy z ElementFx HU, wyrażone w metrach i sekundach.</summary>
    internal sealed class HellParticleLayer
    {
        internal readonly string Sprite;
        internal readonly Color[] Ramp;
        internal readonly bool Additive, AlignToVelocity, Grow, Rock, Upright, FlipbookFromStart, Local;
        internal readonly float SizeMin, SizeMax, SpeedMin, SpeedMax, Spread, Lifetime, Turbulence, Damping, EmitRadius;
        internal readonly Vector3 Gravity;
        internal readonly int Amount, Flipbook;
        internal readonly Vector2Int Atlas;
        /// <summary>Wariant wyglądu w uber-shaderze (HellVfxMaterials); null = profil tekstury.</summary>
        internal readonly string Look;

        internal HellParticleLayer(string Sprite, Color[] Ramp, bool Additive, float SizeMin, float SizeMax,
            float SpeedMin, float SpeedMax, float Spread, Vector3 Gravity, float Lifetime, int Amount,
            bool AlignToVelocity = false, bool Grow = false, float Turbulence = 0, float Damping = 0,
            bool Rock = false, bool Upright = false, float EmitRadius = 0.2f, int Flipbook = 0,
            bool FlipbookFromStart = false, bool Local = false, Vector2Int Atlas = default, string Look = null)
        {
            this.Sprite = Sprite; this.Ramp = Ramp; this.Additive = Additive; this.SizeMin = SizeMin; this.SizeMax = SizeMax;
            this.SpeedMin = SpeedMin; this.SpeedMax = SpeedMax; this.Spread = Spread; this.Gravity = Gravity;
            this.Lifetime = Lifetime; this.Amount = Amount; this.AlignToVelocity = AlignToVelocity; this.Grow = Grow;
            this.Turbulence = Turbulence; this.Damping = Damping; this.Rock = Rock; this.Upright = Upright;
            this.EmitRadius = EmitRadius; this.Flipbook = Flipbook; this.FlipbookFromStart = FlipbookFromStart;
            this.Local = Local; this.Atlas = Atlas; this.Look = Look;
        }
    }

    /// <summary>
    /// Adapter GpuParticles3D → ParticleSystem. Amount HU oznacza liczbę żywych cząsteczek,
    /// dlatego emisja ciągła wynosi Amount / Lifetime. Atlas losuje obraz raz, flipbook odtwarza klatki.
    /// </summary>
    public sealed class HellParticles : MonoBehaviour
    {
        ParticleSystem[] systems;
        HellParticleLayer[] recipes;
        bool oneShot, retiring;
        float age, life, scale = 1f, amount = 1f;
        internal bool pooled;
        public int LayerCount => systems == null ? 0 : systems.Length;
        public float MaxLifetime { get; private set; }
        public bool Emitting { get; private set; }

        internal static HellParticles Create(string name, HellParticleLayer[] layers, bool burst, Transform parent = null)
        {
            if (!FxLibrary.Enabled) return null;
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var fx = go.AddComponent<HellParticles>();
            fx.recipes = layers; fx.oneShot = burst;
            fx.systems = new ParticleSystem[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                fx.systems[i] = Build(layers[i], burst, go.transform);
                fx.MaxLifetime = Mathf.Max(fx.MaxLifetime, layers[i].Lifetime);
            }
            return fx;
        }

        static ParticleSystem Build(HellParticleLayer l, bool burst, Transform parent)
        {
            var go = new GameObject(l.Sprite);
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false; main.loop = !burst; main.duration = 1f;
            main.startLifetime = l.Lifetime; main.startSpeed = new ParticleSystem.MinMaxCurve(l.SpeedMin, l.SpeedMax);
            main.startColor = Color.white; main.gravityModifier = 0;
            main.simulationSpace = l.Local ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = Mathf.Max(8, l.Amount + 8);
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            main.startRotation = l.Upright ? new ParticleSystem.MinMaxCurve(-14 * Mathf.Deg2Rad, 14 * Mathf.Deg2Rad)
                : l.AlignToVelocity ? new ParticleSystem.MinMaxCurve(0) : new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            SetSize(ps, l, 1f);

            var emission = ps.emission;
            emission.rateOverTime = burst ? 0 : l.Amount / l.Lifetime;
            if (burst) emission.SetBursts(new[] { new ParticleSystem.Burst(0, (short)l.Amount) });
            var shape = ps.shape;
            shape.shapeType = l.Spread >= 120 ? ParticleSystemShapeType.Sphere : ParticleSystemShapeType.Cone;
            shape.radius = l.EmitRadius; shape.angle = Mathf.Min(90, l.Spread);
            shape.rotation = new Vector3(-90, 0, 0);
            shape.randomDirectionAmount = l.Spread >= 120 ? 1f : 0f;

            var force = ps.forceOverLifetime;
            force.enabled = l.Gravity != Vector3.zero; force.space = ParticleSystemSimulationSpace.World;
            force.x = l.Gravity.x; force.y = l.Gravity.y; force.z = l.Gravity.z;
            var color = ps.colorOverLifetime;
            color.enabled = true; color.color = Ramp(l.Ramp);
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1, l.Grow ? AnimationCurve.Linear(0, 0.3f, 1, 1)
                : l.Flipbook > 0 ? new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(0.25f, 1), new Keyframe(0.7f, 0.85f), new Keyframe(1, 0.2f))
                : new AnimationCurve(new Keyframe(0, 0.6f), new Keyframe(0.2f, 1), new Keyframe(1, 0)));
            if (!l.Upright && !l.AlignToVelocity)
            {
                var rotation = ps.rotationOverLifetime; rotation.enabled = true;
                rotation.z = new ParticleSystem.MinMaxCurve(-40 * Mathf.Deg2Rad, 40 * Mathf.Deg2Rad);
            }
            if (l.Turbulence > 0)
            {
                var noise = ps.noise; noise.enabled = true; noise.strength = l.Turbulence * 0.2f;
                noise.frequency = 0.5f; noise.scrollSpeed = 0.5f; noise.quality = ParticleSystemNoiseQuality.Low;
            }
            if (l.Damping > 0)
            {
                var drag = ps.limitVelocityOverLifetime; drag.enabled = true;
                drag.drag = new ParticleSystem.MinMaxCurve(l.Damping * 0.5f, l.Damping);
                drag.multiplyDragByParticleSize = false; drag.multiplyDragByParticleVelocity = false;
            }
            int x = l.Flipbook > 0 ? l.Flipbook : l.Atlas.x;
            int y = l.Flipbook > 0 ? l.Flipbook : l.Atlas.y;
            if (x > 0 && y > 0)
            {
                var sheet = ps.textureSheetAnimation;
                sheet.enabled = true; sheet.numTilesX = x; sheet.numTilesY = y;
                sheet.animation = ParticleSystemAnimationType.WholeSheet;
                sheet.startFrame = l.FlipbookFromStart ? new ParticleSystem.MinMaxCurve(0) : new ParticleSystem.MinMaxCurve(0, 1);
                // Jednorazowa eksplozja zwalnia pod koniec (szybki wybuch, wolne dogasanie); pętle płomieni grają równo.
                sheet.frameOverTime = l.Flipbook > 0 && l.FlipbookFromStart
                    ? new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 0, 0, 2.2f), new Keyframe(1, 1, 0.35f, 0)))
                    : new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0, 1, l.Flipbook > 0 ? 1 : 0));
                sheet.cycleCount = 1;
            }
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = l.Rock ? HellVfxMaterials.Rock : HellVfxMaterials.Sprite(l.Sprite, l.Additive, l.Look);
            if (!l.Rock) HellVfxMaterials.ApplyStreams(ps, renderer);
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.maxParticleSize = 2f;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            if (l.Rock)
            {
                renderer.renderMode = ParticleSystemRenderMode.Mesh; renderer.mesh = ProcMesh.Sphere();
                renderer.sharedMaterial = HellVfxMaterials.Rock;
            }
            else if (l.AlignToVelocity)
            {
                renderer.renderMode = ParticleSystemRenderMode.Stretch; renderer.velocityScale = 0.04f; renderer.lengthScale = 1.5f;
            }
            return ps;
        }

        internal static Gradient Ramp(Color[] colors)
        {
            var rgb = new GradientColorKey[colors.Length]; var alpha = new GradientAlphaKey[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                float t = i / (float)(colors.Length - 1);
                rgb[i] = new GradientColorKey(colors[i], t); alpha[i] = new GradientAlphaKey(colors[i].a, t);
            }
            var gradient = new Gradient(); gradient.SetKeys(rgb, alpha); return gradient;
        }

        static void SetSize(ParticleSystem ps, HellParticleLayer l, float factor)
        {
            var main = ps.main;
            float sheet = l.Sprite == "flame_sheet" ? 1.35f : l.Sprite == "fireball_sheet" ? 1.5f : 1f;
            float width = l.Rock ? 0.18f : l.Sprite == "droplet" ? 0.45f : l.Flipbook > 0 ? 0.8f : 1f;
            float height = l.Rock ? 0.15f : 1f;
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(l.SizeMin * width * sheet * factor, l.SizeMax * width * sheet * factor);
            main.startSizeY = new ParticleSystem.MinMaxCurve(l.SizeMin * height * sheet * factor, l.SizeMax * height * sheet * factor);
            main.startSizeZ = main.startSizeX;
        }

        public void SetAmount(float ratio)
        {
            amount = Mathf.Clamp01(ratio);
            for (int i = 0; i < systems.Length; i++)
            {
                var emission = systems[i].emission;
                emission.rateOverTime = oneShot ? 0 : recipes[i].Amount * amount / recipes[i].Lifetime;
                if (oneShot) emission.SetBursts(new[] { new ParticleSystem.Burst(0, (short)Mathf.RoundToInt(recipes[i].Amount * amount)) });
            }
        }

        /// <summary>
        /// Efekt na poruszającej się postaci (status): cząsteczki w przestrzeni świata przejmują bieżącą prędkość emitera,
        /// więc lecą razem z celem – zostaje krótki ogon zamiast smugi długiej na cały czas życia cząsteczek.
        /// </summary>
        public HellParticles Follow(float inherit = 0.95f)
        {
            for (int i = 0; i < systems.Length; i++)
            {
                var main = systems[i].main;
                if (main.simulationSpace != ParticleSystemSimulationSpace.World) continue;
                main.emitterVelocityMode = ParticleSystemEmitterVelocityMode.Transform;
                var iv = systems[i].inheritVelocity;
                iv.enabled = true; iv.mode = ParticleSystemInheritVelocityMode.Current; iv.curve = new ParticleSystem.MinMaxCurve(inherit);
            }
            return this;
        }

        public void SetScale(float value)
        {
            scale = Mathf.Max(0.05f, value);
            for (int i = 0; i < systems.Length; i++) SetSize(systems[i], recipes[i], scale);
        }

        public void SetBurstRadius(float radius)
        {
            SetScale(Mathf.Clamp(radius / 2.4f, 0.35f, 1f));
            SetAmount(Mathf.Clamp((radius - 0.4f) / 2.6f, 0.15f, 1f));
            for (int i = 0; i < systems.Length; i++)
            {
                var shape = systems[i].shape; shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = Mathf.Max(0.1f, radius * 0.35f); shape.randomDirectionAmount = 1;
                if (radius >= 2.8f && recipes[i].Sprite == "fireball_sheet")
                {
                    var emission = systems[i].emission; emission.SetBursts(Array.Empty<ParticleSystem.Burst>());
                }
            }
        }

        public void SetDisc(float radius, bool ring = false)
        {
            for (int i = 0; i < systems.Length; i++)
            {
                var shape = systems[i].shape; shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = Mathf.Max(0.1f, radius); shape.radiusThickness = ring ? Mathf.Clamp01(0.3f / shape.radius) : 1;
                shape.rotation = new Vector3(90, 0, 0); shape.randomDirectionAmount = 0;
                // Emiter leży na ziemi, prędkość początkowa biegnie w górę jak w HU.
                var velocity = systems[i].velocityOverLifetime; velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.World;
                velocity.x = new ParticleSystem.MinMaxCurve(0,0); velocity.y = new ParticleSystem.MinMaxCurve(recipes[i].SpeedMin, recipes[i].SpeedMax); velocity.z = new ParticleSystem.MinMaxCurve(0,0);
                var main = systems[i].main; main.startSpeed = 0;
            }
            if (!ring) SetAmount(Mathf.Clamp(radius * radius / 8f, 0.4f, 1));
        }

        public void SetBox(float halfWidth, float halfLength)
        {
            SetDisc(1);
            foreach (var ps in systems)
            {
                var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box;
                shape.rotation = Vector3.zero; shape.scale = new Vector3(halfWidth * 2, 0.1f, halfLength * 2);
            }
            SetAmount(Mathf.Clamp(halfWidth * halfLength / 3f, 0.3f, 1));
        }

        public void SetOutward(float min, float max)
        {
            foreach (var ps in systems)
            {
                var velocity = ps.velocityOverLifetime; velocity.enabled = true;
                velocity.x = new ParticleSystem.MinMaxCurve(0,0); velocity.y = new ParticleSystem.MinMaxCurve(0,0); velocity.z = new ParticleSystem.MinMaxCurve(0,0);
                velocity.radial = new ParticleSystem.MinMaxCurve(min, max);
            }
        }

        public void PointAlong(Vector3 direction, float angle = -1, float speed = 0)
        {
            if (direction.sqrMagnitude < 0.001f) direction = Vector3.up;
            foreach (var ps in systems)
            {
                var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone;
                shape.rotation = Quaternion.LookRotation(direction).eulerAngles;
                shape.randomDirectionAmount = 0;
                if (angle >= 0) shape.angle = Mathf.Min(90, angle);
                if (speed > 0) { var main = ps.main; main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.7f, speed); }
            }
        }

        public void SetStream(Vector3 velocity, Camera camera)
        {
            if (camera == null) return;
            var view = camera.transform.InverseTransformDirection(-velocity);
            if (view.x * view.x + view.y * view.y < 0.01f) return;
            float angle = -Mathf.Atan2(view.x, view.y);
            for (int i = 0; i < systems.Length; i++) if (recipes[i].Upright)
            {
                var main = systems[i].main; main.startRotation = new ParticleSystem.MinMaxCurve(angle - 0.24f, angle + 0.24f);
            }
        }

        public void Play(float duration = 0)
        {
            gameObject.SetActive(true); retiring = false; age = 0; life = duration; Emitting = true;
            foreach (var ps in systems) { ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); ps.Play(false); }
        }

        /// <summary>Dogaszenie w miejscu końca ruchu, także po zniszczeniu pocisku lub wyłączeniu statusu.</summary>
        public void StopAndRelease()
        {
            if (retiring) return;
            transform.SetParent(null, true); retiring = true; Emitting = false; age = 0;
            foreach (var ps in systems)
            {
                var main = ps.main; main.loop = false;
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
            // Odczepienie z OnDestroy rodzica może ominąć następny Update komponentu.
            // Harmonogram Unity sprząta również taki obiekt po ostatniej cząsteczce.
            if (!pooled) Destroy(gameObject, MaxLifetime + 0.15f);
        }

        void Update()
        {
            age += Time.deltaTime;
            if (!oneShot && !retiring && life > 0 && age >= life) StopAndRelease();
            if (!retiring && !oneShot) return;
            // Po zatrzymaniu żadna cząsteczka nie może żyć dłużej niż MaxLifetime.
            // IsAlive Unity bywa true dla zatrzymanej pętli do końca cyklu emitera.
            if (!retiring || age <= MaxLifetime + 0.15f)
                foreach (var ps in systems) if (ps.IsAlive(false)) return;
            Emitting = false;
            if (pooled) gameObject.SetActive(false); else Destroy(gameObject);
        }
    }
}
