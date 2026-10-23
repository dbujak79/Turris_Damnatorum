// Pęknięcia gruntu po uderzeniu (FxDecal na ProcMesh.GroundQuad): szczeliny rozchodzące się od krateru,
// przyciemnione wnętrze i słaby żar w szczelinach, który stygnie szybciej niż same pęknięcia (_Color.a z FxDecal).
Shader "Turris/FxCracks"
{
    Properties
    {
        _NoiseTex ("Szum", 2D) = "gray" {}
        _Color ("Wygaszanie (alfa)", Color) = (1,1,1,1)
        _TintColor ("Kolor (nieużywany)", Color) = (1,1,1,1)
        _Dark ("Szczeliny", Color) = (0.03,0.025,0.02,1)
        [HDR] _Glow ("Żar w szczelinach", Color) = (1.6,0.55,0.12,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off Offset -1, -1
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST, _Color, _TintColor, _Dark, _Glow;
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
                float d = length(i.uv);
                // Ziarno z pozycji obiektu: każde uderzenie ma inny układ promieni.
                float seed = frac(dot(UNITY_MATRIX_M._m03_m23, float2(0.137, 0.713)));
                float ang = frac(atan2(i.uv.y, i.uv.x) / 6.2832 + 0.5 + seed);
                float4 w = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.world.xz * 0.6);
                // Szczeliny promieniste: izolinie szumu wzdłuż kąta, zygzakujące z odległością; gałęzie z szumu świata.
                float4 r = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, float2(ang * 3.0, d * 0.35));
                // Nierówna liczba promieni: część znika (hash numeru promienia), szerokość maleje ku końcom.
                float rays = ang * 7 + (r.r - 0.5) * 1.8 + d * 0.4;
                float rayId = floor(rays) + floor(seed * 97);
                float keep = step(0.3, frac(sin(rayId * 12.9898) * 43758.5453));
                float reachRay = lerp(0.55, 1.0, frac(sin(rayId * 78.233) * 12345.678));
                float width = (0.02 + 0.045 * (1 - d)) * (0.7 + r.g * 0.6);
                float radial = (1 - smoothstep(0.0, width, abs(frac(rays) - 0.5) * (0.6 + d))) * keep * (1 - smoothstep(reachRay - 0.2, reachRay, d));
                float branches = (1 - smoothstep(0.0, 0.035, abs(w.b - 0.5))) * smoothstep(0.15, 0.5, d);
                float reach = 1 - smoothstep(0.65, 1.0, d + (w.r - 0.5) * 0.3);
                float crack = saturate(radial + branches * 0.7) * reach;
                float crater = (1 - smoothstep(0.0, 0.32, d + (w.g - 0.5) * 0.15)) * 0.55;
                float heat = fade * fade * (1 - smoothstep(0.1, 0.7, d));
                float3 c = lerp(_Dark.rgb, _Glow.rgb, saturate(heat * crack));
                float alpha = saturate(crack * 0.9 + crater) * fade;
                return float4(MixFog(c, i.fog), alpha);
            }
            ENDHLSL
        }
    }
}
