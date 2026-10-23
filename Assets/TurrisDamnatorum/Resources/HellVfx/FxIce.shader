// Kryształ lodu (siatka z IceCrystals): fasety oświetlone słońcem, fresnel, połysk i wewnętrzna poświata HDR.
// _Dissolve (MaterialPropertyBlock) rozpuszcza kryształ szumem w przestrzeni świata, z jasną krawędzią rozpadu.
Shader "Turris/FxIce"
{
    Properties
    {
        _NoiseTex ("Szum", 2D) = "gray" {}
        [HDR] _Deep ("Głębia", Color) = (0.01,0.05,0.16,1)
        [HDR] _Mid ("Fasety", Color) = (0.08,0.3,0.72,1)
        [HDR] _Rim ("Krawędź (fresnel)", Color) = (0.45,0.78,1,1)
        [HDR] _Edge ("Krawędź rozpadu", Color) = (2.2,3.4,4.5,1)
        _Glow ("Wewnętrzna poświata", Float) = 0.6
        _Dissolve ("Rozpad", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On Cull Back
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST, _Deep, _Mid, _Rim, _Edge;
                float _Glow, _Dissolve;
            CBUFFER_END
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 vertex : SV_POSITION; float3 normal : TEXCOORD0; float3 world : TEXCOORD1; float2 uv : TEXCOORD2; float fog : TEXCOORD3; };
            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o; VertexPositionInputs p = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = p.positionCS; o.world = p.positionWS; o.normal = TransformObjectToWorldNormal(v.normal);
                o.uv = v.uv; o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                float n = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.world.xz * 0.9 + i.world.y * 0.37).r;
                float cut = n - _Dissolve * 1.05;
                clip(cut);
                float3 N = normalize(i.normal);
                float3 V = normalize(GetWorldSpaceViewDir(i.world));
                Light light = GetMainLight();
                float ndl = saturate(dot(N, light.direction));
                float fres = pow(1 - saturate(dot(N, V)), 3);
                float spec = pow(saturate(dot(reflect(-light.direction, N), V)), 48) * 2.0;
                // Poświata od podstawy ku czubkowi (uv.y), jakby światło było uwięzione w lodzie.
                // Kontrast faset: ciemny granat w cieniu, nasycony błękit w świetle; jasne tylko krawędzie i odblask.
                float facet = pow(ndl, 1.6);
                float3 c = lerp(_Deep.rgb, _Mid.rgb, facet) * (0.35 + 0.4 * light.color)
                    + _Rim.rgb * fres * 0.9 + light.color * spec * 0.8
                    + _Mid.rgb * _Glow * (1 - i.uv.y) * 0.5;
                c += _Edge.rgb * (1 - smoothstep(0.0, 0.07, cut)) * step(0.001, _Dissolve);
                float alpha = lerp(0.42, 0.92, saturate(fres + facet * 0.25));
                return float4(MixFog(c, i.fog), alpha);
            }
            ENDHLSL
        }
    }
}
