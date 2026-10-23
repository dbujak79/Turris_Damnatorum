// Szron rozchodzący się po ziemi (FxDecal na ProcMesh.GroundQuad, rośnie od środka): biało-błękitna warstwa
// z poszarpanym brzegiem, żyłki kryształków (izolinie szumu) i iskrzenie. Wygaszanie z FxDecal w _Color.a.
Shader "Turris/FxFrost"
{
    Properties
    {
        _NoiseTex ("Szum", 2D) = "gray" {}
        _Color ("Wygaszanie (alfa)", Color) = (1,1,1,1)
        _TintColor ("Kolor (nieużywany)", Color) = (1,1,1,1)
        [HDR] _Frost ("Szron", Color) = (0.55,0.78,1,1)
        [HDR] _Vein ("Żyłki", Color) = (1.4,1.9,2.4,1)
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
                float4 _NoiseTex_ST, _Color, _TintColor, _Frost, _Vein;
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
                float4 a = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, w * 0.45);
                // Druga próbka obrócona i w nieregularnej skali – bez widocznego powtarzania kafla szumu.
                float2 wr = float2(w.x * 0.8 - w.y * 0.6, w.x * 0.6 + w.y * 0.8);
                float4 b = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, wr * 0.83 + 0.21);
                // Brzeg szronu postrzępiony jak rosnące kryształy.
                float edge = d + (a.g - 0.5) * 0.35 + (b.r - 0.5) * 0.2;
                float cover = 1 - smoothstep(0.62, 0.97, edge);
                // Szron gęstszy przy brzegu rosnącej plamy (świeży front), rzadszy w środku; żyłki cienkie i nieliczne.
                float front = smoothstep(0.2, 0.85, edge);
                float frost = cover * lerp(0.18, 0.5, b.g) * lerp(0.7, 1.2, front);
                float veins = (1 - smoothstep(0.0, 0.022, abs(a.b - 0.5))) * cover * smoothstep(0.35, 0.6, b.r);
                // Iskrzenie z drobniejszej próbki: małe punkty zamiast plam.
                float sparkle = smoothstep(0.92, 0.97, SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, wr * 2.9 + 0.57).b) * cover;
                float3 c = _Frost.rgb * frost + _Vein.rgb * (veins * 0.6 + sparkle * 1.5);
                float alpha = saturate(frost * 0.75 + veins * 0.5 + sparkle) * fade;
                return float4(MixFog(c, i.fog), alpha);
            }
            ENDHLSL
        }
    }
}
