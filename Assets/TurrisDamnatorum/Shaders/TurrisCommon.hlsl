// Wspólne funkcje shaderów Turris (URP).
#ifndef TURRIS_COMMON_INCLUDED
#define TURRIS_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

// Stałe kolorów w shaderach zapisano w przestrzeni gamma (projekt był w Gamma).
// W Linear przeliczamy barwę do liniowej, a jasność ponad 1 zachowujemy jako mnożnik HDR.
float3 TurrisColor(float3 c)
{
#if defined(UNITY_COLORSPACE_GAMMA)
    return c;
#else
    float m = max(c.r, max(c.g, c.b));
    if (m <= 1.0) return SRGBToLinear(c);
    return SRGBToLinear(c / m) * m;
#endif
}

// Odwrotność dla kolorów materiału, które Unity w Linear już przeliczył – gdy shader miesza je ze stałymi gamma.
float3 TurrisGammaOf(float3 c)
{
#if defined(UNITY_COLORSPACE_GAMMA)
    return c;
#else
    return LinearToSRGB(saturate(c));
#endif
}

#endif
