// Drganie gorącego powietrza: próbka _CameraOpaqueTexture przesunięta przewijanym szumem w obrębie maski
// cząsteczki (alfa tekstury × alfa koloru). Rysowane przed innymi przezroczystymi (kolejka 2998),
// bo tekstura nieprzezroczystych nie zawiera efektów. Wymaga Opaque Texture w assecie URP.
Shader "Turris/FxDistort"
{
    Properties
    {
        _MainTex ("Maska", 2D) = "white" {}
        _NoiseTex ("Szum", 2D) = "gray" {}
        _Strength ("Siła", Range(0,0.1)) = 0.018
        _Speed ("Prędkość falowania", Float) = 0.9
    }
    SubShader
    {
        Tags { "Queue"="Transparent-2" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST, _NoiseTex_ST;
                float _Strength, _Speed;
            CBUFFER_END
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float4 uv : TEXCOORD0; float4 uv2 : TEXCOORD1; };
            struct v2f { float4 vertex : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; float rnd : TEXCOORD1; float eyeDepth : TEXCOORD2; };
            v2f vert(appdata v)
            {
                v2f o; VertexPositionInputs p = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = p.positionCS; o.eyeDepth = -p.positionVS.z; o.color = v.color;
                o.uv = TRANSFORM_TEX(v.uv.xy, _MainTex); o.rnd = v.uv2.w;
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                float mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a * i.color.a;
                float2 screen = GetNormalizedScreenSpaceUV(i.vertex);
                // Miękko przy podłożu, żeby nie „wycinać” krawędzi geometrii.
                float sceneZ = LinearEyeDepth(SampleSceneDepth(screen), _ZBufferParams);
                mask *= saturate((sceneZ - i.eyeDepth) / 0.4);
                float2 n = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv * 1.7 + float2(i.rnd * 3.7, -_Time.y * _Speed)).rg - 0.5;
                float3 col = SampleSceneColor(screen + n * _Strength * mask);
                return float4(col, saturate(mask * 1.5));
            }
            ENDHLSL
        }
    }
}
