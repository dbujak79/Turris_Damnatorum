// Zastępuje „Legacy Shaders/Particles/Additive” w URP (FxMaterials, TurrisFx*.mat).
Shader "Turris/FxAdditive"
{
    Properties
    {
        [MainColor] _TintColor ("Kolor", Color) = (0.5,0.5,0.5,0.5)
        [MainTexture] _MainTex ("Tekstura", 2D) = "white" {}
        _InvFade ("Miękkość cząsteczek", Range(0.01,3.0)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha One
        ColorMask RGB
        Cull Off ZWrite Off
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex FxVert
            #pragma fragment FxFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #define TURRIS_FX_ADDITIVE
            #define SOFTPARTICLES_ON
            #include "Assets/TurrisDamnatorum/Shaders/TurrisFxParticle.hlsl"
            ENDHLSL
        }
    }
}
