// Zastępuje „Legacy Shaders/Particles/Alpha Blended” w URP (dym, kurz, popiół).
Shader "Turris/FxAlphaBlended"
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
        Blend SrcAlpha OneMinusSrcAlpha
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
            #define SOFTPARTICLES_ON
            #include "Assets/TurrisDamnatorum/Shaders/TurrisFxParticle.hlsl"
            ENDHLSL
        }
    }
}
