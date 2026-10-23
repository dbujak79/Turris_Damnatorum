// Ślad ognia na ziemi (FxDecal na ProcMesh.GroundQuad, UV 0..1). _Hot = 0: ciemne zwęglenie z poszarpanym brzegiem
// (mieszanie alfa); _Hot = 1: żarzące się pęknięcia i drobiny żaru (addytywnie, HDR), stygnące szybciej niż wypalenie.
// Wygaszanie przychodzi z FxDecal w _Color.a; kolor rgb nie jest używany.
Shader "Turris/FxScorch"
{
    Properties
    {
        _NoiseTex ("Szum", 2D) = "gray" {}
        _Color ("Wygaszanie (alfa)", Color) = (1,1,1,1)
        _TintColor ("Kolor (nieużywany)", Color) = (1,1,1,1)
        _Hot ("Żar zamiast zwęglenia", Float) = 0
        [HDR] _HotColor ("Żar: kolor gorący", Color) = (1,0.55,0.12,1)
        [HDR] _CoolColor ("Żar: kolor stygnący", Color) = (0.6,0.06,0.01,1)
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Mieszanie", Float) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha [_DstBlend]
        Cull Off ZWrite Off Offset -1, -1
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Assets/TurrisDamnatorum/Shaders/TurrisCommon.hlsl"
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST, _Color, _TintColor, _HotColor, _CoolColor;
                float _Hot, _DstBlend;
            CBUFFER_END
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float3 world : TEXCOORD1; float fog : TEXCOORD2; };
            v2f vert(appdata v)
            {
                v2f o; VertexPositionInputs p = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = p.positionCS; o.world = p.positionWS; o.uv = v.uv * 2 - 1; o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                float fade = saturate(_Color.a);
                float2 w = i.world.xz;
                float d = length(i.uv);
                float4 a = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, w * 0.31);
                float4 b = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, w * 0.93 + 0.37);
                // Poszarpany brzeg: promień zaburzony szumem, wnętrze nierówno zwęglone.
                float edge = d + (a.r - 0.5) * 0.45 + (b.g - 0.5) * 0.15;
                float burnt = 1 - smoothstep(0.55, 0.98, edge);
                if (_Hot < 0.5)
                {
                    float soot = lerp(0.7, 1.0, b.r);
                    float alpha = burnt * soot * 0.82 * fade;
                    float3 c = TurrisColor(float3(0.035, 0.028, 0.024)) * lerp(0.6, 1.4, a.g);
                    return float4(MixFog(c, i.fog), alpha);
                }
                // Żar: cienkie pęknięcia (izolinie szumu) i drobiny; najjaśniejsze w środku, stygną z kwadratem wygaszania.
                float crack = 1 - smoothstep(0.0, 0.07, abs(a.b - 0.5));
                float specks = smoothstep(0.78, 0.92, b.b);
                float core = 1 - smoothstep(0.0, 0.85, edge);
                float heat = fade * fade;
                float glow = (crack * 0.9 + specks * 0.6) * core * heat;
                // Barwa stygnie od _HotColor do _CoolColor (ogień: żółtopomarańczowa → ciemnoczerwona).
                float3 hotCol = lerp(_CoolColor.rgb, _HotColor.rgb, heat);
                float3 c = hotCol * glow * 4.0;
                return float4(MixFogColor(c, float3(0, 0, 0), i.fog), 1);
            }
            ENDHLSL
        }
    }
}
