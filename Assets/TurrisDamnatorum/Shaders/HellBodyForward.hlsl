// Przebieg forward shadera Turris/HellBody: URP Lit + powierzchnie HU z _HellSurface
// (x – cień, y – woda, z – żar, w – elita). Wartość przychodzi z MaterialPropertyBlock (HellBodySurface).
#ifndef HELL_BODY_FORWARD_INCLUDED
#define HELL_BODY_FORWARD_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
#include "Assets/TurrisDamnatorum/Shaders/TurrisCommon.hlsl"

float4 _HellSurface;

void HellBodyFragment(
    Varyings input
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out uint outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);
    surfaceData.albedo *= lerp(1, 0.18, _HellSurface.x) * lerp(1, 0.72, _HellSurface.y);
    surfaceData.smoothness = lerp(surfaceData.smoothness, 0.94, _HellSurface.y);

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, UNDO_TRANSFORM_TEX(input.uv, _BaseMap));

#if defined(_DBUFFER)
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

    InitializeBakedGIData(input, inputData);

    float rim = pow(1 - saturate(dot(inputData.viewDirectionWS, inputData.normalWS)), 3);
    float glow = TurrisGammaOf(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv * 5).rgb).r;
    float ember = pow(saturate(glow - 0.28), 3) * (15 + 3 * sin(_Time.y * 5));
    surfaceData.emission += TurrisColor(float3(1.8, 0.18, 0.02)) * ember * _HellSurface.z
        + TurrisColor(float3(1.2, 0.58, 0.06)) * rim * _HellSurface.w
        + TurrisColor(float3(0.17, 0.03, 0.28)) * rim * _HellSurface.x;

    half4 color = UniversalFragmentPBR(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent());

    outColor = color;

#ifdef _WRITE_RENDERING_LAYERS
    outRenderingLayers = EncodeMeshRenderingLayer();
#endif
}

#endif
