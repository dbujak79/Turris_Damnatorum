// Wygenerowane przez tools/art/import_hu_vfx.py z World/ElementFx.cs (Hell Unleashed).
// Parametry warstw ze źródła; adapter silnika: HellParticles.cs. Nie edytuj ręcznie.
using UnityEngine;
namespace Turris
{
internal static class HellParticleRecipes
{
    private static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);
    private static readonly Color[] White = { C(1,1,1,0), C(1,1,1,1), C(1,1,1,0.9f), C(1,1,1,0) };
internal static HellParticleLayer[] LayersFor(HellFxElement element, int kind)
    {
        if (element == HellFxElement.Ice)
        {
            return IceLayers(kind);
        }

        bool burst = kind == 0;
        bool trail = kind == 2;
        return element switch
        {
            HellFxElement.Fire => FireLayers(kind),
            HellFxElement.Water => new[]
            {
                new HellParticleLayer("droplet", new[] { C(0.8f, 0.95f, 1, 1), C(0.35f, 0.7f, 1, 0.9f), C(0.2f, 0.45f, 0.9f, 0) },
                    true, 0.14f, 0.3f, burst ? 3.5f : 1.2f, burst ? 7 : 2.6f, burst ? 75 : 25, new Vector3(0, -9, 0),
                    0.7f, burst ? 40 : 12, AlignToVelocity: true),
                new HellParticleLayer("smoke", new[] { C(0.4f, 0.7f, 1, 0), C(0.35f, 0.65f, 1, 0.12f), C(0.3f, 0.6f, 0.9f, 0) },
                    true, 0.6f, 1.3f, 0.3f, 1.0f, 40, new Vector3(0, 0.3f, 0), 0.8f, burst ? 5 : 4, Grow: true),
            },
            HellFxElement.Air => new[]
            {
                burst
                    ? new HellParticleLayer("wisp", new[] { C(0.8f, 1, 0.9f, 0), C(0.75f, 1, 0.9f, 0.7f), C(0.6f, 0.9f, 0.8f, 0) },
                        true, 0.6f, 1.2f, 3, 6, 90, new Vector3(0, 0.5f, 0), 0.6f, 22, AlignToVelocity: true, Turbulence: 1.4f, Damping: 2)
                    : new HellParticleLayer("glow", new[] { C(0.85f, 1, 0.95f, 0), C(0.8f, 0.95f, 0.9f, 0.45f), C(0.7f, 0.9f, 0.85f, 0) },
                        true, 0.04f, 0.09f, 0.4f, 1.4f, 120, new Vector3(0, 0.3f, 0), 1.0f, 14, Turbulence: 2.0f, Damping: 1),
                new HellParticleLayer("smoke", new[] { C(0.7f, 0.9f, 0.85f, 0), C(0.6f, 0.85f, 0.8f, 0.18f), C(0.5f, 0.8f, 0.7f, 0) },
                    true, 1.0f, 1.8f, 0.5f, 1.5f, 60, Vector3.zero, 0.9f, burst ? 8 : 5, Grow: true, Turbulence: 1.0f),
            },
            HellFxElement.Earth => new[]
            {
                new HellParticleLayer("chunk", new[] { C(1, 1, 1), C(1, 1, 1) },
                    false, 0.6f, 1.4f, burst ? 3.5f : 1.0f, burst ? 7 : 2.2f, burst ? 40 : 20, new Vector3(0, -12, 0),
                    burst ? 0.9f : 0.6f, burst ? 18 : 6, Rock: true),
                new HellParticleLayer("smoke", new[] { C(0.25f, 0.2f, 0.14f, 0), C(0.3f, 0.24f, 0.16f, 0.5f), C(0.2f, 0.16f, 0.1f, 0) },
                    false, 1.0f, 2.2f, 0.5f, 1.6f, 50, new Vector3(0, 0.3f, 0), 1.3f, burst ? 12 : 6, Grow: true, Turbulence: 0.3f),
                new HellParticleLayer("spark", new[] { C(1, 0.8f, 0.4f), C(1, 0.5f, 0.15f), C(0.5f, 0.2f, 0, 0) },
                    true, 0.1f, 0.18f, burst ? 3 : 1, burst ? 6 : 2, 60, new Vector3(0, -5, 0), 0.6f, burst ? 14 : 4, AlignToVelocity: true),
            },
            HellFxElement.Enemy => new[]
            {
                new HellParticleLayer("spark", new[] { C(1, 0.6f, 0.4f), C(0.9f, 0.15f, 0.08f), C(0.4f, 0, 0, 0) },
                    true, 0.12f, 0.22f, burst ? 3 : 1, burst ? 7 : 2.5f, burst ? 80 : 25, new Vector3(0, -5, 0),
                    0.7f, burst ? 24 : 8, AlignToVelocity: true),
                new HellParticleLayer("smoke", new[] { C(0.12f, 0.02f, 0.02f, 0), C(0.16f, 0.03f, 0.03f, 0.6f), C(0.05f, 0.01f, 0.01f, 0) },
                    false, 0.9f, 1.8f, 0.4f, 1.4f, 40, new Vector3(0, 0.6f, 0), 1.2f, burst ? 10 : 6, Grow: true, Turbulence: 0.5f),
            },
            HellFxElement.Ash => new[]
            {
                new HellParticleLayer("smoke", new[] { C(0.2f, 0.18f, 0.17f, 0), C(0.22f, 0.2f, 0.19f, 0.85f), C(0.12f, 0.11f, 0.1f, 0) },
                    false, 0.12f, 0.3f, 0.4f, 1.6f, 180, new Vector3(0, -0.5f, 0), 1.6f, 34, Turbulence: 0.9f, Damping: 1.0f),
                new HellParticleLayer("spark", new[] { C(1, 0.85f, 0.5f), C(1, 0.4f, 0.08f), C(0.4f, 0.05f, 0, 0) },
                    true, 0.08f, 0.16f, 0.6f, 2.2f, 60, new Vector3(0, 1.6f, 0), 1.3f, 22, AlignToVelocity: true, Turbulence: 0.6f),
                new HellParticleLayer("smoke", new[] { C(0.06f, 0.05f, 0.05f, 0), C(0.08f, 0.07f, 0.07f, 0.5f), C(0.05f, 0.05f, 0.05f, 0) },
                    false, 1.0f, 1.8f, 0.3f, 0.9f, 40, new Vector3(0, 0.7f, 0), 1.5f, 6, Grow: true, Turbulence: 0.4f),
            },
            HellFxElement.Soul => new[]
            {
                new HellParticleLayer("glow", new[] { C(0.95f, 0.8f, 1, 0), C(0.75f, 0.45f, 1, 0.9f), C(0.45f, 0.15f, 0.9f, 0) },
                    true, 0.05f, 0.12f, 0.2f, 0.8f, 180, new Vector3(0, 0.5f, 0), 1.6f, burst ? 20 : 12, Turbulence: 1.8f, EmitRadius: 0.25f),
                new HellParticleLayer("smoke", new[] { C(0.4f, 0.15f, 0.6f, 0), C(0.35f, 0.12f, 0.55f, 0.18f), C(0.2f, 0.05f, 0.35f, 0) },
                    true, 0.4f, 0.8f, 0.1f, 0.4f, 60, new Vector3(0, 0.3f, 0), 1.4f, burst ? 6 : 4, Grow: true, Turbulence: 0.8f),
            },
            _ => new[]
            {
                new HellParticleLayer("spark", new[] { C(1, 1, 0.9f), C(1, 0.85f, 0.6f), C(0.6f, 0.5f, 0.4f, 0) },
                    true, 0.1f, 0.2f, burst ? 4 : 1, burst ? 8 : 2.5f, burst ? 80 : 30, new Vector3(0, -7, 0),
                    0.5f, burst ? 22 : 5, AlignToVelocity: true, Damping: 2),
                new HellParticleLayer("smoke", new[] { C(0.3f, 0.28f, 0.26f, 0), C(0.3f, 0.28f, 0.26f, 0.35f), C(0.2f, 0.2f, 0.2f, 0) },
                    false, 0.8f, 1.5f, 0.4f, 1.2f, 50, new Vector3(0, 0.3f, 0), 1.0f, burst ? 8 : 4, Grow: true),
            },
        };
    }

private static HellParticleLayer[] IceLayers(int kind)
    {
        Vector2Int shards = new(4, 4);
        Vector2Int sheet = new(4, 2);
        Color[] chips = { C(0.9f, 0.95f, 1, 1), C(0.85f, 0.92f, 1, 1), C(0.8f, 0.9f, 1, 0) };
        Color[] rime = { C(0.85f, 0.95f, 1, 0), C(0.8f, 0.92f, 1, 0.9f), C(0.6f, 0.85f, 1, 0) };
        return kind switch
        {
            0 => new[]
            {
                new HellParticleLayer("ice_shards", chips, false, 0.22f, 0.5f, 3.5f, 7.5f, 70, new Vector3(0, -14, 0), 0.85f, 22,
                    Damping: 1.0f, Atlas: shards),
                // Ice v3.1: small fragments thrown with the big splinters, so the break has a fine spray as well.
                new HellParticleLayer("ice_shards", chips, false, 0.08f, 0.16f, 1.8f, 3.0f, 80, new Vector3(0, -10, 0), 0.65f, 8,
                    Damping: 1.0f, Atlas: shards),
                new HellParticleLayer("frost_mist", new[] { C(0.75f, 0.88f, 1, 0), C(0.75f, 0.88f, 1, 0.35f), C(0.7f, 0.85f, 1, 0) },
                    false, 0.9f, 1.8f, 0.4f, 1.4f, 90, new Vector3(0, 0.2f, 0), 1.3f, 8, Grow: true, Turbulence: 0.4f, Atlas: sheet),
                new HellParticleLayer("frost_rime", rime, true, 0.08f, 0.18f, 1.0f, 3.5f, 80, new Vector3(0, -1.5f, 0), 1.0f, 30,
                    Damping: 2.0f, Atlas: sheet),
            },
            1 => new[]
            {
                new HellParticleLayer("frost_mist", new[] { C(0.75f, 0.88f, 1, 0), C(0.75f, 0.88f, 1, 0.22f), C(0.7f, 0.85f, 1, 0) },
                    false, 0.7f, 1.3f, 0.1f, 0.4f, 40, new Vector3(0, 0.05f, 0), 2.2f, 10, Grow: true, Turbulence: 0.3f, Atlas: sheet),
                new HellParticleLayer("frost_rime", rime, true, 0.05f, 0.12f, 0.2f, 0.6f, 40, new Vector3(0, 0.15f, 0), 1.6f, 24,
                    Turbulence: 0.6f, Atlas: sheet),
            },
            _ => new[]
            {
                new HellParticleLayer("frost_mist", new[] { C(0.75f, 0.88f, 1, 0), C(0.75f, 0.88f, 1, 0.4f), C(0.7f, 0.85f, 1, 0) },
                    false, 0.35f, 0.7f, 0.05f, 0.3f, 180, Vector3.zero, 0.7f, 16, Grow: true, Atlas: sheet),
                new HellParticleLayer("frost_rime", rime, true, 0.05f, 0.1f, 0.3f, 1.0f, 180, new Vector3(0, -2, 0), 0.6f, 20, Atlas: sheet),
                new HellParticleLayer("ice_shards", chips, false, 0.08f, 0.16f, 0.5f, 1.5f, 180, new Vector3(0, -9, 0), 0.5f, 6, Atlas: shards),
            },
        };
    }

private static HellParticleLayer[] FireLayers(int kind)
    {
        HellParticleLayer Flames(float sizeMin, float sizeMax, float speedMin, float speedMax, float spread, float lifetime, int amount, float damping = 0.0f) =>
            new("flame_sheet", White, true, sizeMin, sizeMax, speedMin, speedMax, spread, new Vector3(0, 2.2f, 0), lifetime, amount,
                Turbulence: 0.5f, Damping: damping, Upright: true, Flipbook: 8);
        // Embers: the white chips of VFX v2's atlas, tinted by the ramp, each particle keeping one chip.
        HellParticleLayer Embers(int amount, float lifetime) =>
            new("embers_atlas", new[] { C(1, 0.85f, 0.45f, 0), C(1, 0.6f, 0.15f, 1), C(0.9f, 0.25f, 0.03f, 0.8f), C(0.4f, 0.05f, 0, 0) },
                true, 0.22f, 0.4f, 0.5f, 2.0f, 70, new Vector3(0, 1.4f, 0), lifetime, amount, Turbulence: 1.6f, Damping: 0.6f,
                Atlas: new Vector2Int(4, 4));

        // Painted puffs of VFX v2 (fire_puffs_atlas): a random one of sixteen per particle, never played as frames.
        // The sheet carries its own colour, so the ramp only fades it in and out.
        HellParticleLayer Puffs(float sizeMin, float sizeMax, float speedMin, float speedMax, float spread, float lifetime, int amount, float damping = 0.0f) =>
            new("fire_puffs_atlas", new[] { C(1, 1, 1, 0), C(1, 1, 1, 0.9f), C(1, 1, 1, 0.6f), C(1, 1, 1, 0) },
                true, sizeMin, sizeMax, speedMin, speedMax, spread, new Vector3(0, 1.6f, 0), lifetime, amount,
                Grow: true, Turbulence: 0.4f, Damping: damping, Atlas: new Vector2Int(4, 4));
        HellParticleLayer Smoke(int amount, float size) =>
            new("smoke", new[] { C(0.05f, 0.04f, 0.04f, 0), C(0.07f, 0.05f, 0.05f, 0.0f), C(0.06f, 0.05f, 0.05f, 0.6f), C(0.03f, 0.03f, 0.03f, 0) },
                false, size, size * 2.0f, 0.5f, 1.3f, 35, new Vector3(0, 1.0f, 0), 1.8f, amount, Grow: true, Turbulence: 0.5f);

        return kind switch
        {
            // Burst.
            0 => new[]
            {
                new HellParticleLayer("fireball_sheet", White, true, 2.6f, 3.6f, 0.1f, 0.6f, 180, new Vector3(0, 1.2f, 0), 0.75f, 6,
                    Flipbook: 8, FlipbookFromStart: true, EmitRadius: 0.25f),
                Flames(1.4f, 2.6f, 3.0f, 6.5f, 85, 0.6f, 30, damping: 7.0f),
                Puffs(1.1f, 1.9f, 2.0f, 4.5f, 85, 0.55f, 10, damping: 5.0f),
                new HellParticleLayer("spark", new[] { C(1, 0.95f, 0.75f), C(1, 0.55f, 0.12f), C(0.6f, 0.1f, 0, 0) },
                    true, 0.08f, 0.18f, 5, 11, 85, new Vector3(0, -7, 0), 0.9f, 46, AlignToVelocity: true, Damping: 1.2f),
                Embers(26, 1.8f),
                Smoke(10, 0.9f),
            },

            // Trail (projectiles, flames on a body).
            2 => new[]
            {
                // The ball burns as it flies: flames ride on it (local space) and leave nothing behind
                // but a few short-lived sparks.
                new HellParticleLayer("flame_sheet", White, true, 0.8f, 1.3f, 0.2f, 0.6f, 25, new Vector3(0, 1.2f, 0), 0.3f, 14,
                    Turbulence: 0.3f, Upright: true, Flipbook: 8, EmitRadius: 0.12f, Local: true),
                new HellParticleLayer("glow", new[] { C(1, 0.8f, 0.4f, 0), C(1, 0.6f, 0.2f, 0.5f), C(0.9f, 0.3f, 0.05f, 0) },
                    true, 0.5f, 0.8f, 0.0f, 0.1f, 10, Vector3.zero, 0.2f, 6, Local: true),
                new HellParticleLayer("spark", new[] { C(1, 0.9f, 0.6f), C(1, 0.5f, 0.1f), C(0.5f, 0.08f, 0, 0) },
                    true, 0.04f, 0.08f, 0.5f, 1.5f, 60, new Vector3(0, -2, 0), 0.25f, 10, AlignToVelocity: true),
            },

            // Burning ground.
            _ => new[]
            {
                // A soft glow under the flames fills the gaps between them, so the patch burns as one.
                new HellParticleLayer("glow", new[] { C(1, 0.6f, 0.2f, 0), C(1, 0.45f, 0.1f, 0.18f), C(0.8f, 0.2f, 0.03f, 0) },
                    true, 1.2f, 1.8f, 0.05f, 0.2f, 20, new Vector3(0, 0.2f, 0), 0.9f, 6),
                Flames(0.9f, 1.6f, 0.2f, 0.7f, 12, 0.65f, 34),
                Flames(0.45f, 0.8f, 0.3f, 0.9f, 18, 0.45f, 26),
                Puffs(0.7f, 1.2f, 0.2f, 0.6f, 15, 0.8f, 12),
                Embers(16, 1.6f),
                Smoke(5, 0.7f),
            },
        };
    }

internal static HellParticleLayer[] HandFlame(float size)
    {
        float s = Mathf.Max(0.1f, size);
        HellParticleLayer[] layers =
        {
            // Additive particles stack, so every colour sits well below white: the stack itself makes
            // the hot core, the tongues stay orange and red.
            new("glow", new[] { C(1, 0.7f, 0.3f, 0.0f), C(1, 0.55f, 0.18f, 0.22f), C(0.9f, 0.25f, 0.05f, 0.0f) },
                true, 0.3f * s, 0.42f * s, 0.02f, 0.1f * s, 10, Vector3.zero, 0.25f, 5, EmitRadius: 0.02f * s),
            new("flame_sheet", new[] { C(1, 1, 1, 0), C(1, 0.95f, 0.85f, 1), C(1, 0.85f, 0.7f, 0.85f), C(1, 0.6f, 0.5f, 0) },
                true, 1.3f * s, 1.8f * s, 0.15f * s, 0.45f * s, 8, new Vector3(0, 0.9f * s, 0), 0.65f, 7,
                Turbulence: 0.3f, Upright: true, EmitRadius: 0.05f * s, Flipbook: 8),
            new("spark", new[] { C(1, 0.9f, 0.6f), C(1, 0.5f, 0.12f), C(0.5f, 0.08f, 0, 0) },
                true, 0.04f * s, 0.09f * s, 0.8f * s, 2.0f * s, 25, new Vector3(0, 0.6f * s, 0), 0.9f, 6,
                AlignToVelocity: true, Turbulence: 1.4f, EmitRadius: 0.06f * s),
            new("smoke", new[] { C(0.06f, 0.05f, 0.05f, 0), C(0.08f, 0.07f, 0.06f, 0.35f), C(0.05f, 0.05f, 0.05f, 0) },
                false, 0.25f * s, 0.55f * s, 0.4f * s, 0.9f * s, 15, new Vector3(0, 0.8f * s, 0), 1.0f, 4,
                Grow: true, Turbulence: 0.6f, EmitRadius: 0.05f * s),
        };
        return layers;
    }
}
}
