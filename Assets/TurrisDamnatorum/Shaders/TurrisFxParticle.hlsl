// Odpowiednik „Legacy Shaders/Particles/Additive” i „Alpha Blended” dla URP:
// ten sam wzór (2 × kolor wierzchołka × _TintColor × tekstura), miękkie cząsteczki z _InvFade i mgła.
#ifndef TURRIS_FX_PARTICLE_INCLUDED
#define TURRIS_FX_PARTICLE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    half4 _TintColor;
    float _InvFade;
CBUFFER_END

struct Attributes
{
    float4 positionOS : POSITION;
    half4 color : COLOR;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    half4 color : COLOR;
    float2 uv : TEXCOORD0;
    float eyeDepth : TEXCOORD1;
    float fogCoord : TEXCOORD2;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings FxVert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
    o.positionCS = p.positionCS;
    o.eyeDepth = -p.positionVS.z;
    o.color = v.color;
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.fogCoord = ComputeFogFactor(p.positionCS.z);
    return o;
}

half4 FxFrag(Varyings i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
#if defined(SOFTPARTICLES_ON)
    float2 uv = GetNormalizedScreenSpaceUV(i.positionCS);
    float sceneZ = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
    i.color.a *= saturate(_InvFade * (sceneZ - i.eyeDepth));
#endif
    half4 col = 2.0h * i.color * _TintColor * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    col.a = saturate(col.a);
#if defined(TURRIS_FX_ADDITIVE)
    col.rgb = MixFogColor(col.rgb, half3(0, 0, 0), i.fogCoord);
#else
    col.rgb = MixFog(col.rgb, i.fogCoord);
#endif
    return col;
}

#endif
