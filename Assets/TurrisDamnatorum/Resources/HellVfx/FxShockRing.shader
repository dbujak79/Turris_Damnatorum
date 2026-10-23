// Ściana pyłu fali uderzeniowej (siatka FxShockRing: pochylony walec, u – obwód, v – wysokość).
// Kłęby z przewijanego szumu, gęste przy ziemi, rzedną ku górze i z wiekiem (_Fade z MaterialPropertyBlock).
Shader "Turris/FxShockRing"
{
    Properties
    {
        _NoiseTex ("Szum", 2D) = "gray" {}
        _Dust ("Pył", Color) = (0.46,0.39,0.31,1)
        _Fade ("Wygaszanie", Range(0,1)) = 1
        _Age ("Wiek (0..1)", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST, _Dust;
                float _Fade, _Age;
            CBUFFER_END
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float fog : TEXCOORD1; float eyeDepth : TEXCOORD2; };
            v2f vert(appdata v)
            {
                v2f o; VertexPositionInputs p = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = p.positionCS; o.uv = v.uv; o.fog = ComputeFogFactor(p.positionCS.z); o.eyeDepth = -p.positionVS.z;
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                float u = i.uv.x, h = i.uv.y;
                // Miękki pas pyłu: jedna niskoczęstotliwościowa modulacja (wyżej drobne wzory szumu dawały regularny „splot”).
                float n1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, float2(u * 2, h * 0.45 - _Age * 0.6)).r;
                float n2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, float2(u * 2 + 0.5, 0.37)).g;
                // Poszarpany szczyt wznoszący się z wiekiem; gęsto przy ziemi.
                float top = 0.45 + 0.4 * n2 + _Age * 0.2;
                float body = (1 - smoothstep(top - 0.4, top, h)) * smoothstep(0.0, 0.08, h);
                float alpha = body * (0.45 + 0.55 * n1) * (1 - _Age * 0.5) * _Fade;
                float sceneZ = LinearEyeDepth(SampleSceneDepth(GetNormalizedScreenSpaceUV(i.vertex)), _ZBufferParams);
                alpha *= saturate((sceneZ - i.eyeDepth) / 0.25);
                float3 c = _Dust.rgb * lerp(0.75, 1.1, n1) * lerp(0.8, 1.1, h);
                return float4(MixFog(c, i.fog), saturate(alpha));
            }
            ENDHLSL
        }
    }
}
