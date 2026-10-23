// Uber-shader cząsteczek HU w URP. Przy zerowych parametrach dodatkowych działa jak dawny prosty shader
// (tekstura × kolor wierzchołka × _TintColor × _Intensity). Strumienie wierzchołków ustawia
// HellVfxMaterials.ApplyStreams: TEXCOORD0 = UV + Custom1.xy (x – erozja w czasie życia),
// TEXCOORD1 = UV2 + AnimBlend + StableRandomX (płynny flipbook, losowe przesunięcie szumu).
Shader "Turris/HellParticle"
{
    Properties
    {
        _MainTex ("Tekstura HU", 2D) = "white" {}
        _TintColor ("Kolor", Color) = (1,1,1,1)
        _Intensity ("Jasność w pipeline Unity", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Mieszanie", Float) = 1
        _NoiseTex ("Szum (erozja, falowanie)", 2D) = "gray" {}
        _HotBoost ("Jądro HDR: wzmocnienie", Float) = 0
        _HotPower ("Jądro HDR: wykładnik jasności", Float) = 2
        _Erosion ("Erozja krawędzi pod koniec życia", Range(0,1.5)) = 0
        _ErosionSoft ("Miękkość erozji", Range(0.02,1)) = 0.2
        _Flow ("Falowanie UV", Range(0,0.1)) = 0
        _FlowScale ("Skala falowania", Float) = 1.5
        _FlowSpeed ("Prędkość falowania", Float) = 0.6
        [Toggle] _FlipBlend ("Płynny flipbook", Float) = 0
        _SoftFade ("Miękkie przecięcie z podłożem [m]", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha [_DstBlend]
        Cull Off ZWrite Off
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST, _NoiseTex_ST, _TintColor;
                float _Intensity, _DstBlend, _HotBoost, _HotPower, _Erosion, _ErosionSoft;
                float _Flow, _FlowScale, _FlowSpeed, _FlipBlend, _SoftFade;
            CBUFFER_END
            struct appdata
            {
                float4 vertex : POSITION; float4 color : COLOR;
                float4 uv : TEXCOORD0;   // xy – UV, zw – Custom1.xy
                float4 uv2 : TEXCOORD1;  // xy – UV2 (następna klatka), z – AnimBlend, w – StableRandomX
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f
            {
                float4 vertex : SV_POSITION; float4 color : COLOR;
                float4 uv : TEXCOORD0; float4 uv2 : TEXCOORD1; float eyeDepth : TEXCOORD2;
            };
            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                VertexPositionInputs p = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = p.positionCS; o.eyeDepth = -p.positionVS.z;
                o.color = v.color * _TintColor;
                o.uv = float4(TRANSFORM_TEX(v.uv.xy, _MainTex), v.uv.zw);
                o.uv2 = float4(TRANSFORM_TEX(v.uv2.xy, _MainTex), v.uv2.zw);
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv.xy, uv2 = i.uv2.xy;
                float rnd = i.uv2.w;
                if (_Flow > 0)
                {
                    float2 n = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv * _FlowScale + float2(rnd * 7.31, rnd * 3.17 - _Time.y * _FlowSpeed)).rg - 0.5;
                    uv += n * _Flow; uv2 += n * _Flow;
                }
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                if (_FlipBlend > 0.5) tex = lerp(tex, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv2), saturate(i.uv2.z));
                float4 color = tex * i.color;

                // Erozja: z wiekiem cząsteczka rozpada się na strzępy zamiast równo bledniąć.
                float e = saturate(i.uv.z) * _Erosion;
                if (e > 0)
                {
                    float n = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv.xy * 1.3 + rnd * 5.13).b;
                    color.a *= saturate((n + _ErosionSoft - e * (1 + _ErosionSoft)) / _ErosionSoft);
                }

                // Jądro HDR: najjaśniejsze miejsca tekstury przekraczają 1 i łapie je bloom.
                float lum = dot(color.rgb, float3(0.299, 0.587, 0.114));
                color.rgb *= _Intensity * (1 + _HotBoost * pow(saturate(lum), _HotPower));

                if (_SoftFade > 0)
                {
                    float sceneZ = LinearEyeDepth(SampleSceneDepth(GetNormalizedScreenSpaceUV(i.vertex)), _ZBufferParams);
                    color.a *= saturate((sceneZ - i.eyeDepth) / _SoftFade);
                }
                return color;
            }
            ENDHLSL
        }
    }
}
